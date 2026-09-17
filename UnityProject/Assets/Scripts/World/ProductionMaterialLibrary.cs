using UnityEngine;

namespace VectorRush
{
    public sealed class ProductionMaterialLibrary : ScriptableObject
    {
        public bool stage1Finish;
        public Material craftSurfaceTemplate;
        public Material craftMetalFallback;
        public Material craftGlass;
        public Material craftEngineAccent;
        public Material craftEngineCore;
    }
}
