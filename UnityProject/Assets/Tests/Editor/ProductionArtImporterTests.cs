using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VectorRush.Editor;

namespace VectorRush.Tests
{
    public sealed class ProductionArtImporterTests
    {
        readonly List<string> createdAssetRoots = new List<string>();

        [TearDown]
        public void DeleteImportedTestAssets()
        {
            foreach (string root in createdAssetRoots.OrderByDescending(value => value.Length))
                if (AssetDatabase.IsValidFolder(root) || AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(root))
                    AssetDatabase.DeleteAsset(root);
            createdAssetRoots.Clear();
            foreach (string folder in new[]
                     {
                         "Assets/Art/NocturneProduction/Tests",
                         "Assets/World/NocturneProduction/Tests",
                         "Assets/Settings/NocturneProduction/Tests",
                         "Assets/Scenes/NocturneProductionTests"
                     })
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", folder));
                if (Directory.GetFileSystemEntries(absolute).Length == 0) AssetDatabase.DeleteAsset(folder);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        [Test]
        public void ImporterAndJsonContractsAreAvailableToTheEditor()
        {
            Assert.That(FindEditorType("VectorRush.Editor.ProductionArtManifest"), Is.Not.Null);
            Assert.That(FindEditorType("VectorRush.Editor.ProductionLayout"), Is.Not.Null);
            Assert.That(FindEditorType("VectorRush.Editor.ProductionLighting"), Is.Not.Null);
            Assert.That(FindEditorType("VectorRush.Editor.ProductionTrackProfile"), Is.Not.Null);
            Type importer = FindEditorType("VectorRush.Editor.ProductionArtImporter");
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.GetMethod("ValidatePackage", BindingFlags.Static | BindingFlags.Public), Is.Not.Null);
        }

