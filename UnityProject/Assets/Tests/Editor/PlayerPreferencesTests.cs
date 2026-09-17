using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class PlayerPreferencesTests
    {
        string directory;

        [SetUp]
        public void SetUp() => directory = Path.Combine(Path.GetTempPath(), "VectorRush-Preferences-" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void SavedLevelsAndSensitivityClampAndRelaunch()
        {
            var preferences = new PlayerPreferences(directory);
            preferences.SetVolumes(-2f, .45f, 8f);
            preferences.SetSteeringSensitivity(4f);
            preferences.SetShake(false);
            preferences.Save();
            var relaunched = new PlayerPreferences(directory);
            Assert.That(relaunched.MasterVolume, Is.Zero);
            Assert.That(relaunched.MusicVolume, Is.EqualTo(.45f));
            Assert.That(relaunched.EffectsVolume, Is.EqualTo(1f));
            Assert.That(relaunched.SteeringSensitivity, Is.EqualTo(1.5f));
            Assert.That(relaunched.ShakeEnabled, Is.False);
        }

        [Test]
        public void ConflictingRequiredDrivingBindingIsRejectedWithoutChangingEitherAction()
        {
            var preferences = new PlayerPreferences(directory);
            KeyCode throttle = preferences.BindingFor(PlayerAction.Throttle);
            KeyCode brake = preferences.BindingFor(PlayerAction.Brake);
            bool accepted = preferences.TryRebind(PlayerAction.Throttle, brake, out string error);
            Assert.That(accepted, Is.False);
            Assert.That(error, Does.Contain("Brake"));
            Assert.That(preferences.BindingFor(PlayerAction.Throttle), Is.EqualTo(throttle));
            Assert.That(preferences.BindingFor(PlayerAction.Brake), Is.EqualTo(brake));
        }

        [Test]
        public void ResetRestoresEveryDocumentedDefault()
        {
            var preferences = new PlayerPreferences(directory);
            Assert.That(preferences.TryRebind(PlayerAction.Throttle, KeyCode.T, out _), Is.True);
            preferences.SetVolumes(.2f, .3f, .4f);
            preferences.SetSteeringSensitivity(.5f);
            preferences.SetShake(false);
            preferences.ResetDefaults();
            Assert.That(preferences.MasterVolume, Is.EqualTo(1f));
            Assert.That(preferences.MusicVolume, Is.EqualTo(.72f));
            Assert.That(preferences.EffectsVolume, Is.EqualTo(1f));
            Assert.That(preferences.SteeringSensitivity, Is.EqualTo(1f));
            Assert.That(preferences.ShakeEnabled, Is.True);
            Assert.That(preferences.BindingFor(PlayerAction.Throttle), Is.EqualTo(KeyCode.W));
            Assert.That(preferences.BindingFor(PlayerAction.Brake), Is.EqualTo(KeyCode.S));
            Assert.That(preferences.BindingFor(PlayerAction.SteerLeft), Is.EqualTo(KeyCode.A));
            Assert.That(preferences.BindingFor(PlayerAction.SteerRight), Is.EqualTo(KeyCode.D));
            Assert.That(preferences.BindingFor(PlayerAction.Boost), Is.EqualTo(KeyCode.Space));
            Assert.That(preferences.BindingFor(PlayerAction.Recover), Is.EqualTo(KeyCode.R));
        }

        [Test]
        public void MalformedPreferencesFallBackWithoutDeletingUnrelatedData()
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "player-preferences.json"), "bad-json");
            File.WriteAllText(Path.Combine(directory, "unrelated.dat"), "keep");
            var preferences = new PlayerPreferences(directory);
            Assert.That(preferences.MasterVolume, Is.EqualTo(1f));
            Assert.That(preferences.BindingFor(PlayerAction.Boost), Is.EqualTo(KeyCode.Space));
            Assert.That(File.ReadAllText(Path.Combine(directory, "unrelated.dat")), Is.EqualTo("keep"));
            Assert.That(File.ReadAllText(Path.Combine(directory, "player-preferences.json")), Is.EqualTo("bad-json"));
        }

        [Test]
        public void ControllerFocusCanReachAndActivateEveryPauseAction()
        {
            string[] actions = { "resume", "restart", "settings", "quit" };
            var focus = new MenuFocusController(actions);
            for (int i = 0; i < actions.Length; i++)
            {
                Assert.That(focus.SelectedAction, Is.EqualTo(actions[i]));
                Assert.That(focus.Activate(), Is.EqualTo(actions[i]));
                focus.Move(1);
            }
            Assert.That(focus.SelectedAction, Is.EqualTo(actions[0]));
        }
    }
}
