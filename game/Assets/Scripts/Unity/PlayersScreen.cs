using System.Collections.Generic;
using System.Linq;
using Patate.Application;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// « Qui joue ? » : entre le choix du mode et la sélection des
    /// personnages (ou le salon en ligne), chaque joueur humain choisit son
    /// appareil. N'importe quel appareil pilote l'écran ; un appui sur un
    /// appareil allume son nom, pour le reconnaître. Les règles sont dans
    /// <see cref="PlayerAssignment"/>.
    /// </summary>
    public sealed class PlayersScreen : IGameScreen
    {
        /// <summary>Durée pendant laquelle un appareil qu'on vient d'utiliser reste allumé.</summary>
        const float Glow = 0.8f;

        readonly GameFlow flow;
        readonly Dictionary<PlayerSource, float> lastPress = new Dictionary<PlayerSource, float>();
        PlayerAssignment assignment;
        List<PlayerSource> available;
        float age;

        public PlayersScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        public void Enter()
        {
            age = 0;
            available = MenuInput.Devices().ToList();
            assignment = new PlayerAssignment(flow.Setup.Mode, flow.Setup, available);
        }

        public void Update(float deltaTime)
        {
            age += deltaTime;
            // Le navigateur ne signale une manette qu'après un premier appui : la liste suit les branchements.
            available = MenuInput.Devices().ToList();
            foreach (var device in available)
                if (MenuInput.Of(device) != MenuCommand.None) lastPress[device] = Time.unscaledTime;
            if (age < 0.2f) return; // laisse retomber l'appui qui a ouvert l'écran

            var command = MenuInput.Any();
            if ((command & MenuCommand.Back) != 0)
            {
                flow.Go(new ModeSelectScreen(flow));
                return;
            }
            if (!assignment.Command(command, available)) return;
            assignment.ApplyTo(flow.Setup);
            flow.Go(flow.Setup.Mode == MatchMode.Online ? flow.Lobby() : new CharacterSelectScreen(flow, keepPlayers: true));
        }

        public void OnGUI()
        {
            var previous = Ui.Begin();
            float time = Time.unscaledTime, width = Ui.Width;
            Ui.Background(0.5f);
            string title = assignment.Mode == MatchMode.Versus ? "QUI JOUE ?" : "VOTRE APPAREIL";
            Ui.Text(new Rect(0, 40, width, 80), title, Ui.Style(64), Ui.Yellow, 4);

            const float rowWidth = 900, rowHeight = 150, gap = 40;
            int rows = assignment.Mode == MatchMode.Solo ? 2 : assignment.Humans;
            float x = width / 2 - rowWidth / 2, y = 200;
            for (int row = 0; row < assignment.Humans; row++)
            {
                var device = assignment.Devices[row];
                string name = device.HasValue ? MenuInput.DeviceName(device.Value) : "Aucun appareil";
                if (device.HasValue && !available.Contains(device.Value)) name += " (débranchée)";
                string label = assignment.Mode == MatchMode.Online ? "VOUS" : $"JOUEUR {row + 1}";
                var rect = new Rect(x, y + row * (rowHeight + gap), rowWidth, rowHeight);
                Ui.Button(rect, $"<     {name.ToUpperInvariant()}     >", null, row == assignment.Row, true, time);
                Ui.Tag(new Rect(rect.x + 24, rect.y - 22, 300, 44), label, TextAnchor.MiddleLeft, Ui.PlayerColors[row], 26);
            }
            if (assignment.Mode == MatchMode.Solo)
            {
                var rect = new Rect(x, y + (rowHeight + gap), rowWidth, rowHeight);
                Ui.Button(rect, InputSources.Describe(flow.Setup.Players[1]).ToUpperInvariant(), null, false, false, time);
                Ui.Tag(new Rect(rect.x + 24, rect.y - 22, 300, 44), "JOUEUR 2", TextAnchor.MiddleLeft, Ui.PlayerColors[1], 26);
            }

            DrawDevices(new Rect(0, y + rows * (rowHeight + gap) + 30, width, 200), time);

            string status = assignment.Ready(available) ? "Entrée, Espace ou A : continuer" : "Choisissez un appareil branché pour continuer";
            Ui.Hint($"Haut / Bas : joueur      Gauche / Droite : appareil      {status}      Échap ou B : retour");
            GUI.matrix = previous;
        }

        /// <summary>Appareils branchés, allumés quand on s'en sert, et le joueur qui les a pris.</summary>
        void DrawDevices(Rect area, float time)
        {
            Ui.Text(new Rect(area.x, area.y, area.width, 40), "Appareils branchés : appuyez sur un bouton pour reconnaître le vôtre", Ui.Style(26, fontStyle: FontStyle.Normal), Ui.Muted, 0);
            const float tagWidth = 300, tagGap = 24;
            float total = available.Count * tagWidth + (available.Count - 1) * tagGap;
            float left = area.center.x - total / 2;
            for (int i = 0; i < available.Count; i++)
            {
                var device = available[i];
                bool lit = lastPress.TryGetValue(device, out var pressed) && time - pressed < Glow;
                int owner = System.Array.FindIndex(assignment.Devices, d => d.Equals(device));
                var color = owner >= 0 ? Ui.PlayerColors[owner] : Ui.Muted;
                var rect = new Rect(left + i * (tagWidth + tagGap), area.y + 56, tagWidth, 64);
                Ui.Fill(rect, new Color(Ui.Navy.r, Ui.Navy.g, Ui.Navy.b, 0.8f));
                Ui.Frame(rect, lit ? Ui.Yellow : color, lit ? 5 : 3);
                string text = MenuInput.DeviceName(device) + (owner >= 0 ? $"  ·  J{owner + 1}" : "");
                Ui.Text(rect, text, Ui.Style(28), lit ? Ui.Yellow : Ui.Cream, 2);
            }
            if (Gamepad.all.Count == 0)
                Ui.Text(new Rect(area.x, area.y + 140, area.width, 40), "Une manette n'apparaît qu'après un premier appui sur l'un de ses boutons.", Ui.Style(24, fontStyle: FontStyle.Normal), Ui.Cream, 0);
        }

        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }
}
