using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Visual-only Meridian district; the road, physics and driving camera remain authoritative.</summary>
    public sealed class OpeningCity : MonoBehaviour
    {
        public const string Revision = "meridian-opening-01";
        public static bool Enabled { get; private set; }
        sealed class Site
        {
            public string asset;
            public Vector3 center;
            public Quaternion rotation;
            public Vector2 half;
            public float scale;
        }
        static readonly List<Site> sites = new List<Site>();
        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        readonly List<Mesh> meshes = new List<Mesh>();
        readonly Dictionary<Material,List<CombineInstance>> batches = new Dictionary<Material,List<CombineInstance>>();
        Mesh cube;

        public static void Prepare(TrackPath track)
        {
            sites.Clear();
            var args = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(args, "-vrOpeningCity");
            Enabled = !(flag >= 0 && flag + 1 < args.Length && args[flag + 1] == "off");
            foreach(var asset in new[]{"MeridianExchange","TransitTerraces","RiversideOffices","CanalWorks"})
                if(!Resources.Load<GameObject>("OpeningCity/"+asset)) Enabled=false;
            if(!Enabled) return;
            AddWorldSite(track,"MeridianExchange",new Vector3(245,0,280),Quaternion.Euler(0,-12,0),1,new Vector2(36,31));
            AddSite(track,"TransitTerraces",.175f,-105,1,new Vector2(52,40));
            AddSite(track,"RiversideOffices",.102f,89,1,new Vector2(26,24));
            AddSite(track,"RiversideOffices",.291f,161,.82f,new Vector2(26,24));
            AddSite(track,"CanalWorks",.055f,-75,1,new Vector2(29,27));
            AddSite(track,"CanalWorks",.300f,-81,1,new Vector2(29,27));
            AddSite(track,"CanalWorks",.360f,-115,1,new Vector2(29,27));
            AddWorldSite(track,"RiversideOffices",new Vector3(-44,0,195),Quaternion.Euler(0,-18,0),1.22f,new Vector2(26,24));
            AddWorldSite(track,"TransitTerraces",new Vector3(-40,0,110),Quaternion.Euler(0,-18,0),1,new Vector2(52,40));
        }

        static void AddWorldSite(TrackPath track,string asset,Vector3 c,Quaternion q,float scale,Vector2 half)
        {
            for(int i=0;i<1200;i++)
            {
                var p=Quaternion.Inverse(q)*(track.Evaluate(i/1200f).Position-c);
                float dx=Mathf.Max(0,Mathf.Abs(p.x)-half.x*scale),dz=Mathf.Max(0,Mathf.Abs(p.z)-half.y*scale);
                if(dx*dx+dz*dz<22*22)throw new InvalidOperationException("Meridian reveal site crosses course clearance.");
            }
            sites.Add(new Site{asset=asset,center=c,rotation=q,scale=scale,half=half*scale});
        }

        static void AddSite(TrackPath track,string asset,float progress,float offset,float scale,Vector2 half)
        {
            var f=track.Evaluate(progress);
            var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up).normalized,Vector3.up);
            var c=f.Position+q*Vector3.right*offset;c.y=0;
            // Full-course footprint clearance, including adjacent portions of the bend.
            for(int attempt=0;attempt<12;attempt++)
            {
                bool clear=true;
                for(int i=0;i<1200;i++)
                {
                    var p=Quaternion.Inverse(q)*(track.Evaluate(i/1200f).Position-c);
                    float dx=Mathf.Max(0,Mathf.Abs(p.x)-half.x*scale);
                    float dz=Mathf.Max(0,Mathf.Abs(p.z)-half.y*scale);
                    if(dx*dx+dz*dz<22*22){clear=false;break;}
                }
                if(clear){sites.Add(new Site{asset=asset,center=c,rotation=q,scale=scale,half=half*scale});return;}
                c+=q*Vector3.right*Mathf.Sign(offset)*10;
            }
            Debug.LogError("VR_OPENING_CITY no safe footprint for "+asset);
        }

        public static bool Reserved(Vector3 c,Quaternion q,Vector2 half)
        {
            if(!Enabled)return false;
            Vector3 a=q*Vector3.right,b=q*Vector3.forward;
            foreach(var site in sites)
            {
                Vector3 delta=c-site.center;delta.y=0;
                Vector3 u=site.rotation*Vector3.right,v=site.rotation*Vector3.forward;
                bool separate=false;
                foreach(var axis in new[]{a,b,u,v})
                {
                    float extent=half.x*Mathf.Abs(Vector3.Dot(a,axis))+half.y*Mathf.Abs(Vector3.Dot(b,axis))+
                        site.half.x*Mathf.Abs(Vector3.Dot(u,axis))+site.half.y*Mathf.Abs(Vector3.Dot(v,axis))+6;
                    if(Mathf.Abs(Vector3.Dot(delta,axis))>extent){separate=true;break;}
                }
                if(!separate)return true;
            }
            return false;
        }

        public void Build(WorldBuilder world,TrackPath track)
        {
            if(!Enabled)return;
            materials["OC_Concrete"]=world.MakeMaterial("Meridian / cast slate",new Color(.32f,.365f,.39f),.38f,.04f);
            materials["OC_Pale"]=world.MakeMaterial("Meridian / mineral edge returns",new Color(.48f,.49f,.46f),.44f,.06f);
            materials["OC_Steel"]=world.MakeMaterial("Meridian / midnight steel",new Color(.055f,.085f,.11f),.50f,.4f);
            materials["OC_Bronze"]=world.MakeMaterial("Meridian / anodized bronze",new Color(.24f,.15f,.075f),.52f,.6f);
            materials["OC_Glass"]=world.MakeMaterial("Meridian / smoked blue glazing",new Color(.035f,.085f,.12f),.85f,.2f);
            materials["OC_Warm"]=world.MakeMaterial("Meridian / warm soffit",new Color(.8f,.55f,.25f),.35f,0,new Color(1.4f,.74f,.28f));
            materials["OC_Cool"]=world.MakeMaterial("Meridian / service diffuser",new Color(.4f,.58f,.64f),.35f,0,new Color(.42f,.8f,.94f));
            materials["OC_TransitEdge"]=world.MakeMaterial("Meridian / low service markers",new Color(.3f,.21f,.09f),.35f,0,new Color(.32f,.15f,.055f));
            materials["OC_ServiceDeck"]=world.MakeMaterial("Meridian / service asphalt",new Color(.085f,.12f,.145f),.43f,.03f);
            var rooms=new Material(Shader.Find("VectorRush/NightWindows")){name="Meridian / occupied suites"};
            rooms.SetColor("_BaseColor",new Color(.075f,.13f,.17f));
            rooms.SetColor("_WarmColor",new Color(.72f,.44f,.23f));
            rooms.SetColor("_CoolColor",new Color(.27f,.45f,.57f));
            rooms.SetFloat("_Density",.63f);rooms.SetFloat("_Intensity",.72f);
            rooms.SetFloat("_CellWidth",2.3f);rooms.SetFloat("_FloorHeight",4f);
            rooms.SetFloat("_Seed",41);rooms.SetFloat("_BandInterval",6);
            materials["OC_Rooms"]=rooms;
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube=primitive.GetComponent<MeshFilter>().sharedMesh;Destroy(primitive);
            foreach(var site in sites)
            {
                var obj=Instantiate(Resources.Load<GameObject>("OpeningCity/"+site.asset),site.center,site.rotation,transform);
                obj.name="Meridian / "+site.asset;
                obj.transform.localScale=Vector3.one*site.scale;
                var bounds=new Bounds(site.center,Vector3.zero);
                foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
                {
                    var slots=renderer.sharedMaterials;
                    for(int i=0;i<slots.Length;i++)
                    {
                        string key=slots[i]?slots[i].name.Split(' ')[0]:"";
                        if(materials.TryGetValue(key,out var material))slots[i]=material;
                        else Debug.LogError("VR_OPENING_CITY unmapped material "+key);
                    }
                    renderer.sharedMaterials=slots;renderer.shadowCastingMode=ShadowCastingMode.On;
                    bounds.Encapsulate(renderer.bounds);
                }
                Debug.Log("VR_OPENING_CITY asset="+site.asset+" center="+site.center.ToString("F3")+" bounds="+bounds+" scale="+site.scale);
                // Mounted architectural washers. Their cones face the building, away from the racing deck.
                float h=site.asset=="MeridianExchange"?85:site.asset=="RiversideOffices"?40:22;
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 socket=new Vector3(side*site.half.x*.78f,9,-site.half.y-2);
                    Box(site.center+site.rotation*socket,new Vector3(1,.45f,.7f),site.rotation,materials["OC_Steel"]);
                    Spot("Meridian facade wash",site.center+site.rotation*socket,
                        site.center+site.rotation*new Vector3(side*site.half.x*.45f,h,0),
                        h>50?2300:1050,h>50?155:95,75,side<0?new Color(.65f,.80f,1):new Color(1,.76f,.48f));
                }
            }
            BuildViaduct(track);
            BuildLowerTransit(track);
            Flush();
            Debug.Log("VR_OPENING_CITY enabled=true revision="+Revision+" sites="+sites.Count+" roadCameraPhysicsUnchanged=true");
        }

        void BuildViaduct(TrackPath track)
        {
            for(int i=1;i<=13;i++)
            {
                float t=i*.025f;var f=track.Evaluate(t);
                var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up),Vector3.up);
                var deckQ=Quaternion.LookRotation(f.Forward,f.Up);
                // Bearings and cap connect directly to the retained deck soffit.
                Box(f.Position-f.Up*2.35f,new Vector3(23,1.1f,5.6f),deckQ,materials["OC_Steel"]);
                Box(f.Position-f.Up*4,new Vector3(24,2.1f,5),deckQ,materials["OC_Concrete"]);
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 bottom=new Vector3(f.Position.x,1.2f,f.Position.z)+q*Vector3.right*side*6.5f;
                    Vector3 top=f.Position+f.Right*side*8-f.Up*5;
                    Beam(bottom,top,2.6f,materials["OC_Concrete"]);
                    Box(bottom,new Vector3(7,2.4f,9),q,materials["OC_Steel"]);
                    Vector3 legSocket=Vector3.Lerp(bottom,top,Mathf.Clamp01((top.y-7-bottom.y)/(top.y-bottom.y)));
                    Vector3 capSocket=f.Position+f.Right*side*2-f.Up*5;
                    Beam(legSocket,capSocket,1.25f,materials["OC_Steel"]);
                    Box(top+q*Vector3.forward*2.7f,new Vector3(1.2f,.3f,.25f),q,materials["OC_Warm"]);
                }
            }
        }

        void BuildLowerTransit(TrackPath track)
        {
            // A continuous inhabited service viaduct, 20 m below the racing deck.
            // Short chords overlap at joints; this is visual-only geometry without colliders.
            for(int i=0;i<38;i++)
            {
                float t=.035f+i*.0075f;
                var a=track.Evaluate(t);var b=track.Evaluate(t+.0075f);
                Vector3 p=a.Position-a.Right*38-Vector3.up*23;
                Vector3 end=b.Position-b.Right*38-Vector3.up*23;
                var q=Quaternion.LookRotation(end-p,Vector3.up);
                float length=Vector3.Distance(p,end)+.55f;
                Box((p+end)*.5f,new Vector3(12,1.5f,length),q,materials["OC_Concrete"]);
                Box((p+end)*.5f+q*Vector3.up*.78f,new Vector3(11,.08f,length),q,materials["OC_ServiceDeck"]);
                for(int side=-1;side<=1;side+=2)
                {
                    Box((p+end)*.5f+q*Vector3.right*side*5.7f+Vector3.up*.95f,new Vector3(.5f,1,length),q,materials["OC_Steel"]);
                    Box((p+end)*.5f+q*Vector3.right*side*5.75f+Vector3.up*1.48f,new Vector3(.18f,.13f,length),q,materials["OC_TransitEdge"]);
                }
                if(i%3==0)
                {
                    float height=p.y-.9f;
                    Box(new Vector3(p.x,height*.5f,p.z),new Vector3(2.2f,height,3.5f),q,materials["OC_Concrete"]);
                    Box(new Vector3(p.x,.4f,p.z),new Vector3(5,.8f,7),q,materials["OC_Steel"]);
                    Vector3 lamp=p+q*Vector3.right*5+Vector3.up*5;
                    Box(lamp-Vector3.up*2,new Vector3(.25f,5,.25f),q,materials["OC_Steel"]);
                    Box(lamp+q*Vector3.left,new Vector3(2.3f,.2f,.65f),q,materials["OC_Warm"]);
                    Spot("Lower transit road pool",lamp+q*Vector3.left,p,180,24,100,new Color(1,.73f,.43f));
                }
            }
            ServiceTerminal(track,.035f,true);
            ServiceTerminal(track,.32f,false);
        }
        void ServiceTerminal(TrackPath track,float t,bool start)
        {
            var f=track.Evaluate(t);
            Vector3 c=f.Position-f.Right*38-Vector3.up*23;
            var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up));
            if(start)q*=Quaternion.Euler(0,180,0);
            c-=q*Vector3.forward*6;
            Box(c,new Vector3(21,1.8f,28),q,materials["OC_Concrete"]);
            Box(c+Vector3.up*7,new Vector3(23,1.2f,30),q,materials["OC_Concrete"]);
            Box(c+q*Vector3.forward*13+Vector3.up*3.5f,new Vector3(20,7,1.4f),q,materials["OC_Steel"]);
            for(int side=-1;side<=1;side+=2)
            {
                Box(c+q*Vector3.right*side*9.2f+Vector3.up*3.6f,new Vector3(.8f,5,24),q,materials["OC_Rooms"]);
                Box(c+q*Vector3.right*side*8.7f+Vector3.up*6.1f,new Vector3(.4f,.2f,23),q,materials["OC_Warm"]);
                for(int end=-1;end<=1;end+=2)
                {
                    var p=c+q*new Vector3(side*9.5f,0,end*11);
                    Box(new Vector3(p.x,(c.y+7)*.5f,p.z),new Vector3(1.6f,c.y+7,2),q,materials["OC_Concrete"]);
                    Box(new Vector3(p.x,.5f,p.z),new Vector3(4,1,5),q,materials["OC_Steel"]);
                }
            }
        }

        void Spot(string name,Vector3 p,Vector3 target,float intensity,float range,float angle,Color tint)
        {
            var obj=new GameObject(name);obj.transform.SetParent(transform);
            obj.transform.SetPositionAndRotation(p,Quaternion.LookRotation(target-p));
            var light=obj.AddComponent<Light>();light.type=LightType.Spot;light.color=tint;
            light.intensity=intensity;light.range=range;light.spotAngle=angle;light.innerSpotAngle=angle*.55f;
            light.shadows=LightShadows.None;light.cullingMask=~(1<<8);
        }
        void Beam(Vector3 a,Vector3 b,float width,Material material)
            =>Box((a+b)*.5f,new Vector3(width,width,(b-a).magnitude),Quaternion.LookRotation(b-a),material);
        void Box(Vector3 p,Vector3 size,Quaternion q,Material material)
        {
            if(!batches.TryGetValue(material,out var items)){items=new List<CombineInstance>();batches.Add(material,items);}
            items.Add(new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(p,q,size)});
        }
        void Flush()
        {
            foreach(var pair in batches)
            {
                var mesh=new Mesh{name="Meridian infrastructure / "+pair.Key.name,indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(pair.Value.ToArray(),true,true);meshes.Add(mesh);
                var obj=new GameObject(mesh.name);obj.transform.SetParent(transform,false);
                obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            batches.Clear();
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);if(materials.TryGetValue("OC_Rooms",out var rooms)&&rooms)Destroy(rooms);}
    }
}
