using System;
using System.Collections.Generic;
using System.Linq;

namespace Patate.Combat
{
    /// <summary>Progression des actions et décisions d'un combattant.</summary>
    public static class FighterBehavior
    {
        /// <summary>Le combattant peut-il choisir sa prochaine action ce tick ? En l'air, seulement un coup sauté.</summary>
        public static bool IsActionable(FighterState fighter)
        {
            var kind = fighter.Action.Kind;
            return kind == ActionKind.Idle || kind == ActionKind.Walk || kind == ActionKind.Guard || kind == ActionKind.Crouch
                || kind == ActionKind.Jump;
        }

        /// <summary>Se tourner vers l'adversaire : au sol et libre uniquement (un saut garde son orientation).</summary>
        public static bool CanTurn(FighterState fighter) =>
            fighter.Posture != Posture.Air && IsActionable(fighter);

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
                    if (move == null || action.Tick >= move.TotalDuration) fighter.Action = Neutral(fighter, definition);
                    else SegmentActions.RunEntries(move, action.Tick, fighter, definition, state, events);
                    break;
                case ActionKind.Hitstun:
                    // Touché en l'air : l'étourdissement dure au moins jusqu'à la réception.
                    if (action.Tick >= action.Duration && fighter.Posture != Posture.Air) fighter.Action = ActionState.Of(ActionKind.Idle);
                    break;
                case ActionKind.Blockstun:
                case ActionKind.Land:
                    if (action.Tick >= action.Duration) fighter.Action = ActionState.Of(ActionKind.Idle);
                    break;
                case ActionKind.PreJump:
                    if (action.Tick >= definition.Gameplay.Stats.Jump.PreJumpTicks) TakeOff(fighter, definition, action.Direction);
                    break;
            }
        }

        /// <summary>Après un coup : en l'air on continue le saut (sans second coup), au sol on revient au repos.</summary>
        static ActionState Neutral(FighterState fighter, CompiledFighter definition)
        {
            if (fighter.Posture != Posture.Air) return ActionState.Of(ActionKind.Idle);
            return new ActionState { Kind = ActionKind.Jump, Tick = definition.Gameplay.Stats.Jump.AirTicks(fighter.VelocityY) };
        }

        static void TakeOff(FighterState fighter, CompiledFighter definition, int direction)
        {
            var jump = definition.Gameplay.Stats.Jump;
            fighter.Posture = Posture.Air;
            fighter.VelocityY = jump.Velocity;
            fighter.VelocityX = direction == 0 ? 0 : direction * (direction == fighter.Facing ? jump.ForwardSpeed : jump.BackSpeed);
            fighter.AirMoveUsed = false;
            fighter.Action = ActionState.Of(ActionKind.Jump);
        }

        /// <summary>Réception au sol (appelé par la physique quand Y revient à 0).</summary>
        public static void Land(FighterState fighter, CompiledFighter definition)
        {
            fighter.Posture = Posture.Stand;
            fighter.Y = 0;
            fighter.VelocityX = 0;
            fighter.VelocityY = 0;
            var landing = definition.Gameplay.Stats.Jump.LandingTicks;
            switch (fighter.Action.Kind)
            {
                case ActionKind.Jump:
                case ActionKind.Move: // un coup sauté s'arrête à la réception
                    fighter.Action = new ActionState { Kind = ActionKind.Land, Duration = landing };
                    break;
                case ActionKind.Hitstun:
                    fighter.Action.Duration = Math.Max(fighter.Action.Duration, fighter.Action.Tick + landing);
                    break;
            }
        }

        /// <summary>Choix d'un combattant disponible : coup, saut, accroupi, marche, garde ou repos.</summary>
        public static void Decide(FighterState fighter, Buttons input, SimulationContext context, MatchState state, List<MatchEvent> events)
        {
            var definition = context.Of(fighter);
            if (fighter.Posture == Posture.Air)
            {
                if (fighter.Action.Kind == ActionKind.Jump && !fighter.AirMoveUsed && TryStartMove(fighter, MoveStance.Air, definition, state, events))
                    fighter.AirMoveUsed = true;
                return;
            }

            int direction = InputRules.Direction(input, fighter.Facing);
            bool crouching = InputRules.IsCrouching(direction);
            if (TryStartMove(fighter, crouching ? MoveStance.Crouch : MoveStance.Stand, definition, state, events))
            {
                fighter.Posture = crouching ? Posture.Crouch : Posture.Stand;
                return;
            }

            var intent = InputRules.Horizontal(input, fighter.Facing);
            if (InputRules.IsJumping(direction))
            {
                fighter.Posture = Posture.Stand;
                int sense = intent == HorizontalIntent.Forward ? fighter.Facing : intent == HorizontalIntent.Back ? -fighter.Facing : 0;
                fighter.Action = new ActionState { Kind = ActionKind.PreJump, Direction = sense };
                return;
            }

            ActionState next;
            bool guarding = intent == HorizontalIntent.Back && IsThreatened(fighter, context, state);
            if (crouching) next = ActionState.Of(guarding ? ActionKind.Guard : ActionKind.Crouch);
            else if (guarding) next = ActionState.Of(ActionKind.Guard);
            else if (intent == HorizontalIntent.None) next = ActionState.Of(ActionKind.Idle);
            else next = new ActionState { Kind = ActionKind.Walk, Forward = intent == HorizontalIntent.Forward };
            var posture = crouching ? Posture.Crouch : Posture.Stand;
            // Conserver le compteur si rien ne change : les boucles d'animation restent continues.
            if (!fighter.Action.SameNeutralAs(next) || fighter.Posture != posture) fighter.Action = next;
            fighter.Posture = posture;
        }

        static bool TryStartMove(FighterState fighter, MoveStance stance, CompiledFighter definition, MatchState state, List<MatchEvent> events)
        {
            foreach (var move in definition.MovesByPriority)
            {
                if (move.Definition.CancelOnly || move.Definition.Stance != stance || !Triggered(move, fighter) || !SegmentActions.CanStartMove(move, fighter, definition, state)) continue;
                StartMove(fighter, move, definition, state, events);
                return true;
            }
            return false;
        }

        static void StartMove(FighterState fighter, CompiledMove move, CompiledFighter definition, MatchState state, List<MatchEvent> events)
        {
            fighter.Action = new ActionState { Kind = ActionKind.Move, MoveId = move.Id };
            events.Add(new MatchEvent { Type = MatchEventType.MoveStart, Tick = state.Tick, Slot = fighter.Slot, MoveId = move.Id });
            SegmentActions.RunEntries(move, 0, fighter, definition, state, events);
        }

        /// <summary>
        /// Annulation : un coup au sol qui a touché (ou été gardé, selon la liste)
        /// cède la place à l'un des coups qu'il déclare, dès l'appui. Appelé aussi
        /// pendant le gel d'impact : le nouveau coup démarre quand le gel se termine.
        /// </summary>
        public static bool TryCancel(FighterState fighter, Buttons input, CompiledFighter definition, MatchState state, List<MatchEvent> events)
        {
            var action = fighter.Action;
            if (action.Kind != ActionKind.Move || !action.HasHit || fighter.Posture == Posture.Air) return false;
            var current = definition.Move(action.MoveId);
            if (current == null) return false;
            bool crouching = InputRules.IsCrouching(InputRules.Direction(input, fighter.Facing));
            var stance = crouching ? MoveStance.Crouch : MoveStance.Stand;
            foreach (var move in definition.MovesByPriority)
            {
                bool allowed = current.Definition.Cancels.Contains(move.Id) || (action.HitLanded && current.Definition.CancelsOnHit.Contains(move.Id));
                if (!allowed || move.Definition.Stance != stance || !Triggered(move, fighter) || !SegmentActions.CanStartMove(move, fighter, definition, state)) continue;
                fighter.Posture = crouching ? Posture.Crouch : Posture.Stand;
                StartMove(fighter, move, definition, state, events);
                return true;
            }
            return false;
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
