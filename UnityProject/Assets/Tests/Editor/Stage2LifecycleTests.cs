using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VectorRush.Tests
{
    public sealed class Stage2LifecycleTests
    {
        [Test]
        public void ReturningFromPausedResultsStopsPendingRivalsAndAllowsFreshRace()
        {
            string profile = Path.Combine(Path.GetTempPath(), "vr-stage2-" + Guid.NewGuid().ToString("N"));
            var owned = new List<GameObject>();
            try
            {
                var trackObject = new GameObject("Stage2 track"); owned.Add(trackObject);
                var track = trackObject.AddComponent<TrackPath>(); track.Ensure();
                var racers = new List<HoverVehicle>();
                for (int i = 0; i < 2; i++)
                {
                    var obj = new GameObject("Stage2 racer " + i); owned.Add(obj);
                    var racer = obj.AddComponent<HoverVehicle>(); racer.Initialize(track, i == 0, i);
                    racers.Add(racer);
                }
                var directorObject = new GameObject("Stage2 director"); owned.Add(directorObject);
                var director = directorObject.AddComponent<RaceDirector>();
                director.Initialize(track, racers, "stage2-test", new RaceRecords(profile));
                director.StartRace();
                var field = typeof(RaceDirector).GetField("finishLedger", BindingFlags.NonPublic | BindingFlags.Instance);
                var ledger = (RaceFinishLedger)field.GetValue(director);
                ledger.Advance(100f); ledger.Cross(0, 100f);
                typeof(RaceDirector).GetProperty("Phase").SetValue(director, RacePhase.Finished);
                director.TogglePause();
                Assert.That(Time.timeScale, Is.Zero);
                director.ReturnToTitle();
                Assert.That(director.Phase, Is.EqualTo(RacePhase.Menu));
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                Assert.That(director.HasPendingRivals, Is.False, "Leaving results must stop the post-finish continuation.");
                foreach (var racer in racers)
                {
                    Assert.That(director.CanSimulate(racer), Is.False);
                    Assert.That(racer.Body.isKinematic, Is.True);
                }
                Assert.That(director.FinishRecords, Is.Empty);
                Assert.That(director.CurrentRecordComparison, Is.Null);
                Assert.That(File.Exists(Path.Combine(profile, "race-records.json")), Is.False);
                director.StartRace();
                Assert.That(director.Phase, Is.EqualTo(RacePhase.Countdown));
                Assert.That(director.RaceTime, Is.Zero);
                Assert.That(director.IsFinished(racers[0]), Is.False);
                Assert.That(racers[0].Body.detectCollisions, Is.True);
            }
            finally
            {
                for (int i = owned.Count - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(owned[i]);
                Time.timeScale = 1f;
                if (Directory.Exists(profile)) Directory.Delete(profile, true);
            }
        }

        [Test]
        public void FailedRecordWriteIsExposedWithoutInventingPersonalBestAndClearsOnRetry()
        {
            string directory = Path.Combine(Path.GetTempPath(), "vr-record-failure-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string blocker = Path.Combine(directory, "blocked");
            File.WriteAllText(blocker, "A file prevents creating the persistence directory.");
            try
            {
                var records = new RaceRecords(Path.Combine(blocker, "records.json"));
                LogAssert.Expect(LogType.Warning, new Regex("Could not save personal best:"));
                var failed = records.Submit("stage2-test", RaceDirector.DrivingRulesId, 40f, 125f, false);
                Assert.That(records.LastSaveError, Is.Not.Empty);
                Assert.That(failed.UpdatedLapBest || failed.UpdatedRaceBest, Is.False);
                Assert.That(records.GetBest("stage2-test", RaceDirector.DrivingRulesId).HasBestLap, Is.False);
                File.Delete(blocker);
                var saved = records.Submit("stage2-test", RaceDirector.DrivingRulesId, 40f, 125f, false);
                Assert.That(saved.UpdatedLapBest, Is.True);
                Assert.That(records.LastSaveError, Is.Empty);
            }
            finally { Directory.Delete(directory, true); }
        }
    }
}
