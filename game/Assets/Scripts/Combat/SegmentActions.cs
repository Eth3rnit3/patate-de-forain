using System;
using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>
    /// Comportement déclarable dans les données, déclenché une fois à l'entrée
    /// d'un segment. Vocabulaire FERMÉ : un paquet de personnage ne peut que
    /// référencer les actions connues du moteur, jamais fournir du code.
    /// </summary>
    public abstract class SegmentAction
    {
        /// <summary>Nom utilisé dans les données (champ "type").</summary>
        public abstract string Type { get; }
    }

    public sealed class SpawnProjectileAction : SegmentAction
    {
        public const string TypeName = "spawnProjectile";
        public override string Type => TypeName;
        public readonly string Projectile;
        /// <summary>Point d'émission relatif au pivot, personnage regardant à droite.</summary>
        public readonly PointI Offset;

        public SpawnProjectileAction(string projectile, PointI offset)
        {
            Projectile = projectile; Offset = offset;
        }
    }

    /// <summary>
    /// Point d'extension du moteur. Chaque type d'action a ici un traitement
    /// unique, réutilisable par tous les combattants. Ajouter une mécanique =
    /// une classe SegmentAction, un traitement ici, une lecture dans
    /// Patate.Content (FighterPackageReader). Jamais de condition sur l'identité
    /// d'un personnage.
    /// </summary>
    public static class SegmentActions
    {
        interface IHandler
        {
            bool CanStart(SegmentAction action, FighterState fighter, CompiledFighter definition, MatchState state);
            void Run(SegmentAction action, FighterState fighter, CompiledFighter definition, MatchState state, List<MatchEvent> events);
        }

        sealed class Handler<T> : IHandler where T : SegmentAction
        {
            readonly Func<T, FighterState, CompiledFighter, MatchState, bool> canStart;
            readonly Action<T, FighterState, CompiledFighter, MatchState, List<MatchEvent>> run;

            public Handler(Func<T, FighterState, CompiledFighter, MatchState, bool> canStart, Action<T, FighterState, CompiledFighter, MatchState, List<MatchEvent>> run)
            {
                this.canStart = canStart; this.run = run;
            }

            public bool CanStart(SegmentAction a, FighterState f, CompiledFighter d, MatchState s) => canStart((T)a, f, d, s);
            public void Run(SegmentAction a, FighterState f, CompiledFighter d, MatchState s, List<MatchEvent> e) => run((T)a, f, d, s, e);
        }

        static readonly Dictionary<string, IHandler> handlers = new Dictionary<string, IHandler>
        {
            [SpawnProjectileAction.TypeName] = new Handler<SpawnProjectileAction>(CanSpawn, Spawn),
        };

        /// <summary>Types d'actions que les données peuvent utiliser (vérifié à la validation).</summary>
        public static IEnumerable<string> KnownTypes => handlers.Keys;

        static IHandler HandlerFor(SegmentAction action) =>
            handlers.TryGetValue(action.Type, out var handler) ? handler : throw new InvalidOperationException($"Action inconnue : {action.Type}");

        public static bool CanStartMove(CompiledMove move, FighterState fighter, CompiledFighter definition, MatchState state)
        {
            foreach (var segment in move.Segments)
                foreach (var action in segment.Definition.OnEnter)
                    if (!HandlerFor(action).CanStart(action, fighter, definition, state)) return false;
            return true;
        }

        /// <summary>Déclenche les actions des segments qui commencent exactement à <paramref name="tick"/>.</summary>
        public static void RunEntries(CompiledMove move, int tick, FighterState fighter, CompiledFighter definition, MatchState state, List<MatchEvent> events)
        {
            foreach (var segment in move.Segments)
            {
                if (segment.Start != tick) continue;
                foreach (var action in segment.Definition.OnEnter) HandlerFor(action).Run(action, fighter, definition, state, events);
            }
        }

        /// <summary>La limite est comptée par lanceur : en combat miroir, chacun a la sienne.</summary>
        static bool CanSpawn(SpawnProjectileAction action, FighterState fighter, CompiledFighter definition, MatchState state)
        {
            var projectile = definition.Projectile(action.Projectile);
            if (projectile == null) return false;
            int active = 0;
            foreach (var candidate in state.Projectiles)
                if (candidate.Owner == fighter.Slot && candidate.ProjectileId == action.Projectile) active++;
            return active < projectile.MaxActive;
        }

        static void Spawn(SpawnProjectileAction action, FighterState fighter, CompiledFighter definition, MatchState state, List<MatchEvent> events)
        {
            var projectile = new ProjectileState
            {
                Id = state.NextProjectileId++,
                Owner = fighter.Slot,
                ProjectileId = action.Projectile,
                X = fighter.X + action.Offset.X * fighter.Facing,
                Y = fighter.Y + action.Offset.Y,
                Facing = fighter.Facing,
            };
            state.Projectiles.Add(projectile);
            events.Add(new MatchEvent
            {
                Type = MatchEventType.ProjectileSpawn,
                Tick = state.Tick,
                Slot = fighter.Slot,
                ProjectileId = projectile.ProjectileId,
                ProjectileInstance = projectile.Id,
                X = projectile.X,
                Y = projectile.Y,
                Facing = projectile.Facing,
            });
        }
    }
}
