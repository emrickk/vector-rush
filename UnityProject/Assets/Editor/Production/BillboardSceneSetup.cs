using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace VectorRush.Editor
{
    public static class BillboardSceneSetup
    {
        public const string Candidate="Assets/Scenes/AnimatedBillboardsStage6.unity";
        const string Root="Assets/Art/AnimatedBillboardsStage6";
        static int serial;
        static Texture2D[] frames;
        static Texture2D ticker,titles;
        static Material frameMat;
        static float Luminance(int theme)=>new[]{1.2f,.9f,1.1f,1.22f}[theme];
        static Material[] glow;
        static readonly Color[] colors={new Color(.67f,.28f,1),new Color(.65f,1,.03f),new Color(1,.12f,.055f),new Color(1,.40f,.07f)};
        struct Placement
        {
            public int anchor,group,campaign,style;public Vector3 offset;public float yaw,width,height,cropStart,cropWidth;public bool kinetic;
            public Placement(int a,int g,int c,int mode,Vector3 off,float heading,float w,float h,float start=0,float span=1,bool holo=false)
            {anchor=a;group=g;campaign=c;style=mode;offset=off;yaw=heading;width=w;height=h;cropStart=start;cropWidth=span;kinetic=holo;}
        }
        static Placement[] Layout()=>new[]{
            // One readable approach landmark, with clear buildings before and after it.
            new Placement(9,0,0,2,new Vector3(0,10,5),180,16,26),
            // Hero and ticker sit within the chase-camera approach view.
            new Placement(12,1,2,2,new Vector3(-5,9,0),90,18,28),
            new Placement(12,1,2,1,new Vector3(-5,-8,0),90,23,4),
            // South and east building faces read in sequence as the road rounds the corner.
            new Placement(4,2,1,2,new Vector3(8,3,0),-90,16,24),
            new Placement(4,2,3,2,new Vector3(0,3,-8),0,16,24),
            // A broad triptych on the outer bend, low enough to stay in the driving frame.
            new Placement(14,3,0,2,new Vector3(-6,4,9),90,7.8f,30,0,1f/3),
            new Placement(14,3,0,2,new Vector3(-6,4,0),90,7.8f,30,1f/3,1f/3),
            new Placement(14,3,0,2,new Vector3(-6,4,-9),90,7.8f,30,2f/3,1f/3),
            // South-facing landscape display is visible on approach to the district exit.
            new Placement(16,4,3,0,new Vector3(-4,0,-5),0,30,13,0,1,true),
            new Placement(16,4,1,1,new Vector3(-19,9,-5),90,12,3.5f)
        };
        public static void PrepareAndBuild(){Prepare();ProductionSceneSetup.BuildExperienceCandidate();}
        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence")+"-author";
            if(Directory.Exists(output))throw new IOException("Use a fresh evidence path");Directory.CreateDirectory(output);
            string before=ProductionSceneSetup.Hash(File.ReadAllBytes(AtmosphereSceneSetup.Candidate));
            var scene=EditorSceneManager.OpenScene(AtmosphereSceneSetup.Candidate,OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene,Candidate,true);scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();serial=0;
            frames=Enumerable.Range(0,4).Select(i=>LoadTexture("Frames"+i,8192)).ToArray();ticker=LoadTexture("Ticker",2048);titles=LoadTexture("Titles",2048);
            frameMat=Lit("Display housings",new Color(.034f,.042f,.057f),.65f,.6f);
            glow=colors.Select((c,i)=>Lit("Display accent "+i,c*.12f,.55f,.4f,c*.55f)).ToArray();
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            var poses=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Stage1/Generated/SignReflectionPoses.asset");
            var old=poses.GetPixels();var layout=Layout();int count=layout.Length;var data=new Color[count*8];
            // Preserve architecture and source meshes; remove only old advertisement faces.
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>())if(renderer.sharedMaterial&&renderer.sharedMaterial.name.StartsWith("City advertisement"))renderer.enabled=false;
            StripOldFrames(world,old);
            var root=new GameObject("Animated city advertisements");root.transform.SetParent(world.transform,false);
            var playback=root.AddComponent<AnimatedBillboards>();root.AddComponent<BillboardMotionEvidence>();
            var displays=new List<Transform>();var kinetic=new List<Transform>();
            for(int i=0;i<count;i++){
                var spec=layout[i];var p=old[spec.anchor*4];
                Vector3 anchor=new Vector3(p.r,p.g,p.b),position=anchor+spec.offset;
                Quaternion rotation=Quaternion.Euler(0,spec.yaw,0);int theme=spec.campaign;float width=spec.width,height=spec.height;
                Vector3 right=rotation*Vector3.right,up=Vector3.up,normal=rotation*Vector3.back;
                var mount=new GameObject("Cluster "+spec.group+" / Display "+i.ToString("D2")+" / "+new[]{"AURORA","VOLT","ECHO","ORBIT"}[theme]);mount.transform.SetParent(root.transform,false);mount.transform.SetPositionAndRotation(position,rotation);
                Cube(mount.transform,"Housing",new Vector3(0,0,.55f),new Vector3(width+1.1f,height+1.1f,1.1f),frameMat);
                Cube(mount.transform,"Upper accent",new Vector3(0,height*.5f+.36f,-.04f),new Vector3(width+.65f,.055f,.08f),glow[theme]);
                Cube(mount.transform,"Lower accent",new Vector3(0,-height*.5f-.36f,-.04f),new Vector3(width+.65f,.055f,.08f),glow[theme]);
                var screen=new GameObject("Animated surface "+i);screen.transform.SetParent(mount.transform,false);screen.transform.localPosition=Vector3.back*.18f;
                screen.AddComponent<MeshFilter>().sharedMesh=Save(Panel(width,height,false),"Screen-"+i+".asset");
                var material=new Material(Shader.Find("VectorRush/Animated Billboard")){name="Live campaign "+i};BindFrames(material);material.SetFloat("_Campaign",theme);material.SetFloat("_Phase",spec.group*1.17f);material.SetFloat("_Aspect",width/height);material.SetFloat("_Style",spec.style);material.SetFloat("_CropStart",spec.cropStart);material.SetFloat("_CropWidth",spec.cropWidth);material.SetFloat("_Intensity",Luminance(theme));
                screen.AddComponent<MeshRenderer>().sharedMaterial=Save(material,"Campaign-"+i+".mat");displays.Add(screen.transform);
                Vector3 surface=screen.transform.position;
                data[i*8]=new Color(surface.x,surface.y,surface.z,theme);data[i*8+1]=new Color(right.x,right.y,right.z,width);data[i*8+2]=new Color(up.x,up.y,up.z,height);
                data[i*8+3]=new Color(normal.x,normal.y,normal.z,spec.group*1.17f);data[i*8+4]=new Color(spec.style,spec.cropStart,spec.cropWidth,Luminance(theme));
                // Small projecting kinetic landmarks are attached above selected signs, clear of the road.
                if(spec.kinetic){
                    var holo=new GameObject("Kinetic orbital display "+i);holo.transform.SetParent(mount.transform,false);holo.transform.localPosition=new Vector3(0,height*.5f+5,-2.8f);
                    AddMesh(holo.transform,"Outer orbit",Ring(3.6f,.11f,72,8),glow[theme],Quaternion.Euler(0,0,22));
                    AddMesh(holo.transform,"Inner orbit",Ring(2.65f,.09f,64,8),glow[(theme+1)%4],Quaternion.Euler(65,20,0));
                    var core=GameObject.CreatePrimitive(PrimitiveType.Sphere);core.name="Luminous core";UnityEngine.Object.DestroyImmediate(core.GetComponent<Collider>());core.transform.SetParent(holo.transform,false);core.transform.localScale=Vector3.one*1.65f;core.GetComponent<MeshRenderer>().sharedMaterial=glow[theme];kinetic.Add(holo.transform);
                }
            }
            playback.displays=displays.ToArray();playback.kineticObjects=kinetic.ToArray();
            foreach(var light in world.GetComponentsInChildren<Light>()){
                if(light.name.StartsWith("Architectural sign spill")||light.name.StartsWith("Advertisement road spill"))light.enabled=false;
            }
            // Broad low-intensity facade wash belongs only to occupied clusters.
            foreach(int i in new[]{0,1,3,5,8}){
                var face=displays[i];var lamp=new GameObject("Cluster facade wash "+layout[i].group);lamp.transform.SetParent(root.transform,false);
                lamp.transform.SetPositionAndRotation(face.position-face.forward*5,face.rotation);var light=lamp.AddComponent<Light>();light.type=LightType.Spot;light.range=24;light.spotAngle=105;light.innerSpotAngle=55;light.intensity=layout[i].campaign==1?95:140;light.color=colors[layout[i].campaign];light.shadows=LightShadows.None;
            }
            var newPoses=new Texture2D(8,count,TextureFormat.RGBAFloat,false,true){name="Architectural display poses and composition",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};newPoses.SetPixels(data);newPoses.Apply();newPoses=Save(newPoses,"SignPoses.asset");
            // Clone only the reflection materials. Preserve the dry/wet palette and all road meshes.
            var clones=new Dictionary<Material,Material>();
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>()){
                var materials=renderer.sharedMaterials;bool changed=false;
                for(int i=0;i<materials.Length;i++){
                    var m=materials[i];if(!m||m.shader.name!="VectorRush/City Glass Reflection"||!m.HasProperty("_RoadSurface")||m.GetFloat("_RoadSurface")<.5f)continue;
                    if(!clones.TryGetValue(m,out var replacement)){
                        replacement=new Material(m){name=m.name+" / animated ads"};replacement.shader=Shader.Find("VectorRush/Animated City Reflection");BindFrames(replacement);replacement.SetTexture("_SignData",newPoses);replacement.SetFloat("_SignCount",count);replacement=Save(replacement,"Reflection-"+clones.Count+".mat");clones.Add(m,replacement);
                    }
                    materials[i]=replacement;changed=true;
                }
                if(changed)renderer.sharedMaterials=materials;
            }
            BillboardMounts.Build(world,playback.displays,Root,output,frameMat);
            world.artRevision="animated-billboards-stage6-06";world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(before!=ProductionSceneSetup.Hash(File.ReadAllBytes(AtmosphereSceneSetup.Candidate)))throw new InvalidDataException("Atmosphere source scene changed");
            File.WriteAllText(Path.Combine(output,"billboards.json"),"{\"signs\":"+count+",\"clusters\":5,\"campaigns\":4,\"videoFramesPerCampaign\":64,\"sourceVideoSeconds\":5,\"echoPlaybackSeconds\":12,\"kineticDisplays\":"+kinetic.Count+",\"sourceSceneUnchanged\":true,\"courseHash\":\""+world.courseHash+"\"}");
        }
        static Texture2D LoadTexture(string name,int size)
        {
            string path=Root+"/"+name+".png";var imp=AssetImporter.GetAtPath(path) as TextureImporter;if(!imp)throw new IOException("Missing animation asset "+path);
            imp.mipmapEnabled=true;imp.mipmapFilter=TextureImporterMipFilter.KaiserFilter;imp.anisoLevel=4;imp.wrapMode=TextureWrapMode.Clamp;imp.filterMode=FilterMode.Trilinear;imp.maxTextureSize=size;imp.npotScale=TextureImporterNPOTScale.None;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void BindFrames(Material m){for(int i=0;i<4;i++)m.SetTexture("_Frames"+i,frames[i]);m.SetTexture("_Ticker",ticker);m.SetTexture("_Titles",titles);}
        static Material Lit(string name,Color c,float smooth,float metal,Color? emission=null){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);if(emission.HasValue){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission.Value);}return Save(m,name+".mat");}
        static void Cube(Transform parent,string name,Vector3 position,Vector3 scale,Material material){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localScale=scale;o.GetComponent<MeshRenderer>().sharedMaterial=material;}
        static void AddMesh(Transform parent,string name,Mesh mesh,Material material,Quaternion rotation){var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localRotation=rotation;o.AddComponent<MeshFilter>().sharedMesh=Save(mesh,"Kinetic-"+(serial++)+".asset");o.AddComponent<MeshRenderer>().sharedMaterial=material;}
        static Mesh Panel(float width,float height,bool curved){int segments=curved?24:1;var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();for(int i=0;i<=segments;i++){float x=i/(float)segments;float z=curved?-.65f*(1-Mathf.Pow((x-.5f)*2,2)):0;v.Add(new Vector3((x-.5f)*width,-height*.5f,z));v.Add(new Vector3((x-.5f)*width,height*.5f,z));uv.Add(new Vector2(x,0));uv.Add(new Vector2(x,1));if(i<segments){int n=i*2;tri.AddRange(new[]{n,n+1,n+2,n+2,n+1,n+3});}}var m=new Mesh{vertices=v.ToArray(),uv=uv.ToArray(),triangles=tri.ToArray()};m.RecalculateNormals();m.RecalculateBounds();return m;}
        static Mesh Ring(float radius,float tube,int a,int b){var v=new List<Vector3>();var tri=new List<int>();for(int i=0;i<a;i++)for(int j=0;j<b;j++){float x=i*2*Mathf.PI/a,y=j*2*Mathf.PI/b;v.Add(new Vector3(Mathf.Cos(x)*(radius+tube*Mathf.Cos(y)),Mathf.Sin(x)*(radius+tube*Mathf.Cos(y)),tube*Mathf.Sin(y)));int n=i*b+j,k=i*b+(j+1)%b,l=((i+1)%a)*b+j,q=((i+1)%a)*b+(j+1)%b;tri.AddRange(new[]{n,k,l,k,q,l});}var m=new Mesh{vertices=v.ToArray(),triangles=tri.ToArray()};m.RecalculateNormals();return m;}
        static void StripOldFrames(ProductionWorld world,Color[] poses)
        {
            foreach(var filter in world.GetComponentsInChildren<MeshFilter>()){
                if(filter.GetComponent<Collider>())continue;var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.sharedMaterial||renderer.sharedMaterial.name!="Satin titanium")continue;
                var mesh=filter.sharedMesh;if(!mesh||mesh.subMeshCount!=1)continue;var verts=mesh.vertices;var triangles=mesh.triangles;var kept=new List<int>();
                for(int t=0;t<triangles.Length;t+=3){bool remove=false;for(int s=0;s<poses.Length/4&&!remove;s++){
                    var p=poses[s*4];var r=poses[s*4+1];var u=poses[s*4+2];var n=poses[s*4+3];Vector3 normal=new Vector3(n.r,n.g,n.b),center=new Vector3(p.r,p.g,p.b)-normal*.58f;bool inside=true;
                    for(int k=0;k<3;k++){var d=filter.transform.TransformPoint(verts[triangles[t+k]])-center;if(Mathf.Abs(Vector3.Dot(d,new Vector3(r.r,r.g,r.b)))>r.a*.5f+.56f||Mathf.Abs(Vector3.Dot(d,new Vector3(u.r,u.g,u.b)))>u.a*.5f+.56f||Mathf.Abs(Vector3.Dot(d,normal))>.56f){inside=false;break;}}
                    remove=inside;
                }if(!remove){kept.Add(triangles[t]);kept.Add(triangles[t+1]);kept.Add(triangles[t+2]);}}
                if(kept.Count==triangles.Length)continue;var copy=UnityEngine.Object.Instantiate(mesh);copy.triangles=kept.ToArray();copy.RecalculateBounds();filter.sharedMesh=Save(copy,"Former-frame-batch-"+(serial++)+".asset");
            }
        }
        static T Save<T>(T asset,string name)where T:UnityEngine.Object{string path=Root+"/"+name;var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(asset,path);return asset;}
    }
}
