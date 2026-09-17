using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    public sealed class ProductionWorld : MonoBehaviour
    {
        public TrackPath track;
        public ProductionMaterialLibrary materials;
        public string courseHash;
        public string artRevision;
        public bool layoutComplete;

        public void ValidateReady()
        {
            var errors = new List<string>();
            if (!track) errors.Add("track reference is missing");
            if (!materials) errors.Add("production material library is missing");
            else
            {
                if (!materials.craftSurfaceTemplate) errors.Add("craft surface template is missing");
                if (!materials.craftMetalFallback) errors.Add("craft metal fallback is missing");
                if (!materials.craftGlass) errors.Add("craft glass is missing");
                if (!materials.craftEngineAccent) errors.Add("craft engine accent is missing");
                if (!materials.craftEngineCore) errors.Add("craft engine core is missing");
            }
            if (string.IsNullOrWhiteSpace(courseHash) || courseHash.Length != 64 || !IsHex(courseHash))
                errors.Add("courseHash must be a 64-character SHA-256 value");
            if (string.IsNullOrWhiteSpace(artRevision)) errors.Add("artRevision is missing");
            if (track && (!Approximately(track.transform.position, Vector3.zero) ||
                          !Approximately(track.transform.rotation, Quaternion.identity) ||
                          !Approximately(track.transform.lossyScale, Vector3.one)))
                errors.Add("track transform must remain at identity");
            if (errors.Count > 0) throw new InvalidOperationException("Production world is not ready: " + string.Join("; ", errors));
            track.Ensure();
        }

        static bool IsHex(string value)
        {
            for (int i = 0; i < value.Length; i++)
                if (!Uri.IsHexDigit(value[i])) return false;
            return true;
        }

        static bool Approximately(Vector3 a, Vector3 b) => (a - b).sqrMagnitude <= 1e-8f;
        static bool Approximately(Quaternion a, Quaternion b) => Mathf.Abs(Quaternion.Dot(a, b)) >= .999999f;
    }
}
