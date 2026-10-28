using NUnit.Framework;
using Patate.Application;
using Patate.UnityAdapters;

namespace Patate.Tests
{
    /// <summary>Règles de l'écran de sélection : rejoindre, choisir, verrouiller, décor, réglages produits.</summary>
    public class CharacterSelectionTests
    {
        static readonly string[] Roster = { "napoleon", "saint-louis" };
        static readonly PlayerSource Left = PlayerSource.Keyboard(0), Right = PlayerSource.Keyboard(1), Pad = PlayerSource.Gamepad(0);

        static CharacterSelection Versus(params string[] stages) =>
            new CharacterSelection(MatchMode.Versus, Roster, stages.Length > 0 ? stages : new[] { "training" }, MatchSetup.Default());

        [Test]
        public void ChaqueAppareilPrendLaPremierePlaceLibre()
        {
            var selection = Versus();
            Assert.AreEqual(0, selection.Join(Pad));
            Assert.AreEqual(1, selection.Join(Right));
            Assert.AreEqual(0, selection.Join(Pad), "rejoindre deux fois ne change pas de place");
            Assert.AreEqual(-1, selection.Join(Left), "plus de place");
        }

        [Test]
        public void VersusProduitLesReglagesChoisis()
        {
            var selection = Versus();
            Assert.AreEqual(1, selection.Slots[0].Cursor, "curseur de départ sur le combattant précédent");
            selection.Join(Right);
            selection.Join(Pad);
            selection.Command(Right, MenuCommand.Left);
            selection.Command(Right, MenuCommand.Confirm);
            Assert.AreEqual(SelectionPhase.Fighters, selection.Phase);
            selection.Command(Pad, MenuCommand.Confirm);
            Assert.AreEqual(SelectionPhase.Done, selection.Phase, "un seul décor : pas d'étape de décor");

            var setup = selection.ToSetup();
            Assert.AreEqual(MatchMode.Versus, setup.Mode);
            CollectionAssert.AreEqual(new[] { "napoleon", "saint-louis" }, setup.Fighters);
            CollectionAssert.AreEqual(new[] { Right, Pad }, setup.Players);
            Assert.AreEqual("training", setup.Stage);
        }

        [Test]
        public void RetourDeverrouillePuisLibereLaPlace()
        {
            var selection = Versus();
            selection.Join(Left);
            selection.Command(Left, MenuCommand.Confirm);
            Assert.IsTrue(selection.Slots[0].Locked);
            selection.Command(Left, MenuCommand.Right);
            Assert.AreEqual(1, selection.Slots[0].Cursor, "verrouillé : le curseur ne bouge plus");
            selection.Command(Left, MenuCommand.Back);
            Assert.IsFalse(selection.Slots[0].Locked);
            selection.Command(Left, MenuCommand.Back);
            Assert.IsFalse(selection.Slots[0].Joined);
            selection.Command(Left, MenuCommand.Confirm);
            Assert.IsFalse(selection.Slots[0].Locked, "un appareil qui a quitté sa place ne pilote plus rien");
        }

        [Test]
        public void PlusieursDecorsAjoutentUneEtapeDeChoix()
        {
            var selection = Versus("fete", "training");
            selection.Join(Left);
            selection.Join(Right);
            selection.Command(Left, MenuCommand.Confirm);
            selection.Command(Right, MenuCommand.Confirm);
            Assert.AreEqual(SelectionPhase.Stage, selection.Phase);
            Assert.AreEqual("training", selection.Stages[selection.StageCursor], "décor de départ : le précédent");
            selection.Command(Right, MenuCommand.Right);
            selection.Command(Right, MenuCommand.Back);
            Assert.AreEqual(SelectionPhase.Fighters, selection.Phase, "retour : on revient aux combattants");
            selection.Command(Right, MenuCommand.Confirm);
            selection.Command(Left, MenuCommand.Confirm);
            Assert.AreEqual("fete", selection.ToSetup().Stage);
        }

        [Test]
        public void SoloLeJoueurChoisitAussiPourLOrdinateur()
        {
            var cpu = PlayerSource.Cpu(3);
            var selection = new CharacterSelection(MatchMode.Solo, Roster, new[] { "training" }, MatchSetup.Default(), cpu);
            Assert.IsTrue(selection.Slots[1].IsCpu);
            Assert.AreEqual(0, selection.Join(Pad));
            Assert.AreEqual(-1, selection.Join(Left), "Solo : une seule place humaine");

            selection.Command(Pad, MenuCommand.Confirm);
            Assert.AreEqual(1, selection.ControlledSlot(Pad), "après son choix, le joueur pilote le curseur de l'ordinateur");
            selection.Command(Pad, MenuCommand.Right);
            selection.Command(Pad, MenuCommand.Back);
            Assert.IsFalse(selection.Slots[0].Locked, "retour : le joueur revient à son propre choix");
            selection.Command(Pad, MenuCommand.Confirm);
            selection.Command(Pad, MenuCommand.Confirm);
            Assert.AreEqual(SelectionPhase.Done, selection.Phase);

            var setup = selection.ToSetup();
            Assert.AreEqual(MatchMode.Solo, setup.Mode);
            CollectionAssert.AreEqual(new[] { Pad, cpu }, setup.Players);
            CollectionAssert.AreEqual(new[] { "saint-louis", "napoleon" }, setup.Fighters);
        }

        [Test]
        public void ChangerDePersonnageGardeLesJoueurs()
        {
            var previous = new MatchSetup { Mode = MatchMode.Versus, Players = new[] { Pad, Left }, Fighters = new[] { "napoleon", "napoleon" } };
            var selection = new CharacterSelection(MatchMode.Versus, Roster, new[] { "training" }, previous, keepPlayers: true);
            Assert.AreEqual(0, selection.SlotOf(Pad));
            Assert.AreEqual(1, selection.SlotOf(Left));
            Assert.AreEqual(0, selection.Slots[1].Cursor);
            Assert.IsFalse(selection.Slots[0].Locked);
        }
    }
}
