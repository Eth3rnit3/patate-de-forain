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
            CollectionAssert.AreEquivalent(new[] { "special_alesia", "special_bouclier", "stand_punch_light", "fureur_crochet", "fureur_massue", "stand_kick_light", "stand_kick_heavy", "crouch_punch", "crouch_kick", "jump_punch", "jump_kick" }, fighter.Gameplay.Moves.Select(m => m.Id));
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

        /// <summary>Joue une suite d'appuis au contact de Napoléon (un appui par tick listé) ; renvoie les coups qui ont touché, dans l'ordre.</summary>
        static System.Collections.Generic.List<(string move, ActionKind before)> PlayAtContact(System.Func<int, Buttons> p1, System.Func<int, Buttons> p2, int ticks)
        {
            var session = AgainstNapoleon();
            session.Step(Buttons.None, Buttons.None);
            session.Step(Buttons.None, Buttons.None);
            session.State.Fighters[0].X = -100;
            session.State.Fighters[1].X = 60;
            var hits = new System.Collections.Generic.List<(string, ActionKind)>();
            for (int tick = 0; tick < ticks; tick++)
            {
                var before = session.State.Fighters[1].Action.Kind;
                foreach (var e in session.Step(p1(tick), p2(tick)))
                    if (e.Type == MatchEventType.Hit && e.Slot == 0) hits.Add((e.MoveId, before));
            }
            return hits;
        }

        [Test]
        public void LaFureurArverneEnchaineTroisPoingsEnCombo()
        {
            // Poing, poing, poing : appuis espacés de 4 ticks, pris dès que le coup précédent a touché.
            var hits = PlayAtContact(t => t % 4 == 0 && t < 80 ? Buttons.LightPunch : Buttons.None, _ => Buttons.None, 160);
            CollectionAssert.AreEqual(new[] { "stand_punch_light", "fureur_crochet", "fureur_massue" }, hits.Select(h => h.move));
            Assert.AreEqual(ActionKind.Hitstun, hits[1].before, "le crochet doit toucher pendant l'étourdissement du poing");
            Assert.AreEqual(ActionKind.Hitstun, hits[2].before, "la massue doit toucher pendant l'étourdissement du crochet");
        }

        [Test]
        public void LaFureurNePartPasDuNeutre()
        {
            var fighter = Vercingetorix().Gameplay;
            Assert.IsTrue(fighter.Moves.Single(m => m.Id == "fureur_crochet").CancelOnly);
            Assert.IsTrue(fighter.Moves.Single(m => m.Id == "fureur_massue").CancelOnly);
            // Un seul appui de poing : seul le poing sort.
            var hits = PlayAtContact(t => t == 0 ? Buttons.LightPunch : Buttons.None, _ => Buttons.None, 60);
            CollectionAssert.AreEqual(new[] { "stand_punch_light" }, hits.Select(h => h.move));
        }

        [Test]
        public void LeBouclierGauloisSeGardeDeboutPasAccroupi()
        {
            // Bas, bas-arrière, arrière + poing (Vercingétorix regarde à droite : arrière = gauche).
            Buttons Motion(int t) => t == 0 ? Buttons.Down : t == 1 ? Buttons.Down | Buttons.Left : t == 2 ? Buttons.Left | Buttons.LightPunch : Buttons.None;
            var crouching = PlayAtContact(Motion, _ => Buttons.Down | Buttons.Right, 90);
            CollectionAssert.AreEqual(new[] { "special_bouclier" }, crouching.Select(h => h.move), "garde basse : le coup d'en haut doit passer");
            var standing = PlayAtContact(Motion, _ => Buttons.Right, 90);
            CollectionAssert.IsEmpty(standing, "garde debout : le Bouclier gaulois doit être arrêté");
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
