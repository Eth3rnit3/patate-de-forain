using System.Collections.Generic;
using NUnit.Framework;
using Patate.Combat;

namespace Patate.Tests
{
    public class DamageAndBlockTests
    {
        static (Buttons, Buttons) JabThen(int i, Buttons p2) => (Bench.Tap(i, 0, Buttons.LightPunch), p2);

        [Test]
        public void UnCoupToucheUneSeuleFoisMemeSiSaPhaseActiveDurePlusieursTicks()
        {
            var bench = new Bench(gap: 200).Run(40, i => JabThen(i, Buttons.None));
            Assert.AreEqual(1, bench.Count(MatchEventType.Hit));
            Assert.AreEqual(1000 - SyntheticFighter.JabHit.Damage, bench.P2.Health);
        }

        [Test]
        public void AppliqueEtourdissementEtGelPuisRendLaMain()
        {
            var bench = new Bench(gap: 200);
            int hitIndex = -1;
            var kinds = new List<ActionKind>();
            bench.Run(60, i => JabThen(i, Buttons.None), (s, events, index) =>
            {
                if (events.Exists(e => e.Type == MatchEventType.Hit)) hitIndex = index;
                kinds.Add(s.Fighters[1].Action.Kind);
            });
            Assert.Greater(hitIndex, 0);
            // Gel + étourdissement : aucune action possible pendant hitstop + hitstun ticks.
            int stunned = kinds.GetRange(hitIndex, kinds.Count - hitIndex).FindAll(k => k == ActionKind.Hitstun).Count;
            Assert.AreEqual(SyntheticFighter.JabHit.Hitstop + SyntheticFighter.JabHit.Hitstun, stunned);
            Assert.AreEqual(ActionKind.Idle, kinds[kinds.Count - 1]);
        }

        [Test]
        public void RepousseLaCibleDuReculPrevu()
        {
            var bench = new Bench(gap: 200);
            int before = bench.P2.X;
            bench.Run(60, i => JabThen(i, Buttons.None));
            Assert.AreEqual(SyntheticFighter.JabHit.PushbackHit, bench.P2.X - before);
        }

        [Test]
        public void UnCoupHorsDePorteeNeToucheRien()
        {
            var bench = new Bench(gap: 600).Run(30, i => JabThen(i, Buttons.None));
            Assert.AreEqual(0, bench.Count(MatchEventType.Hit) + bench.Count(MatchEventType.Block));
        }

        [Test]
        public void TenirArriereBloqueSansDegatsAvecReculDeGarde()
        {
            var bench = new Bench(gap: 200);
            int before = bench.P2.X;
            // Le joueur 2 est à droite : arrière = droite.
            bench.Run(40, i => JabThen(i, Buttons.Right));
            Assert.AreEqual(1, bench.Count(MatchEventType.Block));
            Assert.AreEqual(0, bench.Count(MatchEventType.Hit));
            Assert.AreEqual(1000, bench.P2.Health);
            Assert.GreaterOrEqual(bench.P2.X, before + SyntheticFighter.JabHit.PushbackBlock);
        }

        [Test]
        public void SeMetEnGardeSansReculerFaceAUneAttaqueProche()
        {
            var bench = new Bench(gap: 200);
            var kinds = new List<ActionKind>();
            bench.Run(3, i => JabThen(i, Buttons.Right), (s, _, __) => kinds.Add(s.Fighters[1].Action.Kind));
            Assert.AreEqual(ActionKind.Guard, kinds[0]);
        }

        [Test]
        public void TenirAvantNeBloquePas()
        {
            var bench = new Bench(gap: 200).Run(40, i => JabThen(i, Buttons.Left));
            Assert.AreEqual(1, bench.Count(MatchEventType.Hit));
        }

        [Test]
        public void UnCombattantEnPleineAttaqueNePeutPasBloquer()
        {
            var bench = new Bench(gap: 200).Run(20, i => (i == 1 ? Buttons.LightPunch : Buttons.None, i == 0 ? Buttons.Special : Buttons.Right));
            var first = bench.Events.Find(e => e.Type == MatchEventType.Hit || e.Type == MatchEventType.Block);
            Assert.AreEqual(MatchEventType.Hit, first.Type);
            Assert.AreEqual(1, first.Defender);
        }

        [Test]
        public void UneAttaqueNonBlocableTraverseLaGarde()
        {
            var bench = new Bench(gap: 200, gameplay: SyntheticFighter.Gameplay(jabHit: SyntheticFighter.JabHit.With(blockable: false)));
            bench.Run(40, i => JabThen(i, Buttons.Right));
            Assert.AreEqual(1, bench.Count(MatchEventType.Hit));
        }

        [Test]
        public void DeuxCoupsActifsAuMemeInstantSEchangent()
        {
            var bench = new Bench(gap: 200).Run(40, i => (Bench.Tap(i, 0, Buttons.LightPunch), Bench.Tap(i, 0, Buttons.LightPunch)));
            Assert.AreEqual(2, bench.Count(MatchEventType.Hit));
            Assert.AreEqual(bench.P1.Health, bench.P2.Health);
        }
    }
}
