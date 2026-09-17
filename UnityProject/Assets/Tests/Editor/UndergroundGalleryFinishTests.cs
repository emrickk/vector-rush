using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class UndergroundGalleryFinishTests
    {
        [Test]
        public void FinishedScenePreservesPublishedDrivingMeshesAndBaseline()
        {
            // Fixed source fingerprints of the five published authoritative collision meshes.
            string[] expected={
                "123092a196c614972fa8eb39225fdd16fa6a32f1f98d7b6aadb2c31acbc4d14b",
                "b7758b8151b561919e7611053f4ab6b1dc7022640b801698252861bd21ba72fa",
                "bef48f4c0a8fdb269875730cb2988c30e3bd9ada914d5e40a4da19223fb2d8da",
                "fad9abe7d6257c532854a24389c50d88fd6056578863aefe438ab6bbe3b89947",
                "d0e12f7f9b3d450c96b6e6c549ad3a8a2dc202d7c2093bc0b25fb50b1e0884e6"};
            var scene=EditorSceneManager.OpenScene(Editor.UndergroundGallerySetup.Candidate,OpenSceneMode.Additive);
            try
            {
                var world=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<ProductionWorld>()).Single();
                Assert.That(Editor.ProductionSceneSetup.CourseHash(world.track),Is.EqualTo("e5b995eaedbda504165e40147acfbd697321b38a249fd1f08f770642b0a2dc4f"));
                var roads=world.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.Contains("authoritative collision")).OrderBy(c=>c.name).ToArray();
                Assert.That(roads.Length,Is.EqualTo(5));
                for(int i=0;i<roads.Length;i++)Assert.That(Hash(AssetDatabase.GetAssetPath(roads[i].sharedMesh)),Is.EqualTo(expected[i]),roads[i].name);
                Assert.That(Hash(Editor.EnvironmentStructureSetup.Candidate),Is.EqualTo("83195acf00ea5140cb4600a1d686ca1cd1641683c7b16729aa8b7a57e4c2f7b5"));
            }
            finally {EditorSceneManager.CloseScene(scene,true);}
        }
        [Test]
        public void SavedFinishGeometryLeavesDrivingCorridorOpen()
        {
            var scene=EditorSceneManager.OpenScene(Editor.UndergroundGallerySetup.Candidate,OpenSceneMode.Additive);
            try
            {
                var world=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<ProductionWorld>()).Single();
                var finish=world.GetComponentsInChildren<Transform>().Single(t=>t.name==Editor.UndergroundGalleryFinish.RootName);
                Assert.That(finish.GetComponentsInChildren<MeshCollider>().Length,Is.GreaterThan(0));
                Editor.UndergroundGalleryFinish.ValidateCorridor(world);
            }
            finally {EditorSceneManager.CloseScene(scene,true);}
        }
        static string Hash(string path)=>System.BitConverter.ToString(SHA256.Create().ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();
    }
}
