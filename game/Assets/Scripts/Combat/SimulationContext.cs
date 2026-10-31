using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>Données immuables nécessaires pour avancer un combat. Jamais sérialisées avec l'état.</summary>
    public sealed class SimulationContext
    {
        public readonly MatchRules Rules;
        /// <summary>Définition de chaque emplacement ; un combat miroir passe deux fois la même.</summary>
        public readonly CompiledFighter[] Fighters;

        public SimulationContext(MatchRules rules, CompiledFighter first, CompiledFighter second)
        {
            Rules = rules;
            Fighters = new[] { first, second };
        }

        public CompiledFighter Of(FighterState fighter) => Fighters[fighter.Slot];
    }

    /// <summary>
    /// Requêtes de collision en lecture seule, utilisées par les règles ET par
    /// l'affichage de débogage : ce que l'on voit est exactement ce qui est testé.
    /// </summary>
    public static class Hitboxes
    {
        public static CompiledSegment CurrentSegment(FighterState fighter, CompiledFighter definition)
        {
            if (fighter.Action.Kind != ActionKind.Move) return null;
            return definition.Move(fighter.Action.MoveId)?.SegmentAt(fighter.Action.Tick);
        }

        public static WorldBox Pushbox(FighterState fighter, CompiledFighter definition) =>
            Geometry.ToWorld(definition.Gameplay.Body.For(fighter.Posture).Pushbox, fighter.X, fighter.Y, fighter.Facing);

        public static List<WorldBox> Hurtboxes(FighterState fighter, CompiledFighter definition)
        {
            var result = new List<WorldBox>();
            if (fighter.Action.Kind == ActionKind.KO) return result;
            var boxes = CurrentSegment(fighter, definition)?.Definition.Hurtboxes ?? definition.Gameplay.Body.For(fighter.Posture).Hurtboxes;
            foreach (var box in boxes) result.Add(Geometry.ToWorld(box, fighter.X, fighter.Y, fighter.Facing));
            return result;
        }

        /// <summary>Hitboxes actives ; vides si le coup a déjà touché (un seul impact par exécution).</summary>
        public static List<WorldBox> Active(FighterState fighter, CompiledFighter definition)
        {
            var result = new List<WorldBox>();
            if (fighter.Action.Kind != ActionKind.Move || fighter.Action.HasHit) return result;
            var segment = CurrentSegment(fighter, definition);
            if (segment == null || segment.Definition.Phase != Phase.Active) return result;
            foreach (var box in segment.Definition.Hitboxes) result.Add(Geometry.ToWorld(box, fighter.X, fighter.Y, fighter.Facing));
            return result;
        }

        /// <summary>Hitboxes du segment actif, même déjà consommées (affichage de débogage).</summary>
        public static List<WorldBox> Declared(FighterState fighter, CompiledFighter definition)
        {
            var result = new List<WorldBox>();
            var segment = CurrentSegment(fighter, definition);
            if (segment == null || segment.Definition.Phase != Phase.Active) return result;
            foreach (var box in segment.Definition.Hitboxes) result.Add(Geometry.ToWorld(box, fighter.X, fighter.Y, fighter.Facing));
            return result;
        }

        public static bool Projectile(ProjectileState projectile, CompiledFighter owner, out WorldBox box)
        {
            var definition = owner.Projectile(projectile.ProjectileId);
            box = definition == null ? default : Geometry.ToWorld(definition.Hitbox, projectile.X, projectile.Y, projectile.Facing);
            return definition != null;
        }
    }
}
