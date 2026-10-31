using System.Collections.Generic;
using NUnit.Framework;
using Patate.Combat;

namespace Patate.Tests
{
    /// <summary>Saut, accroupissement, gardes haute et basse, coups accroupis et sautés.</summary>
    public class StanceTests
    {
        static readonly JumpStats Jump = JumpStats.Default;
        /// <summary>Ticks entre l'appui sur haut et la réception.</summary>
        static readonly int AirTicks = 2 * Jump.Velocity / Jump.Gravity + 1;

        static Bench Stances(int gap) => new Bench(gap: gap, gameplay: SyntheticFighter.WithStances());

        [Test]
        public void UnSautVerticalMonteRetombeEtRevientAuRepos()
        {
            var bench = Stances(1000);
            int x = bench.P1.X, highest = 0, airborne = 0;
            bench.Run(Jump.PreJumpTicks + AirTicks + Jump.LandingTicks + 2, i => (Bench.Tap(i, 0, Buttons.Up), Buttons.None), (s, _, __) =>
            {
                highest = System.Math.Max(highest, s.Fighters[0].Y);
                if (s.Fighters[0].Posture == Posture.Air) airborne++;
            });
            Assert.AreEqual(Jump.Velocity * (Jump.Velocity / Jump.Gravity + 1) / 2, highest);
            Assert.AreEqual(AirTicks - 1, airborne);
            Assert.AreEqual(0, bench.P1.Y);
            Assert.AreEqual(x, bench.P1.X);
            Assert.AreEqual(ActionKind.Idle, bench.P1.Action.Kind);
        }

        [Test]
        public void LeSensDuSautEstFixeAuDecollageEtLaMarcheNAgitPasEnLAir()
        {
            var bench = Stances(1000);
            int x = bench.P1.X;
            // Saut avant (haut + droite), puis droite tenue tout le saut : seule la vitesse du saut compte.
            bench.Run(Jump.PreJumpTicks + AirTicks, i => (i == 0 ? Buttons.Up | Buttons.Right : Buttons.Right, Buttons.None));
            Assert.AreEqual(Posture.Stand, bench.P1.Posture);
            Assert.AreEqual(x + AirTicks * Jump.ForwardSpeed, bench.P1.X);
        }

        [Test]
        public void BasAvantAccroupitSansMarcher()
        {
            var bench = Stances(1000);
            int x = bench.P1.X;
            bench.Run(20, i => (Buttons.Down | Buttons.Right, Buttons.None));
            Assert.AreEqual(ActionKind.Crouch, bench.P1.Action.Kind);
            Assert.AreEqual(Posture.Crouch, bench.P1.Posture);
            Assert.AreEqual(x, bench.P1.X);
        }

        [Test]
        public void AccroupiLeMemeBoutonDonneLeCoupAccroupi()
        {
            var bench = Stances(200).Run(10, i => (Buttons.Down | Bench.Tap(i, 0, Buttons.LightPunch), Buttons.None));
            Assert.AreEqual("crouch_jab", bench.Events.Find(e => e.Type == MatchEventType.MoveStart).MoveId);
        }

        [Test]
        public void UnCoupHautPasseAuDessusDUnCombattantAccroupi()
        {
            var bench = Stances(200).Run(30, i => (Bench.Tap(i, 0, Buttons.LightPunch), Buttons.Down));
            Assert.AreEqual(0, bench.Count(MatchEventType.Hit) + bench.Count(MatchEventType.Block));
        }

        [Test]
        public void UnCoupBasSeGardeAccroupiPasDebout()
        {
            (Buttons, Buttons) Sweep(int i, Buttons p2) => (Buttons.Down | Bench.Tap(i, 0, Buttons.LightKick), p2);
            var standing = Stances(200).Run(30, i => Sweep(i, Buttons.Right));
            Assert.AreEqual(1, standing.Count(MatchEventType.Hit));
            var crouching = Stances(200).Run(30, i => Sweep(i, Buttons.Right | Buttons.Down));
            Assert.AreEqual(1, crouching.Count(MatchEventType.Block));
            Assert.AreEqual(1000, crouching.P2.Health);
        }

        /// <summary>Saut vertical, coup sauté à la descente (assez bas pour toucher un combattant accroupi) ; le joueur 2 adopte sa garde quand le coup part.</summary>
        static Bench Dive(Buttons guard)
        {
            int diveAt = Jump.PreJumpTicks + 24;
            return Stances(150).Run(70, i => (Bench.Tap(i, 0, Buttons.Up) | Bench.Tap(i, diveAt, Buttons.LightPunch), i >= diveAt ? guard : Buttons.None));
        }

        [Test]
        public void UnCoupSauteSeGardeDeboutPasAccroupi()
        {
            Assert.AreEqual(1, Dive(Buttons.Right).Count(MatchEventType.Block));
            Assert.AreEqual(1, Dive(Buttons.Right | Buttons.Down).Count(MatchEventType.Hit));
        }

        [Test]
        public void UnSeulCoupParSautEtLaReceptionInterromptLeCoup()
        {
            var bench = Stances(1000);
            var kinds = new List<ActionKind>();
            // Coup tardif : la réception tombe avant sa fin. Deuxième appui ignoré.
            int late = Jump.PreJumpTicks + AirTicks - 8;
            bench.Run(Jump.PreJumpTicks + AirTicks + 2, i => (Bench.Tap(i, 0, Buttons.Up) | Bench.Tap(i, late, Buttons.LightPunch) | Bench.Tap(i, late + 3, Buttons.LightPunch), Buttons.None),
                (s, _, __) => kinds.Add(s.Fighters[0].Action.Kind));
            Assert.AreEqual(1, bench.Count(MatchEventType.MoveStart));
            Assert.Contains(ActionKind.Land, kinds);
            Assert.AreEqual(0, bench.P1.Y);
        }

        [Test]
        public void EnLAirOnNePeutPasGarder()
        {
            var fighter = new FighterState { Posture = Posture.Air, Action = ActionState.Of(ActionKind.Jump), InputHistory = new[] { Buttons.Left } };
            Assert.IsFalse(CombatResolution.CanBlock(fighter, SyntheticFighter.JabHit.With(), 1));
        }

        [Test]
        public void UnEtatEnPleinSautSeSauvegardeEtSeRestaureExactement()
        {
            var bench = Stances(400).Run(Jump.PreJumpTicks + 10, i => (Bench.Tap(i, 0, Buttons.Up | Buttons.Right), Buttons.None));
            Assert.AreEqual(Posture.Air, bench.P1.Posture);
            var bytes = StateCodec.Serialize(bench.State);
            CollectionAssert.AreEqual(bytes, StateCodec.Serialize(StateCodec.Deserialize(bytes)));
        }
    }
}
