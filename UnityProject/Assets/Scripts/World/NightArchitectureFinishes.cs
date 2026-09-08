using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Shared, normal-map-free finishes for the landmarks' four-metre UV0.</summary>
    public static class NightArchitectureFinishes
    {
        const string Root = "Art/Environment/Finishes/";
        struct Maps
        {
            public Texture2D Color, MetallicSmoothness;
        }
        static readonly Dictionary<string, Maps> cache = new Dictionary<string, Maps>();
        static Material template;
        static bool checkedTemplate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCache()
        {
            cache.Clear();
            template = null;
            checkedTemplate = false;
        }

        /// <summary>
        /// Call once during material construction. Keeps the caller's base color,
        /// emission and material ownership. Unsupported finish keys are unchanged.
        /// </summary>
        public static bool Apply(Material material, string finishKey)
        {
            if (!material) return false;
            string family;
            float fallbackMetallic;
            switch (finishKey)
            {
                case "Ceramic": family = "WarmCeramic"; fallbackMetallic = .035f; break;
                case "Concrete": family = "CastConcrete"; fallbackMetallic = .025f; break;
                case "Titanium": family = "SatinTitanium"; fallbackMetallic = .665f; break;
                case "Structure":
                case "Oxide": family = "ServiceCoating"; fallbackMetallic = .335f; break;
                default: return false;
            }
            if (!checkedTemplate)
            {
                checkedTemplate = true;
                template = Resources.Load<Material>(Root + "ArchitectureFinishLit");
                if (!template || !template.IsKeywordEnabled("_METALLICSPECGLOSSMAP"))
                    Debug.LogError("Architecture finish template or retained metallic-map variant is missing.");
            }
            if (!template || material.shader != template.shader ||
                !template.IsKeywordEnabled("_METALLICSPECGLOSSMAP")) return false;
            // WorldBuilder's SurfaceLit materials already enable emission, even
            // when its color is black. Keep that state and use the saved variant.
            if (!material.IsKeywordEnabled("_EMISSION"))
            {
                Debug.LogError("Architecture finishes require a WorldBuilder SurfaceLit material with the retained emission variant.");
                return false;
            }
            if (!cache.TryGetValue(family, out var maps))
            {
                maps = new Maps
                {
                    Color = Resources.Load<Texture2D>(Root + family + "_BaseColor"),
                    MetallicSmoothness = Resources.Load<Texture2D>(Root + family + "_MetallicSmoothness")
                };
                cache.Add(family, maps);
                if (!maps.Color || !maps.MetallicSmoothness)
                    Debug.LogError("Missing architecture finish texture pair: " + family);
            }
            if (!maps.Color || !maps.MetallicSmoothness) return false;

            material.SetTexture("_BaseMap", maps.Color);
            material.SetTextureScale("_BaseMap", Vector2.one);
            material.SetTextureOffset("_BaseMap", Vector2.zero);
            material.SetTexture("_MetallicGlossMap", maps.MetallicSmoothness);
            material.SetTextureScale("_MetallicGlossMap", Vector2.one);
            material.SetTextureOffset("_MetallicGlossMap", Vector2.zero);
            material.SetFloat("_WorkflowMode", 1f);
            material.SetFloat("_Metallic", fallbackMetallic);
            // URP takes metallic directly from R, and multiplies packed alpha by
            // _Smoothness. Alpha already contains the authored absolute finish.
            material.SetFloat("_Smoothness", 1f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.DisableKeyword("_SPECULAR_SETUP");
            material.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            material.DisableKeyword("_NORMALMAP");
            material.SetTexture("_BumpMap", null);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            return true;
        }
    }
}
