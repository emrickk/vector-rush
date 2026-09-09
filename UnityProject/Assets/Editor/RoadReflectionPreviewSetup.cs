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
            if (!ssr)
            {
                ssr = ScriptableObject.CreateInstance<ScreenSpaceReflectionRendererFeature>();
                ssr.name = "Nocturne SSR preview";
                AssetDatabase.AddObjectToAsset(ssr, renderer);
                renderer.rendererFeatures.Add(ssr);
            }
            ssr.afterOpaque = false;
            ssr.SetActive(true);
            EditorUtility.SetDirty(ssr);
            EditorUtility.SetDirty(renderer);
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
            string output = Path.GetFullPath("../Builds/Vector Rush-SSR-preview-01.app");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            // Development retains native profiler markers. Both off/on runs use this binary.
            // No script debugging or automatic profiler connection is enabled.
            Debug.Log("VR_SSR_BUILD options=Development output=" + output);
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
