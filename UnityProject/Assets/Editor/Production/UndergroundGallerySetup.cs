using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Editor
{
    public static class UndergroundGallerySetup
    {
        public const string Candidate = "Assets/Scenes/UndergroundGalleryStage2.unity";
        const string AssetsRoot = "Assets/Art/UndergroundGalleryStage2";
        static TrackPath original, track;
        static Transform root;
        static Vector3[] route;
        static readonly Dictionary<Vector2,float> projections=new();
        static readonly Dictionary<Material,ProductionGeometry> batches=new();
        static int savedMeshes;

        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence");
            Directory.CreateDirectory(output);
            string baselineHash=Hash(EnvironmentStructureSetup.Candidate);
            var scene=EditorSceneManager.OpenScene(EnvironmentStructureSetup.Candidate,OpenSceneMode.Single);
            if(!EditorSceneManager.SaveScene(scene,Candidate,true))throw new IOException("Cannot preserve baseline scene");
            scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();track=world.track;
            var temporary=new GameObject("Authoring baseline curve");original=temporary.AddComponent<TrackPath>();
            string oldCourse=ProductionSceneSetup.CourseHash(original);
            if(oldCourse!=world.courseHash)throw new InvalidDataException("Unexpected starting course");
            route=Enumerable.Range(0,1201).Select(i=>original.Evaluate(i/1200f).Position).ToArray();
            projections.Clear(); batches.Clear(); savedMeshes=0;
            track.SetUndergroundGallery(true);
            Directory.CreateDirectory(AssetsRoot); AssetDatabase.Refresh();
            foreach(var filter in world.GetComponentsInChildren<MeshFilter>(true))
            {
                string path=PathOf(filter.transform);
                var renderer=filter.GetComponent<MeshRenderer>(); if(!renderer)continue;
                var material=renderer.sharedMaterial;
                bool collision=filter.GetComponent<MeshCollider>();
                bool structure=path.Contains("Environment stage 1 /");
                bool rail=material && (material.name.StartsWith("Preserved rail") || material.name.Contains("localized road response") ||
                    (material.HasProperty("_RoadSurface") && material.GetFloat("_RoadSurface")>.5f));
                bool signal=material && (material.name=="Cyan rail core" || material.name=="Magenta signal core" || material.name=="Amber service lamps");
                bool gallery=path.Contains("04 / station and civic approach/");
                if(!collision && !structure && !rail && !signal && !gallery)continue;
                var source=filter.sharedMesh; var vertices=source.vertices; bool changed=false;
                Mesh copy=null;
                if(gallery)
                {
                    var retained=new List<int>(); var tris=source.triangles;
                    for(int i=0;i<tris.Length;i+=3)
                    {
                        Vector3 center=filter.transform.TransformPoint((vertices[tris[i]]+vertices[tris[i+1]]+vertices[tris[i+2]])/3);
                        float p=Project(center); var f=original.Evaluate(p);
                        if(p>.778f && p<.894f && Mathf.Abs(Vector3.Dot(center-f.Position,f.Right))<23) { changed=true; continue; }
                        retained.AddRange(new[]{tris[i],tris[i+1],tris[i+2]});
                    }
                    if(changed) { copy=UnityEngine.Object.Instantiate(source); copy.triangles=retained.ToArray(); }
                }
                else for(int i=0;i<vertices.Length;i++)
                {
                    var v=filter.transform.TransformPoint(vertices[i]); float p=Project(v);
                    if(p<=.685f || p>=.97f)continue;
                    var a=original.Evaluate(p); Vector3 local=v-a.Position;
                    if(signal && (Mathf.Abs(Vector3.Dot(local,a.Right))>13 || Mathf.Abs(Vector3.Dot(local,a.Up))>4))continue;
                    var b=track.EvaluateParameter(original.ParameterAtProgress(p));
                    if(structure)
                    {
                        // Foundations remain vertical and attached to the same ground datum.
                        v.y+=TrackPath.GalleryOffset(p)*Mathf.Clamp01((v.y+24)/(a.Position.y+20));
                    }
                    else v=b.Position+b.Right*Vector3.Dot(local,a.Right)+b.Up*Vector3.Dot(local,a.Up)+b.Forward*Vector3.Dot(local,a.Forward);
                    vertices[i]=filter.transform.InverseTransformPoint(v); changed=true;
                }
                if(!changed)continue;
                if(!copy){copy=UnityEngine.Object.Instantiate(source);copy.vertices=vertices;}
                copy.name=source.name+" / underground";copy.RecalculateNormals();copy.RecalculateTangents();copy.RecalculateBounds();
                filter.sharedMesh=Save(copy,"Route-"+(savedMeshes++)+".asset");
                if(collision)filter.GetComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
            }
            foreach(var light in world.GetComponentsInChildren<Light>(true))
            {
                float p=Project(light.transform.position);
                if(PathOf(light.transform).Contains("04 / station and civic approach/") && p>.778f && p<.894f)
                    light.enabled=false;
                if(light.name.StartsWith("Local rail wash"))
                {
                    var a=original.Evaluate(p); var b=track.EvaluateParameter(original.ParameterAtProgress(p));
                    var d=light.transform.position-a.Position;
                    light.transform.position=b.Position+b.Right*Vector3.Dot(d,a.Right)+b.Up*Vector3.Dot(d,a.Up)+b.Forward*Vector3.Dot(d,a.Forward);
                }
            }
            root=new GameObject("Underground gallery / cutting, portal and city foundations").transform;root.SetParent(world.transform,false);
            BuildArchitecture(); Flush();
            world.courseHash=ProductionSceneSetup.CourseHash(track);world.artRevision="underground-gallery-stage2-02";
            string validation=Validate();
            ProductionSceneSetup.ExportContextToDirectory(track,Path.Combine(output,"course"),"candidate-working-tree");
            UnityEngine.Object.DestroyImmediate(temporary);
            world.ValidateReady(); EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene))throw new IOException("Cannot save underground candidate");
            AssetDatabase.SaveAssets();
            if(Hash(EnvironmentStructureSetup.Candidate)!=baselineHash)throw new InvalidDataException("Baseline was modified");
            File.WriteAllText(Path.Combine(output,"underground-validation.txt"),$"scene={Candidate}\nbaselineSha256={baselineHash}\nbaselineUnchanged=true\noldCourse={oldCourse}\nnewCourse={world.courseHash}\nclonedMeshes={savedMeshes}\n"+validation);
            Debug.Log("UNDERGROUND_CANDIDATE_READY "+world.courseHash+" "+validation);
        }

        static float Project(Vector3 v)
        {
            var key=new Vector2(v.x,v.z); if(projections.TryGetValue(key,out float value))return value;
            float best=float.MaxValue;value=0;
            for(int i=0;i<1200;i++)
            {
                var a=new Vector2(route[i].x,route[i].z);var ab=new Vector2(route[i+1].x,route[i+1].z)-a;
                float t=Mathf.Clamp01(Vector2.Dot(key-a,ab)/ab.sqrMagnitude);float d=(key-a-t*ab).sqrMagnitude;
                if(d<best){best=d;value=(i+t)/1200f;}
            }
            projections[key]=value;return value;
        }
        static TrackFrame Frame(float p)=>track.EvaluateParameter(original.ParameterAtProgress(p));
        static Vector3 Right(TrackFrame f)=>Vector3.Cross(Vector3.up,f.Forward).normalized;
        static Vector3 At(TrackFrame f,float x,float y)=>f.Position+Right(f)*x+Vector3.up*y;
        static Quaternion Rotation(TrackFrame f)=>Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up),Vector3.up);
        static float WallHeight(float p)
        {
            if(p>=.791f && p<=.884f)return 21;
            float enter=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.681f,.756f,p));
            float exit=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.895f,.972f,p));
            return Mathf.Lerp(1,13,Mathf.Min(enter,exit));
        }
        static void BuildArchitecture()
        {
            var concrete=Material("Foundation / mineral concrete",new Color(.28f,.30f,.32f),.25f);
            var panels=Material("Gallery / pale ceramic",new Color(.42f,.46f,.48f),.4f);
            var stone=Material("Terrace coping / blue slate",new Color(.14f,.18f,.21f),.35f);
            var dark=Material("Recesses / charcoal",new Color(.035f,.049f,.055f),.22f);
            var metal=Material("Services / brushed alloy",new Color(.24f,.29f,.31f),.48f);
            var warm=Material("Recessed warm light",new Color(.8f,.42f,.12f),.4f, new Color(2.2f,1.05f,.28f));
            var cool=Material("Wayfinding cyan",new Color(.025f,.43f,.5f),.35f,new Color(.06f,1.4f,1.9f));
            concrete.SetTexture("_BaseMap",ConcreteTexture());EditorUtility.SetDirty(concrete);
            const float start=.681f,end=.972f,step=.003f;
            int span=0;
            for(float p=start;p<end;p+=step,span++)
            {
                float next=Mathf.Min(end,p+step);var a=Frame(p);var b=Frame(next); var f=Frame((p+next)*.5f);
                bool roof=p>=.791f && p<.884f;
                float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.681f,.756f,p));
                float exit=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.895f,.972f,p));
                float height=WallHeight(p),heightB=WallHeight(next);
                float cityA=Mathf.Max(a.Position.y+height+2,Mathf.Lerp(a.Position.y+2,59,Mathf.Min(reveal,exit)));
                float cityB=Mathf.Max(b.Position.y+height+2,Mathf.Lerp(b.Position.y+2,59,Mathf.Min(reveal,exit)));
                foreach(int side in new[]{-1,1})
                {
                    // Two solid tiers: the lower wall follows the cutting; the upper city datum does not.
                    Prism(a,b,side*17.6f,side*22,-7,-7,height,heightB,concrete);
                    Prism(a,b,side*22,side*34,-10,-10,cityA-a.Position.y,cityB-b.Position.y,concrete);
                    Prism(a,b,side*17.35f,side*22.3f,height,heightB,height+.45f,heightB+.45f,stone);
                    Prism(a,b,side*22,side*34,cityA-a.Position.y,cityB-b.Position.y,cityA-a.Position.y+.45f,cityB-b.Position.y+.45f,stone);
                    // Usable shoulder, drainage and an unbroken plinth hide exposed track supports.
                    Prism(a,b,side*12.2f,side*17.65f,-2.2f,-2.2f,-.55f,-.55f,stone);
                    Prism(a,b,side*17.0f,side*17.7f,-2,-2,1.2f,1.2f,stone);
                    if(span%2==0)
                    {
                        Box(At(f,side*17.25f,height*.5f),new Vector3(.65f,height+1, .36f),Rotation(f),stone);
                        if(height>5 && span%6!=0)
                        {
                            Box(At(f,side*17.26f,3.2f),new Vector3(.18f,2.8f,3.4f),Rotation(f),dark);
                            for(int slat=0;slat<5;slat++)Box(At(f,side*17.10f,2.2f+slat*.42f),new Vector3(.12f,.09f,3.2f),Rotation(f),metal);
                            Box(At(f,side*17.04f,4.9f),new Vector3(.18f,.14f,3.4f),Rotation(f),warm);
                        }
                        else if(height>5)
                        {
                            Box(At(f,side*17.24f,1.65f),new Vector3(.22f,3.8f,2.5f),Rotation(f),metal);
                            Box(At(f,side*17.09f,1.65f),new Vector3(.08f,3.5f,.07f),Rotation(f),dark);
                            Box(At(f,side*17.0f,3.85f),new Vector3(.28f,.16f,2.7f),Rotation(f),warm);
                        }
                    }
                    if(!roof && height>3)
                    {
                        // Terrace balustrades establish human scale well above the descending road.
                        Prism(a,b,side*22.1f,side*22.25f,cityA-a.Position.y+1.3f,cityB-b.Position.y+1.3f,cityA-a.Position.y+1.43f,cityB-b.Position.y+1.43f,metal);
                        if(span%2==0)Box(At(f,side*22.18f,(cityA+cityB)*.5f-f.Position.y+.85f),new Vector3(.16f,1.5f,.16f),Rotation(f),metal);
                    }
                    if(height>5 && span%4==0)
                    {
                        float mounting=Mathf.Min(height-1,9);
                        Box(At(f,side*17.0f,mounting),new Vector3(.9f,.25f,1.1f),Rotation(f),stone);
                        Box(At(f,side*16.7f,mounting-.16f),new Vector3(.5f,.08f,.65f),Rotation(f),warm);
                        Practical("Wall wash "+span+" "+side,At(f,side*16.6f,mounting-.3f),At(f,side*17.3f,1.5f),220,18,95);
                    }
                    if(height>4)
                    {
                        Prism(a,b,side*17.25f,side*17.5f,1.45f,1.45f,1.57f,1.57f,cool);
                        if(roof)Prism(a,b,side*17.2f,side*17.52f,6.5f,6.5f,12.0f,12.0f,panels);
                    }
                }
                if(roof)
                {
                    Prism(a,b,-22,22,18,18,cityA-a.Position.y,cityB-b.Position.y,concrete);
                    Prism(a,b,-17.4f,17.4f,17.75f,17.75f,18.05f,18.05f,dark);
                    if(span%2==0)
                    {
                        Box(At(f,0,17.3f),new Vector3(35,.6f,.6f),Rotation(f),metal);
                        Box(At(f,0,16.9f),new Vector3(15,.16f,.8f),Rotation(f),warm);
                        Practical("Gallery practical "+span,At(f,0,15.8f),At(f,0,0),450,28,118);
                    }
                }
            }
            foreach(float p in new[]{.791f,.884f})
            {
                var f=Frame(p);var q=Rotation(f);
                Box(At(f,0,19.2f),new Vector3(45,3.4f,6),q,stone);
                Box(At(f,0,17.4f),new Vector3(34.5f,.22f,4.8f),q,warm);
                foreach(int side in new[]{-1,1})
                {
                    Box(At(f,side*20.4f,8),new Vector3(5.4f,19,6),q,stone);
                    Box(At(f,side*17.6f,8.2f),new Vector3(.2f,14.3f,4.5f),q,panels);
                }
                // Inset threshold crown: quiet cyan against the heavier, warm-lit portal.
                Box(At(f,0,21.1f),new Vector3(23,.22f,6.15f),q,cool);
            }
        }
        static void Practical(string name,Vector3 position,Vector3 target,float intensity,float range,float angle)
        {
            var lamp=new GameObject(name);lamp.transform.SetParent(root,false);lamp.transform.position=position;
            lamp.transform.rotation=Quaternion.LookRotation((target-position).normalized,Vector3.forward);
            var light=lamp.AddComponent<Light>();light.type=LightType.Spot;light.color=new Color(1,.68f,.39f);
            light.intensity=intensity;light.range=range;light.spotAngle=angle;light.innerSpotAngle=angle*.5f;light.shadows=LightShadows.None;
        }
        static Texture2D ConcreteTexture()
        {
            const int size=512;var texture=new Texture2D(size,size,TextureFormat.RGBA32,true){name="Four metre cast panels",wrapMode=TextureWrapMode.Repeat,anisoLevel=4};
            var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=x/(float)size,v=y/(float)size;
                float fine=Mathf.PerlinNoise(u*119+3,v*119+7),broad=Mathf.PerlinNoise(u*7,v*9);
                float streak=Mathf.PerlinNoise(u*33,2.7f)*Mathf.Pow(1-v,2)*.13f;
                float seam=x<2 || y<2 || Mathf.Abs(y-size/2)<1 ? .56f : 1;
                float value=(.82f+fine*.16f+broad*.09f-streak)*seam;
                // Four small formwork ties per slab, deliberately restrained at racing distance.
                bool tie=(Mathf.Abs(x-size*.17f)<2 || Mathf.Abs(x-size*.83f)<2) && (Mathf.Abs(y-size*.15f)<2 || Mathf.Abs(y-size*.65f)<2);
                if(tie)value*=.55f;
                colors[y*size+x]=new Color(value,value*.985f,value*.96f,1);
            }
            texture.SetPixels(colors);texture.Apply(true);return Save(texture,"CastConcrete.asset");
        }
        static void Prism(TrackFrame a,TrackFrame b,float left,float right,float bottomA,float bottomB,float topA,float topB,Material material)
        {
            if(left>right){float swap=left;left=right;right=swap;}
            Vector3 l0=At(a,left,bottomA),r0=At(a,right,bottomA),l1=At(b,left,bottomB),r1=At(b,right,bottomB);
            Vector3 l2=At(a,left,topA),r2=At(a,right,topA),l3=At(b,left,topB),r3=At(b,right,topB);
            var g=Geometry(material);
            g.Quad(l2,l3,r3,r2);g.Quad(l0,r0,r1,l1);g.Quad(l0,l1,l3,l2);
            g.Quad(r0,r2,r3,r1);g.Quad(l0,l2,r2,r0);g.Quad(l1,r1,r3,l3);
        }
        static ProductionGeometry Geometry(Material m){if(!batches.TryGetValue(m,out var g))batches[m]=g=new ProductionGeometry();return g;}
        static void Box(Vector3 center,Vector3 size,Quaternion rotation,Material m)=>Geometry(m).Box(center,size,rotation);
        static Material Material(string name,Color color,float smooth,Color emission=default)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,enableInstancing=true};
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",.15f);
            if(emission.maxColorComponent>0){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);}
            return Save(m,name.Replace(" / ","-")+".mat");
        }
        static T Save<T>(T asset,string name) where T:UnityEngine.Object
        {
            string path=AssetsRoot+"/"+name;var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        static void Flush()
        {
            foreach(var pair in batches)
            {
                var obj=new GameObject(pair.Key.name);obj.transform.SetParent(root,false);obj.isStatic=true;
                var mesh=Save(pair.Value.Mesh(pair.Key.name),pair.Key.name.Replace(" / ","-")+".asset");
                obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
                obj.AddComponent<MeshCollider>().sharedMesh=mesh;
            }
        }
        static string Validate()
        {
            Physics.SyncTransforms();float maxGrade=0,maxGap=0;int clearanceChecks=0;
            var road=UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Single(c=>c.name.StartsWith("Running surface"));
            var walls=root.GetComponentsInChildren<MeshCollider>();
            for(int i=0;i<2400;i++)
            {
                var f=track.Evaluate(i/2400f);maxGrade=Mathf.Max(maxGrade,Mathf.Abs(f.Forward.y)/Vector3.ProjectOnPlane(f.Forward,Vector3.up).magnitude);
                foreach(float lane in new[]{-9f,0f,9f})
                {
                    var point=f.Position+f.Right*lane;
                    if(!road.Raycast(new Ray(point+f.Up*4,-f.Up),out var hit,8))throw new InvalidDataException("Road collider missing at "+i+" lane "+lane);
                    maxGap=Mathf.Max(maxGap,Mathf.Abs(hit.distance-4));
                    foreach(var wall in walls)
                        if(wall.Raycast(new Ray(point+f.Up*10,-f.Up),out _,9))throw new InvalidDataException("Gallery intrudes into driving corridor at "+i+" lane "+lane);
                    clearanceChecks++;
                }
            }
            if(maxGap>.35f)throw new InvalidDataException("Road/curve alignment gap "+maxGap);
            if(maxGrade>.4f)throw new InvalidDataException("Excessive grade "+maxGrade);
            return $"maxGrade={maxGrade:F4}\nmaxRoadAlignmentError={maxGap:F4}\ndrivingClearanceChecks={clearanceChecks}\nextraDepth=22\n";
        }
        static string Hash(string file)=>BitConverter.ToString(SHA256.Create().ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant();
        [Serializable] sealed class Capture { public Shot[] frames; }
        [Serializable] sealed class Shot { public Vector3 cameraPosition; }
        public static void ValidateCapture()
        {
            string evidence=ProductionSceneSetup.RequiredFlag("-productionEvidence");
            var capture=JsonUtility.FromJson<Capture>(File.ReadAllText(Path.Combine(evidence,"preview.json")));
            var args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-experienceScene");
            string scene=flag>=0 && flag+1<args.Length?args[flag+1]:Candidate;
            EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            var enclosure=world.transform.Cast<Transform>().Single(t=>t.name.StartsWith("Underground gallery /"));
            var colliders=enclosure.GetComponentsInChildren<MeshCollider>();Physics.SyncTransforms();
            float distance=0;
            for(int i=1;i<capture.frames.Length;i++)
            {
                var a=capture.frames[i-1].cameraPosition;var delta=capture.frames[i].cameraPosition-a;
                foreach(var collider in colliders)
                {
                    if(collider.Raycast(new Ray(a,delta.normalized),out _,delta.magnitude+.25f))throw new InvalidDataException("Native camera crossed enclosure at frame "+i);
                    foreach(var direction in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
                        if(collider.Raycast(new Ray(capture.frames[i].cameraPosition,direction),out _, .35f))throw new InvalidDataException("Native camera too near enclosure at frame "+i);
                }
                distance+=delta.magnitude;
            }
            File.WriteAllText(Path.Combine(evidence,"camera-clearance.txt"),$"Actual native camera samples={capture.frames.Length}\nSwept camera distance={distance:F2}m\nNew-enclosure crossings=0\nSix-axis proximity hits within 0.35m=0\nNot a proof of every manual camera or driving path.\n");
        }
        public static void Audit()
        {
            var output = ProductionSceneSetup.RequiredFlag("-productionEvidence");
            Directory.CreateDirectory(output);
            EditorSceneManager.OpenScene(EnvironmentStructureSetup.Candidate, OpenSceneMode.Single);
            var world = UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            var text = new StringBuilder();
            text.AppendLine("COURSE " + ProductionSceneSetup.CourseHash(world.track));
            foreach (var f in world.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = f.GetComponent<Renderer>();
                text.AppendLine($"MESH {PathOf(f.transform)} | {f.sharedMesh?.vertexCount} | {f.sharedMesh?.bounds} | {string.Join(",", r ? r.sharedMaterials.Select(m=>m ? m.name : "null") : Array.Empty<string>())} | collider={f.GetComponent<Collider>() != null}");
            }
            foreach (var c in world.GetComponentsInChildren<Collider>(true))
                text.AppendLine("COLLIDER " + PathOf(c.transform) + " " + c.GetType().Name);
            for (int i=65;i<=98;i++)
            {
                var f=world.track.Evaluate(i*.01f);
                text.AppendLine($"COURSE {i*.01f:F2} {f.Position} grade={f.Forward.y/Vector3.ProjectOnPlane(f.Forward,Vector3.up).magnitude:F4}");
            }
            File.WriteAllText(System.IO.Path.Combine(output,"audit.txt"),text.ToString());
            Debug.Log("UNDERGROUND_AUDIT_READY");
        }
        static string PathOf(Transform t) => t.parent ? PathOf(t.parent)+"/"+t.name : t.name;
    }
}
