#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace VectorRush.Tests
{
    public sealed class HoverVehicleInputTests
    {
        GameObject trackObject, vehicleObject, directorObject;
        HoverVehicle vehicle;
        RaceDirector director;
        Gamepad gamepad;
        object inputManager;
        PropertyInfo runPlayerUpdatesInEditMode;
        bool previousPlayerUpdatesInEditMode;
        InputSettings.UpdateMode previousUpdateMode;

        [SetUp] public void SetUp()
        {
            // InputManager.ShouldRunUpdate rejects player updates in EditMode unless
            // this package-provided override is set. Keep the override local to tests.
            inputManager = typeof(InputSystem).GetField("s_Manager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            runPlayerUpdatesInEditMode = inputManager.GetType().GetProperty("runPlayerUpdatesInEditMode");
            Assert.That(runPlayerUpdatesInEditMode, Is.Not.Null, "Installed InputSystem must expose its EditMode player-update override.");
            previousPlayerUpdatesInEditMode = (bool)runPlayerUpdatesInEditMode.GetValue(inputManager);
            previousUpdateMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            runPlayerUpdatesInEditMode.SetValue(inputManager, true);
            trackObject = new GameObject("Input test track");
            var track = trackObject.AddComponent<TrackPath>();
            track.Ensure();
            vehicleObject = new GameObject("Input test vehicle");
            vehicle = vehicleObject.AddComponent<HoverVehicle>();
            vehicle.Initialize(track, true, 0);
            directorObject = new GameObject("Input test director");
            director = directorObject.AddComponent<RaceDirector>();
            Invoke(director, "Awake");
            director.Initialize(track, new List<HoverVehicle> { vehicle });
            director.StartRace();
            typeof(RaceDirector).GetProperty(nameof(RaceDirector.CountdownRemaining)).SetValue(director, 0f);
            Invoke(director, "Update");
            Assert.That(director.Phase, Is.EqualTo(RacePhase.Racing));
            gamepad = InputSystem.AddDevice<Gamepad>();
            gamepad.MakeCurrent();
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            UpdateInput();
            // Register frame-press tracking before injecting the transition under test.
            Assert.That(gamepad.buttonNorth.wasPressedThisFrame, Is.False);
        }

        [TearDown] public void TearDown()
        {
            if (gamepad != null && gamepad.added) InputSystem.RemoveDevice(gamepad);
            if (directorObject) Object.DestroyImmediate(directorObject);
            if (vehicleObject) Object.DestroyImmediate(vehicleObject);
            if (trackObject) Object.DestroyImmediate(trackObject);
            if (runPlayerUpdatesInEditMode != null)
            {
                InputSystem.settings.updateMode = previousUpdateMode;
                runPlayerUpdatesInEditMode.SetValue(inputManager, previousPlayerUpdatesInEditMode);
            }
            Time.timeScale = 1f;
        }

        [Test] public void RecoveryPressedDuringPauseDoesNotExecuteOrSpendBoostAfterResume()
        {
            director.TogglePause();
            Assert.That(Time.timeScale, Is.Zero);
            InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.North));
            UpdateInput();
            gamepad.MakeCurrent();
            Assert.That(gamepad.buttonNorth.wasPressedThisFrame, Is.True);
            // Reproduce a render update with no intervening physics update while paused.
            Invoke(vehicle, "Update");
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            UpdateInput();
            float energyBeforeResume = vehicle.Boost01;
            director.TogglePause();
            Invoke(vehicle, "FixedUpdate");
            Assert.That(vehicle.RecoveryCount, Is.Zero);
            Assert.That(vehicle.Boost01, Is.EqualTo(energyBeforeResume));
        }

        [Test] public void RecoveryPressedWhileRacingStillExecutes()
        {
            InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.North));
            UpdateInput();
            gamepad.MakeCurrent();
            Assert.That(gamepad.buttonNorth.wasPressedThisFrame, Is.True);
            Invoke(vehicle, "Update");
            Invoke(vehicle, "FixedUpdate");
            Assert.That(vehicle.RecoveryCount, Is.EqualTo(1));
            Assert.That(vehicle.Boost01, Is.LessThan(1f));
        }

        [TestCase(.25f)] [TestCase(1f)]
        public void AnalogThrottleIsReportedIndependentlyOfHighCoastingSpeed(float trigger)
        {
            // Input-only fixture: seed velocity without advancing physics. The native
            // throttle evidence run reaches its coasting speed through real acceleration.
            vehicle.Body.linearVelocity=vehicle.transform.forward*60f;
            float speed=vehicle.SpeedKph;
            InputSystem.QueueStateEvent(gamepad,new GamepadState { rightTrigger=trigger });
            UpdateInput(); gamepad.MakeCurrent(); Invoke(vehicle,"Update");
            Assert.That(vehicle.ThrottleInput,Is.EqualTo(trigger).Within(.002f));
            Assert.That(vehicle.SpeedKph,Is.EqualTo(speed).Within(.01f));
            Assert.That(vehicle.SpeedKph,Is.GreaterThan(200f));
        }

        [Test] public void ReleasingTriggerReportsZeroThrottleWhileCoastingSpeedRemainsHigh()
        {
            vehicle.Body.linearVelocity=vehicle.transform.forward*60f;
            InputSystem.QueueStateEvent(gamepad,new GamepadState { rightTrigger=1f });
            UpdateInput(); gamepad.MakeCurrent(); Invoke(vehicle,"Update");
            Assert.That(vehicle.ThrottleInput,Is.EqualTo(1f).Within(.002f));
            float speed=vehicle.SpeedKph;
            InputSystem.QueueStateEvent(gamepad,new GamepadState());
            UpdateInput(); gamepad.MakeCurrent(); Invoke(vehicle,"Update");
            Assert.That(vehicle.ThrottleInput,Is.Zero.Within(.002f));
            Assert.That(vehicle.SpeedKph,Is.EqualTo(speed).Within(.01f));
            Assert.That(vehicle.SpeedKph,Is.GreaterThan(200f));
        }

        static void UpdateInput()
        {
            typeof(InputSystem).GetMethod("Update", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(InputUpdateType) }, null).Invoke(null, new object[] { InputUpdateType.Dynamic });
        }

        static void Invoke(object target, string method)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }
    }
}
#endif
