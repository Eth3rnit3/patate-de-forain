using System;
using System.Collections.Generic;
using Patate.Application;

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
        /// <summary>
        /// Sources locales branchées en ce moment, proposées par la sélection
        /// des joueurs. Les manettes s'ajoutent ici.
        /// </summary>
        public static IEnumerable<PlayerSource> Connected()
        {
            yield return PlayerSource.Keyboard(0);
            yield return PlayerSource.Keyboard(1);
        }

        /// <param name="slot">Place du joueur (0 ou 1).</param>
        /// <param name="session">Combat en cours, pour les sources qui le lisent (IA).</param>
        public static ILocalInput Create(PlayerSource source, int slot, Func<MatchSession> session)
        {
            switch (source.Kind)
            {
                case PlayerSourceKind.Keyboard:
                    return source.Index == 0 ? KeyboardInput.PlayerOne() : KeyboardInput.PlayerTwo();
                default:
                    throw new NotSupportedException($"Source d'entrées pas encore disponible : {source}");
            }
        }
    }
}
