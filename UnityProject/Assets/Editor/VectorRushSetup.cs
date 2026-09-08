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
            rendererData.renderingMode=RenderingMode.ForwardPlus;
            var lightSettings=new SerializedObject(pipeline);
            lightSettings.FindProperty("m_AdditionalLightsRenderingMode").intValue=(int)LightRenderingMode.PerPixel;
            lightSettings.ApplyModifiedPropertiesWithoutUndo();pipeline.maxAdditionalLightsCount=8;
            rendererData.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if(!rendererData.postProcessData)throw new Exception("URP post-processing resource is missing");
            ScreenSpaceAmbientOcclusion occlusion=null;
            foreach(var feature in rendererData.rendererFeatures)if(feature is ScreenSpaceAmbientOcclusion existing)occlusion=existing;
            if(!occlusion){
                occlusion=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();occlusion.name="Craft and architecture contact depth";
                AssetDatabase.AddObjectToAsset(occlusion,rendererData);rendererData.rendererFeatures.Add(occlusion);
            }
            var ao=new SerializedObject(occlusion);var aoSettings=ao.FindProperty("m_Settings");
            aoSettings.FindPropertyRelative("Intensity").floatValue=.7f;
            aoSettings.FindPropertyRelative("Radius").floatValue=.65f;
            aoSettings.FindPropertyRelative("DirectLightingStrength").floatValue=.08f;
            aoSettings.FindPropertyRelative("Falloff").floatValue=90f;
            aoSettings.FindPropertyRelative("Downsample").boolValue=false;
            aoSettings.FindPropertyRelative("Source").intValue=1;
            aoSettings.FindPropertyRelative("Samples").intValue=1;
            ao.ApplyModifiedPropertiesWithoutUndo();occlusion.SetActive(true);EditorUtility.SetDirty(occlusion);
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
            var road=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/RoadSurface.mat");
            if(!road){road=new Material(surface);AssetDatabase.CreateAsset(road,"Assets/Resources/RoadSurface.mat");}
            // A diffuse composite deck avoids the static harbor cubemap projecting
            // large reflected silhouettes over the driving line. Other surfaces retain reflections.
            road.SetFloat("_EnvironmentReflections",0);road.SetFloat("_SpecularHighlights",1);
            road.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");road.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            road.SetTexture("_BumpMap",Texture2D.normalTexture);road.SetFloat("_BumpScale",.25f);road.EnableKeyword("_NORMALMAP");
            road.SetTexture("_MetallicGlossMap",Texture2D.whiteTexture);road.EnableKeyword("_METALLICSPECGLOSSMAP");
            road.SetFloat("_Metallic",0);road.SetFloat("_Smoothness",.16f);road.SetColor("_EmissionColor",Color.black);road.EnableKeyword("_EMISSION");EditorUtility.SetDirty(road);
            PrepareRoadReflectionMaterial(road);
            PrepareCoastalMaterial();
            PrepareCraftSurfaceMaterial();
            // The lit template retains only needed variants; these small shaders are used by name.
            var gs=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            gs.FindProperty("m_FogStripping").intValue=1; // Custom: retain the runtime Exp2 mode.
            gs.FindProperty("m_FogKeepLinear").boolValue=false;
            gs.FindProperty("m_FogKeepExp").boolValue=false;
            gs.FindProperty("m_FogKeepExp2").boolValue=true;
            var shaders=gs.FindProperty("m_AlwaysIncludedShaders");
            foreach(string name in new[]{"Skybox/Procedural","VectorRush/Ocean","Universal Render Pipeline/Unlit","VectorRush/Ion Trail","VectorRush/NightWindows","VectorRush/Night Sky"}){
                var shader=Shader.Find(name);if(!shader)continue;bool found=false;
                for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;
                if(!found){int i=shaders.arraySize;shaders.InsertArrayElementAtIndex(i);shaders.GetArrayElementAtIndex(i).objectReferenceValue=shader;}
            }gs.ApplyModifiedPropertiesWithoutUndo();
            var importer=AssetImporter.GetAtPath("Assets/Resources/Art/HeroShip.fbx") as ModelImporter;
            if(importer){importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.importCameras=false;importer.importLights=false;importer.globalScale=1;importer.SaveAndReimport();}
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("VECTOR RUSH").AddComponent<VectorBootstrap>();
            // Runtime scenery uses Exp2 atmosphere. Keep that native shader variant
            // by serializing the same fog mode into the bootstrap scene before build.
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogColor=new Color(.041f,.074f,.086f).gamma;RenderSettings.fogDensity=.0018f;
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Solstice.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Solstice.unity",true)};
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("VECTOR_SETUP_COMPLETE");
        }
        static void PrepareRoadReflectionMaterial(Material control)
        {
            const string path="Assets/Resources/RoadSurfaceReflections.mat";
            var reflected=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!reflected){reflected=new Material(control);AssetDatabase.CreateAsset(reflected,path);}
            // Retain the exact mapped Lit variant in the native build; keep the original control intact.
            reflected.CopyPropertiesFromMaterial(control);
            reflected.SetFloat("_EnvironmentReflections",1);
            reflected.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            EditorUtility.SetDirty(reflected);
        }
        static void PrepareCraftSurfaceMaterial()
        {
            const string folder="Assets/Resources/Art/ShipSurfaces/";
            if(!Directory.Exists(folder))return;
            foreach(string key in new[]{"Ivory","Graphite","Metal","Ceramic"})
                foreach(string suffix in new[]{"BaseColor","Normal","MetallicSmoothness","Occlusion"})
                {
                    string path=folder+key+"_"+suffix+".png";
                    var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                    if(!importer)throw new Exception("Incomplete selected ship surface payload: "+path);
                    importer.textureType=suffix=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
                    importer.sRGBTexture=suffix=="BaseColor";
                    importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=false;
                    importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;
                    importer.anisoLevel=8;importer.maxTextureSize=2048;importer.mipmapEnabled=true;
                    importer.textureCompression=TextureImporterCompression.CompressedHQ;
                    importer.SaveAndReimport();
                }
            const string materialPath="Assets/Resources/CraftSurfaceLit.mat";
            var craft=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!craft){craft=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(craft,materialPath);}
            craft.SetColor("_BaseColor",Color.white);craft.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Ivory_BaseColor.png"));
            craft.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Ivory_Normal.png"));craft.SetFloat("_BumpScale",1);craft.EnableKeyword("_NORMALMAP");
            craft.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Ivory_MetallicSmoothness.png"));craft.SetFloat("_Smoothness",1);craft.SetFloat("_SmoothnessTextureChannel",0);craft.EnableKeyword("_METALLICSPECGLOSSMAP");
            craft.SetTexture("_OcclusionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Ivory_Occlusion.png"));craft.SetFloat("_OcclusionStrength",.6f);craft.EnableKeyword("_OCCLUSIONMAP");
            craft.SetColor("_EmissionColor",Color.black);craft.EnableKeyword("_EMISSION");EditorUtility.SetDirty(craft);
        }
        static void PrepareCoastalMaterial()
        {
            var cliffImporter=AssetImporter.GetAtPath("Assets/Resources/Art/Environment/Solstice_CoastalCliff_C.fbx") as ModelImporter;
            if(cliffImporter){cliffImporter.importNormals=ModelImporterNormals.Import;cliffImporter.importTangents=ModelImporterTangents.CalculateMikk;cliffImporter.importCameras=false;cliffImporter.importLights=false;cliffImporter.SaveAndReimport();}
            const string folder="Assets/Resources/Art/Environment/Textures/";
            foreach(string name in new[]{"Rock3_CC0_Albedo.jpg","Rock3_CC0_NormalGL.png","Rock3_DERIVED_MetallicSmoothness.png"}){
                var importer=AssetImporter.GetAtPath(folder+name) as TextureImporter;
                if(!importer)throw new Exception("Missing coastal texture: "+name);
                importer.textureType=name.Contains("Normal")?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=name.Contains("Albedo");importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;
                importer.anisoLevel=8;importer.maxTextureSize=2048;importer.mipmapEnabled=true;importer.SaveAndReimport();
            }
            const string path="Assets/Resources/CoastalRock.mat";
            var rock=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!rock){rock=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(rock,path);}
            rock.SetColor("_BaseColor",Color.white);rock.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Rock3_CC0_Albedo.jpg"));
            rock.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Rock3_CC0_NormalGL.png"));rock.SetFloat("_BumpScale",.75f);rock.EnableKeyword("_NORMALMAP");
            rock.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Rock3_DERIVED_MetallicSmoothness.png"));rock.SetFloat("_Metallic",0);rock.SetFloat("_Smoothness",1);rock.SetFloat("_SmoothnessTextureChannel",0);rock.EnableKeyword("_METALLICSPECGLOSSMAP");
            rock.SetTextureScale("_BaseMap",new Vector2(4,4));EditorUtility.SetDirty(rock);
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
