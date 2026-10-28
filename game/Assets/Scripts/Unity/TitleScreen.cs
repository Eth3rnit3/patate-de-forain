using UnityEngine;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>Écran titre : le logo sur la fête foraine, n'importe quelle touche ou bouton pour entrer.</summary>
    public sealed class TitleScreen : IGameScreen
    {
        readonly GameFlow flow;
        float age;

        public TitleScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        public void Enter() => age = 0;

        public void Update(float deltaTime)
        {
            age += deltaTime;
            // Un court délai évite qu'une touche encore enfoncée en revenant d'un autre écran ne relance aussitôt.
            if (age > 0.3f && AnyPress()) flow.Go(new ModeSelectScreen(flow));
        }

        static bool AnyPress()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame && !Keyboard.current.escapeKey.wasPressedThisFrame) return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            foreach (var pad in Gamepad.all)
                if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame) return true;
            return (MenuInput.Any() & MenuCommand.Confirm) != 0;
        }

        public void OnGUI()
        {
            var previous = Ui.Begin();
            float time = Time.unscaledTime;
            Ui.Background(0.15f);
            float width = Mathf.Min(Ui.Width * 0.82f, 1300);
            float arrive = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.6f));
            float height = width / 2;
            var logo = new Rect(Ui.Width / 2 - width / 2, 90 + Mathf.Sin(time * 1.6f) * 8 - (1 - arrive) * 120, width, height);
            Ui.Logo(logo, arrive);

            float blink = 0.8f + 0.2f * Mathf.Sin(time * 4.5f);
            var prompt = new Rect(0, 780, Ui.Width, 70);
            Ui.Marquee(new Rect(Ui.Width / 2 - 470, prompt.y - 18, 940, 106), Ui.Red, true, time);
            Ui.Text(prompt, "APPUYEZ SUR UNE TOUCHE", Ui.Style(52), Ui.Yellow, 4, blink * arrive);
            Ui.Text(new Rect(0, 870, Ui.Width, 40), "clavier ou manette", Ui.Style(26, fontStyle: FontStyle.Normal), Ui.Cream, 2, arrive);
            Ui.Text(new Rect(0, Ui.ReferenceHeight - 50, Ui.Width - 30, 36), "Prototype", Ui.Style(20, TextAnchor.MiddleRight, FontStyle.Normal), Ui.Muted, 0);
            GUI.matrix = previous;
        }

        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }
}
