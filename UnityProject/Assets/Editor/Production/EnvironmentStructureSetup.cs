using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush.Editor
{
    // Stage 1: isolated visual infrastructure candidate. Never edits source meshes or the course.
    public static class EnvironmentStructureSetup
    {
        public const string Candidate = "Assets/Scenes/EnvironmentStructureStage1.unity";
        const string AssetsRoot = "Assets/Art/EnvironmentStructureStage1";
        static readonly Dictionary<Material, List<CombineInstance>> batches = new();
        static readonly List<Mesh> scratch = new();
        static Mesh cube;
        static TrackPath track;
        static Transform root;
        static int piers, podiums;

        public static void Prepare()
        {
            var evidence = ProductionSceneSetup.RequiredFlag("-productionEvidence");
            Directory.CreateDirectory(evidence);
            string baselineHash = Hash(HybridRouteSetup.HybridScene);
            var scene = EditorSceneManager.OpenScene(HybridRouteSetup.HybridScene, OpenSceneMode.Single);
            if (!EditorSceneManager.SaveScene(scene, Candidate, true)) throw new IOException("Cannot copy baseline");
            scene = EditorSceneManager.OpenScene(Candidate, OpenSceneMode.Single);
            var world = UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            track = world.track;
            string courseHash = ProductionSceneSetup.CourseHash(track);
            string colliderSignature = ColliderSignature(world);
            var oldSupports = world.GetComponentsInChildren<MeshRenderer>(true)
                .Where(r => r.sharedMaterial && r.sharedMaterial.name == "Preserved support slate").ToArray();
            if (oldSupports.Length != 1) throw new InvalidDataException("Expected exactly one isolated support batch, found " + oldSupports.Length);
            // This material is used exclusively for the old paired posts in BuildInfrastructure.
            oldSupports[0].enabled = false;
            Directory.CreateDirectory(AssetsRoot);
            AssetDatabase.Refresh();
            root = new GameObject("Environment stage 1 / urban viaduct and podiums").transform;
            root.SetParent(world.transform, false);
            batches.Clear(); scratch.Clear(); piers = podiums = 0;
            var concrete = Material("Concrete", new Color(.23f, .27f, .29f), .24f);
            var inset = Material("Recessed service bays", new Color(.055f, .075f, .085f), .18f);
            var edge = Material("Deck edge", new Color(.15f, .19f, .22f), .32f);
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube = primitive.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(primitive);

            const int spans = 1200;
            for (int i = 0; i < spans; i++)
            {
                float p = i / (float)spans;
                var a = track.Evaluate(p); var b = track.Evaluate((i + 1f) / spans);
                // Closed slab follows both banking and grade. All its geometry is below racing surface.
                Prism(a, b, 12.15f, -.9f, -3.8f, false, concrete);
                for (int side = -1; side <= 1; side += 2)
                {
                    Beam(Point(a, side * 12.05f, -.65f), Point(b, side * 12.05f, -.65f), .45f, .5f, edge);
                    Beam(Point(a, side * 11.7f, -3.45f), Point(b, side * 11.7f, -3.45f), .6f, .5f, edge);
                }
                if (Podium(p)) { Prism(a, b, 12.05f, -3.85f, -24f, true, concrete); podiums++; }
            }
            // Ordinary spans, not a pair of bank-tilted legs at every rail lamp.
            int count = Mathf.CeilToInt(track.Length / 34f);
            for (int i = 0; i < count; i++)
            {
                float p = i / (float)count;
                if (Podium(p)) continue;
                var f = track.Evaluate(p);
                var horizontal = Vector3.ProjectOnPlane(f.Forward, Vector3.up).normalized;
                var q = Quaternion.LookRotation(horizontal, Vector3.up);
                float underside = Mathf.Min(Point(f, -11.5f, -3.8f).y, Point(f, 11.5f, -3.8f).y);
                float cap = underside - .7f;
                Box(new Vector3(f.Position.x, (-24 + cap - 1f) * .5f, f.Position.z), new Vector3(5.5f, cap - 1f + 24, 4.8f), q, concrete);
                Box(new Vector3(f.Position.x, -24.5f, f.Position.z), new Vector3(10, 1, 9), q, concrete);
                Box(new Vector3(f.Position.x, cap, f.Position.z), new Vector3(23.5f, 2, 5.6f), q, concrete);
                foreach (float side in new[] { -1f, 1f })
                {
                    var bearing = Point(f, side * 9.5f, -3.8f);
                    float bottom = cap + 1;
                    Box(new Vector3(bearing.x, (bottom + bearing.y) * .5f, bearing.z), new Vector3(2.8f, Mathf.Max(.15f, bearing.y - bottom + .15f), 3), q, edge);
                }
                piers++;
            }
            // Shallow, dark service recesses establish a building scale without new lights or textures.
            for (float p = .40f; p < .485f; p += .009f)
            {
                if (!Podium(p)) continue;
                var f = track.Evaluate(p);
                var right = Vector3.ProjectOnPlane(f.Right, Vector3.up).normalized;
                var q = Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward, Vector3.up), Vector3.up);
                foreach (int side in new[] { -1, 1 })
                {
                    var c = f.Position + right * (side * 12.2f); c.y = -13;
                    Box(c, new Vector3(.18f, 11, 10), q, inset);
                    c.y = -6.9f; Box(c, new Vector3(.8f, .6f, 11), q, edge);
                }
            }
            Flush();
            ValidateClearance();
            world.artRevision = "environment-structure-stage1-01";
            if (ProductionSceneSetup.CourseHash(track) != courseHash || ColliderSignature(world) != colliderSignature)
                throw new InvalidDataException("Structural art changed course/colliders");
            world.ValidateReady();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Cannot save structural candidate");
            AssetDatabase.SaveAssets();
            if (Hash(HybridRouteSetup.HybridScene) != baselineHash) throw new InvalidDataException("Baseline scene changed");
            File.WriteAllText(Path.Combine(evidence, "structure.json"), JsonUtility.ToJson(new Report {
                scene = Candidate, revision = world.artRevision, courseHash = courseHash, baselineSha256 = baselineHash,
                disabledSupportBatches = oldSupports.Length, pierCount = piers, podiumSegments = podiums,
                courseAndCollidersUnchanged = true, baselineUnchanged = true, addedLights = 0
            }, true));
            Debug.Log("ENVIRONMENT_STRUCTURE_READY piers=" + piers + " podiumSegments=" + podiums);
        }

        // Leave a bridge opening between the two podium sections in the owner's marked district.
        public static bool Podium(float p) => (p >= .395f && p < .425f) || (p >= .452f && p < .485f) || (p >= .705f && p < .752f);
        static Vector3 Point(TrackFrame f, float x, float y) => f.Position + f.Right * x + f.Up * y;
        static void Prism(TrackFrame a, TrackFrame b, float half, float top, float bottom, bool ground, Material material)
        {
            var v = new[] { Point(a,-half,top), Point(a,half,top), Point(b,half,top), Point(b,-half,top),
                Point(a,-half,bottom), Point(a,half,bottom), Point(b,half,bottom), Point(b,-half,bottom) };
            if (ground) for (int i=4;i<8;i++) { v[i]=v[i-4]; v[i].y=bottom; }
            // Split face vertices: averaged corner normals made every short span look fluted.
            int[] faces={0,3,2,1,4,5,6,7,0,1,5,4,3,7,6,2,0,4,7,3,1,2,6,5};
            var flat=faces.Select(index=>v[index]).ToArray(); var indices=new List<int>();
            for(int face=0;face<6;face++) { int k=face*4; indices.AddRange(new[]{k,k+1,k+2,k,k+2,k+3}); }
            var mesh = new Mesh { vertices=flat, triangles=indices.ToArray() };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); scratch.Add(mesh);
            Add(mesh, Matrix4x4.identity, material);
        }
        static void Beam(Vector3 a, Vector3 b, float width, float height, Material m) => Box((a+b)*.5f,new Vector3(width,height,Vector3.Distance(a,b)+.04f),Quaternion.LookRotation(b-a),m);
        static void Box(Vector3 p, Vector3 size, Quaternion q, Material m) => Add(cube,Matrix4x4.TRS(p,q,size),m);
        static void Add(Mesh mesh, Matrix4x4 matrix, Material material)
        {
            if (!batches.TryGetValue(material,out var list)) batches[material]=list=new List<CombineInstance>();
            list.Add(new CombineInstance { mesh=mesh, transform=matrix });
        }
        static Material Material(string name, Color color, float smooth)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")) { name=name, enableInstancing=true };
            m.SetColor("_BaseColor",color); m.SetFloat("_Smoothness",smooth); m.SetFloat("_Metallic",.08f);
            return Save(m,name+".mat");
        }
        static T Save<T>(T asset,string name) where T:UnityEngine.Object
        {
            string path=AssetsRoot+"/"+name; var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if (old) { EditorUtility.CopySerialized(asset,old); UnityEngine.Object.DestroyImmediate(asset); EditorUtility.SetDirty(old); return old; }
            AssetDatabase.CreateAsset(asset,path); return asset;
        }
        static void Flush()
        {
            foreach (var pair in batches)
            {
                var mesh=new Mesh { name=pair.Key.name,indexFormat=IndexFormat.UInt32 };
                mesh.CombineMeshes(pair.Value.ToArray(),true,true); mesh.RecalculateBounds();
                var obj=new GameObject(pair.Key.name); obj.transform.SetParent(root,false); obj.isStatic=true;
                obj.AddComponent<MeshFilter>().sharedMesh=Save(mesh,pair.Key.name+".asset");
                obj.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            foreach (var mesh in scratch) UnityEngine.Object.DestroyImmediate(mesh);
            scratch.Clear(); batches.Clear();
        }
        static string ColliderSignature(ProductionWorld world) => string.Join("\n",world.GetComponentsInChildren<Collider>(true).Select(c=>c.GetType().Name+":"+EditorJsonUtility.ToJson(c)));
        static void ValidateClearance()
        {
            // Test the authored triangles directly: no edit-mode physics cooking state or gameplay colliders.
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var mesh=filter.sharedMesh; var vertices=mesh.vertices.Select(filter.transform.TransformPoint).ToArray();
                var triangles=mesh.triangles;
                var bounds=new Bounds[triangles.Length/3];
                for(int t=0;t<triangles.Length;t+=3)
                {
                    var bound=new Bounds(vertices[triangles[t]],Vector3.zero);
                    bound.Encapsulate(vertices[triangles[t+1]]); bound.Encapsulate(vertices[triangles[t+2]]);
                    bound.Expand(.001f); bounds[t/3]=bound;
                }
                for (int i=0;i<2400;i++)
                {
                    var f=track.Evaluate(i/2400f);
                    foreach (float x in new[] { -10.7f,-5f,0f,5f,10.7f })
                    {
                        Vector3 origin=Point(f,x,8), direction=-f.Up.normalized;
                        var rayBounds=new Bounds(origin,Vector3.zero); rayBounds.Encapsulate(origin+direction*8.15f);
                        for (int t=0;t<triangles.Length;t+=3)
                        {
                            if(!bounds[t/3].Intersects(rayBounds)) continue;
                            var a=vertices[triangles[t]]; var e1=vertices[triangles[t+1]]-a; var e2=vertices[triangles[t+2]]-a;
                            var h=Vector3.Cross(direction,e2); float det=Vector3.Dot(e1,h);
                            // Rays on a span seam are coplanar with its end cap; a relative
                            // tolerance avoids unstable division there. Top faces remain tested.
                            if (Mathf.Abs(det)<.0001f*e1.magnitude*e2.magnitude) continue;
                            var s=origin-a; float u=Vector3.Dot(s,h)/det;
                            if (u<0 || u>1) continue;
                            var q=Vector3.Cross(s,e1); float v=Vector3.Dot(direction,q)/det;
                            if (v<0 || u+v>1) continue;
                            float distance=Vector3.Dot(e2,q)/det;
                            if (distance>=0 && distance<=8.15f)
                            {
                                throw new InvalidDataException("New structure intrudes into racing clearance at progress "+i/2400f+" lateral "+x+" on "+filter.name+" distance="+distance+" triangle="+t/3+" a="+a+" b="+(a+e1)+" c="+(a+e2));
                            }
                        }
                    }
                }
            }
        }
        static string Hash(string path) { using var sha=SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant(); }
        [Serializable] sealed class Report { public string scene,revision,courseHash,baselineSha256; public int disabledSupportBatches,pierCount,podiumSegments,addedLights; public bool courseAndCollidersUnchanged,baselineUnchanged; }
    }
}
