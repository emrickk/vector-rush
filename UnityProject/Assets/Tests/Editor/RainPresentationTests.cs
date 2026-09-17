using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Tests
{
 public class RainPresentationTests
 {
  [Test] public void RainAudioRespectsCoverMuteAndPause()
  {
   Assert.That(RainPresentation.AudioLevel(1,0,false),Is.Zero);
   Assert.That(RainPresentation.AudioLevel(1,1,true),Is.Zero);
   Assert.That(RainPresentation.AudioLevel(0,1,false),Is.LessThan(RainPresentation.AudioLevel(1,1,false)*.2f));
  }
  [Test] public void RoofSuppressesRainButVehicleLayerDoesNot()
  {
   var roof=GameObject.CreatePrimitive(PrimitiveType.Cube);
   try {
    roof.transform.position=new Vector3(10000,15,10000);roof.transform.localScale=new Vector3(20,1,20);Physics.SyncTransforms();
    Assert.That(RainPresentation.IsSheltered(new Vector3(10000,10,10000)),Is.True);
    roof.layer=8;Physics.SyncTransforms();Assert.That(RainPresentation.IsSheltered(new Vector3(10000,10,10000)),Is.False);
   }finally{Object.DestroyImmediate(roof);}
  }
  [Test] public void RainCandidatePreservesDrivingMeshesAndCourse()
  {
   var dry=EditorSceneManager.OpenScene(Editor.SmoothRoadSetup.Candidate,OpenSceneMode.Single);
   var before=Object.FindFirstObjectByType<ProductionWorld>();string hash=before.courseHash;
   var meshes=before.GetComponentsInChildren<MeshCollider>().Select(c=>c.sharedMesh).ToArray();
   var wet=EditorSceneManager.OpenScene(Editor.RainSceneSetup.Candidate,OpenSceneMode.Single);
   var after=Object.FindFirstObjectByType<ProductionWorld>();Assert.That(after.courseHash,Is.EqualTo(hash));
   CollectionAssert.AreEqual(meshes,after.GetComponentsInChildren<MeshCollider>().Select(c=>c.sharedMesh).ToArray());
   var rain=Object.FindFirstObjectByType<RainPresentation>();Assert.That(rain,Is.Not.Null);Assert.That(rain.rainLoop,Is.Not.Null);
   Assert.That(rain.rainLoop.length,Is.GreaterThan(5));
  }
 }
}
