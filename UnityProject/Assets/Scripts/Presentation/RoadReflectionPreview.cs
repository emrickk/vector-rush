using System;
using UnityEngine;
using UnityEngine.Rendering;
#if URP_SCREEN_SPACE_REFLECTION
using System.Reflection;
using Unity.Profiling;
using UnityEngine.Rendering.Universal;
#endif

namespace VectorRush
{
    // Prototype-only command line configuration; not a player-facing graphics option.
    public sealed class RoadReflectionPreview : MonoBehaviour
    {
        public static void Configure(VolumeProfile profile)
        {
            if (!profile) throw new ArgumentNullException(nameof(profile));
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-vrRoadSSR");
            bool requested = index >= 0 && index + 1 < args.Length && args[index + 1] == "on";
#if URP_SCREEN_SPACE_REFLECTION
            if (!profile.TryGet<ScreenSpaceReflectionVolumeSettings>(out var ssr))
                ssr = profile.Add<ScreenSpaceReflectionVolumeSettings>();
            ssr.mode.Override(requested
                ? ScreenSpaceReflectionVolumeSettings.ReflectionMode.OpaquesOnly
                : ScreenSpaceReflectionVolumeSettings.ReflectionMode.Disabled);
            ssr.resolution.Override(ScreenSpaceReflectionVolumeSettings.Resolution.Half);
            ssr.upscalingMethod.Override(ScreenSpaceReflectionVolumeSettings.UpscalingMethod.Bilinear);
            ssr.marchingMethod.Override(ScreenSpaceReflectionVolumeSettings.MarchingMethod.Linear);
            ssr.maxRaySteps.Override(32);
            ssr.maxRayLength.Override(30f);
            ssr.hitRefinementSteps.Override(5);
            ssr.objectThickness.Override(.325f);
            ssr.finalThicknessMultiplier.Override(.05f);
            ssr.roughnessFilter.Override(ScreenSpaceReflectionVolumeSettings.RoughReflectionsQuality.GaussianBlur);
            ssr.roughnessScale.Override(0f);
            ssr.reflectionStrength.Override(1f);
            ssr.minimumSmoothness.Override(.05f);
            ssr.smoothnessFadeStart.Override(.1f);
            ssr.reflectSky.Override(false);
            ssr.temporalFiltering.Override(false);
            Debug.Log("VR_SSR_REQUESTED " + requested + " compiled=true mode=" + ssr.mode.value);
            if (Array.IndexOf(args, "-vrSSRDiagnostics") >= 0)
                new GameObject("SSR preview diagnostics").AddComponent<RoadReflectionPreview>();
#else
            Debug.Log("VR_SSR_REQUESTED " + requested + " compiled=false effective=Disabled");
#endif
        }

#if URP_SCREEN_SPACE_REFLECTION
        // URP's resource container is internal. This read-only reflection path also works
        // in the Mono preview player and reports unavailable rather than assuming retention.
        public static object FindPersistentResources()
        {
            var type = typeof(ScreenSpaceReflectionRendererFeature).Assembly.GetType(
                "UnityEngine.Rendering.Universal.ScreenSpaceReflectionPersistentResources");
            if (type == null) return null;
            foreach (var method in typeof(GraphicsSettings).GetMethods(BindingFlags.Public | BindingFlags.Static))
                if (method.Name == "TryGetRenderPipelineSettings" && method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 1)
                {
                    var values = new object[] { null };
                    return (bool)method.MakeGenericMethod(type).Invoke(null, values) ? values[0] : null;
                }
            return null;
        }

        static readonly string[] MarkerNames = { "SSR - Upscaling", "SSR - Depth Pyramid Generation", "SSR - Final Blit" };
        readonly ProfilerRecorder[] recorders = new ProfilerRecorder[MarkerNames.Length];
        readonly long[] framesWithSamples = new long[MarkerNames.Length];
        readonly long[] maximumNanoseconds = new long[MarkerNames.Length];
        bool reported;

        void Start()
        {
            Debug.Log("VR_SSR_DIAGNOSTICS enabled; CPU marker samples do not prove GPU output correctness. Depth/normal/smoothness image inspection remains external.");
            StartRecorders();
        }

        void StartRecorders()
        {
            for (int i = 0; i < recorders.Length; i++)
                if (!recorders[i].Valid)
                {
                    recorders[i].Dispose();
                    recorders[i] = ProfilerRecorder.StartNew(ProfilerCategory.Render, MarkerNames[i], 1);
                }
        }

