using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class UndergroundGalleryTests
    {
        [Test]
        public void ProfileIsOptInAndRestoresExactHistoricalCourse()
        {
            var go=new GameObject("Underground test");
            try
            {
                var track=go.AddComponent<TrackPath>();
                string baseline=Editor.ProductionSceneSetup.CourseHash(track);
                Assert.That(baseline,Is.EqualTo("598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059"));
                track.SetUndergroundGallery(true);
                Assert.That(Editor.ProductionSceneSetup.CourseHash(track),Is.Not.EqualTo(baseline));
                track.SetUndergroundGallery(false);
                Assert.That(Editor.ProductionSceneSetup.CourseHash(track),Is.EqualTo(baseline));
            }
            finally { Object.DestroyImmediate(go); }
        }
        [Test]
        public void DescentChangesElevationNotHorizontalLayoutAndHasBoundedGrade()
        {
            var a=new GameObject("Original");var b=new GameObject("Candidate");
            try
            {
                var old=a.AddComponent<TrackPath>();var track=b.AddComponent<TrackPath>();track.SetUndergroundGallery(true);
                for(int i=0;i<2400;i++)
                {
                    float p=i/2400f,t=old.ParameterAtProgress(p);var f=old.Evaluate(p);var g=track.EvaluateParameter(t);
                    Assert.That(g.Position.x,Is.EqualTo(f.Position.x));Assert.That(g.Position.z,Is.EqualTo(f.Position.z));
                    Assert.That(g.Position.y-f.Position.y,Is.EqualTo(TrackPath.GalleryOffset(p)).Within(.002f));
                    Assert.That(Mathf.Abs(g.Forward.y)/Vector3.ProjectOnPlane(g.Forward,Vector3.up).magnitude,Is.LessThan(.4f));
                    Assert.That(Vector3.Dot(g.Up,g.Forward),Is.EqualTo(0).Within(.00001f));
                    var tangent=(track.EvaluateParameter(t+.00002f).Position-track.EvaluateParameter(t-.00002f).Position).normalized;
                    Assert.That(Vector3.Dot(g.Forward,tangent),Is.GreaterThan(.999f));
                }
                Assert.That(TrackPath.GalleryOffset(.83f),Is.EqualTo(-22).Within(.0001f));
                Assert.That(TrackPath.GalleryOffset(.1f),Is.EqualTo(0));
                Assert.That(TrackPath.GalleryOffset(.99f),Is.EqualTo(0).Within(.0001f));
            }
            finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
        }
        [Test]
        public void ArcLengthClosestPointAndProfileBoundaryFramesAreContinuous()
        {
            var go=new GameObject("Underground projection");
            try
            {
                var track=go.AddComponent<TrackPath>();track.SetUndergroundGallery(true);
                for(int i=0;i<1200;i++)
                {
                    float p=i/1200f;var f=track.Evaluate(p);float nearest=track.ClosestProgress(f.Position);
                    float error=Mathf.Abs(Mathf.DeltaAngle(p*360,nearest*360))/360;
                    Assert.That(error,Is.LessThan(.00001f));
                    Assert.That(Vector3.Distance(f.Position,track.Evaluate(p+.00001f).Position),Is.LessThan(.03f));
                }
                Assert.That(Vector3.Distance(track.Evaluate(0).Position,track.Evaluate(1).Position),Is.LessThan(.00001f));
            }
            finally{Object.DestroyImmediate(go);}
        }
        [Test]
        public void SavedCandidateOwnsItsRoadMeshesAndCourseIdentity()
        {
            var scene=EditorSceneManager.OpenScene(Editor.UndergroundGallerySetup.Candidate,OpenSceneMode.Additive);
            try
            {
                var world=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<ProductionWorld>()).Single();
                Assert.That(world.track.UndergroundGallery,Is.True);
                Assert.That(world.courseHash,Is.EqualTo(Editor.ProductionSceneSetup.CourseHash(world.track)));
                var road=world.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.Contains("authoritative collision")).ToArray();
                Assert.That(road.Length,Is.EqualTo(5));
                foreach(var c in road)
                {
                    Assert.That(UnityEditor.AssetDatabase.GetAssetPath(c.sharedMesh),Does.StartWith("Assets/Art/UndergroundGalleryStage2/"));
                    Assert.That(c.GetComponent<MeshFilter>().sharedMesh,Is.SameAs(c.sharedMesh));
                }
                var warm=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/UndergroundGalleryStage2/Recessed warm light.mat");
                Assert.That(warm.IsKeywordEnabled("_EMISSION"),Is.True,"Lamp faces must survive import with emission enabled");
                Assert.That(warm.globalIlluminationFlags,Is.EqualTo(MaterialGlobalIlluminationFlags.RealtimeEmissive));
            }
            finally{EditorSceneManager.CloseScene(scene,true);}
        }
    }
}
