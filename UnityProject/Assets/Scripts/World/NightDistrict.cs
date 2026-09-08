using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Original night city. Scenery boxes/pipes are batched by district/material.</summary>
    public sealed class NightDistrict : MonoBehaviour
    {
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Material> ownMaterials=new List<Material>();
        readonly Dictionary<Material,Batch> batches=new Dictionary<Material,Batch>();
        readonly List<Vector3> accepted=new List<Vector3>();
        public IReadOnlyList<Vector3> DistrictCenters => accepted;
        Material concrete,steel,trim,asphalt,warm,cyan,windows;
        Vector3[] samples;
        float clearance;

        public void Build(WorldBuilder world,TrackPath track)
        {
            concrete=world.MakeMaterial("Night / blue slate ceramic",new Color(.12f,.17f,.23f),.42f,.1f);
            steel=world.MakeMaterial("Night / industrial gunmetal",new Color(.032f,.049f,.075f),.55f,.45f);
            trim=world.MakeMaterial("Night / weathered aluminum",new Color(.29f,.36f,.42f),.65f,.5f);
            asphalt=world.MakeMaterial("Night / service asphalt",new Color(.025f,.035f,.049f),.5f,.08f);
            warm=world.MakeMaterial("Night / amber wayfinding",new Color(.55f,.22f,.055f),.25f,0,new Color(2.2f,.75f,.15f));
            cyan=world.MakeMaterial("Night / cool architectural light",new Color(.08f,.3f,.42f),.25f,0,new Color(.12f,.75f,1.2f));
            var shader=Shader.Find("VectorRush/NightWindows");
            windows=shader?new Material(shader):world.MakeMaterial("Night / window fallback",new Color(.08f,.15f,.22f),.75f,.1f,new Color(.15f,.24f,.35f));
            if(shader){ownMaterials.Add(windows);windows.SetFloat("_Density",.36f);}
            samples=new Vector3[720];for(int i=0;i<samples.Length;i++)samples[i]=track.Evaluate((float)i/samples.Length).Position;
            clearance=track.Width*.5f+30f;
            Box(Vector3.zero,Quaternion.identity,new Vector3(0,-1.15f,0),new Vector3(2400,.5f,2400),asphalt);
            Flush("Night city ground");
            var a=Resources.Load<GameObject>("Art/Environment/Solstice_TerraceTower_A");
            var b=Resources.Load<GameObject>("Art/Environment/Solstice_SplitTower_B");
            float[] anchors={.13f,.40f,.67f,.88f};
            for(int i=0;i<anchors.Length;i++)
            {
                var frame=track.Evaluate(anchors[i]);Vector3 outward=Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
                Quaternion q=Quaternion.LookRotation(-outward,Vector3.up);Vector3 center=frame.Position+outward*135f;center.y=0;
                for(int n=0;n<12&&!Clear(center,q,new Vector2(82,77));n++)center+=outward*15f;
                if(!Clear(center,q,new Vector2(82,77)))continue;
                accepted.Add(center);
                District(center,q,i,a,b);
                Flush("Night district "+(i+1));
            }
            // Low industrial service blocks fill selected middle-distance gaps.
            for(int i=0;i<6;i++)
            {
                var frame=track.Evaluate(.04f+i*.16f);Vector3 direction=Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
                Vector3 center=frame.Position-direction*115f;center.y=0;Quaternion q=Quaternion.LookRotation(direction,Vector3.up);
                if(!Clear(center,q,new Vector2(43,32)))continue;
                bool occupied=false;foreach(var other in accepted)if((center-other).sqrMagnitude<120*120)occupied=true;
                if(occupied)continue;
                Industrial(center,q,i);Flush("Inner service works "+i);
            }
        }

        void District(Vector3 c,Quaternion q,int index,GameObject a,GameObject b)
        {
            Box(c,q,new Vector3(0,1.2f,0),new Vector3(154,4.4f,140),steel);
            Box(c,q,new Vector3(0,3.55f,0),new Vector3(151,.3f,137),concrete);
            // Occupied street-edge podium: distinct deep piers, recessed storefronts and canopy.
            Box(c,q,new Vector3(0,8,53),new Vector3(130,9,17),steel);
            Box(c,q,new Vector3(0,7.7f,61.6f),new Vector3(125,6,.16f),windows);
            Box(c,q,new Vector3(0,12.7f,55),new Vector3(134,.65f,22),concrete);
            Box(c,q,new Vector3(0,12.25f,65.9f),new Vector3(115,.14f,.2f),index%2==0?warm:cyan);
            for(int i=0;i<14;i++)Box(c,q,new Vector3(-63+i*9.6f,8,62),new Vector3(.75f,9,1.4f),trim);
            Vector3[] positions={new Vector3(-39,3.7f,-17),new Vector3(28,3.7f,-28),new Vector3(9,3.7f,23)};
            float[] scales={1.1f,.84f,.99f};
            for(int i=0;i<positions.Length;i++)
            {
                var source=(index+i)%2==0?a:b;if(!source)continue;
                var tower=Instantiate(source,transform);tower.name="Night / occupied authored tower "+index+"-"+i;
                tower.transform.SetPositionAndRotation(c+q*positions[i],q);tower.transform.localScale=Vector3.one*scales[i];
                foreach(var renderer in tower.GetComponentsInChildren<Renderer>())
                {
                    var mats=renderer.sharedMaterials;
                    for(int m=0;m<mats.Length;m++)
                    {
                        string name=mats[m]?mats[m].name:"Ivory";
                        mats[m]=name.Contains("Glass")?windows:name.Contains("Metal")?trim:name.Contains("Ivory")?concrete:steel;
                    }
                    renderer.sharedMaterials=mats;
                }
                // Narrow crown lights identify silhouettes without illuminating complete slabs.
                Box(c,q,positions[i]+new Vector3(0,73.5f*scales[i],0),new Vector3(12*scales[i],.22f,.3f),i==1?warm:cyan);
            }
            // Separated service viaduct occupies district frontage, safely outside race footprint.
            Box(c,q,new Vector3(0,6.2f,73),new Vector3(140,1.3f,10),concrete);
            Box(c,q,new Vector3(0,6.89f,73),new Vector3(140,.08f,8.4f),asphalt);
            for(int x=-60;x<=60;x+=24)
            {
                Box(c,q,new Vector3(x,2.1f,73),new Vector3(1.7f,7,3),steel);
                Box(c,q,new Vector3(x,6.97f,73),new Vector3(5,.03f,.16f),warm);
            }
            Box(c,q,new Vector3(0,7.7f,77.6f),new Vector3(140,1,.3f),steel);
            Box(c,q,new Vector3(0,8.25f,77.6f),new Vector3(140,.08f,.16f),warm);
            // Human-scale access furniture and linked utilities, batched into very few draws.
            for(int i=0;i<7;i++)
            {
                float x=-59+i*19;
                Box(c,q,new Vector3(x,5,66),new Vector3(.18f,3,.18f),trim);
                Box(c,q,new Vector3(x,6.6f,66),new Vector3(.45f,.25f,.45f),warm);
            }
            Industrial(c+q*new Vector3(-38,3.7f,-45),q,index,true);
        }

        void Industrial(Vector3 c,Quaternion q,int index,bool compact=false)
        {
            float sx=compact?.52f:1f;
            Vector3 dims=new Vector3(72*sx,12,40*sx);
            Box(c,q,new Vector3(0,6,0),dims,steel);
            Box(c,q,new Vector3(0,12.4f,0),new Vector3(dims.x+2,.7f,dims.z+2),concrete);
            Box(c,q,new Vector3(0,6.3f,dims.z*.5f+.06f),new Vector3(dims.x-3,9,.12f),windows);
            for(int i=0;i<9;i++)
            {
                float x=-dims.x*.47f+i*dims.x*.1175f;
                Box(c,q,new Vector3(x,6.2f,dims.z*.5f+.7f),new Vector3(.55f,12,1.4f),concrete);
                if(i%3==0)Box(c,q,new Vector3(x+.4f,9.8f,dims.z*.5f+1.5f),new Vector3(3,.13f,.15f),warm);
            }
            // Roof plant: cooling banks with fins, risers and connected bent pipe runs.
            for(int i=0;i<3;i++)
            {
                Vector3 unit=new Vector3(-dims.x*.3f+i*dims.x*.3f,14,0);
                Box(c,q,unit,new Vector3(8*sx,3,9*sx),concrete);
                for(int f=0;f<6;f++)Box(c,q,unit+new Vector3(0,-1+f*.4f,4.6f*sx),new Vector3(7*sx,.11f,.25f),steel);
                Pipe(c,q,new Vector3(unit.x,12.9f,-dims.z*.25f),new Vector3(unit.x,18,-dims.z*.25f),.65f,trim);
                Pipe(c,q,new Vector3(unit.x,18,-dims.z*.25f),new Vector3(unit.x+5*sx,18,-dims.z*.25f),.65f,trim);
                Box(c,q,new Vector3(unit.x,18.8f,-dims.z*.25f),new Vector3(.2f,.2f,.2f),warm);
            }
            Pipe(c,q,new Vector3(-dims.x*.48f,13.2f,-dims.z*.32f),new Vector3(dims.x*.48f,13.2f,-dims.z*.32f),.8f,trim);
            Box(c,q,new Vector3(0,.05f,dims.z*.5f+7),new Vector3(dims.x+10,.15f,11),asphalt);
        }

        bool Clear(Vector3 center,Quaternion q,Vector2 half)
        {
            var inverse=Quaternion.Inverse(q);
            foreach(var p in samples)
            {
                Vector3 d=inverse*(p-center);float x=Mathf.Max(0,Mathf.Abs(d.x)-half.x),z=Mathf.Max(0,Mathf.Abs(d.z)-half.y);
                if(x*x+z*z<clearance*clearance)return false;
            }
            return true;
        }
        Batch Get(Material material){if(!batches.TryGetValue(material,out var batch)){batch=new Batch();batches.Add(material,batch);}return batch;}
        void Box(Vector3 c,Quaternion q,Vector3 p,Vector3 size,Material mat)
        {
            var batch=Get(mat);Vector3 h=size*.5f;
            Vector3[] corners={new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)};
            int[] faces={0,3,2,1,5,6,7,4,4,7,3,0,1,2,6,5,3,7,6,2,4,0,1,5};
            for(int f=0;f<6;f++)batch.Quad(c+q*(p+corners[faces[f*4]]),c+q*(p+corners[faces[f*4+1]]),c+q*(p+corners[faces[f*4+2]]),c+q*(p+corners[faces[f*4+3]]));
        }
        void Pipe(Vector3 c,Quaternion q,Vector3 a,Vector3 b,float radius,Material material)
        {
            Vector3 axis=(b-a).normalized;Vector3 side=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;
            Vector3 up=Vector3.Cross(axis,side);var batch=Get(material);
            for(int i=0;i<8;i++)
            {
                float t=i*Mathf.PI*.25f,u=(i+1)*Mathf.PI*.25f;Vector3 p=(side*Mathf.Cos(t)+up*Mathf.Sin(t))*radius,r=(side*Mathf.Cos(u)+up*Mathf.Sin(u))*radius;
                batch.Quad(c+q*(a+p),c+q*(a+r),c+q*(b+r),c+q*(b+p));
            }
        }
        void Flush(string name)
        {
            foreach(var pair in batches)
            {
                var mesh=new Mesh{name=name+" / "+pair.Key.name};if(pair.Value.vertices.Count>65535)mesh.indexFormat=IndexFormat.UInt32;
                mesh.SetVertices(pair.Value.vertices);mesh.SetTriangles(pair.Value.triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
                var go=new GameObject(mesh.name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            batches.Clear();
        }
        sealed class Batch
        {
            public readonly List<Vector3> vertices=new List<Vector3>();public readonly List<int> triangles=new List<int>();
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);foreach(var mat in ownMaterials)if(mat)Destroy(mat);}
    }
}
