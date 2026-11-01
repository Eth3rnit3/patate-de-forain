using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Enchaînements des vrais paquets : chaque annulation déclarée doit faire un vrai combo au contact.</summary>
    public class ChainPackageTests
    {
        static FighterCatalog Catalog() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));

        /// <summary>Appui le plus simple du coup (sans quart de cercle), avec bas tenu pour un coup accroupi.</summary>
        static Buttons Press(MoveDefinition move)
        {
            var command = move.Commands.FirstOrDefault(c => c.Motion.Count == 0) ?? move.Commands[0];
            return command.Button | (move.Stance == MoveStance.Crouch ? Buttons.Down : Buttons.None);
        }

        /// <summary>
        /// Au contact de Napoléon immobile : le premier coup touche, on appuie la suite
        /// dès le tick suivant, et la suite doit toucher pendant l'étourdissement.
        /// Renvoie null si c'est un combo, sinon ce qui a manqué.
        /// </summary>
        static string Combo(FighterCatalog catalog, string fighterId, MoveDefinition first, MoveDefinition second)
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 1;
            var session = MatchSession.Start(catalog, fighterId, "napoleon", rules, StatPolicy.Free());
            session.Step(Buttons.None, Buttons.None);
            session.Step(Buttons.None, Buttons.None);
            var attacker = catalog.Get(fighterId).Gameplay.Body.Pushbox;
            var defender = catalog.Get("napoleon").Gameplay.Body.Pushbox;
            session.State.Fighters[0].X = -100;
            session.State.Fighters[1].X = -100 + attacker.X + attacker.W + defender.X + defender.W;

            bool firstHit = false, secondPressed = false;
            for (int tick = 0; tick < 150; tick++)
            {
                var input = !firstHit ? (tick == 0 ? Press(first) : first.Stance == MoveStance.Crouch ? Buttons.Down : Buttons.None)
                    : !secondPressed ? Press(second) : second.Stance == MoveStance.Crouch ? Buttons.Down : Buttons.None;
                if (firstHit) secondPressed = true;
                var stunnedBefore = session.State.Fighters[1].Action.Kind == ActionKind.Hitstun;
                foreach (var e in session.Step(input, Buttons.None))
                {
                    if (e.Type != MatchEventType.Hit || e.Slot != 0) continue;
                    if (!firstHit && e.MoveId == first.Id) firstHit = true;
                    else if (firstHit && (e.MoveId == second.Id || e.ProjectileId != null))
                        return stunnedBefore ? null : "la suite touche après la fin de l'étourdissement";
                }
            }
            return firstHit ? "la suite ne touche pas" : "le premier coup ne touche pas";
        }

        [Test]
        public void ChaqueEnchainementDeclareEstUnComboAuContact()
        {
            var catalog = Catalog();
            var failures = new List<string>();
            int chains = 0;
            foreach (var fighterId in catalog.Fighters.Select(f => f.Id))
            {
                var moves = catalog.Get(fighterId).Gameplay.Moves;
                // Une suite d'enchaînement ne part pas du neutre : son propre test la joue depuis le début de la chaîne.
                foreach (var first in moves.Where(m => !m.CancelOnly))
                    foreach (var targetId in first.Cancels.Concat(first.CancelsOnHit))
                    {
                        chains++;
                        var problem = Combo(catalog, fighterId, first, moves.Single(m => m.Id == targetId));
                        if (problem != null) failures.Add($"{fighterId} : {first.Id} → {targetId} : {problem}");
                    }
            }
            Assert.Greater(chains, 0);
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        [Test]
        public void SansAnnulationLaSuiteAttendLaFinDuCoup()
        {
            // Le pied puissant ne s'annule en aucun normal : appuyer poing pendant sa fin ne fait rien.
            var catalog = Catalog();
            var heavy = catalog.Get("napoleon").Gameplay.Moves.Single(m => m.Id == "stand_kick_heavy");
            Assert.IsFalse(heavy.Cancels.Contains("stand_punch_light"));
            var problem = Combo(catalog, "napoleon", heavy, catalog.Get("napoleon").Gameplay.Moves.Single(m => m.Id == "stand_punch_light"));
            Assert.IsNotNull(problem);
        }
    }
}
