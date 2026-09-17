using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace VectorRush.Editor
{
    public static class ProductionSceneSetup
    {
        public const string ScenePath="Assets/Scenes/NocturneProduction.unity";
        public const string Root="Assets/World/NocturneProduction";
        static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        [Serializable] public class Frame { public float progress,distanceMeters;public float[] position,forward,right,up; }
        [Serializable] public class Course { public float width,length;public Frame[] frames; }
        [Serializable] public class Context { public int contractVersion=1; public string courseHash,sourceCommit;public Course course;public float cameraDistance=10.5f,cameraHeight=4.6f; public string cameraFov="65 + clamp01(speedKph/380)*10 + (boosting ? 5 : 0)"; }
        [Serializable] public class BuildIdentity { public string status,unityVersion,buildGuid,scene,appPath,courseHash,artRevision;public int renderers,lights; }
        [Serializable] public class ExperiencePreparation { public string status,unityVersion,revision,courseHash,packagePath,directAssetRoot,worldAssetRoot,sceneAssetPath,settingsAssetRoot;public int instances,lights,reflectionProbes; }
        public static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        static float[] V(Vector3 v)=>new[]{v.x,v.y,v.z};
        public static Course ReadCourse(TrackPath track)
        {
            track.Ensure();var data=new Course{width=track.Width,length=track.Length,frames=new Frame[1201]};
            for(int i=0;i<=1200;i++){float p=i/1200f;var f=track.Evaluate(p);data.frames[i]=new Frame{progress=p,distanceMeters=p*track.Length,position=V(f.Position),forward=V(f.Forward),right=V(f.Right),up=V(f.Up)};}
            return data;
        }
        public static string CourseHash(TrackPath track)=>Hash(Utf8.GetBytes(JsonUtility.ToJson(ReadCourse(track))));
        public static string RequiredFlag(string flag)
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,flag);
            if(i<0||i+1>=args.Length||args[i+1].StartsWith("-"))throw new ArgumentException("Missing "+flag+" <absolute path>");
            if(!Path.IsPathRooted(args[i+1]))throw new ArgumentException(flag+" must be absolute");return Path.GetFullPath(args[i+1]);
        }
        static string OptionalFlag(string flag,string fallback)
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,flag);
            if(i<0)return fallback;if(i+1>=args.Length||args[i+1].StartsWith("-"))throw new ArgumentException("Missing value for "+flag);
            return args[i+1];
        }
        static string FreshEvidence(){string path=RequiredFlag("-productionEvidence");if(Directory.Exists(path)&&Directory.GetFileSystemEntries(path).Length>0)throw new IOException("Evidence folder must be fresh: "+path);Directory.CreateDirectory(path);return path;}
        public static void ExportContext()
        {
            string evidence=FreshEvidence();var go=new GameObject("Course export");
            try{var track=go.AddComponent<TrackPath>();ExportContextToDirectory(track,evidence,ReadCommit());
                Debug.Log("PRODUCTION_CONTEXT_COMPLETE "+CourseHash(track));
            }finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        public static void ExportContextToDirectory(TrackPath track,string output,string sourceCommit)
        {
            if(!track)throw new ArgumentNullException(nameof(track));
            if(string.IsNullOrWhiteSpace(output))throw new ArgumentException("Output directory is required",nameof(output));
            output=Path.GetFullPath(output);Directory.CreateDirectory(output);
            var course=ReadCourse(track);byte[] courseBytes=Utf8.GetBytes(JsonUtility.ToJson(course));
            File.WriteAllBytes(Path.Combine(output,"course-data.json"),courseBytes);
            var context=new Context{course=course,courseHash=Hash(courseBytes),sourceCommit=sourceCommit};
            string geometry=JsonUtility.ToJson(context,false);
            File.WriteAllText(Path.Combine(output,"geometry-context.json"),geometry,Utf8);
            File.WriteAllText(Path.Combine(output,"geometry-context.sha256"),Hash(Utf8.GetBytes(geometry))+"\n",Utf8);
            File.WriteAllText(Path.Combine(output,"C1_STATUS.md"),"# C1 context export\n\nExact closed-course geometry exported for downstream production work.\n",Utf8);
        }
        static string ReadCommit(){string git=Path.GetFullPath("../.git");if(!Directory.Exists(git))return "isolated-validation-copy";string head=File.ReadAllText(Path.Combine(git,"HEAD")).Trim();if(!head.StartsWith("ref: "))return head;string path=Path.Combine(git,head.Substring(5));return File.Exists(path)?File.ReadAllText(path).Trim():"see repository build record";}
        public static void ImportArt(){throw new NotSupportedException("The owner-authorized direct pipeline uses persistent Unity-authored geometry. External package import is not implemented; use BuildScene with -productionEvidence.");}
        public static void PrepareExperienceScene()
        {
            string evidence=FreshEvidence();string package=RequiredFlag("-experiencePackage");
            string direct=OptionalFlag("-experienceDirectRoot","Assets/Resources/ExperienceArt");
            string world=OptionalFlag("-experienceWorldRoot","Assets/World/Experience");
            string scene=OptionalFlag("-experienceScene","Assets/Scenes/NocturneExperience.unity");
            var trackObject=new GameObject("Experience course validation");
            try
            {
                var track=trackObject.AddComponent<TrackPath>();string courseHash=CourseHash(track);
                ProductionImportResult import=ProductionArtImporter.ResolveExistingPayload(package,courseHash,direct,world);
                string settings=OptionalFlag("-experienceSettingsRoot","Assets/Settings/Experience/"+import.revision);
                ProductionSceneBuildResult build=ProductionSceneBuilder.BuildScene(import,scene,settings);
                var report=new ExperiencePreparation{status="EXPERIENCE_SCENE_PREPARED_NOT_ART_ACCEPTED",unityVersion=Application.unityVersion,
                    revision=import.revision,courseHash=courseHash,packagePath=package,directAssetRoot=direct,worldAssetRoot=import.worldAssetRoot,
                    sceneAssetPath=build.sceneAssetPath,settingsAssetRoot=build.settingsAssetRoot,instances=build.instanceCount,
                    lights=build.lightCount,reflectionProbes=build.reflectionProbeCount};
                File.WriteAllText(Path.Combine(evidence,"experience-scene.json"),JsonUtility.ToJson(report,true),Utf8);
                Debug.Log("EXPERIENCE_SCENE_PREPARED "+import.revision+" "+courseHash);
            }
            finally{UnityEngine.Object.DestroyImmediate(trackObject);}
        }
        public static void UpgradeExperienceScene()
        {
            string evidence=FreshEvidence();string package=RequiredFlag("-experiencePackage");
            string scene=OptionalFlag("-experienceScene","Assets/Scenes/NocturneExperience.unity");
            string settings=OptionalFlag("-experienceSettingsRoot","Assets/Settings/Experience/integration-upgrade");
            var trackObject=new GameObject("Experience upgrade validation");
            try
            {
                var track=trackObject.AddComponent<TrackPath>();string courseHash=CourseHash(track);
                ValidatedProductionPackage validated=ProductionArtImporter.ValidatePackage(package,courseHash);
                ProductionSceneBuildResult build=ProductionSceneBuilder.UpgradeSceneIntegration(scene,settings,validated.lighting.environment);
                var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
                var report=new ExperiencePreparation{status="EXPERIENCE_SCENE_UPGRADED_NOT_ART_ACCEPTED",unityVersion=Application.unityVersion,
                    revision=world.artRevision,courseHash=world.courseHash,packagePath=package,directAssetRoot="UNCHANGED",worldAssetRoot="UNCHANGED",
                    sceneAssetPath=build.sceneAssetPath,settingsAssetRoot=build.settingsAssetRoot,instances=0,
                    lights=build.lightCount,reflectionProbes=build.reflectionProbeCount};
                File.WriteAllText(Path.Combine(evidence,"experience-scene.json"),JsonUtility.ToJson(report,true),Utf8);
                Debug.Log("EXPERIENCE_SCENE_UPGRADED "+world.artRevision+" "+world.courseHash);
            }
            finally{UnityEngine.Object.DestroyImmediate(trackObject);}
        }
        public static void BuildScene()
        {
            string evidence=FreshEvidence();CreateScene();VerifySavedScene();
            File.WriteAllText(Path.Combine(evidence,"scene-proof.json"),JsonUtility.ToJson(Identity("SCENE_SERIALIZATION_VERIFIED"),true),Utf8);
            Debug.Log("PRODUCTION_SCENE_COMPLETE "+ScenePath);
        }
        public static void CreateScene()
        {
            if(AssetDatabase.IsValidFolder(Root))throw new IOException("Production asset folder already exists; author a new revision rather than overwrite it: "+Root);
            if(File.Exists(ScenePath))throw new IOException("Production scene already exists; refusing to overwrite it.");
            Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("Nocturne / persistent production world");var world=root.AddComponent<ProductionWorld>();
            world.track=root.AddComponent<TrackPath>();world.courseHash=CourseHash(world.track);world.artRevision="direct-architecture-01";
            var palette=new ProductionArchitecture(world.track,root.transform);palette.Build();
            var library=ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            library.craftSurfaceTemplate=UnityEngine.Object.Instantiate(Resources.Load<Material>("CraftSurfaceLit"));
            if(!library.craftSurfaceTemplate)throw new InvalidOperationException("Authored craft surface template missing");
            AssetDatabase.CreateAsset(library.craftSurfaceTemplate,Root+"/Materials/CraftSurface.mat");
            library.craftMetalFallback=palette.Material("Craft metal",new Color(.3f,.37f,.4f),.7f,.8f);
            library.craftGlass=palette.Material("Craft glass",new Color(.008f,.014f,.019f),.87f);
            library.craftEngineAccent=palette.Material("Craft nozzle",new Color(.045f,.19f,.25f),.62f,.35f,new Color(.015f,.46f,.68f));
            library.craftEngineCore=palette.Material("Craft core",new Color(.65f,.85f,.92f),.42f,0,new Color(1.7f,2.6f,3));
            AssetDatabase.CreateAsset(library,Root+"/CraftMaterials.asset");world.materials=library;
            var bootstrap=new GameObject("VECTOR RUSH / production").AddComponent<VectorBootstrap>();bootstrap.productionWorld=world;
            var camera=new GameObject("Race camera").AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=67;camera.nearClipPlane=.18f;camera.farClipPlane=5000;camera.allowHDR=true;
            camera.gameObject.AddComponent<AudioListener>();camera.gameObject.AddComponent<ChaseCamera>();
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            profile.Add<Bloom>().intensity.Override(.25f);profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.4f);color.contrast.Override(6);color.saturation.Override(-4);
            profile.Add<Vignette>().intensity.Override(.13f);AssetDatabase.CreateAsset(profile,Root+"/Grade.asset");
            var volume=new GameObject("Persistent night grade").AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.035f,.055f,.075f).gamma;RenderSettings.fogDensity=.0018f;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.22f,.28f,.36f);RenderSettings.ambientEquatorColor=new Color(.12f,.16f,.21f);RenderSettings.ambientGroundColor=new Color(.055f,.07f,.09f);
            var sun=new GameObject("Blue hour key").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.color=new Color(.7f,.81f,1);sun.intensity=1.15f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.65f;RenderSettings.sun=sun;
            var sky=new Material(Shader.Find("VectorRush/Night Sky"));AssetDatabase.CreateAsset(sky,Root+"/Materials/NightSky.mat");RenderSettings.skybox=sky;
            // Dedicated renderer avoids SSR prototype references while preserving legacy settings.
            var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();renderer.renderingMode=RenderingMode.ForwardPlus;
            renderer.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            AssetDatabase.CreateAsset(renderer,Root+"/Renderer.asset");
            var ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Production contact depth";AssetDatabase.AddObjectToAsset(ao,renderer);renderer.rendererFeatures.Add(ao);
            var pipeline=UniversalRenderPipelineAsset.Create(renderer);pipeline.supportsHDR=true;pipeline.msaaSampleCount=4;pipeline.shadowDistance=160;pipeline.shadowCascadeCount=4;
            pipeline.mainLightShadowmapResolution=2048;pipeline.maxAdditionalLightsCount=8;
            AssetDatabase.CreateAsset(pipeline,Root+"/Pipeline.asset");EditorUtility.SetDirty(renderer);
            world.ValidateReady();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
        }
        public static void VerifySavedScene()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var worlds=UnityEngine.Object.FindObjectsByType<ProductionWorld>(FindObjectsSortMode.None);
            if(worlds.Length!=1)throw new InvalidOperationException("Expected one production world");worlds[0].ValidateReady();
            if(worlds[0].courseHash!=CourseHash(worlds[0].track))throw new InvalidOperationException("Saved course hash mismatch");
            foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                if(!filter.sharedMesh||!AssetDatabase.Contains(filter.sharedMesh))throw new InvalidOperationException("Nonpersistent mesh: "+filter.name);
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                foreach(var material in renderer.sharedMaterials)if(!material||!AssetDatabase.Contains(material))throw new InvalidOperationException("Nonpersistent material: "+renderer.name);
            if(UnityEngine.Object.FindObjectsByType<WorldBuilder>(FindObjectsSortMode.None).Length>0)throw new InvalidOperationException("Legacy generator present");
            if(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length!=1)throw new InvalidOperationException("Camera count mismatch");
        }
        public static void BakeScene()
        {
            string evidence=FreshEvidence();VerifySavedScene();if(!Lightmapping.Bake())throw new InvalidOperationException("Lightmap bake failed");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());File.WriteAllText(Path.Combine(evidence,"bake.txt"),"Unity Lightmapping.Bake completed; native lighting acceptance remains pending.\n");
        }
        public static void BuildCandidate()
        {
            string output=RequiredFlag("-productionBuildOutput");
            if(!output.EndsWith(".app",StringComparison.OrdinalIgnoreCase)||Directory.Exists(output)||File.Exists(output)||Path.GetFileName(output)=="Vector Rush.app")throw new IOException("Require a fresh, separately named .app output");
            string evidence=FreshEvidence();VerifySavedScene();
            var originalGraphics=GraphicsSettings.defaultRenderPipeline;var originalQuality=QualitySettings.renderPipeline;
            var originalGlobal=GraphicsSettings.GetSettingsForRenderPipeline<UniversalRenderPipeline>();
            try
            {
                const string globalPath=Root+"/GlobalSettings.asset";
                var global=AssetDatabase.LoadAssetAtPath<RenderPipelineGlobalSettings>(globalPath);
                if(!global)global=RenderPipelineGlobalSettingsUtils.Create(typeof(UniversalRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings",true),globalPath);
                GraphicsSettings.RegisterRenderPipelineSettings<UniversalRenderPipeline>(global);
                var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root+"/Pipeline.asset");GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Production build failed: "+report.summary.result);
                var identity=Identity("NATIVE_BUILD_COMPLETE_NOT_QUALITY_ACCEPTED");identity.appPath=output;identity.buildGuid=report.summary.guid.ToString();
                File.WriteAllText(Path.Combine(evidence,"build-identity.json"),JsonUtility.ToJson(identity,true),Utf8);Debug.Log("PRODUCTION_BUILD_COMPLETE "+output);
            }finally{GraphicsSettings.RegisterRenderPipelineSettings<UniversalRenderPipeline>(originalGlobal);GraphicsSettings.defaultRenderPipeline=originalGraphics;QualitySettings.renderPipeline=originalQuality;AssetDatabase.SaveAssets();}
        }
        public static void BuildExperienceCandidate()
        {
            string scene=OptionalFlag("-experienceScene","Assets/Scenes/NocturneExperience.unity");
            string output=RequiredFlag("-experienceBuildOutput");
            if(!output.EndsWith(".app",StringComparison.OrdinalIgnoreCase)||Directory.Exists(output)||File.Exists(output))
                throw new IOException("Require a fresh .app output for -experienceBuildOutput");
            string evidence=FreshEvidence();
            EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
            VerifyExperienceScene(scene);
            var configuration=UnityEngine.Object.FindFirstObjectByType<ProductionRenderConfiguration>();
            var pipeline=configuration.renderPipeline as UniversalRenderPipelineAsset;
            if(!pipeline)throw new InvalidOperationException("Experience scene has no URP render pipeline");
            var originalGraphics=GraphicsSettings.defaultRenderPipeline;var originalQuality=QualitySettings.renderPipeline;
            try
            {
                GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Experience build failed: "+report.summary.result);
                var identity=Identity("NATIVE_EXPERIENCE_BUILD_COMPLETE_NOT_QUALITY_ACCEPTED",scene);identity.appPath=output;identity.buildGuid=report.summary.guid.ToString();
                File.WriteAllText(Path.Combine(evidence,"build-identity.json"),JsonUtility.ToJson(identity,true),Utf8);
                Debug.Log("EXPERIENCE_BUILD_COMPLETE "+output);
            }
            finally{GraphicsSettings.defaultRenderPipeline=originalGraphics;QualitySettings.renderPipeline=originalQuality;AssetDatabase.SaveAssets();}
        }
        static void VerifyExperienceScene(string scene)
        {
            var worlds=UnityEngine.Object.FindObjectsByType<ProductionWorld>(FindObjectsSortMode.None);
            if(worlds.Length!=1)throw new InvalidOperationException("Experience scene requires exactly one ProductionWorld");
            worlds[0].ValidateReady();
            if(UnityEngine.Object.FindObjectsByType<VectorBootstrap>(FindObjectsSortMode.None).Length!=1)throw new InvalidOperationException("Experience scene requires exactly one VectorBootstrap");
            if(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Count(value=>value.CompareTag("MainCamera"))!=1)throw new InvalidOperationException("Experience scene requires exactly one active MainCamera");
            var volume=UnityEngine.Object.FindFirstObjectByType<Volume>();
            if(!volume||!volume.sharedProfile||!volume.sharedProfile.TryGet(out MotionBlur _))throw new InvalidOperationException("Experience scene is missing its persistent motion blur override");
            if(UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length<1)throw new InvalidOperationException("Experience scene is missing a global lighting baseline");
            if(!UnityEngine.Object.FindFirstObjectByType<ProductionRenderConfiguration>())throw new InvalidOperationException("Experience scene is missing its render configuration: "+scene);
        }
        static BuildIdentity Identity(string status,string scenePath=ScenePath)
        {
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();return new BuildIdentity{status=status,unityVersion=Application.unityVersion,scene=scenePath,courseHash=world.courseHash,artRevision=world.artRevision,renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length,lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length};
        }
    }
}
