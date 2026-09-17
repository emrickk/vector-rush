using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace VectorRush.Tests
{
    public class BillboardTests
    {
        [Test] public void AnimationClockLoopsAndSupportsIndependentPhases()
        {
            Assert.That(AnimatedBillboards.FrameAtTime(0,0),Is.Zero);
            Assert.That(AnimatedBillboards.FrameAtTime(3.75f,0),Is.EqualTo(32).Within(.001));
            Assert.That(AnimatedBillboards.FrameAtTime(7.5f,0),Is.Zero);
            Assert.That(AnimatedBillboards.FrameAtTime(0,.83f),Is.Not.EqualTo(0));
            Assert.That(AnimatedBillboards.FrameAtTime(6,0,2),Is.EqualTo(63));
            for(int campaign=0;campaign<4;campaign++)Assert.That(AnimatedBillboards.FrameAtTime(AnimatedBillboards.PlaybackSeconds[campaign],0,campaign),Is.Zero);
        }
        [Test] public void CandidatePreservesEveryColliderAndWeatherProfile()
        {
            EditorSceneManager.OpenScene(Editor.AtmosphereSceneSetup.Candidate,OpenSceneMode.Single);
            var original=Object.FindFirstObjectByType<ProductionWorld>();var hash=original.courseHash;
            var colliders=original.GetComponentsInChildren<Collider>().Select(Key).OrderBy(x=>x).ToArray();
            var profile=Object.FindFirstObjectByType<RainPresentation>().shelterProfile;
            EditorSceneManager.OpenScene(Editor.BillboardSceneSetup.Candidate,OpenSceneMode.Single);
            var candidate=Object.FindFirstObjectByType<ProductionWorld>();
            CollectionAssert.AreEqual(colliders,candidate.GetComponentsInChildren<Collider>().Select(Key).OrderBy(x=>x).ToArray());
            Assert.That(candidate.courseHash,Is.EqualTo(hash));
            Assert.That(Object.FindFirstObjectByType<RainPresentation>().shelterProfile,Is.EqualTo(profile));
            Assert.That(RenderSettings.fogDensity,Is.EqualTo(.0055f).Within(.00001));
        }
        [Test] public void AllScreensHaveVideoFramesAndMatchingReflectionPhases()
        {
            EditorSceneManager.OpenScene(Editor.BillboardSceneSetup.Candidate,OpenSceneMode.Single);
            var player=Object.FindFirstObjectByType<AnimatedBillboards>();Assert.That(player.displays.Length,Is.EqualTo(10));Assert.That(player.kineticObjects.Length,Is.EqualTo(1));
            var ratios=player.displays.Select(t=>t.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Aspect")).Distinct().Count();Assert.That(ratios,Is.GreaterThanOrEqualTo(4));
            for(int i=0;i<player.displays.Length;i++){
                var m=player.displays[i].GetComponent<MeshRenderer>().sharedMaterial;Assert.That(m.shader.name,Is.EqualTo("VectorRush/Animated Billboard"));Assert.That(m.GetFloat("_Phase"),Is.GreaterThanOrEqualTo(0));
                for(int j=0;j<4;j++){var t=m.GetTexture("_Frames"+j);Assert.That(t,Is.Not.Null);Assert.That(t.width,Is.EqualTo(3072));Assert.That(t.height,Is.EqualTo(4608));}
            }
            var reflection=Object.FindFirstObjectByType<ProductionWorld>().GetComponentsInChildren<MeshRenderer>().SelectMany(r=>r.sharedMaterials).First(m=>m&&m.shader.name=="VectorRush/Animated City Reflection");
            Assert.That(reflection.GetFloat("_SignCount"),Is.EqualTo(player.displays.Length));
            Assert.That(reflection.GetTexture("_Frames0"),Is.EqualTo(player.displays[0].GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Frames0")));
        }
        [Test] public void ScreensClearTheirHousingAndUseArchitecturalClusters()
        {
            EditorSceneManager.OpenScene(Editor.BillboardSceneSetup.Candidate,OpenSceneMode.Single);
            var signs=Object.FindFirstObjectByType<AnimatedBillboards>().displays;
            var groups=signs.GroupBy(t=>t.parent.name.Split('/')[0]).ToArray();
            Assert.That(groups.Length,Is.EqualTo(5));Assert.That(groups.Any(g=>g.Count()==3),Is.True);
            Assert.That(signs.Max(t=>t.position.y)-signs.Min(t=>t.position.y),Is.GreaterThan(18));
            foreach(var t in signs){
                var mesh=t.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(mesh.vertices.Max(v=>v.z)+t.localPosition.z,Is.LessThan(-.1f));
                float yaw=Mathf.Repeat(t.eulerAngles.y,90);Assert.That(Mathf.Min(yaw,90-yaw),Is.LessThan(.01f));
                var tex=t.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Frames0") as Texture2D;Assert.That(tex.mipmapCount,Is.GreaterThan(1));
            }
        }
        static string Key(Collider c)=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"");
    }
}
