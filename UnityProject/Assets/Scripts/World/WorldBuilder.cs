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
        Texture2D roadGrain,roadNormals,roadSmoothness;
        ReflectionProbe coastalProbe;
        public Material Ivory => ivory;
        public Material Signal => signal;
        public Material Graphite => graphite;
        public Material Engine => cyan;
        public Material Glass => glass;
        public Material Metal => metal;
        public Material MakeMaterial(string name,Color color,float smooth=.5f,float metallic=0,Color? emission=null,string templateName="SurfaceLit")
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(!shader) shader=Shader.Find("Standard");
            var template=Resources.Load<Material>(templateName);
            var m=template?new Material(template):new Material(shader);m.name=name;m.SetColor("_BaseColor",color);m.color=color;
            m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);
            m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission??Color.black);
            ownedMaterials.Add(m);return m;
        }
        public void Build(TrackPath path)
        {
            track=path;track.Ensure();
            road=MakeMaterial("Satin graphite running deck",new Color(.13f,.145f,.165f),.9f,0f,templateName:"RoadSurface");
            const int textureSize=512;
            roadGrain=new Texture2D(textureSize,textureSize,TextureFormat.RGBA32,true){name="Deck aggregate",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=8};
            roadNormals=new Texture2D(textureSize,textureSize,TextureFormat.RGBA32,true,true){name="Deck fine relief",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=8};
            roadSmoothness=new Texture2D(textureSize,textureSize,TextureFormat.RGBA32,true,true){name="Satin deck service wear",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=8};
            var random=new System.Random(419);var pixels=new Color[textureSize*textureSize];var normals=new Color[pixels.Length];var masks=new Color[pixels.Length];
            for(int y=0;y<textureSize;y++)for(int x=0;x<textureSize;x++){
                int index=y*textureSize+x;float grain=(float)random.NextDouble();
                float patch=Mathf.PerlinNoise(x*.021f+2.7f,y*.009f+1.1f);
                float shade=.88f+grain*.07f+patch*.03f;pixels[index]=new Color(shade,shade,shade,1);
                normals[index]=new Color(.5f+(grain-.5f)*.08f,.5f+((float)random.NextDouble()-.5f)*.065f,1,1);
                float u=x/(float)(textureSize-1),v=y/(float)(textureSize-1);
                float dampPatch=PeriodicNoise(u,v,(textureSize-1)*.021f,(textureSize-1)*.009f,2.7f,1.1f);
                float dampStreak=PeriodicNoise(u,v,(textureSize-1)*.12f,(textureSize-1)*.005f,0,0);
                // Deliberate satin material redesign: the original damp finish exposed unresolved
                // specular facets. Keep topology/normals/light shadows, and broaden the reflection.
                masks[index]=new Color(0,0,0,Mathf.Lerp(.35f,.5f,Mathf.SmoothStep(.2f,.8f,dampPatch*.65f+dampStreak*.35f)));
            }
            roadGrain.SetPixels(pixels);roadGrain.Apply(true,true);roadNormals.SetPixels(normals);roadNormals.Apply(true,true);roadSmoothness.SetPixels(masks);roadSmoothness.Apply(true,true);
            road.SetTexture("_BaseMap",roadGrain);road.SetTexture("_BumpMap",roadNormals);road.SetTexture("_MetallicGlossMap",roadSmoothness);road.SetTextureScale("_BaseMap",new Vector2(3,1));
            ivory=MakeMaterial("Ceramic ivory",new Color(.22f,.28f,.33f),.5f,.3f);
            graphite=MakeMaterial("Structural graphite",new Color(.025f,.05f,.065f),.55f,.5f);
            signal=MakeMaterial("Signal citron",new Color(.56f,.75f,.006f),.4f,0f);
            cyan=MakeMaterial("Ion turquoise",new Color(.01f,.52f,.67f),.6f,.1f,new Color(.025f,1.5f,2.2f));
            metal=MakeMaterial("Brushed titanium",new Color(.3f,.37f,.4f),.7f,.8f);
            glass=MakeMaterial("Smoked glass",new Color(.025f,.1f,.14f),.98f,.7f);
            rock=MakeMaterial("Basalt",new Color(.17f,.23f,.23f),.08f);
            Ribbon("Running surface",-11,11,0,road,true);
            Ribbon("Track underbody",-13.3f,13.3f,-1.7f,graphite,false,true);
            for(int side=-1;side<=1;side+=2){
                Ribbon("Ivory edge",side*10.2f,side*10.48f,.025f,ivory,false);
                Ribbon("Drain channel",side*10.6f,side*10.85f,.03f,metal,false);
                Ribbon("Shoulder",side*11f,side*12f,-.02f,ivory,true);
                Wall(side);Ribbon("Barrier luminous cap",side*11.78f,side*11.91f,2.03f,cyan,false);
                Ribbon("Maintenance ledge",side*12.1f,side*13.3f,-.12f,ivory,false);
                Fascia(side*13.3f,-1.7f,-.12f,ivory);
                Fascia(side*12.16f,-.1f,.40f,graphite);
            }
            for(int i=0;i<200;i++) {
                var f=track.Evaluate(i/200f);
                var joint=Box("Deck expansion joint",f.Position+f.Up*.02f,new Vector3(20,.009f,.035f),Quaternion.LookRotation(f.Forward,f.Up),graphite);
                joint.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
                if(i%2==0) {
                    Box("Lane datum",f.Position+f.Up*.025f,new Vector3(.13f,.025f,3),Quaternion.LookRotation(f.Forward,f.Up),ivory);
                }
                if(i%5==0) Support(f);
            }
            for(int i=0;i<7;i++) DirectionMarkings(.06f+i*.135f);
            Gate(0,"NOCTURNE",true);
            Gate(.29f,"VECTOR  /  01",false);Gate(.56f,"MIDNIGHT  /  02",false);Gate(.8f,"FINAL SECTOR",false);
            for(int i=0;i<7;i++) {float t=.35f+i*.009f;var f=track.Evaluate(t);
                var q=Quaternion.LookRotation(f.Forward,f.Up);
                for(int s=-1;s<=1;s+=2) Box("Aero tunnel rib",f.Position+f.Right*s*14+f.Up*9,new Vector3(.9f,19,1.1f),q*Quaternion.Euler(0,0,s*12),ivory);
                Box("Tunnel crown",f.Position+f.Up*18,new Vector3(24,.75f,1.1f),q,ivory);
            }
            gameObject.AddComponent<NightDistrict>().Build(this,track);gameObject.AddComponent<NightTrackLighting>().Build(this,track);BuildLighting();
        }
        static float PeriodicNoise(float u,float v,float periodX,float periodY,float offsetX,float offsetY)
        {
            // Blend translated samples so opposite tile edges have matching values
            // and slopes. Only the dampness mask uses this; grain and relief stay unchanged.
            float x=u*periodX+offsetX,y=v*periodY+offsetY;
            float blendX=Mathf.SmoothStep(0,1,u),blendY=Mathf.SmoothStep(0,1,v);
            float near=Mathf.Lerp(Mathf.PerlinNoise(x,y),Mathf.PerlinNoise(x-periodX,y),blendX);
            float far=Mathf.Lerp(Mathf.PerlinNoise(x,y-periodY),Mathf.PerlinNoise(x-periodX,y-periodY),blendX);
            return Mathf.Lerp(near,far,blendY);
        }
        void Ribbon(string name,float left,float right,float height,Material mat,bool collider,bool faceDown=false)
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
            if(faceDown)for(int i=0;i<tris.Length;i+=3){int swap=tris[i+1];tris[i+1]=tris[i+2];tris[i+2]=swap;}
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
        void Fascia(float lateral,float bottom,float top,Material material)
        {
            // Continuous deck construction follows the same sampled bank as the race surface.
            // This stays outside the existing safety collider; no new driving boundary is implied.
            const int n=960;var vertices=new Vector3[(n+1)*2];var uv=new Vector2[vertices.Length];var triangles=new int[n*6];
            for(int i=0;i<=n;i++){
                var f=track.Evaluate(i/(float)n);vertices[i*2]=f.Position+f.Right*lateral+f.Up*bottom;
                vertices[i*2+1]=f.Position+f.Right*lateral+f.Up*top;
                uv[i*2]=new Vector2(i*.2f,0);uv[i*2+1]=new Vector2(i*.2f,1);
                if(i==n)continue;int a=i*2,k=i*6;
                if(lateral>0){triangles[k]=a;triangles[k+1]=a+1;triangles[k+2]=a+2;triangles[k+3]=a+1;triangles[k+4]=a+3;triangles[k+5]=a+2;}
                else {triangles[k]=a;triangles[k+1]=a+2;triangles[k+2]=a+1;triangles[k+3]=a+1;triangles[k+4]=a+2;triangles[k+5]=a+3;}
            }
            MeshObject("Deck edge / continuous fascia",vertices,uv,triangles,material,false);
        }
        void MeshObject(string name,Vector3[] vertices,Vector2[] uv,int[] triangles,Material mat,bool collider)
        {
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();ownedMeshes.Add(mesh);
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
            // Follow the bank beneath the deck; a level bearing can rise through the low shoulder.
            Box("Deck bearing",f.Position-f.Up*3,new Vector3(23,1.8f,4),Quaternion.LookRotation(f.Forward,f.Up),metal);
            Box("Pier foot",new Vector3(f.Position.x,1,f.Position.z),new Vector3(11,2,12),q,graphite);
        }
        void DirectionMarkings(float t)
        {
            // Small non-emissive arrows at the shoulders communicate direction without implying a boost pad.
            for(int side=-1;side<=1;side+=2){var f=track.Evaluate(t);var q=Quaternion.LookRotation(f.Forward,f.Up);
                var p=f.Position+f.Right*side*8.3f+f.Up*.042f;
                Box("Painted route arrow shaft",p,new Vector3(.16f,.015f,2.5f),q,ivory);
                for(int wing=-1;wing<=1;wing+=2)
                    Box("Painted route arrow head",p+f.Forward*1.15f+f.Right*wing*.31f,new Vector3(.15f,.015f,1f),q*Quaternion.Euler(0,-wing*38,0),ivory);
            }
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
            if(!gameObject.AddComponent<AuthoredCity>().Build(this,track))Debug.LogWarning("Authored coastal city assets are missing");
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
            // Separate the distant city from the neutral foreground without lifting exposure or bloom.
            // RenderSettings converts its sRGB color on upload. Match the linear sky palette.
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.041f,.074f,.086f).gamma;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0018f;
            RenderSettings.ambientMode=AmbientMode.Custom;var ambient=new SphericalHarmonicsL2();ambient.AddAmbientLight(new Color(.14f,.16f,.18f));RenderSettings.ambientProbe=ambient;
            var moon=new GameObject("Midnight soft key");moon.transform.SetParent(transform);moon.transform.rotation=Quaternion.Euler(43,-28,0);
            var light=moon.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(.78f,.85f,.92f);light.intensity=.78f;light.shadows=LightShadows.Soft;light.shadowStrength=.4f;RenderSettings.sun=light;
            var skyShader=Shader.Find("VectorRush/Night Sky");if(skyShader){var sky=new Material(skyShader);ownedMaterials.Add(sky);RenderSettings.skybox=sky;}
            var probeObject=new GameObject("Night architecture reflection environment");probeObject.transform.SetParent(transform);probeObject.transform.position=new Vector3(0,58,-170);
            coastalProbe=probeObject.AddComponent<ReflectionProbe>();coastalProbe.mode=ReflectionProbeMode.Realtime;coastalProbe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;coastalProbe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;coastalProbe.resolution=256;coastalProbe.size=new Vector3(1800,600,1800);coastalProbe.farClipPlane=1600;coastalProbe.intensity=.8f;coastalProbe.cullingMask=~(1<<8);
        }
        IEnumerator Start(){yield return new WaitForEndOfFrame();if(coastalProbe)coastalProbe.RenderProbe();}
        void OnDestroy(){foreach(var m in ownedMeshes)if(m)Destroy(m);foreach(var m in ownedMaterials)if(m)Destroy(m);if(roadGrain)Destroy(roadGrain);if(roadNormals)Destroy(roadNormals);if(roadSmoothness)Destroy(roadSmoothness);}
    }
}