        [Test]
        public void CompleteImmutablePackageValidatesBeforeImport()
        {
            string package = NewPackage();
            try
            {
                ValidatedProductionPackage result = ProductionArtImporter.ValidatePackage(package, CourseHash);
                Assert.That(result.manifest.revision, Is.EqualTo("art-package-diagnostic-01"));
                Assert.That(result.layout.instances, Has.Length.EqualTo(1));
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void ContractNestedTrackProfilePointsAreAcceptedAndFlattened()
        {
            string package = NewPackage();
            string path = Path.Combine(package, "track-profile.json");
            File.WriteAllText(path, "{\"contractVersion\":1,\"courseHash\":\"" + CourseHash + "\",\"strips\":[{\"id\":\"DiagnosticFascia\",\"materialId\":\"DiagnosticMaterial\",\"pointsXY\":[[-13,-1],[-13,0]],\"surfaceRole\":\"fascia\",\"uvMetersPerTile\":1,\"twoSided\":false}]}");
            RewriteIntegrityFiles(package);
            try
            {
                ValidatedProductionPackage result = ProductionArtImporter.ValidatePackage(package, CourseHash);
                Assert.That(result.trackProfile.strips[0].pointsXY, Is.EqualTo(new[] { -13f, -1f, -13f, 0f }));
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void TraversalPathIsRejectedEvenWhenTheEscapedFileExists()
        {
            string package = NewPackage(manifest => manifest.assets[0].lod0 = "../escaped.fbx");
            File.WriteAllText(Path.Combine(package, "..", "escaped.fbx"), "outside");
            try
            {
                var error = Assert.Throws<InvalidDataException>(() => ProductionArtImporter.ValidatePackage(package, CourseHash));
                Assert.That(error.Message, Does.Contain("inside the package"));
            }
            finally
            {
                File.Delete(Path.Combine(package, "..", "escaped.fbx"));
                Directory.Delete(package, true);
            }
        }

        [Test]
        public void DuplicateMaterialIdIsRejected()
        {
            string package = NewPackage(manifest => manifest.materials = new[] { manifest.materials[0], manifest.materials[0] });
            try
            {
                var error = Assert.Throws<InvalidDataException>(() => ProductionArtImporter.ValidatePackage(package, CourseHash));
                Assert.That(error.Message, Does.Contain("Duplicate material id"));
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void NonUnitPlacementScaleIsRejected()
        {
            string package = NewPackage(null, layout => layout.instances[0].scale = new[] { 1f, .9f, 1f });
            try
            {
                var error = Assert.Throws<InvalidDataException>(() => ProductionArtImporter.ValidatePackage(package, CourseHash));
                Assert.That(error.Message, Does.Contain("unit positive scale"));
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void PayloadMutationAfterReadyIsRejectedByChecksum()
        {
            string package = NewPackage();
            File.AppendAllText(Path.Combine(package, "meshes", "Diagnostic_LOD0.fbx"), "changed");
            try
            {
                var error = Assert.Throws<InvalidDataException>(() => ProductionArtImporter.ValidatePackage(package, CourseHash));
                Assert.That(error.Message, Does.Contain("Checksum mismatch"));
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void InvalidPackageDoesNotMutateExistingImportDestinations()
        {
            string package = NewPackage();
            string token = Guid.NewGuid().ToString("N");
            string artRoot = "Assets/Art/NocturneProduction/Tests/" + token;
            string worldRoot = "Assets/World/NocturneProduction/Tests/" + token;
            CreateAssetFolder(artRoot);
            CreateAssetFolder(worldRoot);
            createdAssetRoots.Add(artRoot);
            createdAssetRoots.Add(worldRoot);
            string sentinelPath = Path.Combine(Application.dataPath, artRoot.Substring("Assets/".Length), "sentinel.txt");
            File.WriteAllText(sentinelPath, "preserve-me");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string[] before = Directory.GetFiles(Path.GetDirectoryName(sentinelPath), "*", SearchOption.AllDirectories)
                .Select(path => path.Substring(Application.dataPath.Length - "Assets".Length).Replace(Path.DirectorySeparatorChar, '/'))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            File.AppendAllText(Path.Combine(package, "meshes", "Diagnostic_LOD0.fbx"), "changed-after-ready");
            try
            {
                Assert.Throws<InvalidDataException>(() => ProductionArtImporter.ImportPackage(package, CourseHash, artRoot, worldRoot));
                string[] after = Directory.GetFiles(Path.GetDirectoryName(sentinelPath), "*", SearchOption.AllDirectories)
                    .Select(path => path.Substring(Application.dataPath.Length - "Assets".Length).Replace(Path.DirectorySeparatorChar, '/'))
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                Assert.That(after, Is.EqualTo(before));
                Assert.That(File.ReadAllText(sentinelPath), Is.EqualTo("preserve-me"));
                Assert.That(Directory.GetDirectories(Path.Combine(Application.dataPath, worldRoot.Substring("Assets/".Length))), Is.Empty);
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void ImportCopiesImmutablePayloadAndCreatesPersistentConfiguredAssets()
        {
            string package = NewImportablePackage();
            string token = Guid.NewGuid().ToString("N");
            string artRoot = "Assets/Art/NocturneProduction/Tests/" + token;
            string worldRoot = "Assets/World/NocturneProduction/Tests/" + token;
            createdAssetRoots.Add(artRoot);
            createdAssetRoots.Add(worldRoot);
            try
            {
                ProductionImportResult result = ProductionArtImporter.ImportPackage(package, CourseHash, artRoot, worldRoot);
                Assert.That(result.revision, Is.EqualTo("art-package-diagnostic-01"));
                Assert.That(result.artAssetRoot, Is.EqualTo(artRoot + "/art-package-diagnostic-01"));
                Assert.That(result.worldAssetRoot, Is.EqualTo(worldRoot + "/art-package-diagnostic-01"));

                foreach (ProductionChecksumRecord entry in result.package.checksums.files)
                {
                    string source = Path.Combine(package, entry.path);
                    string copied = Path.Combine(Application.dataPath, result.payloadAssetRoot.Substring("Assets/".Length), entry.path);
                    Assert.That(File.Exists(copied), Is.True, entry.path);
                    Assert.That(Hash(File.ReadAllBytes(copied)), Is.EqualTo(Hash(File.ReadAllBytes(source))), entry.path);
                }

                string modelPath = result.importedAssetRoot + "/meshes/Diagnostic_LOD0.fbx";
                var modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
                Assert.That(modelImporter, Is.Not.Null);
                Assert.That(modelImporter.globalScale, Is.EqualTo(1f));
                Assert.That(modelImporter.importCameras, Is.False);
                Assert.That(modelImporter.importLights, Is.False);
                Assert.That(modelImporter.generateSecondaryUV, Is.True);

                string textureRoot = result.importedAssetRoot + "/textures/Diagnostic_";
                var baseImporter = AssetImporter.GetAtPath(textureRoot + "BaseColor.png") as TextureImporter;
                var normalImporter = AssetImporter.GetAtPath(textureRoot + "Normal.png") as TextureImporter;
                var maskImporter = AssetImporter.GetAtPath(textureRoot + "MetallicSmoothness.png") as TextureImporter;
                Assert.That(baseImporter.sRGBTexture, Is.True);
                Assert.That(normalImporter.textureType, Is.EqualTo(TextureImporterType.NormalMap));
                Assert.That(maskImporter.sRGBTexture, Is.False);

                string materialPath = result.worldAssetRoot + "/Materials/DiagnosticMaterial.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                Assert.That(material, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(material.GetTexture("_BaseMap")), Is.EqualTo(textureRoot + "BaseColor.png"));
                Assert.That(AssetDatabase.GetAssetPath(material.GetTexture("_BumpMap")), Is.EqualTo(textureRoot + "Normal.png"));
                Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.True);
                Assert.That(material.IsKeywordEnabled("_METALLICSPECGLOSSMAP"), Is.True);
                Assert.That(material.IsKeywordEnabled("_OCCLUSIONMAP"), Is.True);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(result.worldAssetRoot + "/Prefabs/Diagnostic.prefab"), Is.Not.Null);
            }
            finally { Directory.Delete(package, true); }
        }

        [Test]
        public void DiagnosticSceneSurvivesReopenWithPersistentProductionReferences()
        {
            string package = NewImportablePackage();
            string token = Guid.NewGuid().ToString("N");
            string artRoot = "Assets/Art/NocturneProduction/Tests/" + token;
            string worldRoot = "Assets/World/NocturneProduction/Tests/" + token;
            string settingsRoot = "Assets/Settings/NocturneProduction/Tests/" + token;
            string sceneRoot = "Assets/Scenes/NocturneProductionTests";
            string scenePath = sceneRoot + "/" + token + ".unity";
            createdAssetRoots.Add(artRoot);
            createdAssetRoots.Add(worldRoot);
            createdAssetRoots.Add(settingsRoot);
            createdAssetRoots.Add(scenePath);
            try
            {
                ProductionImportResult import = ProductionArtImporter.ImportPackage(package, CourseHash, artRoot, worldRoot);
                ProductionSceneBuildResult build = ProductionSceneBuilder.BuildScene(import, scenePath, settingsRoot);
                Assert.That(build.sceneAssetPath, Is.EqualTo(scenePath));
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                ProductionWorld production = UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
                Assert.That(production, Is.Not.Null);
                Assert.That(production.track, Is.Not.Null);
                Assert.That(production.track.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(production.track.transform.rotation, Is.EqualTo(Quaternion.identity));
                Assert.That(production.track.transform.lossyScale, Is.EqualTo(Vector3.one));
                Assert.That(production.courseHash, Is.EqualTo(CourseHash));
                Assert.That(production.artRevision, Is.EqualTo(import.revision));
                Assert.That(production.materials, Is.Not.Null);
                Assert.That(AssetDatabase.Contains(production.materials), Is.True);
                Assert.That(UnityEngine.Object.FindObjectsByType<WorldBuilder>(FindObjectsSortMode.None), Is.Empty);
                Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                    .Where(camera => camera.CompareTag("MainCamera")).ToArray();
                Assert.That(cameras, Has.Length.EqualTo(1));
                Assert.That(UnityEngine.Object.FindObjectsByType<TrackPath>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Any(value => value.name.Contains("Running surface")), Is.True);
                Assert.That(GameObject.Find("Zone • viaduct/DiagnosticInstance"), Is.Not.Null);
                foreach (Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    foreach (Material material in renderer.sharedMaterials)
                        Assert.That(material == null || AssetDatabase.Contains(material), Is.True, renderer.name + " has a transient material");
                production.ValidateReady();
            }
            finally
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                Directory.Delete(package, true);
            }
        }

        [Test]
        public void FullCourseOrientedClearanceReportsTheOffendingInstanceAndPosition()
        {
            var trackObject = new GameObject("clearance test track");
            string package = null;
            try
            {
                TrackPath track = trackObject.AddComponent<TrackPath>();
                TrackFrame frame = track.Evaluate(.2f);
                package = NewPackage(
                    manifest =>
                    {
                        manifest.assets[0].boundsMin = new[] { -2f, 0f, -8f };
                        manifest.assets[0].boundsMax = new[] { 2f, 6f, 8f };
                    },
                    layout =>
                    {
                        layout.instances[0].id = "IntrudingLongRoof";
                        layout.instances[0].position = new[] { frame.Position.x, frame.Position.y, frame.Position.z };
                        layout.instances[0].rotation = new[] { frame.Forward.x, frame.Forward.y, frame.Forward.z, 0f };
                        Quaternion rotation = Quaternion.LookRotation(frame.Forward, frame.Up);
                        layout.instances[0].rotation = new[] { rotation.x, rotation.y, rotation.z, rotation.w };
                    });
                ValidatedProductionPackage validated = ProductionArtImporter.ValidatePackage(package, CourseHash);
                IReadOnlyList<ProductionClearanceIssue> issues = ProductionSceneBuilder.CheckClearance(validated);
                Assert.That(issues.Select(value => value.instanceId), Does.Contain("IntrudingLongRoof"));
                ProductionClearanceIssue issue = issues.First(value => value.instanceId == "IntrudingLongRoof");
                Assert.That(Vector3.Distance(issue.instancePosition, frame.Position), Is.LessThan(.001f));
            }
            finally
            {
                if (package != null) Directory.Delete(package, true);
                UnityEngine.Object.DestroyImmediate(trackObject);
            }
        }

        const string CourseHash = "598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059";

        static string NewPackage(Action<ProductionArtManifest> changeManifest = null, Action<ProductionLayout> changeLayout = null)
        {
            string root = Path.Combine(Path.GetTempPath(), "VectorRush-Package-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "meshes"));
            Directory.CreateDirectory(Path.Combine(root, "textures"));
            Directory.CreateDirectory(Path.Combine(root, "source"));
            File.WriteAllText(Path.Combine(root, "meshes", "Diagnostic_LOD0.fbx"), "diagnostic lod0");
            File.WriteAllText(Path.Combine(root, "meshes", "Diagnostic_LOD1.fbx"), "diagnostic lod1");
            foreach (string map in new[] { "BaseColor", "Normal", "MetallicSmoothness", "Occlusion" })
                File.WriteAllText(Path.Combine(root, "textures", "Diagnostic_" + map + ".png"), "diagnostic " + map);
            File.WriteAllText(Path.Combine(root, "source", "Diagnostic.blend"), "DIAGNOSTIC_ONLY");
            File.WriteAllText(Path.Combine(root, "asset-stats.json"), "{}");

            var material = new ProductionMaterialRecord
            {
                id = "DiagnosticMaterial", baseColor = "textures/Diagnostic_BaseColor.png", normal = "textures/Diagnostic_Normal.png",
                metallicSmoothness = "textures/Diagnostic_MetallicSmoothness.png", occlusion = "textures/Diagnostic_Occlusion.png", emission = "",
                metersPerTile = new[] { 1f, 1f }, uvMode = "metric", normalScale = 1f,
                emissionColorLinear = new[] { 0f, 0f, 0f }, emissionIntensity = 0f
            };
            var manifest = new ProductionArtManifest
            {
                contractVersion = 1, revision = "art-package-diagnostic-01", courseHash = CourseHash, layoutComplete = false,
                sourceBlend = "source/Diagnostic.blend", materials = new[] { material },
                assets = new[] { new ProductionAssetRecord { id = "Diagnostic", lod0 = "meshes/Diagnostic_LOD0.fbx", lod1 = "meshes/Diagnostic_LOD1.fbx", pivot = "ground-center", boundsMin = new[] { -1f, 0f, -2f }, boundsMax = new[] { 1f, 3f, 2f }, materialSlots = new[] { material.id }, trackAssembly = false, collider = "none", lightmapUVs = "generate" } }
            };
            changeManifest?.Invoke(manifest);
            WriteJson(root, "manifest.json", manifest);

            var layout = new ProductionLayout
            {
                contractVersion = 1, revision = manifest.revision, courseHash = CourseHash,
                zones = new[] { new ProductionZoneRecord { id = "viaduct", startProgress = 0f, endProgress = .3f } },
                instances = new[] { new ProductionInstanceRecord { id = "DiagnosticInstance", assetId = "Diagnostic", zoneId = "viaduct", position = new[] { 30f, 10f, 20f }, rotation = new[] { 0f, 0f, 0f, 1f }, scale = new[] { 1f, 1f, 1f }, frameMode = "upright", role = "far", gi = "none" } }
            };
            changeLayout?.Invoke(layout);
            WriteJson(root, "layout.json", layout);
            WriteJson(root, "lighting.json", new ProductionLighting { contractVersion = 1, revision = manifest.revision, courseHash = CourseHash, environment = new ProductionEnvironmentRecord { fogColorLinear = new[] { .01f, .02f, .03f }, fogDensity = .001f, exposureEV = 0f, ambientTintLinear = new[] { .1f, .1f, .1f }, skyMaterialId = "production-night-sky" }, lights = Array.Empty<ProductionLightRecord>(), reflectionVolumes = Array.Empty<ProductionReflectionVolumeRecord>() });
            WriteJson(root, "track-profile.json", new ProductionTrackProfile { contractVersion = 1, courseHash = CourseHash, strips = new[] { new ProductionTrackStripRecord { id = "DiagnosticFascia", materialId = material.id, pointsXY = new[] { -13f, -1f, -13f, 0f }, surfaceRole = "fascia", uvMetersPerTile = 1f, twoSided = false } } });

            var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => new ProductionChecksumRecord { path = path.Substring(root.Length + 1).Replace(Path.DirectorySeparatorChar, '/'), sha256 = Hash(File.ReadAllBytes(path)) })
                .OrderBy(entry => entry.path, StringComparer.Ordinal).ToArray();
            var checksums = new ProductionChecksums { contractVersion = 1, revision = manifest.revision, files = files };
            WriteJson(root, "checksums.json", checksums);
            WriteJson(root, "READY.json", new ProductionReady { contractVersion = 1, revision = manifest.revision, courseHash = CourseHash, checksumsSha256 = Hash(File.ReadAllBytes(Path.Combine(root, "checksums.json"))) });
            return root;
        }

        static string NewImportablePackage()
        {
            const string sourceModel = "Assets/Resources/Art/Environment/Nocturne_PlatformButtress_A.fbx";
            const string sourceTextureRoot = "Assets/Resources/Art/ShipSurfaces/Ivory_";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourceModel);
            Assert.That(prefab, Is.Not.Null);
            Bounds bounds = LocalBounds(prefab);
            string package = NewPackage(manifest =>
            {
                manifest.assets[0].boundsMin = new[] { bounds.min.x, bounds.min.y, bounds.min.z };
                manifest.assets[0].boundsMax = new[] { bounds.max.x, bounds.max.y, bounds.max.z };
            });
            File.Copy(Path.GetFullPath(Path.Combine(Application.dataPath, "..", sourceModel)), Path.Combine(package, "meshes", "Diagnostic_LOD0.fbx"), true);
            File.Copy(Path.GetFullPath(Path.Combine(Application.dataPath, "..", sourceModel)), Path.Combine(package, "meshes", "Diagnostic_LOD1.fbx"), true);
            foreach (string map in new[] { "BaseColor", "Normal", "MetallicSmoothness", "Occlusion" })
                File.Copy(Path.GetFullPath(Path.Combine(Application.dataPath, "..", sourceTextureRoot + map + ".png")), Path.Combine(package, "textures", "Diagnostic_" + map + ".png"), true);
            RewriteIntegrityFiles(package);
            return package;
        }

        static Bounds LocalBounds(GameObject prefab)
        {
            bool initialized = false;
            Bounds result = default;
            Matrix4x4 rootToLocal = prefab.transform.worldToLocalMatrix;
            foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                Bounds bounds = filter.sharedMesh.bounds;
                Matrix4x4 matrix = rootToLocal * filter.transform.localToWorldMatrix;
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 point = matrix.MultiplyPoint3x4(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z)));
                            if (!initialized) { result = new Bounds(point, Vector3.zero); initialized = true; }
                            else result.Encapsulate(point);
                        }
            }
            Assert.That(initialized, Is.True);
            return result;
        }

        static void RewriteIntegrityFiles(string root)
        {
            File.Delete(Path.Combine(root, "checksums.json"));
            File.Delete(Path.Combine(root, "READY.json"));
            ProductionArtManifest manifest = JsonUtility.FromJson<ProductionArtManifest>(File.ReadAllText(Path.Combine(root, "manifest.json")));
            var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => new ProductionChecksumRecord { path = path.Substring(root.Length + 1).Replace(Path.DirectorySeparatorChar, '/'), sha256 = Hash(File.ReadAllBytes(path)) })
                .OrderBy(entry => entry.path, StringComparer.Ordinal).ToArray();
            var checksums = new ProductionChecksums { contractVersion = 1, revision = manifest.revision, files = files };
            WriteJson(root, "checksums.json", checksums);
            WriteJson(root, "READY.json", new ProductionReady { contractVersion = 1, revision = manifest.revision, courseHash = CourseHash, checksumsSha256 = Hash(File.ReadAllBytes(Path.Combine(root, "checksums.json"))) });
        }

        static void CreateAssetFolder(string path)
        {
            string current = "Assets";
            foreach (string segment in path.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + segment;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segment);
                current = next;
            }
        }

        static void WriteJson(string root, string relative, object value)
        {
            File.WriteAllText(Path.Combine(root, relative), JsonUtility.ToJson(value, false), new UTF8Encoding(false));
        }

        static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var value = new StringBuilder(64);
                foreach (byte item in sha.ComputeHash(bytes)) value.Append(item.ToString("x2"));
                return value.ToString();
            }
        }

        static Type FindEditorType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
