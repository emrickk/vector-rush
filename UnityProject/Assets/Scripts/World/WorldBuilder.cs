using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    public sealed class WorldBuilder : MonoBehaviour
    {
        TrackPath track;
        Material road, ivory, graphite, signal, cyan, metal, glass, rock;
        readonly List<Mesh> ownedMeshes=new List<Mesh>();
        readonly List<Material> ownedMaterials=new List<Material>();
        Texture2D roadGrain;
        ReflectionProbe coastalProbe;
        public Material Ivory => ivory;
        public Material Signal => signal;
        public Material Graphite => graphite;
        public Material Engine => cyan;
        public Material Glass => glass;
        public Material Metal => metal;
        public Material MakeMaterial(string name,Color color,float smooth=.5f,float metallic=0,Color? emission=null)
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(!shader) shader=Shader.Find("Standard");
            var template=Resources.Load<Material>("SurfaceLit");
            var m=template?new Material(template):new Material(shader);m.name=name;m.SetColor("_BaseColor",color);m.color=color;
            m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);
            m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission??Color.black);
            ownedMaterials.Add(m);return m;
        }
        public void Build(TrackPath path)
        {
            track=path;track.Ensure();
            road=MakeMaterial("Graphite composite running surface",new Color(.16f,.205f,.23f),.33f,.12f);
            roadGrain=new Texture2D(256,256,TextureFormat.RGBA32,true){name="Asphalt microaggregate",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=8};
            var grainRandom=new System.Random(419);var pixels=new Color[256*256];
            for(int i=0;i<pixels.Length;i++){float n=.78f+(float)grainRandom.NextDouble()*.22f;pixels[i]=new Color(n,n,n,1);}
            roadGrain.SetPixels(pixels);roadGrain.Apply(true,true);road.SetTexture("_BaseMap",roadGrain);road.SetTextureScale("_BaseMap",new Vector2(12,5));
            ivory=MakeMaterial("Ceramic ivory",new Color(.88f,.88f,.79f),.54f,.18f);
            graphite=MakeMaterial("Structural graphite",new Color(.025f,.05f,.065f),.55f,.5f);
            signal=MakeMaterial("Signal citron",new Color(.56f,.75f,.006f),.4f,0f);
            cyan=MakeMaterial("Ion turquoise",new Color(.015f,.88f,.81f),.81f,.3f,new Color(.075f,4.4f,4.05f));
            metal=MakeMaterial("Brushed titanium",new Color(.3f,.37f,.4f),.7f,.8f);
            glass=MakeMaterial("Smoked glass",new Color(.025f,.1f,.14f),.98f,.7f);
            rock=MakeMaterial("Basalt",new Color(.17f,.23f,.23f),.08f);
            Ribbon("Running surface",-11,11,0,road,true);
            Ribbon("Track underbody",-12,12,-1.7f,graphite,false);
            for(int side=-1;side<=1;side+=2){
                Ribbon("Ivory edge",side*10.2f,side*10.48f,.025f,ivory,false);
                Ribbon("Drain channel",side*10.6f,side*10.85f,.03f,metal,false);
                Ribbon("Shoulder",side*11f,side*12f,-.02f,ivory,true);
                Wall(side);Ribbon("Barrier luminous cap",side*11.72f,side*12.06f,2.03f,cyan,false);
            }
            for(int i=0;i<200;i++) {
                var f=track.Evaluate(i/200f);
                var joint=Box("Deck expansion joint",f.Position+f.Up*.02f,new Vector3(20,.018f,.12f),Quaternion.LookRotation(f.Forward,f.Up),graphite);
                joint.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
                if(i%2==0) {
                    Box("Lane datum",f.Position+f.Up*.025f,new Vector3(.13f,.025f,3),Quaternion.LookRotation(f.Forward,f.Up),ivory);
                }
                if(i%5==0) Support(f);
            }
            for(int i=0;i<7;i++) BoostStrip(.06f+i*.135f);
            Gate(0,"SOLSTICE",true);
            Gate(.29f,"VECTOR  /  01",false);Gate(.56f,"PACIFIC  /  02",false);Gate(.8f,"FINAL SECTOR",false);
            for(int i=0;i<7;i++) {float t=.35f+i*.009f;var f=track.Evaluate(t);
                var q=Quaternion.LookRotation(f.Forward,f.Up);
                for(int s=-1;s<=1;s+=2) Box("Aero tunnel rib",f.Position+f.Right*s*14+f.Up*9,new Vector3(.9f,19,1.1f),q*Quaternion.Euler(0,0,s*12),ivory);
                Box("Tunnel crown",f.Position+f.Up*18,new Vector3(24,.75f,1.1f),q,ivory);
            }
            BuildLandscape();gameObject.AddComponent<CoastalSetDressing>().Build(this,track);BuildLighting();
        }
        void Ribbon(string name,float left,float right,float height,Material mat,bool collider)
        {
            if(left>right){float v=left;left=right;right=v;}
            const int n=960;
            int columns=right-left>5f?12:1, stride=columns+1;
            var vertices=new Vector3[(n+1)*stride];var uv=new Vector2[vertices.Length];var tris=new int[n*columns*6];
            for(int i=0;i<=n;i++) {var f=track.Evaluate((float)i/n);
                for(int j=0;j<=columns;j++){
                    float across=(float)j/columns;int index=i*stride+j;
                    vertices[index]=f.Position+f.Right*Mathf.Lerp(left,right,across)+f.Up*height;
                    uv[index]=new Vector2(across,i*.25f);
                    if(i<n&&j<columns){int k=(i*columns+j)*6,a=index;
                        tris[k]=a;tris[k+1]=a+stride;tris[k+2]=a+1;
                        tris[k+3]=a+1;tris[k+4]=a+stride;tris[k+5]=a+stride+1;
                    }
                }
            }
            MeshObject(name,vertices,uv,tris,mat,collider);
        }
        void Wall(int side)
        {
            const int n=960;var vertices=new Vector3[(n+1)*4];var uv=new Vector2[vertices.Length];var tris=new List<int>();
            for(int i=0;i<=n;i++) {var f=track.Evaluate((float)i/n);
                for(int j=0;j<4;j++){float x=(j<2?11.7f:12.1f)*side;float y=(j==0||j==3)?-.3f:2f;
                    vertices[i*4+j]=f.Position+f.Right*x+f.Up*y;uv[i*4+j]=new Vector2(j,i*.2f);}
                if(i<n)for(int j=0;j<4;j++){int a=i*4+j,b=i*4+(j+1)%4,c=a+4,d=b+4;
                    if(side>0)tris.AddRange(new[]{a,c,b,b,c,d});else tris.AddRange(new[]{a,b,c,b,d,c});}
            }
            MeshObject("Safety barrier",vertices,uv,tris.ToArray(),ivory,true);
        }
        void MeshObject(string name,Vector3[] vertices,Vector2[] uv,int[] triangles,Material mat,bool collider)
        {
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();ownedMeshes.Add(mesh);
            var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
            if(collider) go.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
        public GameObject Box(string name,Vector3 position,Vector3 scale,Quaternion rotation,Material mat)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(transform,false);
            g.transform.SetPositionAndRotation(position,rotation);g.transform.localScale=scale;
            g.GetComponent<Renderer>().sharedMaterial=mat;Destroy(g.GetComponent<Collider>());return g;
        }
        void Support(TrackFrame f)
        {
            var q=Quaternion.LookRotation(f.Forward,Vector3.up);
            Box("Ocean pier",new Vector3(f.Position.x,(f.Position.y-4)*.5f,f.Position.z),new Vector3(3.5f,f.Position.y-4,5),q,ivory);
            Box("Deck bearing",f.Position-Vector3.up*3,new Vector3(23,1.8f,4),q,metal);
            Box("Pier foot",new Vector3(f.Position.x,1,f.Position.z),new Vector3(11,2,12),q,graphite);
        }
        void BoostStrip(float t)
        {
            for(int j=0;j<5;j++){var f=track.Evaluate(t+j*.0018f);var q=Quaternion.LookRotation(f.Forward,f.Up);
                for(int s=-1;s<=1;s+=2)Box("Boost chevron",f.Position+f.Right*s*2+f.Up*.045f,new Vector3(4.3f,.04f,.4f),q*Quaternion.Euler(0,s*-25,0),cyan);}
        }
        void Gate(float t,string title,bool start)
        {
            var f=track.Evaluate(t);var q=Quaternion.LookRotation(f.Forward,f.Up);
            for(int s=-1;s<=1;s+=2){Box("Pylon",f.Position+f.Right*s*14+f.Up*6,new Vector3(2,16,2),q*Quaternion.Euler(0,0,s*9),ivory);
                Box("Pylon trim",f.Position+f.Right*s*13.5f+f.Up*7,new Vector3(.28f,14,2.1f),q,signal);}
            Box("Gate header",f.Position+f.Up*13,new Vector3(29,3,1.7f),q,graphite);
            Label(title,f.Position+f.Up*13-f.Forward*.9f,q,1.7f,ivory.color);
            if(start){for(int r=0;r<3;r++)for(int j=0;j<22;j++)Box("Start grid",f.Position+f.Right*(j-10.5f)+f.Forward*(r-1)+f.Up*.04f, new Vector3(1,.035f,1),q,(r+j)%2==0?ivory:graphite);}
        }
        void Label(string text,Vector3 p,Quaternion q,float size,Color color)
        {
            var g=new GameObject(text);g.transform.SetParent(transform,false);g.transform.SetPositionAndRotation(p,q);
            var tm=g.AddComponent<TextMesh>();tm.text=text;tm.fontSize=90;tm.characterSize=size*.1f;tm.anchor=TextAnchor.MiddleCenter;tm.color=color;
        }
        void BuildLandscape()
        {
            Shader waterShader=Shader.Find("VectorRush/Ocean");
            var water=waterShader?new Material(waterShader):MakeMaterial("Ocean",new Color(.015f,.25f,.38f),.95f,.4f);
            if(waterShader)ownedMaterials.Add(water);
            Box("Pacific Ocean",new Vector3(0,-1,0),new Vector3(16000,.5f,16000),Quaternion.identity,water);
            var random=new System.Random(1181);
            for(int i=0;i<30;i++) {
                float angle=(float)random.NextDouble()*Mathf.PI*2;
                float radius= i<30? 95+(float)random.NextDouble()*65 : 550+(float)random.NextDouble()*700;
                Vector3 p=new Vector3(Mathf.Sin(angle)*radius,0,Mathf.Cos(angle)*radius);
                float height=24+(float)random.NextDouble()*100;
                if(i<30){
                    Box("Coastal tower",p+Vector3.up*height*.5f,new Vector3(12+(float)random.NextDouble()*18,height,15+(float)random.NextDouble()*18),Quaternion.Euler(0,i*19,0),i%3==0?glass:ivory);
                    Box("Tower crown",p+Vector3.up*(height+1),new Vector3(23,2,23),Quaternion.Euler(0,i*19,0),signal);
                    for(int k=12;k<height;k+=9) Box("Facade band",p+Vector3.up*k,new Vector3(24,.6f,24),Quaternion.Euler(0,i*19,0),metal);
                }
            }
            var start=track.Evaluate(.015f);var rot=Quaternion.LookRotation(start.Forward,start.Up);
            for(int i=0;i<6;i++){
                Box("Grandstand terrace",start.Position+start.Right*(24+i*3)+start.Up*(i*1.5f-1),new Vector3(4,1.5f,85),rot,ivory);
                Box("Grandstand seating",start.Position+start.Right*(24+i*3)+start.Up*(i*1.5f),new Vector3(2,.3f,83),rot,i%2==0?signal:graphite);
            }
            for(int bay=-3;bay<=3;bay++){
                Vector3 along=start.Forward*bay*12;
                Box("Grandstand roof blade",start.Position+along+start.Right*32+start.Up*14,new Vector3(24,.3f,11),rot*Quaternion.Euler(0,0,-9),ivory);
                Box("Grandstand cantilever",start.Position+along+start.Right*33+start.Up*13.4f,new Vector3(22,.7f,.35f),rot*Quaternion.Euler(0,0,-9),metal);
                Box("Grandstand roof column",start.Position+along+start.Right*43+start.Up*9,new Vector3(.6f,9,.6f),rot,metal);
                Box("Grandstand front fascia",start.Position+along+start.Right*20.2f+start.Up*15.85f,new Vector3(.3f,.9f,11),rot,graphite);
            }
        }
        void BuildLighting()
        {
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.5f,.72f,.79f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0007f;
            RenderSettings.ambientMode=AmbientMode.Custom;var ambient=new SphericalHarmonicsL2();ambient.AddAmbientLight(new Color(.36f,.43f,.49f));RenderSettings.ambientProbe=ambient;
            var sun=new GameObject("Pacific afternoon sun");sun.transform.SetParent(transform);sun.transform.rotation=Quaternion.Euler(32,-32,0);
            var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.965f,.88f);light.intensity=1.65f;light.shadows=LightShadows.Soft;light.shadowStrength=.75f;RenderSettings.sun=light;
            var skyShader=Shader.Find("Skybox/Procedural");if(skyShader){var sky=new Material(skyShader);ownedMaterials.Add(sky);sky.SetFloat("_SunSize",.025f);sky.SetFloat("_AtmosphereThickness",.8f);sky.SetColor("_SkyTint",new Color(.48f,.65f,.76f));sky.SetColor("_GroundColor",new Color(.25f,.4f,.45f));sky.SetFloat("_Exposure",1.15f);RenderSettings.skybox=sky;}
            var probeObject=new GameObject("Coastal reflection environment");probeObject.transform.SetParent(transform);probeObject.transform.position=new Vector3(0,50,-180);
            coastalProbe=probeObject.AddComponent<ReflectionProbe>();coastalProbe.mode=ReflectionProbeMode.Realtime;coastalProbe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;coastalProbe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;coastalProbe.resolution=256;coastalProbe.size=new Vector3(1800,600,1800);coastalProbe.farClipPlane=2500;coastalProbe.intensity=1f;
        }
        IEnumerator Start(){yield return new WaitForEndOfFrame();if(coastalProbe)coastalProbe.RenderProbe();}
        void OnDestroy(){foreach(var m in ownedMeshes)if(m)Destroy(m);foreach(var m in ownedMaterials)if(m)Destroy(m);if(roadGrain)Destroy(roadGrain);}
    }
}
