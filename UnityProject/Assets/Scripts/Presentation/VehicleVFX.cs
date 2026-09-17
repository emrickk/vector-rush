using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Presentation only: short world-space ions, contact sparks and road-facing suspension light.</summary>
    public sealed class VehicleVFX : MonoBehaviour
    {
        HoverVehicle vehicle;
        ParticleSystem ions, sparks, impactFlashes;
        Material particleMaterial, sparkMaterial, impactMaterial;
        readonly Light[] suspension = new Light[2];
        readonly Dictionary<EntityId, ContactState> contacts = new Dictionary<EntityId, ContactState>();
        readonly List<EntityId> staleContacts = new List<EntityId>();
        System.Random random;
        float emissionCredit;
        bool wasBoosting;
        readonly Vector3[] exhaustExits = new Vector3[2];
        int lastRecovery;
        RacePhase previousPhase;

        public event Action<VehicleContactSignal> ContactFeedback;
        public int ActiveContactCount => contacts.Count;

        public void Initialize(HoverVehicle craft)
        {
            if (vehicle) return;
            vehicle = craft;
            random = new System.Random(StableSeed(craft.DisplayName, craft.IsPlayer));
            if (IonPropulsion.Stage2Enabled)
            {
                bool authored = ShipEngineAnchors.TryLoad(out var anchors);
                for (int i = 0; i < exhaustExits.Length; i++)
                    exhaustExits[i] = authored ? anchors[i].Exit + Vector3.back * .18f
                        : new Vector3(i == 0 ? -1.68f : 1.68f, -.035f, -3.65f);
            }
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
            CreateExperienceContactSystems(emitter.transform);
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
            if (phase != RacePhase.Racing && previousPhase == RacePhase.Racing) EndAllContacts();
            if (lastRecovery != vehicle.RecoveryCount || (phase == RacePhase.Countdown && previousPhase != phase))
            {
                EndAllContacts(); ClearTransientParticles(); emissionCredit = 0;
                wasBoosting = false; lastRecovery = vehicle.RecoveryCount;
            }
            if (IonPropulsion.Stage2Enabled)
            {
                if (phase == RacePhase.Menu && previousPhase != phase)
                { EndAllContacts(); ClearTransientParticles(); emissionCredit = 0; wasBoosting = false; }
                // Explicit pause also covers a future pause implementation that does
                // not rely on timeScale; resume preserves existing short-lived ions.
                if (phase == RacePhase.Paused) SetParticlePause(true);
                else SetParticlePause(false);
            }
            previousPhase = phase;
            if (phase == RacePhase.Paused) return;
            ReapStaleContacts();
            float speed = Mathf.Clamp01(vehicle.SpeedKph / 340f);
            bool boost = RaceDirector.Instance && RaceDirector.Instance.CanSimulate(vehicle) && vehicle.IsBoosting;
            if (boost && !(IonPropulsion.Stage2Enabled && IonPropulsion.JetPolishEnabled))
            {
                if (IonPropulsion.Stage2Enabled && !wasBoosting)
                    for (int i = 0; i < (vehicle.IsPlayer ? 5 : 2); i++) { EmitIon(-1); EmitIon(1); }
                emissionCredit += Time.deltaTime * (vehicle.IsPlayer ? (IonPropulsion.Stage2Enabled ? 44f : 35f) : 10f);
                int count = Mathf.Min(4,Mathf.FloorToInt(emissionCredit)); emissionCredit -= count;
                for(int i=0;i<count;i++) { EmitIon(-1); EmitIon(1); }
            }
            else emissionCredit = 0;
            wasBoosting = boost;
            float grounded = RaceDirector.Instance && RaceDirector.Instance.CanSimulate(vehicle) ? (vehicle.IsGrounded ? 1f : .15f) : .75f;
            float target = (boost ? 15f : 7f + speed*3f) * grounded;
            for(int i=0;i<suspension.Length;i++)
                if(suspension[i]) suspension[i].intensity = Mathf.Lerp(suspension[i].intensity,target,1f-Mathf.Exp(-7f*Time.deltaTime));
        }

        void EmitIon(int side)
        {
            Transform visual = vehicle.VisualRoot ? vehicle.VisualRoot : transform;
            Vector3 origin = visual.TransformPoint(IonPropulsion.Stage2Enabled ? exhaustExits[side < 0 ? 0 : 1] : new Vector3(side*1.68f,-.035f,-3.65f));
            if (IonPropulsion.Stage2Enabled)
            {
                // Inherit craft velocity so the trail stays short even at top speed.
                // Independent lateral offsets break up paired ruler-straight streaks.
                Vector3 inherited = vehicle.Body ? vehicle.Body.linearVelocity : Vector3.zero;
                var trail = new ParticleSystem.EmitParams
                {
                    position = origin + visual.right * Between(-.17f, .17f) + visual.up * Between(-.10f, .10f),
                    velocity = inherited - visual.forward * Between(14f, 23f)
                        + visual.right * Between(-1.2f, 1.2f) + visual.up * Between(-.8f, .8f),
                    startLifetime = Between(.10f, .17f), startSize = Between(.045f, .074f),
                    startColor = new Color(.43f, .84f, 1f, .48f)
                };
                ions.Emit(trail, 1);
                return;
            }
            Vector3 drift = vehicle.Body ? vehicle.Body.linearVelocity*.84f : Vector3.zero;
            var particle = new ParticleSystem.EmitParams
            {
                position = origin + visual.right*Between(-.10f,.10f),
                velocity = drift - visual.forward*Between(8f,15f) + visual.right*Between(-.7f,.7f) + visual.up*Between(-.4f,.4f),
                startLifetime = Between(.10f,.18f), startSize = Between(.025f,.048f),
                startColor = new Color(.30f,.78f,1f,.62f)
            };
            ions.Emit(particle,1);
        }

        void CreateExperienceContactSystems(Transform parent)
        {
            var shader = Resources.Load<Shader>("Shaders/IonAnimatedFlame");
            var sparkAtlas = Resources.Load<Texture2D>("ExperienceArt/vfx/vrx_vfx_spark_a");
            var impactAtlas = Resources.Load<Texture2D>("ExperienceArt/vfx/vrx_vfx_impact_a");
            if (!shader || !sparkAtlas || !impactAtlas) return;
            sparkMaterial = ContactMaterial(shader, sparkAtlas, "Artist-authored contact sparks");
            impactMaterial = ContactMaterial(shader, impactAtlas, "Artist-authored contact flash");
            sparks = ContactSystem("Directional contact sparks", parent, sparkMaterial, true, vehicle.IsPlayer ? 96 : 28);
            impactFlashes = ContactSystem("Contact-local impact flashes", parent, impactMaterial, false, vehicle.IsPlayer ? 8 : 3);
        }

        static Material ContactMaterial(Shader shader, Texture2D texture, string label)
        {
            var result = new Material(shader) { name = label };
            result.SetTexture("_MainTex", texture);
            result.SetFloat("_UseAtlasColor", 1f);
            result.SetFloat("_Intensity", 1f);
            return result;
        }

        ParticleSystem ContactSystem(string label, Transform parent, Material material, bool staticVariant, int maximum)
        {
            var go = new GameObject(label); go.layer = gameObject.layer; go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main; main.loop = true; main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = maximum;
            main.startSpeed = 0; main.gravityModifier = 0;
            var emission = system.emission; emission.enabled = false;
            var shape = system.shape; shape.enabled = false;
            var animation = system.textureSheetAnimation; animation.enabled = true;
            animation.numTilesX = 4; animation.numTilesY = 2;
            animation.animation = ParticleSystemAnimationType.WholeSheet;
            animation.frameOverTime = staticVariant ? new ParticleSystem.MinMaxCurve(0)
                : new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0,0,1,1));
            animation.startFrame = staticVariant ? new ParticleSystem.MinMaxCurve(0,1) : new ParticleSystem.MinMaxCurve(0);
            var renderer = system.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
            renderer.renderMode = staticVariant ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            renderer.lengthScale = staticVariant ? 5.5f : 1f; renderer.velocityScale = 0; renderer.cameraVelocityScale = 0;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off; renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            system.useAutoRandomSeed = false; system.randomSeed = (uint)random.Next(1,int.MaxValue);
            system.Play(); return system;
        }

        void ClearTransientParticles()
        {
            ions.Clear(true); if (sparks) sparks.Clear(true); if (impactFlashes) impactFlashes.Clear(true);
        }

        void SetParticlePause(bool pause)
        {
            SetParticlePause(ions,pause); SetParticlePause(sparks,pause); SetParticlePause(impactFlashes,pause);
        }

        static void SetParticlePause(ParticleSystem system, bool pause)
        {
            if (!system) return;
            if (pause && !system.isPaused) system.Pause(true);
            else if (!pause && system.isPaused) system.Play(true);
        }

        void OnCollisionEnter(Collision collision) { ProcessCollision(collision, VehicleContactPhase.Enter); }
        void OnCollisionStay(Collision collision) { ProcessCollision(collision, VehicleContactPhase.Stay); }

        void OnCollisionExit(Collision collision)
        {
            if (!vehicle || collision == null || !collision.collider) return;
            EntityId otherId = collision.collider.GetEntityId();
            if (!contacts.TryGetValue(otherId, out ContactState state)) return;
            Publish(otherId, VehicleContactPhase.Exit, state, new VehicleContactProfile(0f, 0f, false));
            contacts.Remove(otherId);
        }

        void ProcessCollision(Collision collision, VehicleContactPhase phase)
        {
            if (!vehicle || !ions || collision == null || !collision.collider || collision.contactCount == 0 ||
                !RaceDirector.Instance || RaceDirector.Instance.Phase != RacePhase.Racing) return;

            var contact = collision.GetContact(0);
            EntityId otherId = collision.collider.GetEntityId();
            if (!contacts.TryGetValue(otherId, out ContactState state))
            {
                state = new ContactState();
                contacts.Add(otherId, state);
                phase = VehicleContactPhase.Enter;
            }
            if (phase == VehicleContactPhase.Stay) state.duration += Time.fixedDeltaTime;
            state.point = contact.point;
            state.normal = contact.normal;
            Vector3 tangent = Vector3.ProjectOnPlane(collision.relativeVelocity, contact.normal);
            state.tangent = tangent.sqrMagnitude > .0001f ? tangent.normalized : Vector3.zero;
            float normalSpeed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal));
            float mass = vehicle.Body ? Mathf.Max(1f, vehicle.Body.mass) : 850f;
            VehicleContactProfile profile = VehicleContactMath.Evaluate(normalSpeed,
                collision.impulse.magnitude / mass, tangent.magnitude, state.duration);
            state.profile = profile;

            float now = Time.unscaledTime;
            state.lastSeenTime = now;
            bool publish = phase == VehicleContactPhase.Enter || now >= state.nextSignalTime;
            if (publish)
            {
                Publish(otherId, phase, state, profile);
                state.nextSignalTime = now + .08f;
            }
            bool shouldEmit = profile.CombinedIntensity01 > .025f &&
                (phase == VehicleContactPhase.Enter || (profile.IsScraping && now >= state.nextEmissionTime));
            if (shouldEmit)
            {
                EmitContact(state, phase == VehicleContactPhase.Enter);
                state.nextEmissionTime = now + (vehicle.IsPlayer ? .045f : .09f);
            }
        }

        void Publish(EntityId otherId, VehicleContactPhase phase, ContactState state, VehicleContactProfile profile)
        {
            ContactFeedback?.Invoke(new VehicleContactSignal(vehicle, otherId, phase, state.point, state.normal,
                state.tangent, profile, state.duration, Time.unscaledTime));
        }

        void EmitContact(ContactState state, bool initial)
        {
            float strength = state.profile.CombinedIntensity01;
            int maximum = vehicle.IsPlayer ? (initial ? 18 : 7) : (initial ? 7 : 3);
            int count = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(2f, maximum, strength)), 2, maximum);
            Vector3 inherited = vehicle.Body ? vehicle.Body.linearVelocity * .18f : Vector3.zero;
            for(int i=0;i<count;i++)
            {
                Vector3 spread = RandomUnitVector() * Between(.5f, 3.2f);
                Vector3 slide = state.tangent * Between(1.5f, 7f) * Mathf.Lerp(.35f, 1f, state.profile.TangentialIntensity01);
                var particle = new ParticleSystem.EmitParams
                {
                    position = state.point + state.normal * .08f,
                    velocity = inherited + slide + state.normal * Between(1.5f, 6f) + spread,
                    startLifetime = Between(.10f, initial ? .30f : .22f),
                    startSize = Between(.022f, .052f) * Mathf.Lerp(.8f, 1.25f, strength),
                    startColor = sparks ? Color.white :
                        Color.Lerp(new Color(1f,.28f,.035f,.86f), new Color(1f,.86f,.34f,1f), Between(0f,1f))
                };
                (sparks ? sparks : ions).Emit(particle,1);
            }
            if (initial && impactFlashes)
                impactFlashes.Emit(new ParticleSystem.EmitParams {
                    position = state.point + state.normal * .06f,
                    velocity = Vector3.zero,
                    startLifetime = 8f/24f,
                    startSize = Mathf.Lerp(.35f, 1.35f, strength),
                    startColor = Color.white
                },1);
        }

        void EndAllContacts()
        {
            if (contacts.Count == 0) return;
            foreach (var pair in contacts)
                Publish(pair.Key, VehicleContactPhase.Exit, pair.Value, new VehicleContactProfile(0f, 0f, false));
            contacts.Clear();
        }

        void ReapStaleContacts()
        {
            if (contacts.Count == 0) return;
            float now = Time.unscaledTime;
            staleContacts.Clear();
            foreach (var pair in contacts)
                if (now - pair.Value.lastSeenTime > .15f) staleContacts.Add(pair.Key);
            for (int i=0;i<staleContacts.Count;i++)
            {
                EntityId id = staleContacts[i];
                ContactState state = contacts[id];
                Publish(id, VehicleContactPhase.Exit, state, new VehicleContactProfile(0f, 0f, false));
                contacts.Remove(id);
            }
            staleContacts.Clear();
        }

        float Between(float minimum, float maximum) => Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
        Vector3 RandomUnitVector()
        {
            Vector3 value = new Vector3(Between(-1f,1f), Between(-1f,1f), Between(-1f,1f));
            return value.sqrMagnitude > .0001f ? value.normalized : Vector3.up;
        }
        static int StableSeed(string value, bool player)
        {
            unchecked
            {
                int seed = player ? 48611 : 9173;
                if (value != null) for (int i=0;i<value.Length;i++) seed = seed * 31 + value[i];
                return seed;
            }
        }

        void OnDisable()
        {
            EndAllContacts();
            StopAndClear(ions); StopAndClear(sparks); StopAndClear(impactFlashes);
        }
        void OnEnable()
        {
            if (ions && ions.isStopped) ions.Play(true);
            if (sparks && sparks.isStopped) sparks.Play(true);
            if (impactFlashes && impactFlashes.isStopped) impactFlashes.Play(true);
        }
        void OnDestroy()
        {
            EndAllContacts();
            if (particleMaterial) Destroy(particleMaterial);
            if (sparkMaterial) Destroy(sparkMaterial);
            if (impactMaterial) Destroy(impactMaterial);
        }

        static void StopAndClear(ParticleSystem system)
        {
            if (system) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        sealed class ContactState
        {
            public Vector3 point, normal, tangent;
            public float duration, nextSignalTime, nextEmissionTime, lastSeenTime;
            public VehicleContactProfile profile;
        }
    }
}
