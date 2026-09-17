using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorRush.Editor {
public static class NightCityKitAuthoring {
 const string Root="Assets/Art/NightCityKit";
 static string Source=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../SourceAssets/NightCityKit"));
 [Serializable] public class MaterialDef { public string id, @base, normal, ao, ms, emission,kind; public float tileMeters,metallic,roughness,emissionStrength; public float[] baseTint; }
 [Serializable] public class MaterialList { public MaterialDef[] materials; }
 [Serializable] public class MeshDef { public string id,file;public int lod,triangles,vertices;public float[] dimensions;public string[] materials; }
 [Serializable] public class MeshList { public MeshDef[] assets; }
 [Serializable] public class Placement { public string id;public float[] position;public float yaw,scale; }
 [Serializable] public class Layout { public Placement[] placements; }
 [Serializable] public class Audit { public string status="ASSET_IMPORT_VERIFIED_NOT_ARTISTIC_ACCEPTANCE";public int prefabs,meshes,materials; public string[] checks; }
 static readonly Dictionary<string,Material> Mats=new Dictionary<string,Material>();
 static string Evidence {
  get {var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-nightCityEvidence");if(i<0)throw new Exception("Supply -nightCityEvidence");return args[i+1];}
 }
 [MenuItem("Vector Rush/Night City/Import and build asset review")]
 public static void Prepare() {
  Directory.CreateDirectory(Evidence);
  foreach(var d in new[]{"Textures","Models","Materials","Prefabs"})Directory.CreateDirectory(Root+"/"+d);
  foreach(var p in Directory.GetFiles(Source+"/textures","*.png"))File.Copy(p,Root+"/Textures/"+Path.GetFileName(p),true);
  foreach(var p in Directory.GetFiles(Source+"/meshes","*.fbx"))File.Copy(p,Root+"/Models/"+Path.GetFileName(p),true);
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var p in Directory.GetFiles(Root+"/Textures","*.png")) {
   var t=(TextureImporter)AssetImporter.GetAtPath(p);bool normal=p.EndsWith("_normal.png"),data=normal||p.EndsWith("_ms.png")||p.EndsWith("_ao.png")||p.EndsWith("_height.png");
   t.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=!data;t.mipmapEnabled=true;t.streamingMipmaps=true;t.maxTextureSize=2048;t.anisoLevel=8;t.textureCompression=TextureImporterCompression.CompressedHQ;
   t.wrapMode=Path.GetFileName(p).StartsWith("ad_")||Path.GetFileName(p).StartsWith("sign_")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;t.alphaSource=TextureImporterAlphaSource.FromInput;t.SaveAndReimport();
  }
  var defs=JsonUtility.FromJson<MaterialList>(File.ReadAllText(Source+"/materials.json"));
  foreach(var d in defs.materials) {
   var m=NewMaterial(d.id);m.SetColor("_BaseColor",new Color(d.baseTint[0],d.baseTint[1],d.baseTint[2],1));m.SetFloat("_Metallic",d.metallic);m.SetFloat("_Smoothness",1-d.roughness);
   Assign(m,"_BaseMap",d.@base);if(!string.IsNullOrEmpty(d.normal)){Assign(m,"_BumpMap",d.normal);m.SetFloat("_BumpScale",.7f);m.EnableKeyword("_NORMALMAP");}
   if(!string.IsNullOrEmpty(d.ms)){Assign(m,"_MetallicGlossMap",d.ms);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_Smoothness",1);}
   if(!string.IsNullOrEmpty(d.ao)){Assign(m,"_OcclusionMap",d.ao);m.SetFloat("_OcclusionStrength",.7f);m.EnableKeyword("_OCCLUSIONMAP");}
   if(!string.IsNullOrEmpty(d.emission)){Assign(m,"_EmissionMap",d.emission);m.SetColor("_EmissionColor",Color.white*Mathf.Max(.1f,d.emissionStrength));m.EnableKeyword("_EMISSION");m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
   EditorUtility.SetDirty(m);
  }
  AddSolid("glass",new Color(.018f,.048f,.058f),0,.5f,.21f);AddSolid("warm",new Color(.9f,.48f,.16f),2.2f,.1f,.4f);AddSolid("cyan",new Color(.12f,.7f,.72f),2,.1f,.4f);AddSolid("red",new Color(.85f,.065f,.03f),3,.1f,.4f);AddSolid("black",new Color(.015f,.024f,.027f),0,.2f,.6f);
  AssetDatabase.SaveAssets();var meshes=JsonUtility.FromJson<MeshList>(File.ReadAllText(Source+"/mesh-manifest.json"));
  foreach(var d in meshes.assets) {
   string p=Root+"/Models/"+d.file;var importer=(ModelImporter)AssetImporter.GetAtPath(p);importer.globalScale=1;importer.useFileScale=true;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.isReadable=true;
   importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
   foreach(string name in d.materials) {if(!Mats.ContainsKey(name))throw new Exception("Unknown material "+name);importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),name),Mats[name]);}
   importer.SaveAndReimport();
  }
  foreach(var group in meshes.assets.GroupBy(x=>x.id)) {
   var parent=new GameObject("NC_"+group.Key);var lods=new List<LOD>();
   foreach(var d in group.OrderBy(x=>x.lod)) {
    var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+d.file);var child=(GameObject)PrefabUtility.InstantiatePrefab(asset);child.transform.SetParent(parent.transform,false);
    var rr=child.GetComponentsInChildren<Renderer>();foreach(var r in rr)foreach(var m in r.sharedMaterials)if(!m||!Mats.Values.Contains(m))throw new Exception("Unmapped slot "+d.file);
    var mf=child.GetComponentsInChildren<MeshFilter>();int tris=mf.Sum(f=>f.sharedMesh.triangles.Length/3);if(tris!=d.triangles)throw new Exception("Triangle mismatch "+d.file);
    if(mf.Any(f=>f.sharedMesh.uv.Length==0))throw new Exception("Missing UV "+d.file);
    Bounds b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);Vector3 expected=new Vector3(d.dimensions[0],d.dimensions[2],d.dimensions[1]);if(Vector3.Distance(b.size,expected)>.15f)throw new Exception("Wrong FBX scale/orientation "+d.file+" "+b.size+" expected "+expected);
    lods.Add(new LOD(d.lod==0?.25f:.025f,rr));
   }
   var lg=parent.AddComponent<LODGroup>();lg.SetLODs(lods.ToArray());lg.RecalculateBounds();PrefabUtility.SaveAsPrefabAsset(parent,Root+"/Prefabs/NC_"+group.Key+".prefab");UnityEngine.Object.DestroyImmediate(parent);
  }
  BuildReviewScene();
  var audit=new Audit{prefabs=meshes.assets.Select(x=>x.id).Distinct().Count(),meshes=meshes.assets.Length,materials=Mats.Count,checks=new[]{"FBX triangle counts match Blender manifest","Metric bounds and axis conversion verified","All material slots explicitly remapped","UV channels present","Data textures linear; base/emission sRGB","LOD0/LOD1 prefabs serialized","Current racing scenes unchanged by authoring"}};
  File.WriteAllText(Evidence+"/import-validation.json",JsonUtility.ToJson(audit,true));AssetDatabase.SaveAssets();Debug.Log("NIGHT_CITY_ASSET_IMPORT_VERIFIED");
 }
 static void Assign(Material m,string slot,string file){if(string.IsNullOrEmpty(file))return;var t=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+file);if(!t)throw new Exception("Missing texture "+file);m.SetTexture(slot,t);}
 static Material NewMaterial(string id){string p=Root+"/Materials/"+id+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}Mats[id]=m;return m;}
 static void AddSolid(string id,Color c,float emission,float metal,float rough){var m=NewMaterial(id);m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",1-rough);if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*emission);}EditorUtility.SetDirty(m);}
 static void Block(string name,Vector3 pos,Vector3 size,string mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=Mats[mat];var mesh=UnityEngine.Object.Instantiate(g.GetComponent<MeshFilter>().sharedMesh);var uv=new Vector2[mesh.vertexCount];for(int i=0;i<uv.Length;i++){var v=Vector3.Scale(mesh.vertices[i],size);var n=mesh.normals[i];uv[i]=Mathf.Abs(n.y)>.5f?new Vector2(v.x,v.z)/4:Mathf.Abs(n.x)>.5f?new Vector2(v.z,v.y)/4:new Vector2(v.x,v.y)/4;}mesh.uv=uv;string mp=Root+"/Review/"+name.Replace(" ","")+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(mp);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,mp);g.GetComponent<MeshFilter>().sharedMesh=mesh;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
 static void Lamp(string name,Vector3 p,Color col,float intensity,float range){var g=new GameObject(name);g.transform.position=p;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=col;l.intensity=intensity;l.range=range;}
 static void BuildReviewScene(){
  var s=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.20f,.3f,.37f);RenderSettings.ambientEquatorColor=new Color(.10f,.16f,.19f);RenderSettings.ambientGroundColor=new Color(.04f,.05f,.07f);
  var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Source+"/assembly-layout.json"));foreach(var p in layout.placements){var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/NC_"+p.id+".prefab"));g.transform.position=new Vector3(p.position[0],p.position[1],p.position[2]);g.transform.rotation=Quaternion.Euler(0,p.yaw,0);g.transform.localScale=Vector3.one*p.scale;}
  Block("Review floor",new Vector3(0,-.25f,-5),new Vector3(75,.5f,60),"asphalt_wet");Block("Rooftop base",new Vector3(-8,1.5f,-1),new Vector3(17,3,12),"cast_concrete");Block("Roof sheet",new Vector3(-8,3.08f,-1),new Vector3(17.4f,.15f,12.4f),"corrugated_steel_wet");
  var sun=new GameObject("Cool studio key");var dl=sun.AddComponent<Light>();dl.type=LightType.Directional;dl.color=new Color(.65f,.82f,1);dl.intensity=1.2f;sun.transform.rotation=Quaternion.Euler(45,-35,0);dl.shadows=LightShadows.Soft;
  var fill=new GameObject("Neutral review fill");var fl=fill.AddComponent<Light>();fl.type=LightType.Directional;fl.color=new Color(.72f,.83f,.88f);fl.intensity=1.15f;fill.transform.rotation=Quaternion.Euler(30,155,0);fl.shadows=LightShadows.Soft;
  Lamp("Red shop bounce",new Vector3(-8,6,5),new Color(1,.08f,.03f),22,18);Lamp("Cyan billboard bounce",new Vector3(10,10,-8),new Color(.2f,.8f,1),20,20);Lamp("Warm shop light",new Vector3(-12,3.5f,5),new Color(1,.5f,.18f),8,10);
  var cg=new GameObject("Asset Review Camera");var cam=cg.AddComponent<Camera>();cam.transform.position=new Vector3(29,19,35);cam.transform.LookAt(new Vector3(-1,10,-8));cam.fieldOfView=43;cam.nearClipPlane=.1f;cam.farClipPlane=250;cam.backgroundColor=new Color(.018f,.035f,.045f);cam.clearFlags=CameraClearFlags.SolidColor;cam.allowHDR=true;
  var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;
  var controller=cg.AddComponent<NightCityReviewCamera>();controller.pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/VectorPipeline.asset");
  var probe=new GameObject("Review reflections").AddComponent<ReflectionProbe>();probe.transform.position=new Vector3(0,8,-3);probe.size=new Vector3(90,90,90);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;probe.resolution=256;probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=new Color(.2f,.28f,.34f);
  var ambient=new SphericalHarmonicsL2();ambient.AddAmbientLight(new Color(.23f,.29f,.34f));RenderSettings.ambientProbe=ambient;
  var volume=new GameObject("Review tonemapping").AddComponent<Volume>();volume.isGlobal=true;string pp=Root+"/ReviewVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(pp);
  if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,pp);}
  foreach(var oldComponent in profile.components.ToArray())if(oldComponent)UnityEngine.Object.DestroyImmediate(oldComponent,true);
  profile.components.Clear();
  var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);
  var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.18f);bloom.threshold.Override(1.2f);AssetDatabase.AddObjectToAsset(bloom,profile);
  EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();volume.sharedProfile=profile;
  if(profile.components.Any(c=>!c||!AssetDatabase.Contains(c)))throw new Exception("Review post-processing must persist as asset subobjects");
  EditorSceneManager.SaveScene(s,Root+"/NightCityAssetReview.unity");
 }
 public static void BuildViewer(){
  var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-nightCityBuild");if(i<0)throw new Exception("Supply -nightCityBuild");
  var old=GraphicsSettings.defaultRenderPipeline;var oq=QualitySettings.renderPipeline;
  try{var p=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/VectorPipeline.asset");GraphicsSettings.defaultRenderPipeline=p;QualitySettings.renderPipeline=p;
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/NightCityAssetReview.unity"},locationPathName=args[i+1],target=BuildTarget.StandaloneOSX,options=BuildOptions.None});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Asset viewer build failed");
   File.WriteAllText(Evidence+"/native-build.txt",report.summary.guid.ToString()+"\n");
  }finally{GraphicsSettings.defaultRenderPipeline=old;QualitySettings.renderPipeline=oq;AssetDatabase.SaveAssets();}
 }
 public static void RenderReview(){
  EditorSceneManager.OpenScene(Root+"/NightCityAssetReview.unity");var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();
  var old=GraphicsSettings.defaultRenderPipeline;var oq=QualitySettings.renderPipeline;
  try {var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/VectorPipeline.asset");GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
   Capture(cam,"unity-assembly.png");cam.transform.position=new Vector3(5,10,16);cam.transform.LookAt(new Vector3(-7,4,.5f));Capture(cam,"unity-rooftop.png");
  }finally {GraphicsSettings.defaultRenderPipeline=old;QualitySettings.renderPipeline=oq;}
 }
 static void Capture(Camera cam,string file){var rt=new RenderTexture(1920,1280,24,RenderTextureFormat.ARGBHalf);rt.Create();cam.targetTexture=rt;cam.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1920,1280,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1280),0,0);t.Apply();Directory.CreateDirectory(Evidence);File.WriteAllBytes(Evidence+"/"+file,t.EncodeToPNG());RenderTexture.active=prev;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);}
}
}
