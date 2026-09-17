using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class MotionVFXContractTests
    {
        [Test]
        public void CameraFovIsBoundedAndBoostAddsAControlledStep()
        {
            float cruise = ChaseCameraMotion.TargetFov(1f, 0f, false);
            float boost = ChaseCameraMotion.TargetFov(1f, 1f, false);
            Assert.That(cruise, Is.EqualTo(77.5f).Within(.001f));
            Assert.That(boost, Is.EqualTo(83f).Within(.001f));
            Assert.That(ChaseCameraMotion.TargetFov(5f, 5f, false), Is.EqualTo(boost).Within(.001f));
        }

        [Test]
        public void TurnLookAheadContractsWithoutChangingRaceSpeed()
        {
            float straight = ChaseCameraMotion.FocusLead(.8f, 0f, false);
            float corner = ChaseCameraMotion.FocusLead(.8f, 1f, false);
            Assert.That(corner, Is.LessThan(straight));
            Assert.That(ChaseCameraMotion.FollowSmoothTime(.8f, 1f), Is.GreaterThan(
                ChaseCameraMotion.FollowSmoothTime(.8f, 0f)));
        }

        [Test]
        public void ReducedMotionDisablesOptionalMotionBlur()
        {
            Assert.That(SpeedMotionBlurModel.TargetIntensity(1f, 1f, true), Is.Zero);
            Assert.That(SpeedMotionBlurModel.TargetIntensity(.2f, 0f, false), Is.Zero);
            Assert.That(SpeedMotionBlurModel.TargetIntensity(1f, 1f, false), Is.InRange(.25f, .32f));
        }

        [Test]
        public void PresentationBlurEngagesAtOrdinaryCruiseAndHonorsReducedMotion()
        {
            float cruise = ChaseCameraMotion.Speed01(175f);
            Assert.That(SpeedMotionBlurModel.PresentationIntensity(cruise, 0, false), Is.GreaterThan(.15f));
            Assert.That(SpeedMotionBlurModel.PresentationIntensity(cruise, 0, false), Is.LessThan(.3f));
            Assert.That(SpeedMotionBlurModel.PresentationIntensity(0, 0, false), Is.Zero);
            Assert.That(SpeedMotionBlurModel.PresentationIntensity(1, 1, true), Is.Zero);
            Assert.That(SpeedMotionBlurModel.PresentationIntensity(3, 3, false), Is.EqualTo(.4f).Within(.001f));
        }

        [Test]
        public void LookBackTransitionKeepsClearOfTheShipAndAvoidsVerticalAim()
        {
            for (int i = 0; i <= 100; i++)
            {
                Vector3 offset = ChaseCamera.ViewOffset(i / 100f, 10f, 3.25f);
                Assert.That(offset.x, Is.InRange(-.001f, 6.001f));
                Assert.That(new Vector2(offset.x, offset.z).magnitude, Is.GreaterThanOrEqualTo(5.99f));
                Assert.That(offset.y, Is.InRange(3.249f, 5.251f));
            }
            Assert.That(ChaseCamera.ViewOffset(0, 10, 3.25f).z, Is.EqualTo(-10));
            Assert.That(ChaseCamera.ViewOffset(1, 10, 3.25f).z, Is.EqualTo(10));
        }

        [Test]
        public void ContactModelSeparatesImpactFromEstablishedScrape()
        {
            VehicleContactProfile impact = VehicleContactMath.Evaluate(22f, 4f, 1f, 0f);
            VehicleContactProfile scrape = VehicleContactMath.Evaluate(2f, 0f, 25f, .3f);
            Assert.That(impact.NormalIntensity01, Is.GreaterThan(.8f));
            Assert.That(impact.IsScraping, Is.False);
            Assert.That(scrape.TangentialIntensity01, Is.GreaterThan(.75f));
            Assert.That(scrape.IsScraping, Is.True);
        }

        [Test]
        public void ContactKeysAreStableAndColliderSpecific()
        {
            GameObject vehicle = new GameObject("contact key vehicle");
            GameObject firstCollider = new GameObject("contact key collider A");
            GameObject secondCollider = new GameObject("contact key collider B");
            try
            {
                var first = new VehicleContactKey(vehicle.GetEntityId(), firstCollider.GetEntityId());
                Assert.That(new VehicleContactKey(vehicle.GetEntityId(), firstCollider.GetEntityId()), Is.EqualTo(first));
                Assert.That(new VehicleContactKey(vehicle.GetEntityId(), secondCollider.GetEntityId()), Is.Not.EqualTo(first));
                Assert.That(new VehicleContactKey(secondCollider.GetEntityId(), firstCollider.GetEntityId()), Is.Not.EqualTo(first));
            }
            finally
            {
                Object.DestroyImmediate(secondCollider);
                Object.DestroyImmediate(firstCollider);
                Object.DestroyImmediate(vehicle);
            }
        }

        [Test]
        public void IntegrationBridgePreservesContactPhaseAndStableIdentity()
        {
            GameObject vehicle = new GameObject("bridge vehicle");
            GameObject collider = new GameObject("bridge collider");
            try
            {
                var key = new VehicleContactKey(vehicle.GetEntityId(), collider.GetEntityId());
                Assert.That(VehicleContactAudioBridge.MapPhase(VehicleContactPhase.Enter), Is.EqualTo(ContactSignalPhase.Enter));
                Assert.That(VehicleContactAudioBridge.MapPhase(VehicleContactPhase.Stay), Is.EqualTo(ContactSignalPhase.Stay));
                Assert.That(VehicleContactAudioBridge.MapPhase(VehicleContactPhase.Exit), Is.EqualTo(ContactSignalPhase.Exit));
                Assert.That(VehicleContactAudioBridge.ContactKey(key), Is.EqualTo(
                    vehicle.GetEntityId().ToString() + ":" + collider.GetEntityId().ToString()));
            }
            finally
            {
                Object.DestroyImmediate(collider);
                Object.DestroyImmediate(vehicle);
            }
        }
    }
}
