using NUnit.Framework;
using Patate.Application;
using Patate.UnityAdapters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Patate.Tests
{
    /// <summary>
    /// Commandes des menus sur une manette virtuelle de l'Input System. Les
    /// appuis de boutons (wasPressedThisFrame) ne se déclenchent pas en
    /// EditMode sans le cadre de test de l'Input System : seuls le stick, lu
    /// par valeur, et la liste des appareils sont vérifiés ici.
    /// </summary>
    public class MenuInputTests
    {
        Gamepad pad;
        Keyboard keyboard;
        PlayerSource device;

        [SetUp]
        public void AddVirtualDevices()
        {
            pad = InputSystem.AddDevice<Gamepad>("Manette de test");
            keyboard = InputSystem.AddDevice<Keyboard>("Clavier de test");
            keyboard.MakeCurrent();
            int index = 0;
            while (Gamepad.all[index] != pad) index++;
            device = PlayerSource.Gamepad(index);
            InputSystem.Update();
        }

        [TearDown]
        public void RemoveVirtualDevices()
        {
            InputSystem.RemoveDevice(pad);
            InputSystem.RemoveDevice(keyboard);
        }

        MenuCommand Stick(float x, float y)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(x, y) });
            InputSystem.Update();
            return MenuInput.Of(device);
        }

        [Test]
        public void LeStickDonneUnAppuiPuisAttendAvantDeRepeter()
        {
            Assert.AreEqual(MenuCommand.Down, Stick(0, -0.9f));
            Assert.AreEqual(MenuCommand.None, Stick(0, -0.9f), "pas de répétition avant le délai");
            Assert.AreEqual(MenuCommand.None, Stick(0.3f, 0), "sous le seuil : rien");
            Assert.AreEqual(MenuCommand.Left, Stick(-0.8f, 0.2f), "la direction dominante l'emporte");
            Assert.AreEqual(MenuCommand.Up, Stick(0.1f, 0.7f));
        }

        [Test]
        public void LeStickPiloteAussiLesMenusCommuns()
        {
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(0.9f, 0) });
            InputSystem.Update();
            Assert.AreEqual(MenuCommand.Right, MenuInput.Any());
        }

        [Test]
        public void LesDeuxMoitiesDuClavierPuisChaqueManette()
        {
            CollectionAssert.AreEqual(new[] { PlayerSource.Keyboard(0), PlayerSource.Keyboard(1) }, System.Linq.Enumerable.Take(MenuInput.Devices(), 2));
            CollectionAssert.Contains(MenuInput.Devices(), device);
        }
    }
}
