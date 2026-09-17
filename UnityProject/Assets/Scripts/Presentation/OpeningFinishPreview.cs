using System;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Command-line gates for the bounded opening-finish preview.</summary>
    public static class OpeningFinishPreview
    {
        const int ConstructionMask = 1;
        const int SurfaceMask = 2;

        // Parsing uses managed command-line data only. Unity objects are not created or
        // queried while this type and its fields are initialized.
        static readonly int enabledMask = ReadEnabledMask(Environment.GetCommandLineArgs());
        static bool configurationLogged;

        public static bool ConstructionEnabled => (enabledMask & ConstructionMask) != 0;
        public static bool SurfaceEnabled => (enabledMask & SurfaceMask) != 0;

        public static void LogConfiguration()
        {
            if (configurationLogged) return;
            configurationLogged = true;
            Debug.Log("VR_OPENING_FINISH_CONFIGURATION mode=" + SelectedMode() +
                " constructionEnabled=" + ConstructionEnabled +
                " surfaceEnabled=" + SurfaceEnabled +
                " cameraPhysicsUnchanged=true");
        }

        static int ReadEnabledMask(string[] args)
        {
            int index = Array.IndexOf(args, "-vrOpeningFinish");
            if (index < 0 || index + 1 >= args.Length) return 0;
            switch (args[index + 1])
            {
                case "construction": return ConstructionMask;
                case "surface": return SurfaceMask;
                case "combined": return ConstructionMask | SurfaceMask;
                case "off":
                default: return 0;
            }
        }

        static string SelectedMode()
        {
            switch (enabledMask)
            {
                case ConstructionMask: return "construction";
                case SurfaceMask: return "surface";
                case ConstructionMask | SurfaceMask: return "combined";
                default: return "off";
            }
        }
    }
}
