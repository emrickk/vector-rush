using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    public sealed class CraftMaterialFactory : IDisposable
    {
        const string SurfaceFolder = "Art/ShipSurfaces/";
        readonly ProductionMaterialLibrary library;
        readonly List<Material> ownedMaterials = new List<Material>();
        bool disposed;

        public Material MetalFallback => library.craftMetalFallback;
        public Material Glass => library.craftGlass;
        public Material EngineAccent => library.craftEngineAccent;
        public Material EngineCore => library.craftEngineCore;

        public CraftMaterialFactory(ProductionMaterialLibrary library)
        {
            this.library = library ? library : throw new ArgumentNullException(nameof(library));
        }

        public Material Create(string name, Color tint, float smoothness = .5f, float metallic = 0f, Color? emission = null)
        {
            ThrowIfDisposed();
            if (!library.craftSurfaceTemplate) throw new InvalidOperationException("Production craft surface template is missing");
            var material = new Material(library.craftSurfaceTemplate) { name = name };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            material.color = tint;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission ?? Color.black);
            }
            ownedMaterials.Add(material);
            return material;
        }

        public Material CreateMapped(string name, string key, Color tint, float smoothness, float metallic)
        {
            ThrowIfDisposed();
            var color = Resources.Load<Texture2D>(SurfaceFolder + key + "_BaseColor");
            var normal = Resources.Load<Texture2D>(SurfaceFolder + key + "_Normal");
            var mask = Resources.Load<Texture2D>(SurfaceFolder + key + "_MetallicSmoothness");
            var occlusion = Resources.Load<Texture2D>(SurfaceFolder + key + "_Occlusion");
            bool complete = color && normal && mask && occlusion;
            if (color && !complete) Debug.LogError("Incomplete ship surface payload: " + key);
            var material = Create(name, tint, smoothness, metallic);
            if (!complete) return material;
            material.SetTexture("_BaseMap", color);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1f);
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetFloat("_Smoothness", 1f);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetTexture("_OcclusionMap", occlusion);
            material.SetFloat("_OcclusionStrength", .6f);
            material.EnableKeyword("_OCCLUSIONMAP");
            material.SetTextureScale("_BaseMap", Vector2.one);
            return material;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var material in ownedMaterials)
            {
                if (!material) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(material);
                else UnityEngine.Object.DestroyImmediate(material);
            }
            ownedMaterials.Clear();
        }

        void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(CraftMaterialFactory));
        }
    }
}
