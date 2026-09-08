using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Short translucent propulsion at optional authored apertures, with the original V2 layout as fallback.</summary>
    public sealed class IonPropulsion : MonoBehaviour
    {
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
        readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
        float response;
        public float ExhaustResponse => response;
        public float ExhaustDemand => vehicle && RaceDirector.Instance && RaceDirector.Instance.Phase == RacePhase.Racing
            ? vehicle.ThrottleInput * (vehicle.IsBoosting ? 1.35f : 1f) : 0f;

        public void Initialize(HoverVehicle craft)
        {
            if (vehicle) return;
            vehicle = craft;
            var shader = Resources.Load<Shader>("Shaders/IonPlume");
            if (!shader) { Debug.LogError("Ion Plume shader missing from Resources"); enabled = false; return; }
            plumeMaterial = CreateMaterial(shader, "Soft cyan plasma envelope", 0, new Color(.035f,.62f,1.1f,.38f));
            innerMaterial = CreateMaterial(shader, "Soft pale ion spine", 0, new Color(.55f,.86f,1.1f,.30f));
            coreMaterial = CreateMaterial(shader, "Recessed pale plasma core", 1, new Color(.65f,.88f,1.05f,.95f));
            ringMaterial = CreateMaterial(shader, "Subtle nozzle rim accents", 2, new Color(.025f,.76f,1.3f,.18f));
            plumeMesh = CreatePlume(); ringMesh = CreateRing();
            Transform parent = craft.VisualRoot ? craft.VisualRoot : craft.transform;
            bool authored=ShipEngineAnchors.TryLoad(out var engineAnchors);
            for (int i = 0; i < 3; i++)
            {
                float size = i == 2 ? .35f : 1f;
                var definition=authored?engineAnchors[i]:null;
                float mantleRadius=authored?definition.OpeningRadius*.55f:.37f*size;
                float spineRadius=authored?definition.OpeningRadius*.22f:.13f*size;
                jetLengthScales[i]=authored?Mathf.Clamp(definition.OpeningRadius/.509f,.2f,2f):(i==2?.60f:1f);
                var anchor = new GameObject(i == 2 ? "Central ion aperture" : (i == 0 ? "Port ion aperture" : "Starboard ion aperture"));
                anchor.transform.SetParent(parent, false);
                anchor.layer = parent.gameObject.layer;
                anchor.transform.localPosition = authored?definition.Exit:(i == 2 ? new Vector3(0,-.08f,-2.76f) : new Vector3(i == 0 ? -1.68f : 1.68f,-.035f,-3.405f));
                var jet = new GameObject("Variable length thrust volume"); jet.transform.SetParent(anchor.transform, false); jets[i] = jet.transform;
                jet.layer = anchor.layer;
                outerJets[i] = MeshRenderer("Tapered ion mantle", jet.transform, plumeMesh, plumeMaterial, Vector3.zero, new Vector3(mantleRadius,mantleRadius,1));
                innerJets[i] = MeshRenderer("White blue jet spine", jet.transform, plumeMesh, innerMaterial, new Vector3(0,0,-.02f), new Vector3(spineRadius,spineRadius,.74f));
                var core = GameObject.CreatePrimitive(PrimitiveType.Quad); core.name = "Radiance inside engine throat";
                core.transform.SetParent(anchor.transform,false); core.layer = anchor.layer;
                // Keep the compact radiance at the throat; the outer cavity remains physical and dark.
                core.transform.localPosition = authored?definition.Throat-definition.Exit-Vector3.forward*.012f:new Vector3(0,0,i == 2 ? .315f : .67f);
                core.transform.localScale = Vector3.one*(authored?definition.CompactCoreRadius*2f:.5f*size);
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
                    lights[i] = lamp.AddComponent<Light>(); lights[i].type = LightType.Point; lights[i].color = new Color(.12f,.62f,1f); lights[i].range = 1.55f; lights[i].shadows = LightShadows.None;
                }
            }
            var effects = craft.GetComponent<VehicleVFX>();
            if (!effects) effects = craft.gameObject.AddComponent<VehicleVFX>();
            effects.Initialize(craft);
        }

        void LateUpdate()
        {
            if (!vehicle || !plumeMaterial) return;
            var phase = RaceDirector.Instance ? RaceDirector.Instance.Phase : RacePhase.Menu;
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
