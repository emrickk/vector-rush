using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class OpeningRoadLightResponseTests
    {
        [Test] public void OrdinaryLaunchEnablesCandidate()
            => Assert.That(OpeningRoadLightResponse.IsEnabled(new[] { "Vector Rush" }), Is.True);

        [Test] public void DedicatedBaselineSwitchDisablesOnlyRoadCandidate()
        {
            Assert.That(OpeningRoadLightResponse.IsEnabled(new[] { "Vector Rush", "-vrRoadLightBaseline" }), Is.False);
            Assert.That(OpeningRoadLightResponse.IsEnabled(new[] { "Vector Rush", "-vrBlueBaseline" }), Is.True);
        }

        [TestCase(56, true)] [TestCase(57, true)] [TestCase(0, true)] [TestCase(18, true)]
        [TestCase(19, false)] [TestCase(55, false)]
        public void PassageBoundsAreExplicit(int index,bool expected)
            => Assert.That(OpeningRoadLightResponse.ContainsFixture(index), Is.EqualTo(expected));
    }
}
