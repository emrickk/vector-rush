using System;
using UnityEngine;

namespace VectorRush
{
    // Staging contract. References point to prepared assets; runtime never recolors shared materials.
    [CreateAssetMenu(menuName = "Vector Rush/AAA/Bounded exemplar")]
    public sealed class AAAExemplar : ScriptableObject
    {
        public string revision;
        // Stable float-bit AAACourseIdentity of the TrackPath used to author every placement.
        public string courseHash;
        // SHA-256 of the exact course-data.json bytes checked during import. This is provenance,
        // not the runtime identity, because JSON float formatting can vary across Unity contexts.
        public string sourceCourseSha256;
        [Range(0, 1)] public float startProgress;
        [Range(0, 1)] public float endProgress;
        public bool fullSpanLayout;
        public bool atomicReplacement;
        public Placement[] placements = Array.Empty<Placement>();
        public MaterialBinding[] materials = Array.Empty<MaterialBinding>();
        public ReplacementTarget[] rendererReplacements = Array.Empty<ReplacementTarget>();
        public ReplacementTarget[] lightReplacements = Array.Empty<ReplacementTarget>();
        public LightFixture[] lights = Array.Empty<LightFixture>();

        [Serializable]
        public sealed class Placement
        {
            public string semanticGroup;
            public GameObject prefab;
            [Range(0, 1)] public float progress;
            public bool followBank;
            // Metres: X right, Y up, Z forward in the selected route frame.
            public Vector3 offset;
            // Explicit correction for the imported asset's orientation; never mirror by negative scale.
            public Vector3 rotationDegrees;
            [Min(.001f)] public float scale = 1;
            // Empty lists mean additive staging. Only fully contained renderers can be hidden.
            public Bounds replacementVolume;
            public string[] replaceRendererNames = Array.Empty<string>();
            public string[] replaceLightNames = Array.Empty<string>();
        }

        [Serializable]
        public sealed class MaterialBinding
        {
            public string sourceSlotName;
            public Material material;
        }

        [Serializable]
        public sealed class ReplacementTarget
        {
            public string name;
            [Min(1)] public int expectedCount = 1;
        }

        [Serializable]
        public sealed class LightFixture
        {
            public string id;
            public LightType type;
            public Vector3 worldPosition;
            public Vector3 worldDirection;
            public Color color = Color.white;
            public float intensity;
            public float range;
            public float outerAngle;
            public float innerAngle;
            public LightShadows shadows;
            public string housingSemanticGroup;
            public string socket;
        }
    }
}
