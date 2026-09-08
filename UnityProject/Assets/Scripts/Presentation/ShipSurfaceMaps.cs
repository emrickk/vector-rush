using UnityEngine;

namespace VectorRush
{
    // Candidate ship surface payloads live separately from environment materials.
    // Call only for ship materials; the world's shared metal must remain independent.
    public static class ShipSurfaceMaps
    {
        const string Folder="Art/ShipSurfaces/";
        public static bool HasSurface(string key)=>Resources.Load<Texture2D>(Folder+key+"_BaseColor");

        public static Material Create(WorldBuilder world,string name,string key,Color tint,float smoothness,float metallic)
        {
            var color=Resources.Load<Texture2D>(Folder+key+"_BaseColor");
            var normal=Resources.Load<Texture2D>(Folder+key+"_Normal");
            var mask=Resources.Load<Texture2D>(Folder+key+"_MetallicSmoothness");
            var occlusion=Resources.Load<Texture2D>(Folder+key+"_Occlusion");
            bool complete=color&&normal&&mask&&occlusion;
            if(color&&!complete)Debug.LogError("Incomplete ship surface payload: "+key);
            var material=world.MakeMaterial(name,tint,smoothness,metallic,templateName:complete?"CraftSurfaceLit":"SurfaceLit");
            if(!complete)return material;
            material.SetTexture("_BaseMap",color);
            material.SetTexture("_BumpMap",normal);material.SetFloat("_BumpScale",1);material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap",mask);material.SetFloat("_Smoothness",1);material.SetFloat("_SmoothnessTextureChannel",0);material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetTexture("_OcclusionMap",occlusion);material.SetFloat("_OcclusionStrength",.6f);material.EnableKeyword("_OCCLUSIONMAP");
            material.SetTextureScale("_BaseMap",Vector2.one);
            return material;
        }
    }
}
