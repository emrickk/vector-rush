using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class AAAGalleryIntegrationTests
    {
        readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i]) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        [Test]
        public void AtomicTargetSetRequiresExactWarmDetailsAndExpectedLocalLights()
        {
            GameObject root = Own(new GameObject("world"));
            TrackPath track = root.AddComponent<TrackPath>();
            root.AddComponent<NightTrackLighting>();
            foreach (string name in NightTrackLighting.ReplaceableWarmRendererNames)
            {
                GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube); owned.Add(part);
                part.name = name; part.transform.SetParent(root.transform, false);
                UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            }
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Cube); owned.Add(shell);
            shell.name = NightTrackLighting.WarmContinuousShell; shell.transform.SetParent(root.transform, false);
            UnityEngine.Object.DestroyImmediate(shell.GetComponent<Collider>());
            TrackFrame frame = track.Evaluate(.88f);
            AddLights(root, NightTrackLighting.WarmSurfaceWash, NightTrackLighting.WarmSurfaceWashCount, frame.Position + frame.Up * 10);
            AddLights(root, NightTrackLighting.WarmRoadPool, NightTrackLighting.WarmRoadPoolCount, frame.Position + frame.Up * 14);
            AAAExemplar payload = Own(ScriptableObject.CreateInstance<AAAExemplar>());
            payload.atomicReplacement = true;
            payload.rendererReplacements = RendererTargets(); payload.lightReplacements = LightTargets();

            Assert.That(AAABaselineIntegration.ValidateGalleryReplacementTargets(payload, root.transform, track), Is.Null);
            root.transform.Find(NightTrackLighting.WarmCassettes).gameObject.SetActive(false);
            Assert.That(AAABaselineIntegration.ValidateGalleryReplacementTargets(payload, root.transform, track), Is.Null,
                "Inactive baseline children remain valid atomic targets.");
            UnityEngine.Object.DestroyImmediate(root.transform.Find(NightTrackLighting.WarmReturns).gameObject);
            Assert.That(AAABaselineIntegration.ValidateGalleryReplacementTargets(payload, root.transform, track),
                Does.StartWith("gallery renderer target count mismatch"));
        }

        [Test]
        public void ClosedReplacementContractRejectsProtectedShellAndPartialCoverage()
        {
            AAAExemplar payload = ValidPayload();
            payload.rendererReplacements[0].name = NightTrackLighting.WarmContinuousShell;
            Assert.That(AAABaselineIntegration.Validate(payload), Does.Contain("closed warm-detail set"));
            payload.rendererReplacements = RendererTargets();
            payload.placements[payload.placements.Length - 1].progress = .89f;
            Assert.That(AAABaselineIntegration.Validate(payload), Does.Contain("reach both full-span boundaries"));
        }

        [Test]
        public void GalleryFixtureContractRejectsNonLocalTypeAndExcessIntensity()
        {
            AAAExemplar payload = ValidPayload();
            Assert.That(AAABaselineIntegration.Validate(payload), Is.Null);
            payload.lights[0].type = LightType.Directional;
            Assert.That(AAABaselineIntegration.Validate(payload), Does.Contain("point and spot"));
            payload.lights[0].type = LightType.Spot;
            payload.lights[0].intensity = 2001;
            Assert.That(AAABaselineIntegration.Validate(payload), Does.Contain("intensity or range"));
            payload.lights[0].intensity = 350;
            payload.lights[0].socket = null;
            Assert.That(AAABaselineIntegration.Validate(payload), Does.Contain("housing placement and socket"));
        }

        [Test]
        public void GalleryPublicationRequiresGalleryButHasNoOpeningDependency()
        {
            AAAExemplar gallery = Own(ScriptableObject.CreateInstance<AAAExemplar>());
            MethodInfo method = EditorType("AAAExemplarValidation").GetMethod("RequireGalleryResource");
            Assert.That(method, Is.Not.Null);
            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { gallery }),
                "A gallery-only publication has no opening-resource dependency.");
            var missingGallery = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { null }));
            Assert.That(missingGallery.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(missingGallery.InnerException.Message, Does.Contain(AAABaselineIntegration.GalleryResource));
        }

        AAAExemplar ValidPayload()
        {
            var payload = Own(ScriptableObject.CreateInstance<AAAExemplar>());
            payload.revision = "exemplar-01-gallery02/station/BLOCKED_VISUAL_REVIEW";
            payload.courseHash = new string('a', 64); payload.sourceCourseSha256 = new string('b', 64);
            payload.startProgress = AAABaselineIntegration.WarmGalleryStart;
            payload.endProgress = AAABaselineIntegration.WarmGalleryEnd;
            payload.fullSpanLayout = true; payload.atomicReplacement = true;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Assert.That(shader, Is.Not.Null);
            var material = Own(new Material(shader)); material.name = "TestGalleryMaterial";
            var prefab = GameObject.CreatePrimitive(PrimitiveType.Cube); owned.Add(prefab); prefab.name = "TestGalleryPrefab";
            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<Collider>());
            prefab.GetComponent<MeshRenderer>().sharedMaterial = material;
            payload.materials = new[] { new AAAExemplar.MaterialBinding { sourceSlotName = material.name, material = material } };
            payload.placements = new[] {
                Placement("entrance", prefab, .86f), Placement("bay-1", prefab, .867f),
                Placement("bay-2", prefab, .874f), Placement("bay-3", prefab, .881f),
                Placement("bay-4", prefab, .888f), Placement("bay-5", prefab, .895f),
                Placement("exit", prefab, AAABaselineIntegration.WarmGalleryEnd - .0004f)
            };
            payload.rendererReplacements = RendererTargets(); payload.lightReplacements = LightTargets();
            payload.lights = new[] { Fixture("pool-1", "entrance"), Fixture("pool-2", "entrance"), Fixture("pool-3", "exit") };
            return payload;
        }

        static AAAExemplar.Placement Placement(string name, GameObject prefab, float progress) => new AAAExemplar.Placement {
            semanticGroup = name, prefab = prefab, progress = progress, followBank = true, scale = 1,
            replacementVolume = new Bounds(Vector3.zero, Vector3.zero), replaceRendererNames = Array.Empty<string>(), replaceLightNames = Array.Empty<string>()
        };

        static AAAExemplar.LightFixture Fixture(string id, string housing) => new AAAExemplar.LightFixture {
            id = id, type = LightType.Spot, worldDirection = Vector3.down, color = new Color(1, .72f, .46f),
            intensity = 350, range = 30, outerAngle = 74, innerAngle = 36, shadows = LightShadows.None,
            housingSemanticGroup = housing, socket = "test diffuser socket"
        };

        static AAAExemplar.ReplacementTarget[] RendererTargets()
        {
            var result = new AAAExemplar.ReplacementTarget[NightTrackLighting.ReplaceableWarmRendererNames.Length];
            for (int i = 0; i < result.Length; i++) result[i] = new AAAExemplar.ReplacementTarget {
                name = NightTrackLighting.ReplaceableWarmRendererNames[i], expectedCount = 1 };
            return result;
        }

        static AAAExemplar.ReplacementTarget[] LightTargets() => new[] {
            new AAAExemplar.ReplacementTarget { name = NightTrackLighting.WarmSurfaceWash, expectedCount = NightTrackLighting.WarmSurfaceWashCount },
            new AAAExemplar.ReplacementTarget { name = NightTrackLighting.WarmRoadPool, expectedCount = NightTrackLighting.WarmRoadPoolCount }
        };

        void AddLights(GameObject root, string name, int count, Vector3 position)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject(name); go.transform.SetParent(root.transform, false); go.transform.position = position;
                go.AddComponent<Light>().type = LightType.Spot;
            }
        }

        static Type EditorType(string name)
        {
            Type type = Type.GetType("VectorRush.Editor." + name + ", Assembly-CSharp-Editor");
            if (type != null) return type;
            throw new InvalidOperationException("Missing editor type " + name);
        }

        T Own<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }
    }
}
