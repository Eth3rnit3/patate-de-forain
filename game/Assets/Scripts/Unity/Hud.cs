using System.Collections.Generic;
using System.Linq;
using Patate.Application;
using Patate.Combat;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Interface du combat, au style de la fête foraine (voir <see cref="Ui"/>) :
    /// portraits, jauges de vie avec traînée des dégâts récents, médaillon du
    /// chronomètre, noms, annonces de manche peintes, pause et aide des
    /// commandes ; plus l'affichage de débogage (boîtes de collision exactes de
    /// la simulation, états, entrées).
    /// </summary>
    public sealed class Hud
    {
        const float Top = 34, BarHeight = 46, PortraitSize = 150;
        /// <summary>Délai avant que la traînée rattrape la jauge, et sa vitesse (fraction de jauge par seconde).</summary>
        const float TrailDelay = 0.45f, TrailSpeed = 0.7f;

        readonly GUIStyle mono;
        readonly float[] trail = { 1, 1 }, shown = { 1, 1 }, trailWait = { 0, 0 };
        readonly string[] tags;

        /// <param name="players">Source des commandes de chaque place, affichée sous le nom (« J1 · Clavier droit »).</param>
        public Hud(PlayerSource[] players = null)
        {
            tags = new string[2];
            for (int slot = 0; slot < 2; slot++)
                tags[slot] = players == null ? $"J{slot + 1}" : $"J{slot + 1} · {MenuInput.DeviceName(players[slot])}";
            mono = new GUIStyle { fontSize = 18, font = Font.CreateDynamicFontFromOSFont(new[] { "Menlo", "Consolas", "Courier New" }, 18), normal = { textColor = new Color(0.92f, 0.92f, 0.95f) }, richText = true };
        }

        public void Draw(MatchState state, FighterVisuals[] fighters, ILocalInput[] inputs, bool showHelp, bool paused)
        {
            var previous = Ui.Begin();
            float width = Ui.Width, time = Time.unscaledTime;
            // Voile en haut : les jauges restent lisibles sur n'importe quel décor.
            Ui.Gradient(new Rect(0, 0, width, 240), new Color(0.02f, 0.03f, 0.12f, 0.7f), new Color(0.02f, 0.03f, 0.12f, 0));

            for (int slot = 0; slot < 2; slot++)
            {
                float max = Mathf.Max(1, fighters[slot].Package.Gameplay.Stats.Health);
                UpdateTrail(slot, Mathf.Clamp01(state.Fighters[slot].Health / max));
                DrawFighter(slot, fighters[slot], width, time);
            }
            DrawTimer(state, width, time);
            DrawAnnouncement(state, fighters, width);

            if (paused)
            {
                Ui.Fill(new Rect(0, 0, width, Ui.ReferenceHeight), new Color(0.02f, 0.03f, 0.1f, 0.6f));
                Ui.Text(new Rect(0, 400, width, 140), "PAUSE", Ui.Style(120), Ui.Yellow, 6);
                Ui.Text(new Rect(0, 560, width, 50), "Échap : reprendre      F5 : recommencer la manche", Ui.Style(32), Ui.Cream, 3);
            }
            else if (state.Round.Phase == RoundPhase.Over)
            {
                float blink = 0.6f + 0.4f * Mathf.Sin(time * 4);
                Ui.Text(new Rect(0, 700, width, 50), "Entrée ou Start : continuer      F5 : rejouer la manche", Ui.Style(34), Ui.Cream, 3, blink);
            }

            if (showHelp) DrawHelp(inputs, width, time);
            else Ui.Text(new Rect(20, Ui.ReferenceHeight - 38, width - 40, 30),
                "F1 : débogage   F2 : commandes   F3 / F4 : changer de combattant   F5 : recommencer   Échap : pause",
                Ui.Style(20, TextAnchor.MiddleLeft, FontStyle.Normal), new Color(1, 1, 1, 0.75f), 1.5f);
            GUI.matrix = previous;
        }

        /// <summary>
        /// La traînée claire montre les dégâts récents puis rejoint la jauge.
        /// Avance seulement au dessin final de l'image (OnGUI est appelé
        /// plusieurs fois par image).
        /// </summary>
        void UpdateTrail(int slot, float ratio)
        {
            shown[slot] = ratio;
            if (Event.current.type != EventType.Repaint) return;
            if (ratio >= trail[slot])
            {
                trail[slot] = ratio; // soin ou nouvelle manche : pas de traînée
                trailWait[slot] = TrailDelay;
                return;
            }
            float delta = Time.unscaledDeltaTime;
            if (trailWait[slot] > 0) trailWait[slot] -= delta;
            else trail[slot] = Mathf.Max(ratio, trail[slot] - TrailSpeed * delta);
        }

        void DrawFighter(int slot, FighterVisuals visuals, float width, float time)
        {
            bool left = slot == 0;
            var color = Ui.PlayerColors[slot];
            float barWidth = Mathf.Max(200, width / 2 - 110 - 30 - PortraitSize - 24);

            // Portrait : médaillon cerclé de la couleur du joueur.
            var portrait = new Rect(left ? 30 : width - 30 - PortraitSize, Top - 10, PortraitSize, PortraitSize);
            Ui.Fill(new Rect(portrait.x + 5, portrait.y + 7, portrait.width, portrait.height), new Color(0, 0, 0, 0.45f));
            Ui.Fill(portrait, color);
            var inner = new Rect(portrait.x + 6, portrait.y + 6, portrait.width - 12, portrait.height - 12);
            Ui.Gradient(inner, Ui.NavyLight, Ui.Navy);
            // Portrait dessiné regardant à droite : miroir pour le joueur 2.
            Ui.Texture(inner, visuals.Portrait, mirror: !left);
            Ui.Bulbs(new Rect(portrait.x + 3, portrait.y + 3, portrait.width - 6, portrait.height - 6), false, time, 37);

            // Jauge de vie : cadre bleu marine et jaune, se vide vers le chronomètre comme en arcade.
            float x = left ? portrait.xMax + 24 : portrait.x - 24 - barWidth;
            var frame = new Rect(x - 8, Top - 8, barWidth + 16, BarHeight + 16);
            Ui.Fill(new Rect(frame.x + 4, frame.y + 6, frame.width, frame.height), new Color(0, 0, 0, 0.45f));
            Ui.Fill(frame, Ui.Navy);
            Ui.Frame(new Rect(frame.x + 3, frame.y + 3, frame.width - 6, frame.height - 6), Ui.Yellow, 3);
            var bar = new Rect(x, Top, barWidth, BarHeight);
            Ui.Fill(bar, new Color(0.22f, 0.03f, 0.05f));
            Rect Part(float ratio) => new Rect(left ? bar.xMax - bar.width * ratio : bar.x, bar.y, bar.width * ratio, bar.height);
            Ui.Fill(Part(trail[slot]), new Color(1f, 0.93f, 0.8f));
            float health = shown[slot];
            bool danger = health < 0.25f;
            var top = danger ? Color.Lerp(Ui.Red, Ui.Orange, 0.5f + 0.5f * Mathf.Sin(time * 10)) : Color.Lerp(Ui.Orange, Ui.Yellow, health);
            Ui.Gradient(Part(health), top, danger ? new Color(0.6f, 0.05f, 0.05f) : Ui.Orange);
            Ui.Fill(new Rect(bar.x, bar.y + 4, bar.width, 5), new Color(1, 1, 1, 0.18f)); // reflet

            // Nom et origine des commandes sous la jauge.
            var nameRect = new Rect(x, Top + BarHeight + 12, barWidth, 46);
            var align = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Ui.Text(nameRect, visuals.Package.Name.ToUpperInvariant(), Ui.Style(36, align), Ui.Cream, 3);
            Ui.Text(new Rect(x, nameRect.yMax, barWidth, 30), tags[slot], Ui.Style(22, align, FontStyle.Normal), color, 2);
        }

        static void DrawTimer(MatchState state, float width, float time)
        {
            int seconds = (state.Round.Timer + 59) / 60;
            bool hurry = seconds <= 10 && state.Round.Phase == RoundPhase.Fight;
            var center = new Vector2(width / 2, Top + 50);
            float pulse = hurry ? 1 + 0.06f * Mathf.Sin(time * 12) : 1;
            Ui.Disc(center + new Vector2(4, 6), 80, new Color(0, 0, 0, 0.45f));
            Ui.Disc(center, 82 * pulse, Ui.Navy);
            Ui.Disc(center, 74 * pulse, hurry ? Ui.Red : Ui.Yellow);
            Ui.Disc(center, 66 * pulse, Ui.Navy);
            Ui.Text(new Rect(center.x - 80, center.y - 50, 160, 100), seconds.ToString("00"), Ui.Style(Mathf.RoundToInt(72 * pulse)), hurry ? Ui.Red : Ui.Cream, 4);
        }

        void DrawAnnouncement(MatchState state, FighterVisuals[] fighters, float width)
        {
            var round = state.Round;
            var center = new Vector2(width / 2, 440);
            float age = round.PhaseTick / 60f;
            switch (round.Phase)
            {
                case RoundPhase.Intro:
                    if (round.PhaseTick < 45) Ui.Word("manche-1", "MANCHE 1", center, 230, age);
                    else Ui.Word("prets", "PRÊTS ?", center, 230, (round.PhaseTick - 45) / 60f);
                    break;
                case RoundPhase.Fight:
                    if (round.PhaseTick < 50) Ui.Word("combat", "COMBAT !", center, 260, age);
                    break;
                case RoundPhase.KO:
                    Ui.Word("ko", "K.O. !", center, 300, age);
                    break;
                case RoundPhase.TimeOver:
                    Ui.Word("temps-ecoule", "TEMPS ÉCOULÉ !", center, 230, age);
                    break;
                case RoundPhase.Over:
                    if (round.Winner == RoundState.Draw) Ui.Word("egalite", "ÉGALITÉ", center, 230, age);
                    else
                    {
                        Ui.Word("victoire", "VICTOIRE !", center, 230, age);
                        var winner = fighters[round.Winner].Package.Name;
                        Ui.Text(new Rect(0, center.y + 130, width, 60), $"{winner} (J{round.Winner + 1}) l'emporte", Ui.Style(44), Ui.Cream, 3, Mathf.Clamp01(age / 0.3f));
                    }
                    break;
            }
        }

        void DrawHelp(ILocalInput[] inputs, float width, float time)
        {
            var panel = new Rect(width / 2 - 640, 190, 1280, 600);
            Ui.Marquee(panel, Ui.Red, false, time, 40);
            Ui.Text(new Rect(panel.x, panel.y + 26, panel.width, 50), "COMMANDES", Ui.Style(44), Ui.Yellow, 3);
            for (int slot = 0; slot < 2; slot++)
            {
                float x = panel.x + 60 + slot * 600;
                Ui.Text(new Rect(x, panel.y + 90, 560, 40), tags[slot], Ui.Style(32, TextAnchor.MiddleLeft), Ui.PlayerColors[slot], 2);
                int line = 0;
                foreach (var (action, keys) in inputs[slot].Help())
                    Ui.Text(new Rect(x, panel.y + 145 + 42 * line++, 560, 40), $"<b>{action}</b> : {keys}", Ui.Style(22, TextAnchor.MiddleLeft, FontStyle.Normal, true), Ui.Cream, 0);
            }
            Ui.Text(new Rect(panel.x + 60, panel.yMax - 120, panel.width - 120, 90),
                "Les touches sont des positions physiques : les noms affichés suivent la disposition de votre clavier.\n" +
                "F1 : débogage   F2 : fermer l'aide   F3 / F4 : combattant J1 / J2   F5 : recommencer   Échap : pause",
                Ui.Style(20, TextAnchor.MiddleLeft, FontStyle.Normal, true), Ui.Muted, 0);
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

        static void Fill(Rect rect, Color color) => Ui.Fill(rect, color);
    }
}
