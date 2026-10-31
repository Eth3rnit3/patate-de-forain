using System.Collections.Generic;
using System.Linq;
using Patate.Application;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Règles de l'écran « Qui joue ? », sans Unity : chaque joueur humain
    /// choisit son appareil (moitié de clavier ou manette) parmi ceux
    /// branchés. Deux joueurs n'ont jamais le même appareil : prendre celui
    /// de l'autre les échange. En Solo et en ligne, un seul joueur choisit ;
    /// l'ordinateur ou l'adversaire distant gardent leur place.
    /// </summary>
    public sealed class PlayerAssignment
    {
        public readonly MatchMode Mode;
        /// <summary>Appareil de chaque joueur humain (null si aucun appareil n'est branché).</summary>
        public readonly PlayerSource?[] Devices;
        /// <summary>Joueur dont on change l'appareil.</summary>
        public int Row;

        public int Humans => Devices.Length;

        /// <param name="previous">Réglages précédents : leurs appareils sont repris s'ils sont encore branchés.</param>
        /// <param name="available">Appareils branchés, dans l'ordre d'affichage (<see cref="MenuInput.Devices"/>).</param>
        public PlayerAssignment(MatchMode mode, MatchSetup previous, IReadOnlyList<PlayerSource> available)
        {
            Mode = mode;
            Devices = new PlayerSource?[mode == MatchMode.Versus ? 2 : 1];
            for (int row = 0; row < Humans; row++)
            {
                var wanted = previous.Players[row];
                if (IsHuman(wanted) && available.Contains(wanted) && !Taken(wanted, row)) Devices[row] = wanted;
            }
            for (int row = 0; row < Humans; row++)
                if (Devices[row] == null) Devices[row] = available.Where(d => !Taken(d, row)).Cast<PlayerSource?>().FirstOrDefault();
        }

        static bool IsHuman(PlayerSource source) => source.Kind == PlayerSourceKind.Keyboard || source.Kind == PlayerSourceKind.Gamepad;

        bool Taken(PlayerSource device, int row)
        {
            for (int other = 0; other < Humans; other++)
                if (other != row && Devices[other].Equals(device)) return true;
            return false;
        }

        /// <summary>Tout le monde a un appareil branché.</summary>
        public bool Ready(IReadOnlyList<PlayerSource> available) => Devices.All(d => d.HasValue && available.Contains(d.Value));

        /// <summary>Applique une commande de menu. Renvoie vrai quand on valide avec des appareils prêts.</summary>
        public bool Command(MenuCommand command, IReadOnlyList<PlayerSource> available)
        {
            if ((command & MenuCommand.Up) != 0) Row = (Row + Humans - 1) % Humans;
            if ((command & MenuCommand.Down) != 0) Row = (Row + 1) % Humans;
            if ((command & MenuCommand.Left) != 0) Cycle(-1, available);
            if ((command & MenuCommand.Right) != 0) Cycle(1, available);
            return (command & MenuCommand.Confirm) != 0 && Ready(available);
        }

        /// <summary>Appareil suivant ou précédent ; s'il était à l'autre joueur, celui-ci reçoit l'ancien.</summary>
        void Cycle(int step, IReadOnlyList<PlayerSource> available)
        {
            if (available.Count == 0) return;
            var current = Devices[Row];
            int index = current.HasValue ? available.ToList().IndexOf(current.Value) : -1;
            var next = available[index < 0 ? (step > 0 ? 0 : available.Count - 1) : (index + step + available.Count) % available.Count];
            for (int other = 0; other < Humans; other++)
                if (other != Row && Devices[other].Equals(next)) Devices[other] = current;
            Devices[Row] = next;
        }

        /// <summary>Reporte les appareils dans les réglages ; l'ordinateur et l'adversaire distant n'y sont pas touchés.</summary>
        public void ApplyTo(MatchSetup setup)
        {
            setup.Mode = Mode;
            for (int row = 0; row < Humans; row++)
                if (Devices[row].HasValue) setup.Players[row] = Devices[row].Value;
        }
    }
}
