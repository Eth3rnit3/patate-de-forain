using System;
using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>L'adversaire de l'ordinateur, contre des joueurs scriptés, sans rendu.</summary>
    public class CpuPlayerTests
    {
        const int Easy = 0, Normal = 1, Hard = 2;
        const int RoundTicks = 99 * 60;

        static MatchSession Match()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            var catalog = FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));
            return MatchSession.Start(catalog, "saint-louis", "saint-louis", rules, StatPolicy.Free());
        }

        /// <summary>Le joueur 1 suit le script, l'IA tient la place 2, jusqu'à la fin de la manche.</summary>
        static (MatchSession session, int blocks, int hitsTaken) Play(int level, uint seed, Func<MatchSession, Buttons> opponent)
        {
            MatchSession session = Match();
            var cpu = new CpuPlayer(level, 1, () => session, seed);
            var human = new ScriptedInput(_ => opponent(session));
            int blocks = 0, hitsTaken = 0;
            for (int tick = 0; tick < RoundTicks + 300 && !session.IsOver; tick++)
                foreach (var e in session.Step(human, cpu))
                {
                    if (e.Type == MatchEventType.Block && e.Defender == 1) blocks++;
                    if (e.Type == MatchEventType.Hit && e.Defender == 1) hitsTaken++;
                }
            return (session, blocks, hitsTaken);
        }

        /// <summary>Spécial de loin, avance, puis pied puissant dès que possible au contact.</summary>
        static Buttons Attacker(MatchSession session)
        {
            var state = session.State;
            int gap = state.Fighters[1].X - state.Fighters[0].X;
            bool press = state.Tick % 2 == 0;
            if (Math.Abs(gap) > 700) return press ? Buttons.Special : Buttons.None;
            if (Math.Abs(gap) > 380) return gap > 0 ? Buttons.Right : Buttons.Left;
            return press ? Buttons.HeavyKick : Buttons.None;
        }

        /// <summary>Reste sur place et lance son projectile dès qu'il le peut.</summary>
        static Buttons Zoner(MatchSession session) => session.State.Tick % 2 == 0 ? Buttons.Special : Buttons.None;

        [TestCase(Easy)]
        [TestCase(Normal)]
        [TestCase(Hard)]
        public void GagneContreUnAdversaireImmobile(int level)
        {
            var (session, _, _) = Play(level, 7, _ => Buttons.None);
            Assert.AreEqual(1, session.State.Round.Winner, "l'IA doit gagner");
            Assert.AreEqual(0, session.State.Fighters[0].Health, "par KO, pas au temps");
        }

        [Test]
        public void GagneAvecUnCombattantQuElleNeConnaitPas()
        {
            // Le mannequin de synthèse n'a qu'un poing et un projectile : l'IA les trouve dans son gameplay.
            var bench = new Bench(gap: 600);
            var cpu = new CpuPlayer(Normal, 1, seed: 3);
            bench.Run(RoundTicks, _ => (Buttons.None, cpu.Think(bench.State, bench.Context)));
            Assert.AreEqual(0, bench.P1.Health);
            Assert.AreEqual(1, bench.State.Round.Winner);
        }

        [TestCase(Normal)]
        [TestCase(Hard)]
        public void GardeParfois(int level)
        {
            var (_, blocks, hitsTaken) = Play(level, 11, Zoner);
            Assert.Greater(blocks, 0, "l'IA doit garder au moins une attaque");
            Assert.Greater(hitsTaken, 0, "mais pas toutes");
        }

        [Test]
        public void LeNiveauChangeLeComportement()
        {
            var easy = Play(Easy, 5, Attacker);
            var hard = Play(Hard, 5, Attacker);
            Assert.AreNotEqual(easy.session.InputLog.Select(i => i.Item2), hard.session.InputLog.Select(i => i.Item2));
            Assert.Less(hard.session.State.Fighters[0].Health, easy.session.State.Fighters[0].Health, "le niveau difficile frappe davantage");
            Assert.Greater(Play(Hard, 5, Zoner).blocks, Play(Easy, 5, Zoner).blocks, "et garde davantage");
        }

        [Test]
        public void MemeGraineMemePartie()
        {
            var first = Play(Normal, 42, Attacker).session;
            var second = Play(Normal, 42, Attacker).session;
            Assert.AreEqual(StateCodec.Checksum(first.State), StateCodec.Checksum(second.State));
            var other = Play(Normal, 43, Attacker).session;
            Assert.AreNotEqual(first.InputLog.Select(i => i.Item2), other.InputLog.Select(i => i.Item2), "une autre graine joue autrement");
        }

        [Test]
        public void RecommencerRejoueLaMemePartie()
        {
            MatchSession session = Match();
            var cpu = new CpuPlayer(Normal, 1, () => session, 9);
            var human = new ScriptedInput(_ => Attacker(session));
            for (int tick = 0; tick < 900; tick++) session.Step(human, cpu);
            var checksum = StateCodec.Checksum(session.State);
            session.Restart();
            for (int tick = 0; tick < 900; tick++) session.Step(human, cpu);
            Assert.AreEqual(checksum, StateCodec.Checksum(session.State), "la mémoire et le hasard repartent de zéro");
        }

        [Test]
        public void SansSessionLIANAppuieSurRien() =>
            Assert.AreEqual(Buttons.None, new CpuPlayer(Normal, 1, () => null).InputFor(1));

        [Test]
        public void TroisNiveauxNommes()
        {
            Assert.AreEqual(3, CpuPlayer.LevelCount);
            Assert.AreEqual(new[] { "Facile", "Normal", "Difficile" }, Enumerable.Range(0, 3).Select(CpuPlayer.LevelName));
        }
    }
}
