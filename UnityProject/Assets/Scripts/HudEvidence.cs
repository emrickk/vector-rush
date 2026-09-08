using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Opt-in native HUD phase/aspect evidence. No synthetic race states.</summary>
    public sealed class HudEvidence : MonoBehaviour
    {
        VectorBootstrap bootstrap;
        RaceDirector director;
        HoverVehicle player;
        string folder;
        bool originalAutopilot, initialized;
        int originalCaptureRate, originalWidth, originalHeight;
        float originalTimeScale;
        FullScreenMode originalScreenMode;
        readonly Report report=new Report();
        readonly Vector2Int[] sizes={new Vector2Int(1920,1080),new Vector2Int(1280,800),new Vector2Int(1920,810)};

        public static bool TryStart(VectorBootstrap owner)
        {
            var args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-hudEvidence");
            if(index<0) return false;
            if(index+1>=args.Length || !Path.IsPathRooted(args[index+1]))
            { Debug.LogError("HUD evidence requires -hudEvidence <absolute output folder>."); return false; }
            if(!owner || !owner.Director || !owner.Director.Player || !FindAnyObjectByType<RaceHUD>())
            { Debug.LogError("HUD evidence requires an initialized race, player and HUD."); return false; }
            foreach(var evidence in owner.GetComponents<RaceEvidence>()) { evidence.StopAllCoroutines(); evidence.enabled=false; }
            var runner=owner.gameObject.AddComponent<HudEvidence>(); runner.bootstrap=owner;
            runner.director=owner.Director; runner.player=owner.Director.Player;
            runner.folder=Path.GetFullPath(args[index+1]); runner.StartCoroutine(runner.Run()); return true;
        }

        IEnumerator Run()
        {
            Directory.CreateDirectory(folder);
            originalAutopilot=player.AutopilotForTesting; originalCaptureRate=Time.captureFramerate;
            originalTimeScale=Time.timeScale; originalWidth=Screen.width; originalHeight=Screen.height; originalScreenMode=Screen.fullScreenMode;
            initialized=true; report.startedUtc=DateTime.UtcNow.ToString("o"); report.unityVersion=Application.unityVersion;
            report.scope="Actual native Unity HUD and ordinary race camera. Seven real states are captured at 1920x1080 (16:9), 1280x800 (16:10) and 1920x810 (ultrawide). Existing testing autopilot drives through normal vehicle physics; Time.captureFramerate=24 between snapshots makes this a simulation-time display run, not a performance or human-input test. Simulation is temporarily frozen with Time.timeScale=0 and captureFramerate=0 for matched aspect snapshots; director state is not changed by that freeze. StartRace/TogglePause use normal transitions. Boost is observed, never forced. Final-lap capture waits 0.30 race-time seconds after observing the real lap transition so the notice can fade in; finalLapTransitionObserved records whether the transition was still ahead after pause. Results require an actual three-lap finish; phase, lap progress, speed, energy and VFX values are never injected. This verifies capture coverage, not visual-quality acceptance or button hit testing.";
            Save("RUNNING — native HUD coverage\n");
            try
            {
                player.AutopilotForTesting=true; Time.timeScale=1f; Time.captureFramerate=24;
                yield return new WaitForSecondsRealtime(2f);
                yield return CaptureGroup("01-menu",RacePhase.Menu);
                if(report.error==null)
                {
                    director.StartRace();
                    yield return CaptureGroup("02-countdown",RacePhase.Countdown);
                }
                if(report.error==null)
                {
                    float deadline=Time.realtimeSinceStartup+30f;
                    while((director.Phase!=RacePhase.Racing || director.RaceTime<3f || player.IsBoosting || player.SpeedKph<80f) && Time.realtimeSinceStartup<deadline) yield return null;
                    if(director.Phase!=RacePhase.Racing || player.IsBoosting || player.SpeedKph<80f) Fail("Ordinary moving racing state was not reached.");
                    else yield return CaptureGroup("03-racing",RacePhase.Racing);
                }
                if(report.error==null)
                {
                    float deadline=Time.realtimeSinceStartup+180f;
                    while(director.Phase==RacePhase.Racing && !player.IsBoosting && Time.realtimeSinceStartup<deadline) yield return null;
                    if(director.Phase!=RacePhase.Racing || !player.IsBoosting) Fail("No actual boost was observed before the race ended or the wait expired.");
                    else yield return CaptureGroup("04-boost",RacePhase.Racing,true);
                }
                if(report.error==null)
                {
                    director.TogglePause();
                    yield return CaptureGroup("05-pause",RacePhase.Paused);
                    if(report.error==null) director.TogglePause();
                }
                if(report.error==null)
                {
                    bool transitionAhead=director.Lap<director.TotalLaps;
                    float deadline=Time.realtimeSinceStartup+180f;
                    while(director.Phase==RacePhase.Racing && director.Lap<director.TotalLaps && Time.realtimeSinceStartup<deadline) yield return null;
                    if(director.Phase!=RacePhase.Racing || director.Lap<director.TotalLaps)
                        Fail("The real final lap was not reached while racing.");
                    else
                    {
                        report.finalLapTransitionObserved=transitionAhead;
                        report.finalLapObservationRaceTime=director.RaceTime;
                        // Let the real notice complete its .20 s fade-in before freezing.
                        while(director.Phase==RacePhase.Racing && director.RaceTime-report.finalLapObservationRaceTime<.30f) yield return null;
                        yield return CaptureGroup("06-final-lap",RacePhase.Racing);
                    }
                }
                if(report.error==null)
                {
                    float deadline=Time.realtimeSinceStartup+240f;
                    while(director.Phase==RacePhase.Racing && Time.realtimeSinceStartup<deadline) yield return null;
                    if(director.Phase!=RacePhase.Finished || player.ProgressTracker.CompletedLaps<director.TotalLaps)
                        Fail("The normal race did not complete all laps; results were not fabricated.");
                    else yield return CaptureGroup("07-results",RacePhase.Finished);
                }
                report.finishedRaceTime=director.RaceTime; report.completedLaps=player.ProgressTracker.CompletedLaps; report.playerRecoveries=player.RecoveryCount;
                report.complete=report.error==null && report.views.Count==21 && director.Phase==RacePhase.Finished;
            }
            finally
            {
                Restore(); report.finishedUtc=DateTime.UtcNow.ToString("o");
                Save(report.complete?"COMPLETE — 21 native phase/aspect screenshots. Visual review remains separate.\n":"INCOMPLETE — "+report.error+"\n");
            }
            yield return new WaitForSecondsRealtime(.75f);
            Application.Quit(report.complete?0:1);
        }

        IEnumerator CaptureGroup(string label,RacePhase expected,bool requireBoost=false)
        {
            float previousScale=Time.timeScale; int previousCaptureRate=Time.captureFramerate;
            Time.timeScale=0f; Time.captureFramerate=0;
            try
            {
                foreach(var size in sizes)
                {
                    if(director.Phase!=expected || (requireBoost && !player.IsBoosting))
                    { Fail("Native state changed before capture: "+label); yield break; }
                    Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
                    float deadline=Time.realtimeSinceStartup+8f;
                    do { yield return null; } while((Screen.width!=size.x || Screen.height!=size.y) && Time.realtimeSinceStartup<deadline);
                    if(Screen.width!=size.x || Screen.height!=size.y)
                    { Fail("Window did not reach requested "+size.x+"x"+size.y+"; actual "+Screen.width+"x"+Screen.height+"."); yield break; }
                    for(int i=0;i<6;i++) yield return null;
                    yield return new WaitForEndOfFrame();
                    if(director.Phase!=expected || (requireBoost && !player.IsBoosting))
                    { Fail("Native state changed at screenshot: "+label); yield break; }
                    string filename=label+"-"+size.x+"x"+size.y+".png",path=Path.Combine(folder,filename);
                    var view=new View {
                        file=filename,state=label,phase=director.Phase.ToString(),width=Screen.width,height=Screen.height,
                        aspect=Screen.width/(float)Screen.height,raceTime=director.RaceTime,countdown=director.CountdownRemaining,
                        speedKph=player.SpeedKph,energy01=player.Boost01,throttleInput=player.ThrottleInput,
                        isBoosting=player.IsBoosting,grounded=player.IsGrounded,lap=director.Lap,position=director.Position,
                        completedLaps=player.ProgressTracker.CompletedLaps,normalizedProgress=player.TrackProgress,
                        timeScale=Time.timeScale,captureFramerate=Time.captureFramerate
                    };
                    DateTime requested=DateTime.UtcNow;
                    ScreenCapture.CaptureScreenshot(path);
                    deadline=Time.realtimeSinceStartup+8f;
                    do { yield return null; } while((!File.Exists(path) || File.GetLastWriteTimeUtc(path)<requested) && Time.realtimeSinceStartup<deadline);
                    if(!File.Exists(path) || File.GetLastWriteTimeUtc(path)<requested)
                    { Fail("Screenshot was not written: "+filename); yield break; }
                    report.views.Add(view); Save("RUNNING — native HUD coverage\n");
                }
            }
            finally { Time.captureFramerate=previousCaptureRate; Time.timeScale=previousScale; }
        }

        void Fail(string reason) { if(report.error==null) report.error=reason; Debug.LogError("HUD evidence: "+reason); }
        void Save(string status)
        {
            File.WriteAllText(Path.Combine(folder,"hud-evidence.json"),JsonUtility.ToJson(report,true));
            File.WriteAllText(Path.Combine(folder,"hud-status.txt"),status);
        }
        void Restore()
        {
            if(!initialized) return;
            initialized=false;
            if(player) player.AutopilotForTesting=originalAutopilot;
            Time.captureFramerate=originalCaptureRate; Time.timeScale=originalTimeScale;
            Screen.SetResolution(originalWidth,originalHeight,originalScreenMode);
        }
        void OnDestroy() { Restore(); }
        [Serializable] sealed class Report
        {
            public string scope,startedUtc,finishedUtc,unityVersion,error; public bool complete,finalLapTransitionObserved;
            public float finishedRaceTime,finalLapObservationRaceTime; public int completedLaps,playerRecoveries;
            public List<View> views=new List<View>();
        }
        [Serializable] sealed class View
        {
            public string file,state,phase;
            public int width,height,lap,position,completedLaps,captureFramerate;
            public float aspect,raceTime,countdown,speedKph,energy01,throttleInput,normalizedProgress,timeScale;
            public bool isBoosting,grounded;
        }
    }
}
