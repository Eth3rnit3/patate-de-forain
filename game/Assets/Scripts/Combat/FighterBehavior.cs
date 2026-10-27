using System;
using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>Progression des actions et décisions d'un combattant.</summary>
    public static class FighterBehavior
    {
        /// <summary>Le combattant peut-il choisir sa prochaine action ce tick ?</summary>
        public static bool IsActionable(FighterState fighter)
        {
            var kind = fighter.Action.Kind;
            return kind == ActionKind.Idle || kind == ActionKind.Walk || kind == ActionKind.Guard;
        }

        /// <summary>
        /// Avance l'action en cours d'un tick et revient au neutre quand elle est
        /// terminée. Les actions d'entrée de segment ne sont déclenchées qu'ici et
        /// au démarrage d'un coup : un gel d'impact ne peut donc jamais les rejouer.
        /// </summary>
        public static void Advance(FighterState fighter, CompiledFighter definition, MatchState state, List<MatchEvent> events)
        {
            var action = fighter.Action;
            action.Tick++;
            switch (action.Kind)
            {
                case ActionKind.Move:
                    var move = definition.Move(action.MoveId);
                    if (move == null || action.Tick >= move.TotalDuration) fighter.Action = ActionState.Of(ActionKind.Idle);
                    else SegmentActions.RunEntries(move, action.Tick, fighter, definition, state, events);
                    break;
                case ActionKind.Hitstun:
                case ActionKind.Blockstun:
                    if (action.Tick >= action.Duration) fighter.Action = ActionState.Of(ActionKind.Idle);
                    break;
            }
        }

        /// <summary>Choix d'un combattant disponible : coup, marche, garde ou repos.</summary>
        public static void Decide(FighterState fighter, Buttons input, SimulationContext context, MatchState state, List<MatchEvent> events)
        {
            var definition = context.Of(fighter);
            foreach (var move in definition.MovesByPriority)
            {
                if (!Triggered(move, fighter) || !SegmentActions.CanStartMove(move, fighter, definition, state)) continue;
                fighter.Action = new ActionState { Kind = ActionKind.Move, MoveId = move.Id };
                events.Add(new MatchEvent { Type = MatchEventType.MoveStart, Tick = state.Tick, Slot = fighter.Slot, MoveId = move.Id });
                SegmentActions.RunEntries(move, 0, fighter, definition, state, events);
                return;
            }

            var intent = InputRules.Horizontal(input, fighter.Facing);
            ActionState next;
            if (intent == HorizontalIntent.Back && IsThreatened(fighter, context, state)) next = ActionState.Of(ActionKind.Guard);
            else if (intent == HorizontalIntent.None) next = ActionState.Of(ActionKind.Idle);
            else next = new ActionState { Kind = ActionKind.Walk, Forward = intent == HorizontalIntent.Forward };
            // Conserver le compteur si rien ne change : les boucles d'animation restent continues.
            if (!fighter.Action.SameNeutralAs(next)) fighter.Action = next;
        }

        static bool Triggered(CompiledMove move, FighterState fighter)
        {
            foreach (var command in move.Definition.Commands)
                if (Commands.Matches(command, fighter.InputHistory, fighter.Facing)) return true;
            return false;
        }

        /// <summary>Menace proche : attaque adverse pas encore finie, ou projectile adverse qui approche.</summary>
        public static bool IsThreatened(FighterState fighter, SimulationContext context, MatchState state)
        {
            int range = context.Rules.ProximityGuardDistance;
            var opponent = state.Fighters[MatchState.Opponent(fighter.Slot)];
            if (opponent.Action.Kind == ActionKind.Move && Math.Abs(opponent.X - fighter.X) <= range)
            {
                var move = context.Of(opponent).Move(opponent.Action.MoveId);
                if (move != null && opponent.Action.Tick < move.LastActiveEnd) return true;
            }
            foreach (var projectile in state.Projectiles)
            {
                if (projectile.Owner == fighter.Slot) continue;
                bool approaching = Math.Sign(fighter.X - projectile.X) == projectile.Facing;
                if (approaching && Math.Abs(fighter.X - projectile.X) <= range) return true;
            }
            return false;
        }
    }
}
