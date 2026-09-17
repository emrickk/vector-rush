using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VectorRush
{
    // Opt-in native implementation checks. This never supplies manual-play evidence.
    public sealed class ProductionEvidence : MonoBehaviour
    {
        [Serializable] sealed class RaceResult {public int race;public float playerTime;public int position;public bool frozenResult,pauseFrozen,restartClean;public int[] recoveries;public Finish[] finishers;}
        [Serializable] sealed class Finish {public string racer;public int position;public float time;public bool dnf;}
        [Serializable] sealed class Report {public string buildGuid,unityVersion,scope,status="RUNNING";public List<RaceResult> races=new List<RaceResult>();public float meanMs,p95Ms,p99Ms,maxMs;public int spikesAbove33Ms;}
        [Serializable] sealed class Shot {public string file;public double seconds;public float progress;public Vector3 cameraPosition;public Quaternion cameraRotation;public float fov;}
        [Serializable] sealed class Preview {public string buildGuid,scope;public double startDsp,endDsp;public int sampleRate;public List<Shot> frames=new List<Shot>();public bool complete;}
        string folder,mode;VectorBootstrap owner;readonly List<float> timing=new List<float>();bool measuring;
        public static bool TryStart(VectorBootstrap bootstrap)
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-productionValidation");if(i<0)return false;
            if(i+2>=args.Length||!Path.IsPathRooted(args[i+1]))throw new ArgumentException("-productionValidation <fresh absolute directory> <races|preview|performance>");
            string folder=args[i+1],mode=args[i+2];if(mode!="races"&&mode!="preview"&&mode!="performance")throw new ArgumentException("Unknown validation mode");
            if(Directory.Exists(folder)&&Directory.GetFileSystemEntries(folder).Length>0)throw new IOException("Validation folder must be fresh");Directory.CreateDirectory(folder);
            foreach(var old in bootstrap.GetComponents<RaceEvidence>()){old.StopAllCoroutines();old.enabled=false;}
            var evidence=bootstrap.gameObject.AddComponent<ProductionEvidence>();evidence.owner=bootstrap;evidence.folder=folder;evidence.mode=mode;
            evidence.StartCoroutine(mode=="preview"?evidence.RecordPreview():evidence.RunRaces());return true;
        }
        void Update(){if(measuring)timing.Add(Time.unscaledDeltaTime*1000);}
        IEnumerator RunRaces()
        {
            var report=new Report{buildGuid=Application.buildGUID,unityVersion=Application.unityVersion,scope=mode=="performance"?"Warmed real-time automated physics race; no screenshot/audio/encoder overlap. Frame delivery uses Time.unscaledDeltaTime, not GPU timings.":"Three complete automated native races, post-player finish simulation, pause and restart. No human input or controller hardware tested."};
            var d=owner.Director;d.Player.AutopilotForTesting=true;bool success=false;
            try
            {
                yield return new WaitForSecondsRealtime(3);
                int count=mode=="performance"?1:3;
                for(int race=0;race<count;race++)
                {
                    d.StartRace();bool clean=d.FinishRecords.Count==0&&d.Player.ProgressTracker.CompletedLaps==0;
                    float deadline=Time.realtimeSinceStartup+220;
                    while(d.Phase!=RacePhase.Racing&&Time.realtimeSinceStartup<deadline)yield return null;
                    if(mode=="performance"){yield return new WaitForSecondsRealtime(8);measuring=true;}
                    float nextDiagnostic=0;
                    while(d.Phase!=RacePhase.Finished&&Time.realtimeSinceStartup<deadline)
                    {
                        if(Time.realtimeSinceStartup>=nextDiagnostic)
                        {
                            var player=d.Player;
                            Debug.Log($"PRODUCTION_RACE race={race+1} phase={d.Phase} raceTime={d.RaceTime:F2} scale={Time.timeScale:F2} progress={player.TrackProgress:F5} laps={player.ProgressTracker.CompletedLaps} nextGate={player.ProgressTracker.NextCheckpoint} started={player.ProgressTracker.HasStarted} speed={player.SpeedKph:F1} kinematic={player.Body.isKinematic} autopilot={player.AutopilotForTesting}");
                            nextDiagnostic=Time.realtimeSinceStartup+15;
                        }
                        yield return null;
                    }
                    measuring=false;if(d.Phase!=RacePhase.Finished)throw new InvalidOperationException("Race failed to finish within deadline");
                    float time=d.FinishTime;int position=d.Position;
                    d.TogglePause();int finishCount=d.FinishRecords.Count;yield return new WaitForSecondsRealtime(.5f);
                    bool frozen=d.FinishTime==time&&d.FinishRecords.Count==finishCount&&Time.timeScale==0;d.TogglePause();
                    deadline=Time.realtimeSinceStartup+65;
                    while(d.FinishRecords.Count<d.Racers.Count&&Time.realtimeSinceStartup<deadline)yield return null;
                    if(d.FinishRecords.Count!=d.Racers.Count)throw new InvalidOperationException("Rival finish/timeout did not complete");
                    var result=new RaceResult{race=race+1,playerTime=time,position=position,pauseFrozen=frozen,frozenResult=d.FinishTime==time&&d.Position==position,restartClean=clean,recoveries=new int[d.Racers.Count],finishers=new Finish[d.FinishRecords.Count]};
                    for(int j=0;j<result.recoveries.Length;j++)result.recoveries[j]=d.Racers[j].RecoveryCount;
                    for(int j=0;j<result.finishers.Length;j++){var f=d.FinishRecords[j];result.finishers[j]=new Finish{racer=f.RacerId,position=f.Position,time=f.FinishTime,dnf=f.Status==RacerResultStatus.DidNotFinish};}
                    report.races.Add(result);File.WriteAllText(Path.Combine(folder,"validation.json"),JsonUtility.ToJson(report,true));
                    if(!result.pauseFrozen||!result.frozenResult||!result.restartClean)throw new InvalidOperationException("Race state invariant failed");
                    foreach(var finisher in result.finishers)if(finisher.dnf)throw new InvalidOperationException("Native rival failed to finish; timeout is not a race pass");
                    if(mode!="performance"){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"race-"+(race+1)+"-results.png"));yield return new WaitForSecondsRealtime(.3f);}
                }
                if(timing.Count>0){timing.Sort();float sum=0;foreach(float t in timing){sum+=t;if(t>33.3f)report.spikesAbove33Ms++;}report.meanMs=sum/timing.Count;report.p95Ms=timing[Mathf.FloorToInt((timing.Count-1)*.95f)];report.p99Ms=timing[Mathf.FloorToInt((timing.Count-1)*.99f)];report.maxMs=timing[timing.Count-1];}
                success=true;
            }
            finally{measuring=false;report.status=success?"COMPLETE":"FAILED";File.WriteAllText(Path.Combine(folder,"validation.json"),JsonUtility.ToJson(report,true));Application.Quit(success?0:1);}
        }
        IEnumerator RecordPreview()
        {
            var preview=new Preview{buildGuid=Application.buildGUID,sampleRate=AudioSettings.outputSampleRate,scope="Actual native game, automated steering through ordinary physics, real-time clock (no Time.captureFramerate). PNG captures at approximately 10 Hz with actual DSP timestamps; listener output recorded as stereo float samples. Encoding uses recorded frame times. No performance claim."};
            Directory.CreateDirectory(Path.Combine(folder,"frames"));var d=owner.Director;d.Player.AutopilotForTesting=true;
            var audio=owner.Camera.gameObject.AddComponent<ProductionAudioCapture>();
            var deferredZones=new List<KeyValuePair<string,string>>();
            try
            {
                yield return new WaitForSecondsRealtime(2);d.StartRace();
                while(d.Phase!=RacePhase.Racing||!d.Player.ProgressTracker.HasStarted)yield return null;
                audio.Begin(Path.Combine(folder,"game-audio.f32"));preview.startDsp=AudioSettings.dspTime;
                float[] anchors={.025f,.15f,.28f,.32f,.38f,.49f,.54f,.61f,.70f,.76f,.845f,.955f};int next=0,index=0;double nextShot=preview.startDsp;
                float deadline=Time.realtimeSinceStartup+90;int lap=d.Player.ProgressTracker.CompletedLaps;
                while(d.Player.ProgressTracker.CompletedLaps==lap&&Time.realtimeSinceStartup<deadline)
                {
                    yield return new WaitForEndOfFrame();double now=AudioSettings.dspTime;
                    string capturedFile=null;
                    if(now>=nextShot)
                    {
                        string file="frames/frame-"+(index++).ToString("D5")+".png";
                        ScreenCapture.CaptureScreenshot(Path.Combine(folder,file));
                        capturedFile=file;
                        preview.frames.Add(new Shot{file=file,seconds=now-preview.startDsp,progress=d.Player.TrackProgress,cameraPosition=owner.Camera.transform.position,cameraRotation=owner.Camera.transform.rotation,fov=owner.Camera.fieldOfView});nextShot=now+.1;
                    }
                    if(next<anchors.Length&&d.Player.TrackProgress>=anchors[next]&&d.Player.TrackProgress<.99f)
                    {
                        string zone="zone-"+(next+1).ToString("D2")+".png";
                        if(capturedFile!=null)deferredZones.Add(new KeyValuePair<string,string>(capturedFile,zone));
                        else ScreenCapture.CaptureScreenshot(Path.Combine(folder,zone));
                        next++;
                    }
                    audio.Flush();
                }
                preview.endDsp=AudioSettings.dspTime;audio.End();
                yield return new WaitForSecondsRealtime(2);
                foreach(var zone in deferredZones)
                {
                    string source=Path.Combine(folder,zone.Key),destination=Path.Combine(folder,zone.Value);
                    if(!File.Exists(source))throw new IOException("Deferred route-anchor source screenshot is missing: "+source);
                    File.Copy(source,destination,false);
                }
                preview.complete=d.Player.ProgressTracker.CompletedLaps>lap&&next==anchors.Length;
            }
            finally{audio.End();File.WriteAllText(Path.Combine(folder,"preview.json"),JsonUtility.ToJson(preview,true));Application.Quit(preview.complete?0:1);}
        }
    }
    public sealed class ProductionAudioCapture : MonoBehaviour
    {
        readonly System.Collections.Concurrent.ConcurrentQueue<float[]> pending=new System.Collections.Concurrent.ConcurrentQueue<float[]>();
        volatile bool recording;BinaryWriter writer;
        public void Begin(string path){writer=new BinaryWriter(File.Create(path));recording=true;}
        void OnAudioFilterRead(float[] data,int channels)
        {
            if(!recording)return;var stereo=new float[data.Length/channels*2];
            for(int i=0;i<stereo.Length/2;i++){stereo[i*2]=data[i*channels];stereo[i*2+1]=data[i*channels+Math.Min(1,channels-1)];}pending.Enqueue(stereo);
        }
        public void Flush(){while(pending.TryDequeue(out var data))if(writer!=null)foreach(float value in data)writer.Write(value);}
        public void End(){recording=false;Flush();writer?.Dispose();writer=null;}
        void OnDestroy(){End();}
    }
}
