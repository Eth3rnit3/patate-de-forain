using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.UnityAdapters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Patate.Tests
{
    /// <summary>
    /// Correspondance manette → boutons et « appuyer pour rejoindre », avec des
    /// appareils virtuels. Le banc de l'Input System isole l'état des entrées
    /// et fait tourner des mises à jour « joueur » : « appuyé pendant cette
    /// image » s'y comporte comme en jeu.
    /// </summary>
    public class GamepadInputTests : InputTestFixture
    {
        Gamepad pad;
        Keyboard keyboard;

        public override void Setup()
        {
            base.Setup();
            pad = InputSystem.AddDevice<Gamepad>("Manette de test");
            keyboard = InputSystem.AddDevice<Keyboard>("Clavier de test");
        }

        void Set(GamepadState state)
        {
            InputSystem.QueueStateEvent(pad, state);
            InputSystem.Update();
        }

        Buttons Read(ILocalInput input, GamepadState state)
        {
            Set(state);
            input.Sample();
            return input.InputFor(0);
        }

        static GamepadState Stick(float x, float y) => new GamepadState { leftStick = new Vector2(x, y) };

        [Test]
        public void StickEtCroixDonnentLesDirections()
        {
            var input = InputSources.Create(PlayerSource.Gamepad(pad.deviceId), 0, () => null);
            Assert.AreEqual(Buttons.Left, Read(input, Stick(-1, 0)));
            Assert.AreEqual(Buttons.Right | Buttons.Down, Read(input, Stick(0.7f, -0.7f)), "diagonale bas-droite");
            Assert.AreEqual(Buttons.None, Read(input, Stick(0.3f, 0.2f)), "une légère inclinaison ne compte pas");
            Assert.AreEqual(Buttons.Up | Buttons.Left, Read(input, new GamepadState(GamepadButton.DpadUp, GamepadButton.DpadLeft)));
        }

        [Test]
        public void BoutonsDeFaceEnDispositionArcade()
        {
            var input = InputSources.Create(PlayerSource.Gamepad(pad.deviceId), 0, () => null);
            Assert.AreEqual(Buttons.LightPunch, Read(input, new GamepadState(GamepadButton.West)));
            Assert.AreEqual(Buttons.LightKick, Read(input, new GamepadState(GamepadButton.South)));
            Assert.AreEqual(Buttons.HeavyKick, Read(input, new GamepadState(GamepadButton.East)));
            Assert.AreEqual(Buttons.Special | Buttons.Right, Read(input, new GamepadState(GamepadButton.North, GamepadButton.DpadRight)));
            Assert.AreEqual(Buttons.None, Read(input, new GamepadState(GamepadButton.Start)), "Start n'est pas une commande de combat");
        }

        [Test]
        public void UneManetteDebrancheeNeProduitRien()
        {
            var input = InputSources.Create(PlayerSource.Gamepad(pad.deviceId), 0, () => null);
            Read(input, new GamepadState(GamepadButton.West));
            InputSystem.RemoveDevice(pad);
            input.Sample();
            Assert.AreEqual(Buttons.None, input.InputFor(0));
        }

        [Test]
        public void LesManettesBrancheesSontProposees()
        {
            var sources = InputSources.Connected().ToList();
            CollectionAssert.Contains(sources, PlayerSource.Gamepad(pad.deviceId));
            CollectionAssert.Contains(sources, PlayerSource.Keyboard(0));
            StringAssert.StartsWith("Manette ", InputSources.Describe(PlayerSource.Gamepad(pad.deviceId)));
        }

        [Test]
        public void AppuyerPourRejoindreDesigneLAppareil()
        {
            Assert.IsEmpty(InputSources.JustPressed());
            Press(pad.buttonSouth);
            Assert.AreEqual(new[] { PlayerSource.Gamepad(pad.deviceId) }, InputSources.JustPressed().ToArray());
            InputSystem.Update();
            Assert.IsEmpty(InputSources.JustPressed(), "un bouton tenu ne rejoint qu'une fois");

            Press(keyboard.kKey);
            Assert.AreEqual(new[] { PlayerSource.Keyboard(1) }, InputSources.JustPressed().ToArray());
            Press(keyboard.aKey);
            Assert.IsEmpty(InputSources.JustPressed(), "une direction ne rejoint pas");
            Press(keyboard.fKey, queueEventOnly: true);
            Press(pad.buttonWest, queueEventOnly: true);
            InputSystem.Update();
            Assert.AreEqual(new[] { PlayerSource.Keyboard(0), PlayerSource.Gamepad(pad.deviceId) }, InputSources.JustPressed().ToArray(), "deux appareils dans la même image");
        }

        [Test]
        public void LAideNommeLesBoutons()
        {
            var help = InputSources.Create(PlayerSource.Gamepad(pad.deviceId), 0, () => null).Help().ToDictionary(h => h.action, h => h.keys);
            Assert.AreEqual("X / Carré", help["Poing"]);
            Assert.AreEqual("A / Croix", help["Pied rapide"]);
        }
    }
}
