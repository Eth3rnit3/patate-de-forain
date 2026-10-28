using Patate.Combat;

namespace Patate.UnityAdapters
{
    /// <summary>Salon du jeu en ligne. Provisoire : pas encore disponible, retour au titre.</summary>
    public sealed class LobbyScreen : IGameScreen
    {
        readonly GameFlow flow;

        public LobbyScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        public void Enter() => flow.Go(flow.Title());
        public void Update(float deltaTime) { }
        public void OnGUI() { }
        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }
}
