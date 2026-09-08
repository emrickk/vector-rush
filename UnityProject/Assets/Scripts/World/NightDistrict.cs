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
        readonly Material[] facades=new Material[4];
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
            if(shader){ownMaterials.Add(windows);windows.SetFloat("_Density",.53f);}
            float[] cellWidths={2.1f,1.65f,3.3f,2.65f},floorHeights={3.4f,3.1f,3.8f,3.3f};
            float[] densities={.49f,.36f,.57f,.4f},intensities={.86f,.57f,.70f,.48f};
            for(int i=0;i<facades.Length;i++)
            {
                if(!shader){facades[i]=windows;continue;}
                var facade=new Material(windows){name="Night / facade character "+i};ownMaterials.Add(facade);facades[i]=facade;
                facade.SetFloat("_CellWidth",cellWidths[i]);facade.SetFloat("_FloorHeight",floorHeights[i]);
                facade.SetFloat("_Density",densities[i]);facade.SetFloat("_Intensity",intensities[i]);
                facade.SetFloat("_Seed",3+i*7);facade.SetFloat("_BandInterval",5+i*2);
                if(i==1){facade.SetColor("_WarmColor",new Color(.54f,.46f,.32f));facade.SetColor("_CoolColor",new Color(.32f,.53f,.7f));}
                if(i==3){facade.SetColor("_WarmColor",new Color(.62f,.38f,.22f));facade.SetColor("_CoolColor",new Color(.21f,.41f,.56f));}
            }
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
            BuildUrbanFabric();
        }

        void BuildUrbanFabric()
        {
            var random=new System.Random(64281);
            // Continuous surrounding skyline: staggered rings make parallax and depth,
            // with taller silhouettes behind lower buildings instead of four isolated islands.
            int[] counts={28,32,36};float[] radii={445,625,855};
            for(int ring=0;ring<3;ring++)
            {
                for(int i=0;i<counts[ring];i++)
                {
                    float angle=(i+.27f*ring)*Mathf.PI*2/counts[ring];
                    float r=radii[ring]+((float)random.NextDouble()-.5f)*48;
                    Vector3 center=new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r);
                    Quaternion q=Quaternion.Euler(0,-angle*Mathf.Rad2Deg+90,0);
                    float w=24+(float)random.NextDouble()*20,d=22+(float)random.NextDouble()*21;
                    float height=65+(float)random.NextDouble()*105+ring*27;
                    if(i%9==0)height+=45;
                    bool nearDistrict=false;
                    foreach(var other in accepted)if((center-other).sqrMagnitude<105f*105f)nearDistrict=true;
                    if(nearDistrict||!Clear(center,q,new Vector2(w*.7f+5,d*.7f+5)))continue;
                    SkylineTower(center,q,w,d,height,(i+ring)%5);
                }
                Flush("Layered skyline / depth "+ring);
            }
            // Mid-rise districts bridge the space between the race and the skyline.
            for(int i=0;i<26;i++)
            {
                float a=(i+.4f)*Mathf.PI*2/26;float r=345+(i%3)*17;
                Vector3 c=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
                Quaternion q=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);
                bool nearDistrict=false;foreach(var other in accepted)if((c-other).sqrMagnitude<120*120)nearDistrict=true;
                if(nearDistrict||!Clear(c,q,new Vector2(32,30)))continue;
                SkylineTower(c,q,43,38,25+(i%5)*10,i%5);
                Box(c,q,new Vector3(0,1,0),new Vector3(62,3,58),concrete);
                Box(c,q,new Vector3(0,4.8f,27),new Vector3(54,5,.25f),windows);
                Box(c,q,new Vector3(0,8,28),new Vector3(59,.45f,3),steel);
                Box(c,q,new Vector3(0,7.75f,29.6f),new Vector3(42,.12f,.16f),i%3==0?cyan:warm);
            }
            Flush("Middle city / mixed-use blocks");
            // Interior blocks give the descent an inhabited foreground below the track.
            for(int x=-2;x<=2;x++)for(int z=-2;z<=2;z++)
            {
                Vector3 c=new Vector3(x*70+12,0,z*70+14);Quaternion q=Quaternion.Euler(0,(x+z)%2*90,0);
                if(!Clear(c,q,new Vector2(25,26)))continue;
                SkylineTower(c,q,36,40,19+Mathf.Abs(x*11+z*7)%36,Mathf.Abs(x+z)%5);
            }
            Flush("Inner city / low urban fabric");
            // A real ground-level street grid, seen through the elevated road supports.
            // These lanes and 8m streetlamps cannot reach the 28m minimum track height.
            for(int lane=-7;lane<=7;lane++)
            {
              for(int segment=-7;segment<=7;segment++)
              {
                float x=lane*112,z=segment*112;
                Box(Vector3.zero,Quaternion.identity,new Vector3(x,.02f,z),new Vector3(13,.12f,112),asphalt);
                Box(Vector3.zero,Quaternion.identity,new Vector3(x,.02f,z),new Vector3(112,.12f,13),asphalt);
                for(int side=-1;side<=1;side+=2)
                {
                    Box(Vector3.zero,Quaternion.identity,new Vector3(x+side*6.7f,.16f,z),new Vector3(.07f,.06f,78),warm);
                    Vector3 pole=new Vector3(x+side*8,4,z+35);
                    Box(Vector3.zero,Quaternion.identity,pole,new Vector3(.22f,8,.22f),trim);
                    Box(Vector3.zero,Quaternion.identity,pole+new Vector3(-side*.9f,4,0),new Vector3(2,.12f,.45f),warm);
                    // Tiny opposing traffic queues are architectural light detail, not gameplay AI.
                    for(int vehicle=0;vehicle<2;vehicle++)
                    {
                        Vector3 car=new Vector3(x+side*3,.45f,z-26+vehicle*12+side*9);
                        Box(Vector3.zero,Quaternion.identity,car,new Vector3(1.8f,.8f,4),steel);
                        Box(Vector3.zero,Quaternion.identity,car+new Vector3(0,0,side*2.05f),new Vector3(1.5f,.15f,.1f),side>0?warm:cyan);
                    }
                }
              }
              if(lane%3==0)Flush("Service avenues / sector "+lane);
            }
            Flush("Service avenues / remainder");
        }

        void SkylineTower(Vector3 c,Quaternion q,float w,float d,float h,int style)
        {
            Material facade=facades[(Mathf.FloorToInt(Mathf.Abs(c.x+c.z)*.037f)+style)%4];
            bool detailed=c.x*c.x+c.z*c.z<420f*420f;
            Box(c,q,new Vector3(0,4,0),new Vector3(w+8,8,d+8),steel);
            if(style==1)
            {
                // Unequal paired blades with a deliberate vertical gap and linking floor.
                Box(c,q,new Vector3(-w*.29f,h*.5f,0),new Vector3(w*.43f,h,d),facade);
                Box(c,q,new Vector3(w*.29f,h*.38f,0),new Vector3(w*.43f,h*.76f,d*.87f),facade);
                Box(c,q,new Vector3(0,h*.63f,0),new Vector3(w*.65f,5,d*.57f),steel);
                Box(c,q,new Vector3(-w*.29f,h+.8f,0),new Vector3(w*.44f,1.6f,d+1),concrete);
            }
            else
            {
                float lower=h*.64f,upper=h*.24f;
                Box(c,q,new Vector3(0,lower*.5f,0),new Vector3(w,lower,d),facade);
                Box(c,q,new Vector3(style==2?w*.12f:0,lower+upper*.5f,0),new Vector3(w*.79f,upper,d*.81f),facade);
                if(style==3)
                {
                    // An open mechanical crown replaces the repeated opaque roof block.
                    Box(c,q,new Vector3(0,h*.88f+1,0),new Vector3(w*.64f,2,d*.64f),steel);
                    for(int side=-1;side<=1;side+=2)Box(c,q,new Vector3(side*w*.27f,h*.945f,0),new Vector3(.65f,h*.13f,.65f),trim);
                    Box(c,q,new Vector3(0,h*1.015f,0),new Vector3(w*.57f,.65f,1.2f),concrete);
                }
                else if(style==4)
                    Box(c,q,new Vector3(w*.15f,h*.895f,-d*.15f),new Vector3(w*.5f,3,d*.48f),concrete);
                else
                    Box(c,q,new Vector3(style==2?w*.22f:-w*.12f,h*.94f,-d*.08f),new Vector3(w*.46f,h*.12f,d*.53f),steel);
                Box(c,q,new Vector3(0,lower+.4f,0),new Vector3(w+1,.8f,d+1),concrete);
                Box(c,q,new Vector3(style==2?w*.12f:0,lower+upper+.4f,0),new Vector3(w*.8f,.8f,d*.83f),trim);
            }
            if(detailed)
            {
                // Close buildings have a matte service elevation and deep construction
                // breaks, so the camera reads volumes instead of equally luminous boxes.
                Box(c,q,new Vector3(w*.505f,h*.32f,0),new Vector3(.42f,h*.64f,d*.96f),concrete);
                Box(c,q,new Vector3(-w*.14f,h*.32f,d*.512f),new Vector3(w*.13f,h*.64f,.7f),steel);
                for(int band=1;band<=2;band++)
                    Box(c,q,new Vector3(0,h*.2f*band,0),new Vector3(w+.7f,2.3f,d+.7f),steel);
                for(int fin=0;fin<5;fin++)
                {
                    float finX=-w*.42f+fin*w*.21f;
                    if(style==1&&Mathf.Abs(finX)<w*.13f)continue;
                    Box(c,q,new Vector3(finX,h*.31f,d*.54f),new Vector3(.38f,h*.6f,1.1f),concrete);
                }
                if(style==1)
                {
                    Box(c,q,new Vector3(-w*.29f,h*.84f,d*.51f),new Vector3(w*.4f,1.2f,.8f),trim);
                    Box(c,q,new Vector3(w*.29f,h*.77f,0),new Vector3(w*.44f,1.5f,d*.9f),concrete);
                }
            }
            for(int side=-1;side<=1;side+=2)
            {
                Box(c,q,new Vector3(side*w*.48f,h*.32f,-d*.49f),new Vector3(.8f,h*.64f,1.2f),concrete);
                Box(c,q,new Vector3(side*w*.48f,h*.32f,d*.49f),new Vector3(.8f,h*.64f,1.2f),concrete);
            }
            if(style==0||style==3)
            {
                // A few architectural edge/crown lights establish readable district identity.
                Box(c,q,new Vector3(w*.24f,h*.7f,d*.42f),new Vector3(.18f,h*.22f,.2f),style==0?cyan:warm);
                Box(c,q,new Vector3(0,h+1,d*.29f),new Vector3(w*.52f,.18f,.22f),style==0?cyan:warm);
                Pipe(c,q,new Vector3(0,h,0),new Vector3(0,h+9,0),.3f,trim);
            }
            if(style==4)
            {
                // Vertical utility sign is a restrained grouped light mark, not a bright slab.
                Box(c,q,new Vector3(-w*.5f-1,h*.59f,0),new Vector3(1.8f,19,3.5f),steel);
                for(int mark=0;mark<4;mark++)Box(c,q,new Vector3(-w*.5f-2,h*.59f-6+mark*4,0),new Vector3(.15f,1.2f,2.4f),warm);
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
                        mats[m]=name.Contains("Glass")?facades[(index+i)%4]:name.Contains("Metal")?trim:name.Contains("Ivory")?concrete:steel;
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
