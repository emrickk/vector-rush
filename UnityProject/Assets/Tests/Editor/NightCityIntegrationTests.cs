using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public class NightCityIntegrationTests
    {
        static string[] Colliders(ProductionWorld w)=>w.GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(x=>x).ToArray();
        static string[] Screens()=>Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>t.name+"|"+t.localToWorldMatrix+"|"+AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial)).ToArray();
        [Test] public void IntegrationPreservesP4CourseCollidersWeatherAndAnimatedDisplays()
        {
            EditorSceneManager.OpenScene(Editor.BillboardSceneSetup.Candidate,OpenSceneMode.Single);
            var original=Object.FindFirstObjectByType<ProductionWorld>();var hash=original.courseHash;
            var colliders=Colliders(original);var screens=Screens();
            var shelter=Object.FindFirstObjectByType<RainPresentation>().shelterProfile;
            float fog=RenderSettings.fogDensity;var fogColor=RenderSettings.fogColor;
            EditorSceneManager.OpenScene(Editor.NightCityIntegrationSetup.Candidate,OpenSceneMode.Single);
            var world=Object.FindFirstObjectByType<ProductionWorld>();
            Assert.That(world.courseHash,Is.EqualTo(hash));CollectionAssert.AreEqual(colliders,Colliders(world));CollectionAssert.AreEqual(screens,Screens());
            Assert.That(Object.FindFirstObjectByType<RainPresentation>().shelterProfile,Is.EqualTo(shelter));
            Assert.That(RenderSettings.fogDensity,Is.EqualTo(fog));Assert.That(RenderSettings.fogColor,Is.EqualTo(fogColor));
        }
        [Test] public void IntegratedArtHasLodsMaterialsAndNoAddedPhysics()
        {
            EditorSceneManager.OpenScene(Editor.NightCityIntegrationSetup.Candidate,OpenSceneMode.Single);
            var root=GameObject.Find("Night city / occupied service terraces");Assert.That(root,Is.Not.Null);
            Assert.That(root.GetComponentsInChildren<Collider>(true),Is.Empty);
            Assert.That(root.transform.childCount,Is.EqualTo(5));
            var groups=root.GetComponentsInChildren<LODGroup>();Assert.That(groups.Length,Is.GreaterThanOrEqualTo(25));
            foreach(var group in groups){Assert.That(group.lodCount,Is.EqualTo(2));foreach(var lod in group.GetLODs())Assert.That(lod.renderers.All(r=>r),Is.True);}
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())Assert.That(renderer.sharedMaterials.All(m=>m),Is.True);
        }

        [Test] public void IntegratedMeshesClearTheDrivingCorridorAndAnimatedScreens()
        {
            EditorSceneManager.OpenScene(Editor.NightCityIntegrationSetup.Candidate,OpenSceneMode.Single);
            var world=Object.FindFirstObjectByType<ProductionWorld>();
            var root=GameObject.Find("Night city / occupied service terraces");
            var renderers=root.GetComponentsInChildren<MeshRenderer>();
            var screens=Object.FindFirstObjectByType<AnimatedBillboards>().displays;
            foreach(var renderer in renderers)
            {
                foreach(var screen in screens)
                    Assert.That(renderer.bounds.Intersects(screen.GetComponent<Renderer>().bounds),Is.False,renderer.name+" intersects "+screen.name);
                for(int sample=0;sample<2400;sample++)
                {
                    var frame=world.track.Evaluate(sample/2400f);
                    for(int lateral=-6;lateral<=6;lateral++)
                    {
                        var point=frame.Position+frame.Right*lateral/6f*(world.track.Width*.5f+3);
                        var bounds=renderer.bounds;
                        bool overlaps=point.x>=bounds.min.x&&point.x<=bounds.max.x&&point.z>=bounds.min.z&&point.z<=bounds.max.z;
                        Assert.That(overlaps,Is.False,renderer.name+" overlaps road at "+sample/2400f);
                    }
                }
            }
        }

        [Test] public void RooftopEquipmentHasAStructuralSupportingRoof()
        {
            EditorSceneManager.OpenScene(Editor.NightCityIntegrationSetup.Candidate,OpenSceneMode.Single);
            var root=GameObject.Find("Night city / occupied service terraces");
            foreach(Transform terrace in root.transform)
            {
                var roof=terrace.Find("Shop structural roof").GetComponent<MeshFilter>().sharedMesh.bounds;
                float surface=terrace.Find("Shop structural roof").localPosition.y+roof.max.y;
                foreach(Transform child in terrace)
                {
                    if(!child.name.StartsWith("NC_DuctBank")&&!child.name.StartsWith("NC_NeonRoofSign"))continue;
                    Assert.That(child.localPosition.y,Is.EqualTo(surface).Within(.01f),child.name);
                }
            }
        }
    }
}
