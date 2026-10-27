using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>
    /// État complet d'un combat : uniquement des valeurs (entiers, chaînes,
    /// listes). Il se copie, se sérialise (StateCodec) et se compare tel quel,
    /// ce qui rend possibles replays, synchronisation et rollback. Les
    /// définitions n'y figurent pas : elles sont référencées par DefinitionRef.
    /// </summary>
    public sealed class MatchState
    {
        public const int FormatVersion = 1;
        public string RulesId;
        public int RulesVersion;
        public int Tick;
        public RoundState Round = new RoundState();
        public FighterState[] Fighters = new FighterState[2];
        /// <summary>Ordre d'apparition : sert d'ordre de résolution.</summary>
        public List<ProjectileState> Projectiles = new List<ProjectileState>();
        public int NextProjectileId = 1;

        public static MatchState Create(MatchRules rules, DefinitionRef first, int firstHealth, DefinitionRef second, int secondHealth)
        {
            int center = (rules.ArenaLeft + rules.ArenaRight) / 2;
            var state = new MatchState { RulesId = rules.Id, RulesVersion = rules.Version };
            state.Round.Timer = rules.RoundTicks;
            state.Fighters[0] = FighterState.Create(0, first, firstHealth, center - rules.StartOffset, 1, rules.InputHistoryLength);
            state.Fighters[1] = FighterState.Create(1, second, secondHealth, center + rules.StartOffset, -1, rules.InputHistoryLength);
            return state;
        }

        public MatchState Clone()
        {
            var copy = (MatchState)MemberwiseClone();
            copy.Round = Round.Clone();
            copy.Fighters = new[] { Fighters[0].Clone(), Fighters[1].Clone() };
            copy.Projectiles = new List<ProjectileState>(Projectiles.Count);
            foreach (var projectile in Projectiles) copy.Projectiles.Add(projectile.Clone());
            return copy;
        }

        public static int Opponent(int slot) => 1 - slot;
    }

    public enum RoundPhase { Intro, Fight, KO, TimeOver, Over }

    public sealed class RoundState
    {
        public RoundPhase Phase = RoundPhase.Intro;
        /// <summary>Ticks écoulés dans la phase courante.</summary>
        public int PhaseTick;
        /// <summary>Ticks restants au chronomètre.</summary>
        public int Timer;
        /// <summary>-1 = pas encore de vainqueur, 0 ou 1 = emplacement gagnant, 2 = égalité.</summary>
        public int Winner = -1;

        public const int Draw = 2;

        public RoundState Clone() => (RoundState)MemberwiseClone();
    }

    public enum ActionKind
    {
        Idle,
        Walk,
        /// <summary>Garde de proximité : tient arrière face à une menace, ne recule pas.</summary>
        Guard,
        Move,
        Hitstun,
        Blockstun,
        KO,
    }

    /// <summary>Action en cours. Une seule forme plate pour rester simple à copier et sérialiser.</summary>
    public sealed class ActionState
    {
        public ActionKind Kind;
        public int Tick;
        /// <summary>Walk : true = vers l'avant.</summary>
        public bool Forward;
        /// <summary>Move : identifiant du coup.</summary>
        public string MoveId;
        /// <summary>Move : le coup a déjà touché (ou été bloqué) une fois.</summary>
        public bool HasHit;
        /// <summary>Hitstun / Blockstun : durée totale.</summary>
        public int Duration;

        public static ActionState Of(ActionKind kind) => new ActionState { Kind = kind };

        public ActionState Clone() => (ActionState)MemberwiseClone();

        public bool SameNeutralAs(ActionState other) => Kind == other.Kind && (Kind != ActionKind.Walk || Forward == other.Forward);
    }

    public sealed class PushState
    {
        /// <summary>Distance restant à parcourir.</summary>
        public int Remaining;
        public int TicksLeft;
        public int Direction;
        /// <summary>Combattant qui absorbe le recul si la cible est bloquée par un bord (coups au corps), -1 sinon.</summary>
        public int TransferTo = -1;

        public PushState Clone() => (PushState)MemberwiseClone();
    }

    public sealed class FighterState
    {
        public int Slot;
        public DefinitionRef Definition;
        public int X, Y;
        public int Facing;
        public int Health;
        public ActionState Action;
        /// <summary>Ticks de gel restants (impact). Le combattant ne progresse pas pendant ce temps.</summary>
        public int Hitstop;
        public PushState Push;
        /// <summary>Dernières entrées, la plus récente à la fin. Longueur fixe.</summary>
        public Buttons[] InputHistory;

        public static FighterState Create(int slot, DefinitionRef definition, int health, int x, int facing, int historyLength) =>
            new FighterState
            {
                Slot = slot,
                Definition = definition,
                X = x,
                Facing = facing,
                Health = health,
                Action = ActionState.Of(ActionKind.Idle),
                InputHistory = new Buttons[historyLength],
            };

        public Buttons CurrentInput => InputHistory[InputHistory.Length - 1];

        public void RecordInput(Buttons input)
        {
            System.Array.Copy(InputHistory, 1, InputHistory, 0, InputHistory.Length - 1);
            InputHistory[InputHistory.Length - 1] = input;
        }

        public FighterState Clone()
        {
            var copy = (FighterState)MemberwiseClone();
            copy.Action = Action.Clone();
            copy.Push = Push?.Clone();
            copy.InputHistory = (Buttons[])InputHistory.Clone();
            return copy;
        }
    }

    public sealed class ProjectileState
    {
        public int Id;
        public int Owner;
        public string ProjectileId;
        public int X, Y;
        public int Facing;
        public int Tick;

        public ProjectileState Clone() => (ProjectileState)MemberwiseClone();
    }
}
