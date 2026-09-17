using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Editor
{
    public static class RainSceneSetup
    {
        public const string Candidate="Assets/Scenes/RainRoadStage4.unity";
        public static void PrepareAndBuild(){Prepare();ProductionSceneSetup.BuildExperienceCandidate();}
        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-experienceBuildOutput")>=0)output+="-author";
            Directory.CreateDirectory(output);
            string source=SmoothRoadSetup.Candidate;
            string before=ProductionSceneSetup.Hash(File.ReadAllBytes(source));
            var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);EditorSceneManager.SaveScene(scene,Candidate,true);scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            var rain=new GameObject("Rain / streaks, wet spray and sheltered audio").AddComponent<RainPresentation>();
            rain.rainLoop=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Rain/CityRain.wav");
            if(!rain.rainLoop)throw new InvalidDataException("Missing authored rain loop");
            Directory.CreateDirectory("Assets/Art/RainRoadStage4");AssetDatabase.Refresh();
            int count=0;
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>())
            {
                var original=renderer.sharedMaterial;if(!original)continue;
                bool response=original.name.Contains("localized road response");
                bool road=original.HasProperty("_RoadSurface")&&original.GetFloat("_RoadSurface")>.5f;
                bool deck=renderer.name.StartsWith("Running surface")&&renderer.name.Contains("authoritative collision");
                if(!response&&!road&&!deck)continue;
                var material=new Material(original);material.name=original.name+" / rain";
                if(response)material.SetColor("_Color",original.GetColor("_Color")*1.55f);
                if(road&&material.HasProperty("_Strength"))material.SetFloat("_Strength",.85f);
                if(deck){if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.94f);if(material.HasProperty("_BaseColor")){var c=material.GetColor("_BaseColor");material.SetColor("_BaseColor",new Color(c.r*.84f,c.g*.84f,c.b*.84f,c.a));}}
                string path="Assets/Art/RainRoadStage4/Wet-"+(count++)+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(existing){EditorUtility.CopySerialized(material,existing);UnityEngine.Object.DestroyImmediate(material);material=existing;}else AssetDatabase.CreateAsset(material,path);
                renderer.sharedMaterial=material;
            }
            world.artRevision="rain-road-stage4-02";world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(before!=ProductionSceneSetup.Hash(File.ReadAllBytes(source)))throw new InvalidDataException("Dry scene changed");
            File.WriteAllText(Path.Combine(output,"rain-authoring.txt"),"scene="+Candidate+"\ncourse="+world.courseHash+"\ndrySceneUnchanged=true\nwetMaterials="+count+"\nhandlingUnchanged=true\n");
        }
    }
}
