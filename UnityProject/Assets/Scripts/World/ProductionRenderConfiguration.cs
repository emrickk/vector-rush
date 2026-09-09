using UnityEngine;
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
            GraphicsSettings.defaultRenderPipeline = renderPipeline;
            QualitySettings.renderPipeline = renderPipeline;
        }
    }
}
