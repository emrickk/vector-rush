using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace VectorRush.Tests
{
    public sealed class Stage1AppearanceTests
    {
        [Test]
        public void CircuitInfrastructureContinuesAfterProtectedTestFieldWithoutChangingItsBoundary()
        {
            var type=System.Type.GetType("VectorRush.Editor.Stage1CitySetup, VectorRush.ProductionEditor");
            Assert.That(type,Is.Not.Null,"Stage 1 authoring type must remain available to the boundary contract test.");
            int protectedCount=(int)type.GetField("ProtectedTestFieldSpanCount").GetRawConstantValue();
            int circuitCount=(int)type.GetField("CircuitInfrastructureSpanCount").GetRawConstantValue();
            float step=(float)type.GetField("InfrastructureStep").GetRawConstantValue();
            var progress=type.GetMethod("InfrastructureProgress");
            Assert.That(protectedCount,Is.EqualTo(310));
            Assert.That((float)progress.Invoke(null,new object[]{0}),Is.EqualTo(-.012f).Within(.000001f));
            Assert.That((float)progress.Invoke(null,new object[]{protectedCount}),Is.EqualTo(.3228f).Within(.000001f));
            Assert.That(circuitCount*step,Is.GreaterThanOrEqualTo(1f));
            Assert.That((circuitCount-1)*step,Is.LessThan(1f));
        }
        [Test]
        public void HybridRouteUsesProtectedLightingSceneAndOriginalPostFieldDistricts()
        {
            var type=System.Type.GetType("VectorRush.Editor.HybridRouteSetup, VectorRush.ProductionEditor");
            Assert.That(type,Is.Not.Null);
            Assert.That((string)type.GetField("LightingScene").GetRawConstantValue(),Is.EqualTo("Assets/Scenes/Stage1City.unity"));
            Assert.That((string)type.GetField("OriginalRouteScene").GetRawConstantValue(),Is.EqualTo("Assets/Scenes/NocturneProduction.unity"));
            Assert.That((float)type.GetField("ProtectedEndProgress").GetRawConstantValue(),Is.EqualTo(.3228f).Within(.000001f));
            var zones=(string[])type.GetField("RouteZoneNames").GetValue(null);
            Assert.That(zones,Is.EqualTo(new[]{"02 / canyon and cool gallery","03 / thermal works","04 / station and civic approach","05 / distant city"}));
            Assert.That(typeof(HybridRouteTransition).GetProperty("RouteStart"),Is.Not.Null);
            Assert.That(typeof(HybridRouteTransition).GetProperty("RouteEnd"),Is.Not.Null);
        }
        [Test]
        public void PersistentRoadAndNeonMaterialsSurviveRepeatedAuthoring()
        {
            const string root="Assets/Art/Stage1/Generated/";
            var road=AssetDatabase.LoadAssetAtPath<Material>(root+"Road.mat");
            Assert.That(road,Is.Not.Null);Assert.That(road.shader.name,Is.EqualTo("Universal Render Pipeline/Lit"));
            Assert.That(road.GetTexture("_BaseMap"),Is.Not.Null);
            foreach(string name in new[]{"Cyan rail core","Magenta signal core","Office facade 0","Office facade 1"})
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(root+name+".mat");
                Assert.That(mat,Is.Not.Null,name);Assert.That(mat.IsKeywordEnabled("_EMISSION"),Is.True,name);
                Assert.That(mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive,Is.Not.Zero,name);
            }
        }
        [Test]
        public void DeliveredShipMapsAndBoostMaskReachRendererWhileGlassRemainsUnchanged()
        {
            var library=AssetDatabase.LoadAssetAtPath<ProductionMaterialLibrary>("Assets/Art/Stage1/Generated/CraftLibrary.asset");
            Assert.That(library.stage1Finish,Is.True);
            var root=new GameObject("Stage1 material integration test");
            using(var factory=new CraftMaterialFactory(library))
            {
                try
                {
                    var hull=factory.CreateMapped("Test hull","Ivory",Color.white,.7f,.3f);
                    Assert.That(hull.GetTexture("_BaseMap").name,Is.EqualTo("vrx_ship_ivory_a_base"));
                    Assert.That(hull.GetTexture("_EmissionMap").name,Is.EqualTo("Stage1Energy_Ivory"));
                    var renderer=root.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{hull,library.craftGlass};
                    var vehicle=root.AddComponent<HoverVehicle>();vehicle.VisualRoot=root.transform;
                    var boost=root.AddComponent<ShipBoostVisuals>();boost.Initialize(vehicle);boost.Step(1,1,0,0);
                    var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block,0);
                    Assert.That(block.GetColor("_EmissionColor").b,Is.GreaterThan(3));
                    renderer.GetPropertyBlock(block,1);Assert.That(block.isEmpty,Is.True);
                    boost.ResetState();renderer.GetPropertyBlock(block,0);
                    Assert.That(block.GetColor("_EmissionColor").maxColorComponent,Is.Zero);
                }
                finally{Object.DestroyImmediate(root);}
            }
        }
    }
}
