using Patate.Application;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Choix du mode : Versus local, Solo contre l'ordinateur (avec son
    /// niveau), En ligne. Remplit <see cref="GameFlow.Setup"/> puis passe à la
    /// sélection des personnages, ou au salon pour le jeu en ligne.
    /// </summary>
    public sealed class ModeSelectScreen : IGameScreen
    {
        static readonly string[] Levels = { "Facile", "Normal", "Difficile" };
        // Gardés d'une visite à l'autre : on retrouve le menu comme on l'a laissé.
        static int selected, level = 1;

        readonly GameFlow flow;
        bool cpuAvailable;
        float age;

        public ModeSelectScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        /// <summary>Niveau envoyé à l'IA : <c>PlayerSource.Cpu(1)</c> facile à <c>Cpu(3)</c> difficile.</summary>
        static PlayerSource Cpu => PlayerSource.Cpu(level + 1);

        public void Enter()
        {
            age = 0;
            cpuAvailable = InputSupport.Available(Cpu);
        }

        bool Enabled(int index) => index != 1 || cpuAvailable;

        public void Update(float deltaTime)
        {
            age += deltaTime;
            if (age < 0.2f) return; // laisse retomber l'appui qui a ouvert l'écran
            var command = MenuInput.Any();
            if ((command & MenuCommand.Back) != 0)
            {
                flow.Go(flow.Title());
                return;
            }
            if ((command & MenuCommand.Up) != 0) selected = (selected + 2) % 3;
            if ((command & MenuCommand.Down) != 0) selected = (selected + 1) % 3;
            if (selected == 1 && (command & MenuCommand.Left) != 0) level = Mathf.Max(0, level - 1);
            if (selected == 1 && (command & MenuCommand.Right) != 0) level = Mathf.Min(Levels.Length - 1, level + 1);
            if ((command & MenuCommand.Confirm) == 0 || !Enabled(selected)) return;

            switch (selected)
            {
                case 0:
                    flow.Setup.Mode = MatchMode.Versus;
                    flow.Go(flow.CharacterSelect());
                    break;
                case 1:
                    flow.Setup.Mode = MatchMode.Solo;
                    flow.Setup.Players[1] = Cpu;
                    flow.Go(flow.CharacterSelect());
                    break;
                default:
                    flow.Setup.Mode = MatchMode.Online;
                    flow.Go(flow.Lobby());
                    break;
            }
        }

        public void OnGUI()
        {
            var previous = Ui.Begin();
            float time = Time.unscaledTime;
            Ui.Background(0.45f);
            Ui.Logo(new Rect(Ui.Width / 2 - 290, 24, 580, 290));

            const float width = 820, height = 150, gap = 40;
            float x = Ui.Width / 2 - width / 2, y = 350;
            string solo = cpuAvailable
                ? $"Niveau :   <   {Levels[level]}   >"
                : "Bientôt : l'adversaire ordinateur n'est pas encore branché";
            var items = new[]
            {
                ("VERSUS LOCAL", "Deux joueurs, au clavier ou à la manette"),
                ("SOLO CONTRE L'ORDINATEUR", solo),
                ("EN LIGNE", "Affronter un ami à distance"),
            };
            for (int i = 0; i < items.Length; i++)
            {
                float slide = Mathf.Clamp01((age - i * 0.06f) / 0.25f);
                var rect = new Rect(x + (1 - Mathf.SmoothStep(0, 1, slide)) * (i % 2 == 0 ? -400 : 400), y + i * (height + gap), width, height);
                Ui.Button(rect, items[i].Item1, items[i].Item2, i == selected, Enabled(i), time);
            }
            Ui.Hint("Haut / Bas : choisir      Gauche / Droite : niveau      Entrée, Espace ou A : valider      Échap ou B : retour");
            GUI.matrix = previous;
        }

        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }
}
