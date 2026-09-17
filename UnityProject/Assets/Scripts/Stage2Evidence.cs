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
    /// <summary>Opt-in screenshots, whole-race observation and real-input propulsion diagnostics.</summary>
    public sealed class Stage2Evidence : MonoBehaviour
    {
        public const int CaptureRate=24;
        public const float PropulsionDuration=18f;
        VectorBootstrap owner;
        RaceHUD hud;
        string folder,mode;
        bool video,hideHud,recording,originalAutopilot,originalHud,cleaned;
        int originalRate,sequence;
        float originalScale,startedWall,driverStarted=-1;
        readonly Report report=new Report();
        readonly List<float> sustainedBoostFrames=new List<float>();
#if ENABLE_INPUT_SYSTEM
        Gamepad gamepad,originalGamepad;
        DiagnosticInputBackgroundScope inputBackground;
        int queuedInputEvents;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            string[] args=Environment.GetCommandLineArgs();
            int index=Array.IndexOf(args,"-stage2Evidence");
            if(index<0)return;
            try
            {
                if(index+1>=args.Length||!Path.IsPathRooted(args[index+1]))
                    throw new ArgumentException("-stage2Evidence requires an absolute output directory.");
                string path=Path.GetFullPath(args[index+1]);
                Directory.CreateDirectory(path);
                if(File.Exists(Path.Combine(path,"stage2-evidence.json"))||Directory.Exists(Path.Combine(path,"frames")))
                    throw new IOException("Stage 2 output contains earlier evidence; choose a fresh directory.");
                var bootstrap=VectorBootstrap.Instance;
                if(!bootstrap||!bootstrap.Director||!bootstrap.Director.Player)
                    throw new InvalidOperationException("Stage 2 evidence requires the initialized game.");
                string selected="screens";int m=Array.IndexOf(args,"-stage2Mode");
                if(m>=0){if(m+1>=args.Length)throw new ArgumentException("Missing stage2Mode.");selected=args[m+1];}
                if(selected!="screens"&&selected!="loop"&&selected!="propulsion"&&selected!="performance")
                    throw new ArgumentException("-stage2Mode accepts screens, loop, propulsion or performance.");
                foreach(var old in bootstrap.GetComponents<RaceEvidence>()){old.StopAllCoroutines();old.enabled=false;}
                var e=bootstrap.gameObject.AddComponent<Stage2Evidence>();
                e.owner=bootstrap;e.hud=FindAnyObjectByType<RaceHUD>();e.folder=path;e.mode=selected;
                e.video=Array.IndexOf(args,"-stage2Video")>=0;
                e.hideHud=Array.IndexOf(args,"-stage2HideHud")>=0;
                if(selected=="performance"&&(e.video||e.hideHud))
                    throw new ArgumentException("Performance runs forbid capture/HUD overrides.");
                if(e.hideHud&&selected!="propulsion")
                    throw new ArgumentException("-stage2HideHud is only supported for propulsion diagnostics.");
                e.StartCoroutine(e.GuardedRun());
            }
            catch(Exception error){Debug.LogError("Stage 2 attachment failed: "+error);Application.Quit(2);}
        }

        IEnumerator GuardedRun()
        {
            originalRate=Time.captureFramerate;originalScale=Time.timeScale;
            originalAutopilot=owner.Director.Player.AutopilotForTesting;
            originalHud=hud&&hud.enabled;startedWall=Time.realtimeSinceStartup;
            report.buildGuid=Application.buildGUID;report.mode=mode;
            report.roadLightResponseEnabled=OpeningRoadLightResponse.Enabled;
            report.startedUtc=DateTime.UtcNow.ToString("o");
            report.profileDirectory=Stage2EvidenceProfile.Directory;
            report.propulsionEnabled=IonPropulsion.Stage2Enabled;
            report.exhaustPolishEnabled=IonPropulsion.Stage2Enabled&&IonPropulsion.JetPolishEnabled;
            report.layeredBlueExhaust=report.exhaustPolishEnabled&&IonPropulsion.LayeredEnabled;
            report.width=Screen.width;report.height=Screen.height;
            report.simulationCaptureRate=mode=="performance"?0:CaptureRate;
            report.gpu=SystemInfo.graphicsDeviceName;report.graphicsApi=SystemInfo.graphicsDeviceType.ToString();
            report.driver=mode=="propulsion"||mode=="performance"
                ?"Scripted virtual Input System gamepad, actual trigger/boost and track-following stick; player testing autopilot disabled."
                :"Ordinary test-autopilot inputs through normal race physics.";
            report.scope="Native source-identified evidence. Programmatic menu opening and lifecycle calls are screenshot coverage, not pointer/keyboard/physical-controller validation. Vehicle/camera transforms, speed, energy, laps, times and results are never assigned. Only normal StartRace/TogglePause/ReturnToTitle operations reset lifecycle. Automatic gameplay recoveries are recorded. No audio is captured; no human-driving or artistic acceptance claim.";
            report.timing=mode=="performance"
                ?"Isolated real-time delivered CPU frame intervals during actual sustained boost; no screenshots/encoder, no isolated GPU or hardware-controller claim."
                :"24 Hz simulation-time native capture, ordinary chase camera; PNG encoding changes wall-clock duration and cannot establish real-time performance.";
            if(video)Directory.CreateDirectory(Path.Combine(folder,"frames"));
            Time.captureFramerate=report.simulationCaptureRate;Time.timeScale=1;
            if(hideHud&&hud)hud.enabled=false;
            var stack=new Stack<IEnumerator>();stack.Push(Work());
            // Flatten nested iterators so every capture/input failure writes an incomplete report.
            while(stack.Count>0)
            {
                object current=null;bool advanced=false;
                try
                {
                    advanced=stack.Peek().MoveNext();
                    if(advanced)current=stack.Peek().Current;
                    else{(stack.Pop() as IDisposable)?.Dispose();continue;}
                }
                catch(Exception error){report.error=error.ToString();Debug.LogError("Stage 2 evidence: "+error);break;}
                if(current is IEnumerator nested)stack.Push(nested);
                else yield return current;
            }
            while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();
            report.complete=report.error==null;
            report.finishedUtc=DateTime.UtcNow.ToString("o");report.wallSeconds=Time.realtimeSinceStartup-startedWall;
            report.sequenceFrames=sequence;
            if(mode=="performance")
            {
                sustainedBoostFrames.Sort();report.performanceSamples=sustainedBoostFrames.Count;
                if(sustainedBoostFrames.Count<20){report.complete=false;report.error="Fewer than 20 real sustained-boost frame intervals observed.";}
                else
                {
                    foreach(float value in sustainedBoostFrames)report.meanMs+=value;
                    report.meanMs/=sustainedBoostFrames.Count;
                    report.p50Ms=Percentile(sustainedBoostFrames,.5f);
                    report.p95Ms=Percentile(sustainedBoostFrames,.95f);
                    report.p99Ms=Percentile(sustainedBoostFrames,.99f);
                }
            }
            Cleanup();
            File.WriteAllText(Path.Combine(folder,"stage2-evidence.json"),JsonUtility.ToJson(report,true));
            Application.Quit(report.complete?0:1);
        }

        IEnumerator Work()
        {
            var director=owner.Director;
            bool propulsion=mode=="propulsion"||mode=="performance";
            director.Player.AutopilotForTesting=!propulsion;
            recording=mode=="loop";
            yield return Frames(48,"title-warmup");
            if(propulsion){yield return Propulsion();yield break;}
            if(!hud)throw new InvalidOperationException("No RaceHUD found.");
            yield return View("01-title","title");
            hud.ShowEvidencePage("settings");yield return Frames(24,"settings");yield return View("02-settings","settings");
            hud.ShowEvidencePage("controls");yield return Frames(24,"controls");yield return View("03-controls","controls");
            hud.ShowEvidencePage("bindings");yield return Frames(24,"bindings");yield return View("03b-bindings","bindings");
            hud.ShowEvidencePage("comfort");yield return Frames(24,"comfort");yield return View("03c-comfort","comfort");
            hud.ShowEvidencePage("title");
            director.StartRace();
            yield return Frames(24,"countdown");yield return View("04-countdown","countdown");
            yield return Until(()=>director.Phase==RacePhase.Racing,15,"race-start");
            yield return Frames(96,"racing");yield return View("05-racing","racing");
            director.TogglePause();
            if(director.Phase!=RacePhase.Paused)throw new InvalidOperationException("Pause did not enter Paused.");
            float frozenTime=director.SimulationTime;
            Vector3 frozenPosition=director.Player.Body.position;
            yield return Frames(24,"paused");yield return View("06-paused","paused");
            if(director.SimulationTime!=frozenTime||director.Player.Body.position!=frozenPosition)
                throw new InvalidOperationException("Simulation advanced while paused.");
            hud.ShowEvidencePage("settings");yield return Frames(24,"pause-settings");yield return View("07-pause-settings","pause-settings");
            hud.ShowEvidencePage("race");director.TogglePause();
            yield return Frames(24,"resumed");yield return View("08-resumed","resumed");
            if(mode=="loop")
            {
                yield return Until(()=>director.Phase==RacePhase.Finished,600,"full-race");
                yield return Frames(48,"results-provisional");yield return View("09-results-provisional","results");
                yield return Until(()=>!director.HasPendingRivals,180,"rivals-finishing");
                yield return Frames(24,"results-final");yield return View("10-results-final","results");
                if(director.Player.ProgressTracker.CompletedLaps<director.TotalLaps||director.FinishTime<=0||director.ManualRunEligible)
                    throw new InvalidOperationException("Full-race result/provenance failed.");
                foreach(var finish in director.FinishRecords)
                    report.results.Add(new Result{racerId=finish.RacerId,name=finish.DisplayName,
                        position=finish.Position,status=finish.Status.ToString(),hasTime=finish.HasFinishTime,time=finish.FinishTime});
                report.officialFinishTime=director.FinishTime;
                director.RestartRace();yield return Frames(24,"retry");yield return View("11-retry","countdown");
            }
            director.ReturnToTitle();hud.ShowEvidencePage("title");
            yield return Frames(24,"returned-title");yield return View("12-returned-title","title");
            if(director.Phase!=RacePhase.Menu||Time.timeScale!=1)
                throw new InvalidOperationException("Return to title did not restore menu lifecycle.");
        }

        IEnumerator Propulsion()
        {
#if ENABLE_INPUT_SYSTEM
            // This scope exists only for the opt-in synthetic-input diagnostic.
            // Without it, InputSystem disables a newly added non-background
            // gamepad when another app has focus, even while physics keeps running.
            inputBackground=new DiagnosticInputBackgroundScope();
            report.originalInputBackgroundBehavior=inputBackground.PreviousBehavior.ToString();
            report.diagnosticInputBackgroundBehavior=InputSystem.settings.backgroundBehavior.ToString();
            report.diagnosticRunInBackground=Application.runInBackground;
            report.driver+=" Diagnostic input ignores application focus; the previous input/background policy is restored on cleanup.";
            originalGamepad=Gamepad.current;gamepad=InputSystem.AddDevice<Gamepad>();gamepad.MakeCurrent();
            InputSystem.onBeforeUpdate+=QueueInput;
            InputSystem.onDeviceChange+=ObserveDeviceChange;
            Debug.Log("STAGE2_DIAGNOSTIC_INPUT created device="+gamepad.deviceId+
                " focused="+Application.isFocused+" enabled="+gamepad.enabled+
                " backgroundBehavior="+InputSystem.settings.backgroundBehavior);
            owner.Director.StartRace();
            yield return Until(()=>owner.Director.Phase==RacePhase.Racing,15,"countdown");
            driverStarted=owner.Director.SimulationTime;
            recording=true;
            string previous="";float stageStarted=0;bool captured=false;
            while(owner.Director.SimulationTime-driverStarted<PropulsionDuration)
            {
                float elapsed=owner.Director.SimulationTime-driverStarted;
                var command=CommandAt(elapsed);
                if(previous!=command.name){stageStarted=elapsed;captured=false;previous=command.name;}
                yield return Frames(1,command.name);
                if(!captured&&elapsed-stageStarted>=.3f&&mode!="performance")
                {captured=true;yield return View("propulsion-"+command.name,command.name);}
            }
            bool cruise=false,acceleration=false,boost=false,release=false,empty=false;
            foreach(var sample in report.samples)
            {
                cruise|=sample.stage=="cruise"&&sample.throttle>.2f&&sample.throttle<.4f;
                acceleration|=sample.stage=="acceleration"&&sample.throttle>.95f;
                boost|=sample.stage=="boost"&&sample.boosting;
                release|=sample.stage=="release"&&sample.throttle<.01f&&sample.exhaustResponse<.03f&&sample.speedKph>20;
                empty|=sample.stage=="boost"&&sample.energy<.08f&&!sample.boosting;
            }
            report.cruiseObserved=cruise;report.accelerationObserved=acceleration;
            report.boostObserved=boost;report.releaseObserved=release;report.emptyBoostObserved=empty;
            if(!cruise||!acceleration||!boost||!release)
                throw new InvalidOperationException("Requested cruise/acceleration/boost/release was not observed through actual vehicle inputs.");
#else
            throw new InvalidOperationException("Propulsion evidence requires the Input System.");
#endif
        }

        public readonly struct DriverCommand
        {
            public readonly string name;public readonly float throttle;public readonly bool boost;
            public DriverCommand(string label,float demand,bool boosting){name=label;throttle=demand;boost=boosting;}
        }
        public static DriverCommand CommandAt(float elapsed)
        {
            if(elapsed<.5f)return new DriverCommand("idle",0,false);
            if(elapsed<3)return new DriverCommand("cruise",.3f,false);
            if(elapsed<6)return new DriverCommand("acceleration",1,false);
            if(elapsed<11)return new DriverCommand("boost",1,true);
            if(elapsed<13)return new DriverCommand("release",0,false);
            if(elapsed<16)return new DriverCommand("recharge",.3f,false);
            return new DriverCommand("reboost",1,true);
        }
