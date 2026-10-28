using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Le paquet de Napoléon, deuxième combattant : validité, synchronisation des poses, combat contre Saint Louis.</summary>
    public class NapoleonPackageTests
    {
        static FighterCatalog Catalog() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));
        static FighterPackage Napoleon() => Catalog().Get("napoleon");

        [Test]
        public void LePaquetDeNapoleonEstValide()
        {
            var catalog = Catalog();
            Assert.IsFalse(catalog.Rejected.ContainsKey("napoleon"), catalog.Rejected.TryGetValue("napoleon", out var report) ? report.ToString() : "");
            var napoleon = catalog.Get("napoleon");
            Assert.IsNotNull(napoleon);
            Assert.AreEqual("Napoléon", napoleon.Manifest.Name);
            Assert.AreEqual("0.1.0", napoleon.GameplayRef.Version);
            Assert.AreEqual(64, napoleon.GameplayRef.Hash.Length);
            Assert.AreNotEqual(catalog.Get("saint-louis").GameplayRef.Hash, napoleon.GameplayRef.Hash);
            CollectionAssert.AreEquivalent(new[] { "special_imperial_volley", "stand_punch_light", "stand_kick_light", "stand_kick_heavy" }, napoleon.Gameplay.Moves.Select(m => m.Id));
            CollectionAssert.AreEquivalent(new[] { "imperial_volley" }, napoleon.Gameplay.Projectiles.Select(p => p.Id));
        }

        [Test]
        public void LaPoseAffichePendantLaPhaseActiveEstLaPoseDeFrappe()
        {
            // Poses de frappe (moveset-prototype-v01.md) : poing 3 puis 4 (bras déjà tendu en 3), pied rapide 4, pied puissant 5.
            var package = Napoleon();
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
        public void LaSalveApparaitALEntreeDeLaCinquiemePose()
        {
            var package = Napoleon();
            var move = package.Compile().Move("special_imperial_volley");
            var spawnSegment = move.Segments.Single(s => s.Definition.OnEnter.Count > 0);
            var animation = package.Presentation.Animations["special_imperial_volley_cast"];
            Assert.AreEqual(4, FramePicker.IndexAt(animation, spawnSegment.Start));
            Assert.AreEqual(3, FramePicker.IndexAt(animation, spawnSegment.Start - 1));
        }

        static MatchSession AgainstSaintLouis()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            return MatchSession.Start(Catalog(), "napoleon", "saint-louis", rules, StatPolicy.Free());
        }

        [Test]
        public void LaSalveImperialeToucheSaintLouisAHauteurDePoitrine()
        {
            var session = AgainstSaintLouis();
            int spawns = 0;
            MatchEvent hit = null;
            for (int tick = 0; tick < 240 && hit == null; tick++)
            {
                foreach (var e in session.Step(tick >= 2 && tick < 5 ? Buttons.Special : Buttons.None, Buttons.None))
                {
                    if (e.Type == MatchEventType.ProjectileSpawn && e.Slot == 0) spawns++;
                    if (e.Type == MatchEventType.Hit && e.ProjectileId == "imperial_volley") hit = e;
                }
            }
            Assert.AreEqual(1, spawns);
            Assert.IsNotNull(hit, "le boulet doit toucher Saint Louis immobile");
            Assert.AreEqual(1, hit.Defender);
            Assert.AreEqual(1000 - 90, session.State.Fighters[1].Health);
        }

        [Test]
        public void NapoleonBatSaintLouisImmobileJusquAuKO()
        {
            var session = AgainstSaintLouis();
            int ko = -1;
            var hits = new System.Collections.Generic.HashSet<string>();
            // Napoléon avance et alterne poing et pieds ; Saint Louis reste immobile.
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
            Assert.AreEqual(1, ko, "Saint Louis doit finir KO");
            CollectionAssert.AreEquivalent(new[] { "stand_punch_light", "stand_kick_light", "stand_kick_heavy" }, hits, "chaque coup normal doit porter");
        }
    }
}
