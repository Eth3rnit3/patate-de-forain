using System.Collections.Generic;
using System.Linq;

namespace Patate.Combat
{
    /// <summary>Référence exacte d'une définition : identifiant, version et empreinte du contenu.</summary>
    public sealed class DefinitionRef
    {
        public readonly string Id, Version, Hash;

        public DefinitionRef(string id, string version, string hash)
        {
            Id = id; Version = version; Hash = hash;
        }

        public override string ToString() => $"{Id}@{Version}#{(Hash.Length > 12 ? Hash.Substring(0, 12) : Hash)}";
    }

    public sealed class CompiledSegment
    {
        public readonly SegmentDefinition Definition;
        /// <summary>Tick (relatif au début du coup) où le segment commence ; End est exclu.</summary>
        public readonly int Start, End;

        public CompiledSegment(SegmentDefinition definition, int start)
        {
            Definition = definition; Start = start; End = start + definition.Duration;
        }
    }

    public sealed class CompiledMove
    {
        public readonly MoveDefinition Definition;
        public readonly IReadOnlyList<CompiledSegment> Segments;
        public readonly int TotalDuration;
        /// <summary>Fin de la dernière phase active (0 si aucune).</summary>
        public readonly int LastActiveEnd;

        public CompiledMove(MoveDefinition definition)
        {
            Definition = definition;
            var segments = new List<CompiledSegment>();
            int start = 0;
            foreach (var segment in definition.Segments)
            {
                segments.Add(new CompiledSegment(segment, start));
                start += segment.Duration;
            }
            Segments = segments;
            TotalDuration = start;
            LastActiveEnd = segments.Where(s => s.Definition.Phase == Phase.Active).Select(s => s.End).DefaultIfEmpty(0).Max();
        }

        public string Id => Definition.Id;

        public CompiledSegment SegmentAt(int tick)
        {
            foreach (var segment in Segments) if (tick >= segment.Start && tick < segment.End) return segment;
            return null;
        }
    }

    /// <summary>
    /// Forme pré-calculée d'une définition, construite une fois au lancement du
    /// combat. Fait partie du contexte, jamais de l'état sérialisé.
    /// </summary>
    public sealed class CompiledFighter
    {
        public readonly DefinitionRef Ref;
        public readonly FighterGameplay Gameplay;
        public readonly IReadOnlyDictionary<string, CompiledMove> Moves;
        /// <summary>Coups triés par priorité décroissante, ordre de déclaration en cas d'égalité.</summary>
        public readonly IReadOnlyList<CompiledMove> MovesByPriority;
        public readonly IReadOnlyDictionary<string, ProjectileDefinition> Projectiles;

        public CompiledFighter(DefinitionRef reference, FighterGameplay gameplay)
        {
            Ref = reference;
            Gameplay = gameplay;
            var moves = gameplay.Moves.Select(move => new CompiledMove(move)).ToList();
            Moves = moves.ToDictionary(move => move.Id);
            // OrderBy est un tri stable : l'ordre de déclaration départage les égalités.
            MovesByPriority = moves.OrderByDescending(move => move.Definition.Priority).ToList();
            Projectiles = gameplay.Projectiles.ToDictionary(projectile => projectile.Id);
        }

        public CompiledMove Move(string id) => id != null && Moves.TryGetValue(id, out var move) ? move : null;

        public ProjectileDefinition Projectile(string id) => id != null && Projectiles.TryGetValue(id, out var p) ? p : null;
    }
}
