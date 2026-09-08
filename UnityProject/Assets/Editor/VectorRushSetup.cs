using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace VectorRush.Editor
{
    public static class VectorRushSetup
    {
        [MenuItem("Vector Rush/Prepare project and scene")]
        public static void Prepare()
        {
            Directory.CreateDirectory("Assets/Settings");Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Resources");
            const string pipelinePath="Assets/Settings/VectorPipeline.asset";
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if(!pipeline){
                var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer,"Assets/Settings/VectorRenderer.asset");
                pipeline=UniversalRenderPipelineAsset.Create(renderer);pipeline.name="Vector Pipeline";
                pipeline.renderScale=1;pipeline.msaaSampleCount=4;pipeline.supportsHDR=true;pipeline.shadowDistance=170;pipeline.shadowCascadeCount=4;
                AssetDatabase.CreateAsset(pipeline,pipelinePath);
            }
            GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
            var rendererData=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/VectorRenderer.asset");
            rendererData.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if(!rendererData.postProcessData)throw new Exception("URP post-processing resource is missing");
            EditorUtility.SetDirty(rendererData);
            pipeline.mainLightShadowmapResolution=4096;pipeline.shadowDistance=150;pipeline.shadowCascadeCount=4;
            var pipelineSettings=new SerializedObject(pipeline);pipelineSettings.FindProperty("m_SoftShadowsSupported").boolValue=true;pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            PlayerSettings.companyName="Vector Studio";PlayerSettings.productName="Vector Rush";
            PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
            PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.runInBackground=true;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=2;settings.ApplyModifiedPropertiesWithoutUndo();}
            var surface=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/SurfaceLit.mat");
            if(!surface){surface=new Material(Shader.Find("Universal Render Pipeline/Lit"));surface.EnableKeyword("_EMISSION");surface.SetColor("_EmissionColor",Color.black);AssetDatabase.CreateAsset(surface,"Assets/Resources/SurfaceLit.mat");}
            surface.SetColor("_EmissionColor",Color.white);surface.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;surface.EnableKeyword("_EMISSION");EditorUtility.SetDirty(surface);
            // The lit template retains only needed variants; these small shaders are used by name.
            var gs=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var shaders=gs.FindProperty("m_AlwaysIncludedShaders");
            foreach(string name in new[]{"Skybox/Procedural","VectorRush/Ocean","Universal Render Pipeline/Unlit","VectorRush/Ion Trail"}){
                var shader=Shader.Find(name);if(!shader)continue;bool found=false;
                for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;
                if(!found){int i=shaders.arraySize;shaders.InsertArrayElementAtIndex(i);shaders.GetArrayElementAtIndex(i).objectReferenceValue=shader;}
            }gs.ApplyModifiedPropertiesWithoutUndo();
            var importer=AssetImporter.GetAtPath("Assets/Resources/Art/HeroShip.fbx") as ModelImporter;
            if(importer){importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.importCameras=false;importer.importLights=false;importer.globalScale=1;importer.SaveAndReimport();}
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("VECTOR RUSH").AddComponent<VectorBootstrap>();
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Solstice.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Solstice.unity",true)};
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("VECTOR_SETUP_COMPLETE");
        }
        [MenuItem("Vector Rush/Build macOS player")]
        public static void BuildMac()
        {
            Prepare();
            string output=Path.GetFullPath("../Builds/Vector Rush.app");Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options=new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Solstice.unity"},locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.None};
            var report=BuildPipeline.BuildPlayer(options);
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Vector Rush build failed: "+report.summary.result);
            Debug.Log("VECTOR_BUILD_COMPLETE "+output);
        }
    }
}
