using System.Collections.Generic;
using NUnit.Framework;
using Patate.Combat;

namespace Patate.Tests
{
    public class MoveTimelineTests
    {
        [Test]
        public void SuitExactementPreparationActiviteRecuperationPuisRevientAuNeutre()
        {
            // Adversaire hors de portée : on observe la chronologie sans contact.
            var bench = new Bench(gap: 800);
            var phases = new List<string>();
            var hitboxCounts = new List<int>();
            bench.Run(SyntheticFighter.JabDuration + 2, i => (Bench.Tap(i, 0, Buttons.LightPunch), Buttons.None), (s, _, __) =>
            {
                var fighter = s.Fighters[0];
                var segment = Hitboxes.CurrentSegment(fighter, bench.Context.Fighters[0]);
                phases.Add(segment != null ? segment.Definition.Phase.ToString() : fighter.Action.Kind.ToString());
                hitboxCounts.Add(Hitboxes.Active(fighter, bench.Context.Fighters[0]).Count);
            });

            CollectionAssert.AreEqual(new[]
            {
                "Startup", "Startup", "Startup", "Startup",
                "Active", "Active",
                "Recovery", "Recovery", "Recovery", "Recovery", "Recovery", "Recovery",
                "Idle", "Idle",
            }, phases);
            Assert.AreEqual(SyntheticFighter.JabFirstActive, hitboxCounts.IndexOf(1));
            Assert.AreEqual(2, hitboxCounts.FindAll(c => c > 0).Count);
        }

        [Test]
        public void IgnoreUnNouvelAppuiPendantLeCoup()
        {
            var bench = new Bench(gap: 800).Run(8, i => (i == 0 || i == 5 ? Buttons.LightPunch : Buttons.None, Buttons.None));
            Assert.AreEqual(1, bench.Count(MatchEventType.MoveStart));
        }

        [Test]
        public void DeclencheSurLAppuiPasSurLeMaintien()
        {
            var bench = new Bench(gap: 800).Run(40, _ => (Buttons.LightPunch, Buttons.None));
            Assert.AreEqual(1, bench.Count(MatchEventType.MoveStart));
        }

        [Test]
        public void ReconnaitLeQuartDeCercleAvantSelonLOrientation()
        {
            // Le joueur 2 regarde à gauche : son « avant » est la gauche.
            var sequence = new[] { Buttons.Down, Buttons.Down | Buttons.Left, Buttons.Left, Buttons.Left | Buttons.LightPunch };
            var bench = new Bench(gap: 800).Run(4, i => (Buttons.None, sequence[i]));
            var start = bench.Events.Find(e => e.Type == MatchEventType.MoveStart);
            Assert.IsNotNull(start);
            Assert.AreEqual(1, start.Slot);
            Assert.AreEqual("orb_throw", start.MoveId);
        }

        [Test]
        public void UnQuartDeCercleVersLArriereNeDeclenchePasLeSpecial()
        {
            var sequence = new[] { Buttons.Down, Buttons.Down | Buttons.Right, Buttons.Right, Buttons.Right | Buttons.LightPunch };
            var bench = new Bench(gap: 800).Run(4, i => (Buttons.None, sequence[i]));
            Assert.AreEqual("jab", bench.Events.Find(e => e.Type == MatchEventType.MoveStart)?.MoveId);
        }
    }
}
