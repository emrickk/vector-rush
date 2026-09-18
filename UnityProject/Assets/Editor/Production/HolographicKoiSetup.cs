using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Editor
{
 public static class HolographicKoiSetup
 {
  public const string Candidate="Assets/Scenes/HolographicKoiStage10.unity";
  public const string Root="Assets/Art/HolographicKoi";
  static string Source=>Path.GetFullPath(Application.dataPath+"/../../SourceAssets/HolographicKoi");
  [Serializable] public class Report{public string revision="holographic-koi-stage10-08",courseHash;public bool baselineUnchanged,collisionsUnchanged;public Vector3 site,fishCenter;public int triangles,fishRenderers;public float minimumRoadDistance,minimumSwimClearance;}
  public static void Prepare()
  {
   string evidence=ProductionSceneSetup.RequiredFlag("-productionEvidence");Directory.CreateDirectory(evidence);
   string hash=ProductionSceneSetup.Hash(File.ReadAllBytes(FullLapAdsSetup.Candidate));var scene=EditorSceneManager.OpenScene(FullLapAdsSetup.Candidate,OpenSceneMode.Single);string physics=Physics();EditorSceneManager.SaveScene(scene,Candidate,true);scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
   foreach(var d in new[]{"Models","Meshes","Materials","Prefabs"})Directory.CreateDirectory(Root+"/"+d);
   File.Copy(Source+"/HolographicKoi.fbx",Root+"/Models/HolographicKoi.fbx",true);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
   var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/Models/HolographicKoi.fbx");importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.isReadable=true;importer.globalScale=1;importer.useFileScale=true;importer.SaveAndReimport();
   var materials=new Dictionary<string,Material>();
   materials["body"]=Holo("body",new Color(1,.018f,.005f),3.4f,.65f,0);
   materials["membrane"]=Holo("membrane",new Color(1,.03f,.006f),5.2f,.48f,1);
   materials["filament"]=Holo("filament",new Color(1,.07f,.016f),5.5f,.7f,2);
   materials["gill"]=Holo("gill",new Color(.09f,.004f,.002f),1,.45f,3);
   materials["eye"]=Holo("eye",new Color(.009f,.012f,.015f),1,.80f,3);
   materials["gill"].renderQueue=3060;materials["eye"].renderQueue=3061;
   var asset=new GameObject("Luminous koi projection");var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/HolographicKoi.fbx"));
   foreach(var f in model.GetComponentsInChildren<MeshFilter>()){
    string key=f.GetComponent<Renderer>().sharedMaterial.name;var combined=new Mesh{name="Koi "+key};combined.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
    combined.CombineMeshes(new[]{new CombineInstance{mesh=f.sharedMesh,transform=model.transform.worldToLocalMatrix*f.transform.localToWorldMatrix}},true,true);combined.RecalculateBounds();var bounds=combined.bounds;bounds.Expand(new Vector3(0,2.8f,11));combined.bounds=bounds;
    combined=Save(combined,"Meshes/"+key+".asset");var g=new GameObject("Projected koi / "+key);g.layer=29;g.transform.SetParent(asset.transform,false);g.AddComponent<MeshFilter>().sharedMesh=combined;g.AddComponent<MeshRenderer>().sharedMaterial=materials[key];
   }
   UnityEngine.Object.DestroyImmediate(model);var prefab=PrefabUtility.SaveAsPrefabAsset(asset,Root+"/Prefabs/LuminousKoi.prefab");UnityEngine.Object.DestroyImmediate(asset);
   var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();var root=new GameObject("Luminous koi / turn installation").transform;root.SetParent(world.transform,false);
   var fish=(GameObject)PrefabUtility.InstantiatePrefab(prefab);fish.transform.SetParent(root,false);var encounter=world.track.Evaluate(.615f);
   Vector3 travel=Vector3.ProjectOnPlane(encounter.Forward,Vector3.up).normalized;Vector3 across=Vector3.Cross(Vector3.up,travel);
   Vector3 heading=(travel*.65f-across*.76f).normalized;
   fish.transform.localScale=Vector3.one*2.2f;
   fish.transform.SetPositionAndRotation(encounter.Position+Vector3.up*39,Quaternion.LookRotation(Vector3.Cross(heading,Vector3.up),Vector3.up));
   var site=new GameObject("Projection podium").transform;site.SetParent(root,false);site.SetPositionAndRotation(new Vector3(-275,0,156),Quaternion.Euler(0,60,0));
   var dark=Lit("Projector graphite",new Color(.018f,.027f,.036f),.7f,.5f);
   var metal=Lit("Projector titanium",new Color(.11f,.14f,.16f),.8f,.35f);
   var cyan=Lit("Projector cyan",new Color(.04f,.35f,.6f),.2f,.4f,2.5f);
   var amber=Lit("Service windows",new Color(.45f,.17f,.06f),.1f,.4f,.5f);
   Box(site,"Foundation",new Vector3(0,-37,0),new Vector3(23,4,20),dark);
   Box(site,"Architectural core",new Vector3(0,10,0),new Vector3(18,92,14),dark);
   for(int i=0;i<17;i++){
    float y=-31+i*5;Box(site,"Floor reveal",new Vector3(0,y,0),new Vector3(19,.28f,15),metal);
    for(int j=-3;j<=3;j++)Box(site,"Occupied slit",new Vector3(j*2.5f,y+2,7.02f),new Vector3(1.1f,2.4f,.08f),amber);
   }
   for(int i=-1;i<=1;i+=2)Box(site,"Vertical projection spine",new Vector3(i*9.2f,11,7.1f),new Vector3(.16f,91,.18f),cyan);
   Box(site,"Projection deck",new Vector3(0,57,0),new Vector3(26,1,21),dark);
   Box(site,"Recessed deck light",new Vector3(0,57.6f,10.1f),new Vector3(24,.13f,.17f),cyan);
   Box(site,"Upper instrument deck",new Vector3(0,60,0),new Vector3(19,.5f,15),metal);
   for(int i=-1;i<=1;i++){
    var p=new Vector3(i*6,61,3);Box(site,"Projector housing",p,new Vector3(2,1.3f,3),dark);Box(site,"Projector lens",p+Vector3.up*.7f,new Vector3(1.5f,.10f,2.2f),cyan);
    Beam(site,p+Vector3.up*.8f,new Vector3(i*7,81,-1),i);
   }
   Lamp(root,"Koi red facade spill",fish.transform.position+new Vector3(0,-7,0),new Color(1,.05f,.015f),26,92);
   Lamp(root,"Koi warm lower spill",fish.transform.position+new Vector3(15,-12,12),new Color(1,.19f,.04f),18,70);
   Lamp(root,"Projector blue bounce",site.TransformPoint(new Vector3(0,60,8)),new Color(.06f,.4f,1),6,25);
   var reflection=NewMaterial("Wet koi reflection","VectorRush/Koi Road Reflection");reflection.SetFloat("_Strength",.16f);
   RoadReflection(root,world.track,reflection);
   var playback=root.gameObject.AddComponent<HolographicKoi>();playback.fish=fish.transform;playback.reflectionMaterial=reflection;playback.anchor=fish.transform.position;playback.heading=fish.transform.rotation;playback.swimAcross=across;playback.swimAlong=travel;playback.movingLights=root.GetComponentsInChildren<Light>().Where(l=>l.name.StartsWith("Koi ")).ToArray();
   SubdueCompetingAds(world);
   var report=new Report{courseHash=world.courseHash,site=site.position,fishCenter=fish.transform.position,fishRenderers=fish.GetComponentsInChildren<Renderer>().Length,triangles=fish.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3)};
   report.minimumSwimClearance=ValidateSwimClearance(world,playback);report.minimumRoadDistance=ValidateClearance(world,fish.transform,site);report.collisionsUnchanged=physics==Physics();if(!report.collisionsUnchanged)throw new InvalidDataException("Driving collision state changed");
   world.artRevision=report.revision;world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();report.baselineUnchanged=hash==ProductionSceneSetup.Hash(File.ReadAllBytes(FullLapAdsSetup.Candidate));if(!report.baselineUnchanged)throw new InvalidDataException("Stage 8 modified");
   File.WriteAllText(evidence+"/holographic-koi.json",JsonUtility.ToJson(report,true));
  }
  static Material NewMaterial(string name,string shader){string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.name=name;EditorUtility.SetDirty(m);return m;}
  static Material Holo(string name,Color color,float power,float opacity,float kind){var m=NewMaterial(name,"VectorRush/Holographic Koi");m.SetColor("_Color",color);m.SetFloat("_Power",power);m.SetFloat("_Opacity",opacity);m.SetFloat("_Kind",kind);return m;}
  static Material Lit(string name,Color color,float metal,float smooth,float power=0){var m=NewMaterial(name,"Universal Render Pipeline/Lit");m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);if(power>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*power);}return m;}
  static T Save<T>(T value,string relative) where T:UnityEngine.Object {string path=Root+"/"+relative;var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(value,path);return value;}
  static void Box(Transform parent,string name,Vector3 p,Vector3 size,Material material){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;}
  static void Lamp(Transform parent,string name,Vector3 p,Color color,float intensity,float range){var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.position=p;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;}
  static void Beam(Transform parent,Vector3 a,Vector3 b,int index){var m=NewMaterial("Projection beam","VectorRush/Koi Projection Beam");m.SetColor("_Tint",new Color(.12f,.3f,.65f,.18f));var mesh=new Mesh();mesh.vertices=new[]{a+Vector3.left*.3f,a+Vector3.right*.3f,b+Vector3.left*3,b+Vector3.right*3};mesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateNormals();mesh=Save(mesh,"Meshes/Beam-"+index+".asset");var g=new GameObject("Projection light volume");g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;}
  static void RoadReflection(Transform root,TrackPath track,Material material){int count=240;var vertices=new Vector3[(count+1)*2];var normals=new Vector3[vertices.Length];var uv=new Vector2[vertices.Length];var triangles=new int[count*6];for(int i=0;i<=count;i++){float p=Mathf.Lerp(.535f,.705f,i/(float)count);var f=track.Evaluate(p);for(int side=0;side<2;side++){int n=i*2+side;vertices[n]=f.Position+f.Right*((side*2-1)*(track.Width*.5f-.2f))+f.Up*.018f;normals[n]=f.Up;uv[n]=new Vector2(side,i/(float)count);}if(i<count){int a=i*2,j=i*6;triangles[j]=a;triangles[j+1]=a+2;triangles[j+2]=a+1;triangles[j+3]=a+1;triangles[j+4]=a+2;triangles[j+5]=a+3;}}var mesh=new Mesh{name="Banked wet reflection surface"};mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();mesh=Save(mesh,"Meshes/WetReflection.asset");var g=new GameObject("Koi reflection / wet road only");g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;}
  static void SubdueCompetingAds(ProductionWorld world){var ads=UnityEngine.Object.FindFirstObjectByType<AnimatedBillboards>();var changed=new List<int>();for(int i=0;i<ads.displays.Length;i++){var t=ads.displays[i];if(!t.parent.name.StartsWith("Cluster 2 /"))continue;var r=t.GetComponent<Renderer>();var clone=new Material(r.sharedMaterial);clone.SetFloat("_Intensity",clone.GetFloat("_Intensity")*.32f);r.sharedMaterial=Save(clone,"Materials/Subdued-sign-"+i+".mat");changed.Add(i);}if(changed.Count!=2)throw new InvalidDataException("Expected two competing screens");var map=new Dictionary<Material,Material>();foreach(var r in world.GetComponentsInChildren<MeshRenderer>(true)){var slots=r.sharedMaterials;for(int i=0;i<slots.Length;i++){var old=slots[i];if(!old||old.shader.name!="VectorRush/Animated City Reflection")continue;if(!map.TryGetValue(old,out var clone)){clone=new Material(old);var texture=UnityEngine.Object.Instantiate((Texture2D)old.GetTexture("_SignData"));var pixels=texture.GetPixels();foreach(int k in changed)pixels[k*8+4].a*=.32f;texture.SetPixels(pixels);texture.Apply();texture=Save(texture,"Meshes/ReflectionPoses-"+map.Count+".asset");clone.SetTexture("_SignData",texture);clone=Save(clone,"Materials/Reflection-"+map.Count+".mat");map[old]=clone;}slots[i]=clone;}r.sharedMaterials=slots;}}
  public static float ValidateClearance(ProductionWorld world,Transform fish,Transform site){float min=float.MaxValue;for(int i=0;i<2400;i++){var f=world.track.Evaluate(i/2400f);float d=Vector3.ProjectOnPlane(site.position-f.Position,Vector3.up).magnitude;min=Mathf.Min(min,d);if(d<world.track.Width*.5f+20)throw new InvalidDataException("Solid projection podium too near road");}if(fish.GetComponentsInChildren<Collider>(true).Length>0||site.GetComponentsInChildren<Collider>(true).Length>0)throw new InvalidDataException("Visual-only installation adds collider");return min;}
  public static float ValidateSwimClearance(ProductionWorld world,HolographicKoi swimmer)
  {
   Vector3 position=swimmer.fish.position;Quaternion rotation=swimmer.fish.rotation;float minimum=float.MaxValue;
   var filters=swimmer.fish.GetComponentsInChildren<MeshFilter>();
   for(int step=0;step<120;step++){
    swimmer.ApplySwimPose(step*swimmer.period/120f);bool first=true;Bounds swept=new Bounds();
    foreach(var filter in filters){var b=filter.sharedMesh.bounds;for(int c=0;c<8;c++){var v=filter.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)));if(first){swept=new Bounds(v,Vector3.zero);first=false;}else swept.Encapsulate(v);}}
    for(int i=0;i<1600;i++){var f=world.track.Evaluate(i/1600f);float half=world.track.Width*.5f;if(f.Position.x<swept.min.x-half||f.Position.x>swept.max.x+half||f.Position.z<swept.min.z-half||f.Position.z>swept.max.z+half)continue;minimum=Mathf.Min(minimum,swept.min.y-f.Position.y);}
   }
   swimmer.fish.SetPositionAndRotation(position,rotation);
   if(minimum<7)throw new InvalidDataException("Swimming koi envelope has insufficient overhead clearance: "+minimum);
   return minimum;
  }
  static string Physics()=>string.Join("\n",UnityEngine.Object.FindFirstObjectByType<ProductionWorld>().GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(s=>s));
 }
}
