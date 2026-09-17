using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Explicit native staging only. Does not change baseline lighting, physics or cameras.</summary>
    public sealed class AAABaselineIntegration : MonoBehaviour
    {
        public const string Flag = "-vrAAAExemplar";
        public const string OpeningResource = "AAA/OpeningExemplar";
        public const string GalleryResource = "AAA/GalleryExemplar";
        readonly Dictionary<Renderer, bool> hiddenRenderers = new Dictionary<Renderer, bool>();
        readonly Dictionary<Light, bool> hiddenLights = new Dictionary<Light, bool>();
        GameObject staging;
        public const float WarmGalleryStart = .86f;
        public const float WarmGalleryEnd = .902f;

        // Parent calls this once, immediately after the legacy world.Build(Track).
        public static AAABaselineIntegration TryAttach(WorldBuilder world, TrackPath track)
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, Flag);
            if (index < 0 || index + 1 >= args.Length || args[index + 1] == "off") return null;
            string resource = args[index + 1] == "opening" ? OpeningResource :
                args[index + 1] == "gallery" ? GalleryResource : null;
            if (resource == null) { Debug.LogWarning("VR_AAA_EXEMPLAR unknown mode; baseline retained"); return null; }
            if (!world || !track) { Debug.LogWarning("VR_AAA_EXEMPLAR requires the built legacy world"); return null; }
            if (OpeningFinishPreview.ConstructionEnabled || OpeningFinishPreview.SurfaceEnabled)
            { Debug.LogWarning("VR_AAA_EXEMPLAR disable older opening preview flags first; baseline untouched by AAA hook"); return null; }
            var existing = world.GetComponent<AAABaselineIntegration>();
            if (existing) return existing;
            var payload = Resources.Load<AAAExemplar>(resource);
            if (!payload) { Debug.LogWarning("VR_AAA_EXEMPLAR missing " + resource + "; baseline retained"); return null; }
            string error = ValidateForTrack(payload, track);
            if (error != null) { Debug.LogWarning("VR_AAA_EXEMPLAR rejected: " + error); return null; }
            var hook = world.gameObject.AddComponent<AAABaselineIntegration>();
            try { hook.Apply(world, track, payload); }
            catch (Exception exception)
            {
                hook.RestoreBaseline();
                Destroy(hook);
                Debug.LogWarning("VR_AAA_EXEMPLAR staging failed; baseline restored: " + exception.Message);
                return null;
            }
            return hook;
        }

        public static string Validate(AAAExemplar payload)
        {
            if (!payload || string.IsNullOrWhiteSpace(payload.revision)) return "missing revision";
            if (!AAACourseIdentity.IsCanonical(payload.courseHash)) return "missing or invalid course identity";
            if (!Finite(payload.startProgress) || !Finite(payload.endProgress) || payload.startProgress < 0 ||
                payload.endProgress > 1 || payload.endProgress <= payload.startProgress ||
                payload.endProgress - payload.startProgress > .15f) return "require one non-wrapping interval of at most 15% of the lap";
            if (payload.placements == null || payload.placements.Length == 0) return "empty placements";
            if (payload.rendererReplacements == null || payload.lightReplacements == null || payload.lights == null)
                return "null replacement or lighting arrays";
            var bindings = new HashSet<string>(StringComparer.Ordinal);
            if (payload.materials == null) return "null material bindings";
            foreach (var binding in payload.materials)
            {
                if (binding == null || string.IsNullOrEmpty(binding.sourceSlotName) || !bindings.Add(binding.sourceSlotName)) return "invalid or duplicate slot binding";
                string issue = ValidateMaterial(binding.material);
                if (issue != null) return issue;
            }
            foreach (var p in payload.placements)
            {
                if (p == null || !p.prefab || string.IsNullOrWhiteSpace(p.semanticGroup)) return "missing prefab or semantic group";
                if (!Finite(p.progress) || p.progress < payload.startProgress || p.progress > payload.endProgress ||
                    !Finite(p.offset) || !Finite(p.rotationDegrees) || !Finite(p.scale) || p.scale <= 0) return "invalid placement transform or progress";
                if (!Finite(p.replacementVolume.center) || !Finite(p.replacementVolume.size) ||
                    p.replacementVolume.size.x < 0 || p.replacementVolume.size.y < 0 || p.replacementVolume.size.z < 0) return "invalid replacement volume";
                if (!p.prefab.activeSelf) return "prefab root must be active";
                if (payload.atomicReplacement && ((p.replaceRendererNames != null && p.replaceRendererNames.Length > 0) ||
                    (p.replaceLightNames != null && p.replaceLightNames.Length > 0) || p.replacementVolume.size != Vector3.zero))
                    return "atomic replacement prohibits per-placement replacement volumes and names";
                bool hasMesh = false;
                foreach (var component in p.prefab.GetComponentsInChildren<Component>(true))
                {
                    // A visual-only whitelist rejects scripts before Instantiate could execute them.
                    if (!component) return "missing component in prefab";
                    if (!(component is Transform || component is MeshFilter || component is MeshRenderer || component is LODGroup || component is Light))
                        return "unsupported prefab component " + component.GetType().Name;
                    if (component is Transform t && (t.localScale.x <= 0 || t.localScale.y <= 0 || t.localScale.z <= 0 ||
                        !Finite(t.localScale) || !Finite(t.localPosition) || !Finite(t.localEulerAngles))) return "invalid source transform";
                    if (component is Light light && (payload.atomicReplacement || light.type == LightType.Directional))
                        return payload.atomicReplacement ? "atomic gallery uses payload lights, not embedded prefab lights" : "global directional light prohibited";
                    if (component is MeshFilter filter)
                    {
                        if (!filter.sharedMesh || !Finite(filter.sharedMesh.bounds.center) || !Finite(filter.sharedMesh.bounds.size)) return "missing or invalid mesh bounds";
                        hasMesh = true;
                    }
                    if (component is MeshRenderer renderer)
                    {
                        var meshFilter = renderer.GetComponent<MeshFilter>();
                        if (!meshFilter || !meshFilter.sharedMesh || renderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
                            return "renderer material slots must match mesh submeshes";
                        foreach (var slot in renderer.sharedMaterials)
                        {
                            var material = ResolveMaterial(payload, slot);
                            string issue = ValidateMaterial(material);
                            if (issue != null) return issue;
                        }
                    }
                }
                if (!hasMesh) return "placement requires authored geometry";
            }
            string replacementIssue = ValidateReplacementContract(payload);
            if (replacementIssue != null) return replacementIssue;
            string lightIssue = ValidateLights(payload);
            if (lightIssue != null) return lightIssue;
            return null;
        }

        public static string ValidateForTrack(AAAExemplar payload, TrackPath track)
        {
            if (!payload) return "missing payload";
            if (!track) return "missing runtime track";
            if (!AAACourseIdentity.IsCanonical(payload.courseHash)) return "missing or invalid course identity";
            string runtimeHash = AAACourseIdentity.Compute(track);
            if (!string.Equals(payload.courseHash, runtimeHash, StringComparison.Ordinal))
                return "course identity mismatch payload=" + payload.courseHash + " runtime=" + runtimeHash;
            string issue = Validate(payload);
            if (issue != null) return issue;
            foreach (var fixture in payload.lights)
                if (!GalleryEnvelopeContains(track, fixture.worldPosition)) return "fixture outside warm gallery envelope: " + fixture.id;
            return null;
        }

        public static string ValidateMaterial(Material material)
        {
            if (!material || !material.shader || !material.shader.isSupported) return "missing or unsupported material shader";
            if (material.shader.name != "Universal Render Pipeline/Lit") return "prepared URP/Lit material required: " + material.name;
            foreach (var pair in new[] { new[] { "_BumpMap", "_NORMALMAP" }, new[] { "_MetallicGlossMap", "_METALLICSPECGLOSSMAP" }, new[] { "_EmissionMap", "_EMISSION" } })
                if (material.HasProperty(pair[0]) && material.GetTexture(pair[0]) && !material.IsKeywordEnabled(pair[1])) return material.name + " missing keyword " + pair[1];
            if (material.GetTexture("_MetallicGlossMap") && material.GetFloat("_SmoothnessTextureChannel") != 0) return material.name + " must read smoothness from metallic-map alpha";
            if (material.GetColor("_EmissionColor").maxColorComponent > 0 && !material.IsKeywordEnabled("_EMISSION")) return material.name + " emission keyword disabled";
            return null;
        }

        static Material ResolveMaterial(AAAExemplar payload, Material source)
        {
            if (source)
                foreach (var binding in payload.materials)
                    if (binding.sourceSlotName == source.name) return binding.material;
            return source;
        }

        void Apply(WorldBuilder world, TrackPath track, AAAExemplar payload)
        {
            var baselineRenderers = world.GetComponentsInChildren<Renderer>(true);
            var baselineLights = world.GetComponentsInChildren<Light>(true);
            staging = new GameObject("AAA staging / " + payload.revision);
            staging.SetActive(false);
            staging.transform.SetParent(world.transform, false);
            foreach (var p in payload.placements)
            {
                var frame = track.Evaluate(p.progress);
                Quaternion route = RouteRotation(frame, p.followBank);
                var instance = Instantiate(p.prefab, staging.transform);
                instance.name = "AAA / " + p.semanticGroup;
                instance.transform.SetPositionAndRotation(frame.Position + route * p.offset, route * Quaternion.Euler(p.rotationDegrees));
                instance.transform.localScale = p.prefab.transform.localScale * p.scale;
                foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++) materials[i] = ResolveMaterial(payload, materials[i]);
                    renderer.sharedMaterials = materials;
                }
                foreach (var renderer in baselineRenderers)
                {
                    if (payload.atomicReplacement) break;
                    if (!Named(p.replaceRendererNames, renderer.name) || !ContainsBounds(instance.transform, p.replacementVolume, renderer.bounds)) continue;
                    // A collider on this renderer or any ancestor can still define collision for the visual.
                    if (!CanDisableRendererForReplacement(renderer)) continue;
                    if (!hiddenRenderers.ContainsKey(renderer)) hiddenRenderers.Add(renderer, renderer.enabled);
                    renderer.enabled = false;
                }
                foreach (var light in baselineLights)
                {
                    if (payload.atomicReplacement) break;
                    if (light.type == LightType.Directional || !Named(p.replaceLightNames, light.name) ||
                        !p.replacementVolume.Contains(instance.transform.InverseTransformPoint(light.transform.position))) continue;
                    if (!hiddenLights.ContainsKey(light)) hiddenLights.Add(light, light.enabled);
                    light.enabled = false;
                }
            }
            foreach (var fixture in payload.lights) CreateLight(fixture);
            if (payload.atomicReplacement)
            {
                string targetIssue = CollectAtomicTargets(payload, world.transform, track, baselineRenderers, baselineLights,
                    out var renderers, out var lights);
                if (targetIssue != null) throw new InvalidOperationException(targetIssue);
                foreach (var renderer in renderers)
                { hiddenRenderers.Add(renderer, renderer.enabled); renderer.enabled = false; }
                foreach (var light in lights)
                { hiddenLights.Add(light, light.enabled); light.enabled = false; }
            }
            staging.SetActive(true);
            Debug.Log("VR_AAA_EXEMPLAR loaded revision=" + payload.revision + " placements=" + payload.placements.Length +
                " hiddenRenderers=" + hiddenRenderers.Count + " hiddenLights=" + hiddenLights.Count + " quality=UNREVIEWED");
        }

        void CreateLight(AAAExemplar.LightFixture fixture)
        {
            var go = new GameObject("AAA gallery fixture / " + fixture.id);
            go.transform.SetParent(staging.transform, false);
            go.transform.position = fixture.worldPosition;
            if (fixture.type == LightType.Spot)
            {
                Vector3 up = Mathf.Abs(Vector3.Dot(fixture.worldDirection, Vector3.up)) > .98f ? Vector3.forward : Vector3.up;
                go.transform.rotation = Quaternion.LookRotation(fixture.worldDirection, up);
            }
            var light = go.AddComponent<Light>();
            light.type = fixture.type; light.color = fixture.color; light.intensity = fixture.intensity;
            light.range = fixture.range; light.shadows = fixture.shadows;
            if (fixture.type == LightType.Spot)
            { light.spotAngle = fixture.outerAngle; light.innerSpotAngle = fixture.innerAngle; }
        }

        public static string ValidateGalleryReplacementTargets(AAAExemplar payload, Transform root, TrackPath track)
        {
            if (!root || !track) return "missing gallery replacement root or track";
            return CollectAtomicTargets(payload, root, track, root.GetComponentsInChildren<Renderer>(true),
                root.GetComponentsInChildren<Light>(true), out _, out _);
        }

        static string CollectAtomicTargets(AAAExemplar payload, Transform root, TrackPath track,
            Renderer[] candidates, Light[] lightCandidates, out List<Renderer> renderers, out List<Light> lights)
        {
            renderers = new List<Renderer>(); lights = new List<Light>();
            if (!payload || !payload.atomicReplacement) return "payload is not an atomic gallery replacement";
            var shells = new List<Renderer>();
            foreach (var renderer in candidates)
                if (renderer && renderer.name == NightTrackLighting.WarmContinuousShell && renderer.transform.parent == root) shells.Add(renderer);
            if (shells.Count != 1 || !Finite(shells[0].bounds.center) || shells[0].bounds.size.sqrMagnitude <= 0)
                return "required protected warm gallery continuous shell is missing or ambiguous";
            foreach (var target in payload.rendererReplacements)
            {
                var matches = new List<Renderer>();
                foreach (var renderer in candidates)
                    if (renderer && renderer.name == target.name && renderer.transform.parent == root) matches.Add(renderer);
                if (matches.Count != target.expectedCount) return "gallery renderer target count mismatch " + target.name + " expected=" + target.expectedCount + " actual=" + matches.Count;
                foreach (var renderer in matches)
                {
                    if (!CanDisableRendererForReplacement(renderer))
                        return "gallery renderer target is not replaceable warm detail: " + target.name;
                    if (!Finite(renderer.bounds.center) || !Finite(renderer.bounds.size) || renderer.bounds.size.sqrMagnitude <= 0)
                        return "gallery renderer target has invalid bounds: " + target.name;
                    renderers.Add(renderer);
                }
            }
            foreach (var target in payload.lightReplacements)
            {
                var matches = new List<Light>();
                foreach (var light in lightCandidates)
                    if (light && light.name == target.name && light.transform.parent == root) matches.Add(light);
                if (matches.Count != target.expectedCount) return "gallery light target count mismatch " + target.name + " expected=" + target.expectedCount + " actual=" + matches.Count;
                foreach (var light in matches)
                {
                    if (light.type == LightType.Directional ||
                        !GalleryEnvelopeContains(track, light.transform.position))
                        return "gallery light target is not scoped warm local light: " + target.name;
                    lights.Add(light);
                }
            }
            return null;
        }

        static string ValidateReplacementContract(AAAExemplar payload)
        {
            if (!payload.atomicReplacement)
            {
                if (payload.fullSpanLayout || payload.rendererReplacements.Length > 0 || payload.lightReplacements.Length > 0 || payload.lights.Length > 0)
                    return "non-atomic payload cannot declare full-span replacements or payload lights";
                return null;
            }
            if (!payload.fullSpanLayout || Mathf.Abs(payload.startProgress - WarmGalleryStart) > .000001f ||
                Mathf.Abs(payload.endProgress - WarmGalleryEnd) > .000001f) return "atomic gallery replacement requires exact .86-.902 full span";
            if (!AAACourseIdentity.IsCanonical(payload.sourceCourseSha256)) return "atomic gallery replacement requires source course SHA-256 provenance";
            if (!ExactTargets(payload.rendererReplacements, NightTrackLighting.ReplaceableWarmRendererNames, null))
                return "gallery renderer replacement list is not the closed warm-detail set";
            var expectedLights = new Dictionary<string, int>(StringComparer.Ordinal) {
                { NightTrackLighting.WarmSurfaceWash, NightTrackLighting.WarmSurfaceWashCount },
                { NightTrackLighting.WarmRoadPool, NightTrackLighting.WarmRoadPoolCount }
            };
            if (!ExactTargets(payload.lightReplacements, null, expectedLights))
                return "gallery light replacement list is not the closed warm-local set";
            float min = 1, max = 0; var progress = new List<float>();
            foreach (var placement in payload.placements)
            { min = Mathf.Min(min, placement.progress); max = Mathf.Max(max, placement.progress); progress.Add(placement.progress); }
            if (min > WarmGalleryStart + 1f / 1200 || max < WarmGalleryEnd - 1f / 1200)
                return "gallery placement family does not reach both full-span boundaries";
            if (progress.Count < 7) return "gallery placement family requires at least seven connected anchors";
            progress.Sort();
            for (int i = 1; i < progress.Count; i++)
                if (progress[i] - progress[i - 1] > .01f) return "gallery placement family has an uncovered progress gap";
            return null;
        }

        static bool ExactTargets(AAAExemplar.ReplacementTarget[] actual, string[] names, Dictionary<string, int> counts)
        {
            int expectedCount = names != null ? names.Length : counts.Count;
            if (actual.Length != expectedCount) return false;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var target in actual)
            {
                if (target == null || string.IsNullOrEmpty(target.name) || !seen.Add(target.name)) return false;
                int expected = names != null && Array.IndexOf(names, target.name) >= 0 ? 1 :
                    counts != null && counts.TryGetValue(target.name, out int count) ? count : -1;
                if (target.expectedCount != expected) return false;
            }
            return true;
        }

        static string ValidateLights(AAAExemplar payload)
        {
            if (payload.lights.Length > 32) return "gallery fixture limit is 32";
            if (payload.atomicReplacement && payload.lights.Length == 0) return "atomic gallery replacement requires local fixtures";
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var placements = new HashSet<string>(StringComparer.Ordinal);
            int spots = 0;
            foreach (var placement in payload.placements) placements.Add(placement.semanticGroup);
            foreach (var fixture in payload.lights)
            {
                if (fixture == null || !SafeId(fixture.id) || !ids.Add(fixture.id)) return "missing, unsafe or duplicate fixture id";
                if (fixture.type != LightType.Point && fixture.type != LightType.Spot) return "only point and spot gallery lights are allowed";
                if (!Finite(fixture.worldPosition) || !Finite(fixture.worldDirection) || !Finite(fixture.color.r) ||
                    !Finite(fixture.color.g) || !Finite(fixture.color.b) || fixture.color.r < 0 || fixture.color.r > 1 ||
                    fixture.color.g < 0 || fixture.color.g > 1 || fixture.color.b < 0 || fixture.color.b > 1)
                    return "invalid fixture transform or linear color: " + fixture.id;
                if (!Finite(fixture.intensity) || fixture.intensity <= 0 || fixture.intensity > 2000 ||
                    !Finite(fixture.range) || fixture.range <= 0 || fixture.range > 60) return "invalid fixture intensity or range: " + fixture.id;
                if (fixture.type == LightType.Spot)
                {
                    spots++;
                    if (fixture.worldDirection.sqrMagnitude < .999f || fixture.worldDirection.sqrMagnitude > 1.001f ||
                        !Finite(fixture.outerAngle) || fixture.outerAngle < 10 || fixture.outerAngle > 120 || !Finite(fixture.innerAngle) ||
                        fixture.innerAngle < 0 || fixture.innerAngle > fixture.outerAngle) return "invalid spot aim or angles: " + fixture.id;
                }
                if (fixture.type == LightType.Point && (fixture.worldDirection != Vector3.zero || fixture.outerAngle != 0 || fixture.innerAngle != 0))
                    return "point fixture must omit aim and angles: " + fixture.id;
                if (fixture.shadows != LightShadows.None && fixture.shadows != LightShadows.Hard && fixture.shadows != LightShadows.Soft)
                    return "invalid fixture shadow mode: " + fixture.id;
                if (payload.atomicReplacement && (string.IsNullOrWhiteSpace(fixture.housingSemanticGroup) ||
                    string.IsNullOrWhiteSpace(fixture.socket))) return "gallery fixture requires an authored housing placement and socket: " + fixture.id;
                if (!string.IsNullOrEmpty(fixture.housingSemanticGroup) && !placements.Contains(fixture.housingSemanticGroup))
                    return "fixture housing placement is unresolved: " + fixture.id;
            }
            if (payload.atomicReplacement && spots < 3) return "atomic gallery requires at least three scoped spot fixtures";
            return null;
        }

        public static bool GalleryEnvelopeContains(TrackPath track, Vector3 worldPosition)
        {
            if (!track || !Finite(worldPosition)) return false;
            float best = float.MaxValue, lateral = 0, vertical = 0, forward = 0;
            int first = Mathf.CeilToInt(WarmGalleryStart * 1200), last = Mathf.FloorToInt(WarmGalleryEnd * 1200);
            for (int i = first; i <= last; i++)
            {
                var frame = track.Evaluate(i / 1200f); Vector3 delta = worldPosition - frame.Position;
                float sqr = delta.sqrMagnitude;
                if (sqr >= best) continue;
                best = sqr; lateral = Vector3.Dot(delta, frame.Right); vertical = Vector3.Dot(delta, frame.Up);
                forward = Vector3.Dot(delta, frame.Forward);
            }
            return Mathf.Abs(lateral) <= 18 && vertical >= -2 && vertical <= 20 && Mathf.Abs(forward) <= 3.5f;
        }

        public static Quaternion RouteRotation(TrackFrame frame, bool banked)
        {
            Vector3 up = banked ? frame.Up : Vector3.up;
            if (!Finite(up) || up.sqrMagnitude < .000001f) up = Vector3.up;
            up.Normalize();
            Vector3 forward = Vector3.ProjectOnPlane(frame.Forward, up);
            if (!Finite(forward) || forward.sqrMagnitude < .000001f) forward = Vector3.Cross(frame.Right, up);
            if (!Finite(forward) || forward.sqrMagnitude < .000001f)
                forward = Vector3.ProjectOnPlane(Mathf.Abs(up.z) < .9f ? Vector3.forward : Vector3.right, up);
            return Quaternion.LookRotation(forward.normalized, up);
        }

        static bool ContainsBounds(Transform placement, Bounds volume, Bounds candidate)
        {
            if (volume.size.x <= 0 || volume.size.y <= 0 || volume.size.z <= 0) return false;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        if (!volume.Contains(placement.InverseTransformPoint(candidate.center + Vector3.Scale(candidate.extents, new Vector3(x, y, z))))) return false;
            return true;
        }

        public static bool CanDisableRendererForReplacement(Renderer renderer)
        {
            if (!renderer) return false;
            for (Transform current = renderer.transform; current; current = current.parent)
                if (current.GetComponent<Collider>()) return false;
            return true;
        }

        static bool Named(string[] names, string value) => names != null && Array.IndexOf(names, value) >= 0;
        static bool SafeId(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            for (int i = 0; i < value.Length; i++)
                if (!(char.IsLetterOrDigit(value[i]) || value[i] == '_' || value[i] == '-')) return false;
            return true;
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);

        public void RestoreBaseline()
        {
            if (staging) { staging.SetActive(false); Destroy(staging); staging = null; }
            foreach (var pair in hiddenRenderers) if (pair.Key) pair.Key.enabled = pair.Value;
            foreach (var pair in hiddenLights) if (pair.Key) pair.Key.enabled = pair.Value;
            hiddenRenderers.Clear(); hiddenLights.Clear();
        }

        void OnDisable() => RestoreBaseline();
        void OnDestroy() => RestoreBaseline();
    }
}
