using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Deterministic, mesh-authored coastal geology and offshore infrastructure.</summary>
    public sealed class CoastalSetDressing : MonoBehaviour
    {
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        readonly List<Vector3> cliffCenters = new List<Vector3>();
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
            BuildAuthoredCoast(world, track);

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

        void BuildAuthoredCoast(WorldBuilder world, TrackPath track)
        {
            var cliff=Resources.Load<GameObject>("Art/Environment/Solstice_CoastalCliff_C");
            var rock=Resources.Load<Material>("CoastalRock");
            if(!cliff||!rock){Debug.LogError("Reviewed coastal cliff or textured rock material is missing");return;}
            var scrub=world.MakeMaterial("Coastal scrub",new Color(.23f,.27f,.12f),.1f);
            var city=world.GetComponent<AuthoredCity>();
            if(city!=null)for(int i=0;i<city.HarborCenters.Length;i++){
                Vector3 landward=city.HarborLandwardDirections[i];
                Vector3 across=Vector3.Cross(Vector3.up,landward);
                for(int j=0;j<3;j++){
                    Vector3 center=city.HarborCenters[i]+landward*(108+j%2*28)+across*(j-1)*52;
                    if(DistanceToRoad(new Vector2(center.x,center.z))<86)continue;
                    bool blocksQuay=false;
                    for(int q=0;q<city.HarborCenters.Length;q++){
                        Vector3 delta=center-city.HarborCenters[q],back=city.HarborLandwardDirections[q],side=Vector3.Cross(Vector3.up,back);
                        float dx=Mathf.Max(0,Mathf.Abs(Vector3.Dot(delta,side))-64),dz=Mathf.Max(0,Mathf.Abs(Vector3.Dot(delta,back))-62);
                        if(dx*dx+dz*dz<40*40)blocksQuay=true;
                    }
                    if(blocksQuay)continue;
                    PlaceCliff(cliff,rock,scrub,center,Quaternion.LookRotation(-landward)*Quaternion.Euler(0,(j-1)*13,0),1.12f+j*.06f,"Harbor headland");
                }
            }
            Vector3[] centers={new Vector3(-500,0,-80),new Vector3(590,0,155),new Vector3(140,0,730),new Vector3(-590,0,490),new Vector3(620,0,-420),new Vector3(-440,0,-660)};
            for(int i=0;i<centers.Length;i++){
                float scale=1.3f+(i%3)*.18f;
                var center=SafeCenter(centers[i],58*scale);
                PlaceCliff(cliff,rock,scrub,center,Quaternion.Euler(0,i*71+24,0),scale,"Coastal headland");
                if(i%2==0)PlaceCliff(cliff,rock,scrub,SafeCenter(center+new Vector3(70,0,38),58*scale),Quaternion.Euler(0,i*71+62,0),scale*.82f,"Broken outer headland");
            }
        }

        void PlaceCliff(GameObject source,Material rock,Material scrub,Vector3 position,Quaternion rotation,float scale,string name)
        {
            foreach(var existing in cliffCenters)if(Vector3.ProjectOnPlane(position-existing,Vector3.up).sqrMagnitude<40*40)return;
            cliffCenters.Add(position);
            position.y=-.65f;
            var instance=Instantiate(source,transform);instance.name=name;
            instance.transform.SetPositionAndRotation(position,rotation);instance.transform.localScale=Vector3.one*scale;
            foreach(var renderer in instance.GetComponentsInChildren<Renderer>()){
                var materials=renderer.sharedMaterials;
                for(int m=0;m<materials.Length;m++)materials[m]=materials[m]&&materials[m].name.Contains("Vegetation")?scrub:rock;
                renderer.sharedMaterials=materials;
            }
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
