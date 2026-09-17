using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Opt-in native evidence. Normal launches never attach this component.</summary>
    public sealed class OpeningCityEvidence : MonoBehaviour
    {
        [Serializable] public sealed class Pose
        {
            public string file,anchor;
            public float time,progress,speed,fov,viewBlend,blurIntensity;
            public Vector3 cameraPosition;
            public Quaternion cameraRotation;
            public Vector3[] racerPositions,visualPositions;
            public Quaternion[] racerRotations,visualRotations;
        }
        [Serializable] public sealed class Report
        {
            public string buildGuid,revision,scope;
            public bool openingEnabled,complete,hudHidden,reducedMotion;
            public int width,height,rate=24;
            public List<Pose> frames=new List<Pose>();
        }
        VectorBootstrap owner;
        string folder;
        bool stills,performance,hideHud;
        readonly float[] anchors={.15f,.22f,.30f};
        readonly string[] names={"01-approach","02-bend","03-reveal"};

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-openingEvidence");
            bool performance=false;
            if(i<0){i=Array.IndexOf(args,"-openingPerformance");performance=true;}
            if(i<0||i+1>=args.Length)return;
            if(!Path.IsPathRooted(args[i+1]))throw new ArgumentException("Opening evidence needs an absolute output path.");
            var owner=VectorBootstrap.Instance;if(!owner)return;
            var e=owner.gameObject.AddComponent<OpeningCityEvidence>();e.owner=owner;e.folder=args[i+1];
            e.stills=Array.IndexOf(args,"-openingStills")>=0;e.performance=performance;
            e.hideHud=!performance&&Array.IndexOf(args,"-openingHideHud")>=0;
            if(Array.IndexOf(args,"-motionReducedEvidence")>=0) PlayerPreferences.Current.SetReducedInterfaceMotion(true);
            if(e.hideHud)
            {
                // Clean native backgrounds are an evidence-only diagnostic.
                // A normal launch, or this flag alone, never disables the HUD.
                foreach(var hud in FindObjectsByType<RaceHUD>(FindObjectsSortMode.None))hud.enabled=false;
                Debug.Log("VR_OPENING_EVIDENCE HUD hidden for reference capture");
            }
            foreach(var old in owner.GetComponents<RaceEvidence>()){old.StopAllCoroutines();old.enabled=false;}
            e.StartCoroutine(e.Run());
        }
        IEnumerator Run()
        {
            if(performance){yield return Performance();yield break;}
            Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"frames"));
            var report=new Report{buildGuid=Application.buildGUID,revision=owner.productionWorld?owner.productionWorld.artRevision:OpeningCity.Revision,
                openingEnabled=OpeningCity.Enabled,hudHidden=hideHud,reducedMotion=PlayerPreferences.Current.ReducedInterfaceMotion,width=Screen.width,height=Screen.height,
                scope="18 simulation seconds of the first lap from progress .005; actual native ChaseCamera, normal hover physics, opt-in test autopilot. 24 Hz simulation-time PNG sequence without audio, not real-time performance or human driving. All stills and continuous frames use natural physics/camera motion; comparison is checked from recorded poses, never injected."};
            var d=owner.Director;d.Player.AutopilotForTesting=true;Time.captureFramerate=24;Time.timeScale=1;
            for(int n=0;n<48;n++)yield return null;
            yield return new WaitForFixedUpdate();d.StartRace();
            float deadline=Time.realtimeSinceStartup+180;
            while((d.Phase!=RacePhase.Racing||d.Player.TrackProgress<.005f||d.Player.TrackProgress>.10f)&&Time.realtimeSinceStartup<deadline)yield return null;
            if(d.Player.TrackProgress>.10f)throw new InvalidOperationException("Did not reach first-lap capture start.");
            int next=0;
            for(int frame=0;frame<432;frame++)
            {
                yield return new WaitForEndOfFrame();
                var chase=owner.Camera.GetComponent<ChaseCamera>();
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"-motionViewEvidence")>=0 && chase)
                    chase.LookBackRequested=frame>=96 && frame<132;
                var pose=Read(frame);
                if(next<3&&pose.progress>=anchors[next])
                {
                    pose.anchor=names[next++];
                    if(stills)ScreenCapture.CaptureScreenshot(Path.Combine(folder,pose.anchor+".png"));
                }
                report.frames.Add(pose);
                if(!stills)ScreenCapture.CaptureScreenshot(Path.Combine(folder,pose.file));
            }
            yield return new WaitForSecondsRealtime(2);
            report.complete=next==3;
            if(!stills)
            {
                // Unity honors one screenshot request per frame. Reuse that exact
                // saved native frame for the named anchor instead of requesting twice.
                foreach(var pose in report.frames)
                {
                    string source=Path.Combine(folder,pose.file);
                    if(!File.Exists(source)){report.complete=false;continue;}
                    if(!string.IsNullOrEmpty(pose.anchor))
                        File.Copy(source,Path.Combine(folder,pose.anchor+".png"),true);
                }
            }
            foreach(var name in names)if(!File.Exists(Path.Combine(folder,name+".png")))report.complete=false;
            File.WriteAllText(Path.Combine(folder,"opening-evidence.json"),JsonUtility.ToJson(report,true));
            Application.Quit(report.complete?0:1);
        }
        [Serializable] sealed class PerformanceReport
        {
            public string buildGuid,gpu,scope;
            public bool openingEnabled;
            public int width,height,samples,renderers;
            public float meanMs,p50Ms,p95Ms,p99Ms,startProgress,endProgress;
        }
        IEnumerator Performance()
        {
            Directory.CreateDirectory(folder);
            var d=owner.Director;d.Player.AutopilotForTesting=true;Time.captureFramerate=0;Time.timeScale=1;
            yield return new WaitForSecondsRealtime(3);
            d.StartRace();
            while(d.Phase!=RacePhase.Racing||d.Player.TrackProgress<.005f||d.Player.TrackProgress>.1f)yield return null;
            var r=new PerformanceReport{buildGuid=Application.buildGUID,gpu=SystemInfo.graphicsDeviceName,
                openingEnabled=OpeningCity.Enabled,width=Screen.width,height=Screen.height,
                renderers=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length,startProgress=d.Player.TrackProgress,
                scope="Isolated native real-time first-lap opening, 18 seconds after .005 progress, test autopilot, warmed title. No screenshots or encoder. Delivered CPU frame intervals, not isolated GPU timings or human feel."};
            var times=new List<float>();float start=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup-start<18){yield return null;times.Add(Time.unscaledDeltaTime*1000);}
            r.endProgress=d.Player.TrackProgress;r.samples=times.Count;
            foreach(float value in times)r.meanMs+=value;r.meanMs/=times.Count;
            times.Sort();r.p50Ms=times[times.Count/2];r.p95Ms=times[(int)(times.Count*.95f)];r.p99Ms=times[(int)(times.Count*.99f)];
            File.WriteAllText(Path.Combine(folder,"performance.json"),JsonUtility.ToJson(r,true));Application.Quit();
        }
        Pose Read(int index)
        {
            var camera=owner.Camera;var racers=owner.Director.Racers;
            var p=new Pose{file="frames/frame-"+index.ToString("D4")+".png",time=owner.Director.RaceTime,
                progress=owner.Director.Player.TrackProgress,speed=owner.Director.Player.SpeedKph,
                viewBlend=camera.GetComponent<ChaseCamera>()?.ViewBlend??0,
                blurIntensity=camera.GetComponent<SpeedMotionBlur>()?.CurrentIntensity??0,
                fov=camera.fieldOfView,cameraPosition=camera.transform.position,cameraRotation=camera.transform.rotation,
                racerPositions=new Vector3[racers.Count],racerRotations=new Quaternion[racers.Count],
                visualPositions=new Vector3[racers.Count],visualRotations=new Quaternion[racers.Count]};
            for(int i=0;i<racers.Count;i++)
            {
                p.racerPositions[i]=racers[i].transform.position;p.racerRotations[i]=racers[i].transform.rotation;
                p.visualPositions[i]=racers[i].VisualRoot.position;p.visualRotations[i]=racers[i].VisualRoot.rotation;
            }
            return p;
        }
    }
}
