using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class AAAImportContractTests
    {
        static Type EditorType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("VectorRush.Editor." + name);
                if (type != null) return type;
            }
            throw new InvalidOperationException("Missing editor type " + name);
        }

        [Test]
        public void SharedCourseIdentityMatchesPinnedFloatBitsAcrossJsonSpellings()
        {
            var trackObject = new GameObject("AAA identity contract");
            try
            {
                var track = trackObject.AddComponent<TrackPath>();
                string shared = AAACourseIdentity.Compute(track);
                // Independently computed from both retained JSON exports. Their last
                // decimal digits differ, but all 1,201 frames have identical float bits.
                Assert.That(shared, Is.EqualTo("8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e"));
                Assert.That(AAACourseIdentity.IsCanonical(shared), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(trackObject); }
        }

        [Test]
        public void RuntimeGateRejectsPayloadFromDifferentCourse()
        {
            var trackObject = new GameObject("AAA runtime identity gate");
            var payload = ScriptableObject.CreateInstance<AAAExemplar>();
            try
            {
                var track = trackObject.AddComponent<TrackPath>();
                payload.courseHash = AAACourseIdentity.Compute(track);
                track.Width += 1;
                string issue = AAABaselineIntegration.ValidateForTrack(payload, track);
                Assert.That(issue, Does.StartWith("course identity mismatch"));
                Assert.That(trackObject.GetComponent<AAABaselineIntegration>(), Is.Null,
                    "Identity validation must not attach a runtime component.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(payload);
                UnityEngine.Object.DestroyImmediate(trackObject);
            }
        }

        [Test]
        public void RequiredPublicationRejectsEitherMissingResource()
        {
            var payload = ScriptableObject.CreateInstance<AAAExemplar>();
            try
            {
                MethodInfo method = EditorType("AAAExemplarValidation").GetMethod("RequirePublishedResources");
                Assert.That(method, Is.Not.Null);
                var missingGallery = Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { payload, null }));
                Assert.That(missingGallery.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(missingGallery.InnerException.Message, Does.Contain(AAABaselineIntegration.GalleryResource));

                var missingOpening = Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { null, payload }));
                Assert.That(missingOpening.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(missingOpening.InnerException.Message, Does.Contain(AAABaselineIntegration.OpeningResource));
            }
            finally { UnityEngine.Object.DestroyImmediate(payload); }
        }

        [Test]
        public void RendererReplacementIsBlockedByColliderOnAncestor()
        {
            var parent = new GameObject("Collision-bearing baseline root");
            var child = new GameObject("Named visual child");
            child.transform.SetParent(parent.transform, false);
            var renderer = child.AddComponent<MeshRenderer>();
            var collider = parent.AddComponent<BoxCollider>();
            try
            {
                Assert.That(AAABaselineIntegration.CanDisableRendererForReplacement(renderer), Is.False);
                UnityEngine.Object.DestroyImmediate(collider);
                Assert.That(AAABaselineIntegration.CanDisableRendererForReplacement(renderer), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(parent); }
        }
    }
}
