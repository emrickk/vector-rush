using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Presentation only: short world-space ions, contact sparks and road-facing suspension light.</summary>
    public sealed class VehicleVFX : MonoBehaviour
    {
        HoverVehicle vehicle;
        ParticleSystem ions;
        Material particleMaterial;
        readonly Light[] suspension = new Light[2];
        float emissionCredit, collisionCooldown;
        int lastRecovery;
        RacePhase previousPhase;

        public void Initialize(HoverVehicle craft)
        {
            if (vehicle) return;
            vehicle = craft;
            Shader shader = Resources.Load<Shader>("Shaders/IonPlume");
            if (!shader) { enabled = false; return; }
            particleMaterial = new Material(shader) { name = "Small translucent ion motes and contact sparks" };
            particleMaterial.SetFloat("_Mode",3); particleMaterial.SetFloat("_Intensity",2.1f); particleMaterial.SetColor("_Tint",Color.white);
            var emitter = new GameObject("World space ion particles"); emitter.transform.SetParent(transform,false); ions = emitter.AddComponent<ParticleSystem>();
            emitter.layer = gameObject.layer;
            ions.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ions.main; main.loop = true; main.playOnAwake = false; main.duration = 1; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = craft.IsPlayer ? 96 : 28; main.startLifetime = .23f; main.startSpeed = 0; main.startSize = .045f; main.gravityModifier = 0;
            var emission = ions.emission; emission.enabled = false; var shape = ions.shape; shape.enabled = false;
            var fade = ions.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1) },new[] { new GradientAlphaKey(0,0),new GradientAlphaKey(1,.08f),new GradientAlphaKey(.55f,.35f),new GradientAlphaKey(0,1) });
            fade.color = gradient;
            var size = ions.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(1,.1f)));
            var renderer = ions.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = particleMaterial; renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = .75f; renderer.velocityScale = .007f; renderer.cameraVelocityScale = 0; renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off; renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            ions.Play();
            // Only the viewed craft gets real suspension lights; rivals retain their small emissive engines.
            if (craft.IsPlayer)
                for (int i=0;i<2;i++)
                {
                    var pool = new GameObject(i == 0 ? "Port suspension light pool" : "Starboard suspension light pool"); pool.transform.SetParent(transform,false);
                    pool.transform.localPosition = new Vector3(i == 0 ? -1.35f : 1.35f,-.5f,.1f);
                    pool.transform.localRotation = Quaternion.LookRotation(Vector3.down,Vector3.forward);
                    var light = pool.AddComponent<Light>(); suspension[i] = light; light.type = LightType.Spot; light.color = new Color(.035f,.7f,1f);
                    light.range = 4.8f; light.spotAngle = 108f; light.innerSpotAngle = 76f; light.shadows = LightShadows.None; light.intensity = 6f;
                }
            lastRecovery = vehicle.RecoveryCount;
        }

        void LateUpdate()
        {
            if (!vehicle || !ions) return;
            RacePhase phase = RaceDirector.Instance ? RaceDirector.Instance.Phase : RacePhase.Menu;
            if (lastRecovery != vehicle.RecoveryCount || (phase == RacePhase.Countdown && previousPhase != phase))
            { ions.Clear(true); emissionCredit = 0; lastRecovery = vehicle.RecoveryCount; }
            previousPhase = phase;
            if (phase == RacePhase.Paused) return;
            float speed = Mathf.Clamp01(vehicle.SpeedKph / 340f);
            bool boost = RaceDirector.Instance && RaceDirector.Instance.CanSimulate(vehicle) && vehicle.IsBoosting;
            if (boost)
            {
                emissionCredit += Time.deltaTime * (vehicle.IsPlayer ? 35f : 10f);
                int count = Mathf.Min(4,Mathf.FloorToInt(emissionCredit)); emissionCredit -= count;
                for(int i=0;i<count;i++) { EmitIon(-1); EmitIon(1); }
            }
            else emissionCredit = 0;
            float grounded = RaceDirector.Instance && RaceDirector.Instance.CanSimulate(vehicle) ? (vehicle.IsGrounded ? 1f : .15f) : .75f;
            float target = (boost ? 15f : 7f + speed*3f) * grounded;
            for(int i=0;i<suspension.Length;i++)
                if(suspension[i]) suspension[i].intensity = Mathf.Lerp(suspension[i].intensity,target,1f-Mathf.Exp(-7f*Time.deltaTime));
        }

        void EmitIon(int side)
        {
            Transform visual = vehicle.VisualRoot ? vehicle.VisualRoot : transform;
            Vector3 origin = visual.TransformPoint(new Vector3(side*1.68f,-.035f,-3.65f));
            Vector3 drift = vehicle.Body ? vehicle.Body.linearVelocity*.84f : Vector3.zero;
            var particle = new ParticleSystem.EmitParams
            {
                position = origin + visual.right*Random.Range(-.10f,.10f),
                velocity = drift - visual.forward*Random.Range(8f,15f) + visual.right*Random.Range(-.7f,.7f) + visual.up*Random.Range(-.4f,.4f),
                startLifetime = Random.Range(.10f,.18f), startSize = Random.Range(.025f,.048f),
                startColor = new Color(.30f,.78f,1f,.62f)
            };
            ions.Emit(particle,1);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!vehicle || !ions || !RaceDirector.Instance || RaceDirector.Instance.Phase != RacePhase.Racing || Time.time < collisionCooldown || collision.relativeVelocity.magnitude < 4f || collision.contactCount == 0) return;
            collisionCooldown = Time.time + .12f;
            var contact = collision.GetContact(0);
            int count = Mathf.Clamp(Mathf.RoundToInt(collision.relativeVelocity.magnitude*.45f),3,vehicle.IsPlayer ? 18 : 7);
            Vector3 drift = vehicle.Body ? vehicle.Body.linearVelocity*.5f : Vector3.zero;
            for(int i=0;i<count;i++)
            {
                var particle = new ParticleSystem.EmitParams
                {
                    position = contact.point + contact.normal*.08f,
                    velocity = drift + contact.normal*Random.Range(2f,6f) + Vector3.up*Random.Range(1f,5f) + Random.insideUnitSphere*3f,
                    startLifetime = Random.Range(.10f,.28f), startSize = Random.Range(.022f,.048f),
                    startColor = new Color(1f,.40f,.09f,.9f)
                };
                ions.Emit(particle,1);
            }
        }
        void OnDestroy() { if (particleMaterial) Destroy(particleMaterial); }
    }
}
