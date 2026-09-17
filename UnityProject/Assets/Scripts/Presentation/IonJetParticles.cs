using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    // Independently moving world-space particles extend the original blue plume.
    // A private random stream never changes racing/AI randomness.
    public sealed class IonJetParticles : MonoBehaviour
    {
        ParticleSystem particles;
        Material material;
        Transform nozzle;
        System.Random random;
        float radius, credit;
        bool player, initialized;
        Vector3 previousPosition;
        public int LiveParticles => particles ? particles.particleCount : 0;
        public bool Paused => particles && particles.isPaused;
        public void Initialize(Transform anchor,float nozzleRadius,bool isPlayer)
        {
            nozzle=anchor;radius=nozzleRadius;player=isPlayer;
            random=new System.Random(731+(int)(anchor.localPosition.x*913)+(int)(nozzleRadius*1200));
            Shader shader=Resources.Load<Shader>("Shaders/IonWakeParticle");
            if(!shader){Debug.LogError("Blue exhaust particle shader missing");return;}
            material=new Material(shader){name="Soft blue turbulent exhaust particles"};
            particles=gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=true;main.playOnAwake=false;main.duration=1;
            main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=player?384:96;
            main.startSpeed=0;main.gravityModifier=0;main.scalingMode=ParticleSystemScalingMode.Shape;
            main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
            var emission=particles.emission;emission.enabled=false;
            var shape=particles.shape;shape.enabled=false;
            var color=particles.colorOverLifetime;color.enabled=true;
            var gradient=new Gradient();
            gradient.SetKeys(new[]{new GradientColorKey(new Color(.72f,.94f,1),0),new GradientColorKey(new Color(.06f,.58f,1),.35f),new GradientColorKey(new Color(.025f,.22f,.72f),1)},
                new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.70f,.08f),new GradientAlphaKey(.45f,.28f),new GradientAlphaKey(.12f,.65f),new GradientAlphaKey(0,1)});
            color.color=gradient;
            var size=particles.sizeOverLifetime;size.enabled=true;
            size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.45f),new Keyframe(.2f,1),new Keyframe(.7f,1.35f),new Keyframe(1,.4f)));
            var noise=particles.noise;noise.enabled=true;noise.strength=new ParticleSystem.MinMaxCurve(.45f);
            noise.frequency=1.8f;noise.scrollSpeed=3.2f;noise.damping=true;noise.octaveCount=2;
            var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;
            renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=1.8f;
            renderer.velocityScale=.015f;renderer.cameraVelocityScale=0;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            particles.useAutoRandomSeed=false;particles.randomSeed=(uint)random.Next(1,int.MaxValue);
            particles.Play();
        }
        public void ResetTrail()
        {
            if(particles)particles.Clear(true);
            credit=0;initialized=false;
        }
        public void SetPaused(bool pause)
        {
            if(!particles)return;
            if(pause&&!particles.isPaused)particles.Pause(true);
            else if(!pause&&particles.isPaused)particles.Play(true);
        }
        float Between(float low,float high) {return Mathf.Lerp(low,high,(float)random.NextDouble());}
        public void Step(float dt,float thrust,float boost,float ignition,Vector3 velocity)
        {
            if(!particles||dt<=0||particles.isPaused)return;
            Vector3 position=nozzle.position;
            if(initialized&&Vector3.Distance(position,previousPosition)>25)ResetTrail();
            float scale=radius<.3f?0f:1f;
            if(thrust>.015f)
            {
                float rate=(player?170f:50f)+(player?360f:90f)*boost+ignition*160;
                credit+=dt*rate*thrust*scale;
                int count=Mathf.Min(64,Mathf.FloorToInt(credit));credit-=count;
                for(int i=0;i<count;i++)
                {
                    float t=(i+.5f)/Mathf.Max(count,1);
                    Vector3 source=initialized?Vector3.Lerp(previousPosition,position,t):position;
                    float angle=Between(0,Mathf.PI*2),offset=radius*Between(.04f,.48f);
                    Vector3 radial=nozzle.right*Mathf.Cos(angle)+nozzle.up*Mathf.Sin(angle);
                    float life=Between(.10f,.16f)+boost*Between(.07f,.11f);
                    var emit=new ParticleSystem.EmitParams {
                        position=source+radial*offset-nozzle.forward*radius*Between(.15f,1.1f),
                        velocity=velocity*.42f-nozzle.forward*Between(19f+boost*8,30f+boost*12)
                            +radial*Between(1.0f,3.6f+boost*1.5f),
                        startLifetime=life,startSize=radius*Between(.22f,.46f)*(1+boost*.45f+ignition*.22f),
                        rotation=Between(0,360),startColor=new Color(.52f,.88f,1f,Between(.30f,.65f)*(1+boost*.35f))
                    };
                    particles.Emit(emit,1);
                }
            }
            else credit=0;
            previousPosition=position;initialized=true;
        }
        void OnDestroy()
        {
            if(material){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
        }
    }
}
