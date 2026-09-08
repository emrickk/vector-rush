using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Two original imported landmarks, with complete site reservations.</summary>
    public sealed class NightLandmarks : MonoBehaviour
    {
        struct Site
        {
            public string Resource;
            public Vector3 Center;
            public Quaternion Rotation;
            public Vector2 HalfSize;
        }

        readonly List<Site> sites = new List<Site>(2);
        readonly Dictionary<string, Material> finishes = new Dictionary<string, Material>();
        bool built;

        public void Build(WorldBuilder world, TrackPath track)
        {
            if (built) return;
            built = true;
            track.Ensure();
            // Reserve the entire authored podium, canopy and pipe envelope before
            // loading art, so a missing resource cannot admit a competing city block.
            Reserve(track, "Nocturne_SplitSignal_Mast", .230f, 56f, new Vector2(27, 33));
            Reserve(track, "Nocturne_ThermalExchange_Works", .625f, -51f, new Vector2(23, 43));
            finishes["Ceramic"] = world.MakeMaterial("Landmarks / broad ceramic cladding", new Color(.52f, .53f, .50f), .42f, .04f);
            finishes["Concrete"] = world.MakeMaterial("Landmarks / cast service concrete", new Color(.34f, .355f, .365f), .25f, .03f);
            finishes["Structure"] = world.MakeMaterial("Landmarks / recessed structure", new Color(.075f, .09f, .10f), .42f, .3f);
            finishes["Titanium"] = world.MakeMaterial("Landmarks / satin titanium", new Color(.34f, .40f, .42f), .58f, .66f);
            finishes["Oxide"] = world.MakeMaterial("Landmarks / insulated oxide mains", new Color(.31f, .195f, .11f), .34f, .38f);
            finishes["Glass"] = world.MakeMaterial("Landmarks / deep smoked glazing", new Color(.07f, .11f, .14f), .84f, .40f);
            finishes["Occupied"] = world.MakeMaterial("Landmarks / limited occupied rooms", new Color(.24f, .20f, .15f), .70f, .16f, new Color(.20f, .135f, .08f));
            finishes["Lamp"] = world.MakeMaterial("Landmarks / concealed service lamps", new Color(.45f, .35f, .24f), .4f, .1f, new Color(.75f, .47f, .21f));
            foreach (var finish in finishes)
            {
                bool textured = NightArchitectureFinishes.Apply(finish.Value, finish.Key);
                // Mineral service faces spread their local wash more broadly than the cladding.
                if (textured && finish.Key == "Concrete") finish.Value.SetFloat("_Smoothness", .78f);
            }
            for (int i = 0; i < sites.Count; i++)
            {
                var site = sites[i];
                if (!ClearRoad(track, site))
                {
                    Debug.LogError("Landmark site failed whole-road clearance: " + site.Resource, this);
                    continue;
                }
                var prefab = Resources.Load<GameObject>("Art/Environment/Landmarks/" + site.Resource);
                if (!prefab)
                {
                    Debug.LogError("Missing authored landmark: " + site.Resource, this);
                    continue;
                }
                var landmark = Instantiate(prefab, site.Center, site.Rotation, transform);
                landmark.name = "Landmark / " + site.Resource;
                landmark.transform.localScale = Vector3.one;
                foreach (var renderer in landmark.GetComponentsInChildren<Renderer>())
                {
                    var materials = renderer.sharedMaterials;
                    for (int slot = 0; slot < materials.Length; slot++)
                    {
                        string sourceName = materials[slot] ? materials[slot].name : "";
                        bool matched = false;
                        foreach (var finish in finishes)
                        {
                            if (!sourceName.StartsWith(finish.Key, StringComparison.Ordinal)) continue;
                            materials[slot] = finish.Value;
                            matched = true;
                            break;
                        }
                        if (!matched) Debug.LogError("Unmapped landmark material: " + sourceName, renderer);
                    }
                    renderer.sharedMaterials = materials;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
                // Broad upper modeling and shorter local fills reveal actual structural returns.
                if (i == 0)
                {
                    Wash(site, "Signal / lower structural wash", new Vector3(-22, 48, -29), new Vector3(9, 77, -9), 1500, 96, 78, new Color(.70f, .83f, 1f));
                    Wash(site, "Signal / skyroom return wash", new Vector3(20, 57, 12), new Vector3(10, 89, -10), 1100, 88, 74, new Color(1f, .81f, .58f));
                    Wash(site, "Signal / bracing and soffit fill", new Vector3(-29, 76, -23), new Vector3(8, 79, -8), 650, 56, 72, new Color(.94f, .82f, .66f), true);
                }
                else
                {
                    Wash(site, "Thermal / drum and gallery wash", new Vector3(20, 53, -35), new Vector3(-7, 60, -5), 1650, 98, 86, new Color(.76f, .86f, 1f));
                    Wash(site, "Thermal / pipe supports and service return", new Vector3(29, 40, 31), new Vector3(13, 35, 5), 800, 60, 86, new Color(.95f, .82f, .63f), true);
                    // The early approach sees the end portals (local -Z), before the long service side.
                    Wash(site, "Thermal / end portal wash", new Vector3(-7, 32, -54), new Vector3(2, 16, -39), 450, 42, 86, new Color(.80f, .87f, .94f), true);
                }
                Debug.Log("VECTOR_RUSH_LANDMARK resource=" + site.Resource + " center=" + site.Center + " halfFootprint=" + site.HalfSize, this);
            }
        }

        void Reserve(TrackPath track, string resource, float progress, float side, Vector2 halfSize)
        {
            var frame = track.Evaluate(progress);
            var rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(frame.Forward, Vector3.up).normalized, Vector3.up);
            Vector3 center = frame.Position + rotation * Vector3.right * side;
            center.y = 0;
            sites.Add(new Site { Resource = resource, Center = center, Rotation = rotation, HalfSize = halfSize });
        }

        static bool ClearRoad(TrackPath track, Site site)
        {
            const int samples = 1200;
            float margin = 18f + track.Length / samples;
            var inverse = Quaternion.Inverse(site.Rotation);
            for (int i = 0; i < samples; i++)
            {
                Vector3 p = inverse * (track.Evaluate((float)i / samples).Position - site.Center);
                float x = Mathf.Max(0, Mathf.Abs(p.x) - site.HalfSize.x);
                float z = Mathf.Max(0, Mathf.Abs(p.z) - site.HalfSize.y);
                if (x * x + z * z < margin * margin) return false;
            }
            return true;
        }

        public bool Overlaps(Vector3 center, Quaternion rotation, Vector2 halfSize)
        {
            Vector3 right = rotation * Vector3.right, forward = rotation * Vector3.forward;
            foreach (var site in sites)
            {
                Vector3 siteRight = site.Rotation * Vector3.right, siteForward = site.Rotation * Vector3.forward;
                Vector3 delta = center - site.Center;
                delta.y = 0;
                if (Separated(delta, right, right, forward, halfSize, siteRight, siteForward, site.HalfSize) ||
                    Separated(delta, forward, right, forward, halfSize, siteRight, siteForward, site.HalfSize) ||
                    Separated(delta, siteRight, right, forward, halfSize, siteRight, siteForward, site.HalfSize) ||
                    Separated(delta, siteForward, right, forward, halfSize, siteRight, siteForward, site.HalfSize)) continue;
                return true;
            }
            return false;
        }

        static bool Separated(Vector3 delta, Vector3 axis, Vector3 right, Vector3 forward, Vector2 halfSize,
            Vector3 otherRight, Vector3 otherForward, Vector2 otherHalfSize)
        {
            float extent = Mathf.Abs(Vector3.Dot(axis, right)) * halfSize.x + Mathf.Abs(Vector3.Dot(axis, forward)) * halfSize.y +
                Mathf.Abs(Vector3.Dot(axis, otherRight)) * otherHalfSize.x + Mathf.Abs(Vector3.Dot(axis, otherForward)) * otherHalfSize.y;
            return Mathf.Abs(Vector3.Dot(delta, axis)) > extent + 3f;
        }

        void Wash(Site site, string name, Vector3 localPosition, Vector3 localTarget, float intensity, float range, float angle, Color color, bool excludeRacers = false)
        {
            var housing = new GameObject(name);
            housing.transform.SetParent(transform, false);
            housing.transform.position = site.Center + site.Rotation * localPosition;
            housing.transform.rotation = Quaternion.LookRotation(site.Rotation * (localTarget - localPosition), Vector3.up);
            var light = housing.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.innerSpotAngle = angle * .63f;
            light.shadows = LightShadows.None;
            if (excludeRacers) light.cullingMask = ~(1 << 8);
            light.renderMode = LightRenderMode.Auto;
        }
    }
}
