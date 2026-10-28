using System;
using System.Collections.Generic;
using Patate.Application;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Entrées d'un joueur sur cette machine (clavier, manette, IA), lues une
    /// fois par image d'affichage puis fournies tick par tick à la session.
    /// </summary>
    public interface ILocalInput : IInputProvider
    {
        string Label { get; }
        void Sample();
        /// <summary>Aide lisible : action et touches ou boutons correspondants.</summary>
        IEnumerable<(string action, string keys)> Help();
    }

    /// <summary>
    /// Crée les entrées décrites par un <see cref="PlayerSource"/>. Point
    /// d'extension des manettes, de l'IA et du réseau.
    /// </summary>
    public static class InputSources
    {
        /// <summary>Moitiés du clavier, pour détecter qui appuie (sans état propre à un joueur).</summary>
        static readonly KeyboardInput[] keyboardHalves = { KeyboardInput.PlayerOne(), KeyboardInput.PlayerTwo() };

        /// <summary>
        /// Sources locales branchées en ce moment, proposées par la sélection
        /// des joueurs : les deux moitiés du clavier puis chaque manette. En
        /// WebGL, le navigateur ne signale une manette qu'après un premier appui.
        /// </summary>
        public static IEnumerable<PlayerSource> Connected()
        {
            yield return PlayerSource.Keyboard(0);
            yield return PlayerSource.Keyboard(1);
            foreach (var pad in Gamepad.all) yield return PlayerSource.Gamepad(pad.deviceId);
        }

        /// <summary>
        /// Appuyer pour rejoindre : sources qui viennent d'enfoncer un bouton
        /// pendant cette image (touche d'attaque d'une moitié du clavier ;
        /// bouton de face, Start ou gâchette haute d'une manette). À appeler
        /// une fois par image ; c'est à l'écran de retenir qui a rejoint.
        /// </summary>
        public static IEnumerable<PlayerSource> JustPressed()
        {
            for (int half = 0; half < keyboardHalves.Length; half++)
                if (keyboardHalves[half].JustPressed()) yield return PlayerSource.Keyboard(half);
            foreach (var pad in Gamepad.all)
                if (GamepadInput.JustPressed(pad)) yield return PlayerSource.Gamepad(pad.deviceId);
        }

        /// <summary>Nom lisible d'une source, pour les menus.</summary>
        public static string Describe(PlayerSource source)
        {
            switch (source.Kind)
            {
                case PlayerSourceKind.Keyboard: return source.Index == 0 ? "Clavier gauche" : "Clavier droit";
                case PlayerSourceKind.Gamepad: return GamepadLabel(source.Index);
                case PlayerSourceKind.Cpu: return $"Ordinateur ({CpuPlayer.LevelName(source.Index).ToLowerInvariant()})";
                default: return "En ligne";
            }
        }

        /// <summary>« Manette 1 », « Manette 2 »… selon l'ordre de branchement.</summary>
        static string GamepadLabel(int deviceId)
        {
            for (int index = 0; index < Gamepad.all.Count; index++)
                if (Gamepad.all[index].deviceId == deviceId) return $"Manette {index + 1}";
            return "Manette débranchée";
        }

        /// <param name="source">Pour une manette, l'indice est son identifiant Input System (voir <see cref="Connected"/>).</param>
        /// <param name="slot">Place du joueur (0 ou 1).</param>
        /// <param name="session">Combat en cours, pour les sources qui le lisent (IA). Lu à chaque tick, jamais à la création.</param>
        public static ILocalInput Create(PlayerSource source, int slot, Func<MatchSession> session)
        {
            switch (source.Kind)
            {
                case PlayerSourceKind.Keyboard:
                    return source.Index == 0 ? KeyboardInput.PlayerOne() : KeyboardInput.PlayerTwo();
                case PlayerSourceKind.Gamepad:
                    return new GamepadInput(source.Index, GamepadLabel(source.Index));
                case PlayerSourceKind.Cpu:
                    // Graine différente à chaque combat ; CpuPlayer reste déterministe à graine donnée.
                    return new CpuInput(source.Index, slot, session, unchecked((uint)Environment.TickCount));
                default:
                    throw new NotSupportedException($"Source d'entrées pas encore disponible : {source}");
            }
        }
    }
}
