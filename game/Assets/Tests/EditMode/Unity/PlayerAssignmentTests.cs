using NUnit.Framework;
using Patate.Application;
using Patate.UnityAdapters;

namespace Patate.Tests
{
    /// <summary>Règles de l'écran « Qui joue ? » : appareils proposés, échange, Solo et en ligne, appareil débranché.</summary>
    public class PlayerAssignmentTests
    {
        static readonly PlayerSource Left = PlayerSource.Keyboard(0), Right = PlayerSource.Keyboard(1), Pad = PlayerSource.Gamepad(17), Pad2 = PlayerSource.Gamepad(21);
        static readonly PlayerSource[] KeyboardOnly = { Left, Right };
        static readonly PlayerSource[] WithPads = { Left, Right, Pad, Pad2 };

        static MatchSetup Previous(params PlayerSource[] players)
        {
            var setup = MatchSetup.Default();
            for (int i = 0; i < players.Length; i++) setup.Players[i] = players[i];
            return setup;
        }

        [Test]
        public void ParDefautChaqueJoueurAUneMoitieDuClavier()
        {
            var assignment = new PlayerAssignment(MatchMode.Versus, Previous(PlayerSource.Cpu(1), PlayerSource.Cpu(1)), KeyboardOnly);
            CollectionAssert.AreEqual(new PlayerSource?[] { Left, Right }, assignment.Devices);
            Assert.IsTrue(assignment.Ready(KeyboardOnly));
        }

        [Test]
        public void LesAppareilsPrecedentsSontReprisSilsSontBranches()
        {
            var assignment = new PlayerAssignment(MatchMode.Versus, Previous(Pad, Left), WithPads);
            CollectionAssert.AreEqual(new PlayerSource?[] { Pad, Left }, assignment.Devices);

            var unplugged = new PlayerAssignment(MatchMode.Versus, Previous(Pad, Left), KeyboardOnly);
            CollectionAssert.AreEqual(new PlayerSource?[] { Right, Left }, unplugged.Devices, "manette absente : premier appareil libre");
        }

        [Test]
        public void ChangerDAppareilPasseEnRevueLesAppareilsBranches()
        {
            var assignment = new PlayerAssignment(MatchMode.Versus, MatchSetup.Default(), WithPads);
            assignment.Command(MenuCommand.Right, WithPads); // Left → Right, pris par J2 : échange
            CollectionAssert.AreEqual(new PlayerSource?[] { Right, Left }, assignment.Devices);
            assignment.Command(MenuCommand.Right, WithPads);
            CollectionAssert.AreEqual(new PlayerSource?[] { Pad, Left }, assignment.Devices);
            assignment.Command(MenuCommand.Down, WithPads);
            assignment.Command(MenuCommand.Left, WithPads); // J2 : Left → Pad2 (en boucle)
            CollectionAssert.AreEqual(new PlayerSource?[] { Pad, Pad2 }, assignment.Devices);
            assignment.Command(MenuCommand.Left, WithPads); // Pad2 → Pad, pris par J1 : échange
            CollectionAssert.AreEqual(new PlayerSource?[] { Pad2, Pad }, assignment.Devices);
        }

        [Test]
        public void ValiderAppliqueLesAppareilsAuxReglages()
        {
            var assignment = new PlayerAssignment(MatchMode.Versus, MatchSetup.Default(), WithPads);
            assignment.Command(MenuCommand.Left, WithPads); // J1 : Left → Pad2
            Assert.IsTrue(assignment.Command(MenuCommand.Confirm, WithPads));
            var setup = MatchSetup.Default();
            assignment.ApplyTo(setup);
            Assert.AreEqual(MatchMode.Versus, setup.Mode);
            CollectionAssert.AreEqual(new[] { Pad2, Right }, setup.Players);
        }

        [Test]
        public void EnSoloSeulLeJoueurChoisitEtLOrdinateurGardeSaPlace()
        {
            var setup = Previous(Left, PlayerSource.Cpu(2));
            setup.Mode = MatchMode.Solo;
            var assignment = new PlayerAssignment(MatchMode.Solo, setup, WithPads);
            Assert.AreEqual(1, assignment.Humans);
            assignment.Command(MenuCommand.Down, WithPads);
            Assert.AreEqual(0, assignment.Row, "une seule ligne");
            assignment.Command(MenuCommand.Right, WithPads);
            assignment.Command(MenuCommand.Right, WithPads);
            assignment.ApplyTo(setup);
            CollectionAssert.AreEqual(new[] { Pad, PlayerSource.Cpu(2) }, setup.Players);
        }

        [Test]
        public void EnLigneLeJoueurLocalChoisitSonAppareil()
        {
            var assignment = new PlayerAssignment(MatchMode.Online, MatchSetup.Default(), WithPads);
            assignment.Command(MenuCommand.Left, WithPads);
            var setup = MatchSetup.Default();
            assignment.ApplyTo(setup);
            Assert.AreEqual(MatchMode.Online, setup.Mode);
            Assert.AreEqual(Pad2, setup.Players[0]);
        }

        [Test]
        public void UneManetteDebrancheeEmpecheDeContinuer()
        {
            var assignment = new PlayerAssignment(MatchMode.Versus, Previous(Pad, Right), WithPads);
            Assert.IsFalse(assignment.Command(MenuCommand.Confirm, KeyboardOnly), "la manette a été débranchée");
            Assert.IsFalse(assignment.Ready(KeyboardOnly));
            assignment.Command(MenuCommand.Right, KeyboardOnly); // appareil absent de la liste : on repart du premier
            CollectionAssert.AreEqual(new PlayerSource?[] { Left, Right }, assignment.Devices);
            Assert.IsTrue(assignment.Command(MenuCommand.Confirm, KeyboardOnly));
        }
    }
}
