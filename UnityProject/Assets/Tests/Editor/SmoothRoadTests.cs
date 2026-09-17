using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class SmoothRoadTests
    {
        [Test]
        public void SmoothProfileHasContinuousTangentCurvatureAndBankAtEveryJoin()
        {
            const int n=1200;var source=new Vector3[n];
            for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;source[i]=new Vector3(110*Mathf.Cos(a),8*Mathf.Sin(3*a),70*Mathf.Sin(a));}
            var profile=new SmoothCourseProfile(source,580);
            for(int i=0;i<n;i++)
            {
                float t=i/(float)n;
                profile.Derivatives(t-.0000001f,out var va,out var aa);
                profile.Derivatives(t+.0000001f,out var vb,out var ab);
                Assert.That(Vector3.Distance(va,vb),Is.LessThan(.0001f),"Tangent join "+i);
                Assert.That(Vector3.Distance(aa,ab),Is.LessThan(.0001f),"Curvature join "+i);
                Assert.That(Mathf.Abs(profile.Bank(t-.0000001f)-profile.Bank(t+.0000001f)),Is.LessThan(.001f));
            }
        }
        [Test]
        public void CorrectionReducesCurvatureAndBankRateWithoutLargeLayoutChange()
        {
            var a=new GameObject("before");var b=new GameObject("after");
            try
            {
                var old=a.AddComponent<TrackPath>();old.SetUndergroundGallery(true);
                var track=b.AddComponent<TrackPath>();track.SetUndergroundGallery(true);track.SetSmoothRoad(true);
                var m=Editor.SmoothRoadSetup.Measure(old,track);
                Assert.That(m.maxCenterDisplacement,Is.LessThan(2));
                Assert.That(m.newMaxCurvatureRate,Is.LessThan(m.oldMaxCurvatureRate*.2f));
                Assert.That(m.newMaxBankRate,Is.LessThan(.65f));
                Assert.That(m.newMaxBankRate,Is.LessThan(m.oldMaxBankRate*.4f));
                string before=Editor.ProductionSceneSetup.CourseHash(old);track.SetSmoothRoad(false);
                Assert.That(Editor.ProductionSceneSetup.CourseHash(track),Is.EqualTo(before),"Opt-out must preserve the previous course");
            }
            finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
        }
        [Test]
        public void SavedSmoothSceneUsesMatchingDenseRoadAndOpenCorridor()
        {
            var scene=EditorSceneManager.OpenScene(Editor.SmoothRoadSetup.Candidate,OpenSceneMode.Additive);
            try
            {
                var world=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<ProductionWorld>()).Single();
                Assert.That(world.track.SmoothRoad,Is.True);
                Assert.That(world.courseHash,Is.EqualTo(Editor.ProductionSceneSetup.CourseHash(world.track)));
                var road=world.GetComponentsInChildren<MeshCollider>().Single(c=>c.name.StartsWith("Running surface"));
                Assert.That(road.sharedMesh.vertexCount,Is.EqualTo(3841*13));
                Assert.That(road.sharedMesh,Is.SameAs(road.GetComponent<MeshFilter>().sharedMesh));
                Editor.SmoothRoadSetup.ValidateRoad(world);
            }
            finally{EditorSceneManager.CloseScene(scene,true);}
        }
    }
}
