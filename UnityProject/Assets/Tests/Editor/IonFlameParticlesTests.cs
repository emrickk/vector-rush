using NUnit.Framework;
using UnityEngine;
namespace VectorRush.Tests
{
    public sealed class IonFlameParticlesTests
    {
        GameObject root,anchor;
        IonFlameParticles effect;
        [SetUp]public void Setup()
        {
            root=new GameObject("particle lifecycle test");anchor=new GameObject("nozzle");
            effect=root.AddComponent<IonFlameParticles>();effect.Initialize(anchor.transform,.5f,true);
        }
        [TearDown]public void Cleanup(){Object.DestroyImmediate(root);Object.DestroyImmediate(anchor);}
        [Test]public void EmitsParticlesAndResetRemovesPreviousWorldSpaceWake()
        {
            effect.Step(.03f,1,1,1,Vector3.forward*60);
            Assert.That(effect.LiveParticles,Is.GreaterThan(0));
            effect.ResetTrail();Assert.That(effect.LiveParticles,Is.Zero);
            effect.Step(.03f,0,0,0,Vector3.forward*60);Assert.That(effect.LiveParticles,Is.Zero);
        }
        [Test]public void PauseFreezesEmissionAndResumeContinues()
        {
            effect.Step(.03f,1,1,0,Vector3.forward*60);int count=effect.LiveParticles;
            effect.SetPaused(true);effect.Step(1,1,1,1,Vector3.forward*60);
            Assert.That(effect.Paused,Is.True);Assert.That(effect.LiveParticles,Is.EqualTo(count));
            effect.SetPaused(false);effect.Step(.03f,1,1,0,Vector3.forward*60);
            Assert.That(effect.LiveParticles,Is.GreaterThan(count));
        }
        [Test]public void ParticleEmissionDoesNotConsumeGameplayRandomStream()
        {
            Random.InitState(851);var state=Random.state;float expected=Random.value;Random.state=state;
            effect.Step(.03f,1,1,1,Vector3.forward*60);Assert.That(Random.value,Is.EqualTo(expected));
        }
        [Test] public void TeleportClearsOldWakeEvenWhenThrottleIsReleased()
        {
            effect.Step(.03f,1,1,0,Vector3.forward*60);
            anchor.transform.position=Vector3.forward*100;
            effect.Step(.03f,0,0,0,Vector3.zero);
            Assert.That(effect.LiveParticles,Is.Zero);
        }
        [Test] public void ReleaseAllowsExistingParticlesToExpire()
        {
            effect.Step(.03f,1,1,0,Vector3.forward*60);
            effect.Step(.03f,0,0,0,Vector3.forward*60);
            foreach(var system in root.GetComponentsInChildren<ParticleSystem>()) system.Simulate(1,false,false);
            Assert.That(effect.LiveParticles,Is.Zero);
        }
    }
}
