using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    public enum PropulsionPhase { Off, Thrust, BoostIgnition, BoostSustain, BoostRelease, Coasting }

    /// <summary>Presentation envelopes observe engine commands, never velocity or input intent.</summary>
    public sealed class PropulsionEnvelope
    {
        public float Response { get; private set; }
        public float Boost { get; private set; }
        public float Ignition { get; private set; }
        public float Release { get; private set; }
        public float FlowTime { get; private set; }
        public PropulsionPhase State { get; private set; }
        bool wasBoosting;

        public void Reset()
        {
            Response = Boost = Ignition = Release = FlowTime = 0f;
            State = PropulsionPhase.Off;
            wasBoosting = false;
        }

        public void Step(float deltaTime, float throttle, bool actualBoost, bool canSimulate, bool paused)
        {
            if (paused) return;
            float dt = Mathf.Max(0f, deltaTime);
            float demand = canSimulate ? Mathf.Clamp01(throttle) : 0f;
            bool boosting = canSimulate && actualBoost && demand > .1f;
            if (boosting && !wasBoosting) Ignition = 1f;
            else Ignition = Mathf.MoveTowards(Ignition, 0f, dt / .18f);
            if (!boosting && wasBoosting) Release = 1f;
            else Release = Mathf.MoveTowards(Release, 0f, dt / .32f);
            if (!boosting) Ignition = 0f;
            float target = demand * (boosting ? 1.35f : 1f);
            Response = Mathf.Lerp(Response, target, 1f - Mathf.Exp(-(target > Response ? 20f : 26f) * dt));
            Boost = Mathf.Lerp(Boost, boosting ? 1f : 0f, 1f - Mathf.Exp(-15f * dt));
            if (Response < .003f) Response = 0f;
            if (Boost < .003f) Boost = 0f;
            FlowTime += dt;
            State = Response == 0f ? PropulsionPhase.Off
                : boosting ? (Ignition > 0f ? PropulsionPhase.BoostIgnition : PropulsionPhase.BoostSustain)
                : Boost > .025f ? PropulsionPhase.BoostRelease
                : demand > .01f ? PropulsionPhase.Thrust : PropulsionPhase.Coasting;
            wasBoosting = boosting;
        }
    }

    /// <summary>Thrust-driven soft plasma at authored apertures. The legacy path remains available for matched evidence.</summary>
    public sealed class IonPropulsion : MonoBehaviour
    {
        public static readonly bool Stage2Enabled = ReadStage2Enabled(System.Environment.GetCommandLineArgs());
        public static readonly bool JetPolishEnabled = ReadJetPolishEnabled(System.Environment.GetCommandLineArgs());
        public static bool ReadJetPolishEnabled(string[] args)
        {
            int i=System.Array.IndexOf(args,"-vrExhaustPolish");
            return i<0 || i+1>=args.Length || !string.Equals(args[i+1],"off",System.StringComparison.OrdinalIgnoreCase);
        }
        public static bool LayeredEnabled = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-vrBlueBaseline") < 0;
        readonly IonFlameParticles[] layeredJets = new IonFlameParticles[3];
        readonly IonJetParticles[] polishedJets = new IonJetParticles[3];
        HoverVehicle vehicle;
        Material plumeMaterial, innerMaterial, coreMaterial, ringMaterial;
        Mesh plumeMesh, ringMesh;
        readonly Transform[] jets = new Transform[3];
        readonly Renderer[] cores = new Renderer[3];
        readonly Renderer[] outerJets = new Renderer[3];
        readonly Renderer[] innerJets = new Renderer[3];
        readonly Renderer[] rings = new Renderer[6];
        readonly Light[] lights = new Light[2];
        readonly float[] jetLengthScales = new float[3];
        MaterialPropertyBlock properties;
        readonly PropulsionEnvelope envelope = new PropulsionEnvelope();
        ShipBoostVisuals boostVisuals;
        RacePhase previousPhase;
        int lastRecovery;
        float response;
        bool stage1Finish;
        public float ExhaustResponse => response;
        public float BoostEnvelope => Stage2Enabled ? envelope.Boost : vehicle && vehicle.IsBoosting ? 1f : 0f;
        public float IgnitionEnvelope => Stage2Enabled ? envelope.Ignition : 0f;
        public PropulsionPhase PropulsionState => envelope.State;
        public float ExhaustDemand => vehicle && RaceDirector.Instance && RaceDirector.Instance.CanSimulate(vehicle)
            ? vehicle.ThrottleInput * (vehicle.IsBoosting ? 1.35f : 1f) : 0f;

        public static bool ReadStage2Enabled(string[] args)
        {
            int index = System.Array.IndexOf(args, "-vrPropulsion");
            return index < 0 || index + 1 >= args.Length || !string.Equals(args[index + 1], "off", System.StringComparison.OrdinalIgnoreCase);
        }

        // Native engine allocations must run in a Unity lifecycle callback.
        void Awake() { properties = new MaterialPropertyBlock(); }

        public void Initialize(HoverVehicle craft)
        {
            if (vehicle) return;
            vehicle = craft;
            stage1Finish = VectorBootstrap.Instance && VectorBootstrap.Instance.productionWorld && VectorBootstrap.Instance.productionWorld.materials.stage1Finish;
            boostVisuals = craft.GetComponent<ShipBoostVisuals>();
            if (!boostVisuals) boostVisuals = craft.gameObject.AddComponent<ShipBoostVisuals>();
            boostVisuals.Initialize(craft);
            var shader = Resources.Load<Shader>(Stage2Enabled ? "Shaders/IonPlumeStage2" : "Shaders/IonPlume");
            if (!shader) { Debug.LogError("Ion Plume shader missing from Resources"); enabled = false; return; }
            plumeMaterial = CreateMaterial(shader, "Soft cyan plasma envelope", 0, Stage2Enabled ? new Color(.035f,.59f,1.05f,.42f) : new Color(.035f,.62f,1.1f,.38f));
            innerMaterial = CreateMaterial(shader, "Soft pale ion spine", 0, Stage2Enabled ? new Color(.76f,.94f,1.08f,.54f) : new Color(.55f,.86f,1.1f,.30f));
            coreMaterial = CreateMaterial(shader, "Recessed pale plasma core", 1, new Color(.65f,.88f,1.05f,.95f));
            ringMaterial = CreateMaterial(shader, "Subtle nozzle rim accents", 2, new Color(.025f,.76f,1.3f,.18f));
            plumeMesh = Stage2Enabled ? CreateFullPlume() : CreatePlume(); ringMesh = CreateRing();
            Transform parent = craft.VisualRoot ? craft.VisualRoot : craft.transform;
            bool authored=ShipEngineAnchors.TryLoad(out var engineAnchors);
            for (int i = 0; i < 3; i++)
            {
                float size = i == 2 ? .35f : 1f;
                var definition=authored?engineAnchors[i]:null;
                float mantleRadius=authored?definition.OpeningRadius*(Stage2Enabled?1.04f:.55f):(Stage2Enabled?.67f:.37f)*size;
                float spineRadius=authored?definition.OpeningRadius*(Stage2Enabled?.50f:.22f):(Stage2Enabled?.31f:.13f)*size;
                jetLengthScales[i]=authored?Mathf.Clamp(definition.OpeningRadius/.509f,.2f,2f):(i==2?.60f:1f);
                var anchor = new GameObject(i == 2 ? "Central ion aperture" : (i == 0 ? "Port ion aperture" : "Starboard ion aperture"));
                anchor.transform.SetParent(parent, false);
                anchor.layer = parent.gameObject.layer;
                anchor.transform.localPosition = authored?definition.Exit:(i == 2 ? new Vector3(0,-.08f,-2.76f) : new Vector3(i == 0 ? -1.68f : 1.68f,-.035f,-3.405f));
                var jet = new GameObject("Variable length thrust volume"); jet.transform.SetParent(anchor.transform, false); jets[i] = jet.transform;
                jet.layer = anchor.layer;
                outerJets[i] = MeshRenderer("Tapered ion mantle", jet.transform, plumeMesh, plumeMaterial, Vector3.zero, new Vector3(mantleRadius,mantleRadius,1));
                innerJets[i] = MeshRenderer("White blue jet spine", jet.transform, plumeMesh, innerMaterial, new Vector3(0,0,-.02f), new Vector3(spineRadius,spineRadius,Stage2Enabled?.53f:.74f));
                var core = GameObject.CreatePrimitive(PrimitiveType.Quad); core.name = "Radiance inside engine throat";
                core.transform.SetParent(anchor.transform,false); core.layer = anchor.layer;
                // Keep the compact radiance at the throat; the outer cavity remains physical and dark.
                core.transform.localPosition = authored?definition.Throat-definition.Exit-Vector3.forward*.012f:new Vector3(0,0,i == 2 ? .315f : .67f);
                core.transform.localScale = Vector3.one*(authored?definition.CompactCoreRadius*2f:.5f*size);
                if(stage1Finish)core.transform.localScale*=1.65f;
                Destroy(core.GetComponent<Collider>()); cores[i] = core.GetComponent<Renderer>(); Configure(cores[i], coreMaterial);
                for (int j = 0; j < 2; j++)
                {
                    float ringDepth=authored?(j==0?.018f:(definition.Throat.z-definition.Exit.z)*.28f):(j==0?.018f:.20f);
                    float ringScale=authored?definition.OpeningRadius*(j==0?.94f:.75f)/.334f:size*(j==0?1f:.8f);
                    rings[i*2+j] = MeshRenderer(j == 0 ? "Aperture light ring" : "Recessed accelerator ring", anchor.transform, ringMesh, ringMaterial, new Vector3(0,0,ringDepth), Vector3.one*ringScale);
                }
                if (craft.IsPlayer && i < 2)
                {
                    var lamp = new GameObject("Nozzle reflected light"); lamp.transform.SetParent(anchor.transform,false); lamp.transform.localPosition = new Vector3(0,-.10f,-.3f);
                    lights[i] = lamp.AddComponent<Light>(); lights[i].type = LightType.Point; lights[i].color = new Color(.12f,.62f,1f); lights[i].range = Stage2Enabled ? 2.35f : 1.55f; lights[i].shadows = LightShadows.None;
                    if(stage1Finish)lights[i].range=6.5f;
                }
            }
            if (Stage2Enabled && JetPolishEnabled)
                for (int i=0;i<3;i++)
                {
                    var go=new GameObject("Blue world-space wake " + i);
                    go.transform.SetParent(craft.transform,false); go.layer=craft.gameObject.layer;
                    float nozzleRadius=authored?engineAnchors[i].OpeningRadius:(i==2?.18f:.509f);
                    if (LayeredEnabled) { layeredJets[i]=go.AddComponent<IonFlameParticles>(); layeredJets[i].Initialize(jets[i].parent,nozzleRadius,craft.IsPlayer); }
                    else { polishedJets[i]=go.AddComponent<IonJetParticles>(); polishedJets[i].Initialize(jets[i].parent,nozzleRadius,craft.IsPlayer); }
                }
            var effects = craft.GetComponent<VehicleVFX>();
            if (!effects) effects = craft.gameObject.AddComponent<VehicleVFX>();
            effects.Initialize(craft);
            lastRecovery = craft.RecoveryCount;
        }

        void LateUpdate()
        {
            if (!vehicle || !plumeMaterial) return;
            var phase = RaceDirector.Instance ? RaceDirector.Instance.Phase : RacePhase.Menu;
            if (Stage2Enabled)
            {
                UpdateStage2(phase);
                return;
            }
            // Throttle is the engine command. Coasting speed must not keep the flame alive.
            // Pausing freezes the presentation with the simulation.
            if (phase == RacePhase.Paused) return;
            float desired = ExhaustDemand;
            response = Mathf.Lerp(response, desired, 1f - Mathf.Exp(-(desired > response ? 18f : 26f) * Time.deltaTime));
            if (response < .003f) response = 0f;
            float flicker = 1f + .025f * Mathf.Sin(Time.time * 33f) + .015f * Mathf.Sin(Time.time * 51f);
            for (int i = 0; i < 3; i++)
            {
                float size = jetLengthScales[i];
                float width = Mathf.Lerp(.35f,1f,Mathf.Sqrt(Mathf.Clamp01(response)));
                jets[i].localScale = new Vector3(width,width,(.04f + response*2.05f)*size);
                outerJets[i].enabled = innerJets[i].enabled = response > 0f;
                SetIntensity(outerJets[i], response*2.15f*flicker);
                SetIntensity(innerJets[i], response*2.45f*flicker);
                SetIntensity(cores[i], (.12f + response*2.6f)*flicker);
                SetIntensity(rings[i*2], .10f + response*.40f);
                SetIntensity(rings[i*2+1], .04f + response*.20f);
                if (i < 2 && lights[i]) lights[i].intensity = response*.55f;
            }
        }

        void UpdateStage2(RacePhase phase)
        {
            if (lastRecovery != vehicle.RecoveryCount || (phase == RacePhase.Countdown && previousPhase != phase) ||
                (phase == RacePhase.Menu && previousPhase != phase))
            {
                envelope.Reset();
                if (boostVisuals) boostVisuals.ResetState();
                foreach(var jet in polishedJets) if(jet)jet.ResetTrail();
                foreach(var jet in layeredJets) if(jet)jet.ResetTrail();
                lastRecovery = vehicle.RecoveryCount;
            }
            previousPhase = phase;
            foreach(var jet in polishedJets)if(jet)jet.SetPaused(phase==RacePhase.Paused);
            foreach(var jet in layeredJets)if(jet)jet.SetPaused(phase==RacePhase.Paused);
            if (phase == RacePhase.Paused) return;
            bool active = RaceDirector.Instance && RaceDirector.Instance.CanSimulate(vehicle);
            envelope.Step(Time.deltaTime, vehicle.ThrottleInput, vehicle.IsBoosting, active, false);
            response = envelope.Response;
            float thrust = Mathf.Clamp01(response);
            float boost = envelope.Boost;
            float ignition = envelope.Ignition;
            if (boostVisuals) boostVisuals.Step(thrust, boost, ignition, envelope.Release);
            // A short rise in width accompanies ignition. Sustained boost is longer,
            // with the small central nozzle remaining a subordinate accent.
            float width = Mathf.Lerp(.28f, 1f, Mathf.Sqrt(thrust)) * (1f + boost * .36f + ignition * .18f);
            float length = .04f + thrust * 2.85f + boost * 1.75f + ignition * .42f;
            if(JetPolishEnabled)length *= 1.20f+boost*.48f+ignition*.14f;
            for (int i = 0; i < 3; i++)
            {
                float seed = 2.31f + i * 4.73f;
                float flutter = 1f + (Mathf.PerlinNoise(envelope.FlowTime * 13f, seed) - .5f) * .16f;
                jets[i].localScale = new Vector3(width, width, length * jetLengthScales[i] * flutter);
                outerJets[i].enabled = innerJets[i].enabled = response > 0f;
                if(layeredJets[i]) { outerJets[i].enabled=innerJets[i].enabled=false; layeredJets[i].Step(Time.deltaTime,thrust,boost,ignition,vehicle.Body?vehicle.Body.linearVelocity:Vector3.zero); }
                if(polishedJets[i])polishedJets[i].Step(Time.deltaTime,thrust,boost,ignition,vehicle.Body?vehicle.Body.linearVelocity:Vector3.zero);
                SetStage2Properties(outerJets[i], thrust * (1.60f + boost * .45f + ignition * .24f) * (JetPolishEnabled ? 1.06f + boost*.12f : 1f), seed);
                SetStage2Properties(innerJets[i], thrust * (1.68f + boost * .48f + ignition * .42f) * (JetPolishEnabled ? 1.1f + boost*.2f + ignition*.25f : 1f), seed + .9f);
                SetStage2Properties(cores[i], .12f + thrust * 2.2f + boost * .3f + ignition * .2f, seed);
                SetStage2Properties(rings[i * 2], .10f + thrust * .33f + boost * .11f, seed);
                SetStage2Properties(rings[i * 2 + 1], .04f + thrust * .18f, seed);
                if (i < 2 && lights[i]) lights[i].intensity = thrust * (.62f + boost * .30f + ignition * .18f) * (stage1Finish?4f:1f);
            }
        }

        void SetStage2Properties(Renderer renderer, float intensity, float seed)
        {
            properties.Clear();
            properties.SetFloat("_Intensity", intensity);
            properties.SetFloat("_FlowTime", envelope.FlowTime);
            properties.SetFloat("_Boost", envelope.Boost);
            properties.SetFloat("_Seed", seed);
            renderer.SetPropertyBlock(properties);
        }

        void SetIntensity(Renderer renderer, float value) { properties.SetFloat("_Intensity",value); renderer.SetPropertyBlock(properties); }
        static Material CreateMaterial(Shader shader, string name, float mode, Color tint)
        { var material = new Material(shader) { name = name }; material.SetFloat("_Mode", mode); material.SetColor("_Tint", tint); return material; }
        static void Configure(Renderer renderer, Material material)
        { renderer.sharedMaterial = material; renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false; renderer.lightProbeUsage = LightProbeUsage.Off; renderer.reflectionProbeUsage = ReflectionProbeUsage.Off; }
        static Renderer MeshRenderer(string name, Transform parent, Mesh mesh, Material material, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name); go.transform.SetParent(parent,false); go.layer = parent.gameObject.layer; go.transform.localPosition = position; go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh; var renderer = go.AddComponent<UnityEngine.MeshRenderer>(); Configure(renderer,material); return renderer;
        }
        static Mesh CreatePlume()
        {
            // Crossed translucent sheets have no solid cone silhouette. Shader density
            // reaches zero before every mesh edge; overlapping Gaussian profiles read as gas.
            const int sheets = 3, rows = 14;
            var vertices = new Vector3[sheets*rows*2]; var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length]; var colors = new Color[vertices.Length]; var triangles = new int[sheets*(rows-1)*6];
            for (int sheet=0;sheet<sheets;sheet++)
            {
                float angle=sheet/(float)sheets*Mathf.PI;
                Vector3 across=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0);
                Vector3 normal=new Vector3(Mathf.Sin(angle),-Mathf.Cos(angle),0);
                for(int row=0;row<rows;row++)
                {
                    float t=row/(float)(rows-1);float width=1.10f-.42f*t;
                    for(int edge=0;edge<2;edge++)
                    {
                        int k=(sheet*rows+row)*2+edge;
                        vertices[k]=across*((edge==0?-1f:1f)*width)+Vector3.back*t;
                        normals[k]=normal;uv[k]=new Vector2(t,edge);colors[k]=Color.white;
                    }
                    if(row<rows-1){int k=(sheet*rows+row)*2,q=(sheet*(rows-1)+row)*6;triangles[q]=k;triangles[q+1]=k+2;triangles[q+2]=k+1;triangles[q+3]=k+1;triangles[q+4]=k+2;triangles[q+5]=k+3;}
                }
            }
            var mesh=new Mesh{name="Soft plasma density sheets"};mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();return mesh;
        }
        static Mesh CreateFullPlume()
        {
            const int sheets = 4, rows = 24;
            var vertices = new Vector3[sheets * rows * 2];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new int[sheets * (rows - 1) * 6];
            for (int sheet = 0; sheet < sheets; sheet++)
            {
                float angle = sheet / (float)sheets * Mathf.PI;
                Vector3 across = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                Vector3 normal = new Vector3(Mathf.Sin(angle), -Mathf.Cos(angle), 0);
                for (int row = 0; row < rows; row++)
                {
                    float t = row / (float)(rows - 1);
                    // Gas expands after leaving the aperture before narrowing.
                    // A broad shoulder and shorter hot core prevent a laser silhouette.
                    float shoulder = Mathf.Lerp(.66f, 1.52f, Mathf.SmoothStep(0f, 1f, t / .22f));
                    float width = shoulder * (1f - Mathf.SmoothStep(0f, 1f, (t - .28f) / .72f)) + .025f;
                    for (int edge = 0; edge < 2; edge++)
                    {
                        int k = (sheet * rows + row) * 2 + edge;
                        vertices[k] = across * ((edge == 0 ? -1f : 1f) * width) + Vector3.back * t;
                        normals[k] = normal;
                        uv[k] = new Vector2(t, edge);
                        colors[k] = Color.white;
                    }
                    if (row == rows - 1) continue;
                    int v = (sheet * rows + row) * 2, q = (sheet * (rows - 1) + row) * 6;
                    triangles[q] = v; triangles[q + 1] = v + 2; triangles[q + 2] = v + 1;
                    triangles[q + 3] = v + 1; triangles[q + 4] = v + 2; triangles[q + 5] = v + 3;
                }
            }
            var mesh = new Mesh { name = "Full tapered plasma density sheets" };
            mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.colors = colors; mesh.triangles = triangles;
            mesh.RecalculateBounds();
            // The shader bends and expands each row; retain it at screen edges.
            var bounds = mesh.bounds;
            bounds.Expand(new Vector3(1.5f, 1.5f, 0f));
            mesh.bounds = bounds;
            return mesh;
        }
        static Mesh CreateRing()
        {
            const int sides=40;var vertices=new Vector3[(sides+1)*2];var uv=new Vector2[vertices.Length];var colors=new Color[vertices.Length];var triangles=new int[sides*6];
            for(int i=0;i<=sides;i++){float angle=i/(float)sides*Mathf.PI*2;for(int edge=0;edge<2;edge++){int k=i*2+edge;float radius=edge==0?.305f:.334f;vertices[k]=new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,0);uv[k]=new Vector2(edge,i/(float)sides);colors[k]=Color.white;}
                if(i<sides){int k=i*2,q=i*6;triangles[q]=k;triangles[q+1]=k+2;triangles[q+2]=k+1;triangles[q+3]=k+1;triangles[q+4]=k+2;triangles[q+5]=k+3;}}
            var mesh=new Mesh{name="Thin engine light annulus"};mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void OnDestroy()
        { if(plumeMaterial)Destroy(plumeMaterial);if(innerMaterial)Destroy(innerMaterial);if(coreMaterial)Destroy(coreMaterial);if(ringMaterial)Destroy(ringMaterial);if(plumeMesh)Destroy(plumeMesh);if(ringMesh)Destroy(ringMesh); }
    }
}
