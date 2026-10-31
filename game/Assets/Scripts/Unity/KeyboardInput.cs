using System.Collections.Generic;
using System.Linq;
using Patate.Application;
using Patate.Combat;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Adaptateur clavier local. Les touches sont des positions physiques
    /// (Input System, disposition QWERTY de référence) : sur un clavier AZERTY,
    /// Key.A est la touche marquée « Q ». L'aide affichée en jeu utilise le nom
    /// réel de la touche selon la disposition de l'utilisateur.
    /// </summary>
    public sealed class KeyboardInput : ILocalInput
    {
        public string Label { get; }
        readonly (Buttons button, Key[] keys)[] bindings;
        Buttons sampled;

        KeyboardInput(string label, params (Buttons, Key[])[] bindings)
        {
            Label = label;
            this.bindings = bindings;
        }

        /// <summary>Joueur 1, moitié gauche du clavier.</summary>
        public static KeyboardInput PlayerOne() => new KeyboardInput("Joueur 1",
            (Buttons.Left, new[] { Key.A }), (Buttons.Right, new[] { Key.D }),
            (Buttons.Up, new[] { Key.W }), (Buttons.Down, new[] { Key.S }),
            (Buttons.LightPunch, new[] { Key.F }), (Buttons.LightKick, new[] { Key.G }),
            (Buttons.HeavyKick, new[] { Key.H }), (Buttons.Special, new[] { Key.R }));

        /// <summary>Joueur 2, flèches et moitié droite (ou pavé numérique).</summary>
        public static KeyboardInput PlayerTwo() => new KeyboardInput("Joueur 2",
            (Buttons.Left, new[] { Key.LeftArrow }), (Buttons.Right, new[] { Key.RightArrow }),
            (Buttons.Up, new[] { Key.UpArrow }), (Buttons.Down, new[] { Key.DownArrow }),
            (Buttons.LightPunch, new[] { Key.K, Key.Numpad1 }), (Buttons.LightKick, new[] { Key.L, Key.Numpad2 }),
            (Buttons.HeavyKick, new[] { Key.Semicolon, Key.Numpad3 }), (Buttons.Special, new[] { Key.O, Key.Numpad0 }));

        /// <summary>Lit le clavier ; appelé une fois par image d'affichage.</summary>
        public void Sample()
        {
            var keyboard = Keyboard.current;
            sampled = Buttons.None;
            if (keyboard == null) return;
            foreach (var (button, keys) in bindings)
                if (keys.Any(k => keyboard[k].isPressed)) sampled |= button;
        }

        public Buttons InputFor(int tick) => sampled;

        /// <summary>Une touche d'attaque de cette moitié vient d'être enfoncée (appuyer pour rejoindre).</summary>
        public bool JustPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            const Buttons directions = Buttons.Left | Buttons.Right | Buttons.Up | Buttons.Down;
            return bindings.Any(b => (b.button & directions) == 0 && b.keys.Any(k => keyboard[k].wasPressedThisFrame));
        }

        /// <summary>Aide lisible : nom affiché des touches selon la disposition active.</summary>
        public IEnumerable<(string action, string keys)> Help()
        {
            var keyboard = Keyboard.current;
            string Name(Key key) => keyboard != null ? keyboard[key].displayName : key.ToString();
            string Keys(Buttons button) => string.Join(" / ", bindings.Where(b => b.button == button).SelectMany(b => b.keys).Select(Name));
            yield return ("Déplacement", $"{Keys(Buttons.Left)} {Keys(Buttons.Right)}");
            yield return ("Sauter / s'accroupir", $"{Keys(Buttons.Up)} / {Keys(Buttons.Down)}  (coups en l'air et accroupi)");
            yield return ("Garde", "arrière contre l'adversaire ; bas-arrière pour les coups bas");
            yield return ("Poing", Keys(Buttons.LightPunch));
            yield return ("Pied rapide", Keys(Buttons.LightKick));
            yield return ("Pied puissant", Keys(Buttons.HeavyKick));
            yield return ("Spécial", $"{Keys(Buttons.Special)}  ou  bas, bas-avant, avant + poing");
        }
    }
}
