using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace VectorRush
{
    [DefaultExecutionOrder(-10000)]
    public sealed class ProductionRenderConfiguration : MonoBehaviour
    {
        public RenderPipelineAsset renderPipeline;

        void Awake()
        {
            if (!renderPipeline) return;
            // Evidence-only grading comparison. Never mutates a persistent asset.
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-openingEvidence") >= 0 &&
                Array.IndexOf(args, "-neonLdrGrade") >= 0 && renderPipeline is UniversalRenderPipelineAsset source)
            {
                var comparison = Instantiate(source);
                comparison.colorGradingMode = ColorGradingMode.LowDynamicRange;
                renderPipeline = comparison;
                Debug.Log("NEON_GRADING_COMPARISON LDR; all scene lighting preserved");
            }
            GraphicsSettings.defaultRenderPipeline = renderPipeline;
            QualitySettings.renderPipeline = renderPipeline;
            foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (camera.gameObject.scene == gameObject.scene)
                    camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        }
    }
}
