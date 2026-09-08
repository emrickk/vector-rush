using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace VectorRush
{
    // Captures actual rendering and frame times; command-line recording is opt-in.
    public sealed class RaceEvidence : MonoBehaviour
    {
        readonly List<float> frames=new List<float>();
        string folder;float started;bool collecting;bool autopilot;bool quitAfter;bool recordReplay;
        float nextTelemetry;
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length;i++){
                if(args[i]=="-evidencePath"&&i+1<args.Length)folder=args[i+1];
                if(args[i]=="-autopilot")autopilot=true;
                if(args[i]=="-quitAfterEvidence")quitAfter=true;
                if(args[i]=="-recordReplay")recordReplay=true;
            }
            if(string.IsNullOrEmpty(folder))yield break;
            Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(3);
            yield return Capture("01-title.png");
            var director=VectorBootstrap.Instance.Director;
            director.Player.AutopilotForTesting=autopilot;
            director.StartRace();
            yield return new WaitForSecondsRealtime(4);
            yield return Capture("02-start.png");
            if(recordReplay){yield return RecordReplay();yield break;}
            started=Time.realtimeSinceStartup;collecting=true;
            if(autopilot)StartCoroutine(VerifyRace());
        }
        IEnumerator RecordReplay()
        {
            yield return new WaitForSecondsRealtime(18);
            string frameFolder=Path.Combine(folder,"frames");Directory.CreateDirectory(frameFolder);
            // Deterministic simulation-time capture. This mode does not collect performance measurements.
            Time.captureFramerate=24;
            for(int i=0;i<360;i++){
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(frameFolder,$"frame-{i:D04}.png"));
            }
            Time.captureFramerate=0;
            File.WriteAllText(Path.Combine(folder,"replay-info.txt"),"Actual native Unity gameplay, automated steering through normal vehicle physics. 360 frames at 24 simulation frames per second; 15 seconds. Separate capture run, not a real-time performance measurement.\n");
            if(quitAfter){yield return new WaitForSecondsRealtime(2);Application.Quit();}
        }
        void Update()
        {
            if(!collecting)return;
            frames.Add(Time.unscaledDeltaTime*1000);
            if(Time.realtimeSinceStartup-started>=60){collecting=false;WriteReport();StartCoroutine(Capture("03-race.png"));}
            if(Time.realtimeSinceStartup>nextTelemetry){nextTelemetry=Time.realtimeSinceStartup+5;WriteTelemetry();}
        }
        public IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));
        }
        void WriteTelemetry()
        {
            var d=VectorBootstrap.Instance.Director;
            string text=$"Time {d.RaceTime:F2} Phase {d.Phase} Lap {d.Lap} Position {d.Position}\n";
            foreach(var r in d.Racers)text+=$"{r.DisplayName} speed {r.SpeedKph:F1} progress {r.RaceProgress:F4} track {r.TrackProgress:F4} recoveries {r.RecoveryCount} grounded {r.IsGrounded}\n";
            File.WriteAllText(Path.Combine(folder,"live-telemetry.txt"),text);
        }
        IEnumerator VerifyRace()
        {
            var director=VectorBootstrap.Instance.Director;
            float deadline=Time.realtimeSinceStartup+200;
            while(director.Phase!=RacePhase.Finished&&Time.realtimeSinceStartup<deadline){WriteTelemetry();yield return new WaitForSecondsRealtime(5);}
            WriteTelemetry();
            bool finished=director.Phase==RacePhase.Finished;
            string result=$"Autopilot through real hover physics: {(finished?"FINISHED":"TIMED OUT")}\nTime {director.RaceTime:F2}\nLaps {director.Player.ProgressTracker.CompletedLaps}\nRecoveries {director.Player.RecoveryCount}\n";
            File.WriteAllText(Path.Combine(folder,"race-verification.txt"),result);
            yield return Capture(finished?"04-results.png":"04-timeout.png");
            if(finished){
                for(int i=0;i<3;i++){
                    director.RestartRace();yield return new WaitForSecondsRealtime(1);
                    director.TogglePause();float before=director.CountdownRemaining;yield return new WaitForSecondsRealtime(.5f);
                    bool frozen=Mathf.Abs(director.CountdownRemaining-before)<.01f;director.TogglePause();
                    File.AppendAllText(Path.Combine(folder,"race-verification.txt"),$"Restart {i+1}: countdown pause frozen={frozen}, laps={director.Player.ProgressTracker.CompletedLaps}\n");
                }
            }
            if(collecting){collecting=false;WriteReport();}
            if(quitAfter){yield return new WaitForSecondsRealtime(1);Application.Quit();}
        }
        void WriteReport()
        {
            frames.Sort();if(frames.Count==0)return;
            float sum=0;foreach(float f in frames)sum+=f;
            string report=$"Actual runtime sample — {DateTime.UtcNow:o}\nUnity {Application.unityVersion}\nGPU {SystemInfo.graphicsDeviceName}\nResolution {Screen.width}x{Screen.height}\nAutopilot {autopilot}\nVSync {QualitySettings.vSyncCount}\nSamples {frames.Count}\nMean ms {sum/frames.Count:F2}\nP50 ms {frames[frames.Count/2]:F2}\nP95 ms {frames[Mathf.Min(frames.Count-1,(int)(frames.Count*.95f))]:F2}\nP99 ms {frames[Mathf.Min(frames.Count-1,(int)(frames.Count*.99f))]:F2}\nUnity allocated memory MiB {Profiler.GetTotalAllocatedMemoryLong()/1048576f:F1}\nRace state {VectorBootstrap.Instance.Director.Phase}\nRace lap {VectorBootstrap.Instance.Director.Lap}\nScope: observed frame intervals; not isolated GPU timings or proof of completed race.\n";
            File.WriteAllText(Path.Combine(folder,"runtime-metrics.txt"),report);
        }
    }
}
