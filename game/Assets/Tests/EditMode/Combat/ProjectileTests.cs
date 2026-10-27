using System.Collections.Generic;
using NUnit.Framework;
using Patate.Combat;

namespace Patate.Tests
{
    public class ProjectileTests
    {
        static Buttons Special(int i) => Bench.Tap(i, 0, Buttons.Special);

        [Test]
        public void LeSpecialCreeExactementUnProjectileALEntreeDuSegmentPrevu()
        {
            var bench = new Bench(gap: 1200);
            int startX = bench.P1.X;
            int spawnIndex = -1;
            bench.Run(40, i => (Special(i), Buttons.None), (_, events, index) =>
            {
                if (events.Exists(e => e.Type == MatchEventType.ProjectileSpawn)) spawnIndex = index;
            });
            Assert.AreEqual(1, bench.Count(MatchEventType.ProjectileSpawn));
            Assert.AreEqual(SyntheticFighter.OrbSpawnTick, spawnIndex);
            var spawn = bench.Events.Find(e => e.Type == MatchEventType.ProjectileSpawn);
            Assert.AreEqual(0, spawn.Slot);
            Assert.AreEqual(startX + 100, spawn.X);
            Assert.AreEqual(300, spawn.Y);
        }

        [Test]
        public void UnGelDImpactAuTickDEmissionNeCreePasDeSecondProjectile()
        {
            var bench = new Bench(gap: 1200);
            bench.Run(60, i => (Special(i), Buttons.None), (s, events, _) =>
            {
                // Gel forcé juste après l'émission, comme si le lanceur échangeait un coup.
                if (events.Exists(e => e.Type == MatchEventType.ProjectileSpawn)) s.Fighters[0].Hitstop = 8;
            });
            Assert.AreEqual(1, bench.Count(MatchEventType.ProjectileSpawn));
        }

        [Test]
        public void UnSeulProjectileALaFoisParLanceur()
        {
            var bench = new Bench(gap: 1500).Run(60, i => (i == 0 || i == 35 ? Buttons.Special : Buttons.None, Buttons.None));
            Assert.AreEqual(1, bench.Count(MatchEventType.ProjectileSpawn));
            Assert.AreEqual(1, bench.Count(MatchEventType.MoveStart));
        }

        [Test]
        public void EnMiroirChacunLanceLeSien()
        {
            var bench = new Bench(gap: 1500).Run(15, i => (Special(i), Special(i)));
            var owners = bench.Events.FindAll(e => e.Type == MatchEventType.ProjectileSpawn).ConvertAll(e => e.Slot);
            CollectionAssert.AreEqual(new[] { 0, 1 }, owners);
        }

        [Test]
        public void ToucheLAdversairePuisDisparait()
        {
            var bench = new Bench(gap: 700).Run(120, i => (Special(i), Buttons.None));
            Assert.AreEqual(1, bench.Count(MatchEventType.Hit));
            Assert.AreEqual(1000 - SyntheticFighter.OrbHit.Damage, bench.P2.Health);
            Assert.IsEmpty(bench.State.Projectiles);
            Assert.AreEqual(ProjectileEndReason.Hit, bench.Events.Find(e => e.Type == MatchEventType.ProjectileEnd).Reason);
        }

        [Test]
        public void DeuxProjectilesAdversesSAnnulent()
        {
            var bench = new Bench(gap: 1200).Run(120, i => (Special(i), Special(i)));
            var reasons = bench.Events.FindAll(e => e.Type == MatchEventType.ProjectileEnd).ConvertAll(e => e.Reason);
            CollectionAssert.AreEqual(new[] { ProjectileEndReason.Clash, ProjectileEndReason.Clash }, reasons);
            Assert.AreEqual(0, bench.Count(MatchEventType.Hit));
        }

        [Test]
        public void DisparaitALaFinDeSaDureeDeVie()
        {
            var bench = new Bench(gap: 1200, gameplay: SyntheticFighter.Gameplay(orbSpeed: 1, orbLifetime: 30));
            bench.Run(80, i => (Special(i), Buttons.None));
            Assert.AreEqual(ProjectileEndReason.Expired, bench.Events.Find(e => e.Type == MatchEventType.ProjectileEnd).Reason);
        }

        [Test]
        public void SeBloqueEnTenantArriere()
        {
            var bench = new Bench(gap: 700).Run(120, i => (Special(i), Buttons.Right));
            Assert.AreEqual(1, bench.Count(MatchEventType.Block));
            Assert.AreEqual(1000, bench.P2.Health);
        }
    }
}
