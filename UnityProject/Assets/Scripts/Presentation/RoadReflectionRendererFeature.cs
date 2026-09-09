#if URP_SCREEN_SPACE_REFLECTION
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    // Preview 02: the installed SSR pass requires a game-camera color target.
    // Keep reflection-probe captures on their original renderer path without SSR.
    public sealed class RoadReflectionRendererFeature : ScreenSpaceReflectionRendererFeature
    {
        readonly HashSet<CameraType> reportedCameraTypes = new HashSet<CameraType>();
        bool diagnostics;

        public override void Create()
        {
            base.Create();
            diagnostics = Array.IndexOf(Environment.GetCommandLineArgs(), "-vrSSRDiagnostics") >= 0;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraType = renderingData.cameraData.cameraType;
            bool eligible = cameraType == CameraType.Game;
            if (diagnostics && reportedCameraTypes.Add(cameraType))
                Debug.Log("VR_SSR_CAMERA_SCOPE type=" + cameraType + " action=" + (eligible ? "delegate" : "skip") +
                    " delegationDoesNotProveExecution=true");
            if (!eligible) return;
            base.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
#endif
