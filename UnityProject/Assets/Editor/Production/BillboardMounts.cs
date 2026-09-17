using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VectorRush.Editor
{
    // Authoring-only facade queries. These supports introduce no runtime colliders.
    public static class BillboardMounts
    {
        struct Triangle {public Vector3 a,b,c;}
        [Serializable] sealed class MountRecord {public string display,supportedBy;public int anchors;public float nearestFacade,farthestFacade;}
        [Serializable] sealed class Report {public List<MountRecord> mounts=new List<MountRecord>();}
        public static void Build(ProductionWorld world,Transform[] screens,string root,string evidence,Material metal)
        {
            var triangles=new List<Triangle>();
            foreach(var filter in world.GetComponentsInChildren<MeshFilter>()){
                var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled||!filter.sharedMesh)continue;
                var mesh=filter.sharedMesh;var materials=renderer.sharedMaterials;var vertices=mesh.vertices;
                for(int sub=0;sub<mesh.subMeshCount;sub++){
                    var material=materials[Mathf.Min(sub,materials.Length-1)];
                    if(!material||!(material.name.StartsWith("Office facade")||material.name.StartsWith("Distant facade")))continue;
                    var indices=mesh.GetTriangles(sub);
                    for(int i=0;i<indices.Length;i+=3)triangles.Add(new Triangle{a=filter.transform.TransformPoint(vertices[indices[i]]),b=filter.transform.TransformPoint(vertices[indices[i+1]]),c=filter.transform.TransformPoint(vertices[indices[i+2]])});
                }
            }
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var cube=primitive.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(primitive);
            var parts=new List<CombineInstance>();var report=new Report();
            foreach(var screen in screens){
                var mount=screen.parent;var size=screen.GetComponent<MeshFilter>().sharedMesh.bounds.size;float w=size.x,h=size.y;
                Matrix4x4 matrix=world.transform.worldToLocalMatrix*mount.localToWorldMatrix;
                void Box(Vector3 p,Vector3 scale)=>parts.Add(new CombineInstance{mesh=cube,transform=matrix*Matrix4x4.TRS(p,Quaternion.identity,scale)});
                void Beam(Vector3 a,Vector3 b,float thickness)=>parts.Add(new CombineInstance{mesh=cube,transform=matrix*Matrix4x4.TRS((a+b)*.5f,Quaternion.LookRotation(b-a),new Vector3(thickness,thickness,Vector3.Distance(a,b)))});
                // The lip projects ahead of the screen, making a physical inset without moving its face.
                foreach(float side in new[]{-1f,1f})Box(new Vector3(side*(w*.5f+.32f),0,-.12f),new Vector3(.48f,h+1.35f,.65f));
                Box(new Vector3(0,h*.5f+.7f,-.35f),new Vector3(w+1.65f,.25f,1.6f));
                Box(new Vector3(0,-h*.5f-.7f,-.05f),new Vector3(w+1.65f,.38f,1.9f));
                var record=new MountRecord{display=mount.name,nearestFacade=float.MaxValue};
                void Anchor(float x,float y){
                    var origin=mount.TransformPoint(new Vector3(x,y,1.12f));float depth=Nearest(origin,mount.forward,triangles);
                    if(depth<0)return;float z=depth+1.12f;
                    Box(new Vector3(x,y,(1.05f+z)*.5f),new Vector3(.55f,.55f,Mathf.Max(.15f,z-1.05f+.25f)));
                    Box(new Vector3(x,y,z+.12f),new Vector3(1.6f,2.1f,.5f));
                    if(depth>1.5f)Beam(new Vector3(x,y-Mathf.Min(h*.25f,4f),1.05f),new Vector3(x,y,z),.25f);
                    record.anchors++;record.nearestFacade=Mathf.Min(record.nearestFacade,depth);record.farthestFacade=Mathf.Max(record.farthestFacade,depth);
                }
                foreach(float x in new[]{-w*.32f,w*.32f})foreach(float y in new[]{-h*.28f,h*.28f})Anchor(x,y);
                if(record.anchors==0)Anchor(0,0);
                if(record.anchors==0){
                    string group=mount.name.Split('/')[0];
                    var neighbor=Array.Find(screens,t=>t!=screen&&t.parent.name.StartsWith(group)&&report.mounts.Exists(r=>r.display==t.parent.name&&r.anchors>0));
                    if(neighbor){
                        for(int side=-1;side<=1;side+=2){
                            var a=new Vector3(0,side*h*.25f,1.05f);
                            var destination=neighbor.parent.TransformPoint(new Vector3(0,side*Mathf.Min(h,neighbor.GetComponent<MeshFilter>().sharedMesh.bounds.size.y)*.25f,1.05f));
                            Beam(a,mount.InverseTransformPoint(destination),.65f);
                        }
                        record.supportedBy=neighbor.parent.name;record.nearestFacade=0;
                    }
                }
                report.mounts.Add(record);
            }
            File.WriteAllText(Path.Combine(evidence,"mounts.json"),JsonUtility.ToJson(report,true));
            foreach(var record in report.mounts)if(record.anchors==0&&string.IsNullOrEmpty(record.supportedBy))throw new InvalidDataException("No facade attachment found for "+record.display+"; inspect mounts.json before moving signs.");
            var combined=new Mesh{name="Recessed cabinets and facade-connected steel supports"};combined.CombineMeshes(parts.ToArray(),true,true);combined.RecalculateBounds();
            string path=root+"/Architectural-mounts.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved){EditorUtility.CopySerialized(combined,saved);UnityEngine.Object.DestroyImmediate(combined);EditorUtility.SetDirty(saved);}else{AssetDatabase.CreateAsset(combined,path);saved=combined;}
            var batch=new GameObject("Billboard recessed frames and facade supports");batch.transform.SetParent(world.transform,false);batch.AddComponent<MeshFilter>().sharedMesh=saved;batch.AddComponent<MeshRenderer>().sharedMaterial=metal;
        }
        static float Nearest(Vector3 origin,Vector3 direction,List<Triangle> triangles)
        {
            float nearest=24;
            foreach(var t in triangles){
                var e1=t.b-t.a;var e2=t.c-t.a;var normal=Vector3.Cross(e1,e2).normalized;
                if(Mathf.Abs(Vector3.Dot(normal,direction))<.8f)continue;
                var p=Vector3.Cross(direction,e2);float determinant=Vector3.Dot(e1,p);if(Mathf.Abs(determinant)<1e-6f)continue;
                float inv=1/determinant;var delta=origin-t.a;float u=Vector3.Dot(delta,p)*inv;if(u<0||u>1)continue;
                var q=Vector3.Cross(delta,e1);float v=Vector3.Dot(direction,q)*inv;if(v<0||u+v>1)continue;
                float distance=Vector3.Dot(e2,q)*inv;if(distance>.02f&&distance<nearest)nearest=distance;
            }
            return nearest<24?nearest:-1;
        }
    }
}
