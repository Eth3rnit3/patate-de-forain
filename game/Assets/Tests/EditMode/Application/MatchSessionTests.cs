using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Le vrai Saint Louis en combat miroir, simulé sans rendu ni clavier.</summary>
    public class MatchSessionTests
    {
        static FighterCatalog Catalog() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));

        static MatchSession MirrorMatch()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            return MatchSession.Start(Catalog(), "saint-louis", "saint-louis", rules, StatPolicy.Free());
        }

        [Test]
        public void UnCombatMiroirSeDerouleJusquAuKOSansRendu()
        {
            var session = MirrorMatch();
            int ko = -1;
            // Le joueur 1 avance et alterne poing et pieds ; le joueur 2 reste immobile.
            var pattern = new[] { Buttons.LightPunch, Buttons.None, Buttons.LightKick, Buttons.None, Buttons.HeavyKick, Buttons.None };
            for (int tick = 0; tick < 60 * 60 && ko < 0; tick++)
            {
                var p1 = tick % 40 < 10 ? Buttons.Right : pattern[tick / 40 % pattern.Length];
                foreach (var e in session.Step(p1, Buttons.None)) if (e.Type == MatchEventType.KO) ko = e.Slot;
            }
            Assert.AreEqual(1, ko, "le joueur 2 doit finir KO");
            Assert.AreEqual(0, session.State.Round.Winner);
        }

        [Test]
        public void LeJournalDEntreesRejoueLaMancheALIdentique()
        {
            var session = MirrorMatch();
            for (int tick = 0; tick < 600; tick++)
                session.Step(tick % 30 < 15 ? Buttons.Right : Buttons.LightKick, tick % 50 == 0 ? Buttons.Special : Buttons.Left);
            var original = StateCodec.Checksum(session.State);

            var replay = MirrorMatch();
            foreach (var (first, second) in session.InputLog) replay.Step(first, second);
            Assert.AreEqual(original, StateCodec.Checksum(replay.State));
        }

        [Test]
        public void RecommencerRemetLaMancheAZero()
        {
            var session = MirrorMatch();
            for (int tick = 0; tick < 120; tick++) session.Step(Buttons.Right, Buttons.Left);
            session.Restart();
            Assert.AreEqual(0, session.State.Tick);
            Assert.AreEqual(RoundPhase.Intro, session.State.Round.Phase);
            Assert.AreEqual(1000, session.State.Fighters[1].Health);
            Assert.IsEmpty(session.InputLog);
        }

        [Test]
        public void UnCombattantInconnuEstRefuse() =>
            Assert.Throws<System.ArgumentException>(() => MatchSession.Start(Catalog(), "saint-louis", "napoleon-inexistant", MatchRules.Default(), StatPolicy.Free()));

        [Test]
        public void LHorlogeAPasFixeEstIndependanteDuRafraichissement()
        {
            var clock = new FixedStepClock(60);
            int ticks = 0;
            for (int frame = 0; frame < 144; frame++) ticks += clock.Consume(1.0 / 144); // écran 144 Hz pendant 1 s
            Assert.That(ticks, Is.InRange(59, 60));
            Assert.AreEqual(4, new FixedStepClock(60).Consume(5.0), "une longue pause ne provoque pas de rattrapage massif");
        }
    }
}
