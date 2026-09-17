using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    // Presentation only. Uses a private random stream and never writes vehicle physics.
    [DefaultExecutionOrder(100)]
    public sealed class RainPresentation : MonoBehaviour
    {
        public AudioClip rainLoop;
        public RainShelterProfile shelterProfile;
        public float PlayerWetness { get; private set; } = 1;
        readonly ParticleSystem.Particle[] particleBuffer=new ParticleSystem.Particle[2600];
        float nextCull,roofDistance=-100;
        bool cameraCovered;
        int culledRain,culledSpray;
        public float emissionPerSecond=1750;
        public float OutdoorExposure { get; private set; }
        public int RainCount => rain ? rain.particleCount : 0;
        public int SprayCount => spray ? spray.particleCount : 0;
        public float RainVolume => sound ? sound.volume : 0;
        VectorBootstrap owner;
        ParticleSystem rain,spray;
        Material rainMaterial,sprayMaterial;
        AudioSource sound;
        AudioLowPassFilter lowPass;
        readonly System.Random random=new System.Random(92173);
        float credit,sprayCredit,nextShelter;
        Vector3 previousCamera;
        bool initialized;
        string evidence;float nextEvidence;
        const int EnvironmentMask=~(1<<8);

        public static bool IsSheltered(Vector3 point)
            => Physics.Raycast(point+Vector3.up*.15f,Vector3.up,100,EnvironmentMask,QueryTriggerInteraction.Ignore);
        public static float AudioLevel(float exposure,float effects,bool paused)
            => paused?0:Mathf.Lerp(.025f,.19f,Mathf.Clamp01(exposure))*Mathf.Clamp01(effects);
        float Range(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
        void Start()
        {
            owner=VectorBootstrap.Instance;if(!owner){enabled=false;return;}
            var shader=Resources.Load<Shader>("Shaders/RainParticle");
            if(!shader){Debug.LogError("Rain shader missing");enabled=false;return;}
            rainMaterial=new Material(shader){name="Fine silver rain"};
            sprayMaterial=new Material(shader){name="Low road spray"};sprayMaterial.SetFloat("_Mist",1);
            rain=Create("Camera rain volume",rainMaterial,2600,false);
            spray=Create("Ship road spray",sprayMaterial,700,true);
            sound=gameObject.AddComponent<AudioSource>();sound.clip=rainLoop;sound.loop=true;sound.playOnAwake=false;sound.spatialBlend=0;sound.volume=0;
            lowPass=gameObject.AddComponent<AudioLowPassFilter>();lowPass.cutoffFrequency=7500;
            if(rainLoop)sound.Play();
            OutdoorExposure=IsSheltered(owner.Camera.transform.position)?0:1;
            var args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-productionValidation");
            if(flag>=0&&flag+2<args.Length&&args[flag+2]=="preview"){
                evidence=Path.Combine(args[flag+1],"weather.csv");
                File.WriteAllText(evidence,"seconds,progress,speedKph,exposure,rainParticles,sprayParticles,rainVolume,wetness,roofDepth,fogDensity,culledRain,culledSpray\n");
            }
        }
        ParticleSystem Create(string name,Material material,int budget,bool mist)
        {
            var obj=new GameObject(name);obj.transform.SetParent(transform,false);
            var ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=budget;main.startSpeed=0;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
            var color=ps.colorOverLifetime;color.enabled=true;var g=new Gradient();
            g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.65f,.65f),new GradientAlphaKey(0,1)});color.color=g;
            if(mist){var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(1,1.6f)));}
            var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=material;r.renderMode=mist?ParticleSystemRenderMode.Billboard:ParticleSystemRenderMode.Stretch;r.lengthScale=mist?1:10;r.velocityScale=mist?0:.035f;r.cameraVelocityScale=mist?0:.025f;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;
            ps.useAutoRandomSeed=false;ps.randomSeed=mist?193u:917u;ps.Play();return ps;
        }
        void LateUpdate()
        {
            if(!rain||!owner||!owner.Director)return;
            bool paused=Time.timeScale==0;
            if(paused){if(!rain.isPaused){rain.Pause();spray.Pause();}sound.volume=0;return;}
            if(rain.isPaused){rain.Play();spray.Play();}
            float dt=Time.deltaTime;var camera=owner.Camera.transform;
            if(initialized && Vector3.Distance(previousCamera,camera.position)>35){rain.Clear();spray.Clear();credit=0;}
            previousCamera=camera.position;initialized=true;
            if(Time.time>=nextShelter){
                nextShelter=Time.time+.12f;cameraCovered=IsSheltered(camera.position);
                OutdoorExposure=Mathf.MoveTowards(OutdoorExposure,cameraCovered?0:1,.35f);
                if(shelterProfile)roofDistance=shelterProfile.Distance(owner.Track.ClosestProgress(camera.position));
            }
            if(shelterProfile){
                PlayerWetness=shelterProfile.Wetness(owner.Director.Player.TrackProgress);
                float fogTarget=Mathf.Lerp(.0028f,.0055f,RainShelterProfile.FogExposureAtDistance(roofDistance));
                RenderSettings.fogDensity=Mathf.Lerp(RenderSettings.fogDensity,fogTarget,1-Mathf.Exp(-dt*3));
                if(Time.time>=nextCull){nextCull=Time.time+.06f;culledSpray+=CullSheltered(spray);if(cameraCovered)culledRain+=CullSheltered(rain);}
            }
            sound.volume=Mathf.Lerp(sound.volume,(shelterProfile ? .19f*RainShelterProfile.RainAudibilityAtDistance(roofDistance)*PlayerPreferences.Current.EffectsVolume:AudioLevel(OutdoorExposure,PlayerPreferences.Current.EffectsVolume,false)),1-Mathf.Exp(-dt*5));
            lowPass.cutoffFrequency=Mathf.Lerp(900,7500,OutdoorExposure);
            if(evidence!=null&&Time.time>=nextEvidence){nextEvidence=Time.time+.25f;File.AppendAllText(evidence,string.Format(CultureInfo.InvariantCulture,"{0:F3},{1:F5},{2:F2},{3:F3},{4},{5},{6:F4},{7:F3},{8:F2},{9:F5},{10},{11}\n",Time.time,owner.Director.Player.TrackProgress,owner.Director.Player.SpeedKph,OutdoorExposure,RainCount,SprayCount,RainVolume,PlayerWetness,roofDistance,RenderSettings.fogDensity,culledRain,culledSpray));}
            Vector3 forward=Vector3.ProjectOnPlane(camera.forward,Vector3.up).normalized;
            Vector3 right=Vector3.Cross(Vector3.up,forward);
            credit+=dt*emissionPerSecond;int count=Mathf.Min(70,(int)credit);credit-=count;
            for(int i=0;i<count;i++)
            {
                Vector3 p=camera.position+forward*Range(3,48)+right*Range(-26,26)+Vector3.up*Range(-7,15);
                // Reject drops below road level, under an actual roof, or behind walls.
                if(IsSheltered(p))continue;
                Vector3 v=new Vector3(.8f,-26,1.6f);float life=.7f;
                if(Physics.Raycast(p,v.normalized,out var hit,22,EnvironmentMask,QueryTriggerInteraction.Ignore))life=Mathf.Min(life,hit.distance/v.magnitude);
                if(life<.05f)continue;
                rain.Emit(new ParticleSystem.EmitParams{position=p,velocity=v,startLifetime=life,startSize=Range(.026f,.052f),startColor=new Color(.65f,.76f,.82f,Range(.29f,.48f))},1);
            }
            sprayCredit+=dt*28;int amount=Mathf.Min(3,(int)sprayCredit);sprayCredit-=amount;
            foreach(var vehicle in owner.Director.Racers)
            {
                if(Vector3.SqrMagnitude(vehicle.transform.position-camera.position)>10000)continue;
                float speed=Mathf.Clamp01(vehicle.SpeedKph/180f);if(speed<.15f)continue;
                float wetness=shelterProfile?shelterProfile.Wetness(vehicle.TrackProgress):1;if(wetness<=.001f)continue;
                var f=owner.Track.Evaluate(vehicle.TrackProgress);
                for(int i=0;i<amount;i++)
                {
                    Vector3 p=vehicle.transform.position-vehicle.transform.forward*3.2f+vehicle.transform.right*Range(-2.1f,2.1f);
                    // Project onto the real banked road instead of emitting at exhaust height.
                    p-=f.Up*Vector3.Dot(p-f.Position,f.Up);p+=f.Up*.24f;
                    if(shelterProfile&&IsSheltered(p))continue;
                    spray.Emit(new ParticleSystem.EmitParams{position=p,velocity=-vehicle.transform.forward*Range(2,6)+f.Up*Range(.4f,1.1f)+f.Right*Range(-1,1),startLifetime=Range(.28f,.48f),startSize=Range(.35f,.75f)*speed,startColor=new Color(.44f,.59f,.63f,.17f*speed*wetness)},1);
                }
            }
        }
        int CullSheltered(ParticleSystem ps)
        {
            int count=ps.GetParticles(particleBuffer),removed=0;
            for(int i=0;i<count;i++)if(IsSheltered(particleBuffer[i].position)){particleBuffer[i].remainingLifetime=0;removed++;}
            if(removed>0)ps.SetParticles(particleBuffer,count);return removed;
        }
        void OnDestroy(){if(rainMaterial)Destroy(rainMaterial);if(sprayMaterial)Destroy(sprayMaterial);}
    }
}
