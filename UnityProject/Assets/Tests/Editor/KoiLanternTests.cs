using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Tests
{
    public class KoiLanternTests
    {
        static ProductionWorld Open(string path){EditorSceneManager.OpenScene(path,OpenSceneMode.Single);return Object.FindFirstObjectByType<ProductionWorld>();}
        static string[] Physics(ProductionWorld w)=>w.GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(s=>s).ToArray();
        static string[] Campaigns()=>Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>t.name+"|"+t.localToWorldMatrix+"|"+AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial)).ToArray();
        [Test] public void LandmarkPreservesStage8CourseCollisionsWeatherAndAdvertisements()
        {
            var baseline=Open(Editor.FullLapAdsSetup.Candidate);string hash=baseline.courseHash;var physics=Physics(baseline);var ads=Campaigns();float fog=RenderSettings.fogDensity;var shelter=Object.FindFirstObjectByType<RainPresentation>().shelterProfile;
            var candidate=Open(Editor.KoiLanternSetup.Candidate);
            Assert.That(candidate.courseHash,Is.EqualTo(hash));CollectionAssert.AreEqual(physics,Physics(candidate));CollectionAssert.AreEqual(ads,Campaigns());Assert.That(RenderSettings.fogDensity,Is.EqualTo(fog));Assert.That(Object.FindFirstObjectByType<RainPresentation>().shelterProfile,Is.EqualTo(shelter));
        }
        [Test] public void KoiIsACompleteSupportedMeshAssetOutsideTheFullDrivingCorridor()
        {
            var world=Open(Editor.KoiLanternSetup.Candidate);var motion=Object.FindFirstObjectByType<KoiLanternMotion>();Assert.That(motion,Is.Not.Null);Assert.That(motion.fins.Length,Is.EqualTo(4));Assert.That(motion.fins.All(f=>f&&f.GetComponent<MeshRenderer>()),Is.True);
            Assert.That(Editor.KoiLanternSetup.ValidateSite(world,motion.transform),Is.GreaterThan(34));
            var mesh=motion.GetComponentsInChildren<MeshFilter>();Assert.That(mesh.Length,Is.GreaterThan(8));Assert.That(mesh.All(m=>m.sharedMesh&&m.sharedMesh.vertexCount>0),Is.True);
            Assert.That(motion.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader.isSupported)),Is.True);
            var enclosure=new Bounds(mesh[0].transform.TransformPoint(mesh[0].sharedMesh.bounds.center),Vector3.zero);foreach(var m in mesh)enclosure.Encapsulate(m.GetComponent<Renderer>().bounds);
            Assert.That(enclosure.min.y,Is.LessThanOrEqualTo(-38.9f));Assert.That(enclosure.max.y,Is.GreaterThan(80));
        }
    }
}
