using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class RivalCorridorGuardTests
    {
        [TestCase(-1f)] [TestCase(1f)]
        public void WallContactGetsBoundedInwardForceWhenCorrectionPathIsClear(float side)
        {
            float acceleration=HoverVehicle.RivalCorridorAcceleration(false,true,9f*side,4.6f*side,0f);
            Assert.That(acceleration*side,Is.LessThan(0f).And.GreaterThanOrEqualTo(-6f));
        }

        [Test] public void BlockedInwardTargetDoesNotApplySideForce()
        {
            Assert.That(HoverVehicle.RivalCorridorAcceleration(false,true,8.9f,8.9f,2f),Is.Zero);
        }

        [Test] public void PlayerAndInactiveGuardNeverReceiveAssistance()
        {
            Assert.That(HoverVehicle.RivalCorridorAcceleration(true,true,9f,4.6f,0f),Is.Zero);
            Assert.That(HoverVehicle.RivalCorridorAcceleration(false,false,9f,4.6f,0f),Is.Zero);
        }

        [Test] public void OutwardTargetCannotActivateRecoveryForce()
        {
            Assert.That(HoverVehicle.RivalCorridorAcceleration(false,true,5f,7f,0f),Is.Zero);
        }

        [Test] public void PlayerIsExcludedEvenWhenGuardWasActive()
        {
            bool active=true; float lane=5.8f,speed=96f;
            HoverVehicle.ApplyRivalCorridorGuard(true,8f,12f,22f,ref active,ref lane,ref speed);
            Assert.That(active,Is.False);
            Assert.That(lane,Is.EqualTo(5.8f));
            Assert.That(speed,Is.EqualTo(96f));
        }

        [TestCase(-1f)] [TestCase(1f)]
        public void LoggedWallPositionGetsSymmetricInwardTargetAndSpeedReduction(float side)
        {
            bool active=false; float lane=5.8f*side,speed=75f;
            HoverVehicle.ApplyRivalCorridorGuard(false,7.97f*side,0f,22f,ref active,ref lane,ref speed);
            Assert.That(active,Is.True);
            Assert.That(lane*side,Is.GreaterThan(0f).And.LessThan(5.8f));
            Assert.That(speed,Is.GreaterThan(0f).And.LessThan(75f));
        }

        [Test] public void OutwardDriftTriggersBeforeCurrentPositionReachesBoundary()
        {
            bool active=false; float lane=5.8f,speed=75f;
            HoverVehicle.ApplyRivalCorridorGuard(false,5.8f,2f,22f,ref active,ref lane,ref speed);
            Assert.That(active,Is.True);
            Assert.That(lane,Is.LessThan(5.8f));
        }

        [Test] public void StableOuterLaneKeepsExistingTargetAndSpeed()
        {
            bool active=false; float lane=5.8f,speed=75f;
            HoverVehicle.ApplyRivalCorridorGuard(false,5.8f,0f,22f,ref active,ref lane,ref speed);
            Assert.That(active,Is.False);
            Assert.That(lane,Is.EqualTo(5.8f));
            Assert.That(speed,Is.EqualTo(75f));
        }

        [Test] public void ActiveGuardWaitsForClearanceThenReleasesWithoutRaisingTrafficSpeed()
        {
            bool active=true; float lane=5.8f,speed=20f;
            HoverVehicle.ApplyRivalCorridorGuard(false,6f,-1f,22f,ref active,ref lane,ref speed);
            Assert.That(active,Is.True);
            Assert.That(speed,Is.EqualTo(20f));
            lane=5.8f;
            HoverVehicle.ApplyRivalCorridorGuard(false,5.4f,-1f,22f,ref active,ref lane,ref speed);
            Assert.That(active,Is.False);
            Assert.That(lane,Is.EqualTo(5.8f));
            Assert.That(speed,Is.EqualTo(20f));
        }

        [TestCase(-1f)] [TestCase(1f)]
        public void InwardCorrectionRespectsAdjacentYawedCraftClearance(float side)
        {
            float corrected=HoverVehicle.KeepCorrectionClear(7.97f*side,4.6f*side,0f,6.6f);
            Assert.That(corrected*side,Is.GreaterThanOrEqualTo(6.6f).And.LessThanOrEqualTo(7.97f));
        }

        [Test] public void AlreadyCrowdedCorrectionDoesNotPushFartherTowardWall()
        {
            float corrected=HoverVehicle.KeepCorrectionClear(7.97f,4.6f,3f,6.6f);
            Assert.That(corrected,Is.EqualTo(7.97f));
        }

        [Test] public void RivalOutsideCorrectionPathDoesNotBlockInwardTarget()
        {
            Assert.That(HoverVehicle.KeepCorrectionClear(7.97f,4.6f,10f,5.5f),Is.EqualTo(4.6f));
        }
    }
}
