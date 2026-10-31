using Patate.Application;
using Patate.Combat;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Résultat du combat : le vainqueur à l'honneur, puis revanche (mêmes
    /// réglages), changer de personnage (mêmes joueurs) ou retour au menu.
    /// N'importe quel joueur pilote le menu.
    /// </summary>
    public sealed class ResultScreen : IGameScreen
    {
        static readonly string[] Choices = { "REVANCHE", "CHANGER DE PERSONNAGE", "RETOUR AU MENU" };

        readonly GameFlow flow;
        int selected;
        float age;

        public MatchState Final { get; }

        public ResultScreen(GameFlow flow, MatchState final)
        {
            this.flow = flow;
            Final = final;
        }

        public void Enter() => age = 0;

        public void Update(float deltaTime)
        {
            age += deltaTime;
            if (age < 0.4f) return; // laisse retomber les appuis de fin de combat
            var command = MenuInput.Any();
            if ((command & MenuCommand.Up) != 0) selected = (selected + Choices.Length - 1) % Choices.Length;
            if ((command & MenuCommand.Down) != 0) selected = (selected + 1) % Choices.Length;
            if ((command & MenuCommand.Back) != 0) selected = Choices.Length - 1;
            if ((command & MenuCommand.Confirm) == 0) return;
            switch (selected)
            {
                case 0: flow.Go(flow.Fight()); break;
                case 1: flow.Go(new CharacterSelectScreen(flow, keepPlayers: true)); break;
                default: flow.Go(flow.Title()); break;
            }
        }

        public void OnGUI()
        {
            var previous = Ui.Begin();
            float time = Time.unscaledTime, width = Ui.Width;
            Ui.Background(0.4f, "resultat-fond");
            int winner = Final.Round.Winner;
            bool draw = winner == RoundState.Draw || winner < 0;

            // Les deux combattants, le vainqueur au premier plan, le vaincu dans l'ombre.
            // Les boutons occupent le centre (760) : les portraits prennent ce qui reste de chaque côté.
            float portrait = Mathf.Clamp((width - 760 - 200) / 2, 240, 520);
            for (int slot = 0; slot < 2; slot++)
            {
                bool won = draw || slot == winner;
                float size = won ? portrait : portrait * 0.72f;
                float enter = Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - (won ? 0.1f : 0)) / 0.45f));
                float x = slot == 0 ? 60 - (1 - enter) * 300 : width - 60 - size + (1 - enter) * 300;
                var rect = new Rect(x, Ui.ReferenceHeight - 190 - size, size, size);
                var visuals = flow.Visuals(flow.Setup.Fighters[slot]);
                if (won) Ui.Disc(rect.center, size * 0.47f, new Color(Ui.Yellow.r, Ui.Yellow.g, Ui.Yellow.b, 0.22f + 0.06f * Mathf.Sin(time * 3)));
                Ui.Texture(rect, visuals.Portrait, mirror: slot == 1, alpha: won ? enter : 0.45f * enter);
                string who = Describe(slot);
                Ui.Text(new Rect(rect.x - 40, rect.yMax + 4, rect.width + 80, 50), $"{visuals.Package.Name.ToUpperInvariant()}", Ui.Style(won ? 44 : 32), won ? Ui.Cream : Ui.Muted, 3, enter);
                if (enter > 0.5f) Ui.Tag(new Rect(rect.x - 40, rect.yMax + 54, rect.width + 80, 34), who, TextAnchor.MiddleCenter, Ui.PlayerColors[slot]);
            }

            Ui.Word(draw ? "egalite" : "victoire", draw ? "ÉGALITÉ" : "VICTOIRE !", new Vector2(width / 2, 140), 210, age);
            if (!draw)
            {
                var name = flow.Visuals(flow.Setup.Fighters[winner]).Package.Name;
                Ui.Text(new Rect(0, 250, width, 60), $"{name} ({Describe(winner)}) l'emporte", Ui.Style(40), Ui.Cream, 3, Mathf.Clamp01(age / 0.5f));
            }

            const float buttonWidth = 760, buttonHeight = 110, gap = 28;
            float bx = width / 2 - buttonWidth / 2, by = 380;
            for (int i = 0; i < Choices.Length; i++)
                Ui.Button(new Rect(bx, by + i * (buttonHeight + gap), buttonWidth, buttonHeight), Choices[i], null, i == selected, true, time);
            Ui.Hint("Haut / Bas : choisir      Entrée, Espace ou A : valider");
            GUI.matrix = previous;
        }

        string Describe(int slot)
        {
            var source = flow.Setup.Players[slot];
            return $"J{slot + 1} · {MenuInput.DeviceName(source)}";
        }

        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }
}
