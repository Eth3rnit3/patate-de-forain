using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>
    /// Avance un combat d'exactement un tick. Fonction pure : l'état d'entrée
    /// n'est pas modifié, et (état, entrées, contexte) identiques donnent
    /// toujours le même résultat. C'est la base des replays et d'un futur
    /// rollback réseau. Aucune dépendance au temps réel, au rendu ou au hasard.
    ///
    /// Ordre de résolution (stable, emplacement 0 avant 1) :
    ///  1. manche (introduction, chronomètre) et enregistrement des entrées ;
    ///  2. projectiles existants : déplacement, durée de vie, sortie d'arène ;
    ///  3. combattants : gel d'impact, progression de l'action, décision ;
    ///  4. déplacements, collision corporelle, limites, orientation ;
    ///  5. contacts (tous détectés, puis appliqués) ;
    ///  6. fin de manche.
    /// </summary>
    public static class Simulation
    {
        public static MatchState Step(MatchState previous, Buttons first, Buttons second, SimulationContext context, List<MatchEvent> events)
        {
            var state = previous.Clone();
            state.Tick++;

            bool fighting = UpdateRound(state, context, events);
            var inputs = new[] { fighting ? first : Buttons.None, fighting ? second : Buttons.None };
            for (int slot = 0; slot < 2; slot++) state.Fighters[slot].RecordInput(inputs[slot]);

            AdvanceProjectiles(state, context, events);

            var frozen = new bool[2];
            for (int slot = 0; slot < 2; slot++)
            {
                var fighter = state.Fighters[slot];
                if (fighter.Hitstop > 0)
                {
                    fighter.Hitstop--;
                    frozen[slot] = true;
                    continue;
                }
                FighterBehavior.Advance(fighter, context.Fighters[slot], state, events);
                if (FighterBehavior.IsActionable(fighter)) FighterBehavior.Decide(fighter, inputs[slot], context, state, events);
            }

            FighterPhysics.Move(state, context, frozen);
            UpdateFacing(state, frozen);
            if (fighting) CombatResolution.Resolve(state, context, events);
            ConcludeRound(state, events);
            return state;
        }

        /// <summary>Renvoie vrai si les combattants peuvent agir pendant ce tick.</summary>
        static bool UpdateRound(MatchState state, SimulationContext context, List<MatchEvent> events)
        {
            var round = state.Round;
            round.PhaseTick++;
            switch (round.Phase)
            {
                case RoundPhase.Intro:
                    if (round.PhaseTick < context.Rules.IntroTicks) return false;
                    round.Phase = RoundPhase.Fight;
                    round.PhaseTick = 0;
                    events.Add(new MatchEvent { Type = MatchEventType.RoundStart, Tick = state.Tick });
                    return true;
                case RoundPhase.Fight:
                    if (round.Timer > 0) round.Timer--;
                    return true;
                case RoundPhase.KO:
                case RoundPhase.TimeOver:
                    if (round.PhaseTick >= context.Rules.OutroTicks)
                    {
                        round.Phase = RoundPhase.Over;
                        round.PhaseTick = 0;
                        events.Add(new MatchEvent { Type = MatchEventType.RoundOver, Tick = state.Tick, Winner = round.Winner });
                    }
                    return false;
                default:
                    return false;
            }
        }

        static void ConcludeRound(MatchState state, List<MatchEvent> events)
        {
            var round = state.Round;
            if (round.Phase != RoundPhase.Fight) return;
            var a = state.Fighters[0];
            var b = state.Fighters[1];
            if (a.Health == 0 || b.Health == 0)
            {
                round.Phase = RoundPhase.KO;
                round.PhaseTick = 0;
                round.Winner = a.Health == 0 && b.Health == 0 ? RoundState.Draw : a.Health == 0 ? 1 : 0;
            }
            else if (round.Timer == 0)
            {
                round.Phase = RoundPhase.TimeOver;
                round.PhaseTick = 0;
                round.Winner = a.Health == b.Health ? RoundState.Draw : a.Health > b.Health ? 0 : 1;
                events.Add(new MatchEvent { Type = MatchEventType.TimeOver, Tick = state.Tick });
            }
        }

        static void AdvanceProjectiles(MatchState state, SimulationContext context, List<MatchEvent> events)
        {
            var rules = context.Rules;
            foreach (var projectile in state.Projectiles.ToArray())
            {
                var definition = context.Fighters[projectile.Owner].Projectile(projectile.ProjectileId);
                if (definition == null)
                {
                    CombatResolution.RemoveProjectile(state, projectile, ProjectileEndReason.Expired, events);
                    continue;
                }
                projectile.X += definition.Speed * projectile.Facing;
                projectile.Tick++;
                if (projectile.Tick >= definition.Lifetime)
                    CombatResolution.RemoveProjectile(state, projectile, ProjectileEndReason.Expired, events);
                else if (projectile.X < rules.ArenaLeft - rules.ProjectileArenaMargin || projectile.X > rules.ArenaRight + rules.ProjectileArenaMargin)
                    CombatResolution.RemoveProjectile(state, projectile, ProjectileEndReason.OutOfArena, events);
            }
        }

        /// <summary>Orientation automatique vers l'adversaire, seulement quand le combattant est libre.</summary>
        static void UpdateFacing(MatchState state, bool[] frozen)
        {
            for (int slot = 0; slot < 2; slot++)
            {
                var fighter = state.Fighters[slot];
                if (frozen[slot] || !FighterBehavior.IsActionable(fighter)) continue;
                var opponent = state.Fighters[MatchState.Opponent(slot)];
                if (opponent.X != fighter.X) fighter.Facing = opponent.X > fighter.X ? 1 : -1;
            }
        }
    }
}
