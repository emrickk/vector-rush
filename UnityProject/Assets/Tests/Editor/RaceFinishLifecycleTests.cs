using System.Linq;
using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class RaceFinishLifecycleTests
    {
        [Test]
        public void PlayerResultStaysImmutableWhileRivalsFinishAndOnlyTimeoutRemainderDnf()
        {
            var lifecycle = new RaceFinishLifecycle(new[]
            {
                new RacerIdentity("player", "YOU", true),
                new RacerIdentity("rival-a", "KIRA", false),
                new RacerIdentity("rival-b", "NOVA", false)
            }, 60f);
            lifecycle.Advance(90.875f);
            RacerFinishRecord player = lifecycle.RecordFinish("player", 90.875f);
            Assert.That(player.FinishTime, Is.EqualTo(90.875f));
            Assert.That(player.Position, Is.EqualTo(1));
            Assert.That(lifecycle.CanSimulate("player"), Is.False);
            Assert.That(lifecycle.CanSimulate("rival-a"), Is.True);
            Assert.That(lifecycle.CanSimulate("rival-b"), Is.True);

            lifecycle.Advance(12f);
            RacerFinishRecord rival = lifecycle.RecordFinish("rival-a", 102.625f);
            Assert.That(rival.FinishTime, Is.EqualTo(102.625f));
            Assert.That(rival.Position, Is.EqualTo(2));
            Assert.That(lifecycle.PlayerFinishTime, Is.EqualTo(90.875f));
            Assert.That(lifecycle.PlayerPresentationTime, Is.EqualTo(90.875f));

            lifecycle.Advance(47.9f);
            Assert.That(lifecycle.Records.Count, Is.EqualTo(2));
            lifecycle.Advance(.2f);
            RacerFinishRecord dnf = lifecycle.Records.Single(value => value.RacerId == "rival-b");
            Assert.That(dnf.Status, Is.EqualTo(RacerResultStatus.DidNotFinish));
            Assert.That(dnf.HasFinishTime, Is.False);
            Assert.That(lifecycle.Records.Single(value => value.RacerId == "rival-a").Status, Is.EqualTo(RacerResultStatus.Finished));
            Assert.That(lifecycle.Records.Single(value => value.RacerId == "player").FinishTime, Is.EqualTo(90.875f));
        }

        [Test]
        public void RestartClearsRecordsAndPauseDoesNotAdvanceRaceOrTimeout()
        {
            var lifecycle = new RaceFinishLifecycle(new[]
            {
                new RacerIdentity("player", "YOU", true),
                new RacerIdentity("rival", "KIRA", false)
            }, 60f);
            lifecycle.Advance(30f);
            lifecycle.RecordFinish("player", 30f);
            lifecycle.Advance(45f, true);
            Assert.That(lifecycle.SimulationTime, Is.EqualTo(30f));
            Assert.That(lifecycle.CanSimulate("rival"), Is.True);
            lifecycle.Reset();
            Assert.That(lifecycle.SimulationTime, Is.Zero);
            Assert.That(lifecycle.PlayerFinished, Is.False);
            Assert.That(lifecycle.Records, Is.Empty);
            Assert.That(lifecycle.CanSimulate("player"), Is.True);
            Assert.That(lifecycle.CanSimulate("rival"), Is.True);
        }

        [Test]
        public void RecordObjectsExposeReadOnlyIdentityAndResultProperties()
        {
            foreach (var property in typeof(RacerFinishRecord).GetProperties())
                Assert.That(property.CanWrite, Is.False, property.Name);
        }
    }
}
