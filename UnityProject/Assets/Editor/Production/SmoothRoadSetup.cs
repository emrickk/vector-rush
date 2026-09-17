using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Editor
{
    public static class SmoothRoadSetup
    {
        public const string Candidate="Assets/Scenes/SmoothRoadStage3.unity";
        const string Root="Assets/Art/SmoothRoadStage3";
        static TrackPath old,track;
        static TrackFrame[] reference;
        static readonly Dictionary<Vector2,float> projections=new();
        static int serial;
        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence");
            if(Directory.Exists(output)&&Directory.GetFileSystemEntries(output).Length>0)throw new IOException("Use fresh authoring evidence");
            Directory.CreateDirectory(output);
            string before=ProductionSceneSetup.Hash(File.ReadAllBytes(UndergroundGallerySetup.Candidate));
            var scene=EditorSceneManager.OpenScene(UndergroundGallerySetup.Candidate,OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene,Candidate,true);
            scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();track=world.track;
            var helper=new GameObject("Original finish reference");old=helper.AddComponent<TrackPath>();old.SetUndergroundGallery(true);
            reference=Enumerable.Range(0,2401).Select(i=>old.Evaluate(i/2400f)).ToArray();
            projections.Clear();serial=0;track.SetSmoothRoad(true);
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            foreach(var f in world.GetComponentsInChildren<MeshFilter>(true))
            {
                var r=f.GetComponent<MeshRenderer>();if(!r || !f.sharedMesh)continue;
                var m=r.sharedMaterial;string name=f.name;Mesh mesh=null;
                if(name.Contains("authoritative collision"))
                {
                    if(name.StartsWith("Running"))mesh=ProductionGeometry.Ribbon(track,-11,11,0);
                    else if(name.StartsWith("Left shoulder"))mesh=ProductionGeometry.Ribbon(track,-12,-11,-.02f);
                    else if(name.StartsWith("Right shoulder"))mesh=ProductionGeometry.Ribbon(track,11,12,-.02f);
                    else mesh=ProductionGeometry.Barrier(track,name.StartsWith("Left")?-1:1);
                }
                else if(m && m.HasProperty("_RoadSurface") && m.GetFloat("_RoadSurface")>.5f)
                    mesh=ProductionGeometry.Ribbon(track,-10.7f,10.7f,.024f);
                else if(m && m.name.Contains("localized road response"))
                {
                    int side=m.name.StartsWith("Cyan")?-1:1;
                    mesh=ProductionGeometry.Ribbon(track,side*6.7f,side*10.8f,.035f);
                    var uv=mesh.uv;for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(uv[i].y,side<0?uv[i].x:1-uv[i].x);mesh.uv=uv;
                }
                else if(m && (m.name.StartsWith("Preserved rail") || m.name=="Cyan rail core" || m.name=="Magenta signal core" || m.name=="Amber service lamps"))
                {
                    var vertices=f.sharedMesh.vertices;bool changed=false;
                    for(int i=0;i<vertices.Length;i++)
                    {
                        var v=f.transform.TransformPoint(vertices[i]);float p=Project(v);var a=old.Evaluate(p);var local=v-a.Position;
                        if(Mathf.Abs(Vector3.Dot(local,a.Right))>16 || Mathf.Abs(Vector3.Dot(local,a.Up))>5)continue;
                        var b=track.EvaluateParameter(p);
                        v=b.Position+b.Right*Vector3.Dot(local,a.Right)+b.Up*Vector3.Dot(local,a.Up)+b.Forward*Vector3.Dot(local,a.Forward);
                        vertices[i]=f.transform.InverseTransformPoint(v);changed=true;
                    }
                    if(changed){mesh=UnityEngine.Object.Instantiate(f.sharedMesh);mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();}
                }
                if(!mesh)continue;
                // Generated ribbons are world-space. Existing road filters are identity in the source.
                if(name.Contains("authoritative collision") && (f.transform.position.sqrMagnitude>.001f || Quaternion.Angle(f.transform.rotation,Quaternion.identity)>.01f))
                    throw new InvalidDataException("Nonidentity driving mesh transform");
                mesh.name=f.sharedMesh.name+" / smooth road";
                f.sharedMesh=Save(mesh,"Road-"+(serial++)+".asset");
                var c=f.GetComponent<MeshCollider>();if(c)c.sharedMesh=f.sharedMesh;
            }
            foreach(var l in world.GetComponentsInChildren<Light>(true).Where(l=>l.name.StartsWith("Local rail wash")))
            {float p=Project(l.transform.position);var a=old.Evaluate(p);var b=track.EvaluateParameter(p);var d=l.transform.position-a.Position;l.transform.position=b.Position+b.Right*Vector3.Dot(d,a.Right)+b.Up*Vector3.Dot(d,a.Up)+b.Forward*Vector3.Dot(d,a.Forward);}
            var metrics=Measure(old,track);File.WriteAllText(Path.Combine(output,"smoothness.json"),JsonUtility.ToJson(metrics,true));
            if(metrics.maxCenterDisplacement>3.5f)throw new InvalidDataException("Excessive course displacement "+metrics.maxCenterDisplacement);
            ValidateRoad(world,Path.Combine(output,"road-clearance.txt"));
            world.courseHash=ProductionSceneSetup.CourseHash(track);world.artRevision="smooth-road-stage3-01";
            ProductionSceneSetup.ExportContextToDirectory(track,Path.Combine(output,"course"),"smooth-road-candidate");
            UnityEngine.Object.DestroyImmediate(helper);world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(before!=ProductionSceneSetup.Hash(File.ReadAllBytes(UndergroundGallerySetup.Candidate)))throw new InvalidDataException("Previous scene changed");
            File.WriteAllText(Path.Combine(output,"source-preservation.txt"),"Previous finish scene SHA256="+before+"\nUnchanged=true\nCloned/rebuilt meshes="+serial+"\nCourse="+world.courseHash+"\n");
        }
        static float Project(Vector3 v)
        {
            var key=new Vector2(v.x,v.z);if(projections.TryGetValue(key,out float p))return p;
            float best=float.MaxValue;p=0;
            for(int i=0;i<2400;i++)
            {var a=new Vector2(reference[i].Position.x,reference[i].Position.z);var ab=new Vector2(reference[i+1].Position.x,reference[i+1].Position.z)-a;float f=Mathf.Clamp01(Vector2.Dot(key-a,ab)/ab.sqrMagnitude);float d=(key-a-ab*f).sqrMagnitude;if(d<best){best=d;p=(i+f)/2400f;}}
            projections[key]=p;return p;
        }
        static Mesh Save(Mesh mesh,string name)
        {string path=Root+"/"+name;var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior){EditorUtility.CopySerialized(mesh,prior);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(prior);return prior;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
        static float Roll(TrackFrame f)=>Vector3.SignedAngle(Vector3.Cross(Vector3.up,f.Forward).normalized,f.Right,f.Forward);
        [Serializable] public class Smoothness {public float maxCenterDisplacement;public float oldMaxCurvatureRate,newMaxCurvatureRate,oldMaxBankRate,newMaxBankRate;public float oldLength,newLength;}
        public static Smoothness Measure(TrackPath before,TrackPath after)
        {
            var m=new Smoothness{oldLength=before.Length,newLength=after.Length};
            for(int i=0;i<4800;i++)m.maxCenterDisplacement=Mathf.Max(m.maxCenterDisplacement,Vector3.Distance(before.Evaluate(i/4800f).Position,after.EvaluateParameter(i/4800f).Position));
            Rates(before,out m.oldMaxCurvatureRate,out m.oldMaxBankRate);Rates(after,out m.newMaxCurvatureRate,out m.newMaxBankRate);return m;
        }
        static void Rates(TrackPath t,out float curvatureRate,out float bankRate)
        {
            const float step=.5f;curvatureRate=bankRate=0;float prior=0;
            int count=Mathf.CeilToInt(t.Length/step);
            for(int i=-1;i<count;i++)
            {
                float p=i*step/t.Length;var a=t.Evaluate(p);var b=t.Evaluate(p+step/t.Length);
                float k=Vector3.SignedAngle(Vector3.ProjectOnPlane(a.Forward,Vector3.up),Vector3.ProjectOnPlane(b.Forward,Vector3.up),Vector3.up)*Mathf.Deg2Rad/step;
                if(i>=0)curvatureRate=Mathf.Max(curvatureRate,Mathf.Abs(k-prior)/step);
                bankRate=Mathf.Max(bankRate,Mathf.Abs(Roll(b)-Roll(a))/step);prior=k;
            }
        }
        public static void ValidateRoad(ProductionWorld world,string output=null)
        {
            Physics.SyncTransforms();var road=world.GetComponentsInChildren<MeshCollider>().Single(c=>c.name.StartsWith("Running surface"));
            var others=world.GetComponentsInChildren<MeshCollider>().Where(c=>!c.name.Contains("authoritative collision")&&c.enabled&&c.gameObject.activeInHierarchy).ToArray();float maxGap=0;
            for(int i=0;i<4800;i++)
            {
                var f=world.track.Evaluate(i/4800f);
                foreach(float x in new[]{-9f,0f,9f})
                {var p=f.Position+f.Right*x;if(!road.Raycast(new Ray(p+f.Up*4,-f.Up),out var hit,8))throw new InvalidDataException("Road missing at "+i);maxGap=Mathf.Max(maxGap,Mathf.Abs(hit.distance-4));
                    foreach(var c in others)if(c.Raycast(new Ray(p+f.Up*10,-f.Up),out _,9))throw new InvalidDataException("Corridor obstacle "+c.name+" at "+i);
                }
            }
            if(maxGap>.035f)throw new InvalidDataException("Road alignment error "+maxGap);
            if(output!=null)File.WriteAllText(output,$"laneRays=14400\nmaxRoadAlignmentError={maxGap:F6}\notherColliderIntrusions=0\n");
        }
    }
}
