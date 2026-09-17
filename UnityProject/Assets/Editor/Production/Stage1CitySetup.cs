using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorRush.Editor
{
    // Persistent, bounded art authoring. Never runs during gameplay or changes the course.
    public static class Stage1CitySetup
    {
        const string Root="Assets/Art/Stage1";
        public const string Scene="Assets/Scenes/Stage1City.unity";
        public const float InfrastructureStartProgress=-.012f;
        public const float InfrastructureStep=.00108f;
        public const int ProtectedTestFieldSpanCount=310;
        public const int CircuitInfrastructureSpanCount=926;
        static TrackPath track;static Transform parent;static int serial;
        static readonly Dictionary<Material,List<CombineInstance>> batches=new();
        static readonly List<Mesh> scratch=new();
        static readonly List<Bounds> occupied=new();
        static readonly List<Vector4> signData=new();
        static System.Random rng;
        static readonly Dictionary<Material,Material> glassOverlays=new();
        static Material dark,metal,concrete,cyan,magenta,warm,serviceAdvert,curtainGlass,roadReflection;static Material[] facades,farFacades,ads;
        static float R(float a,float b)=>(float)(a+rng.NextDouble()*(b-a));
        public static void Prepare()
        {
            string evidence=ProductionSceneSetup.RequiredFlag("-productionEvidence");Directory.CreateDirectory(evidence);
            var go=new GameObject("Stage1 import validation");var validationTrack=go.AddComponent<TrackPath>();
            var imported=ProductionArtImporter.ResolveImportedPackage(Path.GetFullPath("../SourceAssets/Stage1/city-s1"),ProductionSceneSetup.CourseHash(validationTrack),"Assets/Art/Stage1/Imported","Assets/World/Stage1");
            UnityEngine.Object.DestroyImmediate(go);
            File.WriteAllText(Path.Combine(evidence,"import.json"),"{\"status\":\"STRICT_BOUNDS_AND_MATERIAL_IMPORT_PASSED\",\"revision\":\"city-s1\",\"models\":30}");
            EditorSceneManager.OpenScene("Assets/Scenes/NocturneExperience.unity",OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();track=world.track;world.artRevision="stage1-circuit-lighting-01";
            Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();serial=0;rng=new System.Random(4381);batches.Clear();occupied.Clear();signData.Clear();scratch.Clear();glassOverlays.Clear();
            parent=new GameObject("Stage 1 • opening city passage").transform;parent.SetParent(world.transform,false);
            // Remove only existing foreground architecture in the bounded opening district.
            foreach(var zone in world.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Zone •")).ToArray())
                foreach(Transform child in zone.Cast<Transform>().ToArray())
                {float p=track.ClosestProgress(child.position);if(p<.31f||p>.975f)UnityEngine.Object.DestroyImmediate(child.gameObject);}
            dark=Mat("Graphite structure",new Color(.065f,.07f,.09f),.38f,.32f);
            metal=Mat("Satin titanium",new Color(.14f,.15f,.18f),.57f,.65f);
            concrete=Mat("Architectural slate",new Color(.10f,.11f,.13f),.27f,.08f);
            cyan=Mat("Cyan rail core",new Color(.05f,.35f,.4f),.5f,.3f,new Color(.04f,3.8f,4.8f));
            magenta=Mat("Magenta signal core",new Color(.3f,.04f,.17f),.5f,.3f,new Color(3.8f,.04f,1.1f));
            warm=Mat("Amber service lamps",new Color(.4f,.22f,.08f),.4f,.1f,new Color(3f,1.5f,.4f));
            facades=new Material[4];farFacades=new Material[4];ads=new Material[4];
            for(int i=0;i<4;i++)
            {
                facades[i]=Mat("Office facade "+i,Color.white,.85f,.55f,Color.white*3.0f);
                facades[i].SetTexture("_BaseMap",Tex("Architecture"+i+"_Base"));facades[i].SetTexture("_EmissionMap",Tex("Architecture"+i+"_Emission"));
                facades[i].SetTexture("_MetallicGlossMap",Tex("Architecture"+i+"_Surface"));facades[i].EnableKeyword("_METALLICSPECGLOSSMAP");
                var normal=Tex("Architecture"+i+"_Normal");var ni=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(normal));ni.textureType=TextureImporterType.NormalMap;ni.SaveAndReimport();facades[i].SetTexture("_BumpMap",normal);facades[i].EnableKeyword("_NORMALMAP");
                facades[i].SetFloat("_EnvironmentReflections",1);facades[i].DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                farFacades[i]=Save(new Material(facades[i]),"Distant facade "+i+".mat");farFacades[i].SetColor("_EmissionColor",new Color(3.5f,4.0f,4.4f));
                var reflection=Save(new Material(Shader.Find("VectorRush/City Glass Reflection")),"Glass reflection "+i+".mat");
                reflection.SetTexture("_SurfaceMap",Tex("Architecture"+i+"_Surface"));reflection.SetFloat("_Strength",1.8f);
                glassOverlays[facades[i]]=reflection;glassOverlays[farFacades[i]]=reflection;
                ads[i]=Mat("City advertisement "+i,Color.white,.5f,.15f,Color.white*1.9f);ads[i].SetTexture("_BaseMap",Tex("Advert"+i));ads[i].SetTexture("_EmissionMap",Tex("Advert"+i));
            }
            curtainGlass=Mat("Continuous blue glazing",new Color(.045f,.085f,.105f),.92f,.72f);
            var curtainReflection=Save(new Material(Shader.Find("VectorRush/City Glass Reflection")),"Curtain glass reflection.mat");curtainReflection.SetFloat("_Strength",2.2f);glassOverlays[curtainGlass]=curtainReflection;
            serviceAdvert=Mat("NOVA landscape inset",Color.white,.5f,.1f,Color.white*1.8f);serviceAdvert.SetTexture("_BaseMap",Tex("ServiceAdvert"));serviceAdvert.SetTexture("_EmissionMap",Tex("ServiceAdvert"));
            BuildCity(imported);BuildRevealDistrict();BuildInfrastructure();BuildServiceFacade();ConfigureRoad();ConfigureLight();ConfigureShip(world);
            Flush();BakeCityReflections();
            foreach(string guid in AssetDatabase.FindAssets("",new[]{Root+"/Generated"})){var asset=AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));if(asset){if(asset is Material m && m.shader.name=="Universal Render Pipeline/Lit")MaterialEditor.FixupEmissiveFlag(m);EditorUtility.SetDirty(asset);}}
            world.ValidateReady();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Scene);AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))foreach(var material in renderer.sharedMaterials)
                if(!material||!AssetDatabase.Contains(material)||ShaderUtil.ShaderHasError(material.shader))throw new InvalidDataException("Stage1 missing/invalid persistent material on "+renderer.name);
            File.WriteAllText(Path.Combine(evidence,"scene.json"),JsonUtility.ToJson(new Summary{revision=world.artRevision,buildings=occupied.Count,renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length,lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length,courseHash=world.courseHash},true));
            Debug.Log("STAGE1_SCENE_READY "+occupied.Count+" buildings");
        }
        [Serializable]class Summary{public string revision,courseHash;public int buildings,renderers,lights;}
        static Texture2D Tex(string name){string p=Root+"/Textures/"+name+".png";var imp=(TextureImporter)AssetImporter.GetAtPath(p);if(name.EndsWith("_Surface"))imp.sRGBTexture=false;imp.wrapMode=TextureWrapMode.Repeat;imp.anisoLevel=8;imp.mipmapEnabled=true;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(p);}
        static T Save<T>(T obj,string name)where T:UnityEngine.Object
        {string path=Root+"/Generated/"+name;var existing=AssetDatabase.LoadAssetAtPath<T>(path);if(existing){EditorUtility.CopySerialized(obj,existing);UnityEngine.Object.DestroyImmediate(obj);EditorUtility.SetDirty(existing);return existing;}AssetDatabase.CreateAsset(obj,path);return obj;}
        static Material Mat(string name,Color color,float smooth,float metallic,Color? emission=null)
        {var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,enableInstancing=true};m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);if(emission.HasValue){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission.Value);}return Save(m,name+".mat");}
        static void AddMesh(Mesh mesh,Matrix4x4 matrix,Material mat){if(!batches.TryGetValue(mat,out var b))batches[mat]=b=new();b.Add(new CombineInstance{mesh=mesh,transform=matrix});}
        static Mesh cube;
        static void Box(Vector3 p,Vector3 size,Quaternion q,Material mat)
        {if(!cube){var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);cube=obj.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(obj);}AddMesh(cube,Matrix4x4.TRS(p,q,size),mat);}
        static void Beam(Vector3 a,Vector3 b,float w,float depth,Material m){Box((a+b)*.5f,new Vector3(w,depth,Vector3.Distance(a,b)),Quaternion.LookRotation(b-a),m);}
        static void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2 tile,Material mat)
        {var mesh=new Mesh{vertices=new[]{a,b,c,d},uv=new[]{Vector2.zero,new Vector2(tile.x,0),tile,new Vector2(0,tile.y)},triangles=new[]{0,2,1,0,3,2}};mesh.RecalculateNormals();mesh.RecalculateTangents();scratch.Add(mesh);AddMesh(mesh,Matrix4x4.identity,mat);if(glassOverlays.TryGetValue(mat,out var reflection))AddMesh(mesh,Matrix4x4.identity,reflection);}
        static void Facade(Vector3 c,float w,float h,float depth,Quaternion q,Material mat)
        {
            Vector3 P(float x,float y,float z)=>c+q*new Vector3(x,y,z);
            float a=w*.5f,b=depth*.5f;
            Quad(P(-a,0,-b),P(a,0,-b),P(a,h,-b),P(-a,h,-b),new Vector2(w/12,h/96),mat);
            Quad(P(a,0,b),P(-a,0,b),P(-a,h,b),P(a,h,b),new Vector2(w/12,h/96),mat);
            Quad(P(a,0,-b),P(a,0,b),P(a,h,b),P(a,h,-b),new Vector2(depth/12,h/96),mat);
            Quad(P(-a,0,b),P(-a,0,-b),P(-a,h,-b),P(-a,h,b),new Vector2(depth/12,h/96),mat);
        }
        static bool Clear(Vector3 p,float w,float d,float height)
        {
            var bound=new Bounds(p+Vector3.up*height*.5f,new Vector3(w+4,height,d+4));
            foreach(var old in occupied)if(old.Intersects(bound))return false;
            for(int i=0;i<1200;i++){var f=track.Evaluate(i/1200f);var v=f.Position+Vector3.up*3;if(v.y>bound.min.y&&v.y<bound.max.y&&Mathf.Abs(v.x-p.x)<w*.5f+17&&Mathf.Abs(v.z-p.z)<d*.5f+17)return false;}
            occupied.Add(bound);return true;
        }
        static void BuildCity(ProductionImportResult imported)
        {
            for(int layer=0;layer<3;layer++)for(int side=-1;side<=1;side+=2)for(int n=0;n<22;n++)
            {
                float progress=-.025f+n*.015f+R(-.003f,.003f);var f=track.Evaluate(progress);var right=Vector3.ProjectOnPlane(f.Right,Vector3.up).normalized;
                Vector3 p=f.Position+right*side*(layer==0?R(43,64):layer==1?R(98,144):R(175,270));p.y=-16-R(0,10);
                float w=R(25,43)+(layer==2?10:0),d=R(24,38),h=R(layer==0?95:130,layer==2?330:195);
                if(!Clear(p,w,d,h))continue;
                var q=Quaternion.identity;int style=rng.Next(4);Material skin=layer==2?farFacades[style]:facades[style];
                Box(p+Vector3.up*5,new Vector3(w+5,10,d+5),q,concrete);
                if(style==2)
                {
                    TowerMass(p,w,d,h*.55f,skin,style);
                    TowerMass(p+new Vector3(w*.07f,h*.55f,0),w*.76f,d*.84f,h*.32f,skin,style);
                    TowerMass(p+new Vector3(w*.11f,h*.87f,0),w*.50f,d*.57f,h*.13f,skin,style);
                }
                else if(style==3)
                {
                    TowerMass(p+Vector3.left*w*.27f,w*.42f,d,h*.9f,skin,style);
                    TowerMass(p+Vector3.right*w*.27f,w*.42f,d,h,skin,style);
                    Box(p+Vector3.up*h*.66f,new Vector3(w,3.5f,d*.4f),q,metal);
                }
                else TowerMass(p,w,d,h,skin,style);
                if(style==0||layer==2)Box(p+Vector3.up*(h+8),new Vector3(1.3f,16,1.3f),q,metal);
                if(n%3==0)Box(p+new Vector3(-w*.48f,h*.6f,-d*.52f),new Vector3(.30f,h*.6f,.28f),q,n%2==0?cyan:magenta);
                if(layer==0&&n%2==0)
                {
                    // Inward-facing ad panel with frame, armature and service balcony.
                    Vector3 toward=Vector3.ProjectOnPlane(f.Position-p,Vector3.up).normalized;var aq=Quaternion.LookRotation(-toward);
                    float originalHeight=R(42,70);
                    Vector3 ap=p+toward*(Mathf.Max(w,d)*.55f);ap.y=f.Position.y+10+(originalHeight-42)*.35f;float aw=n%4==0?14:10,ah=n%4==0?22:18;
                    Box(ap,new Vector3(aw+1,ah+1,1.1f),aq,metal);
                    Vector3 P(float x,float y)=>ap+aq*new Vector3(x,y,-.58f);
                    Quad(P(-aw*.5f,-ah*.5f),P(aw*.5f,-ah*.5f),P(aw*.5f,ah*.5f),P(-aw*.5f,ah*.5f),Vector2.one,ads[(n/2)%4]);
                    var sr=aq*Vector3.right;var su=aq*Vector3.up;var sn=aq*Vector3.back;var sp=ap+toward*.58f;
                    signData.Add(new Vector4(sp.x,sp.y,sp.z,(n/2)%4));signData.Add(new Vector4(sr.x,sr.y,sr.z,aw));signData.Add(new Vector4(su.x,su.y,su.z,ah));signData.Add(new Vector4(sn.x,sn.y,sn.z,1));
                    Box(ap+Vector3.down*(ah*.5f+1.3f),new Vector3(aw+3,.5f,4),aq,dark);
                    var wash=new GameObject("Architectural sign spill "+n+" "+side);wash.transform.SetParent(parent,false);wash.transform.position=ap+toward*10+Vector3.up*(ah*.25f);wash.transform.rotation=Quaternion.LookRotation(-toward);
                    var light=wash.AddComponent<Light>();light.type=LightType.Spot;light.range=42;light.spotAngle=125;light.innerSpotAngle=65;light.intensity=1000;light.color=(n/2)%4==1?new Color(1,.025f,.16f):(n/2)%4==3?new Color(1,.36f,.08f):new Color(.025f,.55f,1);light.shadows=LightShadows.None;
                    var roadSpill=new GameObject("Advertisement road spill "+n+" "+side);roadSpill.transform.SetParent(parent,false);roadSpill.transform.position=ap+toward*2;roadSpill.transform.rotation=Quaternion.LookRotation(f.Position-roadSpill.transform.position);
                    var spill=roadSpill.AddComponent<Light>();spill.type=LightType.Spot;spill.range=75;spill.spotAngle=48;spill.innerSpotAngle=24;spill.intensity=900;spill.color=(n/2)%4==1?new Color(1,.025f,.16f):(n/2)%4==3?new Color(1,.36f,.08f):new Color(.025f,.55f,1);spill.shadows=LightShadows.None;
                }
            }
            // A distant, varied skyline closes the view beyond the opening bend.
            // These are backdrop masses for this passage, not trackside rollout.
            for(int row=0;row<2;row++)for(int n=0;n<17;n++)
            {
                Vector3 p=new Vector3(-330+n*48+R(-12,12),-36,350+row*90+R(-25,25));
                float w=R(22,38),d=R(26,42),h=R(170,350);
                if(!Clear(p,w,d,h))continue;
                TowerMass(p,w,d,h*.75f,farFacades[n%4],n%3);
                TowerMass(p+Vector3.up*h*.75f,w*.67f,d*.67f,h*.25f,farFacades[n%4],n%3);
                if(n%3==0)Box(p+Vector3.up*(h+12),new Vector3(1,24,1),Quaternion.identity,metal);
            }
            // Low city blocks expose roofs, loading levels and street-scale services below the viaduct.
            for(int x=70;x<351;x+=37)for(int z=-260;z<181;z+=39)
            {
                Vector3 p=new Vector3(x+R(-4,4),-22,z+R(-4,4));float w=R(22,29),d=R(22,30),h=R(20,38);
                if(!Clear(p,w,d,h))continue;
                TowerMass(p,w,d,h,facades[rng.Next(4)],1);
                var plant=AssetDatabase.LoadAssetAtPath<GameObject>(imported.worldAssetRoot+"/Prefabs/vrx_city_plant_a.prefab");
                var service=(GameObject)PrefabUtility.InstantiatePrefab(plant);service.name="Corrected roof plant • "+x+" / "+z;service.transform.SetParent(parent,false);service.transform.position=p+Vector3.up*h;
                for(int k=-1;k<=1;k+=2)Box(p+new Vector3(k*8,h+1,0),new Vector3(3,2,5),Quaternion.identity,metal);
            }
            // Retain corrected authored landmark and service-kit geometry as connected secondary masses.
            for(int n=0;n<9;n++)
            {
                var f=track.Evaluate(.035f+n*.032f);var right=Vector3.ProjectOnPlane(f.Right,Vector3.up).normalized;
                Vector3 p=f.Position+right*(n%2==0?72:-72);p.y=-28;
                string id=n%3==0?"vrx_city_landmark_a":"vrx_city_services_b";
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(imported.worldAssetRoot+"/Prefabs/"+id+".prefab");
                if(prefab&&Clear(p,40,42,110)){var ob=(GameObject)PrefabUtility.InstantiatePrefab(prefab);ob.name="Retained corrected kit • "+n;ob.transform.SetParent(parent,false);ob.transform.position=p;}
            }
        }
        // Three readable construction systems: deep vertical frame, ribbon-slab,
        // and stepped mineral towers with grouped glazing. Service uses louvers.
        static void TowerMass(Vector3 p,float w,float d,float h,Material skin,int style)
        {
            var q=Quaternion.identity;
            if(p.y<0&&p.y>-39)Box(new Vector3(p.x,(-39+p.y)*.5f,p.z),new Vector3(w+.8f,p.y+39,d+.8f),q,concrete);
            Box(p+Vector3.up*h*.5f,new Vector3(w-1.8f,h,d-1.8f),q,dark);
            Facade(p+Vector3.up*1.3f,w-1.3f,h-2.6f,d-1.3f,q,skin);
            if(style==0||style==2)
            {
                // A quiet continuous glass spine balances the occupied floor bands.
                float half=w*(style==0?.19f:.11f),z=d*.5f-.55f;
                Vector3 P(float x,float y,float depth)=>p+new Vector3(x,y,depth);
                Quad(P(-half,3,-z),P(half,3,-z),P(half,h-3,-z),P(-half,h-3,-z),Vector2.one,curtainGlass);
                Quad(P(half,3,z),P(-half,3,z),P(-half,h-3,z),P(half,h-3,z),Vector2.one,curtainGlass);
                for(int side=-1;side<=1;side+=2)for(int x=-1;x<=1;x+=2)
                    Box(p+new Vector3(x*(half+.3f),h*.5f,side*d*.5f),new Vector3(.65f,h,1.8f),q,metal);
                for(float y=5;y<h-3;y+=4)for(int side=-1;side<=1;side+=2)
                    Box(p+new Vector3(0,y,side*z),new Vector3(half*2,.10f,.14f),q,metal);
            }
            float bay=style==0?6:style==2?8:12;
            float pier=style==0?.5f:style==2?1.0f:.4f;
            Material frame=style==2?concrete:metal;
            // All four elevations receive structure; geometry stands proud of glass.
            for(int side=-1;side<=1;side+=2)
            {
                for(float x=-w*.5f;x<=w*.5f+.1f;x+=w/Mathf.Ceil(w/bay))
                    Box(p+new Vector3(x,h*.5f,side*d*.5f),new Vector3(pier,h,1.45f),q,frame);
                for(float z=-d*.5f;z<=d*.5f+.1f;z+=d/Mathf.Ceil(d/bay))
                    Box(p+new Vector3(side*w*.5f,h*.5f,z),new Vector3(1.45f,h,pier),q,frame);
            }
            float rhythm=style==1?4:style==2?12:24;
            for(float y=4;y<h-2;y+=rhythm)
            {
                float thick=style==1?.35f:.6f;
                Box(p+Vector3.up*y,new Vector3(w+1.6f,thick,d+1.6f),q,style==1?concrete:frame);
                if(style==1)Box(p+Vector3.up*(y+.45f),new Vector3(w+.6f,.13f,d+.6f),q,metal);
            }
            // Solid mechanical belts and rooftop crowns interrupt the window field.
            for(float y=32;y<h-10;y+=44)
            {
                Box(p+Vector3.up*y,new Vector3(w+.3f,3.3f,d+.3f),q,dark);
                for(int k=0;k<5;k++)Box(p+Vector3.up*(y-1.2f+k*.55f),new Vector3(w+.7f,.13f,d+.7f),q,metal);
            }
            Box(p+Vector3.up*(h-.6f),new Vector3(w+2,1.2f,d+2),q,frame);
            Box(p+Vector3.up*(h+1.8f),new Vector3(w*.6f,3.6f,d*.48f),q,dark);
            for(int k=-1;k<=1;k++)Box(p+new Vector3(k*w*.18f,h+4,0),new Vector3(w*.12f,1.3f,d*.29f),q,metal);
        }
        static void BuildRevealDistrict()
        {
            // Continue only the visible urban backdrop of the reviewed opening.
            // The old reveal had a void west of the race path and an even tower wall.
            Box(new Vector3(-10,-41,165),new Vector3(1050,4,980),Quaternion.identity,dark);
            // Broad street corridors and service decks connect the lower city.
            for(int x=-390;x<410;x+=104)
            {
                Box(new Vector3(x,-38.95f,180),new Vector3(12,.12f,850),Quaternion.identity,concrete);
                for(int z=-210;z<581;z+=58)
                {
                    Box(new Vector3(x+5.4f,-36.3f,z),new Vector3(.25f,5.3f,.25f),Quaternion.identity,metal);
                    Box(new Vector3(x+5.4f,-33.5f,z),new Vector3(.4f,.5f,.6f),Quaternion.identity,warm);
                }
            }
            var layout=new[]{
                new Vector4(58,126,35,100),new Vector4(-12,177,44,160),
                new Vector4(-98,236,52,224),new Vector4(-177,316,42,260),
                new Vector4(24,303,56,178),new Vector4(-87,391,48,300),
                new Vector4(-232,424,61,238),new Vector4(101,399,49,242),
                new Vector4(-320,350,64,185),new Vector4(-368,517,54,310),
                new Vector4(-360,70,65,190),new Vector4(-430,175,42,260),
                new Vector4(-210,95,40,130),new Vector4(-390,275,68,170)};
            for(int i=0;i<layout.Length;i++)
            {
                var v=layout[i];Vector3 p=new Vector3(v.x,-32,v.y);float w=v.z,d=v.z*.78f,h=v.w;
                if(!Clear(p,w+4,d+4,h+8))continue;
                int style=i%3;var skin=i<2?facades[style]:farFacades[style];
                TowerMass(p,w,d,h*.68f,skin,style);
                TowerMass(p+new Vector3(w*.09f,h*.68f,0),w*.76f,d*.8f,h*.23f,skin,style);
                TowerMass(p+new Vector3(w*.12f,h*.91f,0),w*.46f,d*.5f,h*.09f,skin,style);
                if(i%3==2)
                {
                    Box(p+new Vector3(w*.12f,h+11,0),new Vector3(1.2f,22,1.2f),Quaternion.identity,metal);
                    Box(p+new Vector3(w*.12f,h+23,0),new Vector3(1.5f,2,1.5f),Quaternion.identity,magenta);
                }
            }
            for(int x=-279;x<151;x+=52)for(int z=75;z<447;z+=58)
            {
                Vector3 p=new Vector3(x,-35,z);float h=R(35,60);
                if(!Clear(p,38,41,h))continue;
                TowerMass(p,38,41,h,farFacades[(x+279)/52%3],(x+279)/52%3);
                for(int k=-1;k<=1;k+=2)Box(p+new Vector3(k*10,h+5,0),new Vector3(5,5,13),Quaternion.identity,metal);
            }
        }
        static void BakeCityReflections()
        {
            var config=UnityEngine.Object.FindFirstObjectByType<ProductionRenderConfiguration>();
            var original=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
            try
            {
                GraphicsSettings.defaultRenderPipeline=config.renderPipeline;QualitySettings.renderPipeline=config.renderPipeline;
                foreach(var reflection in glassOverlays.Values.Append(roadReflection).Where(m=>m).Distinct())reflection.SetFloat("_Strength",0);
                int index=0;
                foreach(float progress in new[]{.12f,.23f,.31f})
                {
                    var f=track.Evaluate(progress);var ob=new GameObject("Baked city reflection "+index);ob.transform.SetParent(parent,false);ob.transform.position=f.Position+Vector3.up*12;
                    var probe=ob.AddComponent<ReflectionProbe>();probe.resolution=256;probe.hdr=true;probe.size=new Vector3(260,200,260);probe.center=Vector3.up*25;probe.boxProjection=true;probe.blendDistance=45;probe.nearClipPlane=1;probe.farClipPlane=650;probe.clearFlags=ReflectionProbeClearFlags.Skybox;probe.cullingMask=~0;
                    string path=Root+"/Generated/CityReflection"+index+".exr";
                    // The existing captures are part of the protected test-field look. Reuse them
                    // while extending infrastructure beyond the capture boundary; only bootstrap
                    // a capture when the persistent source asset is genuinely absent.
                    var accepted=AssetDatabase.LoadAssetAtPath<Texture>(path);
                    if(!accepted)
                    {
                        if(!Lightmapping.BakeReflectionProbe(probe,path))throw new InvalidOperationException("City reflection capture failed "+index);
                        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                        var importer=(TextureImporter)AssetImporter.GetAtPath(path);var platform=importer.GetDefaultPlatformTextureSettings();platform.format=TextureImporterFormat.RGBAHalf;importer.SetPlatformTextureSettings(platform);importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                        accepted=AssetDatabase.LoadAssetAtPath<Texture>(path);
                    }
                    probe.mode=ReflectionProbeMode.Custom;probe.customBakedTexture=accepted;
                    if(!probe.customBakedTexture)throw new InvalidOperationException("Missing reflection texture "+path);
                    foreach(var reflection in glassOverlays.Values.Append(roadReflection).Where(m=>m).Distinct())
                    {reflection.SetTexture("_City"+index,probe.customBakedTexture);reflection.SetVector("_Capture"+index,ob.transform.position);EditorUtility.SetDirty(reflection);}
                    // Material-scoped reflections preserve the accepted road and craft response.
                    probe.enabled=false;
                    index++;
                }
            }
            finally
            {
                foreach(var reflection in glassOverlays.Values.Append(roadReflection).Where(m=>m).Distinct()){reflection.SetFloat("_Strength",reflection==roadReflection?.55f:reflection.name.Contains("Curtain")?2.2f:1.8f);EditorUtility.SetDirty(reflection);}
                GraphicsSettings.defaultRenderPipeline=original;QualitySettings.renderPipeline=quality;
            }
        }
        static void ConfigureSignReflections()
        {
            // Reflect only the existing advertisement planes, in their actual world poses.
            // This supplements the coarse city captures without reflecting the road into itself.
            int count=signData.Count/4;
            var data=new Texture2D(4,Mathf.Max(1,count),TextureFormat.RGBAFloat,false,true){name="Existing sign reflection poses",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            data.SetPixels(signData.Select(v=>new Color(v.x,v.y,v.z,v.w)).ToArray());data.Apply(false);
            roadReflection.SetTexture("_SignData",Save(data,"SignReflectionPoses.asset"));roadReflection.SetFloat("_SignCount",count);
            var images=new Texture2DArray(256,512,4,TextureFormat.RGBA32,true,false){name="Existing advertisement reflections",filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp};
            for(int k=0;k<4;k++)
            {
                var source=new Texture2D(2,2);ImageConversion.LoadImage(source,File.ReadAllBytes(Root+"/Textures/Advert"+k+".png"));
                var colors=new Color[256*512];for(int y=0;y<512;y++)for(int x=0;x<256;x++)colors[y*256+x]=source.GetPixelBilinear((x+.5f)/256,(y+.5f)/512);
                images.SetPixels(colors,k);UnityEngine.Object.DestroyImmediate(source);
            }
            images.Apply(true);roadReflection.SetTexture("_SignImages",Save(images,"SignReflectionImages.asset"));
        }
        static void BuildInfrastructure()
        {
            // Preserve the accepted trackside material values while architecture evolves.
            var dark=Mat("Preserved rail graphite",new Color(.072f,.096f,.11f),.48f,.6f);
            var metal=Mat("Preserved rail titanium",new Color(.19f,.25f,.28f),.68f,.8f);
            var concrete=Mat("Preserved support slate",new Color(.13f,.17f,.19f),.38f,.15f);
            var reflectC=Save(new Material(Shader.Find("VectorRush/Stage1 Local Rail Response")){name="Cyan localized road response"},"CyanResponse.mat");reflectC.SetColor("_Color",new Color(.025f,.95f,1.5f));
            var reflectM=Save(new Material(Shader.Find("VectorRush/Stage1 Local Rail Response")){name="Magenta localized road response"},"MagentaResponse.mat");reflectM.SetColor("_Color",new Color(1.2f,.01f,.3f));
            roadReflection=Save(new Material(Shader.Find("VectorRush/City Glass Reflection")),"Road city reflection.mat");roadReflection.SetFloat("_RoadSurface",1);roadReflection.SetFloat("_Strength",.55f);ConfigureSignReflections();
            // The first 310 spans are the owner-approved test field and retain their exact
            // placement and settings. Continue the same authored system after
            // its existing boundary so the remaining circuit no longer falls back to the dark base.
            for(int i=0;i<CircuitInfrastructureSpanCount;i++)
            {
                float p=InfrastructureProgress(i);var a=track.Evaluate(p);var b=track.Evaluate(p+InfrastructureStep);
                Vector3 P(TrackFrame f,float x,float y)=>f.Position+f.Right*x+f.Up*y;
                Quad(P(a,-10.7f,.024f),P(a,10.7f,.024f),P(b,10.7f,.024f),P(b,-10.7f,.024f),Vector2.one,roadReflection);
                for(int s=-1;s<=1;s+=2)
                {
                    Beam(P(a,s*11.4f,.6f),P(b,s*11.4f,.6f),.55f,.72f,dark);
                    Beam(P(a,s*11.0f,.36f),P(b,s*11.0f,.36f),.16f,.15f,cyan);
                    Beam(P(a,s*11.65f,1.23f),P(b,s*11.65f,1.23f),.13f,.12f,s==1?magenta:cyan);
                    Beam(P(a,s*11.75f,1.8f),P(b,s*11.75f,1.8f),.20f,.16f,metal);
                    Quad(P(a,s*10.8f,.035f),P(b,s*10.8f,.035f),P(b,s*6.7f,.035f),P(a,s*6.7f,.035f),new Vector2(2,1),s==1?reflectM:reflectC);
                    if(i%5==0)
                    {
                        Beam(P(a,s*11.8f,0),P(a,s*11.8f,1.9f),.25f,.26f,metal);
                        Beam(P(a,s*12f,-.5f),P(a,s*12f,-34),1.0f,1.0f,concrete);
                        Box(P(a,s*11.65f,2.4f),new Vector3(.24f,.35f,.35f),Quaternion.LookRotation(a.Forward,a.Up),warm);
                    }
                }
                if(i%7==0){var q=Quaternion.LookRotation(a.Forward,a.Up);Box(P(a,0,.036f),new Vector3(.18f,.014f,1.7f),q,metal);}
                if(i%12==0)
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        var lamp=new GameObject("Local rail wash "+i+" "+side);lamp.transform.SetParent(parent,false);lamp.transform.position=P(a,side*9.7f,1.0f);
                        var l=lamp.AddComponent<Light>();l.type=LightType.Point;l.color=side<0?new Color(.06f,.7f,1f):new Color(1f,.055f,.28f);l.range=10;l.intensity=.75f;l.shadows=LightShadows.None;
                    }
                }
            }
            // Route-dependent chevrons face the approaching player, outside the protected corridor.
            for(float p=.105f;p<.295f;p+=.017f)
            {
                var f=track.Evaluate(p);float turn=Vector3.SignedAngle(f.Forward,track.Evaluate(p+.015f).Forward,Vector3.up);
                if(Mathf.Abs(turn)<2)continue;
                float outside=turn<0?1:-1;float arrow=turn<0?-1:1;var q=Quaternion.LookRotation(f.Forward,f.Up);
                Vector3 c=f.Position+f.Right*(outside*13.65f)+f.Up*2.5f;
                Box(c,new Vector3(2.6f,1.5f,.22f),q,metal);Box(c-q*Vector3.forward*.13f,new Vector3(2.43f,1.32f,.06f),q,dark);
                Beam(c-f.Up*.75f,c-f.Up*2.5f,.18f,.18f,metal);
                for(int k=-1;k<=1;k+=2)
                {
                    Vector3 P(float x,float y)=>c+q*new Vector3(k*.58f+x*arrow,y,-.18f);
                    Beam(P(-.18f,.42f),P(.21f,0),.11f,.06f,cyan);Beam(P(.21f,0),P(-.18f,-.42f),.11f,.06f,cyan);
                }
            }
            // Two overhead utility bridges frame the bend; the racing sightline remains clear.
            foreach(float p in new[]{.09f,.205f})
            {
                var f=track.Evaluate(p);Vector3 P(float x,float y)=>f.Position+f.Right*x+Vector3.up*y;
                Beam(P(-36,18),P(36,18),2,2,dark);Beam(P(-36,21),P(36,21),.6f,.7f,metal);
                for(int s=-1;s<=1;s+=2){Beam(P(s*18,-35),P(s*18,21),1.5f,1.5f,concrete);Beam(P(s*12,18),P(s*12,20),.3f,.3f,cyan);}
                for(int x=-32;x<33;x+=8){Beam(P(x,18),P(x+4,21),.3f,.3f,metal);Beam(P(x+4,21),P(x+8,18),.3f,.3f,metal);}
                Beam(P(-10,17),P(10,17),.1f,.1f,cyan);
            }
        }
        public static float InfrastructureProgress(int span)=>InfrastructureStartProgress+span*InfrastructureStep;
        static void BuildServiceFacade()
        {
            var f=track.Evaluate(.170f);Vector3 right=Vector3.ProjectOnPlane(f.Right,Vector3.up).normalized;
            var q=Quaternion.LookRotation(right,Vector3.up);Vector3 c=f.Position+right*24+Vector3.up*8;
            Vector3 P(float x,float y,float z)=>c+q*new Vector3(x,y,z);
            Box(c,new Vector3(19,27,3.4f),q,dark);
            // Floor-scale regions: a recessed advertising bay, ventilated plant level and canopy.
            for(int side=-1;side<=1;side+=2)
            {
                Box(P(side*9,0,-2),new Vector3(.8f,29,1),q,metal);
                Box(P(side*8,-39,0),new Vector3(1.1f,74,1.4f),q,concrete);
            }
            Box(P(0,11,-2.6f),new Vector3(20,.8f,4.8f),q,metal);
            Box(P(0,10.55f,-4.9f),new Vector3(18,.12f,.14f),q,cyan);
            Box(P(0,4,-1.85f),new Vector3(16.5f,8.8f,.8f),q,metal);
            Quad(P(-7.8f,.05f,-2.29f),P(7.8f,.05f,-2.29f),P(7.8f,7.95f,-2.29f),P(-7.8f,7.95f,-2.29f),Vector2.one,serviceAdvert);
            for(float y=-10;y<-2;y+=.48f)Box(P(0,y,-2),new Vector3(16,.13f,.40f),q,metal);
            for(float x=-7.5f;x<8;x+=2.5f)Box(P(x,-6,-2.4f),new Vector3(.20f,9,.35f),q,concrete);
            Beam(P(-8,-1,-3),P(8,-1,-3),.4f,.4f,metal);
        }
        static void ConfigureRoad()
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {if(!r.name.Contains("authoritative collision")||!r.sharedMaterial)continue;var m=Save(new Material(r.sharedMaterial){name="Stage1 graphite road"},"Road.mat");var baseColor=new Color(.17f,.18f,.20f);var baseMap=Tex("NeonRoad_Base");m.SetColor("_BaseColor",baseColor);m.SetColor("_Color",baseColor);m.SetTexture("_BaseMap",baseMap);m.SetTexture("_MainTex",baseMap);m.SetTextureScale("_BaseMap",new Vector2(.75f,.4f));m.SetTextureScale("_MainTex",new Vector2(.75f,.4f));var normal=Tex("NeonRoad_Normal");var ni=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(normal));ni.textureType=TextureImporterType.NormalMap;ni.SaveAndReimport();m.SetTexture("_BumpMap",normal);m.SetFloat("_BumpScale",.18f);m.EnableKeyword("_NORMALMAP");m.SetFloat("_Smoothness",.66f);m.SetFloat("_Metallic",.42f);m.SetFloat("_EnvironmentReflections",0);m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");m.SetTexture("_MetallicGlossMap",Tex("NeonRoad_Surface"));m.EnableKeyword("_METALLICSPECGLOSSMAP");r.sharedMaterial=m;}
        }
        static void ConfigureLight()
        {
            RenderSettings.fog=true;RenderSettings.fogDensity=.0028f;RenderSettings.fogColor=new Color(.019f,.018f,.028f).gamma;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.10f,.12f,.18f);RenderSettings.ambientEquatorColor=new Color(.065f,.07f,.095f);RenderSettings.ambientGroundColor=new Color(.025f,.03f,.045f);
            var ambient=new SphericalHarmonicsL2();ambient.AddAmbientLight(new Color(.035f,.045f,.065f));RenderSettings.ambientProbe=ambient;
            RenderSettings.skybox=Save(new Material(Shader.Find("VectorRush/Stage1 City Atmosphere")),"Sky.mat");
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.type==LightType.Directional){l.color=new Color(.78f,.80f,.9f);l.intensity=.14f;l.transform.rotation=Quaternion.Euler(48,-25,0);}
            var config=UnityEngine.Object.FindFirstObjectByType<ProductionRenderConfiguration>();
            var pipeline=Save(UnityEngine.Object.Instantiate((UniversalRenderPipelineAsset)config.renderPipeline),"NeonPipeline.asset");pipeline.colorGradingMode=ColorGradingMode.HighDynamicRange;pipeline.colorGradingLutSize=32;
            var serializedPipeline=new SerializedObject(pipeline);var rendererList=serializedPipeline.FindProperty("m_RendererDataList");
            var renderer=Save(UnityEngine.Object.Instantiate((UniversalRendererData)rendererList.GetArrayElementAtIndex(0).objectReferenceValue),"NeonRenderer.asset");renderer.renderingMode=RenderingMode.ForwardPlus;rendererList.GetArrayElementAtIndex(0).objectReferenceValue=renderer;serializedPipeline.ApplyModifiedPropertiesWithoutUndo();config.renderPipeline=pipeline;
            var volume=UnityEngine.Object.FindFirstObjectByType<Volume>();var profile=Save(UnityEngine.Object.Instantiate(volume.sharedProfile),"Grade.asset");
            // Embed the override copies so the saved scene never mutates the baseline grade.
            foreach(var old in AssetDatabase.LoadAllAssetsAtPath(Root+"/Generated/Grade.asset").OfType<VolumeComponent>().ToArray())UnityEngine.Object.DestroyImmediate(old,true);
            profile.components=new List<VolumeComponent>();foreach(var c in volume.sharedProfile.components){var copy=UnityEngine.Object.Instantiate(c);AssetDatabase.AddObjectToAsset(copy,profile);profile.components.Add(copy);}
            volume.sharedProfile=profile;if(profile.TryGet(out Bloom bloom)){bloom.threshold.Override(.85f);bloom.intensity.Override(1.15f);bloom.scatter.Override(.75f);}if(profile.TryGet(out ColorAdjustments color)){color.postExposure.Override(-.1f);color.contrast.Override(18);color.saturation.Override(8);}
            if(profile.TryGet(out Tonemapping tone))tone.mode.Override(TonemappingMode.Neutral);
            if(profile.TryGet(out MotionBlur blur)){blur.active=false;blur.intensity.Override(0);}
        }
        static void ConfigureShip(ProductionWorld world)
        {
            var old=world.materials;var lib=Save(UnityEngine.Object.Instantiate(old),"CraftLibrary.asset");lib.stage1Finish=true;
            lib.craftSurfaceTemplate=Save(new Material(old.craftSurfaceTemplate),"CraftTemplate.mat");lib.craftSurfaceTemplate.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");lib.craftSurfaceTemplate.SetFloat("_EnvironmentReflections",1);lib.craftSurfaceTemplate.SetFloat("_Smoothness",.78f);
            lib.craftGlass=Mat("Stage1 optical glass",new Color(.018f,.065f,.086f),.96f,.65f);
            lib.craftEngineAccent=Mat("Stage1 nozzle titanium",new Color(.10f,.19f,.23f),.78f,.9f,new Color(.015f,.21f,.35f));
            lib.craftEngineCore=Mat("Stage1 ion core",new Color(.6f,.85f,1),.5f,.1f,new Color(1.6f,4.5f,7));world.materials=lib;
        }
        static void Flush()
        {
            foreach(var pair in batches){var mesh=new Mesh{name=pair.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();mesh=Save(mesh,"Mesh-"+(serial++)+".asset");var ob=new GameObject(pair.Key.name);ob.transform.SetParent(parent,false);ob.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=ob.AddComponent<MeshRenderer>();renderer.sharedMaterial=pair.Key;renderer.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;ob.isStatic=true;}
            foreach(var mesh in scratch)UnityEngine.Object.DestroyImmediate(mesh);scratch.Clear();batches.Clear();
        }
    }
}
