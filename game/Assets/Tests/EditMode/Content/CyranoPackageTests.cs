using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Le paquet de Cyrano, sixième combattant : validité, synchronisation des poses, combat contre Napoléon.</summary>
    public class CyranoPackageTests
    {
        static FighterCatalog Catalog() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));
        static FighterPackage Cyrano() => Catalog().Get("cyrano");

        [Test]
        public void LePaquetDeCyranoEstValide()
        {
            var catalog = Catalog();
            Assert.IsFalse(catalog.Rejected.ContainsKey("cyrano"), catalog.Rejected.TryGetValue("cyrano", out var report) ? report.ToString() : "");
            var cyrano = catalog.Get("cyrano");
            Assert.IsNotNull(cyrano);
            Assert.AreEqual("Cyrano", cyrano.Manifest.Name);
            Assert.AreEqual("0.1.0", cyrano.GameplayRef.Version);
            Assert.AreNotEqual(catalog.Get("napoleon").GameplayRef.Hash, cyrano.GameplayRef.Hash);
            CollectionAssert.AreEquivalent(new[] { "special_panache", "stand_punch_light", "stand_kick_light", "stand_kick_heavy", "crouch_punch", "crouch_kick", "jump_punch", "jump_kick" }, cyrano.Gameplay.Moves.Select(m => m.Id));
            CollectionAssert.AreEquivalent(new[] { "panache" }, cyrano.Gameplay.Projectiles.Select(p => p.Id));
        }

        [Test]
        public void LaPoseAffichePendantLaPhaseActiveEstLaPoseDeFrappe()
        {
            // Poses de frappe (moveset-prototype-v01.md) : poing 3 puis 4, pied rapide 4, pied puissant 5.
            var package = Cyrano();
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
        public void LePanachePartALEntreeDeLaCinquiemePose()
        {
            var package = Cyrano();
            var move = package.Compile().Move("special_panache");
            var spawnSegment = move.Segments.Single(s => s.Definition.OnEnter.Count > 0);
            var animation = package.Presentation.Animations["special_panache_cast"];
            Assert.AreEqual(4, FramePicker.IndexAt(animation, spawnSegment.Start));
            Assert.AreEqual(3, FramePicker.IndexAt(animation, spawnSegment.Start - 1));
        }

        static MatchSession AgainstNapoleon()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            return MatchSession.Start(Catalog(), "cyrano", "napoleon", rules, StatPolicy.Free());
        }

        [Test]
        public void LePanacheToucheNapoleonDebout()
        {
            var session = AgainstNapoleon();
            int spawns = 0;
            MatchEvent hit = null;
            for (int tick = 0; tick < 240 && hit == null; tick++)
            {
                foreach (var e in session.Step(tick >= 2 && tick < 5 ? Buttons.Special : Buttons.None, Buttons.None))
                {
                    if (e.Type == MatchEventType.ProjectileSpawn && e.Slot == 0) spawns++;
                    if (e.Type == MatchEventType.Hit && e.ProjectileId == "panache") hit = e;
                }
            }
            Assert.AreEqual(1, spawns);
            Assert.IsNotNull(hit, "le panache doit toucher Napoléon immobile");
            Assert.AreEqual(1, hit.Defender);
            Assert.AreEqual(1000 - 80, session.State.Fighters[1].Health);
        }

        [Test]
        public void CyranoBatNapoleonImmobileJusquAuKO()
        {
            var session = AgainstNapoleon();
            int ko = -1;
            var hits = new System.Collections.Generic.HashSet<string>();
            // Cyrano avance et alterne poing et pieds ; Napoléon reste immobile.
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
