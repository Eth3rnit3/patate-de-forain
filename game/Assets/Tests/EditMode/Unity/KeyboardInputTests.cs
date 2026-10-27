using NUnit.Framework;
using Patate.Combat;
using Patate.UnityAdapters;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Patate.Tests
{
    /// <summary>Correspondance touches physiques → boutons, avec un clavier virtuel de l'Input System.</summary>
    public class KeyboardInputTests
    {
        Keyboard keyboard;

        [SetUp]
        public void AddVirtualKeyboard()
        {
            keyboard = InputSystem.AddDevice<Keyboard>("Clavier de test");
            keyboard.MakeCurrent();
        }

        [TearDown]
        public void RemoveVirtualKeyboard() => InputSystem.RemoveDevice(keyboard);

        Buttons Press(KeyboardInput input, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            input.Sample();
            return input.InputFor(0);
        }

        [Test]
        public void Joueur1()
        {
            var input = KeyboardInput.PlayerOne();
            Assert.AreEqual(Buttons.Left | Buttons.LightPunch, Press(input, Key.A, Key.F));
            Assert.AreEqual(Buttons.Right | Buttons.Down | Buttons.LightKick, Press(input, Key.D, Key.S, Key.G));
            Assert.AreEqual(Buttons.HeavyKick | Buttons.Special, Press(input, Key.H, Key.R));
            Assert.AreEqual(Buttons.None, Press(input, Key.LeftArrow, Key.K), "les touches du joueur 2 n'agissent pas sur le joueur 1");
        }

        [Test]
        public void Joueur2AvecLesFlechesEtLePaveNumerique()
        {
            var input = KeyboardInput.PlayerTwo();
            Assert.AreEqual(Buttons.Left | Buttons.LightPunch, Press(input, Key.LeftArrow, Key.K));
            Assert.AreEqual(Buttons.Right | Buttons.LightKick, Press(input, Key.RightArrow, Key.Numpad2));
            Assert.AreEqual(Buttons.HeavyKick | Buttons.Special, Press(input, Key.Semicolon, Key.Numpad0));
            Assert.AreEqual(Buttons.None, Press(input, Key.A, Key.F), "les touches du joueur 1 n'agissent pas sur le joueur 2");
        }
    }
}
