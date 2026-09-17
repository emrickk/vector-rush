using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Three bounded particle layers: attached core, turbulent body, and world-space wake.</summary>
    public sealed class IonFlameParticles : MonoBehaviour
    {
        enum FlameLayer { Core, Body, Wake }
        ParticleSystem core, flame, wake;
        Material coreMaterial, flameMaterial, wakeMaterial;
        Transform nozzle;
        System.Random random;
        float radius, flameCredit, wakeCredit;
        float coreCredit;
        bool player, initialized, stage1;
        Vector3 previousPosition;
        public int LiveParticles => (core ? core.particleCount : 0) + (flame ? flame.particleCount : 0) + (wake ? wake.particleCount : 0);
        public bool Paused => flame && flame.isPaused;
        public bool UsesExperienceAtlases { get; private set; }

        void Start()
        {
            // Soft intersections need an actual depth texture, independent of other renderer features.
            if (!player || !Camera.main) return;
            var data = Camera.main.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (data) data.requiresDepthTexture = true;
        }

        public void Initialize(Transform anchor, float nozzleRadius, bool isPlayer)
        {
            nozzle = anchor; radius = nozzleRadius; player = isPlayer;
            stage1 = VectorBootstrap.Instance && VectorBootstrap.Instance.productionWorld &&
                VectorBootstrap.Instance.productionWorld.materials.stage1Finish;
            random = new System.Random(731 + (int)(anchor.localPosition.x * 913));
            var shader = Resources.Load<Shader>("Shaders/IonAnimatedFlame");
            var plumeAtlas = Resources.Load<Texture2D>("ExperienceArt/vfx/vrx_vfx_plume_a");
            var coreAtlas = Resources.Load<Texture2D>("ExperienceArt/vfx/vrx_vfx_core_a");
            var wakeAtlas = Resources.Load<Texture2D>("ExperienceArt/vfx/vrx_vfx_wake_a");
            UsesExperienceAtlases = plumeAtlas && coreAtlas && wakeAtlas;
            if (!UsesExperienceAtlases)
            {
                plumeAtlas = coreAtlas = wakeAtlas = Resources.Load<Texture2D>("Exhaust/BlueFlameAtlas");
            }
            if (!shader || !plumeAtlas) { Debug.LogError("Animated exhaust resources missing"); return; }
            int columns = UsesExperienceAtlases ? 4 : 8;
            int rows = UsesExperienceAtlases ? 4 : 8;
            coreMaterial = CreateMaterial(shader, coreAtlas, "Attached white-blue ion core", UsesExperienceAtlases ? 1.15f : .8f);
            flameMaterial = CreateMaterial(shader, plumeAtlas, "Turbulent blue ion body", 1f);
            wakeMaterial = CreateMaterial(shader, wakeAtlas, "Dispersing blue ion wake", .72f);
            if (stage1)
            {
                coreMaterial.SetFloat("_Intensity", 1.65f);
                flameMaterial.SetFloat("_Intensity", 1.55f);
                coreMaterial.SetFloat("_ColorWhitening", .72f);
                flameMaterial.SetFloat("_ColorWhitening", .58f);
                wakeMaterial.SetFloat("_ColorWhitening", .32f);
            }
            core = CreateLayer("Attached animated ion core", FlameLayer.Core, coreMaterial, columns, rows);
            flame = CreateLayer("Directed turbulent flame", FlameLayer.Body, flameMaterial, columns, rows);
            wake = CreateLayer("Dissipating blue filaments", FlameLayer.Wake, wakeMaterial, columns, rows);
        }

        static Material CreateMaterial(Shader shader, Texture2D texture, string label, float intensity)
        {
            var result = new Material(shader) { name = label };
            result.SetTexture("_MainTex", texture);
            result.SetFloat("_UseAtlasColor", texture && texture.name.StartsWith("vrx_vfx_") ? 1f : 0f);
            result.SetFloat("_Intensity", intensity);
            return result;
        }

        ParticleSystem CreateLayer(string label, FlameLayer layer, Material layerMaterial, int columns, int rows)
        {
            bool trailing = layer == FlameLayer.Wake;
            var go = new GameObject(label); go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = player ? (layer == FlameLayer.Core ? (stage1 ? 180 : 128) : trailing ? 96 : (stage1 ? 384 : 256))
                : (layer == FlameLayer.Core ? 40 : trailing ? 24 : 80);
            main.startSpeed = 0; main.gravityModifier = 0;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var emission = ps.emission; emission.enabled = false;
            var shape = ps.shape; shape.enabled = false;
            var animation = ps.textureSheetAnimation; animation.enabled = true;
            animation.numTilesX = columns; animation.numTilesY = rows;
            animation.animation = ParticleSystemAnimationType.WholeSheet;
            animation.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, 1));
            animation.startFrame = new ParticleSystem.MinMaxCurve(0);
            var colors = ps.colorOverLifetime; colors.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] {
                new GradientColorKey(new Color(.65f,.91f,1),0),
                new GradientColorKey(new Color(.08f,.48f,1),.4f),
                new GradientColorKey(new Color(.035f,.30f,.8f),1)
            }, new[] {
                new GradientAlphaKey(.6f,0), new GradientAlphaKey(1,.06f),
                new GradientAlphaKey(.7f,.35f), new GradientAlphaKey(.22f,.72f),
                new GradientAlphaKey(0,1)
            });
            colors.color = gradient;
            var size = ps.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(
                new Keyframe(0,.7f), new Keyframe(.18f,1), new Keyframe(.65f,.8f), new Keyframe(1,.1f)));
            var noise = ps.noise; noise.enabled = true;
            noise.strength = trailing ? .32f : layer == FlameLayer.Body ? .12f : .035f; noise.frequency = 2.4f;
            noise.scrollSpeed = 2; noise.damping = true;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = layerMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = trailing ? 5f : layer == FlameLayer.Body ? 3f : 1.8f;
            renderer.velocityScale = 0; renderer.cameraVelocityScale = 0;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off; renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            ps.useAutoRandomSeed = false; ps.randomSeed = (uint)random.Next(1,int.MaxValue);
            ps.Play(); return ps;
        }

        public void SetPaused(bool pause)
        {
            SetPaused(core,pause); SetPaused(flame,pause); SetPaused(wake,pause);
        }
        static void SetPaused(ParticleSystem ps, bool pause)
        {
            if (!ps) return;
            if (pause && !ps.isPaused) ps.Pause();
            else if (!pause && ps.isPaused) ps.Play();
        }
        public void ResetTrail()
        {
            if (core) core.Clear(); if (flame) flame.Clear(); if (wake) wake.Clear();
            coreCredit = flameCredit = wakeCredit = 0; initialized = false;
        }
        float Between(float a,float b) => Mathf.Lerp(a,b,(float)random.NextDouble());

        public void Step(float dt,float thrust,float boost,float ignition,Vector3 velocity)
        {
            if (!flame || dt <= 0 || Paused) return;
            Vector3 position = nozzle.position;
            if (initialized && Vector3.Distance(position,previousPosition)>25) ResetTrail();
            Emit(core, ref coreCredit, FlameLayer.Core, dt, thrust, boost, ignition, velocity, position);
            Emit(flame, ref flameCredit, FlameLayer.Body, dt, thrust, boost, ignition, velocity, position);
            Emit(wake, ref wakeCredit, FlameLayer.Wake, dt, thrust, boost, ignition, velocity, position);
            previousPosition = position; initialized = true;
        }
        void Emit(ParticleSystem ps, ref float credit, FlameLayer layer, float dt,
            float thrust,float boost,float ignition,Vector3 velocity,Vector3 position)
        {
            if (thrust <= .015f) { credit = 0; return; }
            bool trailing = layer == FlameLayer.Wake;
            float central = radius < .3f ? 0f : 1f;
            float rate = layer == FlameLayer.Core ? (player ? 190 : 55)
                : trailing ? (player ? 65 : 18) : (player ? 300 : 88);
            if (stage1 && !trailing) rate *= 1.35f;
            credit += dt * rate * (1 + boost * .65f) * thrust * central;
            int count = Mathf.Min(80,Mathf.FloorToInt(credit)); credit -= count;
            for (int i=0;i<count;i++)
            {
                float angle = Between(0,Mathf.PI*2);
                Vector3 radial = nozzle.right*Mathf.Cos(angle)+nozzle.up*Mathf.Sin(angle);
                float t = (i+.5f)/Mathf.Max(1,count);
                float speed = layer == FlameLayer.Core ? Between(14,20) + boost * 3
                    : trailing ? Between(24,31) + boost * 8 : Between(25,35) + boost * 10;
                float inheritance = trailing ? .84f : 1f;
                Vector3 flow = velocity * inheritance - nozzle.forward * speed;
                Vector3 source = initialized ? Vector3.Lerp(previousPosition,position,t) + flow * dt * (1-t) : position;
                // Full inheritance limits relative flame travel to exhaust speed * lifetime.
                // Wake retains slightly less craft motion, leaving a gently curved history.
                var emit = new ParticleSystem.EmitParams {
                    position = source + radial * radius * Between(.02f,.3f),
                    velocity = velocity * inheritance - nozzle.forward*speed +
                        radial * Between(.15f,trailing ? 1.5f : .8f),
                    startLifetime = layer == FlameLayer.Core ? Between(.055f,.085f)+boost*.012f
                        : trailing ? Between(.22f,.32f)+boost*.06f : Between(.10f,.16f)+boost*.055f,
                    startSize = radius * (layer == FlameLayer.Core ? (stage1 ? Between(.58f,.76f) : Between(.32f,.48f))
                        : trailing ? Between(.12f,.25f) : (stage1 ? Between(1.10f,1.60f) : Between(.55f,.9f))) *
                        (1+boost*.18f+ignition*.15f),
                    startColor = new Color(1,1,1,layer == FlameLayer.Core ? .85f : trailing ? .24f : .62f),
                    rotation = 0
                };
                ps.Emit(emit,1);
            }
        }
        void OnDestroy()
        {
            DestroyMaterial(coreMaterial); DestroyMaterial(flameMaterial); DestroyMaterial(wakeMaterial);
        }
        static void DestroyMaterial(Material value)
        {
            if (!value) return;
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
