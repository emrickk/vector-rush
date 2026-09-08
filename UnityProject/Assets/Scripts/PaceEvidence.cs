using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Opt-in full-race observation through normal test-autopilot driving.</summary>
    public sealed class PaceEvidence : MonoBehaviour
    {
        const float SampleInterval=.1f, Warmup=5f, Timeout=180f;
        static readonly float[] PictureTimes={2,10,20,30,45,60};
        VectorBootstrap bootstrap;
        string folder;
        bool capture,originalAutopilot,cleaned;
        int simulationRate,originalCaptureRate;
        float originalTimeScale,startedWall,nextSample,lastSampleTime=-1;
        readonly Report report=new Report();
        readonly List<Accumulator> accumulators=new List<Accumulator>();
        StreamWriter telemetry;

        public static bool TryStart(VectorBootstrap owner)
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-paceEvidence");
            if(index<0)return false;
            if(index+1>=args.Length||!Path.IsPathRooted(args[index+1]))
            { Debug.LogError("Pace evidence requires -paceEvidence <absolute output folder>.");Application.Quit(2);return true; }
            if(!owner||!owner.Director||!owner.Director.Player||!owner.Track)
            { Debug.LogError("Pace evidence requires the initialized native race.");Application.Quit(2);return true; }
            bool pictures=Array.IndexOf(args,"-paceCapture")>=0;
            int rate=pictures?24:0,rateIndex=Array.IndexOf(args,"-paceSimulationRate");
            if(Array.IndexOf(args,"-paceSimulationRate24")>=0)rate=24;
            if(rateIndex>=0&&(rateIndex+1>=args.Length||!int.TryParse(args[rateIndex+1],NumberStyles.Integer,CultureInfo.InvariantCulture,out rate)||(rate!=0&&rate!=24)))
            { Debug.LogError("-paceSimulationRate accepts 0 (real time) or 24 (simulation time).");Application.Quit(2);return true; }
            string output;
            try
            {
                output=Path.GetFullPath(args[index+1]);Directory.CreateDirectory(output);
                if(File.Exists(Path.Combine(output,"pace-evidence.json"))||File.Exists(Path.Combine(output,"pace-samples.jsonl")))
                    throw new IOException("Output already contains pace evidence; choose a new folder.");
            }
            catch(Exception error){Debug.LogError("Pace evidence output: "+error.Message);Application.Quit(2);return true;}
            foreach(var legacy in owner.GetComponents<RaceEvidence>()){legacy.StopAllCoroutines();legacy.enabled=false;}
            var runner=owner.gameObject.AddComponent<PaceEvidence>();runner.bootstrap=owner;runner.folder=output;
            runner.capture=pictures;runner.simulationRate=rate;runner.StartCoroutine(runner.Run());return true;
        }

        IEnumerator Run()
        {
            var director=bootstrap.Director;var player=director.Player;
            originalAutopilot=player.AutopilotForTesting;originalCaptureRate=Time.captureFramerate;originalTimeScale=Time.timeScale;
            startedWall=Time.realtimeSinceStartup;
            report.startedUtc=DateTime.UtcNow.ToString("o");report.simulationCaptureRate=simulationRate;report.picturesRequested=capture;
            report.buildGuid=Application.buildGUID;report.gpu=SystemInfo.graphicsDeviceName;report.graphicsApi=SystemInfo.graphicsDeviceType.ToString();report.graphicsDriver=SystemInfo.graphicsDeviceVersion;
            report.scope="Actual full native race through the normal start/countdown and test-autopilot driver. The harness never assigns vehicle pose, velocity, lap/progress, energy, recovery or race phase. Normal driver recoveries remain part of the race and are counted. Samples observe end-of-frame physical state about every 0.1 race seconds. Positive signed race gap means ahead; wrapped track gap is separate from validated lap distance. Euclidean proximity can include another course branch. Finish times for rivals are first-observed sample times; player finish time comes from RaceDirector. Position changes are observed rank changes, not certified overtakes. This is pace/pack evidence, not a performance or subjective racing-quality pass.";
            report.timing=simulationRate==0?"Real-time native run; Time.captureFramerate=0. Sampling and optional image encoding can affect frame cost; no performance claim.":"Time.captureFramerate=24 simulation-time run. It is repeatable stepping, not real-time frame performance; no rendered camera or vehicle pose is injected.";
            report.trackLengthM=bootstrap.Track.Length;report.totalLaps=director.TotalLaps;report.requestedSampleInterval=SampleInterval;
            try
            {
                telemetry=new StreamWriter(Path.Combine(folder,"pace-samples.jsonl"),false);
                player.AutopilotForTesting=true;Time.captureFramerate=simulationRate;
                foreach(var racer in director.Racers)accumulators.Add(new Accumulator(racer?racer.name:"missing racer",racer&&racer.IsPlayer));
                WriteStatus("STARTING");
                // Let normal rendering initialize before the ordinary race start.
                yield return null;
                director.StartRace();float countdownDeadline=Time.realtimeSinceStartup+12f;
                while(director.Phase==RacePhase.Countdown&&Time.realtimeSinceStartup<countdownDeadline)yield return null;
                if(director.Phase!=RacePhase.Racing)Fail("Native race did not enter Racing within the countdown deadline.");
                float racingWall=Time.realtimeSinceStartup;report.racingStartedUtc=DateTime.UtcNow.ToString("o");
                int picture=0;float nextCheckpoint=10;
                while(report.error==null)
                {
                    yield return new WaitForEndOfFrame();
                    bool finished=director.Phase==RacePhase.Finished;
                    if(director.Phase==RacePhase.Menu){Fail("Race returned to Menu before the player finished.");break;}
                    if(director.RaceTime>=nextSample||finished)
                    {
                        Observe();do{nextSample+=SampleInterval;}while(nextSample<=director.RaceTime);
                    }
                    if(capture&&picture<PictureTimes.Length&&director.RaceTime>=PictureTimes[picture])
                    {
                        CaptureMoment(picture,PictureTimes[picture]);picture++;
                    }
                    if(director.RaceTime>=nextCheckpoint)
                    { telemetry.Flush();WriteStatus("RACING");nextCheckpoint=director.RaceTime+10; }
                    if(finished)
                    {
                        report.completed=true;report.playerFinishTime=director.FinishTime;break;
                    }
                    if(director.RaceTime>=Timeout||Time.realtimeSinceStartup-racingWall>=Timeout)
                    { report.timedOut=true;Fail("Player did not finish within 180 race seconds or the 180-second racing wall-clock watchdog.");break; }
                }
            }
            finally
            {
                FinishReport();Cleanup();
                try
                {
                    File.WriteAllText(Path.Combine(folder,"pace-evidence.json"),JsonUtility.ToJson(report,true));
                    WriteStatus(report.completed&&report.error==null?"COMPLETE":"FAIL / INCOMPLETE");
                }
                catch(Exception error){Debug.LogError("Could not write pace report: "+error.Message);report.error=error.Message;}
            }
            yield return null;
            Application.Quit(report.completed&&report.error==null?0:1);
        }

        void Observe()
        {
            var director=bootstrap.Director;float now=director.RaceTime;
            if(lastSampleTime>=0&&now<=lastSampleTime)return;
            var sample=new Sample{raceTime=now,wallSeconds=Time.realtimeSinceStartup-startedWall,phase=director.Phase.ToString(),officialPlayerPosition=director.Position};
            var racers=director.Racers;var player=director.Player;float length=bootstrap.Track.Length;
            for(int i=0;i<racers.Count;i++)
            {
                var racer=racers[i];
                if(!racer||!racer.Body){Fail("A racer or Rigidbody disappeared during pace evidence.");return;}
                var tracker=racer.ProgressTracker;var frame=bootstrap.Track.Evaluate(racer.TrackProgress);
                var state=new RacerSample {
                    index=i,name=racer.name,isPlayer=racer.IsPlayer,position=racer.Body.position,rotation=racer.Body.rotation,velocity=racer.Body.linearVelocity,
                    trackProgress=racer.TrackProgress,raceProgress=racer.RaceProgress,speedKph=racer.SpeedKph,cruiseSpeedSettingMps=racer.CruiseSpeed,boostSpeedSettingMps=racer.BoostSpeed,
                    completedLaps=tracker!=null?tracker.CompletedLaps:0,nextCheckpoint=tracker!=null?tracker.NextCheckpoint:0,
                    hasStarted=tracker!=null&&tracker.HasStarted,grounded=racer.IsGrounded,boost=racer.IsBoosting,energy01=racer.Boost01,
                    recoveries=racer.RecoveryCount,throttle=racer.ThrottleInput,corridorGuard=racer.AICorridorGuardActive,
                    aiDesiredSpeedMps=racer.AIDesiredSpeed,aiBrake=racer.AIBrakeInput,aiTargetLane=racer.AITargetLane,aiResolvedTargetLane=racer.AIResolvedTargetLane,
                    lateralOffsetM=Vector3.Dot(racer.Body.position-frame.Position,frame.Right),heightAboveTrackM=Vector3.Dot(racer.Body.position-frame.Position,frame.Up),
                    raceGapToPlayerM=(racer.RaceProgress-player.RaceProgress)*length,
                    wrappedTrackGapToPlayerM=(Mathf.Repeat(racer.TrackProgress-player.TrackProgress+.5f,1)-.5f)*length,
                    distanceToPlayerM=Vector3.Distance(racer.Body.position,player.Body.position),
                    nearestOtherM=-1,nearestOtherIndex=-1,nearestWrappedTrackGapM=-1,nearestTrackIndex=-1,progressRank=1
                };
                for(int j=0;j<racers.Count;j++)
                {
                    var other=racers[j];if(j==i||!other||!other.Body)continue;
                    float distance=Vector3.Distance(racer.Body.position,other.Body.position);
                    if(state.nearestOtherM<0||distance<state.nearestOtherM){state.nearestOtherM=distance;state.nearestOtherIndex=j;}
                    float along=Mathf.Abs((Mathf.Repeat(other.TrackProgress-racer.TrackProgress+.5f,1)-.5f)*length);
                    if(state.nearestWrappedTrackGapM<0||along<state.nearestWrappedTrackGapM){state.nearestWrappedTrackGapM=along;state.nearestTrackIndex=j;}
                    if(other.RaceProgress>racer.RaceProgress+.00001f||(Mathf.Abs(other.RaceProgress-racer.RaceProgress)<=.00001f&&j<i))state.progressRank++;
                }
                sample.racers.Add(state);accumulators[i].Add(state,now,director.TotalLaps);
            }
            if(lastSampleTime>=0)report.maxObservedSampleInterval=Mathf.Max(report.maxObservedSampleInterval,now-lastSampleTime);
            if(report.sampleCount>0&&sample.officialPlayerPosition!=report.lastPlayerPosition)report.playerOfficialPositionChanges++;
            report.lastPlayerPosition=sample.officialPlayerPosition;report.sampleCount++;report.lastRaceTime=now;lastSampleTime=now;
            report.samples.Add(sample);telemetry.WriteLine(JsonUtility.ToJson(sample));
        }

        void CaptureMoment(int index,float requestedTime)
        {
            Texture2D image=null;
            try
            {
                var camera=bootstrap.Camera;
                var view=new View{file="pack-"+(index+1).ToString("D2")+"-"+requestedTime.ToString("000",CultureInfo.InvariantCulture)+"s.png",requestedRaceTime=requestedTime,actualRaceTime=bootstrap.Director.RaceTime,width=Screen.width,height=Screen.height,
                    playerPosition=bootstrap.Director.Player.Body.position,cameraPosition=camera.transform.position,cameraRotation=camera.transform.rotation,fieldOfView=camera.fieldOfView,sampleIndex=report.sampleCount-1};
                image=ScreenCapture.CaptureScreenshotAsTexture();
                if(!image)throw new IOException("Native screen capture returned no image.");
                File.WriteAllBytes(Path.Combine(folder,view.file),image.EncodeToPNG());report.views.Add(view);
            }
            catch(Exception error){Fail("Pack screenshot failed: "+error.Message);}
            finally{if(image)Destroy(image);}
        }

        void FinishReport()
        {
            report.finishedUtc=DateTime.UtcNow.ToString("o");report.wallSeconds=Time.realtimeSinceStartup-startedWall;
            if(!report.completed&&report.error==null)report.error="Evidence execution ended before a player finish was observed.";
            float minRemaining=float.PositiveInfinity,maxRemaining=0;
            foreach(var accumulator in accumulators)
            {
                var summary=accumulator.Summarize();report.racers.Add(summary);report.totalRecoveries+=summary.recoveries;
                summary.validatedDistanceToFinishM=Mathf.Max(0,report.totalLaps-summary.finalRaceProgress)*report.trackLengthM;
                if(summary.hasSamples){minRemaining=Mathf.Min(minRemaining,summary.validatedDistanceToFinishM);maxRemaining=Mathf.Max(maxRemaining,summary.validatedDistanceToFinishM);}
                if(report.completed&&!summary.finished)report.unfinishedRacersAtPlayerFinish++;
                if(summary.isPlayer)
                {
                    report.playerNearestUnder40FractionAfter5s=summary.nearestUnder40FractionAfter5s;
                    report.playerNearestUnder80FractionAfter5s=summary.nearestUnder80FractionAfter5s;
                    report.playerNearestGapAfter5s=summary.nearestEuclideanAfter5s;
                }
            }
            report.finishDistanceSpreadM=float.IsInfinity(minRemaining)?0:maxRemaining-minRemaining;
        }

        void WriteStatus(string state)
        {
            File.WriteAllText(Path.Combine(folder,"pace-status.json"),JsonUtility.ToJson(new Status{state=state,error=report.error,raceTime=bootstrap.Director.RaceTime,wallSeconds=Time.realtimeSinceStartup-startedWall,samples=report.sampleCount,pictures=report.views.Count},true));
        }
        void Fail(string message){if(report.error==null)report.error=message;Debug.LogError("Pace evidence: "+message);}
        void Cleanup()
        {
            if(cleaned)return;cleaned=true;
            if(bootstrap&&bootstrap.Director&&bootstrap.Director.Player)bootstrap.Director.Player.AutopilotForTesting=originalAutopilot;
            Time.captureFramerate=originalCaptureRate;Time.timeScale=originalTimeScale;
            if(telemetry!=null){try{telemetry.Flush();telemetry.Dispose();}catch(Exception error){Debug.LogError("Pace telemetry close: "+error.Message);}telemetry=null;}
        }
        void OnDestroy(){Cleanup();}

        sealed class Accumulator
        {
            readonly string name;readonly bool player;
            readonly List<float> nearest=new List<float>(),along=new List<float>(),raceGap=new List<float>();
            RacerSample previous;float previousTime=-1,duration,under40,under80,guard,boost,grounded,speedIntegral;
            int changes,best=int.MaxValue,worst,recoveries;float finish=-1;
            public Accumulator(string name,bool player){this.name=name;this.player=player;}
            public void Add(RacerSample state,float time,int laps)
            {
                if(previous!=null)
                {
                    float dt=Mathf.Max(0,time-Mathf.Max(previousTime,Warmup));
                    if(previousTime>=0&&dt>0)
                    {
                        duration+=dt;if(previous.nearestOtherM>=0&&previous.nearestOtherM<40)under40+=dt;if(previous.nearestOtherM>=0&&previous.nearestOtherM<80)under80+=dt;
                        if(previous.corridorGuard)guard+=dt;if(previous.boost)boost+=dt;if(previous.grounded)grounded+=dt;speedIntegral+=previous.speedKph*dt;
                    }
                    if(state.progressRank!=previous.progressRank)changes++;
                }
                if(time>=Warmup)
                { if(state.nearestOtherM>=0)nearest.Add(state.nearestOtherM);if(state.nearestWrappedTrackGapM>=0)along.Add(state.nearestWrappedTrackGapM);raceGap.Add(state.raceGapToPlayerM); }
                if(finish<0&&state.completedLaps>=laps)finish=time;
                best=Mathf.Min(best,state.progressRank);worst=Mathf.Max(worst,state.progressRank);recoveries=Mathf.Max(recoveries,state.recoveries);previous=state;previousTime=time;
            }
            public RacerSummary Summarize()
            {
                return new RacerSummary{name=name,isPlayer=player,hasSamples=previous!=null,finishObservedAtRaceTime=finish,finished=finish>=0,completedLaps=previous!=null?previous.completedLaps:0,
                    finalTrackProgress=previous!=null?previous.trackProgress:0,finalRaceProgress=previous!=null?previous.raceProgress:0,finalRaceGapToPlayerM=previous!=null?previous.raceGapToPlayerM:0,
                    cruiseSpeedSettingMps=previous!=null?previous.cruiseSpeedSettingMps:0,boostSpeedSettingMps=previous!=null?previous.boostSpeedSettingMps:0,
                    recoveries=recoveries,observedProgressRankChanges=changes,bestObservedProgressRank=best==int.MaxValue?0:best,worstObservedProgressRank=worst,
                    measuredRaceSecondsAfter5s=duration,nearestUnder40FractionAfter5s=Fraction(under40),nearestUnder80FractionAfter5s=Fraction(under80),
                    corridorGuardFractionAfter5s=Fraction(guard),boostFractionAfter5s=Fraction(boost),groundedFractionAfter5s=Fraction(grounded),meanSpeedKphAfter5s=duration>0?speedIntegral/duration:-1,
                    nearestEuclideanAfter5s=Distribution.From(nearest),nearestWrappedTrackGapAfter5s=Distribution.From(along),raceGapToPlayerAfter5s=Distribution.From(raceGap)};
            }
            float Fraction(float seconds)=>duration>0?seconds/duration:-1;
        }

        [Serializable] sealed class Report
        {
            public string scope,timing,startedUtc,racingStartedUtc,finishedUtc,error,buildGuid,gpu,graphicsApi,graphicsDriver;
            public bool completed,timedOut,picturesRequested;
            public int simulationCaptureRate,totalLaps,sampleCount,lastPlayerPosition,playerOfficialPositionChanges,totalRecoveries,unfinishedRacersAtPlayerFinish;
            public float trackLengthM,requestedSampleInterval,maxObservedSampleInterval,lastRaceTime,playerFinishTime,wallSeconds,finishDistanceSpreadM;
            public float playerNearestUnder40FractionAfter5s=-1,playerNearestUnder80FractionAfter5s=-1;
            public Distribution playerNearestGapAfter5s;
            public List<RacerSummary> racers=new List<RacerSummary>();public List<View> views=new List<View>();public List<Sample> samples=new List<Sample>();
        }
        [Serializable] sealed class Sample
        {
            public float raceTime,wallSeconds;public string phase;public int officialPlayerPosition;
            public List<RacerSample> racers=new List<RacerSample>();
        }
        [Serializable] sealed class RacerSample
        {
            public int index,completedLaps,nextCheckpoint,recoveries,progressRank,nearestOtherIndex,nearestTrackIndex;
            public string name;public bool isPlayer,hasStarted,grounded,boost,corridorGuard;
            public Vector3 position,velocity;public Quaternion rotation;
            public float trackProgress,raceProgress,speedKph,cruiseSpeedSettingMps,boostSpeedSettingMps,energy01,throttle,aiDesiredSpeedMps,aiBrake,aiTargetLane,aiResolvedTargetLane,lateralOffsetM,heightAboveTrackM;
            public float raceGapToPlayerM,wrappedTrackGapToPlayerM,distanceToPlayerM,nearestOtherM,nearestWrappedTrackGapM;
        }
        [Serializable] sealed class RacerSummary
        {
            public string name;public bool isPlayer,hasSamples,finished;
            public int completedLaps,recoveries,observedProgressRankChanges,bestObservedProgressRank,worstObservedProgressRank;
            public float finishObservedAtRaceTime,finalTrackProgress,finalRaceProgress,finalRaceGapToPlayerM,measuredRaceSecondsAfter5s,validatedDistanceToFinishM,cruiseSpeedSettingMps,boostSpeedSettingMps;
            public float nearestUnder40FractionAfter5s,nearestUnder80FractionAfter5s,corridorGuardFractionAfter5s,boostFractionAfter5s,groundedFractionAfter5s,meanSpeedKphAfter5s;
            public Distribution nearestEuclideanAfter5s,nearestWrappedTrackGapAfter5s,raceGapToPlayerAfter5s;
        }
        [Serializable] sealed class Distribution
        {
            public int count;public float minimum,p10,median,p90,maximum,mean;
            public static Distribution From(List<float> values)
            {
                var result=new Distribution{count=values.Count};if(values.Count==0)return result;
                values.Sort();float sum=0;foreach(float value in values)sum+=value;
                result.minimum=values[0];result.maximum=values[values.Count-1];result.mean=sum/values.Count;
                result.p10=Quantile(values,.1f);result.median=Quantile(values,.5f);result.p90=Quantile(values,.9f);return result;
            }
            static float Quantile(List<float> values,float q){float p=(values.Count-1)*q;int a=Mathf.FloorToInt(p),b=Mathf.Min(a+1,values.Count-1);return Mathf.Lerp(values[a],values[b],p-a);}
        }
        [Serializable] sealed class View
        {
            public string file;public float requestedRaceTime,actualRaceTime,fieldOfView;public int width,height,sampleIndex;
            public Vector3 playerPosition,cameraPosition;public Quaternion cameraRotation;
        }
        [Serializable] sealed class Status { public string state,error;public float raceTime,wallSeconds;public int samples,pictures; }
    }
}