#if ENABLE_INPUT_SYSTEM
        /// <summary>Temporary input policy for synthetic diagnostics, never a player setting.</summary>
        public sealed class DiagnosticInputBackgroundScope : IDisposable
        {
            readonly InputSettings settings;
            readonly bool previousRunInBackground;
            bool disposed;
            public InputSettings.BackgroundBehavior PreviousBehavior { get; }
            public DiagnosticInputBackgroundScope()
            {
                settings=InputSystem.settings;
                PreviousBehavior=settings.backgroundBehavior;
                previousRunInBackground=Application.runInBackground;
                Application.runInBackground=true;
                settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            }
            public void Dispose()
            {
                if(disposed)return;disposed=true;
                settings.backgroundBehavior=PreviousBehavior;
                Application.runInBackground=previousRunInBackground;
            }
        }
        void ObserveDeviceChange(InputDevice device,InputDeviceChange change)
        {
            if(device==gamepad)Debug.Log("STAGE2_DIAGNOSTIC_INPUT deviceChange="+change+
                " focused="+Application.isFocused+" enabled="+gamepad.enabled);
        }
        void OnApplicationFocus(bool focused)
        {
            if(inputBackground!=null)Debug.Log("STAGE2_DIAGNOSTIC_INPUT focused="+focused+
                " deviceEnabled="+(gamepad!=null&&gamepad.enabled));
        }
        void QueueInput()
        {
            if(gamepad==null||!gamepad.added||!owner)return;
            var player=owner.Director.Player;var state=new GamepadState();
            if(driverStarted>=0&&owner.Director.Phase==RacePhase.Racing)
            {
                var command=CommandAt(owner.Director.SimulationTime-driverStarted);
                var track=owner.Track;var frame=track.Evaluate(player.TrackProgress);
                var target=track.Evaluate(player.TrackProgress+(12+player.Body.linearVelocity.magnitude*.33f)/track.Length);
                Vector3 offset=Vector3.ProjectOnPlane(target.Position-player.Body.position,frame.Up);
                float angle=Vector3.SignedAngle(player.transform.forward,offset.normalized,frame.Up);
                float steer=Mathf.Clamp(HoverVehicle.PursuitSteering(player.Body.linearVelocity.magnitude,angle,offset.magnitude),-1,1);
                float raw=Mathf.Abs(steer)<.001f?0:Mathf.Sign(steer)*Mathf.Lerp(InputSystem.settings.defaultDeadzoneMin,InputSystem.settings.defaultDeadzoneMax,Mathf.Abs(steer));
                state.rightTrigger=command.throttle;state.leftStick=new Vector2(raw,0);
                if(command.boost)state=state.WithButton(GamepadButton.South);
            }
            gamepad.MakeCurrent();InputSystem.QueueStateEvent(gamepad,state);queuedInputEvents++;
        }
