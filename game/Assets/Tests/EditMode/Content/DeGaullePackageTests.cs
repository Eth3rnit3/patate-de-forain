using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Le paquet de De Gaulle, troisième combattant : validité, synchronisation des poses, combat contre Napoléon.</summary>
    public class DeGaullePackageTests
    {
        static FighterCatalog Catalog() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));
        static FighterPackage DeGaulle() => Catalog().Get("de-gaulle");

        [Test]
        public void LePaquetDeDeGaulleEstValide()
        {
            var catalog = Catalog();
            Assert.IsFalse(catalog.Rejected.ContainsKey("de-gaulle"), catalog.Rejected.TryGetValue("de-gaulle", out var report) ? report.ToString() : "");
            var deGaulle = catalog.Get("de-gaulle");
            Assert.IsNotNull(deGaulle);
            Assert.AreEqual("De Gaulle", deGaulle.Manifest.Name);
            Assert.AreEqual("0.1.0", deGaulle.GameplayRef.Version);
            Assert.AreNotEqual(catalog.Get("napoleon").GameplayRef.Hash, deGaulle.GameplayRef.Hash);
            CollectionAssert.AreEquivalent(new[] { "special_appel", "stand_punch_light", "stand_kick_light", "stand_kick_heavy", "crouch_punch", "crouch_kick", "jump_punch", "jump_kick" }, deGaulle.Gameplay.Moves.Select(m => m.Id));
            CollectionAssert.AreEquivalent(new[] { "appel" }, deGaulle.Gameplay.Projectiles.Select(p => p.Id));
        }

        [Test]
        public void LaPoseAffichePendantLaPhaseActiveEstLaPoseDeFrappe()
        {
            // Poses de frappe (moveset-prototype-v01.md) : poing 3 puis 4, pied rapide 4, pied puissant 5.
            var package = DeGaulle();
            var fighter = package.Compile();
            foreach (var (moveId, strikePoses) in new[] { ("stand_punch_light", new[] { 3, 4 }), ("stand_kick_light", new[] { 4 }), ("stand_kick_heavy", new[] { 5 }) })
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

        [Test]
        public void LAppelPartALEntreeDeLaCinquiemePose()
        {
            var package = DeGaulle();
            var move = package.Compile().Move("special_appel");
            var spawnSegment = move.Segments.Single(s => s.Definition.OnEnter.Count > 0);
            var animation = package.Presentation.Animations["special_appel_cast"];
            Assert.AreEqual(4, FramePicker.IndexAt(animation, spawnSegment.Start));
            Assert.AreEqual(3, FramePicker.IndexAt(animation, spawnSegment.Start - 1));
        }

        static MatchSession AgainstNapoleon()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            return MatchSession.Start(Catalog(), "de-gaulle", "napoleon", rules, StatPolicy.Free());
        }

        [Test]
        public void LAppelToucheNapoleonDebout()
        {
            var session = AgainstNapoleon();
            int spawns = 0;
            MatchEvent hit = null;
            for (int tick = 0; tick < 240 && hit == null; tick++)
            {
                foreach (var e in session.Step(tick >= 2 && tick < 5 ? Buttons.Special : Buttons.None, Buttons.None))
                {
                    if (e.Type == MatchEventType.ProjectileSpawn && e.Slot == 0) spawns++;
                    if (e.Type == MatchEventType.Hit && e.ProjectileId == "appel") hit = e;
                }
            }
            Assert.AreEqual(1, spawns);
            Assert.IsNotNull(hit, "l'onde doit toucher Napoléon immobile");
            Assert.AreEqual(1, hit.Defender);
            Assert.AreEqual(1000 - 85, session.State.Fighters[1].Health);
        }

        [Test]
        public void DeGaulleBatNapoleonImmobileJusquAuKO()
        {
            var session = AgainstNapoleon();
            int ko = -1;
            var hits = new System.Collections.Generic.HashSet<string>();
            // De Gaulle avance et alterne poing et pieds ; Napoléon reste immobile.
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
