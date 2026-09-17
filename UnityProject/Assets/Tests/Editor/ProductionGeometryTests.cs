using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class ProductionGeometryTests
    {
        static Type EditorType(string name)
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {var type=assembly.GetType("VectorRush.Editor."+name);if(type!=null)return type;}
            throw new InvalidOperationException("Missing editor type "+name);
        }
        [Test]
        public void PersistentRoadMatchesAuthoritativeBankedSurfaceAndClosesSeam()
        {
            var go=new GameObject("Geometry test");Mesh mesh=null;
            try
            {
                var track=go.AddComponent<TrackPath>();
                mesh=(Mesh)EditorType("ProductionGeometry").GetMethod("Ribbon").Invoke(null,new object[]{track,-11f,11f,0f,false});
                var vertices=mesh.vertices;var normals=mesh.normals;
                Assert.That(vertices.Length,Is.EqualTo(961*13));
                for(int i=0;i<=960;i+=37)
                {
                    var frame=track.Evaluate(i/960f);
                    Assert.That(Vector3.Distance(vertices[i*13],frame.Position-frame.Right*11),Is.LessThan(.0001f));
                    Assert.That(Vector3.Distance(vertices[i*13+12],frame.Position+frame.Right*11),Is.LessThan(.0001f));
                    Assert.That(Vector3.Dot(normals[i*13+6],frame.Up),Is.GreaterThan(.97f));
                }
                for(int j=0;j<13;j++)Assert.That(Vector3.Distance(vertices[j],vertices[960*13+j]),Is.LessThan(.0001f));
            }
            finally{if(mesh)UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(go);}
        }
        [TestCase(-1)] [TestCase(1)]
        public void BarrierRetainsLegacyCollisionEnvelope(int side)
        {
            var go=new GameObject("Barrier test");Mesh mesh=null;
            try
            {
                var track=go.AddComponent<TrackPath>();mesh=(Mesh)EditorType("ProductionGeometry").GetMethod("Barrier").Invoke(null,new object[]{track,side});
                var vertices=mesh.vertices;
                for(int i=0;i<=960;i+=29)
                {
                    var frame=track.Evaluate(i/960f);
                    for(int j=0;j<4;j++)
                    {
                        Vector3 offset=vertices[i*4+j]-frame.Position;
                        Assert.That(Vector3.Dot(offset,frame.Right),Is.EqualTo((j<2?11.7f:12.1f)*side).Within(.0001f));
                        Assert.That(Vector3.Dot(offset,frame.Up),Is.EqualTo(j==0||j==3?-.3f:2f).Within(.0001f));
                    }
                }
            }
            finally{if(mesh)UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(go);}
        }
        [Test]
        public void CourseIdentityIsDeterministicAndChangesWithWidth()
        {
            var go=new GameObject("Hash test");
            try
            {
                var track=go.AddComponent<TrackPath>();var hash=EditorType("ProductionSceneSetup").GetMethod("CourseHash");
                string first=(string)hash.Invoke(null,new object[]{track});Assert.That(first.Length,Is.EqualTo(64));
                Assert.That(hash.Invoke(null,new object[]{track}),Is.EqualTo(first));track.Width=24;
                Assert.That(hash.Invoke(null,new object[]{track}),Is.Not.EqualTo(first));
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
