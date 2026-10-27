using System.Collections.Generic;
using System.Linq;
using Patate.Combat;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Interface simple en IMGUI : barres de vie, chronomètre, messages de
    /// manche, aide des commandes, et affichage de débogage (boîtes de
    /// collision exactes de la simulation, états, entrées).
    /// </summary>
    public sealed class Hud
    {
        const float ReferenceHeight = 1080f;
        readonly GUIStyle big, medium, small, mono;

        public Hud()
        {
            big = new GUIStyle { fontSize = 96, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            medium = new GUIStyle { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
            small = new GUIStyle { fontSize = 22, alignment = TextAnchor.UpperLeft, normal = { textColor = new Color(0.92f, 0.92f, 0.95f) }, richText = true };
            mono = new GUIStyle(small) { fontSize = 18, font = Font.CreateDynamicFontFromOSFont(new[] { "Menlo", "Consolas", "Courier New" }, 18) };
        }

        public void Draw(MatchState state, FighterVisuals[] fighters, KeyboardInput[] inputs, bool showHelp, bool paused)
        {
            var previous = GUI.matrix;
            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale;

            for (int slot = 0; slot < 2; slot++) DrawHealth(slot, state.Fighters[slot], fighters[slot], width);
            int seconds = (state.Round.Timer + 59) / 60;
            var timerStyle = new GUIStyle(big) { fontSize = 72, normal = { textColor = Color.white } };
            GUI.Label(new Rect(width / 2 - 80, 20, 160, 90), seconds.ToString("00"), timerStyle);

            var message = RoundMessage(state, fighters);
            if (message != null) GUI.Label(new Rect(0, 380, width, 200), message, big);
            if (state.Round.Phase == RoundPhase.Over || paused)
                GUI.Label(new Rect(0, 560, width, 60), paused ? "Pause — Échap pour reprendre" : "Entrée ou F5 : nouvelle manche", new GUIStyle(medium) { alignment = TextAnchor.MiddleCenter });

            if (showHelp) DrawHelp(inputs, width);
            else GUI.Label(new Rect(20, ReferenceHeight - 40, width, 30), "F1 : débogage   F2 : commandes   F3 / F4 : changer de combattant   F5 : recommencer   Échap : pause", small);
            GUI.matrix = previous;
        }

        void DrawHealth(int slot, FighterState fighter, FighterVisuals visuals, float width)
        {
            const float barWidth = 620, barHeight = 36, top = 40, portrait = 110;
            float max = Mathf.Max(1, visuals.Package.Gameplay.Stats.Health);
            float ratio = Mathf.Clamp01(fighter.Health / max);
            float x = slot == 0 ? 40 + portrait + 10 : width - 40 - portrait - 10 - barWidth;
            Fill(new Rect(x - 4, top - 4, barWidth + 8, barHeight + 8), new Color(0, 0, 0, 0.7f));
            Fill(new Rect(x, top, barWidth, barHeight), new Color(0.45f, 0.05f, 0.05f));
            float fill = barWidth * ratio;
            // La barre se vide vers le chronomètre, comme en arcade.
            Fill(new Rect(slot == 0 ? x + barWidth - fill : x, top, fill, barHeight), Color.Lerp(new Color(0.95f, 0.25f, 0.1f), new Color(1f, 0.85f, 0.2f), ratio));
            var name = new GUIStyle(medium) { alignment = slot == 0 ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight };
            GUI.Label(new Rect(x, top + barHeight + 6, barWidth, 40), $"{visuals.Package.Name}  <size=22>J{slot + 1}</size>", new GUIStyle(name) { richText = true });
            if (visuals.Portrait != null)
            {
                var rect = new Rect(slot == 0 ? 40 : width - 40 - portrait, top - 10, portrait, portrait);
                Fill(rect, new Color(0, 0, 0, 0.5f));
                // Portrait dessiné regardant à droite : miroir pour le joueur 2.
                if (slot == 1) GUI.DrawTextureWithTexCoords(rect, visuals.Portrait, new Rect(1, 0, -1, 1));
                else GUI.DrawTexture(rect, visuals.Portrait, ScaleMode.ScaleToFit);
            }
        }

        static string RoundMessage(MatchState state, FighterVisuals[] fighters)
        {
            var round = state.Round;
            switch (round.Phase)
            {
                case RoundPhase.Intro: return round.PhaseTick < 45 ? "Manche 1" : "Prêts ?";
                case RoundPhase.Fight: return round.PhaseTick < 50 ? "Combat !" : null;
                case RoundPhase.KO: return "K.O. !";
                case RoundPhase.TimeOver: return "Temps écoulé";
                case RoundPhase.Over:
                    return round.Winner == RoundState.Draw ? "Égalité" : $"{fighters[round.Winner].Package.Name} (J{round.Winner + 1}) l'emporte";
                default: return null;
            }
        }

        void DrawHelp(KeyboardInput[] inputs, float width)
        {
            var panel = new Rect(width / 2 - 620, 200, 1240, 520);
            Fill(panel, new Color(0, 0, 0, 0.8f));
            for (int slot = 0; slot < 2; slot++)
            {
                float x = panel.x + 40 + slot * 600;
                GUI.Label(new Rect(x, panel.y + 20, 560, 40), inputs[slot].Label, medium);
                int line = 0;
                foreach (var (action, keys) in inputs[slot].Help())
                    GUI.Label(new Rect(x, panel.y + 80 + 40 * line++, 560, 40), $"<b>{action}</b> : {keys}", small);
            }
            GUI.Label(new Rect(panel.x + 40, panel.yMax - 120, 1160, 100),
                "Les touches sont des positions physiques : les noms affichés suivent la disposition de votre clavier.\n" +
                "F1 : débogage   F2 : fermer l'aide   F3 / F4 : combattant J1 / J2   F5 : recommencer   Échap : pause", small);
        }

        // --------------------------------------------------------------- débogage

        public void DrawDebug(MatchState state, SimulationContext context, MatchView view, string checksum, IEnumerable<string> missingSounds)
        {
            foreach (var fighter in state.Fighters)
            {
                var definition = context.Of(fighter);
                Outline(Hitboxes.Pushbox(fighter, definition), view, new Color(1f, 0.9f, 0.2f), 1);
                foreach (var box in Hitboxes.Hurtboxes(fighter, definition)) Outline(box, view, new Color(0.3f, 0.7f, 1f), 2);
                var active = Hitboxes.Active(fighter, definition);
                foreach (var box in active) Outline(box, view, new Color(1f, 0.15f, 0.2f), 3);
                // Coup qui a déjà touché : la hitbox existe encore mais ne peut plus toucher.
                if (active.Count == 0) foreach (var box in Hitboxes.Declared(fighter, definition)) Outline(box, view, new Color(0.6f, 0.35f, 0.4f), 1);
                var pivot = view.ToGui(fighter.X, fighter.Y);
                Fill(new Rect(pivot.x - 10, pivot.y - 1, 20, 3), Color.white);
                Fill(new Rect(pivot.x - 1, pivot.y - 10, 3, 20), Color.white);
            }
            foreach (var projectile in state.Projectiles)
                if (Hitboxes.Projectile(projectile, context.Fighters[projectile.Owner], out var box)) Outline(box, view, new Color(1f, 0.55f, 0.1f), 3);

            var lines = new List<string>
            {
                $"tick {state.Tick}   manche {state.Round.Phase} ({state.Round.PhaseTick})   état #{checksum}",
                $"J1 {context.Fighters[0].Ref}   J2 {context.Fighters[1].Ref}   règles {context.Rules.Id} v{context.Rules.Version}",
                $"J1 {Describe(state.Fighters[0], context.Fighters[0])}",
                $"J2 {Describe(state.Fighters[1], context.Fighters[1])}",
                $"projectiles : {state.Projectiles.Count}   FPS affichage : {1f / Mathf.Max(0.0001f, Time.smoothDeltaTime):0}",
                "<color=#ffe633>pushbox</color>  <color=#4db3ff>hurtbox</color>  <color=#ff2633>hitbox</color>  <color=#996070>hitbox consommée</color>  <color=#ff8c1a>projectile</color>",
            };
            var missing = missingSounds.ToList();
            if (missing.Count > 0) lines.Add($"sons non livrés : {string.Join(", ", missing.OrderBy(s => s).Take(8))}{(missing.Count > 8 ? "…" : "")}");
            // En haut, sous les barres de vie : le sol et les pieds restent visibles.
            float top = Screen.height * 0.16f;
            Fill(new Rect(10, top, 760, 18 * lines.Count + 12), new Color(0, 0, 0, 0.6f));
            GUI.Label(new Rect(18, top + 6, 760, 18 * lines.Count), string.Join("\n", lines), new GUIStyle(mono) { fontSize = 14, richText = true });
        }

        static string Describe(FighterState fighter, CompiledFighter definition)
        {
            var a = fighter.Action;
            string detail = a.Kind switch
            {
                ActionKind.Move => $"{a.MoveId} {Hitboxes.CurrentSegment(fighter, definition)?.Definition.Phase} t{a.Tick}{(a.HasHit ? " ✓" : "")}",
                ActionKind.Hitstun or ActionKind.Blockstun => $"{a.Kind} {a.Tick}/{a.Duration}",
                ActionKind.Walk => a.Forward ? "Walk avant" : "Walk arrière",
                _ => a.Kind.ToString(),
            };
            return $"{detail}{(fighter.Hitstop > 0 ? $"  gel {fighter.Hitstop}" : "")}   PV {fighter.Health}   x {fighter.X}   entrées {(int)fighter.CurrentInput:X3}";
        }

        static void Outline(WorldBox box, MatchView view, Color color, float thickness)
        {
            var a = view.ToGui(box.Left, box.Top);
            var b = view.ToGui(box.Right, box.Bottom);
            var rect = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
            Fill(new Rect(rect.x, rect.y, rect.width, rect.height), new Color(color.r, color.g, color.b, 0.12f));
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        static void Fill(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
