using System;
using System.Collections.Generic;
using Patate.Application;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Patate.UnityAdapters
{
    [Flags]
    public enum MenuCommand
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        Confirm = 16,
        Back = 32,
    }

    /// <summary>
    /// Commandes des menus, au clavier et à la manette. Un appareil est
    /// désigné par un <see cref="PlayerSource"/> : moitié du clavier
    /// (0 gauche, 1 droite) ou manette (indice dans <c>Gamepad.all</c>).
    /// Le stick répète la direction quand on le maintient ; la croix et les
    /// touches donnent un appui par pression.
    /// </summary>
    public static class MenuInput
    {
        const float StickThreshold = 0.55f, RepeatDelay = 0.4f, RepeatRate = 0.12f;

        static readonly Key[][] LeftHalf = { new[] { Key.W }, new[] { Key.S }, new[] { Key.A }, new[] { Key.D }, new[] { Key.F, Key.Space }, new[] { Key.G } };
        static readonly Key[][] RightHalf =
        {
            new[] { Key.UpArrow }, new[] { Key.DownArrow }, new[] { Key.LeftArrow }, new[] { Key.RightArrow },
            new[] { Key.K, Key.Enter, Key.NumpadEnter, Key.Numpad1 }, new[] { Key.L, Key.Backspace, Key.Numpad2 },
        };

        sealed class Stick
        {
            public MenuCommand Held;
            public double Next;
            public uint Update = uint.MaxValue;
            public MenuCommand Result;
        }

        static readonly Dictionary<int, Stick> sticks = new Dictionary<int, Stick>();

        /// <summary>Tous les appareils réunis : pour les menus que n'importe qui pilote.</summary>
        public static MenuCommand Any()
        {
            var result = Keys(LeftHalf) | Keys(RightHalf);
            var keyboard = Keyboard.current;
            // Échap revient en arrière partout ; G et L ne servent de retour que dans la sélection, joueur par joueur.
            result &= ~MenuCommand.Back;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)) result |= MenuCommand.Back;
            foreach (var pad in Gamepad.all) result |= Pad(pad);
            return result;
        }

        /// <summary>Commandes d'un seul appareil (sélection des personnages).</summary>
        public static MenuCommand Of(PlayerSource device)
        {
            switch (device.Kind)
            {
                case PlayerSourceKind.Keyboard: return Keys(device.Index == 0 ? LeftHalf : RightHalf);
                case PlayerSourceKind.Gamepad: return device.Index < Gamepad.all.Count ? Pad(Gamepad.all[device.Index]) : MenuCommand.None;
                default: return MenuCommand.None;
            }
        }

        /// <summary>Appareils présents : les deux moitiés du clavier, puis chaque manette.</summary>
        public static IEnumerable<PlayerSource> Devices()
        {
            if (Keyboard.current != null)
            {
                yield return PlayerSource.Keyboard(0);
                yield return PlayerSource.Keyboard(1);
            }
            for (int i = 0; i < Gamepad.all.Count; i++) yield return PlayerSource.Gamepad(i);
        }

        /// <summary>Touche ou bouton de validation d'un appareil, pour l'afficher.</summary>
        public static string ConfirmName(PlayerSource device) => device.Kind == PlayerSourceKind.Keyboard
            ? $"{KeyName(device.Index == 0 ? Key.F : Key.K)} ou {(device.Index == 0 ? "Espace" : "Entrée")}"
            : "bouton A";

        public static string BackName(PlayerSource device) => device.Kind == PlayerSourceKind.Keyboard ? KeyName(device.Index == 0 ? Key.G : Key.L) : "bouton B";

        public static string KeyName(Key key)
        {
            var keyboard = Keyboard.current;
            var name = keyboard != null ? keyboard[key].displayName : null;
            return string.IsNullOrEmpty(name) ? key.ToString() : name.ToUpperInvariant();
        }

        public static string DeviceName(PlayerSource device) => device.Kind switch
        {
            PlayerSourceKind.Keyboard => device.Index == 0 ? "Clavier gauche" : "Clavier droit",
            PlayerSourceKind.Gamepad => $"Manette {device.Index + 1}",
            PlayerSourceKind.Cpu => "Ordinateur",
            _ => "En ligne",
        };

        static MenuCommand Keys(Key[][] layout)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return MenuCommand.None;
            var result = MenuCommand.None;
            for (int i = 0; i < layout.Length; i++)
                foreach (var key in layout[i])
                    if (keyboard[key].wasPressedThisFrame) result |= (MenuCommand)(1 << i);
            return result;
        }

        static MenuCommand Pad(Gamepad pad)
        {
            var result = MenuCommand.None;
            if (pad.dpad.up.wasPressedThisFrame) result |= MenuCommand.Up;
            if (pad.dpad.down.wasPressedThisFrame) result |= MenuCommand.Down;
            if (pad.dpad.left.wasPressedThisFrame) result |= MenuCommand.Left;
            if (pad.dpad.right.wasPressedThisFrame) result |= MenuCommand.Right;
            if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame) result |= MenuCommand.Confirm;
            if (pad.buttonEast.wasPressedThisFrame) result |= MenuCommand.Back;
            return result | StickOf(pad);
        }

        /// <summary>Stick gauche : un appui en entrant dans une direction, puis répétition. Calculé une fois par mise à jour des entrées.</summary>
        static MenuCommand StickOf(Gamepad pad)
        {
            if (!sticks.TryGetValue(pad.deviceId, out var stick)) sticks[pad.deviceId] = stick = new Stick();
            if (stick.Update == InputState.updateCount) return stick.Result;
            stick.Update = InputState.updateCount;
            var value = pad.leftStick.ReadValue();
            var held = MenuCommand.None;
            if (Mathf.Abs(value.x) > Mathf.Abs(value.y)) held = value.x > StickThreshold ? MenuCommand.Right : value.x < -StickThreshold ? MenuCommand.Left : MenuCommand.None;
            else held = value.y > StickThreshold ? MenuCommand.Up : value.y < -StickThreshold ? MenuCommand.Down : MenuCommand.None;
            double now = Time.realtimeSinceStartupAsDouble;
            stick.Result = MenuCommand.None;
            if (held != stick.Held)
            {
                stick.Held = held;
                stick.Result = held;
                stick.Next = now + RepeatDelay;
            }
            else if (held != MenuCommand.None && now >= stick.Next)
            {
                stick.Result = held;
                stick.Next = now + RepeatRate;
            }
            return stick.Result;
        }
    }

    /// <summary>
    /// Ce que le combat sait déjà lire. Les manettes et l'ordinateur arrivent
    /// par <see cref="InputSources"/> : tant qu'il répond « pas encore
    /// disponible », les menus le montrent au lieu de lancer un combat
    /// impossible.
    /// </summary>
    public static class InputSupport
    {
        public static bool Available(PlayerSource device)
        {
            try
            {
                var input = InputSources.Create(device, 1, () => null);
                (input as IDisposable)?.Dispose();
                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch (Exception exception)
            {
                // Une source qui échoue autrement reste indisponible : le menu ne doit pas tomber avec elle.
                Debug.LogWarning($"{device} : {exception.Message}");
                return false;
            }
        }
    }
}
