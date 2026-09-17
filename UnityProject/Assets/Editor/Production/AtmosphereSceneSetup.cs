using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Editor
{
    public static class AtmosphereSceneSetup
    {
        public const string Candidate="Assets/Scenes/RainAtmosphereStage5.unity";
        const string Root="Assets/Art/RainAtmosphereStage5";
        const int Samples=3840,Levels=17;
        public static void PrepareAndBuild(){Prepare();ProductionSceneSetup.BuildExperienceCandidate();}
        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-experienceBuildOutput")>=0)output+="-author";
            if(Directory.Exists(output)&&Directory.GetFileSystemEntries(output).Length>0)throw new IOException("Use fresh evidence");
            Directory.CreateDirectory(output);
            var source=SmoothRoadSetup.Candidate;string before=ProductionSceneSetup.Hash(File.ReadAllBytes(source));
            var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);
            if(!EditorSceneManager.SaveScene(scene,Candidate,true))throw new IOException("Cannot copy dry scene");
            scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();var track=world.track;
            Physics.SyncTransforms();var covered=new bool[Samples];
            for(int i=0;i<Samples;i++){var f=track.Evaluate(i/(float)Samples);covered[i]=RainPresentation.IsSheltered(f.Position+f.Up*2);}
            var profile=ScriptableObject.CreateInstance<RainShelterProfile>();profile.signedRoofDistance=new float[Samples];
            float step=track.Length/Samples;
            for(int i=0;i<Samples;i++){
                float distance=100;
                for(int j=1;j<=Mathf.CeilToInt(100/step);j++)if(covered[(i+j)%Samples]!=covered[i]||covered[(i-j+Samples)%Samples]!=covered[i]){distance=(j-.5f)*step;break;}
                profile.signedRoofDistance[i]=covered[i]?distance:-distance;
            }
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();profile=Save(profile,"RoofProfile.asset");
            var rain=new GameObject("Rain / dry shelters and city haze").AddComponent<RainPresentation>();
            rain.rainLoop=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Rain/CityRain.wav");rain.shelterProfile=profile;
            RenderSettings.fog=true;RenderSettings.fogDensity=.0055f;RenderSettings.fogColor=new Color(.115f,.145f,.18f);
            int serial=0;
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>())
            {
                var original=renderer.sharedMaterial;if(!original)continue;
                bool response=original.name.Contains("localized road response");
                bool reflection=original.HasProperty("_RoadSurface")&&original.GetFloat("_RoadSurface")>.5f;
                bool deck=renderer.name.StartsWith("Running surface")&&renderer.name.Contains("authoritative collision");
                if(!response&&!reflection&&!deck)continue;
                var filter=renderer.GetComponent<MeshFilter>();var sourceMesh=filter.sharedMesh;
                int stride=sourceMesh.vertexCount/(Samples+1);
                if(stride*(Samples+1)!=sourceMesh.vertexCount)throw new InvalidDataException("Unexpected ribbon topology: "+filter.name);
                var bins=Enumerable.Range(0,Levels).Select(_=>new List<int>()).ToArray();var triangles=sourceMesh.triangles;
                for(int i=0;i<triangles.Length;i+=3){
                    float progress=((triangles[i]/stride)+(triangles[i+1]/stride)+(triangles[i+2]/stride))/(3f*Samples);
                    int level=Mathf.RoundToInt(profile.Wetness(progress)*(Levels-1));bins[level].Add(triangles[i]);bins[level].Add(triangles[i+1]);bins[level].Add(triangles[i+2]);
                }
                var mesh=UnityEngine.Object.Instantiate(sourceMesh);mesh.name=sourceMesh.name+" / spatial wetness";mesh.subMeshCount=Levels;
                var materials=new Material[Levels];
                for(int level=0;level<Levels;level++){
                    float wet=level/(float)(Levels-1);mesh.SetTriangles(bins[level],level,false);
                    var m=new Material(original);m.name=original.name+" / wetness "+level;
                    if(response)m.SetColor("_Color",original.GetColor("_Color")*Mathf.Lerp(.12f,1.55f,wet));
                    if(reflection)m.SetFloat("_Strength",Mathf.Lerp(.02f,.85f,wet));
                    if(deck){m.SetFloat("_Smoothness",Mathf.Lerp(.32f,.94f,wet));if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",Mathf.Lerp(.08f,original.GetFloat("_Metallic"),wet));var c=original.GetColor("_BaseColor");m.SetColor("_BaseColor",new Color(c.r,c.g,c.b,c.a)*Mathf.Lerp(1,.84f,wet));}
                    materials[level]=Save(m,"Surface-"+serial+"-"+level+".mat");
                }
                filter.sharedMesh=Save(mesh,"Surface-"+(serial++)+".asset");renderer.sharedMaterials=materials;
                // Collider keeps the original mesh. Only render submesh/material assignment changed.
            }
            world.artRevision="rain-atmosphere-stage5-01";world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(before!=ProductionSceneSetup.Hash(File.ReadAllBytes(source)))throw new InvalidDataException("Dry scene modified");
            File.WriteAllText(Path.Combine(output,"atmosphere.txt"),$"scene={Candidate}\ncourse={world.courseHash}\nroofSamples={covered.Count(x=>x)}\ntotalSamples={Samples}\nrenderSurfaces={serial}\ntransitionMetresOutsideRoof=8\ndrySceneUnchanged=true\ncolliderMeshesUnchanged=true\noutdoorFogDensity=0.0055\n");
        }
        static T Save<T>(T asset,string name) where T:UnityEngine.Object
        {
            string path=Root+"/"+name;var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
    }
}
