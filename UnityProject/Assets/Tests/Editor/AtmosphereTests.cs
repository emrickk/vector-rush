using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Tests
{
 public class AtmosphereTests
 {
  [Test] public void CoveredRoadIsDryAndDistantRainFallsQuiet()
  {
   Assert.That(RainShelterProfile.WetnessAtDistance(1),Is.Zero);
   Assert.That(RainShelterProfile.WetnessAtDistance(40),Is.Zero);
   Assert.That(RainShelterProfile.WetnessAtDistance(-9),Is.EqualTo(1));
   Assert.That(RainShelterProfile.WetnessAtDistance(-4),Is.EqualTo(.5f).Within(.001));
   Assert.That(RainShelterProfile.RainAudibilityAtDistance(40),Is.LessThan(.001));
   Assert.That(RainShelterProfile.FogExposureAtDistance(25),Is.Zero);
  }
  [Test] public void SpatialWetnessDoesNotChangeRoadOrCollisionGeometry()
  {
   EditorSceneManager.OpenScene(Editor.AtmosphereSceneSetup.Candidate,OpenSceneMode.Single);
   var world=Object.FindFirstObjectByType<ProductionWorld>();
   var road=world.GetComponentsInChildren<MeshCollider>().Single(c=>c.name.StartsWith("Running surface"));
   var filter=road.GetComponent<MeshFilter>();
   CollectionAssert.AreEqual(road.sharedMesh.vertices,filter.sharedMesh.vertices);
   Assert.That(filter.sharedMesh.triangles.Length,Is.EqualTo(road.sharedMesh.triangles.Length));
   Assert.That(world.courseHash,Is.EqualTo(Editor.ProductionSceneSetup.CourseHash(world.track)));
   var materials=road.GetComponent<MeshRenderer>().sharedMaterials;
   Assert.That(materials.Length,Is.EqualTo(17));
   Assert.That(materials[0].GetFloat("_Smoothness"),Is.EqualTo(.32f).Within(.001));
   Assert.That(materials[16].GetFloat("_Smoothness"),Is.EqualTo(.94f).Within(.001));
  }
  [Test] public void BakedSheltersHaveNoWetTrianglesInTheirInterior()
  {
   EditorSceneManager.OpenScene(Editor.AtmosphereSceneSetup.Candidate,OpenSceneMode.Single);
   var weather=Object.FindFirstObjectByType<RainPresentation>();var profile=weather.shelterProfile;
   Assert.That(profile.signedRoofDistance.Count(x=>x>20),Is.GreaterThan(50));
   var world=Object.FindFirstObjectByType<ProductionWorld>();
   var road=world.GetComponentsInChildren<MeshCollider>().Single(c=>c.name.StartsWith("Running surface"));
   var mesh=road.GetComponent<MeshFilter>().sharedMesh;int stride=mesh.vertexCount/3841;
   for(int level=1;level<mesh.subMeshCount;level++){
    var indices=mesh.GetTriangles(level);
    for(int i=0;i<indices.Length;i+=3){float progress=(indices[i]/stride+indices[i+1]/stride+indices[i+2]/stride)/(3f*3840);Assert.That(profile.Distance(progress),Is.LessThan(0));}
   }
  }
 }
}
