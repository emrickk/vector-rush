using System;

namespace VectorRush.Editor
{
    [Serializable] public sealed class ProductionArtManifest
    {
        public int contractVersion;
        public string revision;
        public string courseHash;
        public bool layoutComplete;
        public string sourceBlend;
        public ProductionAssetRecord[] assets;
        public ProductionMaterialRecord[] materials;
    }

    [Serializable] public sealed class ProductionAssetRecord
    {
        public string id;
        public string lod0;
        public string lod1;
        public string pivot;
        public float[] boundsMin;
        public float[] boundsMax;
        public string[] materialSlots;
        public bool trackAssembly;
        public string collider;
        public string lightmapUVs;
    }

    [Serializable] public sealed class ProductionMaterialRecord
    {
        public string id;
        public string baseColor;
        public string normal;
        public string metallicSmoothness;
        public string occlusion;
        public string emission;
        public float[] metersPerTile;
        public string uvMode;
        public float normalScale;
        public float[] emissionColorLinear;
        public float emissionIntensity;
    }

    [Serializable] public sealed class ProductionLayout
    {
        public int contractVersion;
        public string revision;
        public string courseHash;
        public ProductionZoneRecord[] zones;
        public ProductionInstanceRecord[] instances;
    }

    [Serializable] public sealed class ProductionZoneRecord
    {
        public string id;
        public float startProgress;
        public float endProgress;
    }

    [Serializable] public sealed class ProductionInstanceRecord
    {
        public string id;
        public string assetId;
        public string zoneId;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public string frameMode;
        public string role;
        public string gi;
    }

    [Serializable] public sealed class ProductionLighting
    {
        public int contractVersion;
        public string revision;
        public string courseHash;
        public ProductionEnvironmentRecord environment;
        public ProductionLightRecord[] lights;
        public ProductionReflectionVolumeRecord[] reflectionVolumes;
    }

    [Serializable] public sealed class ProductionEnvironmentRecord
    {
        public float[] fogColorLinear;
        public float fogDensity;
        public float exposureEV;
        public float[] ambientTintLinear;
        public string skyMaterialId;
    }

    [Serializable] public sealed class ProductionLightRecord
    {
        public string id;
        public string fixtureInstanceId;
        public string type;
        public float[] position;
        public float[] rotation;
        public float[] colorLinear;
        public float intensity;
        public float range;
        public float spotOuterDegrees;
        public float spotInnerDegrees;
        public string mode;
        public bool castsShadows;
    }

    [Serializable] public sealed class ProductionReflectionVolumeRecord
    {
        public string id;
        public float[] position;
        public float[] size;
        public int resolution;
        public float intensity;
        public bool boxProjection;
    }

    [Serializable] public sealed class ProductionTrackProfile
    {
        public int contractVersion;
        public string courseHash;
        public ProductionTrackStripRecord[] strips;
    }

    [Serializable] public sealed class ProductionTrackStripRecord
    {
        public string id;
        public string materialId;
        public float[] pointsXY;
        public string surfaceRole;
        public float uvMetersPerTile;
        public bool twoSided;
    }

    [Serializable] public sealed class ProductionChecksums
    {
        public int contractVersion;
        public string revision;
        public ProductionChecksumRecord[] files;
    }

    [Serializable] public sealed class ProductionChecksumRecord
    {
        public string path;
        public string sha256;
    }

    [Serializable] public sealed class ProductionReady
    {
        public int contractVersion;
        public string revision;
        public string courseHash;
        public string checksumsSha256;
    }
}
