using System.Collections.Generic;
using System.Linq;
using Patate.Application;

namespace Patate.UnityAdapters
{
    /// <summary>Une place de la sélection : vide, puis occupée par un appareil (ou l'ordinateur).</summary>
    public sealed class SelectionSlot
    {
        /// <summary>Appareil du joueur, l'ordinateur, ou null tant que personne n'a rejoint.</summary>
        public PlayerSource? Source;
        /// <summary>Combattant survolé (indice dans la liste des combattants).</summary>
        public int Cursor;
        public bool Locked;

        public bool Joined => Source.HasValue;
        public bool IsCpu => Source?.Kind == PlayerSourceKind.Cpu;
    }

    public enum SelectionPhase { Fighters, Stage, Done }

    /// <summary>
    /// Règles de l'écran de sélection, sans Unity : qui rejoint quelle place,
    /// qui déplace quel curseur, et quand le combat peut commencer. En Solo,
    /// le joueur choisit son combattant puis celui de l'ordinateur. Le décor
    /// se choisit à la fin s'il y en a plusieurs. Le résultat est un
    /// <see cref="MatchSetup"/>.
    /// </summary>
    public sealed class CharacterSelection
    {
        public readonly MatchMode Mode;
        public readonly IReadOnlyList<string> Fighters;
        public readonly IReadOnlyList<string> Stages;
        public readonly SelectionSlot[] Slots = { new SelectionSlot(), new SelectionSlot() };
        public int StageCursor;
        public SelectionPhase Phase { get; private set; } = SelectionPhase.Fighters;

        /// <summary>Colonnes de la grille des portraits.</summary>
        public int Columns => System.Math.Min(System.Math.Max(1, Fighters.Count), 6);

        /// <param name="previous">Réglages précédents : curseurs et décor de départ, et joueurs déjà placés si <paramref name="keepPlayers"/>.</param>
        /// <param name="cpu">Source de l'ordinateur en Solo (niveau compris).</param>
        public CharacterSelection(MatchMode mode, IReadOnlyList<string> fighters, IReadOnlyList<string> stages, MatchSetup previous, PlayerSource? cpu = null, bool keepPlayers = false)
        {
            Mode = mode;
            Fighters = fighters;
            Stages = stages.Count > 0 ? stages : new[] { previous.Stage };
            StageCursor = System.Math.Max(0, Stages.ToList().IndexOf(previous.Stage));
            for (int slot = 0; slot < 2; slot++)
            {
                Slots[slot].Cursor = System.Math.Max(0, fighters.ToList().IndexOf(previous.Fighters[slot]));
                if (keepPlayers && previous.Mode == mode && previous.Players[slot].Kind != PlayerSourceKind.Cpu && previous.Players[slot].Kind != PlayerSourceKind.Remote)
                    Slots[slot].Source = previous.Players[slot];
            }
            if (mode == MatchMode.Solo) Slots[1].Source = cpu ?? PlayerSource.Cpu(1);
        }

        public int SlotOf(PlayerSource device)
        {
            for (int slot = 0; slot < 2; slot++)
                if (Slots[slot].Source.Equals(device)) return slot;
            return -1;
        }

        /// <summary>Un appareil rejoint la première place libre. Renvoie la place, ou -1 si tout est occupé.</summary>
        public int Join(PlayerSource device)
        {
            int existing = SlotOf(device);
            if (existing >= 0) return existing;
            if (Phase != SelectionPhase.Fighters) return -1;
            for (int slot = 0; slot < 2; slot++)
            {
                if (Slots[slot].Joined) continue;
                Slots[slot].Source = device;
                Slots[slot].Locked = false;
                return slot;
            }
            return -1;
        }

        /// <summary>Place dont cet appareil déplace le curseur en ce moment (en Solo, celle de l'ordinateur une fois son propre choix fait).</summary>
        public int ControlledSlot(PlayerSource device)
        {
            int own = SlotOf(device);
            if (own < 0) return -1;
            if (Mode == MatchMode.Solo && own == 0 && Slots[0].Locked) return 1;
            return own;
        }

        /// <summary>Applique les commandes d'un appareil déjà placé.</summary>
        public void Command(PlayerSource device, MenuCommand command)
        {
            int slot = ControlledSlot(device);
            if (slot < 0 || command == MenuCommand.None) return;
            if (Phase == SelectionPhase.Stage)
            {
                if ((command & MenuCommand.Left) != 0) StageCursor = (StageCursor + Stages.Count - 1) % Stages.Count;
                if ((command & MenuCommand.Right) != 0) StageCursor = (StageCursor + 1) % Stages.Count;
                if ((command & MenuCommand.Confirm) != 0) Phase = SelectionPhase.Done;
                else if ((command & MenuCommand.Back) != 0)
                {
                    Slots[slot].Locked = false;
                    Phase = SelectionPhase.Fighters;
                }
                return;
            }
            if (Phase != SelectionPhase.Fighters) return;

            var target = Slots[slot];
            if ((command & MenuCommand.Back) != 0)
            {
                if (target.Locked) target.Locked = false;
                else if (target.IsCpu) Slots[0].Locked = false; // Solo : revenir à son propre choix
                else
                {
                    target.Source = null; // quitter la place
                    if (Mode == MatchMode.Solo) Slots[1].Locked = false;
                }
                return;
            }
            if (target.Locked) return;
            int count = Fighters.Count;
            if (count == 0) return;
            if ((command & MenuCommand.Left) != 0) target.Cursor = (target.Cursor + count - 1) % count;
            if ((command & MenuCommand.Right) != 0) target.Cursor = (target.Cursor + 1) % count;
            if ((command & MenuCommand.Up) != 0 && count > Columns) target.Cursor = (target.Cursor - Columns + count * Columns) % count;
            if ((command & MenuCommand.Down) != 0 && count > Columns) target.Cursor = (target.Cursor + Columns) % count;
            if ((command & MenuCommand.Confirm) != 0)
            {
                target.Locked = true;
                if (Slots.All(s => s.Joined && s.Locked)) Phase = Stages.Count > 1 ? SelectionPhase.Stage : SelectionPhase.Done;
            }
        }

        public MatchSetup ToSetup() => new MatchSetup
        {
            Mode = Mode,
            Fighters = Slots.Select(s => Fighters[s.Cursor]).ToArray(),
            Stage = Stages[StageCursor],
            Players = Slots.Select(s => s.Source ?? PlayerSource.Keyboard(0)).ToArray(),
        };
    }
}
