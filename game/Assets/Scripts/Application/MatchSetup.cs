using System;

namespace Patate.Application
{
    public enum MatchMode { Versus, Solo, Online }

    public enum PlayerSourceKind { Keyboard, Gamepad, Cpu, Remote }

    /// <summary>
    /// D'où viennent les commandes d'un joueur. L'indice dépend du genre :
    /// moitié du clavier (0 gauche, 1 droite), numéro de manette, niveau de
    /// l'IA ; il est ignoré pour un joueur distant.
    /// </summary>
    public readonly struct PlayerSource : IEquatable<PlayerSource>
    {
        public readonly PlayerSourceKind Kind;
        public readonly int Index;

        PlayerSource(PlayerSourceKind kind, int index)
        {
            Kind = kind;
            Index = index;
        }

        public static PlayerSource Keyboard(int half) => new PlayerSource(PlayerSourceKind.Keyboard, half);
        public static PlayerSource Gamepad(int index) => new PlayerSource(PlayerSourceKind.Gamepad, index);
        public static PlayerSource Cpu(int level) => new PlayerSource(PlayerSourceKind.Cpu, level);
        public static PlayerSource Remote() => new PlayerSource(PlayerSourceKind.Remote, 0);

        public bool Equals(PlayerSource other) => Kind == other.Kind && Index == other.Index;
        public override bool Equals(object obj) => obj is PlayerSource other && Equals(other);
        public override int GetHashCode() => (int)Kind * 397 ^ Index;
        public override string ToString() => $"{Kind} {Index}";
    }

    /// <summary>
    /// Ce que les menus produisent et ce que le combat consomme : mode,
    /// combattants, décor et source des commandes de chaque joueur.
    /// </summary>
    public sealed class MatchSetup
    {
        public MatchMode Mode = MatchMode.Versus;
        public string[] Fighters = { "saint-louis", "saint-louis" };
        public string Stage = "fete-foraine";
        public PlayerSource[] Players = { PlayerSource.Keyboard(0), PlayerSource.Keyboard(1) };

        /// <summary>Combat miroir à deux sur un clavier : le prototype d'origine.</summary>
        public static MatchSetup Default() => new MatchSetup();

        public MatchSetup Copy() => new MatchSetup
        {
            Mode = Mode,
            Fighters = (string[])Fighters.Clone(),
            Stage = Stage,
            Players = (PlayerSource[])Players.Clone(),
        };
    }
}
