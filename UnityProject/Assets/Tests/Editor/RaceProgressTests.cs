using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class RaceProgressTests
    {
        static RaceProgress Started()
        {
            var progress = new RaceProgress(.99f);
            Assert.That(progress.Sample(.01f), Is.False, "First start-line crossing arms timing, never awards a lap.");
            Assert.That(progress.HasStarted, Is.True);
            return progress;
        }

        static void DriveToLastGate(RaceProgress progress)
        {
            for (int i = 1; i <= 11; i++)
            {
                progress.Sample(i / 12f - .01f);
                Assert.That(progress.Sample(i / 12f + .01f), Is.False);
            }
            progress.Sample(.99f);
        }

        [Test] public void CompleteOrderedForwardLapCountsExactlyOnce()
        {
            var progress = Started();
            DriveToLastGate(progress);
            Assert.That(progress.Sample(.01f), Is.True);
            Assert.That(progress.CompletedLaps, Is.EqualTo(1));
            Assert.That(progress.Sample(.01f), Is.False);
            Assert.That(progress.CompletedLaps, Is.EqualTo(1));
        }

        [Test] public void ReverseStartLineNeverArmsOrCounts()
        {
            var progress = new RaceProgress(.01f);
            progress.Sample(.99f);
            Assert.That(progress.HasStarted, Is.False);
            Assert.That(progress.CompletedLaps, Is.Zero);
        }

        [Test] public void SkippedCheckpointDoesNotCompleteLap()
        {
            var progress = Started();
            progress.Sample(.2f);
            for (float point = .22f; point < 1f; point += .02f) progress.Sample(point);
            Assert.That(progress.Sample(.01f), Is.False);
            Assert.That(progress.CompletedLaps, Is.Zero);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(1));
        }

        [Test] public void RepeatedForwardGateAfterReversingDoesNotAdvanceAgain()
        {
            var progress = Started();
            progress.Sample(.075f);
            progress.Sample(.09f);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(2));
            progress.Sample(.075f);
            progress.Sample(.09f);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(2));
        }

        [Test] public void ReverseLapDoesNotAwardAnything()
        {
            var progress = Started();
            for (int i = 49; i >= 0; i--) progress.Sample(i / 50f);
            progress.Sample(.99f);
            Assert.That(progress.CompletedLaps, Is.Zero);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(1));
        }

        [Test] public void RespawnAcrossFinishCannotCountLap()
        {
            var progress = Started();
            DriveToLastGate(progress);
            progress.NotifyRespawn(.01f);
            progress.Sample(.015f);
            Assert.That(progress.CompletedLaps, Is.Zero);
        }

        [Test] public void ResetClearsLapsGatesAndStartState()
        {
            var progress = Started();
            DriveToLastGate(progress);
            progress.Sample(.01f);
            progress.Reset(.99f);
            Assert.That(progress.CompletedLaps, Is.Zero);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(1));
            Assert.That(progress.HasStarted, Is.False);
            Assert.That(progress.Distance, Is.LessThan(0f));
        }

        [Test] public void ThreeConsecutiveLapsRemainValid()
        {
            var progress = Started();
            for (int lap = 1; lap <= 3; lap++)
            {
                DriveToLastGate(progress);
                Assert.That(progress.Sample(.01f), Is.True);
                Assert.That(progress.CompletedLaps, Is.EqualTo(lap));
            }
        }

        [Test] public void ShortcutCannotLeapToFrontOfUnvalidatedSector()
        {
            var progress = Started();
            progress.Sample(.7f);
            Assert.That(progress.Distance, Is.LessThanOrEqualTo(1f / RaceProgress.SectorCount));
        }

        [Test] public void RecoveryPreservesOrderedGatesAndCompletedLaps()
        {
            var progress = Started();
            DriveToLastGate(progress);
            progress.Sample(.01f);
            progress.Sample(.075f);
            progress.Sample(.09f);
            float recovery = progress.RecoveryProgress;
            int checkpoint = progress.NextCheckpoint;
            progress.NotifyRespawn(recovery);
            Assert.That(progress.CompletedLaps, Is.EqualTo(1));
            Assert.That(progress.NextCheckpoint, Is.EqualTo(checkpoint));
            Assert.That(progress.Distance, Is.EqualTo(1f + recovery).Within(.0001f));
            progress.Sample(recovery + .005f);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(checkpoint), "Respawning on the previous validated gate must not award it again.");
        }

        [Test] public void StartingGridRecoveryStillRequiresForwardStartCrossing()
        {
            var progress = new RaceProgress(.98f);
            progress.NotifyRespawn(progress.RecoveryProgress);
            Assert.That(progress.RecoveryProgress, Is.EqualTo(.98f), "Recovery must preserve the original grid distance.");
            Assert.That(progress.HasStarted, Is.False);
            progress.Sample(.985f);
            Assert.That(progress.Sample(.01f), Is.False);
            Assert.That(progress.HasStarted, Is.True);
            Assert.That(progress.CompletedLaps, Is.Zero);
        }

        [Test] public void FinishLineOscillationCannotAwardAnotherLap()
        {
            var progress = Started();
            DriveToLastGate(progress);
            Assert.That(progress.Sample(.01f), Is.True);
            for (int i = 0; i < 5; i++)
            {
                Assert.That(progress.Sample(.99f), Is.False);
                Assert.That(progress.Sample(.01f), Is.False);
            }
            Assert.That(progress.CompletedLaps, Is.EqualTo(1));
        }

        [Test] public void OffTrackCrossingAndReentryDoNotAdvanceGate()
        {
            var progress = Started();
            progress.Sample(.075f, false);
            progress.Sample(.09f, true);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(1));
            progress.Sample(.095f, true);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(1));
        }

        [Test] public void InvalidSampleAfterLastGateCannotFinishLap()
        {
            var progress = Started();
            DriveToLastGate(progress);
            Assert.That(progress.Sample(.995f, false), Is.False);
            Assert.That(progress.Sample(.01f, true), Is.False);
            Assert.That(progress.CompletedLaps, Is.Zero);
        }

        [Test] public void GateCorridorAcceptsEdgeHoverAndRejectsFlightOutsideRibbon()
        {
            var frame = new TrackFrame(Vector3.zero, Vector3.forward, 0f);
            Vector3 velocity = Vector3.forward * 30f;
            Assert.That(RaceDirector.IsValidGateSample(frame, new Vector3(11.5f, 1.55f, 0), velocity, 22f, 1.55f), Is.True);
            Assert.That(RaceDirector.IsValidGateSample(frame, new Vector3(13f, 1.55f, 0), velocity, 22f, 1.55f), Is.False);
            Assert.That(RaceDirector.IsValidGateSample(frame, new Vector3(0, 5f, 0), velocity, 22f, 1.55f), Is.False);
            Assert.That(RaceDirector.IsValidGateSample(frame, new Vector3(0, 1.55f, 0), -velocity, 22f, 1.55f), Is.False);
        }

        [Test] public void NonFiniteInputCannotCorruptProgressOrCreateCrossing()
        {
            var progress = Started();
            Assert.That(progress.Sample(float.NaN), Is.False);
            Assert.That(progress.Sample(float.PositiveInfinity), Is.False);
            Assert.That(float.IsNaN(progress.LastProgress), Is.False);
            progress.Sample(.09f);
            Assert.That(progress.NextCheckpoint, Is.EqualTo(1));
            progress.Reset(float.NegativeInfinity);
            Assert.That(float.IsInfinity(progress.LastProgress), Is.False);
            Assert.That(progress.CompletedLaps, Is.Zero);
        }

        [Test] public void FinishFractionDistinguishesCrossingsWithinSamePhysicsStep()
        {
            var earlier = Started();
            var later = Started();
            DriveToLastGate(earlier);
            DriveToLastGate(later);
            earlier.Sample(.995f);
            later.Sample(.98f);
            Assert.That(earlier.Sample(.01f), Is.True);
            Assert.That(later.Sample(.01f), Is.True);
            Assert.That(earlier.LastCrossingFraction, Is.LessThan(later.LastCrossingFraction));
        }
    }
}
