using System;
using System.IO;
using NUnit.Framework;
#if ENABLE_INPUT_SYSTEM
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace VectorRush.Tests
{
    public sealed class Stage2EvidenceTests
    {
        [Test] public void NormalLaunchKeepsExistingPersistence()
        {Assert.That(Stage2EvidenceProfile.Resolve(new[]{"Vector Rush"}),Is.Null);}

        [Test] public void EvidenceDefaultsToSeparateOutputProfile()
        {
            string output=Path.Combine(Path.GetTempPath(),"stage2-evidence-test");
            Assert.That(Stage2EvidenceProfile.Resolve(new[]{"app","-stage2Evidence",output}),
                Is.EqualTo(Path.Combine(Path.GetFullPath(output),"profile")));
        }

        [Test] public void ExplicitProfileSupportsIsolatedRelaunch()
        {
            string profile=Path.Combine(Path.GetTempPath(),"stage2-persisted-test");
            Assert.That(Stage2EvidenceProfile.Resolve(new[]{"app","-stage2Profile",profile}),
                Is.EqualTo(Path.GetFullPath(profile)));
        }

        [TestCase("-stage2Profile")] [TestCase("-stage2Evidence")]
        public void InvalidProfileCannotFallBackIntoRealUserStorage(string flag)
        {
            Assert.Throws<ArgumentException>(()=>Stage2EvidenceProfile.Resolve(new[]{"app",flag}));
            Assert.Throws<ArgumentException>(()=>Stage2EvidenceProfile.Resolve(new[]{"app",flag,"relative"}));
        }

        [Test] public void DiagnosticCommandsReleaseBoostBeforeRechargeAndReboost()
        {
            var cruise=Stage2Evidence.CommandAt(1);
            var accelerate=Stage2Evidence.CommandAt(4);
            var boost=Stage2Evidence.CommandAt(8);
            var release=Stage2Evidence.CommandAt(12);
            var recharge=Stage2Evidence.CommandAt(14);
            var reboost=Stage2Evidence.CommandAt(17);
            Assert.That(cruise.throttle,Is.EqualTo(.3f));
            Assert.That(accelerate.throttle,Is.EqualTo(1));
            Assert.That(accelerate.boost,Is.False);
            Assert.That(boost.boost,Is.True);
            Assert.That(release.throttle,Is.Zero);
            Assert.That(release.boost,Is.False);
            Assert.That(recharge.boost,Is.False);
            Assert.That(reboost.boost,Is.True);
        }

#if ENABLE_INPUT_SYSTEM
        [Test] public void DiagnosticScopeRetainsTriggerThroughFocusHandlersAndRestoresSettings()
        {
            var settings=InputSystem.settings;
            var previousBackground=settings.backgroundBehavior;
            var previousEditor=settings.editorInputBehaviorInPlayMode;
            var previousUpdate=settings.updateMode;
            bool previousRun=Application.runInBackground;
            var previousGamepad=Gamepad.current;
            object manager=typeof(InputSystem).GetField("s_Manager",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            var overrideUpdates=manager.GetType().GetProperty("runPlayerUpdatesInEditMode");
            bool previousOverride=(bool)overrideUpdates.GetValue(manager);
            var lost=manager.GetType().GetMethod("UpdateDeviceStateOnFocusLost",BindingFlags.Instance|BindingFlags.NonPublic);
            var gained=manager.GetType().GetMethod("UpdateDeviceStateOnFocusGained",BindingFlags.Instance|BindingFlags.NonPublic);
            Gamepad pad=null;
            try
            {
                Assert.That(lost,Is.Not.Null,"Pinned InputSystem focus-loss handler must exist.");
                Assert.That(gained,Is.Not.Null,"Pinned InputSystem focus-gain handler must exist.");
                settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDevicesRespectGameViewFocus;
                settings.backgroundBehavior=InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices;
                settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
                overrideUpdates.SetValue(manager,true);
                pad=InputSystem.AddDevice<Gamepad>();
                Assert.That(pad.enabled,Is.True);
                // EditMode does not reproduce the Standalone device-disable policy.
                // Exercise both package handlers within the diagnostic scope; the
                // native focus/device telemetry verifies the actual background run.
                using(new Stage2Evidence.DiagnosticInputBackgroundScope())
                {
                    Assert.That(settings.backgroundBehavior,Is.EqualTo(InputSettings.BackgroundBehavior.IgnoreFocus));
                    Assert.That(Application.runInBackground,Is.True);
                    foreach(var handler in new[]{lost,gained})
                    {
                        handler.Invoke(manager,new object[]{pad,true});
                        Assert.That(pad.enabled,Is.True,"Diagnostic device must remain enabled through focus handlers.");
                        float trigger=handler==lost?.3f:.7f;
                        InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=trigger});
                        typeof(InputSystem).GetMethod("Update",BindingFlags.Static|BindingFlags.NonPublic,null,
                            new[]{typeof(InputUpdateType)},null).Invoke(null,new object[]{InputUpdateType.Dynamic});
                        Assert.That(pad.rightTrigger.ReadValue(),Is.EqualTo(trigger).Within(.001f));
                    }
                }
                Assert.That(settings.backgroundBehavior,Is.EqualTo(InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices));
                Assert.That(Application.runInBackground,Is.EqualTo(previousRun));
            }
            finally
            {
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);
                if(previousGamepad!=null&&previousGamepad.added)previousGamepad.MakeCurrent();
                settings.backgroundBehavior=previousBackground;
                settings.editorInputBehaviorInPlayMode=previousEditor;
                settings.updateMode=previousUpdate;
                overrideUpdates.SetValue(manager,previousOverride);
                Application.runInBackground=previousRun;
            }
        }

        [Test] public void DiagnosticInputScopeRestoresSettingsWhenCaptureThrows()
        {
            var previous=InputSystem.settings.backgroundBehavior;
            bool run=Application.runInBackground;
            Assert.Throws<InvalidOperationException>(()=>{
                using(new Stage2Evidence.DiagnosticInputBackgroundScope())
                {
                    Assert.That(InputSystem.settings.backgroundBehavior,Is.EqualTo(InputSettings.BackgroundBehavior.IgnoreFocus));
                    Assert.That(Application.runInBackground,Is.True);
                    throw new InvalidOperationException("Synthetic capture failure.");
                }
            });
            Assert.That(InputSystem.settings.backgroundBehavior,Is.EqualTo(previous));
            Assert.That(Application.runInBackground,Is.EqualTo(run));
        }
#endif
    }
}
