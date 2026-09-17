using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Tests
{
 public class HolographicKoiTests
 {
  static ProductionWorld Open(string scene){EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);return Object.FindFirstObjectByType<ProductionWorld>();}
  static string[] Physics(ProductionWorld w)=>w.GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(s=>s).ToArray();
  [Test] public void LuminousCandidatePreservesCourseCollisionsWeatherAndAdPlacement()
  {
   var baseline=Open(Editor.FullLapAdsSetup.Candidate);string hash=baseline.courseHash;var physics=Physics(baseline);float fog=RenderSettings.fogDensity;var shelter=Object.FindFirstObjectByType<RainPresentation>().shelterProfile;var ads=Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>t.localToWorldMatrix).ToArray();
   var current=Open(Editor.HolographicKoiSetup.Candidate);Assert.That(current.courseHash,Is.EqualTo(hash));CollectionAssert.AreEqual(physics,Physics(current));Assert.That(RenderSettings.fogDensity,Is.EqualTo(fog));Assert.That(Object.FindFirstObjectByType<RainPresentation>().shelterProfile,Is.EqualTo(shelter));CollectionAssert.AreEqual(ads,Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>t.localToWorldMatrix).ToArray());
   Assert.That(Object.FindFirstObjectByType<KoiLanternMotion>(),Is.Null,"Rejected ceramic model must not be in the new candidate");
  }
  [Test] public void ProjectionUsesCoherentLuminousMeshesAndDedicatedLiveReflection()
  {
   var world=Open(Editor.HolographicKoiSetup.Candidate);var holo=Object.FindFirstObjectByType<HolographicKoi>();Assert.That(holo,Is.Not.Null);Assert.That(holo.fish.GetComponentsInChildren<MeshRenderer>().Length,Is.EqualTo(5));
   foreach(var r in holo.fish.GetComponentsInChildren<MeshRenderer>()){Assert.That(r.gameObject.layer,Is.EqualTo(29));Assert.That(r.sharedMaterial.shader.name,Is.EqualTo("VectorRush/Holographic Koi"));Assert.That(ShaderUtil.ShaderHasError(r.sharedMaterial.shader),Is.False);}
   var eye=holo.fish.GetComponentsInChildren<MeshFilter>().Single(f=>f.sharedMesh.name=="Koi eye");Assert.That(eye.sharedMesh.bounds.center.x,Is.GreaterThan(0),"Imported head is positive X; tail-wave distance must use negative X");
   Assert.That(holo.reflectionMaterial.shader.name,Is.EqualTo("VectorRush/Koi Road Reflection"));Assert.That(ShaderUtil.ShaderHasError(holo.reflectionMaterial.shader),Is.False);Assert.That(holo.GetComponentsInChildren<Collider>().Length,Is.Zero);
   Assert.That(Editor.HolographicKoiSetup.ValidateClearance(world,holo.fish,holo.transform.Find("Projection podium")),Is.GreaterThan(49));
   Assert.That(Editor.HolographicKoiSetup.ValidateSwimClearance(world,holo),Is.GreaterThanOrEqualTo(7));
   var original=holo.fish.position;holo.ApplySwimPose(0);var start=holo.fish.position;holo.ApplySwimPose(holo.period*.25f);Assert.That(Vector3.Distance(start,holo.fish.position),Is.GreaterThan(20));holo.fish.position=original;
   var ads=Object.FindFirstObjectByType<AnimatedBillboards>().displays;Assert.That(ads.Count(t=>AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial).Contains("Subdued-sign")),Is.EqualTo(2));
  }
 }
}
