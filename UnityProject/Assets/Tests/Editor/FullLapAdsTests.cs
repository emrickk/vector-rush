using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public class FullLapAdsTests
    {
        static ProductionWorld Open(string scene){EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);return Object.FindFirstObjectByType<ProductionWorld>();}
        static string[] Colliders(ProductionWorld w)=>w.GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(x=>x).ToArray();
        [Test] public void FullLapRetainsRacingPhysicsWeatherAndCampaignAssets()
        {
            var baseline=Open(Editor.NightCityIntegrationSetup.Candidate);string course=baseline.courseHash;var collisions=Colliders(baseline);
            var materials=Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial)).ToArray();
            var shelter=Object.FindFirstObjectByType<RainPresentation>().shelterProfile;float fog=RenderSettings.fogDensity;
            var world=Open(Editor.FullLapAdsSetup.Candidate);
            Assert.That(world.courseHash,Is.EqualTo(course));CollectionAssert.AreEqual(collisions,Colliders(world));
            CollectionAssert.AreEqual(materials,Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial)).ToArray());
            Assert.That(Object.FindFirstObjectByType<RainPresentation>().shelterProfile,Is.EqualTo(shelter));Assert.That(RenderSettings.fogDensity,Is.EqualTo(fog));
        }
        [Test] public void AllThreeArtworksAppearAndAnimatedGroupsCoverTheLap()
        {
            var world=Open(Editor.FullLapAdsSetup.Candidate);var player=Object.FindFirstObjectByType<AnimatedBillboards>();
            foreach(string name in new[]{"After Hours","Night Market","Last Train"}) {
                var poster=GameObject.Find(name+" / full-lap artwork");Assert.That(poster,Is.Not.Null,name);
                Assert.That(poster.GetComponentsInChildren<MeshRenderer>().Any(r=>r.sharedMaterials.Any(m=>m&&m.name.StartsWith("ad_"))),Is.True,name);
            }
            var progress=player.displays.Select(t=>world.track.ClosestProgress(t.position)).ToArray();
            Assert.That(progress.Count(p=>p<.20f),Is.EqualTo(1));Assert.That(progress.Any(p=>p>.22f&&p<.40f),Is.True);
            Assert.That(progress.Any(p=>p>.55f&&p<.70f),Is.True);Assert.That(progress.Any(p=>p>.70f&&p<.83f),Is.True);Assert.That(progress.Any(p=>p>.83f),Is.True);
            var material=world.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r=>r.sharedMaterials).First(m=>m&&m.shader.name=="VectorRush/Animated City Reflection");
            var poses=(Texture2D)material.GetTexture("_SignData");var pixels=poses.GetPixels();
            for(int i=0;i<player.displays.Length;i++){var c=pixels[i*8];Assert.That(Vector3.Distance(new Vector3(c.r,c.g,c.b),player.displays[i].position),Is.LessThan(.001f));}
            Editor.FullLapAdsSetup.ValidateClearance(world);
        }
    }
}