        void Update()
        {
            for (int i = 0; i < recorders.Length; i++)
                if (recorders[i].Valid && recorders[i].Count > 0)
                {
                    // LastValue is a CPU marker duration; do not label it isolated GPU cost.
                    long value = recorders[i].LastValue;
                    if (value > 0) framesWithSamples[i]++;
                    maximumNanoseconds[i] = Math.Max(maximumNanoseconds[i], value);
                }
            if (Time.frameCount % 120 == 0) StartRecorders();
            if (!reported && Time.frameCount >= 120)
            {
                reported = true;
                ReportNativeState();
                ReportMarkers();
            }
        }

        void ReportNativeState()
        {
            try
            {
                var resources = FindPersistentResources();
                Debug.Log("VR_SSR_NATIVE_RESOURCES container=" + (resources != null));
                if (resources != null)
                    foreach (string property in new[] { "Shader", "BlitShader" })
                    {
                        var shader = resources.GetType().GetProperty(property)?.GetValue(resources) as Shader;
                        Debug.Log("VR_SSR_NATIVE_RESOURCE " + property + "=" + (shader ? shader.name : "MISSING") + " supported=" + (shader && shader.isSupported));
                    }
                var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
                var renderers = pipeline ? field?.GetValue(pipeline) as ScriptableRendererData[] : null;
                int count = 0;
                if (renderers != null)
                    foreach (var renderer in renderers)
                        if (renderer)
                            foreach (var feature in renderer.rendererFeatures)
                                if (feature is ScreenSpaceReflectionRendererFeature ssr)
                                {
                                    count++;
                                    Debug.Log("VR_SSR_NATIVE_FEATURE renderer=" + renderer.name + " active=" + ssr.isActive + " afterOpaque=" + ssr.afterOpaque);
                                    foreach (string name in new[] { "m_Shader", "m_BlitShader", "m_Material", "m_BlitMaterial" })
                                    {
                                        var retained = typeof(ScreenSpaceReflectionRendererFeature).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ssr) as UnityEngine.Object;
                                        Debug.Log("VR_SSR_NATIVE_FEATURE_RESOURCE " + name + "=" + (retained ? retained.name : "UNINITIALIZED"));
                                    }
                                }
                Debug.Log("VR_SSR_NATIVE_FEATURE_COUNT " + count + " introspectionAvailable=" + (renderers != null));
                var settings = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflectionVolumeSettings>();
                Debug.Log("VR_SSR_NATIVE_VOLUME " + (settings ? JsonUtility.ToJson(settings) : "MISSING"));
                var road = Resources.Load<Material>("RoadSurface");
                Debug.Log("VR_SSR_NATIVE_ROAD_TEMPLATE optOut=" + (road && road.IsKeywordEnabled("_SCREENSPACEREFLECTIONS_OFF")) + " templateOnly=true");
                var surface = GameObject.Find("Running surface");
                var surfaceRenderer = surface ? surface.GetComponent<Renderer>() : null;
                var material = surfaceRenderer ? surfaceRenderer.sharedMaterial : null;
                Debug.Log("VR_SSR_NATIVE_ROAD_INSTANCE found=" + (material != null));
                if (material)
                {
                    Debug.Log("VR_SSR_NATIVE_ROAD_MATERIAL name=" + material.name + " shader=" + material.shader.name +
                        " keywords=" + string.Join(",", material.shaderKeywords));
                    foreach (string property in new[] { "_Smoothness", "_BumpScale", "_EnvironmentReflections" })
                        if (material.HasProperty(property))
                            Debug.Log("VR_SSR_NATIVE_ROAD_FLOAT " + property + "=" + material.GetFloat(property).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    foreach (string property in new[] { "_BaseMap", "_MetallicGlossMap", "_BumpMap" })
                        if (material.HasProperty(property))
                        {
                            var texture = material.GetTexture(property);
                            Debug.Log("VR_SSR_NATIVE_ROAD_MAP " + property + "=" + (texture ? texture.name : "NONE") + " pixelsInspected=false");
                        }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("VR_SSR_NATIVE_STATE_UNVERIFIED " + exception.GetType().Name + ": " + exception.Message);
            }
        }

        void ReportMarkers()
        {
            for (int i = 0; i < recorders.Length; i++)
                Debug.Log("VR_SSR_CPU_MARKER name=" + MarkerNames[i] + " valid=" + recorders[i].Valid +
                    " framesWithPositiveSamples=" + framesWithSamples[i] + " maxNanoseconds=" + maximumNanoseconds[i] +
                    " gpuOutputVerified=false");
        }

        void OnDestroy()
        {
            ReportMarkers();
            for (int i = 0; i < recorders.Length; i++) recorders[i].Dispose();
        }
#endif
    }
}
