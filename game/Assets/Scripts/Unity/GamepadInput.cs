using System.Collections.Generic;
using Patate.Combat;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Adaptateur manette (Input System). Toute manette reconnue comme
    /// <see cref="Gamepad"/> convient, y compris en WebGL où le navigateur
    /// expose les manettes Xbox et PlayStation sous la disposition standard.
    /// Disposition arcade : stick gauche ou croix pour les directions, boutons
    /// de face pour les attaques (rangée du haut les poings et le spécial,
    /// rangée du bas les pieds). Une manette débranchée ne produit plus rien.
    /// </summary>
    public sealed class GamepadInput : ILocalInput
    {
        /// <summary>Au-delà, une inclinaison du stick compte comme une direction (une diagonale donne environ 0,7).</summary>
        const float StickThreshold = 0.5f;

        static readonly (Buttons button, GamepadButton pad)[] bindings =
        {
            (Buttons.LightPunch, GamepadButton.West),
            (Buttons.LightKick, GamepadButton.South),
            (Buttons.HeavyKick, GamepadButton.East),
            (Buttons.Special, GamepadButton.North),
        };

        readonly int deviceId;
        Buttons sampled;

        public string Label { get; }

        /// <param name="deviceId">Identifiant Input System de la manette (<see cref="InputDevice.deviceId"/>).</param>
        public GamepadInput(int deviceId, string label)
        {
            this.deviceId = deviceId;
            Label = label;
        }

        Gamepad Device => InputSystem.GetDeviceById(deviceId) as Gamepad;

        /// <summary>Lit la manette ; appelé une fois par image d'affichage.</summary>
        public void Sample() => sampled = Read(Device);

        public Buttons InputFor(int tick) => sampled;

        public static Buttons Read(Gamepad pad)
        {
            if (pad == null || !pad.added) return Buttons.None;
            var stick = pad.leftStick.ReadValue();
            var result = Buttons.None;
            if (stick.x < -StickThreshold || pad.dpad.left.isPressed) result |= Buttons.Left;
            if (stick.x > StickThreshold || pad.dpad.right.isPressed) result |= Buttons.Right;
            if (stick.y > StickThreshold || pad.dpad.up.isPressed) result |= Buttons.Up;
            if (stick.y < -StickThreshold || pad.dpad.down.isPressed) result |= Buttons.Down;
            foreach (var (button, control) in bindings)
                if (pad[control].isPressed) result |= button;
            return result;
        }

        /// <summary>Un bouton d'attaque, Start ou une gâchette haute vient d'être enfoncé (appuyer pour rejoindre).</summary>
        public static bool JustPressed(Gamepad pad)
        {
            if (pad == null || !pad.added) return false;
            foreach (var (_, control) in bindings)
                if (pad[control].wasPressedThisFrame) return true;
            return pad.startButton.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame;
        }

        /// <summary>Aide lisible : noms Xbox et PlayStation, sauf si la manette est reconnue.</summary>
        public IEnumerable<(string action, string keys)> Help()
        {
            var pad = Device;
            yield return ("Déplacement", "stick gauche ou croix");
            yield return ("Garde", "tenir la direction opposée à l'adversaire");
            yield return ("Poing", Name(pad, GamepadButton.West));
            yield return ("Pied rapide", Name(pad, GamepadButton.South));
            yield return ("Pied puissant", Name(pad, GamepadButton.East));
            yield return ("Spécial", $"{Name(pad, GamepadButton.North)}  ou  bas, bas-avant, avant + poing");
        }

        static string Name(Gamepad pad, GamepadButton button)
        {
            string xbox, playstation;
            switch (button)
            {
                case GamepadButton.West: xbox = "X"; playstation = "Carré"; break;
                case GamepadButton.South: xbox = "A"; playstation = "Croix"; break;
                case GamepadButton.East: xbox = "B"; playstation = "Rond"; break;
                default: xbox = "Y"; playstation = "Triangle"; break;
            }
            if (pad is DualShockGamepad) return playstation;
            if (pad is XInputController) return xbox;
            return $"{xbox} / {playstation}";
        }
    }
}
