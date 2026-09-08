using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace VectorRush
{
    /// <summary>Opt-in native trigger/exhaust verification through a virtual gamepad and ordinary physics.</summary>
    public sealed class ThrottleEvidence : MonoBehaviour
    {
        VectorBootstrap bootstrap;
        HoverVehicle player;
        IonPropulsion propulsion;
        string folder;
        float requestedThrottle;
        bool requestedBoost, originalAutopilot;
        int originalCaptureRate;
        readonly Report report=new Report();
#if ENABLE_INPUT_SYSTEM
        Gamepad gamepad, originalGamepad;
#endif

        public static bool TryStart(VectorBootstrap owner)
        {
            var args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-verifyThrottle");
            if(index<0) return false;
            if(index+1>=args.Length || !Path.IsPathRooted(args[index+1]))
            { Debug.LogError("Throttle evidence requires -verifyThrottle <absolute output folder>."); return false; }
#if ENABLE_INPUT_SYSTEM
            if(!owner || !owner.Director || !owner.Director.Player || !owner.Director.Player.GetComponent<IonPropulsion>())
            { Debug.LogError("Throttle evidence requires an initialized player and IonPropulsion."); return false; }
            foreach(var evidence in owner.GetComponents<RaceEvidence>()) { evidence.StopAllCoroutines(); evidence.enabled=false; }
            var runner=owner.gameObject.AddComponent<ThrottleEvidence>(); runner.bootstrap=owner;
            runner.player=owner.Director.Player; runner.propulsion=runner.player.GetComponent<IonPropulsion>();
            runner.folder=Path.GetFullPath(args[index+1]); runner.StartCoroutine(runner.Run()); return true;
#else
            Debug.LogError("Throttle evidence requires the Input System; no verification was run."); return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        IEnumerator Run()
        {
            Directory.CreateDirectory(folder);
            originalAutopilot=player.AutopilotForTesting; originalCaptureRate=Time.captureFramerate;
            report.startedUtc=DateTime.UtcNow.ToString("o");
            report.scope="Actual native Unity screenshots with ordinary race camera/HUD and vehicle physics. A temporary Input System gamepad supplies scripted trigger/boost input and track-following stick input. Player testing autopilot is disabled. No speed, Rigidbody pose, engine demand or VFX response is assigned by this harness. Time.captureFramerate=0; this is an input/rendering verification, not a performance measurement. Samples are taken at end of frame. Release must reach ExhaustResponse <=0.02 within 0.25 simulation seconds of the first observed zero throttle while still above 80 km/h.";
            File.WriteAllText(Path.Combine(folder,"throttle-status.txt"),"RUNNING\n");
            try
            {
                Time.captureFramerate=0; player.AutopilotForTesting=false;
                originalGamepad=Gamepad.current; gamepad=InputSystem.AddDevice<Gamepad>(); gamepad.MakeCurrent();
                InputSystem.onBeforeUpdate+=QueueInput;
                yield return new WaitForSecondsRealtime(2.5f);
                bootstrap.Director.StartRace();
                float deadline=Time.realtimeSinceStartup+8f;
                while(bootstrap.Director.Phase!=RacePhase.Racing && Time.realtimeSinceStartup<deadline) yield return null;
                if(bootstrap.Director.Phase!=RacePhase.Racing) Fail("Native race failed to enter Racing.");
                var stages=new[] {
                    new Stage("01-off",0f,false,.55f), new Stage("02-quarter",.25f,false,2.4f),
                    new Stage("03-full",1f,false,2.3f), new Stage("04-release-coasting",0f,false,.65f),
                    new Stage("05-boost",1f,true,1.25f), new Stage("06-off-after-boost",0f,false,.55f)
                };
                foreach(var stage in stages)
                {
                    if(report.error!=null) break;
                    if(stage.name=="04-release-coasting" && player.SpeedKph<80f)
                    { Fail("Full throttle did not reach 80 km/h through normal physics before the release test."); break; }
                    yield return Hold(stage);
                }
                report.passed=report.error==null && report.releaseSettledSeconds>=0f && report.releaseSettledSeconds<=.25f && report.releaseSpeedKph>=80f;
                if(!report.passed && report.error==null) Fail("Release did not settle below 0.02 within 0.25 s while coasting above 80 km/h.");
            }
            finally
            {
                Cleanup(); report.finishedUtc=DateTime.UtcNow.ToString("o");
                File.WriteAllText(Path.Combine(folder,"throttle-evidence.json"),JsonUtility.ToJson(report,true));
                File.WriteAllText(Path.Combine(folder,"throttle-status.txt"),report.passed?"PASS — actual gamepad input and native exhaust response verified.\n":"FAIL / INCOMPLETE — "+report.error+"\n");
            }
            yield return new WaitForSecondsRealtime(.5f);
            Application.Quit(report.passed?0:1);
        }

        IEnumerator Hold(Stage stage)
        {
            requestedThrottle=stage.throttle; requestedBoost=stage.boost;
            float deadline=Time.realtimeSinceStartup+2f;
            // Let the ordinary Input System update consume the queued device state.
            do { yield return null; }
            while((Mathf.Abs(player.ThrottleInput-stage.throttle)>.02f || (stage.boost && !player.IsBoosting)) && Time.realtimeSinceStartup<deadline);
            if(Mathf.Abs(player.ThrottleInput-stage.throttle)>.02f || (stage.boost && !player.IsBoosting))
            { Fail("Requested native input was not observed for "+stage.name+"."); yield break; }
            float started=Time.time; Sample latest=null;
            do
            {
                yield return new WaitForEndOfFrame();
                latest=new Sample {
                    stage=stage.name, elapsed=Time.time-started, raceTime=bootstrap.Director.RaceTime,
                    requestedThrottle=stage.throttle, throttleInput=player.ThrottleInput,
                    exhaustDemand=propulsion.ExhaustDemand, exhaustResponse=propulsion.ExhaustResponse,
                    speedKph=player.SpeedKph, boost=player.IsBoosting, energy01=player.Boost01,
                    grounded=player.IsGrounded, normalizedProgress=player.TrackProgress
                };
                report.samples.Add(latest);
                if(stage.name=="04-release-coasting" && report.releaseSettledSeconds<0f && latest.throttleInput<=.01f && latest.exhaustDemand<=.01f && latest.exhaustResponse<=.02f)
                { report.releaseSettledSeconds=latest.elapsed; report.releaseSpeedKph=latest.speedKph; }
            } while(Time.time-started<stage.duration);
            float expected=stage.throttle*(stage.boost?1.35f:1f);
            if(Mathf.Abs(latest.throttleInput-stage.throttle)>.02f || Mathf.Abs(latest.exhaustDemand-expected)>.03f || Mathf.Abs(latest.exhaustResponse-expected)>.06f)
                Fail("Settled input/demand/response mismatch for "+stage.name+".");
            string filename=stage.name+".png",path=Path.Combine(folder,filename); DateTime requested=DateTime.UtcNow;
            ScreenCapture.CaptureScreenshot(path);
            deadline=Time.realtimeSinceStartup+8f;
            do { yield return null; } while((!File.Exists(path) || File.GetLastWriteTimeUtc(path)<requested) && Time.realtimeSinceStartup<deadline);
            if(!File.Exists(path) || File.GetLastWriteTimeUtc(path)<requested) Fail("Screenshot was not written: "+filename);
            report.views.Add(new View { file=filename,sample=latest,width=Screen.width,height=Screen.height });
            File.WriteAllText(Path.Combine(folder,"throttle-evidence.json"),JsonUtility.ToJson(report,true));
        }

        void QueueInput()
        {
            if(gamepad==null || !gamepad.added || !player || !bootstrap) return;
            float steer=0f;
            if(bootstrap.Director.Phase==RacePhase.Racing)
            {
                var track=bootstrap.Track; var frame=track.Evaluate(player.TrackProgress);
                var target=track.Evaluate(player.TrackProgress+(12f+player.Body.linearVelocity.magnitude*.33f)/track.Length);
                Vector3 direction=Vector3.ProjectOnPlane(target.Position-player.Body.position,frame.Up).normalized;
                steer=Mathf.Clamp(Vector3.SignedAngle(player.transform.forward,direction,frame.Up)/26f,-1f,1f);
            }
            float rawStick=Mathf.Abs(steer)<.001f?0f:Mathf.Sign(steer)*Mathf.Lerp(InputSystem.settings.defaultDeadzoneMin,InputSystem.settings.defaultDeadzoneMax,Mathf.Abs(steer));
            var state=new GamepadState { rightTrigger=requestedThrottle,leftStick=new Vector2(rawStick,0f) };
            if(requestedBoost) state=state.WithButton(GamepadButton.South);
            gamepad.MakeCurrent(); InputSystem.QueueStateEvent(gamepad,state);
        }

        void Cleanup()
        {
            InputSystem.onBeforeUpdate-=QueueInput;
            if(gamepad!=null && gamepad.added) InputSystem.RemoveDevice(gamepad);
            gamepad=null; if(originalGamepad!=null && originalGamepad.added) originalGamepad.MakeCurrent();
            if(player) player.AutopilotForTesting=originalAutopilot;
            Time.captureFramerate=originalCaptureRate;
        }
        void OnDestroy() { Cleanup(); }
#endif
        void Fail(string reason) { if(report.error==null) report.error=reason; Debug.LogError("Throttle evidence: "+reason); }
        sealed class Stage
        {
            public readonly string name; public readonly float throttle,duration; public readonly bool boost;
            public Stage(string name,float throttle,bool boost,float duration) { this.name=name;this.throttle=throttle;this.boost=boost;this.duration=duration; }
        }
        [Serializable] sealed class Report
        {
            public string scope,startedUtc,finishedUtc,error; public bool passed;
            public float releaseSettledSeconds=-1f,releaseSpeedKph;
            public List<Sample> samples=new List<Sample>(); public List<View> views=new List<View>();
        }
        [Serializable] sealed class Sample
        {
            public string stage;
            public float elapsed,raceTime,requestedThrottle,throttleInput,exhaustDemand,exhaustResponse,speedKph,energy01,normalizedProgress;
            public bool boost,grounded;
        }
        [Serializable] sealed class View { public string file; public Sample sample; public int width,height; }
    }
}
