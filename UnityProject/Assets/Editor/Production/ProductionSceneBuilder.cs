using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace VectorRush.Editor
{
    public sealed class ProductionSceneBuildResult
    {
        public string sceneAssetPath;
        public string settingsAssetRoot;
        public int instanceCount;
        public int lightCount;
        public int reflectionProbeCount;
    }

    public sealed class ProductionClearanceIssue
    {
        public string instanceId;
        public Vector3 instancePosition;
        public float courseProgress;
        public Vector3 coursePosition;

        public override string ToString()
        {
            return instanceId + " at " + Format(instancePosition) + " intersects protected course volume near progress " +
                   courseProgress.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " / " + Format(coursePosition);
        }

        static string Format(Vector3 value) => "(" + value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ", " + value.z.ToString("0.###") + ")";
    }

    public sealed class ProductionClearanceException : Exception
    {
        public IReadOnlyList<ProductionClearanceIssue> Issues { get; }

        public ProductionClearanceException(IReadOnlyList<ProductionClearanceIssue> issues)
            : base("Production layout violates the protected road/craft volume: " + string.Join("; ", issues.Select(value => value.ToString())))
        {
            Issues = issues;
        }
    }

    public static class ProductionSceneBuilder
    {
        const int TrackSegments = 960;
        const int ClearanceSamples = 1200;

        public static ProductionSceneBuildResult BuildScene(
            ProductionImportResult import,
            string sceneAssetPath = "Assets/Scenes/NocturneProduction.unity",
            string settingsAssetRoot = null)
        {
            ValidateImport(import);
            string scenePath = ValidateAssetFile(sceneAssetPath, ".unity", nameof(sceneAssetPath));
            string settingsRoot = ValidateAssetRoot(settingsAssetRoot ?? "Assets/Settings/NocturneProduction/" + import.revision, nameof(settingsAssetRoot));

            IReadOnlyList<ProductionClearanceIssue> clearance = CheckClearance(import.package);
            if (clearance.Count > 0) throw new ProductionClearanceException(clearance);

            EnsureAssetFolder(Path.GetDirectoryName(scenePath).Replace(Path.DirectorySeparatorChar, '/'));
            EnsureAssetFolder(settingsRoot);
            EnsureAssetFolder(import.worldAssetRoot + "/TrackMeshes");

            UniversalRenderPipelineAsset pipeline = CreateProductionRenderAssets(settingsRoot);
            VolumeProfile volumeProfile = CreateVolumeProfile(settingsRoot, import.package.lighting.environment);
            LightingSettings lightingSettings = CreateLightingSettings(settingsRoot);
            ProductionMaterialLibrary craftLibrary = CreateCraftMaterialLibrary(settingsRoot);
            Dictionary<string, Material> artMaterials = LoadArtMaterials(import);
            Material roadMaterial = CreateRoadMaterial(import.worldAssetRoot);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NOCTURNE PRODUCTION • " + import.revision);
            var renderConfiguration = root.AddComponent<ProductionRenderConfiguration>();
            renderConfiguration.renderPipeline = pipeline;
            var production = root.AddComponent<ProductionWorld>();
            production.courseHash = import.package.manifest.courseHash;
            production.artRevision = import.revision;
            production.layoutComplete = import.package.manifest.layoutComplete;
            production.materials = craftLibrary;

            var trackObject = new GameObject("Nocturne production • race spline");
            trackObject.transform.SetParent(root.transform, false);
            production.track = trackObject.AddComponent<TrackPath>();
            production.track.Ensure();

            GameObject trackGeometry = new GameObject("Persistent race geometry");
            trackGeometry.transform.SetParent(root.transform, false);
            BuildAuthoritativeCollision(production.track, trackGeometry.transform, roadMaterial, import.worldAssetRoot + "/TrackMeshes");
            BuildVisualTrackProfile(production.track, import.package.trackProfile, trackGeometry.transform, artMaterials, import.worldAssetRoot + "/TrackMeshes");

            InstantiateLayout(import, root.transform);
            ConfigureEnvironment(import.package.lighting.environment, settingsRoot);
            CreateLights(import.package.lighting, root.transform);
            CreateReflectionProbes(import.package.lighting, root.transform);
            CreateMovingCraftProbeCoverage(production.track, root.transform);
            Lightmapping.lightingSettings = lightingSettings;
            CreateCamera(volumeProfile, root.transform);

            var bootstrap = new GameObject("Vector Rush • production bootstrap").AddComponent<VectorBootstrap>();
            bootstrap.transform.SetParent(root.transform, false);
            bootstrap.productionWorld = production;
            production.ValidateReady();

            string stagingScene = Path.GetDirectoryName(scenePath).Replace(Path.DirectorySeparatorChar, '/') + "/ProductionSceneStaging-" + Guid.NewGuid().ToString("N") + ".unity";
            string backupScene = scenePath + ".backup";
            bool previousMoved = false;
            try
            {
                if (!EditorSceneManager.SaveScene(scene, stagingScene)) throw new IOException("Unable to save production scene staging asset: " + stagingScene);
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(stagingScene, OpenSceneMode.Single);
                VerifyOpenScene(import);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
                {
                    string moveError = AssetDatabase.MoveAsset(scenePath, backupScene);
                    if (!string.IsNullOrEmpty(moveError)) throw new IOException("Unable to preserve previous production scene: " + moveError);
                    previousMoved = true;
                }
                MoveAssetOrThrow(stagingScene, scenePath);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                VerifyOpenScene(import);
                if (previousMoved) AssetDatabase.DeleteAsset(backupScene);
                AssetDatabase.SaveAssets();
            }
            catch
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(stagingScene)) AssetDatabase.DeleteAsset(stagingScene);
                if (previousMoved && !AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)) AssetDatabase.MoveAsset(backupScene, scenePath);
                throw;
            }

            return new ProductionSceneBuildResult
            {
                sceneAssetPath = scenePath,
                settingsAssetRoot = settingsRoot,
                instanceCount = import.package.layout.instances.Length,
                lightCount = import.package.lighting.lights.Length,
                reflectionProbeCount = import.package.lighting.reflectionVolumes.Length
            };
        }

        public static IReadOnlyList<ProductionClearanceIssue> CheckClearance(ValidatedProductionPackage package)
        {
            if (package == null || package.manifest == null || package.layout == null) throw new ArgumentNullException(nameof(package));
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var trackObject = new GameObject("Production clearance track");
                SceneManager.MoveGameObjectToScene(trackObject, preview);
                TrackPath track = trackObject.AddComponent<TrackPath>();
                track.Ensure();
                var assets = package.manifest.assets.ToDictionary(value => value.id, StringComparer.Ordinal);
                var issues = new List<ProductionClearanceIssue>();
                foreach (ProductionInstanceRecord instance in package.layout.instances)
                {
                    ProductionAssetRecord asset = assets[instance.assetId];
                    if (asset.trackAssembly) continue;
                    Vector3 position = V3(instance.position);
                    Quaternion rotation = Q4(instance.rotation);
                    Vector3 minimum = V3(asset.boundsMin);
                    Vector3 maximum = V3(asset.boundsMax);
                    OrientedBox candidate = new OrientedBox(position + rotation * ((minimum + maximum) * .5f), rotation, (maximum - minimum) * .5f);
                    for (int i = 0; i <= ClearanceSamples; i++)
                    {
                        float progress = i / (float)ClearanceSamples;
                        TrackFrame frame = track.Evaluate(progress);
                        float halfAlong = Mathf.Max(.8f, track.Length / ClearanceSamples * .6f);
                        Vector3 center = frame.Position + frame.Up * 3.975f;
                        OrientedBox protectedVolume = new OrientedBox(center, frame.Right, frame.Up, frame.Forward, new Vector3(12.2f, 4.025f, halfAlong));
                        if (!candidate.Intersects(protectedVolume)) continue;
                        issues.Add(new ProductionClearanceIssue { instanceId = instance.id, instancePosition = position, courseProgress = progress, coursePosition = frame.Position });
                        break;
                    }
                }
                return issues;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        static void BuildAuthoritativeCollision(TrackPath track, Transform parent, Material material, string meshRoot)
        {
            CreateRibbon(track, parent, "Running surface • authoritative collision", -11f, 11f, 0f, material, true, false, meshRoot + "/RunningSurface.asset");
            CreateRibbon(track, parent, "Left shoulder • authoritative collision", -12f, -11f, -.02f, material, true, false, meshRoot + "/LeftShoulder.asset");
            CreateRibbon(track, parent, "Right shoulder • authoritative collision", 11f, 12f, -.02f, material, true, false, meshRoot + "/RightShoulder.asset");
            CreateBarrier(track, parent, "Left safety barrier • authoritative collision", -1, material, meshRoot + "/LeftBarrier.asset");
            CreateBarrier(track, parent, "Right safety barrier • authoritative collision", 1, material, meshRoot + "/RightBarrier.asset");
        }

        static void BuildVisualTrackProfile(TrackPath track, ProductionTrackProfile profile, Transform parent, Dictionary<string, Material> materials, string meshRoot)
        {
            foreach (ProductionTrackStripRecord strip in profile.strips)
            {
                int crossSegments = strip.pointsXY.Length / 2 - 1;
                int stride = crossSegments + 1;
                var vertices = new Vector3[(TrackSegments + 1) * stride];
                var uv = new Vector2[vertices.Length];
                var triangles = new int[TrackSegments * crossSegments * 6];
                float distance = 0f;
                Vector3 previous = Vector3.zero;
                for (int i = 0; i <= TrackSegments; i++)
                {
                    TrackFrame frame = track.Evaluate(i / (float)TrackSegments);
                    if (i > 0) distance += Vector3.Distance(previous, frame.Position);
                    previous = frame.Position;
                    for (int j = 0; j < stride; j++)
                    {
                        float x = strip.pointsXY[j * 2];
                        float y = strip.pointsXY[j * 2 + 1];
                        int index = i * stride + j;
                        vertices[index] = frame.Position + frame.Right * x + frame.Up * y;
                        uv[index] = new Vector2(distance / strip.uvMetersPerTile, j / (float)crossSegments);
                        if (i == TrackSegments || j == crossSegments) continue;
                        int triangle = (i * crossSegments + j) * 6;
                        int a = index;
                        triangles[triangle] = a;
                        triangles[triangle + 1] = a + stride;
                        triangles[triangle + 2] = a + 1;
                        triangles[triangle + 3] = a + 1;
                        triangles[triangle + 4] = a + stride;
                        triangles[triangle + 5] = a + stride + 1;
                    }
                }
                Mesh mesh = CreateMesh(strip.id, vertices, uv, triangles, meshRoot + "/Profile-" + strip.id + ".asset");
                GameObject item = CreateMeshObject("Track profile • " + strip.id, parent, mesh, materials[strip.materialId], false);
                item.isStatic = true;
            }
        }

        static void CreateRibbon(TrackPath track, Transform parent, string name, float left, float right, float height, Material material, bool collider, bool faceDown, string meshPath)
        {
            const int columns = 12;
            const int stride = columns + 1;
            var vertices = new Vector3[(TrackSegments + 1) * stride];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[TrackSegments * columns * 6];
            for (int i = 0; i <= TrackSegments; i++)
            {
                TrackFrame frame = track.Evaluate(i / (float)TrackSegments);
                for (int j = 0; j <= columns; j++)
                {
                    float across = j / (float)columns;
                    int index = i * stride + j;
                    vertices[index] = frame.Position + frame.Right * Mathf.Lerp(left, right, across) + frame.Up * height;
                    uv[index] = new Vector2(across, i * .25f);
                    if (i == TrackSegments || j == columns) continue;
                    int triangle = (i * columns + j) * 6;
                    int a = index;
                    triangles[triangle] = a;
                    triangles[triangle + 1] = a + stride;
                    triangles[triangle + 2] = a + 1;
                    triangles[triangle + 3] = a + 1;
                    triangles[triangle + 4] = a + stride;
                    triangles[triangle + 5] = a + stride + 1;
                }
            }
            if (faceDown)
                for (int i = 0; i < triangles.Length; i += 3) (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
            Mesh mesh = CreateMesh(name, vertices, uv, triangles, meshPath);
            CreateMeshObject(name, parent, mesh, material, collider).isStatic = true;
        }

        static void CreateBarrier(TrackPath track, Transform parent, string name, int side, Material material, string meshPath)
        {
            var vertices = new Vector3[(TrackSegments + 1) * 4];
            var uv = new Vector2[vertices.Length];
            var triangles = new List<int>(TrackSegments * 24);
            for (int i = 0; i <= TrackSegments; i++)
            {
                TrackFrame frame = track.Evaluate(i / (float)TrackSegments);
                for (int j = 0; j < 4; j++)
                {
                    float x = (j < 2 ? 11.7f : 12.1f) * side;
                    float y = (j == 0 || j == 3) ? -.3f : 2f;
                    vertices[i * 4 + j] = frame.Position + frame.Right * x + frame.Up * y;
                    uv[i * 4 + j] = new Vector2(j, i * .2f);
                }
                if (i == TrackSegments) continue;
                for (int j = 0; j < 4; j++)
                {
                    int a = i * 4 + j;
                    int b = i * 4 + (j + 1) % 4;
                    int c = a + 4;
                    int d = b + 4;
                    if (side > 0) triangles.AddRange(new[] { a, c, b, b, c, d });
                    else triangles.AddRange(new[] { a, b, c, b, d, c });
                }
            }
            Mesh mesh = CreateMesh(name, vertices, uv, triangles.ToArray(), meshPath);
            CreateMeshObject(name, parent, mesh, material, true).isStatic = true;
        }

        static Mesh CreateMesh(string name, Vector3[] vertices, Vector2[] uv, int[] triangles, string assetPath)
        {
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, assetPath);
            return mesh;
        }

        static GameObject CreateMeshObject(string name, Transform parent, Mesh mesh, Material material, bool collider)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.AddComponent<MeshFilter>().sharedMesh = mesh;
            item.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (collider) item.AddComponent<MeshCollider>().sharedMesh = mesh;
            return item;
        }

        static void InstantiateLayout(ProductionImportResult import, Transform parent)
        {
            var zones = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (ProductionZoneRecord record in import.package.layout.zones)
            {
                var zone = new GameObject("Zone • " + record.id);
                zone.transform.SetParent(parent, false);
                zones.Add(record.id, zone.transform);
            }
            var assets = import.package.manifest.assets.ToDictionary(value => value.id, StringComparer.Ordinal);
            foreach (ProductionInstanceRecord record in import.package.layout.instances)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(import.worldAssetRoot + "/Prefabs/" + record.assetId + ".prefab");
                if (!prefab) throw new InvalidDataException("Imported prefab is missing: " + record.assetId);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = record.id;
                instance.transform.SetParent(zones[record.zoneId], false);
                instance.transform.SetPositionAndRotation(V3(record.position), Q4(record.rotation));
                instance.transform.localScale = Vector3.one;
                StaticEditorFlags flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
                if (record.gi == "lightmap") flags |= StaticEditorFlags.ContributeGI;
                foreach (Transform child in instance.GetComponentsInChildren<Transform>(true)) GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags);
                if (!assets[record.assetId].trackAssembly)
                    foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        static void ConfigureEnvironment(ProductionEnvironmentRecord environment, string settingsRoot)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = LinearColor(environment.fogColorLinear).gamma;
            RenderSettings.fogDensity = environment.fogDensity;
            RenderSettings.ambientMode = AmbientMode.Custom;
            var ambient = new SphericalHarmonicsL2();
            ambient.AddAmbientLight(LinearColor(environment.ambientTintLinear));
            RenderSettings.ambientProbe = ambient;
            Shader shader = Shader.Find("VectorRush/Night Sky") ?? Shader.Find("Skybox/Procedural");
            if (!shader) throw new InvalidOperationException("Production sky shader is unavailable");
            var sky = new Material(shader) { name = "Nocturne production sky" };
            AssetDatabase.CreateAsset(sky, settingsRoot + "/NocturneSky.mat");
            RenderSettings.skybox = sky;
        }

        static void CreateLights(ProductionLighting lighting, Transform parent)
        {
            var root = new GameObject("Authored production lights");
            root.transform.SetParent(parent, false);
            foreach (ProductionLightRecord record in lighting.lights)
            {
                var item = new GameObject(record.id);
                item.transform.SetParent(root.transform, false);
                item.transform.SetPositionAndRotation(V3(record.position), Q4(record.rotation));
                Light light = item.AddComponent<Light>();
                light.type = record.type == "point" ? LightType.Point : record.type == "directional" ? LightType.Directional : LightType.Spot;
                light.color = LinearColor(record.colorLinear);
                light.intensity = record.intensity;
                light.range = record.range;
                light.spotAngle = record.spotOuterDegrees;
                light.innerSpotAngle = record.spotInnerDegrees;
                light.lightmapBakeType = record.mode == "baked" ? LightmapBakeType.Baked : record.mode == "mixed" ? LightmapBakeType.Mixed : LightmapBakeType.Realtime;
                light.shadows = record.castsShadows ? LightShadows.Soft : LightShadows.None;
            }
        }

        static void CreateReflectionProbes(ProductionLighting lighting, Transform parent)
        {
            var root = new GameObject("Authored local reflection volumes");
            root.transform.SetParent(parent, false);
            foreach (ProductionReflectionVolumeRecord record in lighting.reflectionVolumes)
            {
                var item = new GameObject(record.id);
                item.transform.SetParent(root.transform, false);
                item.transform.position = V3(record.position);
                ReflectionProbe probe = item.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Baked;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.size = V3(record.size);
                probe.resolution = record.resolution;
                probe.intensity = record.intensity;
                probe.boxProjection = record.boxProjection;
                probe.cullingMask = ~(1 << 8);
            }
        }

        static void CreateMovingCraftProbeCoverage(TrackPath track, Transform parent)
        {
            var root = new GameObject("Moving craft light probe corridor");
            root.transform.SetParent(parent, false);
            var positions = new List<Vector3>();
            for (int i = 0; i < 120; i++)
            {
                TrackFrame frame = track.Evaluate(i / 120f);
                foreach (float lateral in new[] { -8f, 0f, 8f })
                    foreach (float vertical in new[] { 1.5f, 5f })
                        positions.Add(frame.Position + frame.Right * lateral + frame.Up * vertical);
            }
            root.AddComponent<LightProbeGroup>().probePositions = positions.ToArray();
        }

        static void CreateCamera(VolumeProfile profile, Transform parent)
        {
            var item = new GameObject("Race camera • saved production");
            item.tag = "MainCamera";
            item.transform.SetParent(parent, false);
            Camera camera = item.AddComponent<Camera>();
            camera.nearClipPlane = .18f;
            camera.farClipPlane = 5000f;
            camera.fieldOfView = 65f;
            camera.allowHDR = true;
            item.AddComponent<AudioListener>();
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
            var volumeObject = new GameObject("Race grade • saved production");
            Volume volume = volumeObject.AddComponent<Volume>();
            volumeObject.transform.SetParent(parent, false);
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        static UniversalRenderPipelineAsset CreateProductionRenderAssets(string settingsRoot)
        {
            UniversalRenderPipelineAsset sourcePipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset ??
                                                         QualitySettings.renderPipeline as UniversalRenderPipelineAsset ??
                                                         AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/VectorPipeline.asset");
            UniversalRendererData sourceRenderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/VectorRenderer.asset");
            if (!sourcePipeline || !sourceRenderer) throw new InvalidOperationException("Legacy URP assets are unavailable to clone for production");

            UniversalRendererData renderer = UnityEngine.Object.Instantiate(sourceRenderer);
            renderer.name = "Nocturne Production Renderer";
            List<ScriptableRendererFeature> sourceFeatures = sourceRenderer.rendererFeatures.Where(value => value).ToList();
            renderer.rendererFeatures.Clear();
            AssetDatabase.CreateAsset(renderer, settingsRoot + "/NocturneRenderer.asset");
            foreach (ScriptableRendererFeature sourceFeature in sourceFeatures)
            {
                ScriptableRendererFeature feature = UnityEngine.Object.Instantiate(sourceFeature);
                feature.name = sourceFeature.name;
                AssetDatabase.AddObjectToAsset(feature, renderer);
                renderer.rendererFeatures.Add(feature);
            }
            EditorUtility.SetDirty(renderer);

            UniversalRenderPipelineAsset pipeline = UnityEngine.Object.Instantiate(sourcePipeline);
            pipeline.name = "Nocturne Production Pipeline";
            pipeline.renderScale = 1f;
            pipeline.msaaSampleCount = 4;
            pipeline.supportsHDR = true;
            pipeline.shadowDistance = 150f;
            pipeline.shadowCascadeCount = 4;
            AssetDatabase.CreateAsset(pipeline, settingsRoot + "/NocturnePipeline.asset");
            var serialized = new SerializedObject(pipeline);
            SerializedProperty list = serialized.FindProperty("m_RendererDataList");
            if (list == null) throw new InvalidOperationException("URP renderer data list is unavailable");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            SerializedProperty defaultIndex = serialized.FindProperty("m_DefaultRendererIndex");
            if (defaultIndex != null) defaultIndex.intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            return pipeline;
        }

        static VolumeProfile CreateVolumeProfile(string settingsRoot, ProductionEnvironmentRecord environment)
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Nocturne Production Grade";
            var bloom = profile.Add<Bloom>();
            bloom.threshold.Override(1.3f);
            bloom.intensity.Override(.32f);
            bloom.scatter.Override(.55f);
            var tonemapping = profile.Add<Tonemapping>();
            tonemapping.mode.Override(TonemappingMode.ACES);
            var color = profile.Add<ColorAdjustments>();
            color.postExposure.Override(environment.exposureEV);
            color.contrast.Override(3f);
            color.saturation.Override(3f);
            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(.16f);
            vignette.smoothness.Override(.65f);
            AssetDatabase.CreateAsset(profile, settingsRoot + "/NocturneVolumeProfile.asset");
            return profile;
        }

        static LightingSettings CreateLightingSettings(string settingsRoot)
        {
            var settings = new LightingSettings { name = "Nocturne Production Lighting" };
            AssetDatabase.CreateAsset(settings, settingsRoot + "/NocturneLightingSettings.lighting");
            var serialized = new SerializedObject(settings);
            SetFloatIfPresent(serialized, "m_LightmapResolution", 20f);
            SetIntIfPresent(serialized, "m_LightmapMaxSize", 2048);
            SetBoolIfPresent(serialized, "m_ExportTrainingData", false);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        static ProductionMaterialLibrary CreateCraftMaterialLibrary(string settingsRoot)
        {
            var library = ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            library.name = "Nocturne Production Material Library";
            AssetDatabase.CreateAsset(library, settingsRoot + "/ProductionMaterialLibrary.asset");
            library.craftSurfaceTemplate = CloneMaterial(Resources.Load<Material>("CraftSurfaceLit") ?? Resources.Load<Material>("SurfaceLit"), settingsRoot + "/CraftSurfaceTemplate.mat", "Craft surface template");
            library.craftMetalFallback = CreateSolidMaterial(settingsRoot + "/CraftMetal.mat", "Craft metal fallback", new Color(.3f, .37f, .4f), .7f, .8f, Color.black);
            library.craftGlass = CreateSolidMaterial(settingsRoot + "/CraftGlass.mat", "Craft glass", new Color(.025f, .1f, .14f), .98f, .7f, Color.black);
            library.craftEngineAccent = CreateSolidMaterial(settingsRoot + "/CraftEngineAccent.mat", "Craft engine accent", new Color(.045f, .19f, .25f), .62f, .35f, new Color(.015f, .46f, .68f));
            library.craftEngineCore = CreateSolidMaterial(settingsRoot + "/CraftEngineCore.mat", "Craft engine core", new Color(.65f, .85f, .92f), .42f, 0f, new Color(1.7f, 2.6f, 3f));
            EditorUtility.SetDirty(library);
            return library;
        }

        static Material CreateRoadMaterial(string worldRoot)
        {
            Material source = Resources.Load<Material>("RoadSurface");
            if (!source) source = Resources.Load<Material>("SurfaceLit");
            return CloneMaterial(source, worldRoot + "/Materials/ProductionRoad.mat", "Nocturne production road");
        }

        static Material CloneMaterial(Material source, string path, string name)
        {
            if (!source) throw new InvalidOperationException("Persistent material template is missing for " + name);
            var material = new Material(source) { name = name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Material CreateSolidMaterial(string path, string name, Color baseColor, float smoothness, float metallic, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new InvalidOperationException("URP Lit shader is unavailable");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0f) material.EnableKeyword("_EMISSION");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Dictionary<string, Material> LoadArtMaterials(ProductionImportResult import)
        {
            var result = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (ProductionMaterialRecord record in import.package.manifest.materials)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(import.worldAssetRoot + "/Materials/" + record.id + ".mat");
                if (!material) throw new InvalidDataException("Persistent production material is missing: " + record.id);
                result.Add(record.id, material);
            }
            return result;
        }

        static void VerifyOpenScene(ProductionImportResult import)
        {
            ProductionWorld[] worlds = UnityEngine.Object.FindObjectsByType<ProductionWorld>(FindObjectsSortMode.None);
            if (worlds.Length != 1) throw new InvalidDataException("Saved production scene must contain exactly one ProductionWorld; found " + worlds.Length);
            ProductionWorld world = worlds[0];
            world.ValidateReady();
            if (world.courseHash != import.package.manifest.courseHash || world.artRevision != import.revision)
                throw new InvalidDataException("Saved production scene identity does not match the imported package");
            if (UnityEngine.Object.FindObjectsByType<TrackPath>(FindObjectsSortMode.None).Length != 1)
                throw new InvalidDataException("Saved production scene must contain exactly one TrackPath");
            if (UnityEngine.Object.FindObjectsByType<WorldBuilder>(FindObjectsSortMode.None).Length != 0)
                throw new InvalidDataException("Saved production scene contains a legacy WorldBuilder");
            int cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Count(value => value.CompareTag("MainCamera"));
            if (cameras != 1) throw new InvalidDataException("Saved production scene must contain exactly one active MainCamera; found " + cameras);
            foreach (Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (Material material in renderer.sharedMaterials)
                    if (material && !AssetDatabase.Contains(material)) throw new InvalidDataException("Saved renderer uses a transient material: " + renderer.name);
            foreach (MeshFilter filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (filter.sharedMesh && !AssetDatabase.Contains(filter.sharedMesh)) throw new InvalidDataException("Saved renderer uses a transient mesh: " + filter.name);
        }

        static void ValidateImport(ProductionImportResult import)
        {
            if (import == null || import.package == null) throw new ArgumentNullException(nameof(import));
            if (!AssetDatabase.IsValidFolder(import.artAssetRoot) || !AssetDatabase.IsValidFolder(import.worldAssetRoot))
                throw new DirectoryNotFoundException("Imported production asset roots are missing");
            if (import.revision != import.package.manifest.revision) throw new InvalidDataException("Import revision identity does not match its package");
        }

        static string ValidateAssetFile(string path, string extension, string argument)
        {
            string normalized = path?.Replace('\\', '/').Trim();
            if (string.IsNullOrWhiteSpace(normalized) || !normalized.StartsWith("Assets/", StringComparison.Ordinal) ||
                !normalized.EndsWith(extension, StringComparison.OrdinalIgnoreCase) || normalized.Contains("/../"))
                throw new ArgumentException("Path must be a project-relative " + extension + " asset under Assets: " + path, argument);
            return normalized;
        }

        static string ValidateAssetRoot(string path, string argument)
        {
            string normalized = path?.Replace('\\', '/').TrimEnd('/');
            if (string.IsNullOrWhiteSpace(normalized) || normalized == "Assets" || !normalized.StartsWith("Assets/", StringComparison.Ordinal) || normalized.Contains("/../"))
                throw new ArgumentException("Path must be a project-relative folder under Assets: " + path, argument);
            return normalized;
        }

        static void EnsureAssetFolder(string path)
        {
            string normalized = path.Replace('\\', '/').TrimEnd('/');
            string current = "Assets";
            foreach (string part in normalized.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, part);
                    if (string.IsNullOrEmpty(guid)) throw new IOException("Unable to create asset folder: " + next);
                }
                current = next;
            }
        }

        static void MoveAssetOrThrow(string source, string destination)
        {
            string error = AssetDatabase.MoveAsset(source, destination);
            if (!string.IsNullOrEmpty(error)) throw new IOException("Unable to move " + source + " to " + destination + ": " + error);
        }

        static void SetFloatIfPresent(SerializedObject value, string name, float setting)
        {
            SerializedProperty property = value.FindProperty(name);
            if (property != null) property.floatValue = setting;
        }

        static void SetIntIfPresent(SerializedObject value, string name, int setting)
        {
            SerializedProperty property = value.FindProperty(name);
            if (property != null) property.intValue = setting;
        }

        static void SetBoolIfPresent(SerializedObject value, string name, bool setting)
        {
            SerializedProperty property = value.FindProperty(name);
            if (property != null) property.boolValue = setting;
        }

        static Vector3 V3(float[] value) => new Vector3(value[0], value[1], value[2]);
        static Quaternion Q4(float[] value) => new Quaternion(value[0], value[1], value[2], value[3]);
        static Color LinearColor(float[] value) => new Color(value[0], value[1], value[2], 1f);

        readonly struct OrientedBox
        {
            readonly Vector3 center;
            readonly Vector3[] axes;
            readonly Vector3 extents;

            public OrientedBox(Vector3 center, Quaternion rotation, Vector3 extents)
                : this(center, rotation * Vector3.right, rotation * Vector3.up, rotation * Vector3.forward, extents) { }

            public OrientedBox(Vector3 center, Vector3 right, Vector3 up, Vector3 forward, Vector3 extents)
            {
                this.center = center;
                axes = new[] { right.normalized, up.normalized, forward.normalized };
                this.extents = extents;
            }

            public bool Intersects(OrientedBox other)
            {
                const float epsilon = 1e-5f;
                var rotation = new float[3, 3];
                var absolute = new float[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        rotation[i, j] = Vector3.Dot(axes[i], other.axes[j]);
                        absolute[i, j] = Mathf.Abs(rotation[i, j]) + epsilon;
                    }
                Vector3 delta = other.center - center;
                var translation = new[] { Vector3.Dot(delta, axes[0]), Vector3.Dot(delta, axes[1]), Vector3.Dot(delta, axes[2]) };
                for (int i = 0; i < 3; i++)
                {
                    float radiusA = extents[i];
                    float radiusB = other.extents.x * absolute[i, 0] + other.extents.y * absolute[i, 1] + other.extents.z * absolute[i, 2];
                    if (Mathf.Abs(translation[i]) > radiusA + radiusB) return false;
                }
                for (int j = 0; j < 3; j++)
                {
                    float radiusA = extents.x * absolute[0, j] + extents.y * absolute[1, j] + extents.z * absolute[2, j];
                    float radiusB = other.extents[j];
                    float projected = Mathf.Abs(translation[0] * rotation[0, j] + translation[1] * rotation[1, j] + translation[2] * rotation[2, j]);
                    if (projected > radiusA + radiusB) return false;
                }
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                        int j1 = (j + 1) % 3, j2 = (j + 2) % 3;
                        float radiusA = extents[i1] * absolute[i2, j] + extents[i2] * absolute[i1, j];
                        float radiusB = other.extents[j1] * absolute[i, j2] + other.extents[j2] * absolute[i, j1];
                        float projected = Mathf.Abs(translation[i2] * rotation[i1, j] - translation[i1] * rotation[i2, j]);
                        if (projected > radiusA + radiusB) return false;
                    }
                return true;
            }
        }
    }
}
