using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    // Captures actual rendering and frame times; command-line recording is opt-in.
    public sealed class RaceEvidence : MonoBehaviour
    {
        readonly List<float> frames=new List<float>();
        string folder;float started;bool collecting;bool autopilot;bool quitAfter;bool recordReplay;bool quickEvidence;bool diagnosticRoad;bool diagnosticAO;bool inspectCoast;
        float nextTelemetry;bool capturedCrest,capturedDescent,diagnosticFog;
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length;i++){
                if(args[i]=="-evidencePath"&&i+1<args.Length)folder=args[i+1];
                if(args[i]=="-autopilot")autopilot=true;
                if(args[i]=="-quitAfterEvidence")quitAfter=true;
                if(args[i]=="-recordReplay")recordReplay=true;
                if(args[i]=="-quickEvidence")quickEvidence=true;
                if(args[i]=="-diagnosticRoad")diagnosticRoad=true;
                if(args[i]=="-diagnosticAO")diagnosticAO=true;
                if(args[i]=="-inspectCoast")inspectCoast=true;
                if(args[i]=="-diagnosticFog")diagnosticFog=true;
            }
            if(string.IsNullOrEmpty(folder))yield break;
            Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(3);
            yield return Capture("01-title.png");
            if(diagnosticFog){yield return DiagnoseFog();yield break;}
            if(quickEvidence&&inspectCoast){yield return InspectCoast();yield return new WaitForSecondsRealtime(2);Application.Quit();yield break;}
            if(diagnosticAO){yield return DiagnoseAO();yield break;}
            var director=VectorBootstrap.Instance.Director;
            director.Player.AutopilotForTesting=autopilot;
            director.StartRace();
            yield return new WaitForSecondsRealtime(4);
            yield return Capture("02-start.png");
            if(quickEvidence){yield return new WaitForSecondsRealtime(2);Application.Quit();yield break;}
            if(diagnosticRoad){yield return DiagnoseRoad();yield break;}
            if(recordReplay){yield return RecordReplay();yield break;}
            started=Time.realtimeSinceStartup;collecting=true;
            if(autopilot)StartCoroutine(VerifyRace());
        }
        IEnumerator DiagnoseFog()
        {
            Time.timeScale=0;
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("fog-01-enabled.png");
            RenderSettings.fog=false;
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("fog-02-disabled.png");
            RenderSettings.fog=true;Time.timeScale=1;
            File.WriteAllText(Path.Combine(folder,"fog-diagnostic.txt"),"Matched paused native title view. Only RenderSettings.fog is toggled; enabled Exp2 atmosphere versus disabled. Fog parameters are serialized in the bootstrap scene to preserve native variants. This diagnostic is not a performance measurement.\n");
            if(quitAfter){yield return new WaitForSecondsRealtime(2);Application.Quit();}
        }
        IEnumerator DiagnoseAO()
        {
            var features=Resources.FindObjectsOfTypeAll<ScreenSpaceAmbientOcclusion>();
            yield return Capture("ao-01-enabled.png");
            foreach(var feature in features)feature.SetActive(false);
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("ao-02-disabled.png");
            var probes=FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);var intensities=new float[probes.Length];
            for(int i=0;i<probes.Length;i++){intensities[i]=probes[i].intensity;probes[i].intensity=0;}
            float reflection=RenderSettings.reflectionIntensity;RenderSettings.reflectionIntensity=0;
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("ao-03-no-reflections.png");
            for(int i=0;i<probes.Length;i++)probes[i].intensity=intensities[i];RenderSettings.reflectionIntensity=reflection;
            var sun=RenderSettings.sun;var shadows=sun.shadows;sun.shadows=LightShadows.None;
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("ao-04-no-shadows.png");sun.shadows=shadows;
            var road=GameObject.Find("Running surface").GetComponent<MeshRenderer>();var original=road.sharedMaterial;
            var unlit=new Material(Shader.Find("Universal Render Pipeline/Unlit"));unlit.SetColor("_BaseColor",new Color(.17f,.20f,.23f));road.sharedMaterial=unlit;
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("ao-05-unlit-road.png");road.sharedMaterial=original;Destroy(unlit);
            foreach(var feature in features)feature.SetActive(true);
            File.WriteAllText(Path.Combine(folder,"diagnostic-scope.txt"),$"Identical native starting grid. SSAO enabled versus disabled; {features.Length} loaded feature(s). No changes to meshes, lighting, camera, or materials.\n");
            if(quitAfter){yield return new WaitForSecondsRealtime(2);Application.Quit();}
        }
        IEnumerator InspectCoast()
        {
            var cliff=GameObject.Find("Coastal headland")??GameObject.Find("Harbor headland");
            if(!cliff)yield break;
            var camera=VectorBootstrap.Instance.Camera;var chase=camera.GetComponent<ChaseCamera>();var hud=FindAnyObjectByType<RaceHUD>();
            var position=camera.transform.position;var rotation=camera.transform.rotation;float fov=camera.fieldOfView;
            var bounds=cliff.GetComponentInChildren<Renderer>().bounds;
            chase.enabled=false;hud.enabled=false;Time.timeScale=0;
            camera.fieldOfView=58;camera.transform.position=bounds.center+new Vector3(bounds.size.x*.9f,bounds.size.y*.85f,-bounds.size.z*1.3f);
            camera.transform.LookAt(bounds.center);
            for(int i=0;i<5;i++)yield return null;
            yield return Capture("07-coast-inspection.png");
            camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;chase.enabled=true;hud.enabled=true;Time.timeScale=1;
            File.WriteAllText(Path.Combine(folder,"coast-inspection-scope.txt"),"Dedicated native camera inspection of imported textured cliff; not a normal racing-camera frame. Gameplay views are 02-start,05-crest,06-city-descent.\n");
        }
        IEnumerator DiagnoseRoad()
        {
            var player=VectorBootstrap.Instance.Director.Player;
            float deadline=Time.realtimeSinceStartup+90;
            while((player.TrackProgress<.71f||player.TrackProgress>.75f)&&Time.realtimeSinceStartup<deadline)yield return null;
            Time.timeScale=0;
            var road=GameObject.Find("Running surface").GetComponent<MeshRenderer>();
            var underbody=GameObject.Find("Track underbody").GetComponent<MeshRenderer>();
            var original=road.sharedMaterial;var sun=RenderSettings.sun;var shadows=sun.shadows;
            yield return Capture("diag-01-baseline.png");
            sun.shadows=LightShadows.None;yield return null;yield return Capture("diag-02-no-shadows.png");sun.shadows=shadows;
            var probes=FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);var intensities=new float[probes.Length];
            for(int i=0;i<probes.Length;i++){intensities[i]=probes[i].intensity;probes[i].intensity=0;}
            float reflection=RenderSettings.reflectionIntensity;RenderSettings.reflectionIntensity=0;
            yield return null;yield return Capture("diag-03-no-reflections.png");
            for(int i=0;i<probes.Length;i++)probes[i].intensity=intensities[i];RenderSettings.reflectionIntensity=reflection;
            var unlit=new Material(Shader.Find("Universal Render Pipeline/Unlit"));unlit.SetColor("_BaseColor",new Color(.17f,.20f,.23f));road.sharedMaterial=unlit;
            yield return null;yield return Capture("diag-04-unlit-road.png");road.sharedMaterial=original;
            underbody.enabled=false;yield return null;yield return Capture("diag-05-no-underbody.png");underbody.enabled=true;
            var grain=original.GetTexture("_BaseMap");original.SetTexture("_BaseMap",null);yield return null;yield return Capture("diag-06-no-grain.png");original.SetTexture("_BaseMap",grain);
            var mesh=road.GetComponent<MeshFilter>().sharedMesh;var normals=mesh.normals;var flat=new Vector3[normals.Length];for(int i=0;i<flat.Length;i++)flat[i]=Vector3.up;
            mesh.normals=flat;yield return null;yield return Capture("diag-07-flat-normals.png");mesh.normals=normals;
            Destroy(unlit);Time.timeScale=1;
            File.WriteAllText(Path.Combine(folder,"diagnostic-scope.txt"),"Same paused native chase view; each condition is restored before the next. Baseline, no sun shadows, no reflection probe/ambient reflection, unlit road, hidden track underbody, no grain texture, flat road normals. Diagnostic changes are not delivery settings.\n");
            if(quitAfter){yield return new WaitForSecondsRealtime(2);Application.Quit();}
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
            var progress=VectorBootstrap.Instance.Director.Player.TrackProgress;
            if(!capturedCrest&&progress>=.30f&&progress<.33f){capturedCrest=true;StartCoroutine(Capture("05-crest.png"));}
            if(!capturedDescent&&progress>=.72f&&progress<.75f){capturedDescent=true;StartCoroutine(Capture("06-city-descent.png"));}
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
            if(inspectCoast)yield return InspectCoast();
            if(finished){
                yield return VerifyRestartLaunch("results-to-racing",RacePhase.Finished);
                director.TogglePause();
                yield return VerifyRestartLaunch("paused-race-to-racing",RacePhase.Paused);
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
        IEnumerator VerifyRestartLaunch(string label,RacePhase expectedSource)
        {
            var d=VectorBootstrap.Instance.Director;
            bool source=d.Phase==expectedSource;
            d.RestartRace();
            bool reset=d.Player.ProgressTracker.CompletedLaps==0&&Mathf.Abs(d.RaceTime)<.001f&&d.Player.Boost01>.999f&&d.Player.RecoveryCount==0;
            yield return new WaitForSecondsRealtime(4.2f);
            bool racing=d.Phase==RacePhase.Racing&&d.RaceTime>.2f&&d.Player.SpeedKph>10f;
            File.AppendAllText(Path.Combine(folder,"race-verification.txt"),$"Restart launch {label}: expectedSource={source}, laps/time/energy/recovery reset={reset}, racing/movement={racing}, time={d.RaceTime:F2}, speedKph={d.Player.SpeedKph:F1}\n");
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
