using Patate.Combat;

namespace Patate.UnityAdapters
{
    /// <summary>Écran titre. Provisoire : passe directement à la sélection.</summary>
    public sealed class TitleScreen : IGameScreen
    {
        readonly GameFlow flow;

        public TitleScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        public void Enter() => flow.Go(flow.CharacterSelect());
        public void Update(float deltaTime) { }
        public void OnGUI() { }
        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }

    /// <summary>
    /// Sélection du mode, des joueurs et des combattants : remplit
    /// <see cref="GameFlow.Setup"/>. Provisoire : garde les réglages par
    /// défaut et lance le combat.
    /// </summary>
    public sealed class CharacterSelectScreen : IGameScreen
    {
        readonly GameFlow flow;

        public CharacterSelectScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        public void Enter() => flow.Go(flow.Fight());
        public void Update(float deltaTime) { }
        public void OnGUI() { }
        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }

    /// <summary>Résultat de la manche. Provisoire : revanche immédiate avec les mêmes réglages.</summary>
    public sealed class ResultScreen : IGameScreen
    {
        readonly GameFlow flow;

        public MatchState Final { get; }

        public ResultScreen(GameFlow flow, MatchState final)
        {
            this.flow = flow;
            Final = final;
        }

        public void Enter() => flow.Go(flow.Fight());
        public void Update(float deltaTime) { }
        public void OnGUI() { }
        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }
}
