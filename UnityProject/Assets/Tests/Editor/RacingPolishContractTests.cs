using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class RacingPolishContractTests
    {
        [TestCase(70f, true)]
        [TestCase(70.001f, false)]
        public void ActiveLifecycleAdjudicatesCrossingsBeforeTimeouts(float crossingTime, bool finishes)
        {
            var lifecycle = new RaceFinishLifecycle(new[]
            {
                new RacerIdentity("player", "YOU", true),
                new RacerIdentity("rival-a", "KIRA", false),
                new RacerIdentity("rival-b", "NOVA", false)
            }, 60f);
            lifecycle.BeginStep(10f);
            lifecycle.RecordFinish("player", 10f);
            lifecycle.BeginStep(60.01f);

            RacerFinishRecord result = lifecycle.RecordFinish("rival-a", crossingTime);
            lifecycle.FinalizeTimeouts();

            Assert.That(result != null, Is.EqualTo(finishes));
            Assert.That(lifecycle.Records.Single(value => value.RacerId == "rival-a").Status,
                Is.EqualTo(finishes ? RacerResultStatus.Finished : RacerResultStatus.DidNotFinish));
            Assert.That(lifecycle.Records.Single(value => value.RacerId == "rival-b").Status,
                Is.EqualTo(RacerResultStatus.DidNotFinish));
        }

        [Test]
        public void OrderedProgressReportsAcceptedSectorBoundaryOnce()
        {
            var progress = new RaceProgress(.99f);
            progress.Sample(.01f);
            for (int checkpoint = 1; checkpoint <= 4; checkpoint++)
            {
                float boundary = checkpoint / (float)RaceProgress.SectorCount;
                progress.Sample(boundary - .01f);
                progress.Sample(boundary + .01f);
            }

            Assert.That(progress.LastCrossedCheckpoint, Is.EqualTo(4));
            progress.Sample(4f / RaceProgress.SectorCount + .02f);
            Assert.That(progress.LastCrossedCheckpoint, Is.Zero);
        }

        [Test]
        public void CompetitiveGridStartsPlayerFourthWithTrafficAheadAndBehind()
        {
            float player = HoverVehicle.StartingDistanceBehindLine(true, 0);
            float[] rivals = Enumerable.Range(1, 5)
                .Select(index => HoverVehicle.StartingDistanceBehindLine(false, index)).ToArray();

            Assert.That(rivals.Count(distance => distance < player), Is.EqualTo(3));
            Assert.That(rivals.Count(distance => distance > player), Is.EqualTo(2));
            Assert.That(rivals[2], Is.EqualTo(24f), "The nearest rival ahead should be reachable from the second row.");
        }

        [Test]
        public void RecoveryLaneChoosesClearSideInsteadOfOccupiedCenter()
        {
            Assert.That(HoverVehicle.SelectRecoveryLane(22f, 0f, new[] { 0f, -5.8f }), Is.EqualTo(5.8f).Within(.001f));
            Assert.That(HoverVehicle.SelectRecoveryLane(22f, 0f, Array.Empty<float>()), Is.Zero);
        }

        [Test]
        public void ResolvedKinematicRacersCannotBlockActiveTraffic()
        {
            GameObject trackObject = new GameObject("Traffic filter track");
            GameObject activeObject = new GameObject("Active traffic");
            GameObject resolvedObject = new GameObject("Resolved traffic");
            try
            {
                TrackPath track = trackObject.AddComponent<TrackPath>();
                track.Ensure();
                HoverVehicle active = activeObject.AddComponent<HoverVehicle>();
                HoverVehicle resolved = resolvedObject.AddComponent<HoverVehicle>();
                active.Initialize(track, false, 1);
                resolved.Initialize(track, false, 2);
                active.Body.isKinematic = false;
                resolved.Body.isKinematic = true;

                Assert.That(HoverVehicle.CanInfluenceTraffic(active), Is.True);
                Assert.That(HoverVehicle.CanInfluenceTraffic(resolved), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(resolvedObject);
                UnityEngine.Object.DestroyImmediate(activeObject);
                UnityEngine.Object.DestroyImmediate(trackObject);
            }
        }

        [Test]
        public void AutomationEligibilityIsStickyAndRecoveryOnlyInvalidatesCurrentLap()
        {
            GameObject trackObject = new GameObject("Racing polish track");
            GameObject playerObject = new GameObject("Racing polish player");
            GameObject directorObject = new GameObject("Racing polish director");
            try
            {
                TrackPath track = trackObject.AddComponent<TrackPath>();
                track.Ensure();
                HoverVehicle player = playerObject.AddComponent<HoverVehicle>();
                player.Initialize(track, true, 0);
                RaceDirector director = directorObject.AddComponent<RaceDirector>();
                Invoke(director, "Awake");
                director.Initialize(track, new List<HoverVehicle> { player });
                director.StartRace();
                SetProperty(director, nameof(RaceDirector.Phase), RacePhase.Racing);

                Assert.That(director.ManualRunEligible, Is.True);
                Assert.That(director.CurrentLapEligible, Is.True);
                Invoke(director, "NotifyRecovery", player);
                Assert.That(director.CurrentLapEligible, Is.False);
                Assert.That(director.ManualRunEligible, Is.True);

                player.AutopilotForTesting = true;
                director.CanSimulate(player);
                player.AutopilotForTesting = false;
                Assert.That(director.ManualRunEligible, Is.False);
                director.CanSimulate(player);
                Assert.That(director.ManualRunEligible, Is.False, "Returning to manual control must not restore eligibility.");

                SetField(director, "playerCrossedStart", true);
                SetProperty(director, nameof(RaceDirector.LastLap), 42f);
                SetProperty(director, nameof(RaceDirector.Phase), RacePhase.Finished);
                Assert.That(director.CurrentLapTime, Is.EqualTo(42f),
                    "Lap-local time must freeze at the completed lap while rivals continue resolving.");

                director.RestartRace();
                Assert.That(director.ManualRunEligible, Is.True);
                Assert.That(director.CurrentLapEligible, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(directorObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
                UnityEngine.Object.DestroyImmediate(trackObject);
                Time.timeScale = 1f;
            }
        }

        [Test]
        public void InvalidFinalLapStaysInvalidAndEmitsZeroBasedFinishSector()
        {
            GameObject trackObject = new GameObject("Final-sector track");
            GameObject playerObject = new GameObject("Final-sector player");
            GameObject directorObject = new GameObject("Final-sector director");
            try
            {
                TrackPath track = trackObject.AddComponent<TrackPath>();
                track.Ensure();
                HoverVehicle player = playerObject.AddComponent<HoverVehicle>();
                player.Initialize(track, true, 0);
                RaceDirector director = directorObject.AddComponent<RaceDirector>();
                Invoke(director, "Awake");
                director.Initialize(track, new List<HoverVehicle> { player });
                director.StartRace();
                SetProperty(director, nameof(RaceDirector.CountdownRemaining), 0f);
                Invoke(director, "Update");

                RaceProgress progress = player.ProgressTracker;
                SetProperty(progress, nameof(RaceProgress.CompletedLaps), director.TotalLaps - 1);
                SetProperty(progress, nameof(RaceProgress.NextCheckpoint), RaceProgress.SectorCount);
                SetProperty(progress, nameof(RaceProgress.HasStarted), true);
                SetProperty(progress, nameof(RaceProgress.LastProgress), .99f);
                SetProperty(director, nameof(RaceDirector.CurrentLapEligible), false);
                SetField(director, "playerCrossedStart", true);

                TrackFrame frame = track.Evaluate(.01f);
                player.Body.position = frame.Position + frame.Up * player.HoverHeight;
                player.Body.rotation = Quaternion.LookRotation(frame.Forward, frame.Up);
                player.Body.linearVelocity = frame.Forward * 30f;
                OrderedRaceCrossing? observed = null;
                director.OrderedCrossed += crossing => observed = crossing;

                Invoke(director, "FixedUpdate");

                Assert.That(director.Phase, Is.EqualTo(RacePhase.Finished));
                Assert.That(director.CurrentLapEligible, Is.False);
                Assert.That(director.BestLap, Is.Zero);
                Assert.That(observed.HasValue, Is.True);
                Assert.That(observed.Value.LapIndex, Is.EqualTo(3));
                Assert.That(observed.Value.BoundaryIndex, Is.EqualTo(2));
                Assert.That(observed.Value.NormalizedBoundary, Is.EqualTo(1f));
                Assert.That(observed.Value.LapEligible, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(directorObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
                UnityEngine.Object.DestroyImmediate(trackObject);
                Time.timeScale = 1f;
            }
        }

        [Test]
        public void CameraKeepsExplicitLegacyComparisonModeAvailable()
        {
            GameObject cameraObject = new GameObject("Racing polish camera");
            try
            {
                cameraObject.AddComponent<Camera>();
                ChaseCamera chase = cameraObject.AddComponent<ChaseCamera>();
                chase.SetLegacyComparisonMode(true);
                Assert.That(chase.LegacyComparisonMode, Is.True);
                chase.SetLegacyComparisonMode(false);
                Assert.That(chase.LegacyComparisonMode, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(cameraObject); }
        }

        static object Invoke(object target, string method, params object[] arguments)
        {
            return target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(target, arguments);
        }

        static void SetProperty(object target, string property, object value)
        {
            target.GetType().GetProperty(property).SetValue(target, value);
        }

        static void SetField(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }
    }
}
