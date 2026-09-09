using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush.Editor
{
    public sealed class ValidatedProductionPackage
    {
        public string root;
        public ProductionArtManifest manifest;
        public ProductionLayout layout;
        public ProductionLighting lighting;
        public ProductionTrackProfile trackProfile;
        public ProductionChecksums checksums;
        public ProductionReady ready;
    }

    public sealed class ProductionImportResult
    {
        public string revision;
        public string artAssetRoot;
        public string payloadAssetRoot;
        public string importedAssetRoot;
        public string worldAssetRoot;
        public ValidatedProductionPackage package;
    }

    public static class ProductionArtImporter
    {
        const string StagingPrefix = "ImportStaging-";

        public static ValidatedProductionPackage ValidatePackage(string absolutePackageDirectory, string expectedCourseHash)
        {
            if (string.IsNullOrWhiteSpace(absolutePackageDirectory) || !Path.IsPathRooted(absolutePackageDirectory))
                throw new ArgumentException("Production art package must be an absolute directory", nameof(absolutePackageDirectory));
            string root = Path.GetFullPath(absolutePackageDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("Production art package does not exist: " + root);
            foreach (string required in new[] { "manifest.json", "layout.json", "lighting.json", "track-profile.json", "asset-stats.json", "checksums.json", "READY.json" })
                if (!File.Exists(Path.Combine(root, required))) throw new InvalidDataException("Missing required production package file: " + required);

            ProductionReady ready = Read<ProductionReady>(root, "READY.json");
            ProductionChecksums checksums = Read<ProductionChecksums>(root, "checksums.json");
            ProductionArtManifest manifest = Read<ProductionArtManifest>(root, "manifest.json");
            ProductionLayout layout = Read<ProductionLayout>(root, "layout.json");
            ProductionLighting lighting = Read<ProductionLighting>(root, "lighting.json");
            ProductionTrackProfile trackProfile = ReadTrackProfile(root);

            RequireVersion(ready.contractVersion, "READY.json");
            RequireVersion(checksums.contractVersion, "checksums.json");
            RequireVersion(manifest.contractVersion, "manifest.json");
            RequireVersion(layout.contractVersion, "layout.json");
            RequireVersion(lighting.contractVersion, "lighting.json");
            RequireVersion(trackProfile.contractVersion, "track-profile.json");
            RequireHash(expectedCourseHash, "expected course hash");
            RequireHash(manifest.courseHash, "manifest course hash");
            RequireHash(layout.courseHash, "layout course hash");
            RequireHash(lighting.courseHash, "lighting course hash");
            RequireHash(trackProfile.courseHash, "track-profile course hash");
            RequireHash(ready.courseHash, "READY course hash");
            if (manifest.courseHash != expectedCourseHash || layout.courseHash != expectedCourseHash || lighting.courseHash != expectedCourseHash || trackProfile.courseHash != expectedCourseHash || ready.courseHash != expectedCourseHash)
                throw new InvalidDataException("Production package course hash does not match the selected geometry context");
            RequireId(manifest.revision, "manifest revision");
            if (ready.revision != manifest.revision || checksums.revision != manifest.revision || layout.revision != manifest.revision || lighting.revision != manifest.revision)
                throw new InvalidDataException("Production package revision identities do not match");

            string checksumsPath = Path.Combine(root, "checksums.json");
            string actualChecksumsHash = Hash(File.ReadAllBytes(checksumsPath));
            if (!string.Equals(actualChecksumsHash, ready.checksumsSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("READY checksums SHA-256 does not match checksums.json");
            ValidateChecksums(root, checksums);
            ValidateManifest(root, manifest);
            ValidateLayout(layout, manifest);
            ValidateLighting(lighting, manifest, layout);
            ValidateTrackProfile(trackProfile, manifest);

            return new ValidatedProductionPackage { root = root, manifest = manifest, layout = layout, lighting = lighting, trackProfile = trackProfile, checksums = checksums, ready = ready };
        }

        public static ProductionImportResult ImportPackage(
            string absolutePackageDirectory,
            string expectedCourseHash,
            string artDestinationRoot = "Assets/Art/NocturneProduction/Imported",
            string worldDestinationRoot = "Assets/World/NocturneProduction")
        {
            // Validation intentionally precedes every AssetDatabase or filesystem mutation in
            // the destination. A corrupt READY package cannot disturb the last usable import.
            ValidatedProductionPackage package = ValidatePackage(absolutePackageDirectory, expectedCourseHash);
            string artRoot = ValidateAssetRoot(artDestinationRoot, nameof(artDestinationRoot));
            string worldRoot = ValidateAssetRoot(worldDestinationRoot, nameof(worldDestinationRoot));
            string artFinal = artRoot + "/" + package.manifest.revision;
            string worldFinal = worldRoot + "/" + package.manifest.revision;
            if (AssetDatabase.IsValidFolder(artFinal) || AssetDatabase.IsValidFolder(worldFinal))
                throw new IOException("The immutable production revision has already been imported: " + package.manifest.revision);

            EnsureAssetFolder(artRoot);
            EnsureAssetFolder(worldRoot);
            string token = Guid.NewGuid().ToString("N");
            string artStage = artRoot + "/" + StagingPrefix + token;
            string worldStage = worldRoot + "/" + StagingPrefix + token;
            bool artCommitted = false;
            try
            {
                CopyPayload(package, artStage);
                EnsureAssetFolder(worldStage);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                string importedStage = artStage + "/Imported";
                ConfigurePayloadImporters(package, importedStage);
                ValidateImportedBounds(package, importedStage);
                CreatePersistentMaterialsAndPrefabs(package, importedStage, worldStage);
                AssetDatabase.SaveAssets();

                MoveAssetOrThrow(artStage, artFinal);
                artCommitted = true;
                MoveAssetOrThrow(worldStage, worldFinal);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                return new ProductionImportResult
                {
                    revision = package.manifest.revision,
                    artAssetRoot = artFinal,
                    payloadAssetRoot = artFinal + "/ImmutablePayload~",
                    importedAssetRoot = artFinal + "/Imported",
                    worldAssetRoot = worldFinal,
                    package = package
                };
            }
            catch
            {
                DeleteAssetIfPresent(artStage);
                DeleteAssetIfPresent(worldStage);
                if (artCommitted) DeleteAssetIfPresent(artFinal);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                throw;
            }
        }

        public static ProductionImportResult ResolveImportedPackage(
            string absolutePackageDirectory,
            string expectedCourseHash,
            string artDestinationRoot = "Assets/Art/NocturneProduction/Imported",
            string worldDestinationRoot = "Assets/World/NocturneProduction")
        {
            ValidatedProductionPackage package = ValidatePackage(absolutePackageDirectory, expectedCourseHash);
            string artRoot = ValidateAssetRoot(artDestinationRoot, nameof(artDestinationRoot));
            string worldRoot = ValidateAssetRoot(worldDestinationRoot, nameof(worldDestinationRoot));
            string artFinal = artRoot + "/" + package.manifest.revision;
            string worldFinal = worldRoot + "/" + package.manifest.revision;
            if (!AssetDatabase.IsValidFolder(artFinal) || !AssetDatabase.IsValidFolder(worldFinal))
                return ImportPackage(absolutePackageDirectory, expectedCourseHash, artRoot, worldRoot);
            VerifyImmutableCopy(package, artFinal + "/ImmutablePayload~");
            foreach (ProductionAssetRecord asset in package.manifest.assets)
                if (!AssetDatabase.LoadAssetAtPath<GameObject>(worldFinal + "/Prefabs/" + asset.id + ".prefab"))
                    throw new InvalidDataException("Existing imported revision is incomplete; missing prefab " + asset.id);
            foreach (ProductionMaterialRecord material in package.manifest.materials)
                if (!AssetDatabase.LoadAssetAtPath<Material>(worldFinal + "/Materials/" + material.id + ".mat"))
                    throw new InvalidDataException("Existing imported revision is incomplete; missing material " + material.id);
            return new ProductionImportResult
            {
                revision = package.manifest.revision,
                artAssetRoot = artFinal,
                payloadAssetRoot = artFinal + "/ImmutablePayload~",
                importedAssetRoot = artFinal + "/Imported",
                worldAssetRoot = worldFinal,
                package = package
            };
        }

        static void VerifyImmutableCopy(ValidatedProductionPackage package, string payloadAssetRoot)
        {
            string absolute = AssetPathToAbsolute(payloadAssetRoot);
            if (!Directory.Exists(absolute)) throw new InvalidDataException("Existing imported revision has no immutable payload copy");
            foreach (ProductionChecksumRecord entry in package.checksums.files)
            {
                string path = Path.Combine(absolute, entry.path.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path) || !string.Equals(Hash(File.ReadAllBytes(path)), entry.sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Existing imported revision payload differs from selected package: " + entry.path);
            }
        }

        static void CopyPayload(ValidatedProductionPackage package, string assetDestination)
        {
            string absoluteRoot = AssetPathToAbsolute(assetDestination);
            if (Directory.Exists(absoluteRoot)) throw new IOException("Import staging directory already exists: " + assetDestination);
            string immutableDestination = Path.Combine(absoluteRoot, "ImmutablePayload~");
            Directory.CreateDirectory(immutableDestination);
            foreach (string source in Directory.GetFiles(package.root, "*", SearchOption.AllDirectories))
            {
                string relative = Normalize(source.Substring(package.root.Length + 1));
                string destination = Path.Combine(immutableDestination, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, false);
            }

            // Unity must not attempt to invoke Blender or import evidence/recipe files from the
            // immutable byte-for-byte copy. Only declared runtime FBX/texture dependencies are
            // duplicated into the visible Imported subtree.
            string importedDestination = Path.Combine(absoluteRoot, "Imported");
            var runtimePaths = new HashSet<string>(StringComparer.Ordinal);
            foreach (ProductionAssetRecord asset in package.manifest.assets)
            {
                runtimePaths.Add(Normalize(asset.lod0));
                runtimePaths.Add(Normalize(asset.lod1));
            }
            foreach (ProductionMaterialRecord material in package.manifest.materials)
            {
                runtimePaths.Add(Normalize(material.baseColor));
                runtimePaths.Add(Normalize(material.normal));
                runtimePaths.Add(Normalize(material.metallicSmoothness));
                runtimePaths.Add(Normalize(material.occlusion));
                if (!string.IsNullOrWhiteSpace(material.emission)) runtimePaths.Add(Normalize(material.emission));
            }
            foreach (string relative in runtimePaths)
            {
                string destination = Path.Combine(importedDestination, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(Path.Combine(package.root, relative.Replace('/', Path.DirectorySeparatorChar)), destination, false);
            }
        }

        static void ConfigurePayloadImporters(ValidatedProductionPackage package, string artStage)
        {
            var lightmapPolicy = package.manifest.assets.ToDictionary(value => Normalize(value.lod0), value => value.lightmapUVs, StringComparer.Ordinal);
            foreach (ProductionAssetRecord asset in package.manifest.assets)
            {
                lightmapPolicy[Normalize(asset.lod1)] = asset.lightmapUVs;
            }
            foreach (KeyValuePair<string, string> entry in lightmapPolicy)
            {
                string path = artStage + "/" + entry.Key;
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (!importer) throw new InvalidDataException("Payload is not an importable FBX model: " + entry.Key);
                importer.globalScale = 1f;
                importer.importCameras = false;
                importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Import;
                importer.importTangents = ModelImporterTangents.CalculateMikk;
                importer.generateSecondaryUV = entry.Value == "generate";
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            var texturePolicies = new Dictionary<string, TexturePolicy>(StringComparer.Ordinal);
            foreach (ProductionMaterialRecord material in package.manifest.materials)
            {
                AddTexturePolicy(texturePolicies, material.baseColor, TextureSemantic.Color, material.uvMode);
                AddTexturePolicy(texturePolicies, material.normal, TextureSemantic.Normal, material.uvMode);
                AddTexturePolicy(texturePolicies, material.metallicSmoothness, TextureSemantic.Linear, material.uvMode);
                AddTexturePolicy(texturePolicies, material.occlusion, TextureSemantic.Linear, material.uvMode);
                if (!string.IsNullOrWhiteSpace(material.emission)) AddTexturePolicy(texturePolicies, material.emission, TextureSemantic.Color, material.uvMode);
            }
            foreach (KeyValuePair<string, TexturePolicy> entry in texturePolicies)
            {
                string path = artStage + "/" + Normalize(entry.Key);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!importer) throw new InvalidDataException("Payload is not an importable texture: " + entry.Key);
                importer.textureType = entry.Value.semantic == TextureSemantic.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = entry.Value.semantic == TextureSemantic.Color;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = false;
                importer.wrapMode = entry.Value.uvMode == "metric" ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 8;
                importer.maxTextureSize = 4096;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        static void AddTexturePolicy(Dictionary<string, TexturePolicy> policies, string relative, TextureSemantic semantic, string uvMode)
        {
            string path = Normalize(relative);
            var requested = new TexturePolicy { semantic = semantic, uvMode = uvMode };
            if (policies.TryGetValue(path, out TexturePolicy existing) && (existing.semantic != semantic || existing.uvMode != uvMode))
                throw new InvalidDataException("Texture is referenced with incompatible color-space/channel or wrap policies: " + relative);
            policies[path] = requested;
        }

        static void ValidateImportedBounds(ValidatedProductionPackage package, string artStage)
        {
            foreach (ProductionAssetRecord asset in package.manifest.assets)
            {
                ValidateImportedBounds(asset, artStage + "/" + Normalize(asset.lod0), "LOD0");
                ValidateImportedBounds(asset, artStage + "/" + Normalize(asset.lod1), "LOD1");
            }
        }

        static void ValidateImportedBounds(ProductionAssetRecord asset, string modelPath, string lod)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (!model) throw new InvalidDataException("Imported model is missing after reimport: " + modelPath);
            Bounds actual = CalculateLocalBounds(model);
            Vector3 expectedMin = ToVector3(asset.boundsMin);
            Vector3 expectedMax = ToVector3(asset.boundsMax);
            Vector3 expectedSize = expectedMax - expectedMin;
            Vector3 actualSize = actual.size;
            for (int axis = 0; axis < 3; axis++)
            {
                float tolerance = Mathf.Max(.02f, Mathf.Abs(expectedSize[axis]) * .005f);
                if (Mathf.Abs(actualSize[axis] - expectedSize[axis]) > tolerance ||
                    Mathf.Abs(actual.min[axis] - expectedMin[axis]) > tolerance ||
                    Mathf.Abs(actual.max[axis] - expectedMax[axis]) > tolerance)
                    throw new InvalidDataException(asset.id + " " + lod + " bounds/orientation mismatch on " + AxisName(axis) +
                        ": declared [" + expectedMin[axis] + ", " + expectedMax[axis] + "] imported [" + actual.min[axis] + ", " + actual.max[axis] + "] tolerance " + tolerance);
            }
        }

        static Bounds CalculateLocalBounds(GameObject root)
        {
            bool initialized = false;
            Bounds result = default;
            Matrix4x4 rootToLocal = root.transform.worldToLocalMatrix;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                EncapsulateBounds(ref result, ref initialized, filter.sharedMesh.bounds, rootToLocal * filter.transform.localToWorldMatrix);
            }
            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!renderer.sharedMesh) continue;
                EncapsulateBounds(ref result, ref initialized, renderer.sharedMesh.bounds, rootToLocal * renderer.transform.localToWorldMatrix);
            }
            if (!initialized) throw new InvalidDataException("Imported model contains no mesh geometry: " + root.name);
            return result;
        }

        static void EncapsulateBounds(ref Bounds result, ref bool initialized, Bounds bounds, Matrix4x4 matrix)
        {
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 point = matrix.MultiplyPoint3x4(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z)));
                        if (!initialized) { result = new Bounds(point, Vector3.zero); initialized = true; }
                        else result.Encapsulate(point);
                    }
        }

        static void CreatePersistentMaterialsAndPrefabs(ValidatedProductionPackage package, string artStage, string worldStage)
        {
            EnsureAssetFolder(worldStage + "/Materials");
            EnsureAssetFolder(worldStage + "/Prefabs");
            var materials = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (ProductionMaterialRecord record in package.manifest.materials)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (!shader) throw new InvalidOperationException("URP Lit shader is unavailable; production materials cannot be serialized");
                var material = new Material(shader) { name = record.id };
                material.SetColor("_BaseColor", Color.white);
                material.SetTexture("_BaseMap", LoadTexture(artStage, record.baseColor));
                material.SetTexture("_BumpMap", LoadTexture(artStage, record.normal));
                material.SetFloat("_BumpScale", record.normalScale);
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_MetallicGlossMap", LoadTexture(artStage, record.metallicSmoothness));
                material.SetFloat("_Metallic", 1f);
                material.SetFloat("_Smoothness", 1f);
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_OcclusionMap", LoadTexture(artStage, record.occlusion));
                material.SetFloat("_OcclusionStrength", 1f);
                material.EnableKeyword("_OCCLUSIONMAP");
                material.SetTextureScale("_BaseMap", Vector2.one);
                if (!string.IsNullOrWhiteSpace(record.emission))
                {
                    material.SetTexture("_EmissionMap", LoadTexture(artStage, record.emission));
                    var color = new Color(record.emissionColorLinear[0], record.emissionColorLinear[1], record.emissionColorLinear[2], 1f) * record.emissionIntensity;
                    material.SetColor("_EmissionColor", color);
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                else
                {
                    material.SetColor("_EmissionColor", Color.black);
                    material.DisableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
                AssetDatabase.CreateAsset(material, worldStage + "/Materials/" + record.id + ".mat");
                materials.Add(record.id, material);
            }

            foreach (ProductionAssetRecord asset in package.manifest.assets)
                CreatePrefab(asset, artStage, worldStage, materials);
        }

        static void CreatePrefab(ProductionAssetRecord asset, string artStage, string worldStage, Dictionary<string, Material> materials)
        {
            GameObject lod0Source = AssetDatabase.LoadAssetAtPath<GameObject>(artStage + "/" + Normalize(asset.lod0));
            GameObject lod1Source = AssetDatabase.LoadAssetAtPath<GameObject>(artStage + "/" + Normalize(asset.lod1));
            var root = new GameObject(asset.id);
            try
            {
                GameObject lod0 = UnityEngine.Object.Instantiate(lod0Source, root.transform);
                lod0.name = "LOD0";
                GameObject lod1 = UnityEngine.Object.Instantiate(lod1Source, root.transform);
                lod1.name = "LOD1";
                Renderer[] lod0Renderers = lod0.GetComponentsInChildren<Renderer>(true);
                Renderer[] lod1Renderers = lod1.GetComponentsInChildren<Renderer>(true);
                AssignMaterials(asset, lod0Renderers, materials);
                AssignMaterials(asset, lod1Renderers, materials);
                RemoveColliders(lod0);
                RemoveColliders(lod1);
                if (asset.collider == "mesh")
                {
                    foreach (MeshFilter filter in lod0.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if (!filter.sharedMesh) continue;
                        var collider = filter.gameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = filter.sharedMesh;
                    }
                }
                var group = root.AddComponent<LODGroup>();
                group.SetLODs(new[] { new LOD(.55f, lod0Renderers), new LOD(.12f, lod1Renderers) });
                group.fadeMode = LODFadeMode.CrossFade;
                group.animateCrossFading = false;
                group.RecalculateBounds();
                PrefabUtility.SaveAsPrefabAsset(root, worldStage + "/Prefabs/" + asset.id + ".prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static void AssignMaterials(ProductionAssetRecord asset, Renderer[] renderers, Dictionary<string, Material> materials)
        {
            int slot = 0;
            foreach (Renderer renderer in renderers)
            {
                Material[] assigned = renderer.sharedMaterials;
                for (int i = 0; i < assigned.Length; i++)
                {
                    string sourceName = assigned[i] ? assigned[i].name : string.Empty;
                    string exact = asset.materialSlots.FirstOrDefault(candidate => string.Equals(candidate, sourceName, StringComparison.Ordinal));
                    string id = exact ?? asset.materialSlots[Math.Min(slot, asset.materialSlots.Length - 1)];
                    assigned[i] = materials[id];
                    slot++;
                }
                renderer.sharedMaterials = assigned;
            }
            if (slot == 0) throw new InvalidDataException("Imported asset contains no renderer material slots: " + asset.id);
            if (asset.materialSlots.Length > 1 && slot != asset.materialSlots.Length)
                throw new InvalidDataException("Asset " + asset.id + " declares " + asset.materialSlots.Length + " material slots but imported renderers expose " + slot);
        }

        static void RemoveColliders(GameObject root)
        {
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
        }

        static Texture2D LoadTexture(string artStage, string relative)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(artStage + "/" + Normalize(relative));
            if (!texture) throw new InvalidDataException("Imported texture is missing after reimport: " + relative);
            return texture;
        }

        static string ValidateAssetRoot(string path, string argument)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Asset destination is required", argument);
            string normalized = Normalize(path).TrimEnd('/');
            if (normalized == "Assets" || !normalized.StartsWith("Assets/", StringComparison.Ordinal) || normalized.Contains("/../") || normalized.EndsWith("/..", StringComparison.Ordinal))
                throw new ArgumentException("Asset destination must be a project-relative child of Assets: " + path, argument);
            return normalized;
        }

        static void EnsureAssetFolder(string path)
        {
            string normalized = Normalize(path).TrimEnd('/');
            string current = "Assets";
            foreach (string part in normalized.Substring("Assets/".Length).Split('/'))
            {
                if (string.IsNullOrWhiteSpace(part) || part == "." || part == "..") throw new ArgumentException("Invalid asset folder: " + path);
                string next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, part);
                    if (string.IsNullOrWhiteSpace(guid)) throw new IOException("Unable to create asset folder: " + next);
                }
                current = next;
            }
        }

        static string AssetPathToAbsolute(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        static void MoveAssetOrThrow(string source, string destination)
        {
            string error = AssetDatabase.MoveAsset(source, destination);
            if (!string.IsNullOrEmpty(error)) throw new IOException("Unable to commit imported assets from " + source + " to " + destination + ": " + error);
        }

        static void DeleteAssetIfPresent(string path)
        {
            if (AssetDatabase.IsValidFolder(path) || AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)) AssetDatabase.DeleteAsset(path);
            else
            {
                string absolute = AssetPathToAbsolute(path);
                if (Directory.Exists(absolute)) Directory.Delete(absolute, true);
            }
        }

        static string AxisName(int axis) => axis == 0 ? "X" : axis == 1 ? "Y" : "Z";
        static Vector3 ToVector3(float[] value) => new Vector3(value[0], value[1], value[2]);

        enum TextureSemantic { Color, Normal, Linear }
        struct TexturePolicy { public TextureSemantic semantic; public string uvMode; }

        static void ValidateChecksums(string root, ProductionChecksums checksums)
        {
            if (checksums.files == null) throw new InvalidDataException("checksums.json files array is missing");
            var declared = new HashSet<string>(StringComparer.Ordinal);
            foreach (ProductionChecksumRecord record in checksums.files)
            {
                string path = SafePayloadPath(root, record.path, "checksum path");
                if (!declared.Add(Normalize(record.path))) throw new InvalidDataException("Duplicate checksum path: " + record.path);
                RequireHash(record.sha256, "checksum for " + record.path);
                if (!File.Exists(path)) throw new InvalidDataException("Missing checksummed payload: " + record.path);
                if (!string.Equals(Hash(File.ReadAllBytes(path)), record.sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Checksum mismatch for payload: " + record.path);
            }
            var actual = new HashSet<string>(Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => Normalize(path.Substring(root.Length + 1)))
                .Where(path => path != "READY.json" && path != "checksums.json"), StringComparer.Ordinal);
            if (!declared.SetEquals(actual))
            {
                string missing = string.Join(", ", actual.Except(declared).OrderBy(value => value));
                string extra = string.Join(", ", declared.Except(actual).OrderBy(value => value));
                throw new InvalidDataException("Checksum manifest does not exactly cover the payload. Missing: [" + missing + "] Extra: [" + extra + "]");
            }
        }

        static void ValidateManifest(string root, ProductionArtManifest manifest)
        {
            if (manifest.assets == null || manifest.assets.Length == 0) throw new InvalidDataException("Manifest contains no assets");
            if (manifest.materials == null || manifest.materials.Length == 0) throw new InvalidDataException("Manifest contains no materials");
            RequireFile(root, manifest.sourceBlend, "sourceBlend");
            var materialIds = Unique(manifest.materials.Select(value => value.id), "material");
            Unique(manifest.assets.Select(value => value.id), "asset");
            foreach (ProductionMaterialRecord material in manifest.materials)
            {
                RequireId(material.id, "material id");
                foreach (string path in new[] { material.baseColor, material.normal, material.metallicSmoothness, material.occlusion })
                    RequireFile(root, path, "material " + material.id);
                if (!string.IsNullOrWhiteSpace(material.emission)) RequireFile(root, material.emission, "material " + material.id + " emission");
                Vector(material.metersPerTile, 2, "material " + material.id + " metersPerTile");
                if (material.metersPerTile[0] <= 0f || material.metersPerTile[1] <= 0f) throw new InvalidDataException("Material tile size must be positive: " + material.id);
                Vector(material.emissionColorLinear, 3, "material " + material.id + " emissionColorLinear");
                Finite(material.normalScale, "material normalScale " + material.id);
                Finite(material.emissionIntensity, "material emissionIntensity " + material.id);
                if (material.uvMode != "metric" && material.uvMode != "atlas") throw new InvalidDataException("Invalid uvMode for material " + material.id);
            }
            foreach (ProductionAssetRecord asset in manifest.assets)
            {
                RequireId(asset.id, "asset id");
                RequireFile(root, asset.lod0, "asset " + asset.id + " LOD0");
                RequireFile(root, asset.lod1, "asset " + asset.id + " LOD1");
                Vector(asset.boundsMin, 3, "asset " + asset.id + " boundsMin");
                Vector(asset.boundsMax, 3, "asset " + asset.id + " boundsMax");
                for (int i = 0; i < 3; i++) if (asset.boundsMax[i] < asset.boundsMin[i]) throw new InvalidDataException("Invalid declared bounds for asset " + asset.id);
                if (asset.materialSlots == null || asset.materialSlots.Length == 0) throw new InvalidDataException("Asset has no material slots: " + asset.id);
                foreach (string slot in asset.materialSlots) if (!materialIds.Contains(slot)) throw new InvalidDataException("Asset " + asset.id + " references missing material " + slot);
                if (asset.collider != "none" && asset.collider != "mesh") throw new InvalidDataException("Invalid collider policy for asset " + asset.id);
                if (asset.lightmapUVs != "generate" && asset.lightmapUVs != "authored") throw new InvalidDataException("Invalid lightmapUVs policy for asset " + asset.id);
                if (asset.pivot != "ground-center") throw new InvalidDataException("Invalid pivot policy for asset " + asset.id + ": " + asset.pivot);
            }
        }

        static void ValidateLayout(ProductionLayout layout, ProductionArtManifest manifest)
        {
            if (layout.zones == null || layout.instances == null) throw new InvalidDataException("Layout zones and instances arrays are required");
            var zoneIds = Unique(layout.zones.Select(value => value.id), "zone");
            var assetIds = new HashSet<string>(manifest.assets.Select(value => value.id), StringComparer.Ordinal);
            Unique(layout.instances.Select(value => value.id), "instance");
            foreach (ProductionZoneRecord zone in layout.zones)
            {
                RequireId(zone.id, "zone id");Finite(zone.startProgress, "zone start");Finite(zone.endProgress, "zone end");
                if (zone.startProgress < 0f || zone.endProgress > 1f || zone.endProgress <= zone.startProgress) throw new InvalidDataException("Invalid zone range: " + zone.id);
            }
            foreach (ProductionInstanceRecord instance in layout.instances)
            {
                RequireId(instance.id, "instance id");
                if (!assetIds.Contains(instance.assetId)) throw new InvalidDataException("Instance " + instance.id + " references missing asset " + instance.assetId);
                if (!zoneIds.Contains(instance.zoneId)) throw new InvalidDataException("Instance " + instance.id + " references missing zone " + instance.zoneId);
                Vector(instance.position, 3, "instance " + instance.id + " position");
                Vector(instance.rotation, 4, "instance " + instance.id + " rotation");
                float norm = Mathf.Sqrt(instance.rotation.Sum(value => value * value));
                if (Mathf.Abs(norm - 1f) > .001f) throw new InvalidDataException("Instance rotation must be a unit quaternion: " + instance.id);
                Vector(instance.scale, 3, "instance " + instance.id + " scale");
                if (instance.scale.Any(value => value <= 0f || Mathf.Abs(value - 1f) > .001f)) throw new InvalidDataException("Instance must use unit positive scale: " + instance.id);
                if (instance.frameMode != "upright" && instance.frameMode != "banked") throw new InvalidDataException("Invalid frameMode for instance " + instance.id);
                if (instance.role != "near" && instance.role != "middle" && instance.role != "far") throw new InvalidDataException("Invalid role for instance " + instance.id);
                if (instance.gi != "lightmap" && instance.gi != "probe" && instance.gi != "none") throw new InvalidDataException("Invalid GI policy for instance " + instance.id);
            }
        }

        static void ValidateLighting(ProductionLighting lighting, ProductionArtManifest manifest, ProductionLayout layout)
        {
            if (lighting.environment == null || lighting.lights == null || lighting.reflectionVolumes == null) throw new InvalidDataException("Lighting environment, lights and reflectionVolumes are required");
            Vector(lighting.environment.fogColorLinear, 3, "fogColorLinear");
            Vector(lighting.environment.ambientTintLinear, 3, "ambientTintLinear");
            Finite(lighting.environment.fogDensity, "fogDensity");Finite(lighting.environment.exposureEV, "exposureEV");
            if (lighting.environment.fogDensity < 0f) throw new InvalidDataException("Fog density cannot be negative");
            if (lighting.environment.skyMaterialId != "production-night-sky" && !manifest.materials.Any(value => value.id == lighting.environment.skyMaterialId))
                throw new InvalidDataException("Environment references missing sky material " + lighting.environment.skyMaterialId);
            var instanceIds = new HashSet<string>(layout.instances.Select(value => value.id), StringComparer.Ordinal);
            Unique(lighting.lights.Select(value => value.id), "light");
            Unique(lighting.reflectionVolumes.Select(value => value.id), "reflection volume");
            foreach (ProductionLightRecord light in lighting.lights)
            {
                RequireId(light.id, "light id");
                if (!instanceIds.Contains(light.fixtureInstanceId)) throw new InvalidDataException("Light " + light.id + " references missing fixture instance " + light.fixtureInstanceId);
                Vector(light.position, 3, "light position " + light.id);Vector(light.rotation, 4, "light rotation " + light.id);Vector(light.colorLinear, 3, "light color " + light.id);
                float norm = Mathf.Sqrt(light.rotation.Sum(value => value * value));if (Mathf.Abs(norm - 1f) > .001f) throw new InvalidDataException("Light rotation must be a unit quaternion: " + light.id);
                foreach (float value in new[] { light.intensity, light.range, light.spotOuterDegrees, light.spotInnerDegrees }) Finite(value, "light value " + light.id);
                if (light.intensity < 0f || light.range < 0f) throw new InvalidDataException("Light intensity/range cannot be negative: " + light.id);
                if (light.type != "spot" && light.type != "point" && light.type != "directional") throw new InvalidDataException("Invalid light type: " + light.id);
                if (light.mode != "baked" && light.mode != "mixed" && light.mode != "realtime") throw new InvalidDataException("Invalid light mode: " + light.id);
            }
            foreach (ProductionReflectionVolumeRecord probe in lighting.reflectionVolumes)
            {
                RequireId(probe.id, "reflection volume id");Vector(probe.position, 3, "reflection volume position " + probe.id);Vector(probe.size, 3, "reflection volume size " + probe.id);
                if (probe.size.Any(value => value <= 0f)) throw new InvalidDataException("Reflection volume size must be positive: " + probe.id);
                if (probe.resolution != 128 && probe.resolution != 256) throw new InvalidDataException("Reflection volume resolution must be 128 or 256: " + probe.id);
                Finite(probe.intensity, "reflection volume intensity " + probe.id);if (probe.intensity < 0f) throw new InvalidDataException("Reflection volume intensity cannot be negative: " + probe.id);
            }
        }

        static void ValidateTrackProfile(ProductionTrackProfile profile, ProductionArtManifest manifest)
        {
            if (profile.strips == null) throw new InvalidDataException("Track profile strips array is missing");
            var materialIds = new HashSet<string>(manifest.materials.Select(value => value.id), StringComparer.Ordinal);
            Unique(profile.strips.Select(value => value.id), "track strip");
            foreach (ProductionTrackStripRecord strip in profile.strips)
            {
                RequireId(strip.id, "track strip id");if (!materialIds.Contains(strip.materialId)) throw new InvalidDataException("Track strip references missing material: " + strip.id);
                if (strip.pointsXY == null || strip.pointsXY.Length < 4 || strip.pointsXY.Length % 2 != 0) throw new InvalidDataException("Track strip pointsXY must contain at least two XY points: " + strip.id);
                foreach (float value in strip.pointsXY) Finite(value, "track strip point " + strip.id);
                Finite(strip.uvMetersPerTile, "track strip UV scale " + strip.id);if (strip.uvMetersPerTile <= 0f) throw new InvalidDataException("Track strip uvMetersPerTile must be positive: " + strip.id);
                if (strip.twoSided) throw new InvalidDataException("Track strip twoSided must be false: " + strip.id);
                if (!new[] { "outerBarrier", "underside", "fascia", "drain", "ledge" }.Contains(strip.surfaceRole)) throw new InvalidDataException("Invalid track strip surfaceRole: " + strip.id);
            }
        }

        static T Read<T>(string root, string relative)
        {
            try
            {
                T value = JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(root, relative)));
                if (value == null) throw new InvalidDataException("JSON root is null");
                return value;
            }
            catch (Exception error) when (!(error is InvalidDataException))
            {
                throw new InvalidDataException("Invalid JSON in " + relative + ": " + error.Message, error);
            }
        }

        static ProductionTrackProfile ReadTrackProfile(string root)
        {
            const string relative = "track-profile.json";
            try
            {
                string json = File.ReadAllText(Path.Combine(root, relative));
                ProductionTrackProfile value = JsonUtility.FromJson<ProductionTrackProfile>(FlattenNestedPointArrays(json));
                if (value == null) throw new InvalidDataException("JSON root is null");
                return value;
            }
            catch (Exception error) when (!(error is InvalidDataException))
            {
                throw new InvalidDataException("Invalid JSON in " + relative + ": " + error.Message, error);
            }
        }

        static string FlattenNestedPointArrays(string json)
        {
            const string key = "\"pointsXY\"";
            var output = new StringBuilder(json.Length);
            int cursor = 0;
            while (true)
            {
                int keyIndex = json.IndexOf(key, cursor, StringComparison.Ordinal);
                if (keyIndex < 0) { output.Append(json, cursor, json.Length - cursor); break; }
                int colon = json.IndexOf(':', keyIndex + key.Length);
                int outer = colon < 0 ? -1 : json.IndexOf('[', colon + 1);
                if (outer < 0) throw new InvalidDataException("pointsXY must be a JSON array");
                int first = outer + 1;
                while (first < json.Length && char.IsWhiteSpace(json[first])) first++;
                if (first >= json.Length || json[first] != '[')
                {
                    output.Append(json, cursor, first - cursor);
                    cursor = first;
                    continue;
                }
                output.Append(json, cursor, outer - cursor + 1);
                int depth = 1;
                bool closed = false;
                for (int i = outer + 1; i < json.Length; i++)
                {
                    char character = json[i];
                    if (character == '[')
                    {
                        depth++;
                        if (depth > 2) throw new InvalidDataException("pointsXY entries must each be a two-number array");
                        continue;
                    }
                    if (character == ']')
                    {
                        if (depth == 2) { depth--; continue; }
                        depth--;
                        if (depth == 0)
                        {
                            output.Append(']');
                            cursor = i + 1;
                            closed = true;
                            break;
                        }
                    }
                    else output.Append(character);
                }
                if (!closed) throw new InvalidDataException("pointsXY array is not closed");
            }
            return output.ToString();
        }

        static HashSet<string> Unique(IEnumerable<string> ids, string kind)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in ids)
            {
                RequireId(id, kind + " id");
                if (!result.Add(id)) throw new InvalidDataException("Duplicate " + kind + " id: " + id);
            }
            return result;
        }

        static void RequireFile(string root, string relative, string label)
        {
            string path = SafePayloadPath(root, relative, label);
            if (!File.Exists(path)) throw new InvalidDataException("Missing " + label + " payload: " + relative);
        }

        static string SafePayloadPath(string root, string relative, string label)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative)) throw new InvalidDataException(label + " must be a relative path inside the package: " + relative);
            if (relative.IndexOf('\\') >= 0) throw new InvalidDataException(label + " must use forward-slash package paths: " + relative);
            string full = Path.GetFullPath(Path.Combine(root, relative));
            string prefix = root + Path.DirectorySeparatorChar;
            if (!full.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidDataException(label + " must stay inside the package: " + relative);
            return full;
        }

        static string Normalize(string path) => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        static void RequireVersion(int version, string label) { if (version != 1) throw new InvalidDataException(label + " uses unsupported contract version " + version); }
        static void RequireHash(string hash, string label) { if (string.IsNullOrWhiteSpace(hash) || hash.Length != 64 || hash.Any(value => !Uri.IsHexDigit(value))) throw new InvalidDataException(label + " must be a SHA-256 hex value"); }
        static void RequireId(string id, string label) { if (string.IsNullOrWhiteSpace(id) || id.Any(value => value > 127 || !(char.IsLetterOrDigit(value) || value == '_' || value == '-'))) throw new InvalidDataException(label + " must be non-empty case-sensitive ASCII letters, digits, '_' or '-': " + id); }
        static void Vector(float[] values, int length, string label) { if (values == null || values.Length != length) throw new InvalidDataException(label + " must contain " + length + " finite values");foreach (float value in values) Finite(value, label); }
        static void Finite(float value, string label) { if (float.IsNaN(value) || float.IsInfinity(value)) throw new InvalidDataException(label + " contains NaN or Infinity"); }

        static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var value = new StringBuilder(64);
                foreach (byte item in sha.ComputeHash(bytes)) value.Append(item.ToString("x2"));
                return value.ToString();
            }
        }
    }
}
