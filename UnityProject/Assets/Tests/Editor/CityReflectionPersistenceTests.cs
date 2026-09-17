using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class CityReflectionPersistenceTests
    {
        [Test]
        public void CityCapturePositionsSurviveMaterialSerialization()
        {
            const string path = "Assets/Tests/Editor/TemporaryCityReflection.mat";
            var material = new Material(Shader.Find("VectorRush/City Glass Reflection"));
            var expected = new Vector4(209.9f, 48.85f, -184.89f, 0);
            try
            {
                for (int i = 0; i < 3; i++) material.SetVector("_Capture" + i, expected + Vector4.one * i);
                AssetDatabase.CreateAsset(material, path);
                AssetDatabase.SaveAssets();
                // An undeclared shader uniform works in memory but is omitted from the saved asset.
                string serialized = File.ReadAllText(path);
                for (int i = 0; i < 3; i++) StringAssert.Contains("_Capture" + i + ":", serialized);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var saved = AssetDatabase.LoadAssetAtPath<Material>(path);
                for (int i = 0; i < 3; i++) Assert.That(Vector4.Distance(saved.GetVector("_Capture" + i), expected + Vector4.one * i), Is.LessThan(.0001f));
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                if (material && !AssetDatabase.Contains(material)) Object.DestroyImmediate(material);
            }
        }
    }
}
