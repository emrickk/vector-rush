using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class ReplayPolishGhostTests
    {
        string directory;

        [SetUp]
        public void SetUp() => directory = Path.Combine(Path.GetTempPath(), "VectorRush-Replay-" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            foreach (ReplayPolishController controller in UnityEngine.Object.FindObjectsByType<ReplayPolishController>(FindObjectsSortMode.None))
                UnityEngine.Object.DestroyImmediate(controller.gameObject);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            else if (File.Exists(directory)) File.Delete(directory);
        }

        [Test]
        public void EligibleBestLapSavesLoadsAndSlowerLapDoesNotReplaceIt()
        {
            var store = new PersonalBestGhostStore(directory);
            GhostSaveResult first = store.TrySaveIfFaster(Recording("course-a", "rules-1", 3f));
            GhostSaveResult slower = store.TrySaveIfFaster(Recording("course-a", "rules-1", 4f));
            GhostLoadResult loaded = new PersonalBestGhostStore(directory).Load("course-a", "rules-1");

            Assert.That(first.Status, Is.EqualTo(GhostSaveStatus.Saved));
            Assert.That(slower.Status, Is.EqualTo(GhostSaveStatus.NotFaster));
            Assert.That(loaded.Status, Is.EqualTo(GhostLoadStatus.Loaded));
            Assert.That(loaded.Recording.lapTime, Is.EqualTo(3f));
            Assert.That(new FileInfo(store.FilePath).Length, Is.LessThanOrEqualTo(GhostRecordingSchema.MaximumFileBytes));
        }

        [Test]
        public void FirstRunIncompatibleAndCorruptFilesHaveDistinctSafeFallbacks()
        {
            var store = new PersonalBestGhostStore(directory);
            Assert.That(store.Load("course-a", "rules-1").Status, Is.EqualTo(GhostLoadStatus.NotFound));
            Assert.That(store.TrySaveIfFaster(Recording("course-b", "rules-1", 3f)).Saved, Is.True);
            Assert.That(store.Load("course-a", "rules-1").Status, Is.EqualTo(GhostLoadStatus.Incompatible));

            File.WriteAllText(store.FilePath, "{broken-json");
            GhostLoadResult corrupt = store.Load("course-a", "rules-1");
            Assert.That(corrupt.Status, Is.EqualTo(GhostLoadStatus.Unreadable));
            Assert.That(File.ReadAllText(store.FilePath), Is.EqualTo("{broken-json"));
        }

        [Test]
        public void FailedPersistenceNeverReportsSuccess()
        {
            File.WriteAllText(directory, "this path is a file, not a directory");
            GhostSaveResult result = new PersonalBestGhostStore(directory).TrySaveIfFaster(Recording("course", "rules", 3f));
            Assert.That(result.Status, Is.EqualTo(GhostSaveStatus.Failed));
            Assert.That(result.Saved, Is.False);
        }

        [Test]
        public void SamplingIsBoundedAndPlaybackInterpolatesAcrossProgressWrap()
        {
            GhostLapRecording recording = Recording("course", "rules", 3f);
            Assert.That(GhostPlayback.TryEvaluate(recording, 1.5f, out GhostPose pose), Is.True);
            Assert.That(pose.Position.x, Is.EqualTo(1.5f).Within(.001f));
            Assert.That(pose.Progress, Is.EqualTo(0f).Within(.001f));
            Assert.That(GhostPlayback.TryEvaluate(recording, 3.01f, out _), Is.False, "Playback must finish cleanly after the saved lap.");

            var builder = new GhostLapBuilder();
            for (int i = 0; i < GhostRecordingSchema.MaximumSamples + 20; i++)
                builder.TrySample(i / (float)GhostRecordingSchema.SamplesPerSecond, Vector3.zero, Quaternion.identity, 0f);
            Assert.That(builder.SampleCount, Is.LessThanOrEqualTo(GhostRecordingSchema.MaximumSamples));
            Assert.That(builder.Finish("course", "rules", GhostRecordingSchema.MaximumLapSeconds, new[] { 100f, 200f, 300f },
                Vector3.zero, Quaternion.identity, 0f), Is.Null, "An overflowed recording is never persisted as if complete.");
        }

        [Test]
        public void PlaybackUsesOnlyAuthoritativeLapTimeSoPauseCannotAdvanceIt()
        {
            GhostLapRecording recording = Recording("course", "rules", 3f);
            Assert.That(GhostPlayback.TryEvaluate(recording, 1.25f, out GhostPose beforePause), Is.True);
            Assert.That(GhostPlayback.TryEvaluate(recording, 1.25f, out GhostPose duringPause), Is.True);
            Assert.That(duringPause.Position, Is.EqualTo(beforePause.Position));
            Assert.That(duringPause.Rotation, Is.EqualTo(beforePause.Rotation));
            Assert.That(duringPause.Progress, Is.EqualTo(beforePause.Progress));
        }

        [Test]
        public void SectorCrossingsMustBeOrderedAndRecoveryInvalidatesRemainingSplits()
        {
            var sectors = new SectorComparisonTracker(new[] { 10f, 20f, 30f });
            Assert.That(sectors.TryRecord(1, 19f, true, out _), Is.False);
            Assert.That(sectors.TryRecord(0, 9.5f, true, out float delta), Is.True);
            Assert.That(delta, Is.EqualTo(-.5f).Within(.001f));
            Assert.That(sectors.TryRecord(0, 9.6f, true, out _), Is.False);
            sectors.InvalidateLap();
            Assert.That(sectors.TryRecord(1, 19f, true, out _), Is.False);
            Assert.That(sectors.CompletedSectors, Is.EqualTo(1));
        }

        [Test]
        public void NewLapAndRecoveryClearSectorPresentationInsteadOfMixingLaps()
        {
            Assert.That(new PersonalBestGhostStore(directory).TrySaveIfFaster(Recording("course", "rules", 3f)).Saved, Is.True);
            Transform player = new GameObject("Replay player").transform;
            var controller = new GameObject("Replay test").AddComponent<ReplayPolishController>();
            controller.Initialize("course", "rules", player, directory);
            controller.RaceStarted(true);

            controller.ObserveRace(.1f, .1f, .05f, true, true, RacePhase.Racing);
            controller.OrderedCrossing(0, 1.1f, true);
            controller.OrderedCrossing(1, 2.1f, true);
            controller.ObserveRace(2.9f, 2.9f, .95f, true, true, RacePhase.Racing);
            controller.OrderedCrossing(2, 3.1f, true, new Vector3(3f, 0f, 0f), Quaternion.identity, 0f);
            Assert.That(controller.ResultSectorHasReference(0), Is.True);
            Assert.That(controller.ResultSectorHasReference(1), Is.True);
            Assert.That(controller.ResultSectorHasReference(2), Is.True);

            controller.ObserveRace(3.3f, .2f, .03f, true, true, RacePhase.Racing);
            Assert.That(controller.HasLatestSectorDelta, Is.False);
            Assert.That(controller.ResultSectorHasReference(0), Is.False);
            Assert.That(controller.ResultSectorHasReference(1), Is.False);
            Assert.That(controller.ResultSectorHasReference(2), Is.False);

            controller.OrderedCrossing(0, 1f, true);
            Assert.That(controller.ResultSectorHasReference(0), Is.True);
            controller.InvalidateCurrentLap();
            controller.OrderedCrossing(2, 3f, false);
            Assert.That(controller.HasLatestSectorDelta, Is.False);
            Assert.That(controller.ResultSectorHasReference(0), Is.False);
            Assert.That(controller.ResultSectorHasReference(1), Is.False);
            Assert.That(controller.ResultSectorHasReference(2), Is.False);

            UnityEngine.Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void BoundarySamplesPreserveEligibleLapAcrossStartRenderDelay()
        {
            Transform player = new GameObject("Replay player").transform;
            var controller = new GameObject("Replay test").AddComponent<ReplayPolishController>();
            controller.Initialize("course", "rules", player, directory);
            controller.RaceStarted(true);

            player.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            controller.ObserveRace(0f, 0f, .99f, true, true, RacePhase.Countdown);
            player.SetPositionAndRotation(new Vector3(10f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            controller.ObserveRace(.1f, .08f, .02f, true, true, RacePhase.Racing);
            controller.OrderedCrossing(0, 1f, true);
            controller.OrderedCrossing(1, 2f, true);
            player.SetPositionAndRotation(new Vector3(20f, 0f, 0f), Quaternion.Euler(0f, 120f, 0f));
            controller.ObserveRace(2.9f, 2.88f, .90f, true, true, RacePhase.Racing);
            var finish = new OrderedRaceCrossing(1, 2, 1f, 3f, 2.98f, true);
            controller.OrderedCrossing(finish, 3.02f, new Vector3(32f, 0f, 0f), Quaternion.Euler(0f, 180f, 0f), .02f);

            GhostLoadResult loaded = new PersonalBestGhostStore(directory).Load("course", "rules");
            Assert.That(loaded.Status, Is.EqualTo(GhostLoadStatus.Loaded));
            Assert.That(loaded.Recording.samples[0].time, Is.EqualTo(0f));
            Assert.That(loaded.Recording.samples[0].position.x, Is.EqualTo(2f).Within(.001f),
                "The start pose should be interpolated at the authoritative crossing, not copied from the delayed render.");
            Assert.That(loaded.Recording.samples[loaded.Recording.samples.Length - 1].time, Is.EqualTo(2.98f));
            Assert.That(loaded.Recording.samples[loaded.Recording.samples.Length - 1].position.x, Is.EqualTo(30f));

            UnityEngine.Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void MixedAutomatedManualRunStaysIneligibleAndRestartClearsPlaybackState()
        {
            var controller = new GameObject("Replay test").AddComponent<ReplayPolishController>();
            controller.Initialize("course", "rules", null, directory);
            controller.RaceStarted(true);
            controller.ObserveRace(1f, .1f, false, true, RacePhase.Racing);
            controller.ObserveRace(2f, .2f, true, true, RacePhase.Racing);
            Assert.That(controller.ManualRunEligible, Is.False);
            Assert.That(controller.CurrentLapEligible, Is.False);
            controller.OrderedCrossing(0, 10f, true);
            Assert.That(controller.HasLatestSectorDelta, Is.False);

            controller.RaceRestarted();
            Assert.That(controller.ManualRunEligible, Is.True);
            Assert.That(controller.GhostVisible, Is.False);
            Assert.That(controller.HasLatestSectorDelta, Is.False);
        }

        [Test]
        public void GhostPreferenceDefaultsOnAndPersistsOff()
        {
            var preferences = new PlayerPreferences(directory);
            Assert.That(preferences.GhostEnabled, Is.True);
            preferences.SetGhostEnabled(false);
            preferences.Save();
            Assert.That(new PlayerPreferences(directory).GhostEnabled, Is.False);
        }

        static GhostLapRecording Recording(string course, string rules, float lapTime)
        {
            return new GhostLapRecording
            {
                courseIdentity = course,
                drivingRulesIdentity = rules,
                lapTime = lapTime,
                sectorTimes = new[] { lapTime / 3f, lapTime * 2f / 3f, lapTime },
                samples = new[]
                {
                    new GhostSample { time = 0f, position = Vector3.zero, rotation = Quaternion.identity, progress = .95f },
                    new GhostSample { time = lapTime, position = new Vector3(lapTime, 0f, 0f), rotation = Quaternion.Euler(0f, 90f, 0f), progress = .05f }
                }
            };
        }
    }
}
