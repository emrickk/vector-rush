using UnityEngine;

namespace VectorRush
{
    /// <summary>
    /// Binds the authoritative propulsion envelope to artist-authored hull shaders.
    /// The controller intentionally supplies only normalized state. Astra-owned
    /// materials decide color, outline shape, breakup, and emission response.
    /// </summary>
    public sealed class ShipBoostVisuals : MonoBehaviour
    {
        static readonly int ThrustId = Shader.PropertyToID("_VRThrust");
        static readonly int BoostId = Shader.PropertyToID("_VRBoost");
        static readonly int IgnitionId = Shader.PropertyToID("_VRBoostIgnition");
        static readonly int ReleaseId = Shader.PropertyToID("_VRBoostRelease");

        MaterialPropertyBlock properties;
        Renderer[] hullRenderers = System.Array.Empty<Renderer>();
        readonly System.Collections.Generic.List<(Renderer renderer, int slot)> energySlots = new();
        bool initialized;

        public float Boost01 { get; private set; }
        public float Ignition01 { get; private set; }
        public float Release01 { get; private set; }

        void Awake() { properties = new MaterialPropertyBlock(); }

        public void Initialize(HoverVehicle vehicle)
        {
            if (initialized || !vehicle) return;
            properties ??= new MaterialPropertyBlock();
            Transform visual = vehicle.VisualRoot ? vehicle.VisualRoot : vehicle.transform;
            hullRenderers = visual.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in hullRenderers)
            {
                var materials = renderer.sharedMaterials;
                for (int slot = 0; slot < materials.Length; slot++)
                {
                    var material = materials[slot];
                    var map = material && material.HasProperty("_EmissionMap") ? material.GetTexture("_EmissionMap") : null;
                    if (map && map.name.StartsWith("Stage1Energy_")) energySlots.Add((renderer, slot));
                }
            }
            initialized = true;
            ResetState();
        }

        public void Step(float thrust, float boost, float ignition, float release)
        {
            Boost01 = Mathf.Clamp01(boost);
            Ignition01 = Mathf.Clamp01(ignition);
            Release01 = Mathf.Clamp01(release);
            Apply(Mathf.Clamp01(thrust));
        }

        public void ResetState()
        {
            Boost01 = Ignition01 = Release01 = 0f;
            Apply(0f);
        }

        void Apply(float thrust)
        {
            for (int i = 0; i < hullRenderers.Length; i++)
            {
                Renderer renderer = hullRenderers[i];
                if (!renderer) continue;
                renderer.GetPropertyBlock(properties);
                properties.SetFloat(ThrustId, thrust);
                properties.SetFloat(BoostId, Boost01);
                properties.SetFloat(IgnitionId, Ignition01);
                properties.SetFloat(ReleaseId, Release01);
                renderer.SetPropertyBlock(properties);
                properties.Clear();
            }
            foreach (var entry in energySlots)
            {
                if (!entry.renderer) continue;
                entry.renderer.GetPropertyBlock(properties, entry.slot);
                properties.SetColor("_EmissionColor", new Color(.035f, .72f, 1.2f) *
                    (Boost01 * 3.4f + Ignition01 * .9f));
                properties.SetFloat(BoostId, Boost01);
                entry.renderer.SetPropertyBlock(properties, entry.slot);
                properties.Clear();
            }
        }

        void OnDisable() { ResetState(); }
    }
}
