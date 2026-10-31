using System.Collections.Generic;
using System.Linq;

namespace Patate.Content
{
    /// <summary>
    /// Règles de statistiques d'un mode de jeu. Un paquet valide n'impose pas
    /// pour autant ses statistiques partout : un mode « libre » accepte tout
    /// paquet valide ; un mode « équilibré » (futur jeu en ligne classé) borne
    /// les valeurs et peut n'accepter que des versions approuvées, identifiées
    /// par leur empreinte de gameplay.
    /// </summary>
    public sealed class StatPolicy
    {
        public string Id;
        public int MaxHealth = int.MaxValue;
        public int MaxWalkSpeed = int.MaxValue;
        public int MaxDamagePerHit = int.MaxValue;
        public int MaxProjectileSpeed = int.MaxValue;
        /// <summary>Hauteur maximale du saut (unités) et vitesse horizontale maximale en l'air.</summary>
        public int MaxJumpHeight = int.MaxValue;
        public int MaxJumpSpeed = int.MaxValue;
        /// <summary>Durée minimale de préparation d'un coup qui touche, en ticks.</summary>
        public int MinStartup;
        /// <summary>Empreintes de gameplay approuvées ; null = toute version conforme aux bornes.</summary>
        public HashSet<string> ApprovedGameplayHashes;

        public static StatPolicy Free() => new StatPolicy { Id = "libre" };

        /// <summary>Bornes indicatives, à ajuster quand l'équilibrage commencera.</summary>
        public static StatPolicy Balanced() => new StatPolicy
        {
            Id = "equilibre",
            MaxHealth = 1200,
            MaxWalkSpeed = 10,
            MaxDamagePerHit = 150,
            MaxProjectileSpeed = 16,
            MaxJumpHeight = 480,
            MaxJumpSpeed = 12,
            MinStartup = 3,
        };

        /// <summary>Raisons pour lesquelles le combattant n'est pas admis dans ce mode (vide = admis).</summary>
        public List<string> Check(FighterPackage package)
        {
            var problems = new List<string>();
            var g = package.Gameplay;
            if (ApprovedGameplayHashes != null && !ApprovedGameplayHashes.Contains(package.GameplayRef.Hash))
                problems.Add($"version {package.GameplayRef} non approuvée pour le mode « {Id} »");
            if (g.Stats.Health > MaxHealth) problems.Add($"vie {g.Stats.Health} > {MaxHealth}");
            if (System.Math.Max(g.Stats.WalkForwardSpeed, g.Stats.WalkBackSpeed) > MaxWalkSpeed) problems.Add($"vitesse de marche > {MaxWalkSpeed}");
            var jump = g.Stats.Jump;
            int height = jump.Velocity * (jump.Velocity / jump.Gravity + 1) / 2;
            if (height > MaxJumpHeight) problems.Add($"hauteur de saut {height} > {MaxJumpHeight}");
            if (System.Math.Max(jump.ForwardSpeed, jump.BackSpeed) > MaxJumpSpeed) problems.Add($"vitesse en l'air > {MaxJumpSpeed}");
            foreach (var move in g.Moves)
            {
                if (move.Hit != null && move.Hit.Damage > MaxDamagePerHit) problems.Add($"{move.Id} : dégâts {move.Hit.Damage} > {MaxDamagePerHit}");
                int startup = move.Segments.TakeWhile(s => s.Phase != Combat.Phase.Active).Sum(s => s.Duration);
                if (move.Hit != null && startup < MinStartup) problems.Add($"{move.Id} : préparation {startup} < {MinStartup} ticks");
            }
            foreach (var projectile in g.Projectiles)
            {
                if (projectile.Speed > MaxProjectileSpeed) problems.Add($"{projectile.Id} : vitesse {projectile.Speed} > {MaxProjectileSpeed}");
                if (projectile.Hit != null && projectile.Hit.Damage > MaxDamagePerHit) problems.Add($"{projectile.Id} : dégâts {projectile.Hit.Damage} > {MaxDamagePerHit}");
            }
            return problems;
        }
    }
}
