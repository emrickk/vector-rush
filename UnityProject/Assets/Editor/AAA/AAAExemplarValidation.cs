using System;
using UnityEditor;
using UnityEngine;

namespace VectorRush.Editor
{
    /// <summary>Read-only preflight. Does not call Prepare, save scenes or rewrite imported assets.</summary>
    public static class AAAExemplarValidation
    {
        [MenuItem("Vector Rush/AAA/Validate staged exemplars (read only)")]
        public static void ValidateStaging() => ValidateStaging(false);

        public static void ValidateRequiredStaging() => ValidateStaging(true);

        public static void ValidateRequiredGalleryStaging() => ValidateStaging(false, true);

        public static void RequirePublishedResources(AAAExemplar opening, AAAExemplar gallery)
        {
            if (!opening) throw new InvalidOperationException("Required published resource missing: " + AAABaselineIntegration.OpeningResource);
            if (!gallery) throw new InvalidOperationException("Required published resource missing: " + AAABaselineIntegration.GalleryResource);
        }

        public static void RequireGalleryResource(AAAExemplar gallery)
        {
            if (!gallery) throw new InvalidOperationException("Required published resource missing: " + AAABaselineIntegration.GalleryResource);
        }

        static void ValidateStaging(bool requireBoth, bool requireGallery = false)
        {
            string[] paths = { AAABaselineIntegration.OpeningResource, AAABaselineIntegration.GalleryResource };
            AAAExemplar[] payloads = { Resources.Load<AAAExemplar>(paths[0]), Resources.Load<AAAExemplar>(paths[1]) };
            if (requireBoth) RequirePublishedResources(payloads[0], payloads[1]);
            else if (requireGallery) RequireGalleryResource(payloads[1]);
            var courseObject = new GameObject("AAA preflight course identity") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var track = courseObject.AddComponent<TrackPath>();
                for (int resourceIndex = 0; resourceIndex < paths.Length; resourceIndex++)
                {
                    string path = paths[resourceIndex];
                    AAAExemplar payload = payloads[resourceIndex];
                    if (!payload) { Debug.Log("VR_AAA_PREFLIGHT absent optional resource=" + path); continue; }
                    string error = AAABaselineIntegration.ValidateForTrack(payload, track);
                    if (error != null) throw new InvalidOperationException(path + ": " + error);
                    foreach (var binding in payload.materials) ValidateMaterialAssets(binding.material);
                    foreach (var placement in payload.placements)
                    {
                        if (!AssetDatabase.Contains(placement.prefab)) throw new InvalidOperationException("Prefab must be persistent: " + placement.semanticGroup);
                        foreach (var filter in placement.prefab.GetComponentsInChildren<MeshFilter>(true))
                        {
                            var mesh = filter.sharedMesh;
                            if (!AssetDatabase.Contains(mesh)) throw new InvalidOperationException("Nonpersistent mesh: " + filter.name);
                            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal) ||
                                !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0))
                                throw new InvalidOperationException("Normals and UV0 required: " + mesh.name);
                        }
                        foreach (var renderer in placement.prefab.GetComponentsInChildren<MeshRenderer>(true))
                            foreach (var source in renderer.sharedMaterials)
                            {
                                var material = source;
                                foreach (var binding in payload.materials)
                                    if (source && source.name == binding.sourceSlotName) material = binding.material;
                                ValidateMaterialAssets(material);
                                if (material.GetTexture("_BumpMap"))
                                {
                                    var filter = renderer.GetComponent<MeshFilter>();
                                    if (!filter || !filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
                                        throw new InvalidOperationException("Normal-mapped renderer requires tangents: " + renderer.name);
                                }
                            }
                    }
                    Debug.Log("VR_AAA_PREFLIGHT source contracts passed revision=" + payload.revision +
                        " fixtures=" + payload.lights.Length + "; native shader variants, placement and appearance UNVERIFIED");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(courseObject); }
        }

        static void ValidateMaterialAssets(Material material)
        {
            if (!AssetDatabase.Contains(material)) throw new InvalidOperationException("Prepared material must be persistent");
            foreach (string property in material.GetTexturePropertyNames())
            {
                var texture = material.GetTexture(property);
                if (!texture) continue;
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                if (!importer) throw new InvalidOperationException(material.name + " requires imported texture: " + property);
                if (!importer.mipmapEnabled) throw new InvalidOperationException(material.name + " missing mipmaps: " + property);
                if (property == "_BumpMap" || property == "_DetailNormalMap")
                {
                    if (importer.textureType != TextureImporterType.NormalMap) throw new InvalidOperationException(material.name + " wrong normal import: " + property);
                }
                else if (property == "_BaseMap" || property == "_EmissionMap" || property == "_DetailAlbedoMap")
                {
                    if (!importer.sRGBTexture) throw new InvalidOperationException(material.name + " color texture must use sRGB: " + property);
                }
                else if (property == "_MetallicGlossMap" || property == "_OcclusionMap" || property == "_ParallaxMap" || property == "_DetailMask")
                {
                    if (importer.sRGBTexture) throw new InvalidOperationException(material.name + " data texture must be linear: " + property);
                }
            }
        }
    }
}
