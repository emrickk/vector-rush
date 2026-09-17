using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class PresentationPolishAudioTests
    {
        [Test]
        public void ImpactFeedbackSeparatesContactSeverityWithoutExceedingMixHeadroom()
        {
            ImpactFeedbackProfile ignored = RaceAudio.ImpactFeedbackForSpeed(1.9f);
            ImpactFeedbackProfile light = RaceAudio.ImpactFeedbackForSpeed(5f);
            ImpactFeedbackProfile medium = RaceAudio.ImpactFeedbackForSpeed(13f);
            ImpactFeedbackProfile heavy = RaceAudio.ImpactFeedbackForSpeed(26f);

            Assert.That(ignored.Severity, Is.EqualTo(ImpactSeverity.None));
            Assert.That(light.Severity, Is.EqualTo(ImpactSeverity.Light));
            Assert.That(medium.Severity, Is.EqualTo(ImpactSeverity.Medium));
            Assert.That(heavy.Severity, Is.EqualTo(ImpactSeverity.Heavy));
            Assert.That(light.Volume, Is.LessThan(medium.Volume));
            Assert.That(medium.Volume, Is.LessThan(heavy.Volume));
            Assert.That(heavy.Volume, Is.LessThanOrEqualTo(.52f));
            Assert.That(light.VisualStrength, Is.LessThan(medium.VisualStrength));
            Assert.That(medium.VisualStrength, Is.LessThan(heavy.VisualStrength));
        }

        [Test]
        public void BoostHasASeparateContinuousLayerWithoutChangingAirflow()
        {
            RaceAudioMix normal = RaceAudio.ComputeMix(1f, .8f, false);
            RaceAudioMix committed = RaceAudio.ComputeMix(1f, .8f, true);
            Assert.That(normal.BoostLevel, Is.Zero);
            Assert.That(committed.BoostLevel, Is.GreaterThan(.9f));
            Assert.That(committed.MotorLevel, Is.GreaterThan(normal.MotorLevel));
            Assert.That(committed.WindLevel, Is.EqualTo(normal.WindLevel));
        }

        [Test]
        public void ContactMixSeparatesTransientImpactFromSustainedScrape()
        {
            ContactAudioMix impact = RaceAudio.ContactMix(.85f, .3f, false);
            ContactAudioMix scrape = RaceAudio.ContactMix(.12f, .9f, true);

            Assert.That(impact.ImpactLevel, Is.EqualTo(.85f).Within(.001f));
            Assert.That(impact.ScrapeLevel, Is.Zero);
            Assert.That(scrape.ImpactLevel, Is.Zero);
            Assert.That(scrape.ScrapeLevel, Is.GreaterThan(.7f));
            Assert.That(impact.Ducking, Is.InRange(0f, .38f));
            Assert.That(scrape.Ducking, Is.InRange(0f, .24f));
        }

        [Test]
        public void ContactIntensitySeparatesNormalAndTangentialMotion()
        {
            RaceAudio.ContactIntensity(new UnityEngine.Vector3(0f, -20f, 2f), UnityEngine.Vector3.up, out float normal, out float tangent);
            RaceAudio.ContactIntensity(new UnityEngine.Vector3(20f, -2f, 0f), UnityEngine.Vector3.up, out float grazingNormal, out float grazingTangent);

            Assert.That(normal, Is.GreaterThan(tangent));
            Assert.That(grazingTangent, Is.GreaterThan(grazingNormal));
        }
    }
}
