using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Deterministic, mesh-authored coastal geology and offshore infrastructure.</summary>
    public sealed class CoastalSetDressing : MonoBehaviour
    {
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        Vector2[] road;
        const float Clearance = 112f;

        public void Build(WorldBuilder world, TrackPath track)
        {
            if (!world || !track || world.transform.Find("Solstice coastal geology")) return;
            var root = new GameObject("Solstice coastal geology");
            root.transform.SetParent(world.transform, false);
            root.AddComponent<CoastalSetDressing>().Generate(world, track);
        }

        void Generate(WorldBuilder world, TrackPath track)
        {
            road = new Vector2[481];
            for (int i = 0; i < road.Length; i++) {
                Vector3 p = track.Evaluate(i / 480f).Position;
                road[i] = new Vector2(p.x, p.z);
            }
            var stone = new[] {
                world.MakeMaterial("Basalt weathered face", new Color(.38f,.40f,.37f), .13f),
                world.MakeMaterial("Basalt fractured face", new Color(.285f,.32f,.32f), .10f),
                world.MakeMaterial("Basalt deep fracture", new Color(.19f,.235f,.25f), .08f),
                world.MakeMaterial("Pale mineral strata", new Color(.49f,.48f,.415f), .16f),
                world.MakeMaterial("Wave washed dark basalt", new Color(.13f,.205f,.215f), .39f),
                world.MakeMaterial("Sparse coastal scrub", new Color(.245f,.285f,.16f), .05f)
            };
            var geology = new MeshDraft(6);
            Island(geology, new Vector3(-500,0,-80), 175, 145, 31, 48);
            Island(geology, new Vector3(590,0,155), 150, 125, 57, 48);
            Island(geology, new Vector3(140,0,730), 215, 132, 83, 56);
            Island(geology, new Vector3(-590,0,490), 150, 106, 107, 44);
            Island(geology, new Vector3(620,0,-420), 135, 94, 133, 44);
            Island(geology, new Vector3(-440,0,-660), 170, 96, 159, 44);
            Island(geology, new Vector3(970,0,560), 175, 102, 181, 40);
            Island(geology, new Vector3(-990,0,160), 230, 162, 211, 44);
            Island(geology, new Vector3(50,0,1250), 330, 190, 237, 48);
            Island(geology, new Vector3(1050,0,-540), 220, 125, 263, 44);
            Island(geology, new Vector3(-1050,0,-690), 280, 140, 293, 44);
            // Offshore stacks repeat the same geological language at a smaller scale.
            for (int i = 0; i < 7; i++) {
                float a = .45f + i * .85f;
                Island(geology, new Vector3(Mathf.Cos(a)*485,0,Mathf.Sin(a)*505),
                    17 + i%3*7, 25 + i%4*10, 347+i*17, 16);
            }
            MakeMesh("Layered basalt coast and sea stacks", geology, stone, false);

            var architecture = new MeshDraft(3);
            Landmark(architecture, SafeCenter(new Vector3(540,0,-100), 95), 24, 1f);
            Landmark(architecture, SafeCenter(new Vector3(-445,0,380), 95), -31, .8f);
            MakeMesh("Ivory tidal research arches", architecture,
                new[] { world.Ivory, world.Graphite, world.Signal }, true);
        }

        Vector3 SafeCenter(Vector3 center, float footprint)
        {
            Vector2 p = new Vector2(center.x, center.z);
            Vector2 outward = p.sqrMagnitude > 1 ? p.normalized : Vector2.right;
            for (int attempt = 0; attempt < 100 && DistanceToRoad(p) < footprint + Clearance; attempt++)
                p += outward * 22f;
            return new Vector3(p.x, center.y, p.y);
        }

        float DistanceToRoad(Vector2 p)
        {
            float best = float.MaxValue;
            for (int i = 0; i < road.Length-1; i++) {
                Vector2 a = road[i], d = road[i+1]-a;
                float t = d.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude) : 0;
                best = Mathf.Min(best, (p-a-d*t).sqrMagnitude);
            }
            return Mathf.Sqrt(best);
        }

        void Island(MeshDraft draft, Vector3 position, float radius, float height, int seed, int sides)
        {
            // All generated points fit inside this conservative horizontal disk.
            Vector3 center = SafeCenter(position, radius * 1.6f);
            float phase = seed * .317f;
            float[] radii = {1.1f,1.065f,1.035f,1.02f,.93f,.95f,.79f,.76f,.51f,.24f};
            float[] levels = {-.12f,0,.045f,.18f,.205f,.40f,.44f,.66f,.82f,.87f};
            var rings = new Vector3[radii.Length, sides];
            for (int layer = 0; layer < radii.Length; layer++) for (int j = 0; j < sides; j++) {
                float a = j * Mathf.PI * 2 / sides;
                float outline = 1 + .14f*Mathf.Sin(a*3+phase) + .075f*Mathf.Sin(a*7-phase*.7f) + .04f*Mathf.Cos(a*13+phase);
                float fracture = (Mathf.PerlinNoise(j*.73f+seed,layer*.91f)-.5f)*.065f;
                float r = radius * radii[layer] * (outline + fracture);
                float ridge = (.10f*Mathf.Sin(a*2+phase) + .055f*Mathf.Cos(a*5-phase)) * Mathf.Clamp01(layer*.18f);
                float y = height * (levels[layer] + ridge) - .8f;
                if (layer == 0) y = -14;
                if (layer == 1) y = -1 + Mathf.Sin(a*7+phase)*1.5f;
                // The upper strata drift laterally, creating a broken ridge rather than a cone.
                Vector3 shift = new Vector3(layer*radius*.012f, 0, -layer*radius*.006f);
                rings[layer,j] = center + shift + new Vector3(Mathf.Cos(a)*r, y, Mathf.Sin(a)*r*.76f);
            }
            for (int layer = 0; layer < radii.Length-1; layer++) for (int j = 0; j < sides; j++) {
                int k = (j+1)%sides;
                Vector3 a = rings[layer,j], b = rings[layer,k], c = rings[layer+1,k], d = rings[layer+1,j];
                int material = RockMaterial(layer,j,seed);
                // Split direction alternates, so fractures do not form a repetitive diagonal grid.
                if ((j+layer)%2 == 0) {
                    draft.Face(a,d,c,material); draft.Face(a,c,b,material);
                } else {
                    draft.Face(a,d,b,material); draft.Face(b,d,c,material);
                }
            }
            Vector3 summit = center + new Vector3(radius*.15f, height*.93f-.8f, -radius*.08f);
            for (int j = 0; j < sides; j++)
                draft.Face(rings[9,j], summit, rings[9,(j+1)%sides], j%7==0 ? 5 : (j+seed)%3);
        }

        static int RockMaterial(int layer, int sector, int seed)
        {
            if (layer <= 1) return 4;
            if (layer == 3 || layer == 5) return (sector+seed)%4 == 0 ? 1 : 3;
            if (layer >= 7 && (sector*13+seed)%9 < 3) return 5;
            int variation = (sector*17 + layer*11 + seed)%11;
            return variation < 5 ? 0 : variation < 9 ? 1 : 2;
        }

        void Landmark(MeshDraft draft, Vector3 center, float yaw, float scale)
        {
            Quaternion rotation = Quaternion.Euler(0,yaw,0);
            for (int rib = -1; rib <= 1; rib += 2) {
                const int sections = 44;
                var rings = new Vector3[sections+1,8];
                for (int i = 0; i <= sections; i++) {
                    float t = i/(float)sections;
                    Vector3 local = new Vector3(Mathf.Lerp(-65,65,t), 5+122*Mathf.Sin(Mathf.PI*t), rib*12);
                    Vector3 tangent = new Vector3(130,122*Mathf.PI*Mathf.Cos(Mathf.PI*t),0).normalized;
                    Vector3 side = new Vector3(-tangent.y,tangent.x,0);
                    float width = Mathf.Lerp(8,3.5f,Mathf.Sin(Mathf.PI*t));
                    for (int j = 0; j < 8; j++) {
                        float a = Mathf.PI*.125f + j*Mathf.PI*.25f;
                        Vector3 section = side*Mathf.Cos(a)*width + Vector3.forward*Mathf.Sin(a)*6;
                        rings[i,j] = center + rotation*((local+section)*scale);
                    }
                }
                for (int i = 0; i < sections; i++) for (int j = 0; j < 8; j++) {
                    int k = (j+1)%8;
                    draft.Face(rings[i,j],rings[i,k],rings[i+1,k],0);
                    draft.Face(rings[i,j],rings[i+1,k],rings[i+1,j],0);
                }
                for (int j = 1; j < 7; j++) {
                    draft.Face(rings[0,0],rings[0,j+1],rings[0,j],0);
                    draft.Face(rings[sections,0],rings[sections,j],rings[sections,j+1],0);
                }
            }
            for (int j = -1; j <= 1; j += 2) {
                BoxMesh(draft,center+rotation*new Vector3(j*65,1,0)*scale,new Vector3(24,4,43)*scale,rotation,1);
                BoxMesh(draft,center+rotation*new Vector3(j*65,4,0)*scale,new Vector3(21,2,38)*scale,rotation,0);
            }
            // A restrained, suspended observation deck provides a readable functional scale.
            BoxMesh(draft,center+Vector3.up*89*scale,new Vector3(72,3,37)*scale,rotation,1);
            BoxMesh(draft,center+Vector3.up*92*scale,new Vector3(74,1,39)*scale,rotation,0);
            BoxMesh(draft,center+rotation*new Vector3(0,94,-19)*scale,new Vector3(44,.8f,.8f)*scale,rotation,2);
        }

        static void BoxMesh(MeshDraft d, Vector3 p, Vector3 size, Quaternion q, int material)
        {
            var v = new Vector3[8];
            for (int i = 0; i < 8; i++) v[i] = p+q*Vector3.Scale(new Vector3((i&1)==0?-.5f:.5f,(i&2)==0?-.5f:.5f,(i&4)==0?-.5f:.5f),size);
            int[] t = {0,2,3,0,3,1,4,5,7,4,7,6,0,4,6,0,6,2,1,3,7,1,7,5,0,1,5,0,5,4,2,6,7,2,7,3};
            for (int i=0;i<t.Length;i+=3) d.Face(v[t[i]],v[t[i+1]],v[t[i+2]],material);
        }

        void MakeMesh(string name, MeshDraft data, Material[] materials, bool smooth)
        {
            // Each whole collection uses one renderer and a small fixed number of material slots.
            var mesh = new Mesh { name = name, indexFormat = data.vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(data.vertices); mesh.subMeshCount = data.triangles.Length;
            for (int i=0;i<data.triangles.Length;i++) mesh.SetTriangles(data.triangles[i],i);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); ownedMeshes.Add(mesh);
            var go = new GameObject(name); go.transform.SetParent(transform,false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        sealed class MeshDraft
        {
            public readonly List<Vector3> vertices = new List<Vector3>();
            public readonly List<int>[] triangles;
            public MeshDraft(int slots) { triangles=new List<int>[slots];for(int i=0;i<slots;i++)triangles[i]=new List<int>(); }
            public void Face(Vector3 a,Vector3 b,Vector3 c,int material) {
                int start=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);
                triangles[material].Add(start);triangles[material].Add(start+1);triangles[material].Add(start+2);
            }
        }
        void OnDestroy() { foreach (var mesh in ownedMeshes) if (mesh) Destroy(mesh); }
    }
}
