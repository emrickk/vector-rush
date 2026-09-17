using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class AIPursuitTests
    {
        // A target 20 m along the chord of a 100 m circle lies 5.73917 degrees
        // off the current heading. Required angular velocity is speed / radius.
        [TestCase(40f, .4f)]
        [TestCase(60f, .6f)]
        public void SameRadiusRequestsTheRequiredYawAtDifferentSpeeds(float speed,float requiredYaw)
        {
            float steering=HoverVehicle.PursuitSteering(speed,5.73917f,20f);
            Assert.That(steering*1.12f,Is.EqualTo(requiredYaw).Within(.0001f));
        }

        [Test] public void LeftHandCircleRequestsNegativeYaw()
        {
            Assert.That(HoverVehicle.PursuitSteering(40f,-5.73917f,20f)*1.12f,Is.EqualTo(-.4f).Within(.0001f));
        }

        [Test] public void SlowTurnAccountsForReducedSteeringAuthority()
        {
            float authority=1.12f*Mathf.Lerp(.25f,1f,10f/26f);
            Assert.That(HoverVehicle.PursuitSteering(10f,5.73917f,20f)*authority,Is.EqualTo(.1f).Within(.0001f));
        }

        [Test] public void SharpCorrectionStaysWithinSteeringRange()
        {
            Assert.That(HoverVehicle.PursuitSteering(75f,75f,5f),Is.InRange(-1f,1f));
            Assert.That(HoverVehicle.PursuitSteering(75f,-75f,5f),Is.InRange(-1f,1f));
        }
    }
}
