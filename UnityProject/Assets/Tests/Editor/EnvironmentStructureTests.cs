using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class EnvironmentStructureTests
    {
        [Test]
        public void PodiumIntervalsRetainTheMarkedDistrictBridgeOpening()
        {
            Assert.That(Editor.EnvironmentStructureSetup.Podium(.41f), Is.True);
            Assert.That(Editor.EnvironmentStructureSetup.Podium(.438f), Is.False);
            Assert.That(Editor.EnvironmentStructureSetup.Podium(.47f), Is.True);
            Assert.That(Editor.EnvironmentStructureSetup.Podium(.73f), Is.True);
            Assert.That(Editor.EnvironmentStructureSetup.Podium(.15f), Is.False);
        }

        [Test]
        public void CandidateHasIsolatedBatchedArtWithoutNewLightsOrColliders()
        {
            var scene=EditorSceneManager.OpenScene(Editor.EnvironmentStructureSetup.Candidate,OpenSceneMode.Additive);
            try
            {
                var world=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<ProductionWorld>(true)).Single();
                Assert.That(world.courseHash,Is.EqualTo("598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059"));
                Assert.That(world.artRevision,Is.EqualTo("environment-structure-stage1-01"));
                var root=world.transform.Cast<Transform>().SingleOrDefault(t=>t.name=="Environment stage 1 / urban viaduct and podiums");
                Assert.That(root,Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Collider>(true),Is.Empty);
                Assert.That(root.GetComponentsInChildren<Light>(true),Is.Empty);
                Assert.That(root.GetComponentsInChildren<MeshRenderer>().Length,Is.EqualTo(3));
                var old=world.GetComponentsInChildren<MeshRenderer>(true).Single(r=>r.sharedMaterial && r.sharedMaterial.name=="Preserved support slate");
                Assert.That(old.enabled,Is.False);
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
                    Assert.That(filter.sharedMesh.vertexCount,Is.GreaterThan(0));
            }
            finally { EditorSceneManager.CloseScene(scene,true); }
        }
    }
}
