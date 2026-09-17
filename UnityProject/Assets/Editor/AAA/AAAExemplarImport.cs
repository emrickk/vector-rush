using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace VectorRush.Editor
{
    /// <summary>Explicit, additive staging. No scene, bootstrap or rendering-pipeline preparation.</summary>
    public static class AAAExemplarImport
    {
        const string Schema = "aaa-nocturne-exemplar-1";
        const string ResourceRoot = "Assets/Resources/AAA/";
        const float BoundsTolerance = .01f; // Metres, never a compensating model scale.
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        [Serializable]
        sealed class LegacyFrame { public float progress, distanceMeters; public float[] position, forward, right, up; }
        [Serializable]
        sealed class LegacyCourse { public float width, length; public LegacyFrame[] frames; }

        public static void ImportStaging()
        {
            // No safe report destination exists until this mandatory flag passes.
            string evidence = AbsoluteFlag("-aaaEvidence");
            Require(!Exists(evidence), "-aaaEvidence must be a new, nonexistent directory: " + evidence);
            NoLinks(evidence);
            string package = AbsoluteFlag("-aaaArtPackage");
            string project = Directory.GetParent(Application.dataPath).FullName;
            Require(!Overlap(evidence, project) && !Overlap(evidence, package), "Evidence must be outside the Unity project and art package");
            Directory.CreateDirectory(evidence);
            var report = new JObject {
                ["status"] = "FAILED", ["visualStatus"] = "BLOCKED_VISUAL_REVIEW",
                ["nativeStatus"] = "NOT_RUN", ["package"] = package,
                ["unityVersion"] = Application.unityVersion,
                ["boundsToleranceMeters"] = BoundsTolerance,
                ["lodScreenHeights"] = new JArray(.6f, .02f),
                ["notes"] = new JArray("Gallery 02 replacement is opt-in and atomic; the legacy shell and protected baseline remain.",
                    "LOD thresholds are technical staging defaults, not artist-approved distances.",
                    "Bounds test detects unit/axis discrepancies; visual facing remains unverified.")
            };
            var published = new List<string>();
            AAAExemplar migratedOpening = null;
            string migratedOpeningJson = null;
            AAAExemplar replacedGallery = null;
            string replacedGalleryJson = null;
            GameObject courseObject = null;
            try
            {
                Require(Path.GetFullPath("Assets") == Path.GetFullPath(Application.dataPath), "Run the batch command from its Unity project directory");
                NoLinks(package);
                Require(Directory.Exists(package), "Missing package: " + package);
                JObject manifest = ReadJson(FilePath(package, "manifest.json"));
                JObject layout = ReadJson(FilePath(package, "layout.json"));
                JObject staged = ReadJson(FilePath(package, "STAGED.json"));
                string stagedIdentity = Hash(FilePath(package, "STAGED.json"));
                JObject checksums = ReadJson(FilePath(package, "checksums.json"));
                JObject stats = ReadJson(FilePath(package, "asset-stats.json"));
                string revision = Text(manifest, "revision");
                bool galleryOnly = revision.StartsWith("exemplar-01-gallery02", StringComparison.Ordinal);
                JObject lighting = galleryOnly ? ReadJson(FilePath(package, Text(manifest, "galleryLighting"))) : null;
                JObject sourceDescriptor = galleryOnly ? manifest["courseSource"] as JObject : null;
                string sourceCoursePath = galleryOnly ? Text(sourceDescriptor, "path") : null;
                string sourceCourseFile = galleryOnly ? FilePath(package, sourceCoursePath) : null;
                JObject sourceCourse = galleryOnly ? ReadJson(sourceCourseFile) : null;
                report["manifestSha256"] = Hash(FilePath(package, "manifest.json"));
                report["layoutSha256"] = Hash(FilePath(package, "layout.json"));
                report["checksumsSha256"] = Hash(FilePath(package, "checksums.json"));
                report["stagedSha256"] = stagedIdentity;
                report["sourceIds"] = new JArray(Array(manifest, "assets").Select(a => Text(a, "id")));
                report["materialIds"] = new JArray(Array(manifest, "materials").Select(a => Text(a, "id")));
                report["placementIds"] = new JArray(Array(layout, "instances").Select(a => Text(a, "id")));
                report["layoutComplete"] = Bool(layout, "layoutComplete");
                Require(Text(staged, "checksumsSha256") == (string)report["checksumsSha256"], "STAGED/checksums mismatch; wait for artist finalization");
                Require(Text(staged, "status") == "TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW", "Package is not finalized for technical staging");
                VerifyChecksums(package, checksums);
                foreach (string path in new[] { "manifest.json", "layout.json", "asset-stats.json" }) Covered(checksums, path);
                courseObject = new GameObject("AAA import course identity") { hideFlags = HideFlags.HideAndDontSave };
                var track = courseObject.AddComponent<TrackPath>();
                track.Ensure();
                string currentHash = AAACourseIdentity.Compute(track); // Runtime-shared stable float-bit identity.
                report["currentCourseHash"] = currentHash;
                report["courseHash"] = Text(manifest, "courseHash");
                ValidatePackage(manifest, layout, staged, stats, checksums, lighting, sourceCourse,
                    sourceCoursePath, sourceCourseFile, currentHash, track, galleryOnly);
                string sourceCourseSha256 = galleryOnly ? Hash(sourceCourseFile) : null;
                if (galleryOnly) report["sourceCourseSha256"] = sourceCourseSha256;
                report["sourceBlendSha256"] = Hash(FilePath(package, Text(manifest, "sourceBlend")));
                string destination = ResourceRoot + "Imported/" + revision;
                report["destination"] = destination;
                FreshAssetPath(destination);
                if (!galleryOnly)
                    foreach (string name in new[] { "OpeningExemplar", "GalleryExemplar" }) FreshAssetPath(ResourceRoot + name + ".asset");
                // All source/route checks above precede any destination mutation.
                MakeFolder(destination);
                var materials = ImportMaterials(package, destination, manifest, checksums);
                var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
                var boundsEvidence = new JArray();
                report["importedBounds"] = boundsEvidence;
                HashSet<string> selectedAssets = galleryOnly ? new HashSet<string>(Array(layout, "instances")
                    .Where(p => Text(p, "zoneId") == "station").Select(p => Text(p, "assetId")), StringComparer.Ordinal) : null;
                foreach (JToken asset in Array(manifest, "assets"))
                    if (selectedAssets == null || selectedAssets.Contains(Text(asset, "id")))
                        prefabs.Add(Text(asset, "id"), ImportPrefab(package, destination, asset, stats, materials, checksums, boundsEvidence));
                var recipes = new Dictionary<string, AAAExemplar>();
                if (!galleryOnly) recipes.Add("OpeningExemplar", CreateRecipe("viaduct", revision, currentHash, null, layout, null, track, prefabs, materials, false));
                recipes.Add("GalleryExemplar", CreateRecipe("station", revision, currentHash, sourceCourseSha256, layout, lighting, track, prefabs, materials, galleryOnly));
                MakeFolder(destination + "/Recipes");
                var placementsEvidence = new JArray();
                report["placements"] = placementsEvidence;
                foreach (var pair in recipes)
                {
                    AssetDatabase.CreateAsset(pair.Value, destination + "/Recipes/" + pair.Key + ".asset");
                    ValidatePrepared(pair.Value);
                    foreach (var p in pair.Value.placements)
                        placementsEvidence.Add(new JObject { ["resource"] = pair.Key, ["sourceId"] = p.semanticGroup,
                            ["progress"] = p.progress, ["offset"] = V(p.offset), ["rotationDegrees"] = V(p.rotationDegrees), ["scale"] = p.scale });
                }
                AssetDatabase.SaveAssets();
                VerifyChecksums(package, checksums); // Artist must not change the package mid-import.
                Require(Hash(FilePath(package, "checksums.json")) == (string)report["checksumsSha256"], "Package changed during import");
                Require(Hash(FilePath(package, "STAGED.json")) == stagedIdentity, "STAGED identity changed during import");
                report["lights"] = galleryOnly ? recipes["GalleryExemplar"].lights.Length : 0;
                // Publish opt-in resources last. Gallery 02 preserves the opening resource and updates
                // only the gallery resource after every source/import contract has passed.
                foreach (var pair in recipes)
                {
                    string target = ResourceRoot + pair.Key + ".asset";
                    if (galleryOnly)
                    {
                        Require(pair.Key == "GalleryExemplar", "Gallery-only package attempted non-gallery publication");
                        migratedOpening = Resources.Load<AAAExemplar>(AAABaselineIntegration.OpeningResource);
                        if (migratedOpening)
                        {
                            bool openingNeedsMigration = migratedOpening.courseHash != currentHash || migratedOpening.rendererReplacements == null ||
                                migratedOpening.lightReplacements == null || migratedOpening.lights == null;
                            if (openingNeedsMigration)
                            {
                                if (migratedOpening.courseHash != currentHash)
                                    Require(!migratedOpening.atomicReplacement && migratedOpening.courseHash == LegacyJsonCourseHash(track),
                                        "Retained opening resource is not tied to the verified current course");
                                migratedOpeningJson = EditorJsonUtility.ToJson(migratedOpening);
                                migratedOpening.courseHash = currentHash;
                                migratedOpening.sourceCourseSha256 = sourceCourseSha256;
                                if (migratedOpening.rendererReplacements == null) migratedOpening.rendererReplacements = System.Array.Empty<AAAExemplar.ReplacementTarget>();
                                if (migratedOpening.lightReplacements == null) migratedOpening.lightReplacements = System.Array.Empty<AAAExemplar.ReplacementTarget>();
                                if (migratedOpening.lights == null) migratedOpening.lights = System.Array.Empty<AAAExemplar.LightFixture>();
                                EditorUtility.SetDirty(migratedOpening);
                            }
                        }
                        replacedGallery = Resources.Load<AAAExemplar>(AAABaselineIntegration.GalleryResource);
                        if (replacedGallery)
                        {
                            replacedGalleryJson = EditorJsonUtility.ToJson(replacedGallery);
                            EditorUtility.CopySerialized(pair.Value, replacedGallery);
                            EditorUtility.SetDirty(replacedGallery);
                        }
                        else
                        {
                            FreshAssetPath(target);
                            string error = AssetDatabase.MoveAsset(destination + "/Recipes/" + pair.Key + ".asset", target);
                            Require(string.IsNullOrEmpty(error), "Gallery resource publication failed: " + error);
                            published.Add(target);
                        }
                    }
                    else
                    {
                        FreshAssetPath(target);
                        string error = AssetDatabase.MoveAsset(destination + "/Recipes/" + pair.Key + ".asset", target);
                        Require(string.IsNullOrEmpty(error), "Resource publication failed: " + error);
                        published.Add(target);
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var publishedOpening = Resources.Load<AAAExemplar>(AAABaselineIntegration.OpeningResource);
                var publishedGallery = Resources.Load<AAAExemplar>(AAABaselineIntegration.GalleryResource);
                if (galleryOnly)
                {
                    AAAExemplarValidation.RequireGalleryResource(publishedGallery);
                    AAAExemplarValidation.ValidateRequiredGalleryStaging();
                }
                else
                {
                    AAAExemplarValidation.RequirePublishedResources(publishedOpening, publishedGallery);
                    AAAExemplarValidation.ValidateRequiredStaging();
                }
                report["status"] = "TECHNICAL_STAGING_COMPLETE";
                report["resources"] = galleryOnly ? new JArray(ResourceRoot + "GalleryExemplar.asset") : new JArray(published);
                File.WriteAllText(Path.Combine(evidence, "import-report.json"), report.ToString(Formatting.Indented), Utf8);
                Debug.Log("VR_AAA_IMPORT technical staging complete; BLOCKED_VISUAL_REVIEW; report=" + evidence);
            }
            catch (Exception exception)
            {
                var cleanup = new JArray();
                if (migratedOpening && migratedOpeningJson != null)
                {
                    try
                    {
                        EditorJsonUtility.FromJsonOverwrite(migratedOpeningJson, migratedOpening);
                        EditorUtility.SetDirty(migratedOpening); AssetDatabase.SaveAssets();
                        cleanup.Add(AAABaselineIntegration.OpeningResource + ": restored prior identity metadata");
                    }
                    catch (Exception failure) { cleanup.Add(AAABaselineIntegration.OpeningResource + ": ROLLBACK FAILED " + failure.Message); }
                }
                if (replacedGallery && replacedGalleryJson != null)
                {
                    try
                    {
                        EditorJsonUtility.FromJsonOverwrite(replacedGalleryJson, replacedGallery);
                        EditorUtility.SetDirty(replacedGallery); AssetDatabase.SaveAssets();
                        cleanup.Add(AAABaselineIntegration.GalleryResource + ": restored prior serialized payload");
                    }
                    catch (Exception failure) { cleanup.Add(AAABaselineIntegration.GalleryResource + ": ROLLBACK FAILED " + failure.Message); }
                }
                foreach (string path in published)
                {
                    try { cleanup.Add(path + ": removed=" + AssetDatabase.DeleteAsset(path)); }
                    catch (Exception failure) { cleanup.Add(path + ": ROLLBACK FAILED " + failure.Message); }
                }
                report["status"] = "FAILED";
                report["error"] = exception.ToString();
                report["publicationRollback"] = cleanup;
                report["nextAction"] = "Inspect reported discrepancy; retain staged artifacts for diagnosis. Retry in a fresh destination/revision and fresh evidence directory. Do not scale or adopt art to bypass failure.";
                File.WriteAllText(Path.Combine(evidence, "import-report.json"), report.ToString(Formatting.Indented), Utf8);
                Debug.LogError("VR_AAA_IMPORT failed; report=" + evidence + ": " + exception.Message);
                throw;
            }
            finally { if (courseObject) Object.DestroyImmediate(courseObject); }
        }

        static void ValidatePackage(JObject manifest, JObject layout, JObject staged, JObject stats, JObject sums,
            JObject lighting, JObject sourceCourse, string sourceCoursePath, string sourceCourseFile,
            string courseHash, TrackPath track, bool galleryOnly)
        {
            Require(Text(manifest, "schema") == Schema && Text(layout, "schema") == Schema, "Unsupported manifest/layout schema");
            string revision = Text(manifest, "revision");
            Require(Regex.IsMatch(revision, @"^exemplar-01(?:-[A-Za-z0-9_-]+)?$"), "Revision must be exemplar-01 or exemplar-01-<revision>");
            Require(galleryOnly == revision.StartsWith("exemplar-01-gallery02", StringComparison.Ordinal), "Gallery package mode mismatch");
            if (galleryOnly) Require(Regex.IsMatch(revision, @"^exemplar-01-gallery02(?:-[A-Za-z0-9_]+)?$"), "Invalid Gallery 02 revision");
            Require(Text(staged, "revision") == revision, "STAGED revision mismatch");
            string expectedDocumentHash = galleryOnly ? courseHash : LegacyJsonCourseHash(track);
            foreach (var document in new[] { manifest, layout, staged })
                Require(Text(document, "courseHash") == expectedDocumentHash, "Course hash differs from actual current TrackPath; regenerate package from current course export");
            if (galleryOnly)
            {
                Require(Bool(manifest, "layoutComplete") && Bool(layout, "layoutComplete") && Bool(staged, "layoutComplete"),
                    "Gallery 02 requires a finalized full-span layout");
                Covered(sums, Text(manifest, "galleryLighting"));
                Require(sourceCoursePath == "course-data.json", "Gallery course source must be course-data.json");
                Covered(sums, sourceCoursePath);
                string declaredSourceHash = Text(manifest["courseSource"], "sha256");
                Require(AAACourseIdentity.IsCanonical(declaredSourceHash) && Text(sums, sourceCoursePath) == declaredSourceHash &&
                    Hash(sourceCourseFile) == declaredSourceHash, "Course source byte provenance mismatch");
                ValidateCourseSource(sourceCourse, track, courseHash);
                ValidateGalleryLayout(layout);
                ValidateGalleryLighting(lighting, layout, track, courseHash);
            }
            Require(Text(manifest, "unit") == "metre" && Text(manifest, "sourceToUnity") == "(x,z,-y)", "Unsupported units/source axis contract");
            var convention = manifest["exportConvention"];
            Require(Text(convention, "axis_forward") == "-Z" && Text(convention, "axis_up") == "Y" &&
                Bool(convention, "bake_space_transform") && Text(convention, "apply_scale_options") == "FBX_SCALE_UNITS", "Unsupported FBX export convention");
            Require(manifest["lights"] == null || Array(manifest, "lights").Count == 0,
                "Use the checksummed galleryLighting document for authored local lights");
            Covered(sums, Text(manifest, "sourceBlend"));
            var materialIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JToken m in Array(manifest, "materials"))
            {
                string id = SafeId(m);
                Require(materialIds.Add(id), "Duplicate material " + id);
                ColorValue(m, "baseColorLinear"); ColorValue(m, "emissionColorLinear");
                Range(m, "metallic", 0, 1); Range(m, "roughness", 0, 1); Range(m, "normalScale", 0, 10);
                Range(m, "emissionIntensity", 0, galleryOnly ? 2 : float.MaxValue);
                Require(Text(m, "uvMode") == "metric", "Unsupported UV mode: " + id);
                foreach (string key in new[] { "metersPerTile", "textureScale" })
                    Require(Numbers(m, key, 2).All(v => v > 0), "Invalid UV contract: " + id + "/" + key);
                foreach (string key in new[] { "baseColor", "metallicSmoothness", "normal" })
                    if (m[key] != null) { string file = Text(m, key); Require(file.EndsWith(".png", StringComparison.Ordinal), "PNG required: " + file); Covered(sums, file); }
            }
            Require(materialIds.Count > 0, "Empty materials");
            var assetIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JToken asset in Array(manifest, "assets"))
            {
                string id = SafeId(asset);
                Require(assetIds.Add(id), "Duplicate asset " + id);
                Require(Text(asset, "collider") == "none" && Text(asset, "lightmapUVs") == "generate" && !Bool(asset, "trackAssembly"), "Unsupported asset contract: " + id);
                Bounds declared = ReadBounds(asset, "boundsMin", "boundsMax");
                var slots = Strings(asset, "materialSlots");
                Require(slots.Length > 0 && slots.Distinct().Count() == slots.Length && slots.All(materialIds.Contains), "Unresolved/duplicate material slots: " + id);
                foreach (string lod in new[] { "lod0", "lod1" })
                {
                    string file = Text(asset, lod);
                    Require(file == "meshes/" + id + "_" + lod.ToUpperInvariant() + ".fbx", "Unexpected LOD source path: " + file);
                    Covered(sums, file);
                    JToken s = stats[Path.GetFileNameWithoutExtension(file)];
                    Require(s != null && Text(s, "fbxSha256") == Text(sums, file), "Missing/mismatched LOD stats: " + file);
                    Bounds expected = ReadBounds(s, "unityBoundsMin", "unityBoundsMax");
                    Bounds source = ReadBounds(s, "sourceBoundsMin", "sourceBoundsMax");
                    Vector3 min = source.min, max = source.max;
                    Require(Vector3.Distance(expected.min, new Vector3(min.x, min.z, -max.y)) < .0001f &&
                        Vector3.Distance(expected.max, new Vector3(max.x, max.z, -min.y)) < .0001f, "Stats source-to-Unity axis discrepancy: " + file);
                    if (lod == "lod0") Require(BoundsError(expected, declared) < .0001f, "Manifest/stats bounds mismatch: " + file);
                    Require(Strings(s, "materialSlots").All(slots.Contains), "LOD stats contain undeclared slots: " + file);
                }
            }
            Require(assetIds.Count > 0, "Empty asset list");
            var placementIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (JToken p in Array(layout, "instances"))
            {
                Require(placementIds.Add(SafeId(p)) && assetIds.Contains(Text(p, "assetId")), "Duplicate/unresolved placement");
                string zone = Text(p, "zoneId");
                Require(zone == "viaduct" || zone == "station", "Unsupported exemplar zone: " + zone);
                Require(Text(p, "frameMode") == "banked" || Text(p, "frameMode") == "upright", "Unsupported frame mode");
                Vector(p, "position"); Rotation(p);
                Vector3 scale = Vector(p, "scale");
                Require(scale.x > 0 && Mathf.Abs(scale.x - scale.y) < .000001f && Mathf.Abs(scale.x - scale.z) < .000001f, "Placement requires positive uniform scale: " + Text(p, "id"));
                JToken source = p["sourceFrame"];
                float progress = Range(source, "progress", 0, .999999f);
                int index = p.Value<int>("frameIndex");
                Require(p["frameIndex"]?.Type == JTokenType.Integer && index >= 0 && index < 1200 && Mathf.Abs(progress - index / 1200f) < .000001f, "Invalid source frame index");
                var frame = track.Evaluate(progress);
                Require(Vector3.Distance(Vector(source, "position"), frame.Position) < .002f &&
                    Vector3.Distance(Vector(source, "forward"), frame.Forward) < .0001f &&
                    Vector3.Distance(Vector(source, "right"), frame.Right) < .0001f &&
                    Vector3.Distance(Vector(source, "up"), frame.Up) < .0001f &&
                    Mathf.Abs(Number(source, "distanceMeters") - progress * track.Length) < .002f, "Source frame disagrees with exact TrackPath.Evaluate: " + Text(p, "id"));
            }
            foreach (string zone in galleryOnly ? new[] { "station" } : new[] { "viaduct", "station" })
            {
                float[] progress = Array(layout, "instances").Where(p => Text(p, "zoneId") == zone).Select(p => Number(p["sourceFrame"], "progress")).ToArray();
                Require(progress.Length > 0 && progress.Max() - progress.Min() <= .15f, "Missing or unbounded local exemplar: " + zone);
            }
        }

        static void ValidateCourseSource(JObject course, TrackPath track, string courseHash)
        {
            Require(course != null, "Missing parsed course source");
            Require(SameFloat(Number(course, "width"), track.Width) && SameFloat(Number(course, "length"), track.Length),
                "Course source width/length differs from current TrackPath");
            JArray frames = Array(course, "frames");
            Require(frames.Count == 1201, "Course source requires exactly 1,201 frames");
            for (int i = 0; i <= 1200; i++)
            {
                JToken source = frames[i]; float progress = i / 1200f; TrackFrame frame = track.Evaluate(progress);
                Require(SameFloat(Number(source, "progress"), progress) &&
                    SameFloat(Number(source, "distanceMeters"), progress * track.Length) &&
                    SameVector(Vector(source, "position"), frame.Position) && SameVector(Vector(source, "forward"), frame.Forward) &&
                    SameVector(Vector(source, "right"), frame.Right) && SameVector(Vector(source, "up"), frame.Up),
                    "Course source frame differs from exact current TrackPath at index " + i);
            }
            Require(AAACourseIdentity.Compute(track) == courseHash, "Stable course identity mismatch after source-frame verification");
        }

        static void ValidateGalleryLayout(JObject layout)
        {
            JToken span = layout["gallerySpan"];
            Require(Mathf.Abs(Number(span, "startProgress") - AAABaselineIntegration.WarmGalleryStart) <= .000001f &&
                Mathf.Abs(Number(span, "endProgress") - AAABaselineIntegration.WarmGalleryEnd) <= .000001f,
                "Gallery 02 span must be exactly .86-.902");
            JToken replacement = layout["replacement"];
            Require(Text(replacement, "mode") == "warm-details-and-local-lights", "Unsupported gallery replacement mode");
            string[] renderers = Strings(replacement, "rendererNames");
            Require(new HashSet<string>(renderers, StringComparer.Ordinal).SetEquals(NightTrackLighting.ReplaceableWarmRendererNames) &&
                renderers.Length == NightTrackLighting.ReplaceableWarmRendererNames.Length,
                "Replacement renderers must be the exact closed warm-detail set");
            string[] lights = Strings(replacement, "lightNames");
            Require(lights.Length == 2 && new HashSet<string>(lights, StringComparer.Ordinal).SetEquals(new[] {
                NightTrackLighting.WarmSurfaceWash, NightTrackLighting.WarmRoadPool }),
                "Replacement lights must be the exact warm local-light set");
            float min = 1, max = 0;
            var ordered = new List<float>();
            foreach (JToken placement in Array(layout, "instances").Where(p => Text(p, "zoneId") == "station"))
            {
                float progress = Number(placement["sourceFrame"], "progress");
                Require(progress >= AAABaselineIntegration.WarmGalleryStart - .000001f &&
                    progress <= AAABaselineIntegration.WarmGalleryEnd + .000001f, "Gallery placement outside declared full span");
                min = Mathf.Min(min, progress); max = Mathf.Max(max, progress);
                ordered.Add(progress);
            }
            Require(min <= AAABaselineIntegration.WarmGalleryStart + 1f / 1200 &&
                max >= AAABaselineIntegration.WarmGalleryEnd - 1f / 1200,
                "Gallery placement family must reach the entrance and exit boundaries");
            Require(ordered.Count >= 7, "Gallery placement family requires at least seven connected anchors");
            ordered.Sort();
            for (int i = 1; i < ordered.Count; i++)
                Require(ordered[i] - ordered[i - 1] <= .01f, "Gallery placement family has an uncovered progress gap greater than 1% of lap");
        }

        static void ValidateGalleryLighting(JObject lighting, JObject layout, TrackPath track, string courseHash)
        {
            Require(lighting != null && Text(lighting, "schema") == "aaa-nocturne-gallery-lighting-1", "Unsupported gallery lighting schema");
            Require(Text(lighting, "courseHash") == courseHash, "Gallery lighting course identity mismatch");
            JArray fixtures = Array(lighting, "fixtures");
            Require(fixtures.Count >= 1 && fixtures.Count <= 32, "Gallery lighting requires 1-32 fixtures");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var placements = new HashSet<string>(Array(layout, "instances").Where(p => Text(p, "zoneId") == "station").Select(SafeId), StringComparer.Ordinal);
            int spots = 0;
            foreach (JToken fixture in fixtures)
            {
                string id = SafeId(fixture); Require(ids.Add(id), "Duplicate gallery fixture " + id);
                string type = Text(fixture, "type"); Require(type == "point" || type == "spot", "Unsupported gallery fixture type: " + id);
                Vector3 position = Vector(fixture, "position");
                Require(AAABaselineIntegration.GalleryEnvelopeContains(track, position), "Gallery fixture outside scoped envelope: " + id);
                ColorValue(fixture, "rgb"); Range(fixture, "intensity", float.Epsilon, 2000); Range(fixture, "range", float.Epsilon, 60);
                bool direction = fixture["aimDirection"] != null, target = fixture["target"] != null;
                if (type == "spot")
                {
                    spots++; Require(direction != target, "Spot requires exactly one aimDirection or target: " + id);
                    Vector3 aim = direction ? Vector(fixture, "aimDirection") : Vector(fixture, "target") - position;
                    Require(aim.sqrMagnitude > .000001f, "Spot aim must be nonzero: " + id);
                    if (target) Require(AAABaselineIntegration.GalleryEnvelopeContains(track, Vector(fixture, "target")),
                        "Gallery fixture target outside scoped envelope: " + id);
                    float outer = Range(fixture, "outerAngle", 10, 120), inner = Range(fixture, "innerAngle", 0, 120);
                    Require(inner <= outer, "Spot innerAngle exceeds outerAngle: " + id);
                }
                else
                    Require(!direction && !target && fixture["outerAngle"] == null && fixture["innerAngle"] == null,
                        "Point fixture must omit aim and angles: " + id);
                string shadows = OptionalText(fixture, "shadows", "none");
                Require(shadows == "none" || shadows == "hard" || shadows == "soft", "Invalid shadow mode: " + id);
                string housing = OptionalText(fixture, "housingInstanceId", OptionalText(fixture, "sourcePlacement", null));
                string socket = OptionalText(fixture, "socket", OptionalText(fixture, "sourcePart", null));
                Require(housing != null && socket != null, "Gallery fixture requires an authored housing instance and socket: " + id);
                Require(placements.Contains(housing), "Unresolved fixture housing instance: " + id);
            }
            Require(spots >= 3, "Gallery candidate requires at least three scoped spot fixtures for separated local pools");
        }

        static Dictionary<string, Material> ImportMaterials(string package, string root, JObject manifest, JObject sums)
        {
            MakeFolder(root + "/Materials");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Require(shader && shader.isSupported, "URP/Lit shader unavailable");
            var result = new Dictionary<string, Material>(StringComparer.Ordinal);
            var textureRoles = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (JToken m in Array(manifest, "materials"))
            {
                var material = new Material(shader) { name = Text(m, "id") };
                // Textured base maps already contain the authored color; avoid tinting twice.
                material.SetColor("_BaseColor", m["baseColor"] != null ? Color.white : ColorValue(m, "baseColorLinear"));
                material.SetFloat("_Metallic", Number(m, "metallic"));
                material.SetFloat("_Smoothness", m["metallicSmoothness"] != null ? 1 : 1 - Number(m, "roughness"));
                material.SetFloat("_SmoothnessTextureChannel", 0);
                material.SetFloat("_WorkflowMode", 1);
                material.SetFloat("_BumpScale", Number(m, "normalScale"));
                material.SetFloat("_Surface", 0); material.SetFloat("_Cull", (float)CullMode.Back);
                material.SetFloat("_ZWrite", 1); material.SetFloat("_SrcBlend", (float)BlendMode.One); material.SetFloat("_DstBlend", (float)BlendMode.Zero);
                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue = (int)RenderQueue.Geometry;
                Color emission = ColorValue(m, "emissionColorLinear") * Number(m, "emissionIntensity");
                material.SetColor("_EmissionColor", emission);
                if (emission.maxColorComponent > 0) material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = emission.maxColorComponent > 0 ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                foreach (string key in new[] { "baseColor", "metallicSmoothness", "normal" })
                {
                    if (m[key] == null) continue;
                    string file = Text(m, key), path = root + "/" + file;
                    if (textureRoles.TryGetValue(file, out string role)) Require(role == key, "Conflicting texture role: " + file);
                    else
                    {
                        textureRoles.Add(file, key);
                        CopyAsset(package, root, file, sums);
                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        Require(importer, "Missing texture importer: " + path);
                        importer.textureType = key == "normal" ? TextureImporterType.NormalMap : TextureImporterType.Default;
                        importer.sRGBTexture = key == "baseColor";
                        importer.convertToNormalmap = false;
                        importer.mipmapEnabled = true; importer.wrapMode = TextureWrapMode.Repeat;
                        importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = false;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                    string property = key == "baseColor" ? "_BaseMap" : key == "normal" ? "_BumpMap" : "_MetallicGlossMap";
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    Require(texture, "Texture import failed: " + path);
                    material.SetTexture(property, texture);
                    float[] uv = Numbers(m, "textureScale", 2);
                    material.SetTextureScale(property, new Vector2(uv[0], uv[1]));
                    if (key == "normal") material.EnableKeyword("_NORMALMAP");
                    if (key == "metallicSmoothness") material.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                Require(AAABaselineIntegration.ValidateMaterial(material) == null, "Invalid prepared material " + material.name);
                AssetDatabase.CreateAsset(material, root + "/Materials/" + material.name + ".mat");
                result.Add(material.name, material);
            }
            return result;
        }

        static GameObject ImportPrefab(string package, string root, JToken asset, JObject stats, Dictionary<string, Material> materials, JObject sums, JArray evidence)
        {
            string id = Text(asset, "id");
            var parent = new GameObject(id) { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var lods = new List<LOD>();
                foreach (string lod in new[] { "lod0", "lod1" })
                {
                    string file = Text(asset, lod), path = root + "/" + file;
                    CopyAsset(package, root, file, sums);
                    var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    Require(importer, "Missing model importer: " + path);
                    // The package FBX exporter already bakes its declared -Z/Y space. Unity's
                    // additional axis bake reverses the package's forward sign on 6000.6.
                    importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = false;
                    importer.importAnimation = false; importer.animationType = ModelImporterAnimationType.None;
                    importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
                    importer.importNormals = ModelImporterNormals.Import; importer.importTangents = ModelImporterTangents.Import;
                    importer.generateSecondaryUV = true; importer.isReadable = true;
                    importer.meshCompression = ModelImporterMeshCompression.Off;
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                    foreach (string slot in Strings(asset, "materialSlots"))
                        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), slot), materials[slot]);
                    importer.SaveAndReimport();
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Require(model, "FBX import failed: " + path);
                    var instance = Object.Instantiate(model, parent.transform, false);
                    instance.name = lod.ToUpperInvariant();
                    var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                    Require(renderers.Length > 0, "Empty FBX: " + path);
                    var seen = new HashSet<string>(StringComparer.Ordinal);
                    var expectedSlots = Strings(stats[Path.GetFileNameWithoutExtension(file)], "materialSlots");
                    bool first = true;
                    Bounds actual = default;
                    foreach (var renderer in renderers)
                    {
                        var filter = renderer.GetComponent<MeshFilter>();
                        Require(filter && filter.sharedMesh && filter.sharedMesh.isReadable, "Readable imported mesh required: " + path);
                        Require(renderer.sharedMaterials.Length == filter.sharedMesh.subMeshCount, "Submesh/slot count mismatch: " + path);
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            Require(material && materials.TryGetValue(material.name, out var prepared) && prepared == material && expectedSlots.Contains(material.name), "Unmapped exact FBX slot in " + path);
                            seen.Add(material.name);
                        }
                        // Measure real vertices in prefab-root space, including importer axis/unit transforms.
                        foreach (Vector3 vertex in filter.sharedMesh.vertices)
                        {
                            Vector3 point = parent.transform.InverseTransformPoint(filter.transform.TransformPoint(vertex));
                            Require(Finite(point), "Nonfinite imported vertex: " + path);
                            if (first) { actual = new Bounds(point, Vector3.zero); first = false; } else actual.Encapsulate(point);
                        }
                    }
                    Require(!first && seen.SetEquals(expectedSlots), "Missing/extra imported material slots: " + path);
                    Bounds expected = ReadBounds(stats[Path.GetFileNameWithoutExtension(file)], "unityBoundsMin", "unityBoundsMax");
                    float error = BoundsError(actual, expected);
                    evidence.Add(new JObject { ["sourceId"] = id, ["lod"] = lod, ["file"] = file,
                        ["actualMin"] = V(actual.min), ["actualMax"] = V(actual.max),
                        ["expectedMin"] = V(expected.min), ["expectedMax"] = V(expected.max), ["maxAxisErrorMeters"] = error });
                    Require(error <= BoundsTolerance, "Imported bounds/axis/unit discrepancy " + path + ": " + error + " m; no automatic scale correction permitted");
                    lods.Add(new LOD(lod == "lod0" ? .6f : .02f, renderers));
                }
                var group = parent.AddComponent<LODGroup>();
                group.SetLODs(lods.ToArray()); group.RecalculateBounds();
                parent.hideFlags = HideFlags.None;
                MakeFolder(root + "/Prefabs");
                var prefab = PrefabUtility.SaveAsPrefabAsset(parent, root + "/Prefabs/" + id + ".prefab");
                Require(prefab, "Prefab serialization failed: " + id);
                return prefab;
            }
            finally { Object.DestroyImmediate(parent); }
        }

        static AAAExemplar CreateRecipe(string zone, string revision, string courseHash, string sourceCourseSha256,
            JObject layout, JObject lighting, TrackPath track, Dictionary<string, GameObject> prefabs,
            Dictionary<string, Material> materials, bool atomicGallery)
        {
            var recipe = ScriptableObject.CreateInstance<AAAExemplar>();
            recipe.revision = revision + "/" + zone + "/BLOCKED_VISUAL_REVIEW";
            recipe.courseHash = courseHash;
            recipe.sourceCourseSha256 = sourceCourseSha256;
            recipe.fullSpanLayout = atomicGallery;
            recipe.atomicReplacement = atomicGallery;
            recipe.materials = materials.Select(pair => new AAAExemplar.MaterialBinding { sourceSlotName = pair.Key, material = pair.Value }).ToArray();
            recipe.placements = Array(layout, "instances").Where(p => Text(p, "zoneId") == zone).Select(p =>
            {
                float progress = Number(p["sourceFrame"], "progress");
                var frame = track.Evaluate(progress);
                bool banked = Text(p, "frameMode") == "banked";
                Quaternion route = AAABaselineIntegration.RouteRotation(frame, banked);
                Vector3 absolutePosition = Vector(p, "position");
                Quaternion absoluteRotation = Rotation(p);
                Vector3 offset = Quaternion.Inverse(route) * (absolutePosition - frame.Position);
                Vector3 euler = (Quaternion.Inverse(route) * absoluteRotation).eulerAngles;
                Require(Vector3.Distance(frame.Position + route * offset, absolutePosition) < .001f &&
                    Quaternion.Angle(route * Quaternion.Euler(euler), absoluteRotation) < .05f, "Route conversion round trip failed: " + Text(p, "id"));
                return new AAAExemplar.Placement { semanticGroup = Text(p, "id"), prefab = prefabs[Text(p, "assetId")],
                    progress = progress, followBank = banked, offset = offset, rotationDegrees = euler, scale = Vector(p, "scale").x,
                    replacementVolume = new Bounds(Vector3.zero, Vector3.zero), replaceRendererNames = new string[0], replaceLightNames = new string[0] };
            }).ToArray();
            if (atomicGallery)
            {
                recipe.startProgress = AAABaselineIntegration.WarmGalleryStart;
                recipe.endProgress = AAABaselineIntegration.WarmGalleryEnd;
                recipe.rendererReplacements = NightTrackLighting.ReplaceableWarmRendererNames.Select(name =>
                    new AAAExemplar.ReplacementTarget { name = name, expectedCount = 1 }).ToArray();
                recipe.lightReplacements = new[] {
                    new AAAExemplar.ReplacementTarget { name = NightTrackLighting.WarmSurfaceWash, expectedCount = NightTrackLighting.WarmSurfaceWashCount },
                    new AAAExemplar.ReplacementTarget { name = NightTrackLighting.WarmRoadPool, expectedCount = NightTrackLighting.WarmRoadPoolCount }
                };
                recipe.lights = ReadLightFixtures(lighting);
            }
            if (!atomicGallery)
            {
                recipe.startProgress = recipe.placements.Min(p => p.progress);
                recipe.endProgress = recipe.placements.Max(p => p.progress);
                if (recipe.startProgress == recipe.endProgress)
                { recipe.startProgress = Mathf.Max(0, recipe.startProgress - .0001f); recipe.endProgress = Mathf.Min(1, recipe.endProgress + .0001f); }
            }
            return recipe;
        }

        static AAAExemplar.LightFixture[] ReadLightFixtures(JObject lighting)
        {
            return Array(lighting, "fixtures").Select(fixture =>
            {
                string type = Text(fixture, "type"); Vector3 position = Vector(fixture, "position");
                Vector3 direction = Vector3.zero;
                if (type == "spot") direction = (fixture["aimDirection"] != null ? Vector(fixture, "aimDirection") :
                    Vector(fixture, "target") - position).normalized;
                string shadows = OptionalText(fixture, "shadows", "none");
                return new AAAExemplar.LightFixture {
                    id = Text(fixture, "id"), type = type == "spot" ? LightType.Spot : LightType.Point,
                    worldPosition = position, worldDirection = direction, color = ColorValue(fixture, "rgb"),
                    intensity = Number(fixture, "intensity"), range = Number(fixture, "range"),
                    outerAngle = type == "spot" ? Number(fixture, "outerAngle") : 0,
                    innerAngle = type == "spot" ? Number(fixture, "innerAngle") : 0,
                    shadows = shadows == "soft" ? LightShadows.Soft : shadows == "hard" ? LightShadows.Hard : LightShadows.None,
                    housingSemanticGroup = OptionalText(fixture, "housingInstanceId", OptionalText(fixture, "sourcePlacement", null)),
                    socket = OptionalText(fixture, "socket", OptionalText(fixture, "sourcePart", null))
                };
            }).ToArray();
        }

        static void ValidatePrepared(AAAExemplar recipe)
        {
            string issue = AAABaselineIntegration.Validate(recipe);
            Require(issue == null, "Recipe validation failed: " + issue);
            foreach (var p in recipe.placements)
            {
                Require(AssetDatabase.Contains(p.prefab), "Nonpersistent prefab " + p.semanticGroup);
                foreach (var filter in p.prefab.GetComponentsInChildren<MeshFilter>(true))
                    Require(AssetDatabase.Contains(filter.sharedMesh) && filter.sharedMesh.HasVertexAttribute(VertexAttribute.Normal) &&
                        filter.sharedMesh.HasVertexAttribute(VertexAttribute.TexCoord0) && filter.sharedMesh.HasVertexAttribute(VertexAttribute.Tangent),
                        "Persistent mesh/normals/UV0/tangents required: " + filter.name);
                foreach (var renderer in p.prefab.GetComponentsInChildren<MeshRenderer>(true))
                    foreach (var material in renderer.sharedMaterials)
                    {
                        Require(AssetDatabase.Contains(material), "Nonpersistent material");
                        foreach (string property in new[] { "_BaseMap", "_MetallicGlossMap", "_BumpMap" })
                        {
                            if (!material.GetTexture(property)) continue;
                            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(material.GetTexture(property))) as TextureImporter;
                            Require(importer && importer.mipmapEnabled && (property == "_BumpMap" ? importer.textureType == TextureImporterType.NormalMap :
                                importer.sRGBTexture == (property == "_BaseMap")), "Texture contract failed: " + material.name + "/" + property);
                        }
                    }
            }
        }

        static string AbsoluteFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            Require(args.Count(a => a == flag) == 1, "Require exactly one " + flag + " <absolute path>");
            int i = System.Array.IndexOf(args, flag);
            Require(i + 1 < args.Length && Path.IsPathRooted(args[i + 1]), flag + " requires an absolute path");
            return Path.GetFullPath(args[i + 1]).TrimEnd(Path.DirectorySeparatorChar);
        }
        static bool Exists(string path) => File.Exists(path) || Directory.Exists(path) || File.Exists(path + ".meta");
        static bool Overlap(string a, string b) => a.Equals(b, StringComparison.OrdinalIgnoreCase) ||
            a.StartsWith(b.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase) || b.StartsWith(a.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase);
        static void NoLinks(string path)
        {
            for (string p = Path.GetFullPath(path); !string.IsNullOrEmpty(p); p = Path.GetDirectoryName(p))
                if (File.Exists(p) || Directory.Exists(p)) Require((File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0, "Symlink/reparse path rejected: " + p);
        }
        static string FilePath(string package, string relative)
        {
            Require(!string.IsNullOrWhiteSpace(relative) && !Path.IsPathRooted(relative) && !relative.Contains("\\") && !relative.Contains(":"), "Invalid package path: " + relative);
            Require(relative.Split('/').All(s => s.Length > 0 && s != "." && s != ".."), "Traversal/empty path component: " + relative);
            string path = Path.Combine(package, relative);
            NoLinks(path); Require(File.Exists(path), "Missing package file: " + path);
            return path;
        }
        static void FreshAssetPath(string path)
        {
            NoLinks(Path.GetFullPath(path));
            Require(!Exists(path) && string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)), "Refusing existing destination/resource: " + path);
        }
        static void MakeFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            MakeFolder(parent);
            Require(!string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, Path.GetFileName(path))), "Cannot create asset folder: " + path);
        }
        static void Covered(JObject sums, string file) => Require(sums[file]?.Type == JTokenType.String, "File missing from checksums: " + file);
        static void VerifyChecksums(string package, JObject sums)
        {
            Require(sums.Count > 0, "Empty checksums");
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in sums.Properties())
            {
                string path = FilePath(package, entry.Name);
                Require(unique.Add(entry.Name), "Case-colliding package paths: " + entry.Name);
                Require(entry.Value.Type == JTokenType.String && Regex.IsMatch((string)entry.Value, "^[a-f0-9]{64}$") && Hash(path) == (string)entry.Value, "Checksum mismatch: " + entry.Name);
            }
        }
        static void CopyAsset(string package, string root, string file, JObject sums)
        {
            string source = FilePath(package, file), destination = root + "/" + file;
            Require(Hash(source) == Text(sums, file), "Source changed: " + file);
            FreshAssetPath(destination); MakeFolder(Path.GetDirectoryName(destination).Replace('\\', '/'));
            File.Copy(source, destination, false);
            Require(Hash(destination) == Text(sums, file), "Copied bytes differ: " + file);
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }
        static string Hash(string path)
        { using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
        static string LegacyJsonCourseHash(TrackPath track)
        {
            track.Ensure(); var course = new LegacyCourse { width = track.Width, length = track.Length, frames = new LegacyFrame[1201] };
            for (int i = 0; i <= 1200; i++)
            {
                float progress = i / 1200f; TrackFrame frame = track.Evaluate(progress);
                course.frames[i] = new LegacyFrame { progress = progress, distanceMeters = progress * track.Length,
                    position = Floats(frame.Position), forward = Floats(frame.Forward), right = Floats(frame.Right), up = Floats(frame.Up) };
            }
            byte[] bytes = Utf8.GetBytes(JsonUtility.ToJson(course));
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
        static JObject ReadJson(string path)
        {
            var result = JObject.Parse(File.ReadAllText(path), new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
            foreach (var value in result.Descendants().OfType<JValue>())
                if (value.Type == JTokenType.Float) Require(!double.IsNaN(value.Value<double>()) && !double.IsInfinity(value.Value<double>()), "Nonfinite JSON value: " + path + "/" + value.Path);
            return result;
        }
        static string Text(JToken o, string key)
        { Require(o?[key]?.Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)o[key]), "Missing string " + key); return (string)o[key]; }
        static string OptionalText(JToken o, string key, string fallback)
        { if (o?[key] == null) return fallback; Require(o[key].Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)o[key]), "Invalid optional string " + key); return (string)o[key]; }
        static string SafeId(JToken o)
        { string id = Text(o, "id"); Require(Regex.IsMatch(id, "^[A-Za-z0-9_-]+$"), "Unsafe source ID " + id); return id; }
        static bool Bool(JToken o, string key)
        { Require(o?[key]?.Type == JTokenType.Boolean, "Missing boolean " + key); return (bool)o[key]; }
        static JArray Array(JToken o, string key)
        { Require(o?[key] is JArray, "Missing array " + key); return (JArray)o[key]; }
        static string[] Strings(JToken o, string key)
        { var array = Array(o, key); Require(array.All(v => v.Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)v)), "Invalid strings " + key); return array.Select(v => (string)v).ToArray(); }
        static float Scalar(JToken value)
        { Require(value != null && (value.Type == JTokenType.Float || value.Type == JTokenType.Integer), "Numeric value required"); float v = (float)value; Require(!float.IsNaN(v) && !float.IsInfinity(v), "Nonfinite numeric value"); return v; }
        static float Number(JToken o, string key) => Scalar(o?[key]);
        static float Range(JToken o, string key, float min, float max)
        { float v = Number(o, key); Require(v >= min && v <= max, "Out of range " + key); return v; }
        static float[] Numbers(JToken o, string key, int count)
        { var a = Array(o, key); Require(a.Count == count, "Wrong vector length " + key); return a.Select(Scalar).ToArray(); }
        static Vector3 Vector(JToken o, string key)
        { float[] a = Numbers(o, key, 3); return new Vector3(a[0], a[1], a[2]); }
        static Color ColorValue(JToken o, string key)
        { float[] a = Numbers(o, key, 3); Require(a.All(v => v >= 0 && v <= 1), "Invalid linear color " + key); return new Color(a[0], a[1], a[2], 1); }
        static Quaternion Rotation(JToken o)
        { float[] a = Numbers(o, "rotation", 4); float n = a.Sum(v => v * v); Require(Mathf.Abs(n - 1) < .0001f, "Rotation must be a unit quaternion"); return new Quaternion(a[0], a[1], a[2], a[3]).normalized; }
        static Bounds ReadBounds(JToken o, string minKey, string maxKey)
        { Vector3 min = Vector(o, minKey), max = Vector(o, maxKey); Require(max.x > min.x && max.y > min.y && max.z > min.z, "Degenerate/inverted bounds"); var b = new Bounds(); b.SetMinMax(min, max); return b; }
        static float BoundsError(Bounds a, Bounds b) => Mathf.Max(MaxAbs(a.min - b.min), MaxAbs(a.max - b.max));
        static float MaxAbs(Vector3 v) => Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        static bool SameFloat(float a, float b) => BitConverter.ToInt32(BitConverter.GetBytes(a), 0) == BitConverter.ToInt32(BitConverter.GetBytes(b), 0);
        static bool SameVector(Vector3 a, Vector3 b) => SameFloat(a.x, b.x) && SameFloat(a.y, b.y) && SameFloat(a.z, b.z);
        static bool Finite(Vector3 v) => !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
        static JArray V(Vector3 v) => new JArray(v.x, v.y, v.z);
        static float[] Floats(Vector3 v) => new[] { v.x, v.y, v.z };
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
