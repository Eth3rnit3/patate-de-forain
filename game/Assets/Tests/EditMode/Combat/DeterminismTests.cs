using System;
using NUnit.Framework;
using Patate.Combat;

namespace Patate.Tests
{
    /// <summary>La simulation tourne sans rendu, se rejoue à l'identique et se restaure exactement.</summary>
    public class DeterminismTests
    {
        /// <summary>Séquence scriptée qui mêle marche, garde, coups et spéciaux des deux côtés.</summary>
        static (Buttons, Buttons) Busy(int tick)
        {
            var p1 = new[] { Buttons.Right, Buttons.LightPunch, Buttons.None, Buttons.Special, Buttons.Left, Buttons.None };
            var p2 = new[] { Buttons.Left, Buttons.None, Buttons.Right, Buttons.LightPunch, Buttons.None, Buttons.Special };
            return (p1[tick / 7 % p1.Length], p2[tick / 11 % p2.Length]);
        }

        [Test]
        public void MemesEntreesMemeResultat()
        {
            var first = new Bench().Run(900, Busy);
            var second = new Bench().Run(900, Busy);
            Assert.AreEqual(StateCodec.Checksum(first.State), StateCodec.Checksum(second.State));
            Assert.Greater(first.Events.Count, 10);
        }

        [Test]
        public void SauvegarderRestaurerPuisContinuerEquivautANeJamaisSArreter()
        {
            var full = new Bench().Run(800, Busy);

            var half = new Bench().Run(400, Busy);
            half.State = StateCodec.Deserialize(StateCodec.Serialize(half.State));
            half.Run(400, i => Busy(i + 400));

            CollectionAssert.AreEqual(StateCodec.Serialize(full.State), StateCodec.Serialize(half.State));
        }

        [Test]
        public void LAllerRetourBinaireConserveChaqueChamp()
        {
            var bench = new Bench(gap: 700).Run(25, i => (Bench.Tap(i, 0, Buttons.Special), Buttons.Right));
            Assert.IsNotEmpty(bench.State.Projectiles, "le scénario doit contenir un projectile en vol");
            var restored = StateCodec.Deserialize(StateCodec.Serialize(bench.State));
            Assert.AreEqual(StateCodec.Checksum(bench.State), StateCodec.Checksum(restored));
            Assert.AreEqual(bench.State.Projectiles[0].X, restored.Projectiles[0].X);
            Assert.AreEqual(bench.P1.Action.MoveId, restored.Fighters[0].Action.MoveId);
        }

        [Test]
        public void NeModifieJamaisLEtatRecu()
        {
            var bench = new Bench(gap: 200);
            var before = StateCodec.Serialize(bench.State);
            Simulation.Step(bench.State, Buttons.LightPunch, Buttons.Special, bench.Context, new System.Collections.Generic.List<MatchEvent>());
            CollectionAssert.AreEqual(before, StateCodec.Serialize(bench.State));
        }

        [Test]
        public void LesActionsDuJoueur1HorsDePorteeNeChangentRienPourLeJoueur2()
        {
            var quiet = new Bench(gap: 1400).Run(60);
            var busy = new Bench(gap: 1400).Run(60, i => (i % 20 == 0 ? Buttons.LightPunch : Buttons.None, Buttons.None));
            Assert.AreEqual(Fingerprint(quiet.P2), Fingerprint(busy.P2));
            Assert.AreNotEqual(Fingerprint(quiet.P1), Fingerprint(busy.P1));
        }

        [Test]
        public void DeuxJoueursAvecLaMemeDefinitionGardentDesEtatsSepares()
        {
            var bench = new Bench(gap: 1400).Run(5, i => (Bench.Tap(i, 0, Buttons.LightPunch), Buttons.None));
            Assert.AreEqual(ActionKind.Move, bench.P1.Action.Kind);
            Assert.AreEqual(ActionKind.Idle, bench.P2.Action.Kind);
        }

        static string Fingerprint(FighterState f) =>
            $"{f.X}|{f.Facing}|{f.Health}|{f.Action.Kind}|{f.Action.Tick}|{f.Action.MoveId}|{f.Hitstop}|{string.Join(",", Array.ConvertAll(f.InputHistory, b => (int)b))}";
    }

    public class RoundTests
    {
        [Test]
        public void KOQuandLaVieTombeAZeroPuisFinDeManche()
        {
            var bench = new Bench(gap: 200);
            bench.P2.Health = 30;
            bench.Run(bench.Context.Rules.OutroTicks + 40, i => (Bench.Tap(i, 0, Buttons.LightPunch), Buttons.None));
            var ko = bench.Events.Find(e => e.Type == MatchEventType.KO);
            Assert.IsNotNull(ko);
            Assert.AreEqual(1, ko.Slot);
            Assert.AreEqual(RoundPhase.Over, bench.State.Round.Phase);
            Assert.AreEqual(0, bench.State.Round.Winner);
            Assert.AreEqual(ActionKind.KO, bench.P2.Action.Kind);
        }

        [Test]
        public void FinDuTempsLePlusEnVieGagne()
        {
            var bench = new Bench(gap: 1400);
            bench.State.Round.Timer = 3;
            bench.P1.Health = 400;
            bench.Run(5);
            Assert.AreEqual(RoundPhase.TimeOver, bench.State.Round.Phase);
            Assert.AreEqual(1, bench.State.Round.Winner);
        }

        [Test]
        public void PendantLIntroductionLesEntreesSontIgnorees()
        {
            var bench = new Bench(gap: 1400);
            bench.State.Round.Phase = RoundPhase.Intro;
            bench.Run(10, _ => (Buttons.LightPunch, Buttons.None));
            Assert.AreEqual(0, bench.Count(MatchEventType.MoveStart));
        }

        [Test]
        public void ApresLeKOLesEntreesSontIgnorees()
        {
            var bench = new Bench(gap: 200);
            bench.P2.Health = 30;
            bench.Run(30, i => (Bench.Tap(i, 0, Buttons.LightPunch), Buttons.None));
            Assert.AreEqual(RoundPhase.KO, bench.State.Round.Phase);
            int starts = bench.Count(MatchEventType.MoveStart);
            bench.Run(30, i => (i % 2 == 0 ? Buttons.LightPunch : Buttons.None, Buttons.None));
            Assert.AreEqual(starts, bench.Count(MatchEventType.MoveStart));
        }
    }
}
