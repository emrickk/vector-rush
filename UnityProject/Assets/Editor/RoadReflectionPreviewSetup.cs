using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if URP_SCREEN_SPACE_REFLECTION
using UnityEngine.Rendering.Universal;
#endif

namespace VectorRush.Editor
{
    public static class RoadReflectionPreviewSetup
    {
        [MenuItem("Vector Rush/SSR preview/Enable compile symbol")]
        public static void EnableDefine()
        {
            var target = NamedBuildTarget.Standalone;
            var symbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            if (symbols.Add("URP_SCREEN_SPACE_REFLECTION"))
            {
                var ordered = new List<string>(symbols);
                ordered.Sort(StringComparer.Ordinal);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", ordered));
            }
            AssetDatabase.SaveAssets();
            Debug.Log("VR_SSR_DEFINE_READY End this invocation; run BuildPreview after compilation/domain reload.");
        }

#if URP_SCREEN_SPACE_REFLECTION
        static void RetainRuntimeOffVariants(UniversalRendererData source)
        {
            // URP 17.6 prefilters SSR OFF out when every configured renderer has SSR.
            // An unused, SSR-free renderer supplies the OFF requirement to the build's
            // feature union while the original renderer remains selected for BOTH runs.
            const string path = "Assets/Settings/VectorSSRVariantRetentionRenderer.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/VectorPipeline.asset");
            if (!pipeline) throw new InvalidOperationException("VectorPipeline missing");
            var settings = new SerializedObject(pipeline);
            var renderers = settings.FindProperty("m_RendererDataList");
            var defaultIndex = settings.FindProperty("m_DefaultRendererIndex");
            if (renderers == null || defaultIndex == null || defaultIndex.intValue < 0 ||
                defaultIndex.intValue >= renderers.arraySize ||
                renderers.GetArrayElementAtIndex(defaultIndex.intValue).objectReferenceValue != source)
                throw new InvalidOperationException("SSR preview requires the original VectorRenderer to remain the selected default.");
            int selected = defaultIndex.intValue;
            var retained = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (!retained)
            {
                retained = UnityEngine.Object.Instantiate(source);
                retained.rendererFeatures.RemoveAll(feature => feature is ScreenSpaceReflectionRendererFeature);
                AssetDatabase.CreateAsset(retained, path);
            }
            else
            {
                // Refresh all settings on repeat builds; keep its asset GUID and all
                // non-SSR feature references, including the existing SSAO subasset.
                EditorUtility.CopySerialized(source, retained);
                retained.rendererFeatures.RemoveAll(feature => feature is ScreenSpaceReflectionRendererFeature);
            }
            retained.name = "SSR OFF variant retention (unused renderer)";
            EditorUtility.SetDirty(retained);
            bool found = false;
            for (int i = 0; i < renderers.arraySize; i++)
                if (renderers.GetArrayElementAtIndex(i).objectReferenceValue == retained) found = true;
            if (!found)
            {
                int index = renderers.arraySize;
                renderers.InsertArrayElementAtIndex(index);
                renderers.GetArrayElementAtIndex(index).objectReferenceValue = retained;
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            Debug.Log("VR_SSR_VARIANT_RETENTION renderer=" + path + " defaultIndex=" + selected +
                " originalDefaultPreserved=true expectedSSRPrefilter=Select(1); native off/on verification still required");
        }
#endif

        [MenuItem("Vector Rush/SSR preview/Build separate macOS preview")]
        public static void BuildPreview()
        {
#if URP_SCREEN_SPACE_REFLECTION
            VectorRushSetup.Prepare();
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/VectorRenderer.asset");
            if (!renderer) throw new InvalidOperationException("VectorRenderer missing");
            ScreenSpaceReflectionRendererFeature ssr = null;
            foreach (var feature in renderer.rendererFeatures)
                if (feature is ScreenSpaceReflectionRendererFeature found)
                {
                    if (ssr) throw new InvalidOperationException("Multiple SSR features; resolve duplicate registration before building.");
                    ssr = found;
                }
            if (!(ssr is RoadReflectionRendererFeature))
            {
                var previous = ssr;
                int index = previous ? renderer.rendererFeatures.IndexOf(previous) : -1;
                ssr = ScriptableObject.CreateInstance<RoadReflectionRendererFeature>();
                ssr.name = "Nocturne SSR preview 02 - game cameras";
                AssetDatabase.AddObjectToAsset(ssr, renderer);
                if (index >= 0) renderer.rendererFeatures[index] = ssr;
                else renderer.rendererFeatures.Add(ssr);
                // The prior prototype is preserved in source history and its separate app.
                // Replace only its feature subasset; retain SSAO and feature-list order.
                if (previous) UnityEngine.Object.DestroyImmediate(previous, true);
            }
            ssr.afterOpaque = false;
            ssr.SetActive(true);
            EditorUtility.SetDirty(ssr);
            EditorUtility.SetDirty(renderer);
            RetainRuntimeOffVariants(renderer);
            // The resource type is internal in the pinned URP package. Inspect it without
            // modifying the package or inventing a public type reference.
            var resources = RoadReflectionPreview.FindPersistentResources();
            if (resources == null) throw new InvalidOperationException("SSR persistent resources unavailable in URP global settings.");
            foreach (string property in new[] { "Shader", "BlitShader" })
            {
                var shader = resources.GetType().GetProperty(property)?.GetValue(resources) as Shader;
                if (!shader) throw new InvalidOperationException("SSR persistent resource missing: " + property);
                Debug.Log("VR_SSR_BUILD_RESOURCE " + property + "=" + AssetDatabase.GetAssetPath(shader));
            }
            var globalSettings = AssetDatabase.LoadMainAssetAtPath("Assets/UniversalRenderPipelineGlobalSettings.asset");
            if (!globalSettings) throw new InvalidOperationException("URP global settings asset missing");
            EditorUtility.SetDirty(globalSettings);
            AssetDatabase.SaveAssets();
            string output = Path.GetFullPath("../Builds/Vector Rush-SSR-preview-02.app");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            // Unity 6.4+ managed instrumentation is independent of Development build.
            // The supported variant retains RenderGraph profiling scopes without raw defines.
            PlayerSettings.SetManagedCodeVariant(NamedBuildTarget.Standalone, ManagedCodeVariant.Instrumented);
            AssetDatabase.SaveAssets();
            // Both off/on runs use this binary; no script debugging or auto-connection.
            Debug.Log("VR_SSR_BUILD options=Development managedCodeVariant=Instrumented output=" + output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Solstice.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("SSR preview build failed: " + report.summary.result);
            Debug.Log("VR_SSR_BUILD_COMPLETE " + output);
#else
            throw new InvalidOperationException("SSR feature unavailable. Run EnableDefine in a separate Editor invocation, allow compilation/domain reload, then run BuildPreview.");
#endif
        }
    }
}
