using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class RaceTelemetryTests
    {
        [Test]
        public void SectorTrafficAndAssistanceRemainAvailableForLaterHumanReview()
        {
            var telemetry = new RaceTelemetry();
            telemetry.RecordSector(1, 3, 18.25f);
            telemetry.RecordTraffic(20f, -14f, 2, 1);
            telemetry.AccumulateAssistance(.5f, 2, 1);
            Assert.That(telemetry.Sectors, Has.Count.EqualTo(1));
            Assert.That(telemetry.Sectors[0].Lap, Is.EqualTo(1));
            Assert.That(telemetry.Sectors[0].Sector, Is.EqualTo(3));
            Assert.That(telemetry.Traffic, Has.Count.EqualTo(1));
            Assert.That(telemetry.Traffic[0].NearestSignedGapMeters, Is.EqualTo(-14f));
            Assert.That(telemetry.RivalCorridorGuardSeconds, Is.EqualTo(1f));
            Assert.That(telemetry.PlayerRecoveryCount, Is.EqualTo(1));
            telemetry.Reset();
            Assert.That(telemetry.Sectors, Is.Empty);
            Assert.That(telemetry.Traffic, Is.Empty);
            Assert.That(telemetry.RivalCorridorGuardSeconds, Is.Zero);
        }
    }
}
