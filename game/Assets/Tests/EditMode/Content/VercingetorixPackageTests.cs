using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Le paquet de Vercingétorix, quatrième combattant : validité, synchronisation des poses, charge et combat contre Napoléon.</summary>
    public class VercingetorixPackageTests
    {
        static FighterCatalog Catalog() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));
        static FighterPackage Vercingetorix() => Catalog().Get("vercingetorix");

        [Test]
        public void LePaquetDeVercingetorixEstValide()
        {
            var catalog = Catalog();
            Assert.IsFalse(catalog.Rejected.ContainsKey("vercingetorix"), catalog.Rejected.TryGetValue("vercingetorix", out var report) ? report.ToString() : "");
            var fighter = catalog.Get("vercingetorix");
            Assert.IsNotNull(fighter);
            Assert.AreEqual("Vercingétorix", fighter.Manifest.Name);
            Assert.AreEqual("0.1.0", fighter.GameplayRef.Version);
            CollectionAssert.AreEquivalent(new[] { "special_alesia", "stand_punch_light", "stand_kick_light", "stand_kick_heavy", "crouch_punch", "crouch_kick", "jump_punch", "jump_kick" }, fighter.Gameplay.Moves.Select(m => m.Id));
            CollectionAssert.IsEmpty(fighter.Gameplay.Projectiles);
        }

        [Test]
        public void LaPoseAffichePendantLaPhaseActiveEstLaPoseDeFrappe()
        {
            // Poses de frappe (moveset-prototype-v01.md) : poing 4, pied rapide 4, pied puissant 5, charge 4 à 6.
            var package = Vercingetorix();
            var fighter = package.Compile();
            foreach (var (moveId, strikePoses) in new[] { ("stand_punch_light", new[] { 4 }), ("stand_kick_light", new[] { 4 }), ("stand_kick_heavy", new[] { 5 }), ("special_alesia", new[] { 4, 5, 6 }) })
            {
                var move = fighter.Move(moveId);
                var active = move.Segments.Where(s => s.Definition.Phase == Phase.Active).ToList();
                Assert.AreEqual(strikePoses.Length, active.Count, moveId);
                for (int i = 0; i < active.Count; i++)
                {
                    for (int tick = active[i].Start; tick < active[i].End; tick++)
                    {
                        var state = new FighterState { Action = new ActionState { Kind = ActionKind.Move, MoveId = moveId, Tick = tick } };
                        var pick = FramePicker.ForFighter(state, package.Presentation);
                        Assert.AreEqual(strikePoses[i] - 1, pick.FrameIndex, $"{moveId} tick {tick}");
                    }
                }
            }
        }

        static MatchSession AgainstNapoleon()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            return MatchSession.Start(Catalog(), "vercingetorix", "napoleon", rules, StatPolicy.Free());
        }

        [Test]
        public void LaChargeTraverseLaDistanceEtToucheNapoleon()
        {
            var session = AgainstNapoleon();
            int startX = session.State.Fighters[0].X;
            MatchEvent hit = null;
            for (int tick = 0; tick < 120 && hit == null; tick++)
            {
                foreach (var e in session.Step(tick >= 2 && tick < 5 ? Buttons.Special : Buttons.None, Buttons.None))
                    if (e.Type == MatchEventType.Hit && e.Slot == 0) hit = e;
            }
            Assert.IsNotNull(hit, "la charge doit atteindre Napoléon depuis la distance de départ");
            Assert.AreEqual("special_alesia", hit.MoveId);
            Assert.Greater(session.State.Fighters[0].X - startX, 100, "Vercingétorix doit avancer pendant la charge");
            Assert.AreEqual(1000 - 120, session.State.Fighters[1].Health);
        }

        [Test]
        public void VercingetorixBatNapoleonImmobileJusquAuKO()
        {
            var session = AgainstNapoleon();
            int ko = -1;
            var hits = new System.Collections.Generic.HashSet<string>();
            // Vercingétorix avance et alterne poing et pieds ; Napoléon reste immobile.
            var pattern = new[] { Buttons.LightPunch, Buttons.None, Buttons.LightKick, Buttons.None, Buttons.HeavyKick, Buttons.None };
            for (int tick = 0; tick < 60 * 60 && ko < 0; tick++)
            {
                var p1 = tick % 40 < 10 ? Buttons.Right : pattern[tick / 40 % pattern.Length];
                foreach (var e in session.Step(p1, Buttons.None))
                {
                    if (e.Type == MatchEventType.Hit && e.Slot == 0 && e.MoveId != null) hits.Add(e.MoveId);
                    if (e.Type == MatchEventType.KO) ko = e.Slot;
                }
            }
            Assert.AreEqual(1, ko, "Napoléon doit finir KO");
            CollectionAssert.AreEquivalent(new[] { "stand_punch_light", "stand_kick_light", "stand_kick_heavy" }, hits, "chaque coup normal doit porter");
        }
    }
}
