using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class RacePresentationMotionTests
    {
        [Test]
        public void MenuMotionEntersAndExitsWithoutOvershoot()
        {
            MenuMotionSample start = RacePresentationMotion.Menu(0f, 0f, false, false);
            MenuMotionSample settled = RacePresentationMotion.Menu(1f, 0f, false, false);
            MenuMotionSample exiting = RacePresentationMotion.Menu(1f, RacePresentationMotion.MenuExitSeconds, true, false);

            Assert.That(start.Visibility, Is.Zero);
            Assert.That(start.OffsetX, Is.LessThan(0f));
            Assert.That(settled.Visibility, Is.EqualTo(1f));
            Assert.That(settled.OffsetX, Is.EqualTo(0f).Within(.001f));
            Assert.That(exiting.Visibility, Is.Zero);
            Assert.That(exiting.OffsetX, Is.GreaterThan(0f));
        }

        [Test]
        public void ReducedMotionUsesImmediateStablePresentation()
        {
            MenuMotionSample menu = RacePresentationMotion.Menu(0f, 0f, false, true);
            Assert.That(menu.Visibility, Is.EqualTo(1f));
            Assert.That(menu.OffsetX, Is.Zero);
            Assert.That(RacePresentationMotion.SelectionEmphasis(0f, true), Is.EqualTo(1f));
            Assert.That(RacePresentationMotion.Approach(.2f, 1f, .01f, 4f, 3f, true), Is.EqualTo(1f));
        }

        [Test]
        public void ContactInterruptsPositionAndBoostPresentation()
        {
            Assert.That(RacePresentationMotion.SelectPriority(true, true, true), Is.EqualTo(PresentationCuePriority.Contact));
            Assert.That(RacePresentationMotion.SelectPriority(false, true, true), Is.EqualTo(PresentationCuePriority.Position));
            Assert.That(RacePresentationMotion.SelectPriority(false, false, true), Is.EqualTo(PresentationCuePriority.Boost));
        }

        [Test]
        public void CueEnvelopeHasBoundedOnsetHoldAndExit()
        {
            Assert.That(RacePresentationMotion.CueEnvelope(0f, .1f, .2f, .3f, false), Is.Zero);
            Assert.That(RacePresentationMotion.CueEnvelope(.1f, .1f, .2f, .3f, false), Is.EqualTo(1f));
            Assert.That(RacePresentationMotion.CueEnvelope(.25f, .1f, .2f, .3f, false), Is.EqualTo(1f));
            Assert.That(RacePresentationMotion.CueEnvelope(.45f, .1f, .2f, .3f, false), Is.InRange(0f, 1f));
            Assert.That(RacePresentationMotion.CueEnvelope(.61f, .1f, .2f, .3f, false), Is.Zero);
        }

        [Test]
        public void PositionCueMatchesArtTimingContract()
        {
            Assert.That(RacePresentationMotion.PositionAttackSeconds, Is.EqualTo(.11f));
            Assert.That(RacePresentationMotion.PositionHoldSeconds, Is.EqualTo(.45f));
            Assert.That(RacePresentationMotion.PositionExitSeconds, Is.EqualTo(.17f));
            Assert.That(RacePresentationMotion.PositionEnvelope(.11f, false), Is.EqualTo(1f));
            Assert.That(RacePresentationMotion.PositionEnvelope(.56f, false), Is.EqualTo(1f));
            Assert.That(RacePresentationMotion.PositionEnvelope(.731f, false), Is.Zero);
        }
    }
}
