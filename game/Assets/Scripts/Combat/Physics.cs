using System;

namespace Patate.Combat
{
    /// <summary>
    /// Déplacements, collision corporelle et limites. Ordre stable (emplacement 0
    /// puis 1) et arithmétique entière uniquement.
    /// </summary>
    public static class FighterPhysics
    {
        public static void Move(MatchState state, SimulationContext context, bool[] frozen)
        {
            var displacement = new int[2];
            for (int slot = 0; slot < 2; slot++)
            {
                var fighter = state.Fighters[slot];
                if (frozen[slot]) continue;
                int before = fighter.X;
                fighter.X += IntendedVelocity(fighter, context);
                Fall(fighter, context);
                int transferTo = fighter.Push?.TransferTo ?? -1;
                int pushed = ApplyPush(fighter);
                fighter.X += pushed;
                int correction = ClampToArena(fighter, context);
                // Cible bloquée contre un bord : l'attaquant absorbe le recul qu'elle n'a pas pu faire.
                if (correction != 0 && pushed != 0 && transferTo >= 0)
                {
                    var attacker = state.Fighters[transferTo];
                    attacker.X += Math.Sign(correction) * Math.Min(Math.Abs(correction), Math.Abs(pushed));
                    ClampToArena(attacker, context);
                }
                displacement[slot] = fighter.X - before;
            }
            LimitSeparation(state, context, displacement);
            SeparateBodies(state, context);
        }

        /// <summary>Trajectoire verticale entière ; la réception remet au sol (Y = 0).</summary>
        static void Fall(FighterState fighter, SimulationContext context)
        {
            if (fighter.Posture != Posture.Air) return;
            var definition = context.Of(fighter);
            fighter.Y += fighter.VelocityY;
            fighter.VelocityY -= definition.Gameplay.Stats.Jump.Gravity;
            if (fighter.Y <= 0) FighterBehavior.Land(fighter, definition);
        }

        static int IntendedVelocity(FighterState fighter, SimulationContext context)
        {
            // En l'air, la vitesse du saut remplace la marche et l'avancée des coups.
            if (fighter.Posture == Posture.Air) return fighter.VelocityX;
            var definition = context.Of(fighter);
            var stats = definition.Gameplay.Stats;
            switch (fighter.Action.Kind)
            {
                case ActionKind.Walk:
                    return fighter.Action.Forward ? stats.WalkForwardSpeed * fighter.Facing : -stats.WalkBackSpeed * fighter.Facing;
                case ActionKind.Move:
                    return (Hitboxes.CurrentSegment(fighter, definition)?.Definition.Advance ?? 0) * fighter.Facing;
                default:
                    return 0;
            }
        }

        /// <summary>Recul réparti sur plusieurs ticks ; renvoie le déplacement de ce tick.</summary>
        static int ApplyPush(FighterState fighter)
        {
            var push = fighter.Push;
            if (push == null) return 0;
            int step = (push.Remaining + push.TicksLeft - 1) / push.TicksLeft; // division entière arrondie au-dessus
            push.Remaining -= step;
            push.TicksLeft--;
            if (push.TicksLeft <= 0 || push.Remaining <= 0) fighter.Push = null;
            return step * push.Direction;
        }

        /// <summary>Ramène le combattant dans l'arène ; renvoie la correction appliquée (signée).</summary>
        static int ClampToArena(FighterState fighter, SimulationContext context)
        {
            var box = Hitboxes.Pushbox(fighter, context.Of(fighter));
            int correction = 0;
            if (box.Left < context.Rules.ArenaLeft) correction = context.Rules.ArenaLeft - box.Left;
            else if (box.Right > context.Rules.ArenaRight) correction = context.Rules.ArenaRight - box.Right;
            fighter.X += correction;
            return correction;
        }

        /// <summary>La caméra ne peut pas cadrer deux combattants trop éloignés : on retient celui qui s'éloigne.</summary>
        static void LimitSeparation(MatchState state, SimulationContext context, int[] displacement)
        {
            for (int slot = 0; slot < 2; slot++)
            {
                var fighter = state.Fighters[slot];
                var other = state.Fighters[MatchState.Opponent(slot)];
                int excess = Math.Abs(fighter.X - other.X) - context.Rules.MaxSeparation;
                if (excess <= 0) return;
                int away = Math.Sign(fighter.X - other.X);
                if (Math.Sign(displacement[slot]) != away) continue;
                fighter.X -= away * Math.Min(excess, Math.Abs(displacement[slot]));
            }
        }

        /// <summary>
        /// Les pushboxes ne se chevauchent jamais : chacun recule de moitié, un bord reporte sur l'autre.
        /// Un combattant assez haut en l'air passe au-dessus de l'autre.
        /// </summary>
        static void SeparateBodies(MatchState state, SimulationContext context)
        {
            var a = state.Fighters[0];
            var b = state.Fighters[1];
            var boxA = Hitboxes.Pushbox(a, context.Fighters[0]);
            var boxB = Hitboxes.Pushbox(b, context.Fighters[1]);
            if (boxA.Bottom >= boxB.Top || boxB.Bottom >= boxA.Top) return;
            int overlap = Math.Min(boxA.Right, boxB.Right) - Math.Max(boxA.Left, boxB.Left);
            if (overlap <= 0) return;
            // À position égale, l'emplacement 0 est considéré à gauche : résultat déterministe.
            var left = a.X <= b.X ? a : b;
            var right = left == a ? b : a;
            int half = overlap / 2;
            left.X -= half;
            right.X += overlap - half;
            int leftCorrection = ClampToArena(left, context);
            int rightCorrection = ClampToArena(right, context);
            if (leftCorrection != 0) right.X += leftCorrection;
            if (rightCorrection != 0) left.X += rightCorrection;
            ClampToArena(left, context);
            ClampToArena(right, context);
        }
    }
}
