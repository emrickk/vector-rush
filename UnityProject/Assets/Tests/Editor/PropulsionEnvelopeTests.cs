using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class PropulsionEnvelopeTests
    {
        [Test]
        public void ReleasingThrottleExtinguishesExhaustWithinQuarterSecond()
        {
            var state = new PropulsionEnvelope();
            Advance(state, 1f, 1f, true);
            Assert.That(state.Response, Is.GreaterThan(1.3f));
            Advance(state, .25f, 0f, false);
            Assert.That(state.Response, Is.LessThan(.02f));
            Assert.That(state.Ignition, Is.Zero);
        }

        [Test]
        public void ActualBoostHasIgnitionSustainAndReleaseWithoutInventingThrottle()
        {
            var state = new PropulsionEnvelope();
            state.Step(.02f, 1f, true, true, false);
            Assert.That(state.State, Is.EqualTo(PropulsionPhase.BoostIgnition));
            Assert.That(state.Ignition, Is.EqualTo(1f));
            Advance(state, .4f, 1f, true);
            Assert.That(state.State, Is.EqualTo(PropulsionPhase.BoostSustain));
            state.Step(.02f, 1f, false, true, false);
            Assert.That(state.State, Is.EqualTo(PropulsionPhase.BoostRelease));
            Assert.That(state.Ignition, Is.Zero);
            Assert.That(state.Release, Is.EqualTo(1f));
            Advance(state, .5f, 1f, false);
            Assert.That(state.Boost, Is.Zero);
            Assert.That(state.Release, Is.Zero);
            Assert.That(state.State, Is.EqualTo(PropulsionPhase.Thrust));
        }

        [Test]
        public void PauseFreezesFlowAndEveryEnvelopeThenResumeContinues()
        {
            var state = new PropulsionEnvelope();
            state.Step(.04f, 1f, true, true, false);
            float response = state.Response, boost = state.Boost, ignition = state.Ignition, clock = state.FlowTime;
            state.Step(5f, 0f, false, false, true);
            Assert.That(state.Response, Is.EqualTo(response));
            Assert.That(state.Boost, Is.EqualTo(boost));
            Assert.That(state.Ignition, Is.EqualTo(ignition));
            Assert.That(state.FlowTime, Is.EqualTo(clock));
            state.Step(.04f, 1f, true, true, false);
            Assert.That(state.Response, Is.GreaterThan(response));
            Assert.That(state.Ignition, Is.LessThan(ignition));
        }

        [Test]
        public void InactiveCountdownAndFinishedCraftCannotProduceThrust()
        {
            var state = new PropulsionEnvelope();
            state.Step(.1f, 1f, true, false, false);
            Assert.That(state.Response, Is.Zero);
            Assert.That(state.Boost, Is.Zero);
            Assert.That(state.Ignition, Is.Zero);
            Advance(state, .5f, 1f, true);
            for (int i = 0; i < 30; i++) state.Step(.01f, 1f, true, false, false);
            Assert.That(state.Response, Is.Zero);
            Assert.That(state.Ignition, Is.Zero);
        }

        [Test]
        public void RestartClearsBoostSoNextIgnitionIsFresh()
        {
            var state = new PropulsionEnvelope();
            Advance(state, .5f, 1f, true);
            state.Reset();
            Assert.That(state.Response, Is.Zero);
            Assert.That(state.Boost, Is.Zero);
            Assert.That(state.Release, Is.Zero);
            Assert.That(state.FlowTime, Is.Zero);
            Assert.That(state.State, Is.EqualTo(PropulsionPhase.Off));
            state.Step(.02f, 1f, true, true, false);
            Assert.That(state.Ignition, Is.EqualTo(1f));
        }

        [Test]
        public void EmptyOrRejectedBoostDoesNotStartIgnition()
        {
            var state = new PropulsionEnvelope();
            Advance(state, .5f, 1f, false);
            Assert.That(state.Boost, Is.Zero);
            Assert.That(state.Ignition, Is.Zero);
            state.Reset();
            state.Step(.1f, 0f, true, true, false);
            Assert.That(state.Response, Is.Zero);
            Assert.That(state.Ignition, Is.Zero);
        }

        [TestCase(new string[0], true)]
        [TestCase(new[] { "-vrPropulsion", "off" }, false)]
        [TestCase(new[] { "-vrPropulsion", "on" }, true)]
        public void CandidateIsDefaultAndLegacyRequiresExplicitFlag(string[] args, bool enabled)
        {
            Assert.That(IonPropulsion.ReadStage2Enabled(args), Is.EqualTo(enabled));
        }

        static void Advance(PropulsionEnvelope state, float duration, float throttle, bool boost)
        {
            int frames = UnityEngine.Mathf.RoundToInt(duration / .01f);
            for (int i = 0; i < frames; i++) state.Step(.01f, throttle, boost, true, false);
        }
    }
}
