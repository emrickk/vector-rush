using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace VectorRush.Editor
{
    public static class ProductionSceneSetup
    {
        const int ContractVersion = 1;
        const int ContextSamples = 1200;

        public static void ExportContext()
        {
            string output = RequireAbsoluteFlag("-productionEvidence");
            var trackObject = new GameObject("Nocturne production context track");
            try
            {
                ExportContextToDirectory(trackObject.AddComponent<TrackPath>(), output, ReadSourceCommit());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(trackObject);
            }
        }

        public static void ImportArt()
        {
            string packagePath = RequireAbsoluteFlag("-productionArtPackage");
            string evidencePath = RequireAbsoluteFlag("-productionEvidence");
            EnsureFreshAbsoluteDirectory(evidencePath);
            string courseHash = CurrentCourseHash();
            ProductionImportResult result = ProductionArtImporter.ImportPackage(packagePath, courseHash);
            WriteJson(Path.Combine(evidencePath, "import-result.json"), new ImportEvidence
            {
                contractVersion = ContractVersion,
                revision = result.revision,
                courseHash = courseHash,
                packageDirectory = packagePath,
                artAssetRoot = result.artAssetRoot,
                payloadAssetRoot = result.payloadAssetRoot,
                worldAssetRoot = result.worldAssetRoot,
                readySha256 = Hash(File.ReadAllBytes(Path.Combine(packagePath, "READY.json")))
            });
            File.WriteAllText(Path.Combine(evidencePath, "C2_IMPORT_STATUS.md"), "# C2 production art import\n\nStatus: COMPLETE\n\nRevision: `" + result.revision + "`\nCourse hash: `" + courseHash + "`\nThe immutable payload copy, configured runtime imports, persistent URP materials, and persistent LOD prefabs were verified before commit.\n", new UTF8Encoding(false));
            UnityEngine.Debug.Log("PRODUCTION_ART_IMPORTED revision=" + result.revision + " courseHash=" + courseHash);
        }

        public static void BuildScene()
        {
            string packagePath = RequireAbsoluteFlag("-productionArtPackage");
            string evidencePath = RequireAbsoluteFlag("-productionEvidence");
            EnsureFreshAbsoluteDirectory(evidencePath);
            string courseHash = CurrentCourseHash();
            ProductionImportResult import = ProductionArtImporter.ResolveImportedPackage(packagePath, courseHash);
            ProductionSceneBuildResult result = ProductionSceneBuilder.BuildScene(import);
            WriteJson(Path.Combine(evidencePath, "scene-build-result.json"), new SceneBuildEvidence
            {
                contractVersion = ContractVersion,
                revision = import.revision,
                courseHash = courseHash,
                sceneAssetPath = result.sceneAssetPath,
                settingsAssetRoot = result.settingsAssetRoot,
                instanceCount = result.instanceCount,
                lightCount = result.lightCount,
                reflectionProbeCount = result.reflectionProbeCount
            });
            File.WriteAllText(Path.Combine(evidencePath, "C2_SCENE_STATUS.md"), "# C2 persistent production scene\n\nStatus: COMPLETE\n\nScene: `" + result.sceneAssetPath + "`\nRevision: `" + import.revision + "`\nCourse hash: `" + courseHash + "`\nThe saved scene was reopened and checked for persistent mesh/material references, one track, one production world, one active MainCamera, and no legacy WorldBuilder.\n", new UTF8Encoding(false));
            UnityEngine.Debug.Log("PRODUCTION_SCENE_BUILT " + result.sceneAssetPath + " revision=" + import.revision);
        }

        public static void BakeScene()
        {
            string evidencePath = RequireAbsoluteFlag("-productionEvidence");
            EnsureFreshAbsoluteDirectory(evidencePath);
            const string scenePath = "Assets/Scenes/NocturneProduction.unity";
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)) throw new FileNotFoundException("Production scene is unavailable; run BuildScene first", scenePath);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ProductionWorld world = UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            if (!world) throw new InvalidDataException("Production scene has no ProductionWorld");
            world.ValidateReady();
            if (!Lightmapping.Bake()) throw new InvalidOperationException("Unity lightmap/probe bake failed");
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            WriteJson(Path.Combine(evidencePath, "bake-result.json"), new BakeEvidence
            {
                contractVersion = ContractVersion,
                revision = world.artRevision,
                courseHash = world.courseHash,
                sceneAssetPath = scenePath,
                lightmapCount = LightmapSettings.lightmaps == null ? 0 : LightmapSettings.lightmaps.Length,
                lightProbeCount = LightmapSettings.lightProbes == null ? 0 : LightmapSettings.lightProbes.count
            });
            UnityEngine.Debug.Log("PRODUCTION_SCENE_BAKED " + scenePath + " revision=" + world.artRevision);
        }

        public static void BuildCandidate()
        {
            string evidencePath = RequireAbsoluteFlag("-productionEvidence");
            string outputPath = RequireAbsoluteFlag("-productionBuildOutput");
            EnsureFreshAbsoluteDirectory(evidencePath);
            if (Directory.Exists(outputPath) || File.Exists(outputPath)) throw new IOException("Production build output must be fresh: " + outputPath);
            if (!string.Equals(Path.GetExtension(outputPath), ".app", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Production build output must be an absolute fresh .app path: " + outputPath);
            string originalOutput = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Builds", "Vector Rush.app"));
            if (string.Equals(Path.GetFullPath(outputPath), originalOutput, StringComparison.Ordinal)) throw new InvalidOperationException("Production candidate must not overwrite the original Vector Rush.app");

            const string scenePath = "Assets/Scenes/NocturneProduction.unity";
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)) throw new FileNotFoundException("Production scene is unavailable; run BuildScene first", scenePath);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ProductionWorld world = UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            if (!world) throw new InvalidDataException("Production scene has no ProductionWorld");
            world.ValidateReady();
            if (!world.layoutComplete) throw new InvalidDataException("Production package layoutComplete is false; refusing to package an exemplar/diagnostic scene as the full candidate");
            if (world.artRevision.IndexOf("diagnostic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(value => value.name.IndexOf("DIAGNOSTIC_ONLY", StringComparison.OrdinalIgnoreCase) >= 0))
                throw new InvalidDataException("DIAGNOSTIC_ONLY content remains in the production scene");
            string[] zones = { "viaduct", "canyon", "thermal", "station" };
            foreach (string zone in zones)
                if (!GameObject.Find("NOCTURNE PRODUCTION • " + world.artRevision + "/Zone • " + zone))
                    throw new InvalidDataException("Final production scene is missing required zone: " + zone);

            ProductionRenderConfiguration render = UnityEngine.Object.FindFirstObjectByType<ProductionRenderConfiguration>();
            if (!render || !render.renderPipeline) throw new InvalidDataException("Production scene has no saved render pipeline configuration");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderPipelineAsset previousDefault = GraphicsSettings.defaultRenderPipeline;
            RenderPipelineAsset previousQuality = QualitySettings.renderPipeline;
            BuildReport report;
            try
            {
                GraphicsSettings.defaultRenderPipeline = render.renderPipeline;
                QualitySettings.renderPipeline = render.renderPipeline;
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneOSX,
                    options = BuildOptions.None
                });
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline = previousDefault;
                QualitySettings.renderPipeline = previousQuality;
            }
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Production candidate build failed: " + report.summary.result);
            WriteJson(Path.Combine(evidencePath, "build-report.json"), new BuildEvidence
            {
                contractVersion = ContractVersion,
                revision = world.artRevision,
                courseHash = world.courseHash,
                sceneAssetPath = scenePath,
                buildOutput = outputPath,
                buildGuid = report.summary.guid.ToString(),
                totalBytes = report.summary.totalSize,
                totalSeconds = (float)report.summary.totalTime.TotalSeconds,
                sourceCommit = ReadSourceCommit()
            });
            UnityEngine.Debug.Log("PRODUCTION_CANDIDATE_BUILT " + outputPath + " revision=" + world.artRevision);
        }

        public static string CurrentCourseHash()
        {
            var trackObject = new GameObject("Nocturne current course hash track");
            try
            {
                TrackPath track = trackObject.AddComponent<TrackPath>();
                track.Ensure();
                CourseFrame[] frames = BuildFrames(track);
                var course = new CourseData { width = track.Width, length = track.Length, frames = frames };
                return Hash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(course, false)));
            }
            finally { UnityEngine.Object.DestroyImmediate(trackObject); }
        }

        public static void ExportContextToDirectory(TrackPath track, string absoluteFreshDirectory, string sourceCommit)
        {
            if (!track) throw new ArgumentNullException(nameof(track));
            if (string.IsNullOrWhiteSpace(sourceCommit)) throw new ArgumentException("A source commit is required", nameof(sourceCommit));
            EnsureFreshAbsoluteDirectory(absoluteFreshDirectory);
            track.Ensure();

            CourseFrame[] frames = BuildFrames(track);

            var course = new CourseData { width = track.Width, length = track.Length, frames = frames };
            byte[] courseBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(course, false));
            string courseHash = Hash(courseBytes);
            string coursePath = Path.Combine(absoluteFreshDirectory, "course-data.json");
            File.WriteAllBytes(coursePath, courseBytes);

            var context = new GeometryContext
            {
                contractVersion = ContractVersion,
                sourceCommit = sourceCommit,
                courseHash = courseHash,
                width = track.Width,
                length = track.Length,
                camera = new CameraContext
                {
                    distance = 10.5f,
                    height = 4.6f,
                    nearClip = .18f,
                    farClip = 5000f,
                    speedNormalizationKph = 380f,
                    focusForwardBase = 8f,
                    focusForwardSpeedAdd = 5f,
                    positionDistanceSpeedAdd = .75f,
                    baseFieldOfView = 65f,
                    speedFieldOfViewAdd = 10f,
                    boostFieldOfViewAdd = 5f,
                    fieldOfViewFormula = "65 + clamp01(speedKph / 380) * 10 + (isBoosting ? 5 : 0)"
                },
                frames = frames
            };
            byte[] geometryBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(context, false));
            string geometryPath = Path.Combine(absoluteFreshDirectory, "geometry-context.json");
            File.WriteAllBytes(geometryPath, geometryBytes);
            string geometryHash = Hash(geometryBytes);
            File.WriteAllText(Path.Combine(absoluteFreshDirectory, "geometry-context.sha256"), geometryHash + "\n", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(absoluteFreshDirectory, "C1_STATUS.md"), Status(absoluteFreshDirectory, sourceCommit, courseHash, geometryHash), new UTF8Encoding(false));
            UnityEngine.Debug.Log("PRODUCTION_CONTEXT_EXPORTED " + geometryPath + " sha256=" + geometryHash + " courseHash=" + courseHash);
        }

        static string RequireAbsoluteFlag(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            string value = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.Ordinal)) continue;
                if (value != null) throw new InvalidOperationException("Duplicate command-line flag: " + name);
                if (i + 1 >= args.Length || args[i + 1].StartsWith("-", StringComparison.Ordinal))
                    throw new InvalidOperationException("Missing value for required command-line flag: " + name);
                value = args[++i];
            }
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Missing required command-line flag: " + name);
            if (!Path.IsPathRooted(value)) throw new InvalidOperationException(name + " must be an absolute path: " + value);
            return Path.GetFullPath(value);
        }

        static void EnsureFreshAbsoluteDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
                throw new ArgumentException("Production evidence must be an absolute fresh directory", nameof(path));
            if (Directory.Exists(path) || File.Exists(path))
                throw new IOException("Production evidence path already exists; choose a fresh directory: " + path);
            Directory.CreateDirectory(path);
        }

        static string ReadSourceCommit()
        {
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var info = new ProcessStartInfo("git", "rev-parse HEAD")
            {
                WorkingDirectory = repository,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();
                if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                    throw new InvalidOperationException("Unable to read source commit: " + error);
                return output;
            }
        }

        static float[] Vector(Vector3 value) => new[] { value.x, value.y, value.z };

        static CourseFrame[] BuildFrames(TrackPath track)
        {
            var frames = new CourseFrame[ContextSamples + 1];
            for (int i = 0; i <= ContextSamples; i++)
            {
                float progress = i / (float)ContextSamples;
                TrackFrame frame = track.Evaluate(progress);
                frames[i] = new CourseFrame
                {
                    progress = progress,
                    distanceMeters = i == ContextSamples ? track.Length : track.Length * progress,
                    position = Vector(frame.Position),
                    forward = Vector(frame.Forward),
                    right = Vector(frame.Right),
                    up = Vector(frame.Up)
                };
            }
            return frames;
        }

        static void WriteJson(string path, object value)
        {
            File.WriteAllText(path, JsonUtility.ToJson(value, true) + "\n", new UTF8Encoding(false));
        }

        static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var result = new StringBuilder(64);
                foreach (byte value in sha.ComputeHash(bytes)) result.Append(value.ToString("x2"));
                return result.ToString();
            }
        }

        static string Status(string directory, string sourceCommit, string courseHash, string geometryHash)
        {
            return "# C1 production runtime and geometry context\n\n" +
                   "Status: COMPLETE\n\n" +
                   "- Source commit: `" + sourceCommit + "`\n" +
                   "- Course hash: `" + courseHash + "`\n" +
                   "- Geometry context SHA-256: `" + geometryHash + "`\n" +
                   "- Geometry context: `" + Path.Combine(directory, "geometry-context.json") + "`\n" +
                   "- Course data: `" + Path.Combine(directory, "course-data.json") + "`\n\n" +
                   "The production runtime uses only the saved `ProductionWorld`, its material library, and one scene-authored MainCamera. " +
                   "When the production reference is null, the legacy Solstice world/material/camera path remains active. Production validation never regenerates scenery.\n";
        }

        [Serializable]
        sealed class CourseData
        {
            public float width;
            public float length;
            public CourseFrame[] frames;
        }

        [Serializable]
        sealed class GeometryContext
        {
            public int contractVersion;
            public string sourceCommit;
            public string courseHash;
            public float width;
            public float length;
            public CameraContext camera;
            public CourseFrame[] frames;
        }

        [Serializable]
        sealed class CameraContext
        {
            public float distance;
            public float height;
            public float nearClip;
            public float farClip;
            public float speedNormalizationKph;
            public float focusForwardBase;
            public float focusForwardSpeedAdd;
            public float positionDistanceSpeedAdd;
            public float baseFieldOfView;
            public float speedFieldOfViewAdd;
            public float boostFieldOfViewAdd;
            public string fieldOfViewFormula;
        }

        [Serializable]
        sealed class CourseFrame
        {
            public float progress;
            public float distanceMeters;
            public float[] position;
            public float[] forward;
            public float[] right;
            public float[] up;
        }

        [Serializable] sealed class ImportEvidence
        {
            public int contractVersion; public string revision; public string courseHash; public string packageDirectory;
            public string artAssetRoot; public string payloadAssetRoot; public string worldAssetRoot; public string readySha256;
        }

        [Serializable] sealed class SceneBuildEvidence
        {
            public int contractVersion; public string revision; public string courseHash; public string sceneAssetPath; public string settingsAssetRoot;
            public int instanceCount; public int lightCount; public int reflectionProbeCount;
        }

        [Serializable] sealed class BakeEvidence
        {
            public int contractVersion; public string revision; public string courseHash; public string sceneAssetPath; public int lightmapCount; public int lightProbeCount;
        }

        [Serializable] sealed class BuildEvidence
        {
            public int contractVersion; public string revision; public string courseHash; public string sceneAssetPath; public string buildOutput;
            public string buildGuid; public ulong totalBytes; public float totalSeconds; public string sourceCommit;
        }
    }
}
