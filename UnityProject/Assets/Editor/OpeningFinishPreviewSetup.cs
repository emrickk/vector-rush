using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VectorRush.Editor
{
    public static class OpeningFinishPreviewSetup
    {
        const string ScenePath = "Assets/Scenes/Solstice.unity";
        const string RendererPath = "Assets/Settings/VectorRenderer.asset";
        const string PipelinePath = "Assets/Settings/VectorPipeline.asset";

        [MenuItem("Vector Rush/Opening finish/Build separate macOS preview")]
        public static void BuildPreview()
        {
            string output = ResolveOutput(Environment.GetCommandLineArgs());
            RejectUnsafeOutput(output);

            VectorRushSetup.Prepare();

            // The regional road replaces these template maps at runtime. Keep an authored
            // detail map reference so material validation also retains the required variant.
            var roadFinish = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/OpeningRoadFinish.mat");
            var roadControl = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/RoadSurface.mat");
            var detailSeed = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/Environment/Finishes/ServiceCoating_BaseColor.png");
            var normalSeed = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/ShipSurfaces/Graphite_Normal.png");
            var glossSeed = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/Environment/Finishes/ServiceCoating_MetallicSmoothness.png");
            if (!roadFinish || !roadControl || !detailSeed || !normalSeed || !glossSeed)
                throw new InvalidOperationException("Opening road template, prepared control or authored detail seed is missing.");
            // Prepare's built-in normal/white defaults do not persist as asset references.
            // Preserve the baseline variant as well: runtime replaces these maps and emission
            // before drawing, so both modes retain the existing road response.
            roadControl.SetTexture("_BumpMap", normalSeed);
            roadControl.SetTexture("_MetallicGlossMap", glossSeed);
            roadControl.SetColor("_EmissionColor", Color.white);
            roadControl.DisableKeyword("_DETAIL_MULX2");
            roadControl.DisableKeyword("_DETAIL_SCALED");
            foreach (string keyword in new[] { "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_EMISSION", "_ENVIRONMENTREFLECTIONS_OFF" })
                roadControl.EnableKeyword(keyword);
            EditorUtility.SetDirty(roadControl);
            // Prepare establishes the working control feature combination, including normal,
            // smoothness-map and emission variants. Preserve it on this separate template.
            roadFinish.CopyPropertiesFromMaterial(roadControl);
            roadFinish.SetTexture("_DetailAlbedoMap", detailSeed);
            roadFinish.SetTexture("_BumpMap", normalSeed);
            roadFinish.SetTexture("_MetallicGlossMap", glossSeed);
            roadFinish.SetFloat("_DetailAlbedoMapScale", 1);
            roadFinish.SetFloat("_DetailNormalMapScale", .25f);
            // These persistent seeds survive asset validation. Runtime MakeMaterial/Maps
            // replaces their emission and textures before any regional renderer is created.
            roadFinish.SetColor("_EmissionColor", Color.white);
            roadFinish.DisableKeyword("_DETAIL_SCALED");
            foreach (string keyword in new[] { "_DETAIL_MULX2", "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_EMISSION", "_ENVIRONMENTREFLECTIONS_OFF" })
                roadFinish.EnableKeyword(keyword);
            EditorUtility.SetDirty(roadFinish);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (!pipeline || !renderer) throw new InvalidOperationException("Opening preview renderer settings are missing.");
            if (ReadDefaultRenderer(pipeline) != renderer)
                throw new InvalidOperationException("Opening preview requires VectorRenderer to remain the selected default renderer.");

            ScreenSpaceAmbientOcclusion ssao = null;
            foreach (var feature in renderer.rendererFeatures)
                if (feature is ScreenSpaceAmbientOcclusion found)
                {
                    if (ssao) throw new InvalidOperationException("Multiple SSAO features found; preserve a single known configuration before building.");
                    ssao = found;
                }
            if (!ssao) throw new InvalidOperationException("Existing SSAO feature is missing.");
            bool ssaoWasActive = ssao.isActive;

#if URP_SCREEN_SPACE_REFLECTION
            ScreenSpaceReflectionRendererFeature ssr = null;
            foreach (var feature in renderer.rendererFeatures)
                if (feature is ScreenSpaceReflectionRendererFeature found)
                {
                    if (ssr) throw new InvalidOperationException("Multiple SSR features found; resolve the preview registration before building.");
                    ssr = found;
                }
            if (!ssr) throw new InvalidOperationException("Installed SSR preview feature is missing.");
            ssr.SetActive(false);
            EditorUtility.SetDirty(ssr);
            string ssrState = "installed-active=false";
#else
            string ssrState = "compile-symbol-disabled";
#endif

            if (ReadDefaultRenderer(pipeline) != renderer || ssao.isActive != ssaoWasActive)
                throw new InvalidOperationException("Opening preview setup changed the default renderer or SSAO state.");
            EditorUtility.SetDirty(renderer);
            PlayerSettings.SetManagedCodeVariant(NamedBuildTarget.Standalone, ManagedCodeVariant.Instrumented);
            AssetDatabase.SaveAssets();
            foreach (string keyword in new[] { "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_EMISSION", "_ENVIRONMENTREFLECTIONS_OFF" })
                if (!roadControl.IsKeywordEnabled(keyword))
                    throw new InvalidOperationException("Opening baseline road variant was not retained after save: " + keyword);
            if (!roadControl.GetTexture("_BumpMap") || !roadControl.GetTexture("_MetallicGlossMap") ||
                roadControl.IsKeywordEnabled("_DETAIL_MULX2") || roadControl.IsKeywordEnabled("_DETAIL_SCALED"))
                throw new InvalidOperationException("Opening baseline road maps/keyword combination is incomplete after save.");
            Debug.Log("VR_OPENING_BASELINE_TEMPLATE retained=true requiredKeywordCount=4 detail=false");
            foreach (string keyword in new[] { "_DETAIL_MULX2", "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_EMISSION", "_ENVIRONMENTREFLECTIONS_OFF" })
                if (!roadFinish.IsKeywordEnabled(keyword))
                    throw new InvalidOperationException("Opening road required variant was not retained after save: " + keyword);
            if (!roadFinish.GetTexture("_DetailAlbedoMap") || !roadFinish.GetTexture("_BumpMap") ||
                !roadFinish.GetTexture("_MetallicGlossMap") || roadFinish.IsKeywordEnabled("_DETAIL_SCALED"))
                throw new InvalidOperationException("Opening road template maps/keyword combination is incomplete after save.");
            Debug.Log("VR_OPENING_DETAIL_TEMPLATE retained=true requiredKeywordCount=5 keyword=_DETAIL_MULX2 authoredDetailMap=" +
                roadFinish.GetTexture("_DetailAlbedoMap").name);

            string parent = Path.GetDirectoryName(output);
            if (string.IsNullOrEmpty(parent)) throw new InvalidOperationException("Opening preview output has no parent directory.");
            Directory.CreateDirectory(parent);
            Debug.Log("VR_OPENING_BUILD_CONFIGURATION scene=Solstice target=StandaloneOSX" +
                " managedCodeVariant=Instrumented buildOptions=None sameBinaryModes=off|construction|surface|combined" +
                " defaultMode=off defaultRendererPreserved=true ssaoActive=" + ssao.isActive +
                " ssr=" + ssrState + " output=" + output);

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Opening finish preview build failed: " +
                    (report == null ? "no BuildReport" : report.summary.result.ToString()));
            Debug.Log("VR_OPENING_BUILD_COMPLETE " + output);
        }

        static string ResolveOutput(string[] args)
        {
            int index = Array.IndexOf(args, "-vrOpeningPreviewOutput");
            string requested = "../Builds/Vector Rush-opening-01.app";
            if (index >= 0)
            {
                if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                    throw new ArgumentException("-vrOpeningPreviewOutput requires a separate output path.");
                requested = args[index + 1];
            }
            return Path.GetFullPath(requested);
        }

        static void RejectUnsafeOutput(string output)
        {
            string original = Path.GetFullPath("../Builds/Vector Rush.app");
            if (SamePath(output, original))
                throw new InvalidOperationException("Refusing to overwrite the original Vector Rush app: " + output);
            if (Directory.Exists(output) || File.Exists(output))
                throw new InvalidOperationException("Refusing to overwrite existing opening-preview evidence: " + output);
        }

        static bool SamePath(string left, string right)
        {
            char[] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            return string.Equals(left.TrimEnd(separators), right.TrimEnd(separators), StringComparison.OrdinalIgnoreCase);
        }

        static UniversalRendererData ReadDefaultRenderer(UniversalRenderPipelineAsset pipeline)
        {
            var settings = new SerializedObject(pipeline);
            var renderers = settings.FindProperty("m_RendererDataList");
            var index = settings.FindProperty("m_DefaultRendererIndex");
            if (renderers == null || index == null || index.intValue < 0 || index.intValue >= renderers.arraySize)
                throw new InvalidOperationException("VectorPipeline has no valid default renderer.");
            return renderers.GetArrayElementAtIndex(index.intValue).objectReferenceValue as UniversalRendererData;
        }
    }
}
