using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class ProductionBoundaryTests
    {
        [Test]
        public void ProductionWorldRuntimeBoundaryIsAvailable()
        {
            Assert.That(Type.GetType("VectorRush.ProductionWorld, VectorRush.Runtime"), Is.Not.Null);
        }

        [Test]
        public void ProductionMaterialLibraryRuntimeBoundaryIsAvailable()
        {
            Assert.That(Type.GetType("VectorRush.ProductionMaterialLibrary, VectorRush.Runtime"), Is.Not.Null);
        }

        [Test]
        public void CraftMaterialFactoryRuntimeBoundaryIsAvailable()
        {
            Assert.That(Type.GetType("VectorRush.CraftMaterialFactory, VectorRush.Runtime"), Is.Not.Null);
        }

        [Test]
        public void ProductionSceneSetupExposesAllBatchEntrypoints()
        {
            Type type = FindEditorType("VectorRush.Editor.ProductionSceneSetup");
            Assert.That(type, Is.Not.Null);
            foreach (string method in new[] { "ExportContext", "ImportArt", "BuildScene", "BakeScene", "BuildCandidate" })
                Assert.That(type.GetMethod(method, BindingFlags.Static | BindingFlags.Public), Is.Not.Null, method);
        }

        [Test]
        public void ContextExportWritesExactClosedCourseAndHashesItsBytes()
        {
            string output = Path.Combine(Path.GetTempPath(), "VectorRush-Context-" + Guid.NewGuid().ToString("N"));
            var trackObject = new GameObject("Context export track");
            try
            {
                var track = trackObject.AddComponent<TrackPath>();
                MethodInfo export = FindEditorType("VectorRush.Editor.ProductionSceneSetup")
                    .GetMethod("ExportContextToDirectory", BindingFlags.Static | BindingFlags.Public);
                Assert.That(export, Is.Not.Null);
                export.Invoke(null, new object[] { track, output, "test-source-commit" });

                string coursePath = Path.Combine(output, "course-data.json");
                string geometryPath = Path.Combine(output, "geometry-context.json");
                Assert.That(File.Exists(coursePath), Is.True);
                Assert.That(File.Exists(geometryPath), Is.True);
                Assert.That(File.Exists(Path.Combine(output, "C1_STATUS.md")), Is.True);
                byte[] courseBytes = File.ReadAllBytes(coursePath);
                string expectedCourseHash = Sha256(courseBytes);
                string geometry = File.ReadAllText(geometryPath);
                Assert.That(geometry, Does.Contain("\"courseHash\":\"" + expectedCourseHash + "\""));
                Assert.That(geometry, Does.Contain("\"sourceCommit\":\"test-source-commit\""));
                Assert.That(File.ReadAllText(Path.Combine(output, "geometry-context.sha256")).Trim(), Is.EqualTo(Sha256(File.ReadAllBytes(geometryPath))));

                var data = JsonUtility.FromJson<CourseData>(Encoding.UTF8.GetString(courseBytes));
                Assert.That(data.width, Is.EqualTo(22f));
                Assert.That(data.frames, Has.Length.EqualTo(1201));
                Assert.That(data.frames[0].progress, Is.Zero);
                Assert.That(data.frames[1200].progress, Is.EqualTo(1f));
                Assert.That(data.frames[1200].distanceMeters, Is.EqualTo(data.length).Within(.001f));
                Assert.That(data.frames[1200].position, Is.EqualTo(data.frames[0].position));
                Assert.That(data.frames[1200].forward, Is.EqualTo(data.frames[0].forward));
                Assert.That(data.frames[1200].right, Is.EqualTo(data.frames[0].right));
                Assert.That(data.frames[1200].up, Is.EqualTo(data.frames[0].up));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(trackObject);
                if (Directory.Exists(output)) Directory.Delete(output, true);
            }
        }

        [Test]
        public void ProductionWorldExposesTheSceneAuthoredContract()
        {
            Type type = Type.GetType("VectorRush.ProductionWorld, VectorRush.Runtime");
            Assert.That(type.GetField("track"), Is.Not.Null);
            Assert.That(type.GetField("materials"), Is.Not.Null);
            Assert.That(type.GetField("courseHash"), Is.Not.Null);
            Assert.That(type.GetField("artRevision"), Is.Not.Null);
            Assert.That(type.GetMethod("ValidateReady", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
        }

        [Test]
        public void CraftFactoryExposesOwnedRuntimeMaterialContract()
        {
            Type type = Type.GetType("VectorRush.CraftMaterialFactory, VectorRush.Runtime");
            Assert.That(typeof(IDisposable).IsAssignableFrom(type), Is.True);
            Assert.That(type.GetConstructor(new[] { typeof(ProductionMaterialLibrary) }), Is.Not.Null);
            Assert.That(type.GetMethod("Create", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
            Assert.That(type.GetMethod("CreateMapped", BindingFlags.Instance | BindingFlags.Public), Is.Not.Null);
        }

        [Test]
        public void ProductionWorldRejectsMissingTrackWithoutGeneratingAnything()
        {
            var root = new GameObject("Production world test");
            var world = root.AddComponent<ProductionWorld>();
            try
            {
                var error = Assert.Throws<InvalidOperationException>(() => world.ValidateReady());
                Assert.That(error.Message, Does.Contain("track"));
                Assert.That(root.transform.childCount, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ProductionWorldAcceptsCompleteIdentitySceneWithoutGeneratingAnything()
        {
            var root = new GameObject("Production world test");
            var trackObject = new GameObject("Production track test");
            var world = root.AddComponent<ProductionWorld>();
            var library = ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            var material = NewMaterial();
            try
            {
                world.track = trackObject.AddComponent<TrackPath>();
                world.materials = library;
                library.craftSurfaceTemplate = material;
                library.craftMetalFallback = material;
                library.craftGlass = material;
                library.craftEngineAccent = material;
                library.craftEngineCore = material;
                world.courseHash = new string('a', 64);
                world.artRevision = "art-package-01";
                world.ValidateReady();
                Assert.That(root.transform.childCount, Is.Zero);
                Assert.That(trackObject.transform.childCount, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(library);
                UnityEngine.Object.DestroyImmediate(trackObject);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ProductionWorldRejectsMovedTrackRoot()
        {
            var root = new GameObject("Production world test");
            var trackObject = new GameObject("Production track test");
            var world = root.AddComponent<ProductionWorld>();
            var library = ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            var material = NewMaterial();
            try
            {
                world.track = trackObject.AddComponent<TrackPath>();
                trackObject.transform.position = Vector3.right;
                world.materials = library;
                library.craftSurfaceTemplate = material;
                library.craftMetalFallback = material;
                library.craftGlass = material;
                library.craftEngineAccent = material;
                library.craftEngineCore = material;
                world.courseHash = new string('a', 64);
                world.artRevision = "art-package-01";
                var error = Assert.Throws<InvalidOperationException>(() => world.ValidateReady());
                Assert.That(error.Message, Does.Contain("identity"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(library);
                UnityEngine.Object.DestroyImmediate(trackObject);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CraftFactoryOwnsOnlyTheMaterialInstancesItCreates()
        {
            var library = ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            var template = NewMaterial();
            library.craftSurfaceTemplate = template;
            var factory = new CraftMaterialFactory(library);
            Material instance = factory.Create("Team accent", Color.red, .4f, .2f);
            Assert.That(instance, Is.Not.SameAs(template));
            Assert.That(instance.name, Is.EqualTo("Team accent"));
            Assert.That(instance.GetFloat("_Smoothness"), Is.EqualTo(.4f).Within(.0001f));
            factory.Dispose();
            factory.Dispose();
            Assert.That(instance == null, Is.True);
            Assert.That(template == null, Is.False);
            UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(library);
        }

        [Test]
        public void CraftFactoryCannotCreateAfterDisposal()
        {
            var library = ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            var template = NewMaterial();
            library.craftSurfaceTemplate = template;
            var factory = new CraftMaterialFactory(library);
            factory.Dispose();
            Assert.Throws<ObjectDisposedException>(() => factory.Create("Late material", Color.white));
            UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(library);
        }

        [Test]
        public void ProductionBootstrapUsesTheSavedTrackAndSingleSavedCamera()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrapObject = new GameObject("Production bootstrap test");
            var worldObject = new GameObject("Production world test");
            var cameraObject = new GameObject("Production camera test");
            var library = ScriptableObject.CreateInstance<ProductionMaterialLibrary>();
            var material = NewMaterial();
            try
            {
                var bootstrap = bootstrapObject.AddComponent<VectorBootstrap>();
                var world = worldObject.AddComponent<ProductionWorld>();
                world.track = worldObject.AddComponent<TrackPath>();
                world.materials = library;
                library.craftSurfaceTemplate = material;
                library.craftMetalFallback = material;
                library.craftGlass = material;
                library.craftEngineAccent = material;
                library.craftEngineCore = material;
                world.courseHash = new string('a', 64);
                world.artRevision = "DIAGNOSTIC_ONLY";
                var savedCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();

                FieldInfo productionField = typeof(VectorBootstrap).GetField("productionWorld");
                Assert.That(productionField, Is.Not.Null, "The scene must serialize the production runtime boundary.");
                productionField.SetValue(bootstrap, world);
                Invoke(bootstrap, "UseProductionWorld");

                Assert.That(bootstrap.Track, Is.SameAs(world.track));
                Assert.That(bootstrap.Camera, Is.SameAs(savedCamera));
                Assert.That(UnityEngine.Object.FindObjectsByType<WorldBuilder>(FindObjectsSortMode.None), Is.Empty);
                int gameplayCameras = 0;
                foreach (var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                    if (camera.gameObject.scene == worldObject.scene && camera.CompareTag("MainCamera")) gameplayCameras++;
                Assert.That(gameplayCameras, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(worldObject);
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(library);
            }
        }

        static void Invoke(object target, string method)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
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

        static string Sha256(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var text = new StringBuilder(64);
                foreach (byte value in sha.ComputeHash(bytes)) text.Append(value.ToString("x2"));
                return text.ToString();
            }
        }

        [Serializable]
        sealed class CourseData
        {
            public float width;
            public float length;
            public CourseFrame[] frames;
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

        static Material NewMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Hidden/InternalErrorShader");
            return new Material(shader);
        }
    }
}
