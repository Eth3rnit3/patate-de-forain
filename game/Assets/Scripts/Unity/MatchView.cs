using System.Collections.Generic;
using Patate.Combat;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Rendu d'un combat. Observe l'état et les événements de la simulation ;
    /// ne décide d'aucune règle. Les effets visuels (impacts, tremblement de
    /// caméra) vivent ici, hors de l'état de simulation : ils n'ont pas à être
    /// rejoués lors d'un rollback.
    /// </summary>
    public sealed class MatchView
    {
        const float Scale = 1f / FighterVisuals.WorldUnitsPerUnityUnit;
        /// <summary>Hauteur visible, en unités monde (1080 = un écran HD à l'échelle 1).</summary>
        const float VisibleHeight = 1080f;
        /// <summary>Distance entre le sol et le bas de l'écran.</summary>
        const float GroundMargin = 130f;
        /// <summary>La caméra peut dépasser l'arène : un combattant coincé contre le bord reste entier à l'écran.</summary>
        const float CameraOverscan = 240f;

        readonly Transform root;
        readonly Camera camera;
        readonly MatchRules rules;
        readonly FighterVisuals[] visuals;
        readonly SpriteRenderer[] fighters = new SpriteRenderer[2];
        readonly Dictionary<int, SpriteRenderer> projectiles = new Dictionary<int, SpriteRenderer>();
        readonly List<Effect> effects = new List<Effect>();
        readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
        float shake;
        int shakeTicks;

        /// <summary>Effet d'un combattant (animation de son paquet) ou effet commun (<see cref="SharedEffects"/>).</summary>
        sealed class Effect
        {
            public SpriteRenderer Renderer;
            public FighterVisuals Owner;
            public AnimationBinding Binding;
            public SharedEffects.Animation Shared;
            public int Tick;

            public int Duration => Shared?.Duration ?? Owner.Package.Presentation.Animations[Binding.Animation].TotalDuration;
        }

        public MatchView(Camera camera, MatchRules rules, FighterVisuals first, FighterVisuals second, StageView stage)
        {
            this.camera = camera;
            this.rules = rules;
            visuals = new[] { first, second };
            root = new GameObject("Combat").transform;
            for (int slot = 0; slot < 2; slot++) fighters[slot] = NewRenderer($"Combattant {slot + 1}");
            camera.orthographic = true;
            camera.orthographicSize = VisibleHeight / 2f * Scale;
            camera.backgroundColor = stage.Background;
        }

        public void SetFighters(FighterVisuals first, FighterVisuals second)
        {
            visuals[0] = first;
            visuals[1] = second;
            Clear();
        }

        /// <summary>Réagit aux événements d'un tick (effets, tremblement). Appelé à chaque tick simulé.</summary>
        public void OnEvents(IReadOnlyList<MatchEvent> events)
        {
            foreach (var e in events)
            {
                if (e.Type == MatchEventType.Hit) Shake(Mathf.Clamp(e.Damage / 12f, 2f, 10f), e.Damage >= 80 ? 10 : 6);
                // Coups au corps : étincelle commune au point de contact (les projectiles ont leur propre impact).
                if ((e.Type == MatchEventType.Hit || e.Type == MatchEventType.Block) && e.ProjectileId == null)
                {
                    var spark = e.Type == MatchEventType.Hit ? SharedEffects.Hit : SharedEffects.Block;
                    if (spark != null)
                    {
                        var renderer = Rent("Étincelle");
                        renderer.transform.localPosition = new Vector3(e.X * Scale, e.Y * Scale, 0);
                        // Plus grosse pour un coup lourd ; tournée vers la cible.
                        float size = e.Type == MatchEventType.Hit ? Mathf.Lerp(0.8f, 1.3f, Mathf.InverseLerp(30, 110, e.Damage)) : 1f;
                        renderer.transform.localScale = new Vector3(e.Facing * size, size, 1);
                        renderer.sortingOrder = 30;
                        renderer.sprite = spark.Frames[0];
                        effects.Add(new Effect { Renderer = renderer, Shared = spark });
                    }
                }
                if (e.Type == MatchEventType.ProjectileEnd && e.Reason != ProjectileEndReason.Expired && e.Reason != ProjectileEndReason.OutOfArena)
                {
                    var owner = visuals[e.Slot];
                    if (!owner.Package.Presentation.Projectiles.TryGetValue(e.ProjectileId, out var visual) || visual.Impact == null) continue;
                    var renderer = Rent("Impact");
                    renderer.transform.localPosition = new Vector3(e.X * Scale, e.Y * Scale, 0);
                    renderer.transform.localScale = new Vector3(e.Facing, 1, 1);
                    renderer.sortingOrder = 30;
                    effects.Add(new Effect { Renderer = renderer, Owner = owner, Binding = new AnimationBinding { Animation = visual.Impact, Playback = Playback.Once } });
                }
            }
            // Les effets avancent au rythme de la simulation, pas de l'affichage.
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var effect = effects[i];
                effect.Tick++;
                if (effect.Tick >= effect.Duration)
                {
                    Return(effect.Renderer);
                    effects.RemoveAt(i);
                }
            }
            if (shakeTicks > 0) shakeTicks--;
        }

        public void Render(MatchState state)
        {
            for (int slot = 0; slot < 2; slot++)
            {
                var fighter = state.Fighters[slot];
                var renderer = fighters[slot];
                var pick = FramePicker.ForFighter(fighter, visuals[slot].Package.Presentation);
                renderer.sprite = visuals[slot].Sprite(pick.Sprite);
                renderer.transform.localPosition = new Vector3(fighter.X * Scale, fighter.Y * Scale, 0);
                // Le pivot du sprite est l'origine : le miroir se fait autour des pieds, comme les hitboxes.
                renderer.transform.localScale = new Vector3(fighter.Facing, 1, 1);
                // Le combattant qui attaque passe devant.
                renderer.sortingOrder = fighter.Action.Kind == ActionKind.Move ? 12 : 10 + slot;
            }

            var alive = new HashSet<int>();
            foreach (var projectile in state.Projectiles)
            {
                alive.Add(projectile.Id);
                if (!projectiles.TryGetValue(projectile.Id, out var renderer))
                {
                    renderer = Rent("Projectile");
                    renderer.sortingOrder = 20;
                    projectiles[projectile.Id] = renderer;
                }
                var presentation = visuals[projectile.Owner].Package.Presentation;
                var binding = new AnimationBinding { Animation = presentation.Projectiles[projectile.ProjectileId].Animation, Playback = Playback.Loop };
                renderer.sprite = visuals[projectile.Owner].Sprite(FramePicker.Resolve(binding, presentation, projectile.Tick).Sprite);
                renderer.transform.localPosition = new Vector3(projectile.X * Scale, projectile.Y * Scale, 0);
                renderer.transform.localScale = new Vector3(projectile.Facing, 1, 1);
            }
            var gone = new List<int>();
            foreach (var id in projectiles.Keys) if (!alive.Contains(id)) gone.Add(id);
            foreach (var id in gone)
            {
                Return(projectiles[id]);
                projectiles.Remove(id);
            }

            foreach (var effect in effects)
            {
                if (effect.Shared != null)
                {
                    effect.Renderer.sprite = effect.Shared.At(effect.Tick);
                    continue;
                }
                var pick = FramePicker.Resolve(effect.Binding, effect.Owner.Package.Presentation, effect.Tick);
                effect.Renderer.sprite = effect.Owner.Sprite(pick.Sprite);
            }

            PlaceCamera(state);
        }

        /// <summary>Suit le milieu des combattants sans montrer l'extérieur de l'arène.</summary>
        void PlaceCamera(MatchState state)
        {
            float halfWidth = camera.orthographicSize * camera.aspect / Scale;
            float middle = (state.Fighters[0].X + state.Fighters[1].X) / 2f;
            float min = rules.ArenaLeft - CameraOverscan + halfWidth;
            float max = rules.ArenaRight + CameraOverscan - halfWidth;
            float x = min > max ? (rules.ArenaLeft + rules.ArenaRight) / 2f : Mathf.Clamp(middle, min, max);
            float y = VisibleHeight / 2f - GroundMargin;
            var offset = shakeTicks > 0 ? Random.insideUnitCircle * shake : Vector2.zero; // aléa visuel uniquement
            camera.transform.position = new Vector3((x + offset.x) * Scale, (y + offset.y) * Scale, -10);
        }

        void Shake(float amplitude, int ticks)
        {
            shake = amplitude;
            shakeTicks = Mathf.Max(shakeTicks, ticks);
        }

        /// <summary>Retire les combattants et effets de la scène (fin du combat).</summary>
        public void Dispose() => Object.Destroy(root.gameObject);

        public void Clear()
        {
            foreach (var renderer in projectiles.Values) Return(renderer);
            projectiles.Clear();
            foreach (var effect in effects) Return(effect.Renderer);
            effects.Clear();
            shakeTicks = 0;
        }

        SpriteRenderer NewRenderer(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            return go.AddComponent<SpriteRenderer>();
        }

        SpriteRenderer Rent(string name)
        {
            var renderer = pool.Count > 0 ? pool.Pop() : NewRenderer(name);
            renderer.gameObject.name = name;
            renderer.gameObject.SetActive(true);
            return renderer;
        }

        void Return(SpriteRenderer renderer)
        {
            renderer.gameObject.SetActive(false);
            pool.Push(renderer);
        }

        /// <summary>Monde (unités de simulation) → pixels d'écran IMGUI (origine en haut à gauche).</summary>
        public Vector2 ToGui(float x, float y)
        {
            var screen = camera.WorldToScreenPoint(new Vector3(x * Scale, y * Scale, 0));
            return new Vector2(screen.x, Screen.height - screen.y);
        }
    }
}
