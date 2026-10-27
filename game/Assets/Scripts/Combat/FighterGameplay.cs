using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>
    /// Données de GAMEPLAY d'un combattant : tout ce qui influence le résultat
    /// d'un combat, et rien d'autre. Images, sons et durées d'affichage vivent
    /// dans la définition de présentation (Patate.Content).
    /// Distances en unités monde entières, durées en ticks (60 par seconde).
    /// </summary>
    public sealed class FighterGameplay
    {
        public readonly FighterStats Stats;
        public readonly BodyDefinition Body;
        /// <summary>Ordre significatif : à priorité égale, le premier coup déclaré l'emporte.</summary>
        public readonly IReadOnlyList<MoveDefinition> Moves;
        public readonly IReadOnlyList<ProjectileDefinition> Projectiles;

        public FighterGameplay(FighterStats stats, BodyDefinition body, IReadOnlyList<MoveDefinition> moves, IReadOnlyList<ProjectileDefinition> projectiles)
        {
            Stats = stats; Body = body; Moves = moves; Projectiles = projectiles;
        }
    }

    public sealed class FighterStats
    {
        public readonly int Health, WalkForwardSpeed, WalkBackSpeed;

        public FighterStats(int health, int walkForwardSpeed, int walkBackSpeed)
        {
            Health = health; WalkForwardSpeed = walkForwardSpeed; WalkBackSpeed = walkBackSpeed;
        }
    }

    public sealed class BodyDefinition
    {
        /// <summary>Collision corporelle : empêche les combattants de se traverser.</summary>
        public readonly Box Pushbox;
        /// <summary>Zones vulnérables au repos, en marche, en garde et en réaction.</summary>
        public readonly IReadOnlyList<Box> Hurtboxes;

        public BodyDefinition(Box pushbox, IReadOnlyList<Box> hurtboxes)
        {
            Pushbox = pushbox; Hurtboxes = hurtboxes;
        }
    }

    /// <summary>
    /// Appui sur un bouton, éventuellement avec une direction tenue (Direction),
    /// ou mouvement directionnel puis bouton (Motion, ex. quart de cercle avant 2-3-6).
    /// </summary>
    public sealed class CommandDefinition
    {
        public readonly Buttons Button;
        /// <summary>Direction pavé numérique exigée, 0 = indifférente.</summary>
        public readonly int Direction;
        /// <summary>Séquence de directions, vide si la commande est un simple appui.</summary>
        public readonly IReadOnlyList<int> Motion;
        /// <summary>Nombre de ticks dans lesquels la séquence doit tenir.</summary>
        public readonly int Window;

        public CommandDefinition(Buttons button, int direction = 0, IReadOnlyList<int> motion = null, int window = 0)
        {
            Button = button; Direction = direction; Motion = motion ?? new int[0]; Window = window;
        }
    }

    public enum Phase { Startup, Active, Recovery }

    public sealed class SegmentDefinition
    {
        public readonly Phase Phase;
        public readonly int Duration;
        /// <summary>Uniquement en phase active. Un seul impact par exécution du coup.</summary>
        public readonly IReadOnlyList<Box> Hitboxes;
        /// <summary>Remplace les hurtboxes du corps pendant ce segment (ex. jambe tendue) ; null = celles du corps.</summary>
        public readonly IReadOnlyList<Box> Hurtboxes;
        /// <summary>Déplacement vers l'avant, en unités par tick.</summary>
        public readonly int Advance;
        /// <summary>Actions déclenchées une seule fois, à l'entrée du segment.</summary>
        public readonly IReadOnlyList<SegmentAction> OnEnter;

        public SegmentDefinition(Phase phase, int duration, IReadOnlyList<Box> hitboxes = null, IReadOnlyList<Box> hurtboxes = null,
            int advance = 0, IReadOnlyList<SegmentAction> onEnter = null)
        {
            Phase = phase;
            Duration = duration;
            Hitboxes = hitboxes ?? new Box[0];
            Hurtboxes = hurtboxes;
            Advance = advance;
            OnEnter = onEnter ?? new SegmentAction[0];
        }
    }

    public enum HitLevel { High, Mid, Low }

    public sealed class HitProperties
    {
        public readonly int Damage;
        /// <summary>Dégâts à travers la garde. 0 pour le prototype.</summary>
        public readonly int ChipDamage;
        /// <summary>Ticks pendant lesquels la cible touchée ne peut pas agir (étourdissement).</summary>
        public readonly int Hitstun;
        /// <summary>Ticks de blocage imposés à la cible qui garde.</summary>
        public readonly int Blockstun;
        /// <summary>Gel des deux combattants (coup au corps) ou de la cible (projectile).</summary>
        public readonly int Hitstop;
        /// <summary>Recul de la cible touchée / qui garde, en unités, réparti sur MatchRules.PushbackTicks.</summary>
        public readonly int PushbackHit, PushbackBlock;
        public readonly HitLevel Level;
        public readonly bool Blockable;

        public HitProperties(int damage, int chipDamage, int hitstun, int blockstun, int hitstop, int pushbackHit, int pushbackBlock,
            HitLevel level, bool blockable)
        {
            Damage = damage; ChipDamage = chipDamage; Hitstun = hitstun; Blockstun = blockstun; Hitstop = hitstop;
            PushbackHit = pushbackHit; PushbackBlock = pushbackBlock; Level = level; Blockable = blockable;
        }

        public HitProperties With(int? damage = null, bool? blockable = null) =>
            new HitProperties(damage ?? Damage, ChipDamage, Hitstun, Blockstun, Hitstop, PushbackHit, PushbackBlock, Level, blockable ?? Blockable);
    }

    public sealed class MoveDefinition
    {
        public readonly string Id;
        /// <summary>Plusieurs commandes possibles (ex. touche directe OU quart de cercle).</summary>
        public readonly IReadOnlyList<CommandDefinition> Commands;
        /// <summary>Plus grand = testé en premier. Les spéciaux passent avant les normaux.</summary>
        public readonly int Priority;
        public readonly IReadOnlyList<SegmentDefinition> Segments;
        /// <summary>Null pour un coup sans contact direct (ex. lancer de projectile).</summary>
        public readonly HitProperties Hit;

        public MoveDefinition(string id, IReadOnlyList<CommandDefinition> commands, int priority, IReadOnlyList<SegmentDefinition> segments, HitProperties hit)
        {
            Id = id; Commands = commands; Priority = priority; Segments = segments; Hit = hit;
        }
    }

    public sealed class ProjectileDefinition
    {
        public readonly string Id;
        public readonly int Speed;
        public readonly int Lifetime;
        /// <summary>Collision réelle, indépendante du halo graphique. Relative au centre du projectile.</summary>
        public readonly Box Hitbox;
        /// <summary>Nombre maximal de projectiles de ce type par lanceur.</summary>
        public readonly int MaxActive;
        public readonly HitProperties Hit;

        public ProjectileDefinition(string id, int speed, int lifetime, Box hitbox, int maxActive, HitProperties hit)
        {
            Id = id; Speed = speed; Lifetime = lifetime; Hitbox = hitbox; MaxActive = maxActive; Hit = hit;
        }
    }
}
