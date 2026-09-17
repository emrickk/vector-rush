using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VectorRush.Editor
{
    /// <summary>
    /// Builds a new scene from the protected Stage 1 lighting field and adds the
    /// original Vector Rush route districts after that field. Source scenes are
    /// opened read-only and remain unchanged.
    /// </summary>
    public static class HybridRouteSetup
    {
        public const string LightingScene="Assets/Scenes/Stage1City.unity";
        public const string OriginalRouteScene="Assets/Scenes/NocturneProduction.unity";
        public const string HybridScene="Assets/Scenes/Stage1HybridRoute.unity";
        public const float ProtectedEndProgress=.3228f;
        public const float ProtectedWrapStart=.975f;
        public static readonly string[] RouteZoneNames={
            "02 / canyon and cool gallery",
            "03 / thermal works",
            "04 / station and civic approach",
            "05 / distant city"
        };

        public static void Prepare()
        {
            string evidence=ProductionSceneSetup.RequiredFlag("-productionEvidence");
            Directory.CreateDirectory(evidence);
            if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(LightingScene))throw new FileNotFoundException("Protected lighting scene is missing",LightingScene);
            if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(OriginalRouteScene))throw new FileNotFoundException("Original Vector Rush route scene is missing",OriginalRouteScene);

            var lighting=EditorSceneManager.OpenScene(LightingScene,OpenSceneMode.Single);
            if(!EditorSceneManager.SaveScene(lighting,HybridScene,true))throw new IOException("Could not create hybrid scene copy");
            var hybrid=EditorSceneManager.OpenScene(HybridScene,OpenSceneMode.Single);
            var hybridWorld=FindWorld(hybrid);
            var track=hybridWorld.track;

            // The Stage 1 root is the protected lighting field. Only the earlier
            // imported placeholder environment is removed from this new copy.
            foreach(Transform child in hybridWorld.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Zone •",StringComparison.Ordinal)).ToArray())
                UnityEngine.Object.DestroyImmediate(child.gameObject);

            var original=EditorSceneManager.OpenScene(OriginalRouteScene,OpenSceneMode.Additive);
            var originalWorld=FindWorld(original);
            SceneManager.SetActiveScene(hybrid);
            const string generated="Assets/Art/Stage1/Generated/HybridRoute";
            Directory.CreateDirectory(generated);AssetDatabase.Refresh();
            var routeRoot=new GameObject("Original Vector Rush route • after protected field").transform;
            routeRoot.SetParent(hybridWorld.transform,false);
            int zones=0,renderers=0,lights=0;
            foreach(string zoneName in RouteZoneNames)
            {
                var source=originalWorld.transform.Cast<Transform>().FirstOrDefault(t=>t.name==zoneName);
                if(!source)throw new InvalidDataException("Original route zone missing: "+zoneName);
                var clone=UnityEngine.Object.Instantiate(source.gameObject);
                clone.name=source.name;SceneManager.MoveGameObjectToScene(clone,hybrid);clone.transform.SetParent(routeRoot,false);
                if(zoneName.StartsWith("02 /",StringComparison.Ordinal))TrimProtectedGeometry(clone,track,false);
                else if(zoneName.StartsWith("04 /",StringComparison.Ordinal))TrimProtectedGeometry(clone,track,true);
                zones++;renderers+=clone.GetComponentsInChildren<Renderer>(true).Length;lights+=clone.GetComponentsInChildren<Light>(true).Length;
            }
            var transition=hybridWorld.gameObject.GetComponent<HybridRouteTransition>();
            if(!transition)transition=hybridWorld.gameObject.AddComponent<HybridRouteTransition>();
            transition.Configure(routeRoot.gameObject,ProtectedEndProgress,ProtectedWrapStart);
            EditorSceneManager.CloseScene(original,true);
            hybridWorld.artRevision="stage1-hybrid-route-01";
            hybridWorld.ValidateReady();
            EditorSceneManager.MarkSceneDirty(hybrid);
            if(!EditorSceneManager.SaveScene(hybrid))throw new IOException("Could not save hybrid scene");
            AssetDatabase.SaveAssets();
            File.WriteAllText(Path.Combine(evidence,"hybrid-scene.json"),JsonUtility.ToJson(new Summary{
                revision=hybridWorld.artRevision,courseHash=hybridWorld.courseHash,zones=zones,renderers=renderers,lights=lights,
                protectedEndProgress=ProtectedEndProgress,protectedWrapStart=ProtectedWrapStart
            },true));
            Debug.Log("HYBRID_ROUTE_READY "+zones+" zones "+renderers+" renderers "+lights+" lights");
        }

        static ProductionWorld FindWorld(Scene scene)
        {
            foreach(var root in scene.GetRootGameObjects())
            {
                var world=root.GetComponentInChildren<ProductionWorld>(true);
                if(world)return world;
            }
            throw new InvalidDataException("ProductionWorld missing from "+scene.path);
        }

        static void TrimProtectedGeometry(GameObject zone,TrackPath track,bool trimWrap)
        {
            foreach(var light in zone.GetComponentsInChildren<Light>(true).ToArray())
                if(IsProtected(track.ClosestProgress(light.transform.position),trimWrap))UnityEngine.Object.DestroyImmediate(light.gameObject);
            foreach(var filter in zone.GetComponentsInChildren<MeshFilter>(true).ToArray())
            {
                var source=filter.sharedMesh;if(!source)continue;
                var triangles=source.triangles;var vertices=source.vertices;var kept=new List<int>(triangles.Length);
                for(int i=0;i<triangles.Length;i+=3)
                {
                    Vector3 center=filter.transform.TransformPoint((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3f);
                    if(!IsProtected(track.ClosestProgress(center),trimWrap))
                    {kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);}
                }
                if(kept.Count==0){UnityEngine.Object.DestroyImmediate(filter.gameObject);continue;}
                if(kept.Count==triangles.Length)continue;
                var mesh=UnityEngine.Object.Instantiate(source);mesh.name="Hybrid "+source.name;mesh.SetTriangles(kept,0);mesh.RecalculateBounds();
                string path="Assets/Art/Stage1/Generated/HybridRoute/"+Sanitize(zone.name)+"-"+Sanitize(source.name)+".asset";
                var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}
                else AssetDatabase.CreateAsset(mesh,path);
                filter.sharedMesh=mesh;
            }
        }

        static bool IsProtected(float progress,bool trimWrap)=>progress<ProtectedEndProgress||(trimWrap&&progress>ProtectedWrapStart);
        static string Sanitize(string value)=>new string(value.Select(c=>char.IsLetterOrDigit(c)?c:'-').ToArray());

        [Serializable] sealed class Summary
        {
            public string revision,courseHash;
            public int zones,renderers,lights;
            public float protectedEndProgress,protectedWrapStart;
        }
    }
}
