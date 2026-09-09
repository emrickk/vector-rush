using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class RaceAudioTests
    {
        [Test]
        public void MotorLoadTracksThrottleAndBoostWhileAirflowTracksOnlySpeed()
        {
            RaceAudioMix coasting = RaceAudio.ComputeMix(0f, .7f, false);
            RaceAudioMix loaded = RaceAudio.ComputeMix(1f, .7f, false);
            RaceAudioMix boosted = RaceAudio.ComputeMix(1f, .7f, true);
            Assert.That(loaded.MotorLevel, Is.GreaterThan(coasting.MotorLevel));
            Assert.That(boosted.MotorLevel, Is.GreaterThan(loaded.MotorLevel));
            Assert.That(loaded.WindLevel, Is.EqualTo(coasting.WindLevel));
            Assert.That(boosted.WindLevel, Is.EqualTo(loaded.WindLevel));
            Assert.That(boosted.BoostLevel, Is.GreaterThan(loaded.BoostLevel));
        }

        [Test]
        public void RivalVoiceBudgetChoosesOnlyNearestFiniteCandidates()
        {
            int[] selected = RaceAudio.SelectRivalVoices(new[] { 100f, 4f, float.NaN, 25f, 9f, 1f }, 3);
            Assert.That(selected, Is.EqualTo(new[] { 5, 1, 4 }));
        }

        [Test]
        public void GalleryAndExteriorHaveDifferentAcousticResponse()
        {
            Assert.That(RaceAudio.AcousticWetness(.37f), Is.GreaterThan(RaceAudio.AcousticWetness(.15f)));
            Assert.That(RaceAudio.AcousticWetness(.37f), Is.InRange(0f, 1f));
            Assert.That(RaceAudio.AcousticWetness(.15f), Is.InRange(0f, 1f));
        }
    }
}
