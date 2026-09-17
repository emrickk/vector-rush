using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Editor
{
    public static class KoiLanternSetup
    {
        public const string Candidate="Assets/Scenes/KoiLanternStage9.unity";
        public const string Root="Assets/Art/KoiLanternTower";
        static string Source=>Path.GetFullPath(Application.dataPath+"/../../SourceAssets/KoiLanternTower");
        [Serializable] public class Report{public string revision="koi-lantern-stage9-02",courseHash;public bool baselineUnchanged,collidersUnchanged;public Vector3 position,sculptureCenter;public float yaw,nearestCourseProgress,minimumTrackDistance;public int renderers,triangles;}
        public static void Prepare()
        {
            string evidence=ProductionSceneSetup.RequiredFlag("-productionEvidence");Directory.CreateDirectory(evidence);
            string baseline=ProductionSceneSetup.Hash(File.ReadAllBytes(FullLapAdsSetup.Candidate));
            var scene=EditorSceneManager.OpenScene(FullLapAdsSetup.Candidate,OpenSceneMode.Single);string physics=PhysicsSignature();
            EditorSceneManager.SaveScene(scene,Candidate,true);scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            foreach(var folder in new[]{"Models","Materials","Textures","Prefabs"})Directory.CreateDirectory(Root+"/"+folder);
            File.Copy(Source+"/KoiLanternTower.fbx",Root+"/Models/KoiLanternTower.fbx",true);
            File.Copy(Source+"/scenario/enamel/art.png",Root+"/Textures/KoiEnamel.png",true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/KoiEnamel.png");ti.sRGBTexture=true;ti.mipmapEnabled=true;ti.anisoLevel=8;ti.maxTextureSize=1024;ti.wrapMode=TextureWrapMode.Repeat;ti.SaveAndReimport();
            var mats=new Dictionary<string,Material>();
            Add(mats,"graphite",new Color(.027f,.042f,.046f),.65f,.36f,0);
            Add(mats,"bronze",new Color(.38f,.18f,.052f),.8f,.27f,0);
            Add(mats,"ivory",new Color(.88f,.79f,.58f),.18f,.3f,.5f);
            Add(mats,"red",new Color(.55f,.033f,.012f),.3f,.3f,.35f);
            Add(mats,"enamel",Color.white,.15f,.3f,.5f);
            Add(mats,"fin",new Color(.58f,.27f,.064f),.55f,.3f,.4f);
            Add(mats,"amber",new Color(1,.38f,.055f),.1f,.3f,3f);
            Add(mats,"window",new Color(.72f,.40f,.16f),.2f,.3f,.65f);
            Add(mats,"glass",new Color(.012f,.028f,.03f),.55f,.19f,0);
            Add(mats,"eye",new Color(.006f,.008f,.009f),.6f,.13f,0);
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/KoiEnamel.png");mats["enamel"].SetTexture("_BaseMap",texture);mats["enamel"].SetTexture("_EmissionMap",texture);EditorUtility.SetDirty(mats["enamel"]);
            var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/Models/KoiLanternTower.fbx");importer.globalScale=1;importer.useFileScale=true;importer.importAnimation=false;importer.importLights=false;importer.importCameras=false;importer.isReadable=true;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            foreach(var pair in mats)importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),pair.Key),pair.Value);
            importer.SaveAndReimport();
            var g=new GameObject("Koi Lantern Tower / turn focal point");
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/KoiLanternTower.fbx"));model.transform.SetParent(g.transform,false);
            var motion=g.AddComponent<KoiLanternMotion>();motion.fins=model.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Fin_")).OrderBy(t=>t.name).ToArray();
            if(motion.fins.Length!=4)throw new InvalidDataException("Expected four fin pivots");
            Lamp(g.transform,"Porcelain key",new Vector3(-8,78,12),new Vector3(0,70,0),new Color(1,.79f,.5f),14,38,80);
            Lamp(g.transform,"Cool sculpture fill",new Vector3(12,68,9),new Vector3(0,70,0),new Color(.48f,.72f,1),9,30,85);
            var prefab=PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/KoiLanternTower.prefab");UnityEngine.Object.DestroyImmediate(g);
            g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();g.transform.SetParent(world.transform,false);
            // Forward-exit sightline from the recorded .50-.60 approach; the outer site clears the road.
            g.transform.position=new Vector3(-275,2,156);g.transform.rotation=Quaternion.Euler(0,60,0);
            var report=new Report{position=g.transform.position,sculptureCenter=g.transform.TransformPoint(new Vector3(0,73,0)),yaw=60,courseHash=world.courseHash};
            report.minimumTrackDistance=ValidateSite(world,g.transform);report.nearestCourseProgress=world.track.ClosestProgress(g.transform.position);
            report.renderers=g.GetComponentsInChildren<Renderer>().Length;report.triangles=g.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);
            report.collidersUnchanged=physics==PhysicsSignature();if(!report.collidersUnchanged)throw new InvalidDataException("Collision state changed");
            world.artRevision=report.revision;world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            report.baselineUnchanged=baseline==ProductionSceneSetup.Hash(File.ReadAllBytes(FullLapAdsSetup.Candidate));if(!report.baselineUnchanged)throw new InvalidDataException("Stage 8 changed");
            File.WriteAllText(evidence+"/landmark.json",JsonUtility.ToJson(report,true));Debug.Log("KOI_LANTERN_AUTHORED "+report.minimumTrackDistance);
        }
        static void Add(Dictionary<string,Material> mats,string name,Color color,float metal,float rough,float emission)
        {
            string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",1-rough);m.enableInstancing=true;
            if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}mats[name]=m;EditorUtility.SetDirty(m);
        }
        static void Lamp(Transform parent,string name,Vector3 p,Vector3 target,Color color,float power,float range,float angle)
        {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localRotation=Quaternion.LookRotation(target-p);var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=color;l.intensity=power;l.range=range;l.spotAngle=angle;l.shadows=LightShadows.None;}
        public static float ValidateSite(ProductionWorld world,Transform root)
        {
            if(root.GetComponentsInChildren<Collider>(true).Length!=0)throw new InvalidDataException("Landmark must be visual only");
            float closest=float.MaxValue;
            for(int i=0;i<2400;i++){
                var f=world.track.Evaluate(i/2400f);float d=Vector3.ProjectOnPlane(root.position-f.Position,Vector3.up).magnitude;closest=Mathf.Min(closest,d);
                // Entire tower and animated fin envelope fits within 20 metres of its vertical axis.
                if(d<world.track.Width*.5f+23)throw new InvalidDataException("Landmark envelope enters racing corridor at "+i/2400f);
            }
            return closest;
        }
        static string PhysicsSignature()=>string.Join("\n",UnityEngine.Object.FindFirstObjectByType<ProductionWorld>().GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(x=>x));
    }
}