#endif
        IEnumerator Frames(int count,string label)
        {
            for(int i=0;i<count;i++)
            {
                yield return new WaitForEndOfFrame();
#if ENABLE_INPUT_SYSTEM
                if(inputBackground!=null&&gamepad!=null&&!gamepad.enabled)
                    throw new InvalidOperationException("Synthetic gamepad was disabled during diagnostic capture.");
#endif
                if(recording)Observe(label);
            }
        }
        IEnumerator Until(Func<bool> predicate,float seconds,string label)
        {
            float deadline=Time.realtimeSinceStartup+seconds;
            while(!predicate())
            {
                if(Time.realtimeSinceStartup>deadline)throw new TimeoutException("Timed out: "+label);
                yield return Frames(1,label);
            }
        }
        IEnumerator View(string name,string label)
        {
            if(mode=="performance")throw new InvalidOperationException("Screenshots forbidden in performance mode.");
            yield return new WaitForEndOfFrame();
            // A video frame is captured on this render too, retaining a continuous sequence.
            if(recording)Observe(label);
            var sample=Sample(label);sample.file=name+".png";
            SavePng(sample.file);report.views.Add(sample);
        }
        void Observe(string label)
        {
            var sample=Sample(label);
            if(mode=="performance")
            {
                float elapsed=owner.Director.SimulationTime-driverStarted;
                // Exclude ignition and exhausted tail. Timestamp boundaries are shared by A/B.
                if(elapsed>=7&&elapsed<9.5f&&sample.boosting)sustainedBoostFrames.Add(Time.unscaledDeltaTime*1000);
            }
            if(video)
            {
                sample.file="frames/frame-"+sequence.ToString("D5")+".png";SavePng(sample.file);sequence++;
            }
            report.samples.Add(sample);
        }
        SampleData Sample(string label)
        {
            var d=owner.Director;var player=d.Player;var camera=owner.Camera;
            var propulsion=player.GetComponent<IonPropulsion>();
            var command=CommandAt(Mathf.Max(0,d.SimulationTime-driverStarted));
            var sample=new SampleData{
                stage=label,phase=d.Phase.ToString(),page=hud?hud.EvidencePage:"",
                raceTime=d.RaceTime,simulationTime=d.SimulationTime,wallSeconds=Time.realtimeSinceStartup-startedWall,
                speedKph=player.SpeedKph,throttle=player.ThrottleInput,boosting=player.IsBoosting,energy=player.Boost01,
                requestedThrottle=driverStarted>=0?command.throttle:-1,requestedBoost=driverStarted>=0&&command.boost,
                exhaustDemand=propulsion?propulsion.ExhaustDemand:0,exhaustResponse=propulsion?propulsion.ExhaustResponse:0,
                progress=player.TrackProgress,lap=d.Lap,position=d.Position,recoveries=player.RecoveryCount,
                manualEligible=d.ManualRunEligible,grounded=player.IsGrounded,
                playerPosition=player.Body.position,playerRotation=player.Body.rotation,velocity=player.Body.linearVelocity,
                visualPosition=player.VisualRoot.position,visualRotation=player.VisualRoot.rotation,
                cameraPosition=camera.transform.position,cameraRotation=camera.transform.rotation,fov=camera.fieldOfView,
                width=Screen.width,height=Screen.height,applicationFocused=Application.isFocused};
#if ENABLE_INPUT_SYSTEM
            sample.syntheticDeviceEnabled=gamepad!=null&&gamepad.enabled;
            sample.syntheticDeviceCurrent=gamepad!=null&&Gamepad.current==gamepad;
            sample.syntheticTrigger=gamepad!=null?gamepad.rightTrigger.ReadValue():-1;
            sample.queuedInputEvents=queuedInputEvents;
#endif
            return sample;
        }
        void SavePng(string relative)
        {
            Texture2D image=null;
            try
            {
                image=ScreenCapture.CaptureScreenshotAsTexture();
                if(!image)throw new IOException("Native screenshot returned null.");
                File.WriteAllBytes(Path.Combine(folder,relative),image.EncodeToPNG());
            }
            finally{if(image)Destroy(image);}
        }
        static float Percentile(List<float> values,float fraction)=>values[Mathf.Min(values.Count-1,Mathf.FloorToInt((values.Count-1)*fraction))];
        void Cleanup()
        {
            if(cleaned)return;cleaned=true;
#if ENABLE_INPUT_SYSTEM
            InputSystem.onBeforeUpdate-=QueueInput;
            InputSystem.onDeviceChange-=ObserveDeviceChange;
            if(gamepad!=null&&gamepad.added)InputSystem.RemoveDevice(gamepad);
            if(originalGamepad!=null&&originalGamepad.added)originalGamepad.MakeCurrent();
            inputBackground?.Dispose();inputBackground=null;
#endif
            if(owner&&owner.Director&&owner.Director.Player)owner.Director.Player.AutopilotForTesting=originalAutopilot;
            if(hud)hud.enabled=originalHud;
            Time.captureFramerate=originalRate;Time.timeScale=originalScale;
        }
        void OnDestroy(){Cleanup();}
        [Serializable] sealed class Report
        {
            public string buildGuid,mode,scope,timing,driver,profileDirectory,startedUtc,finishedUtc,error,gpu,graphicsApi;
            public string originalInputBackgroundBehavior,diagnosticInputBackgroundBehavior;
            public bool exhaustPolishEnabled;
            public bool layeredBlueExhaust;
            public bool roadLightResponseEnabled;
            public bool complete,propulsionEnabled,cruiseObserved,accelerationObserved,boostObserved,releaseObserved,emptyBoostObserved,diagnosticRunInBackground;
            public int width,height,simulationCaptureRate,sequenceFrames,performanceSamples;
            public float wallSeconds,officialFinishTime,meanMs,p50Ms,p95Ms,p99Ms;
            public List<SampleData> samples=new List<SampleData>(),views=new List<SampleData>();
            public List<Result> results=new List<Result>();
        }
        [Serializable] sealed class SampleData
        {
            public string file,stage,phase,page;
            public float raceTime,simulationTime,wallSeconds,speedKph,throttle,energy,requestedThrottle,exhaustDemand,exhaustResponse,progress,fov,syntheticTrigger;
            public bool boosting,requestedBoost,manualEligible,grounded,applicationFocused,syntheticDeviceEnabled,syntheticDeviceCurrent;
            public int lap,position,recoveries,width,height,queuedInputEvents;
            public Vector3 playerPosition,velocity,visualPosition,cameraPosition;
            public Quaternion playerRotation,visualRotation,cameraRotation;
        }
        [Serializable] sealed class Result
        {public string racerId,name,status;public int position;public bool hasTime;public float time;}
    }
}
