using NUnit.Framework;
using Patate.Combat;

namespace Patate.Tests
{
    public class CollisionTests
    {
        [Test]
        public void DeuxCombattantsQuiAvancentNeSeChevauchentJamais()
        {
            var bench = new Bench(gap: 400);
            bench.Run(120, _ => (Buttons.Right, Buttons.Left), (s, _, __) =>
            {
                var a = Hitboxes.Pushbox(s.Fighters[0], bench.Context.Fighters[0]);
                var b = Hitboxes.Pushbox(s.Fighters[1], bench.Context.Fighters[1]);
                Assert.LessOrEqual(a.Right, b.Left);
            });
        }

        [Test]
        public void AvancerContreLAdversaireLePousseMoinsVitequEnMarcheLibre()
        {
            var bench = new Bench(gap: 130);
            int start = bench.P2.X;
            bench.Run(30, _ => (Buttons.Right, Buttons.None));
            int pushed = bench.P2.X - start;
            Assert.Greater(pushed, 0);
            Assert.Less(pushed, 30 * 8);
        }

        [Test]
        public void ResteDansLArene()
        {
            var bench = new Bench(gap: 400).Run(600, _ => (Buttons.Left, Buttons.Right));
            foreach (var fighter in bench.State.Fighters)
            {
                var box = Hitboxes.Pushbox(fighter, bench.Context.Of(fighter));
                Assert.GreaterOrEqual(box.Left, bench.Context.Rules.ArenaLeft);
                Assert.LessOrEqual(box.Right, bench.Context.Rules.ArenaRight);
            }
            Assert.LessOrEqual(System.Math.Abs(bench.P1.X - bench.P2.X), bench.Context.Rules.MaxSeparation);
        }

        [Test]
        public void CoinceAuBordLaCibleNeReculePlusEtLAttaquantAbsorbeLeRecul()
        {
            var bench = new Bench(gap: 200);
            int wall = bench.Context.Rules.ArenaRight;
            bench.P2.X = wall - 60; // pushbox collée au bord droit
            bench.P1.X = bench.P2.X - 200;
            int attackerBefore = bench.P1.X;
            bench.Run(40, i => (Bench.Tap(i, 0, Buttons.LightPunch), Buttons.None));
            Assert.AreEqual(wall - 60, bench.P2.X);
            Assert.Less(bench.P1.X, attackerBefore);
        }

        [Test]
        public void SeTourneVersLAdversaireQuandIlEstLibre()
        {
            var bench = new Bench(gap: 400);
            bench.P1.X = 300;
            bench.P2.X = -300;
            bench.Run(1);
            Assert.AreEqual(-1, bench.P1.Facing);
            Assert.AreEqual(1, bench.P2.Facing);
        }

        [Test]
        public void NeSeRetournePasEnPleineAttaque()
        {
            var bench = new Bench(gap: 800).Run(1, _ => (Buttons.LightPunch, Buttons.None));
            bench.P2.X = bench.P1.X - 600;
            bench.Run(3);
            Assert.AreEqual(1, bench.P1.Facing);
        }
    }
}
