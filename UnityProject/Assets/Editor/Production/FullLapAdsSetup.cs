using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Editor
{
    // Explicit authoring only. Stage 7 and its assets remain the playable comparison.
    public static class FullLapAdsSetup
    {
        public const string Candidate="Assets/Scenes/FullLapAdsStage8.unity";
        const string Root="Assets/Art/FullLapAdsStage8";
        const string Kit="Assets/Art/NightCityKit";
        static Transform root;
        static TrackPath track;
        static Material steel,concrete;
        static int serial;
        [Serializable] public class Site {public string name;public float progress;public int side;public Vector3 position;}
        [Serializable] public class Report {public string revision="full-lap-ads-stage8-02",courseHash;public bool sourceUnchanged,collidersUnchanged;public List<Site> sites=new();}
        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence");Directory.CreateDirectory(output);
            string hash=ProductionSceneSetup.Hash(File.ReadAllBytes(NightCityIntegrationSetup.Candidate));
            var scene=EditorSceneManager.OpenScene(NightCityIntegrationSetup.Candidate,OpenSceneMode.Single);
            string physics=PhysicsSignature();
            EditorSceneManager.SaveScene(scene,Candidate,true);scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();world.ValidateReady();track=world.track;
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();serial=0;
            steel=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/painted_graphite.mat");
            concrete=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/cast_concrete.mat");
            root=new GameObject("Full-lap advertising / supported compositions").transform;root.SetParent(world.transform,false);
            var playback=UnityEngine.Object.FindFirstObjectByType<AnimatedBillboards>();
            var report=new Report{courseHash=world.courseHash};
            // Keep one opening landmark; distribute the remaining compositions through the lap.
            float[] progress={.045f,.265f,.615f,.765f,.885f};int[] sides={1,-1,1,-1,1};
            for(int group=0;group<5;group++) {
                var screens=playback.displays.Where(t=>t.parent.name.StartsWith("Cluster "+group+" /")).ToArray();
                if(screens.Length==0)throw new InvalidDataException("Missing animated cluster "+group);
                if(group==0){report.sites.Add(new Site{name="Opening / AURORA",progress=track.ClosestProgress(screens[0].position),side=sides[group],position=screens[0].position});continue;}
                var pivot=screens.Select(t=>t.parent.position).Aggregate(Vector3.zero,(a,b)=>a+b)/screens.Length;
                var oldRotation=screens[0].parent.rotation;
                var composition=new GameObject("Animated cluster "+group).transform;composition.SetParent(root,false);composition.SetPositionAndRotation(pivot,oldRotation);
                foreach(var screen in screens)screen.parent.SetParent(composition,true);
                var frame=track.Evaluate(progress[group]);var right=Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
                var heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(frame.Forward,Vector3.up)+right*sides[group]*.8f);
                heading=Quaternion.Euler(0,Mathf.Round(heading.eulerAngles.y/90)*90,0);
                // Full cabinet bounds, including its return panel, determine ground-supported siting.
                composition.rotation=heading;var bounds=BoundsOf(composition);
                Vector3 target=frame.Position+right*sides[group]*43;
                target.y=frame.Position.y+5+(pivot.y-bounds.min.y);
                composition.position=target;
                // Move the matching facade wash with its cluster; leave all weather/world lights alone.
                var wash=world.GetComponentsInChildren<Light>(true).FirstOrDefault(l=>l.name=="Cluster facade wash "+group);
                if(wash){wash.transform.position=target+heading*Quaternion.Inverse(oldRotation)*(wash.transform.position-pivot);wash.transform.rotation=heading*Quaternion.Inverse(oldRotation)*wash.transform.rotation;}
                if(group>=3) {
                    if(group==4) {
                        // A projecting blade would cross the narrow gallery shoulder. Fit it as a lower ticker.
                        screens[0].parent.localPosition=new Vector3(0,2,0);
                        screens[1].parent.localPosition=new Vector3(0,-7,0);
                        screens[1].parent.localRotation=Quaternion.identity;
                    }
                    composition.localScale=Vector3.one*.30f;
                    composition.rotation=Quaternion.LookRotation(right*sides[group],Vector3.up);
                    composition.position=frame.Position+right*sides[group]*16.05f+Vector3.up*(group==3?6.5f:7.4f);
                    target=composition.position;
                    foreach(var screen in screens)WallSupports(screen.parent);
                } else foreach(var screen in screens)Supports(screen.parent,screen.GetComponent<MeshFilter>().sharedMesh.bounds.size);
                report.sites.Add(new Site{name="Animated cluster "+group,progress=progress[group],side=sides[group],position=target});
            }
            // Replace the old baked support batch at the new screen locations.
            RebuildCabinets(world,playback);
            PlacePoster("Sign_portrait","After Hours",.315f,-1,2.6f,report);
            PlacePoster("Sign_koi","Night Market",.685f,1,2.4f,report);
            PlacePoster("Sign_transit","Last Train",.935f,-1,1.5f,report);
            UpdateReflections(world,playback);
            ValidateClearance(world);
            report.collidersUnchanged=physics==PhysicsSignature();if(!report.collidersUnchanged)throw new InvalidDataException("Racing collision changed");
            world.artRevision=report.revision;world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            report.sourceUnchanged=hash==ProductionSceneSetup.Hash(File.ReadAllBytes(NightCityIntegrationSetup.Candidate));
            if(!report.sourceUnchanged)throw new InvalidDataException("Stage 7 source modified");
            File.WriteAllText(output+"/full-lap-ads.json",JsonUtility.ToJson(report,true));
            Debug.Log("FULL_LAP_ADS_AUTHORED: 5 animated compositions and 3 art-kit campaigns");
        }
        static void PlacePoster(string id,string title,float progress,int side,float scale,Report report)
        {
            var frame=track.Evaluate(progress);var right=Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Prefabs/NC_"+id+".prefab");if(!prefab)throw new FileNotFoundException(id);
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.name=title+" / full-lap artwork";g.transform.SetParent(root,false);g.transform.localScale=Vector3.one*scale;
            var face=-Vector3.ProjectOnPlane(frame.Forward,Vector3.up)-right*side*.7f;
            var q=Quaternion.LookRotation(face);g.transform.rotation=Quaternion.Euler(0,Mathf.Round(q.eulerAngles.y/45)*45,0);
            g.transform.position=frame.Position+right*side*30+Vector3.up*3;
            if(progress>.70f) {
                g.transform.rotation=Quaternion.LookRotation(-right*side,Vector3.up);
                g.transform.position=frame.Position+right*side*15.95f+Vector3.up*2;
                WallSupports(g.transform);
                report.sites.Add(new Site{name=title,progress=progress,side=side,position=g.transform.position});return;
            }
            // Source prefabs include their frame and small mounting feet; add columns to the city base.
            var mount=new GameObject(title+" / structural support").transform;mount.SetParent(root,false);mount.SetPositionAndRotation(g.transform.position,g.transform.rotation);
            float width=id=="Sign_koi"?6.2f: id=="Sign_portrait"?3.5f:3f;
            foreach(float x in new[]{-.32f,.32f})Column(mount,new Vector3(x*width*scale,0,-.25f*scale),-39,steel);
            Box(mount,"Foundation",new Vector3(0,-39-mount.position.y-.5f,0),new Vector3(width*scale+2,1,4),concrete);
            report.sites.Add(new Site{name=title,progress=progress,side=side,position=g.transform.position});
        }
        static void WallSupports(Transform display)
        {
            var nearest=track.Evaluate(track.ClosestProgress(display.position));
            var right=Vector3.ProjectOnPlane(nearest.Right,Vector3.up).normalized;
            float side=Mathf.Sign(Vector3.Dot(display.position-nearest.Position,right));
            var bounds=BoundsOf(display);float half=Mathf.Min(bounds.size.y*.28f,3);
            foreach(float y in new[]{-half,half}) {
                var a=bounds.center+Vector3.up*y;
                var b=a+right*side*(17.45f-Mathf.Abs(Vector3.Dot(a-nearest.Position,right)));
                var beam=new GameObject("Wall anchor / "+display.name).transform;beam.SetParent(root,false);
                beam.position=(a+b)*.5f;beam.rotation=Quaternion.LookRotation(b-a);
                Box(beam,"Short wall bracket",Vector3.zero,new Vector3(.22f,.24f,Mathf.Max(.25f,Vector3.Distance(a,b))),steel);
            }
        }
        static void Supports(Transform mount,Vector3 size)
        {
            var stand=new GameObject(mount.name+" / foundation").transform;stand.SetParent(root,false);stand.SetPositionAndRotation(mount.position,mount.rotation);
            float floor=-size.y*.5f-.7f;
            foreach(float x in new[]{-.30f,.30f})Column(stand,new Vector3(x*size.x,floor,.8f),-39,steel);
            Box(stand,"Foundation",new Vector3(0,-39-stand.position.y-.5f,.8f),new Vector3(size.x*.75f+2,1,4),concrete);
            Box(stand,"Supporting crosshead",new Vector3(0,floor,.8f),new Vector3(size.x+1,1.0f,2.5f),steel);
        }
        static void RebuildCabinets(ProductionWorld world,AnimatedBillboards playback)
        {
            foreach(var r in world.GetComponentsInChildren<Renderer>(true).Where(r=>r.name=="Billboard recessed frames and facade supports"))r.enabled=false;
            foreach(var screen in playback.displays) {
                var mount=screen.parent;var size=screen.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                var trim=new GameObject("Recessed cabinet / "+screen.name).transform;trim.SetParent(mount,false);
                foreach(float side in new[]{-1f,1f})Box(trim,"Side lip",new Vector3(side*(size.x*.5f+.32f),0,-.12f),new Vector3(.48f,size.y+1.35f,.65f),steel);
                Box(trim,"Rain hood",new Vector3(0,size.y*.5f+.7f,-.35f),new Vector3(size.x+1.65f,.25f,1.6f),steel);
                Box(trim,"Bottom ledge",new Vector3(0,-size.y*.5f-.7f,-.05f),new Vector3(size.x+1.65f,.38f,1.9f),steel);
            }
            // The preserved opening screen also receives grounded support after the old batch is hidden.
            Supports(playback.displays[0].parent,playback.displays[0].GetComponent<MeshFilter>().sharedMesh.bounds.size);
        }
        static void Column(Transform parent,Vector3 top,float ground,Material material)
        {
            float bottom=ground-parent.position.y;
            if(top.y<=bottom)throw new InvalidDataException("Invalid support height");
            Box(parent,"Braced column",new Vector3(top.x,(top.y+bottom)*.5f,top.z),new Vector3(.65f,top.y-bottom,.75f),material);
        }
        static void Box(Transform parent,string name,Vector3 position,Vector3 size,Material material)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;
        }
        static Bounds BoundsOf(Transform parent)
        {
            var rr=parent.GetComponentsInChildren<Renderer>();if(rr.Length==0)throw new InvalidDataException("Empty composition");
            var b=rr[0].bounds;foreach(var r in rr.Skip(1))b.Encapsulate(r.bounds);return b;
        }
        static void UpdateReflections(ProductionWorld world,AnimatedBillboards playback)
        {
            int count=playback.displays.Length;var data=new Color[count*8];
            for(int i=0;i<count;i++) {
                var t=playback.displays[i];var m=t.GetComponent<Renderer>().sharedMaterial;var size=Vector3.Scale(t.GetComponent<MeshFilter>().sharedMesh.bounds.size,t.lossyScale);
                var p=t.position;var r=t.right;var u=t.up;var n=-t.forward;
                data[i*8]=new Color(p.x,p.y,p.z,m.GetFloat("_Campaign"));data[i*8+1]=new Color(r.x,r.y,r.z,size.x);data[i*8+2]=new Color(u.x,u.y,u.z,size.y);
                data[i*8+3]=new Color(n.x,n.y,n.z,m.GetFloat("_Phase"));data[i*8+4]=new Color(m.GetFloat("_Style"),m.GetFloat("_CropStart"),m.GetFloat("_CropWidth"),m.GetFloat("_Intensity"));
            }
            var poses=new Texture2D(8,count,TextureFormat.RGBAFloat,false,true){name="Full lap animated display poses",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};poses.SetPixels(data);poses.Apply();poses=Save(poses,"SignPoses.asset");
            var clones=new Dictionary<Material,Material>();
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>(true)) {
                var slots=renderer.sharedMaterials;bool changed=false;
                for(int i=0;i<slots.Length;i++) {
                    var m=slots[i];if(!m||m.shader.name!="VectorRush/Animated City Reflection")continue;
                    if(!clones.TryGetValue(m,out var clone)){clone=new Material(m){name=m.name+" / full lap"};clone.SetTexture("_SignData",poses);clone.SetFloat("_SignCount",count);clone=Save(clone,"Reflection-"+(serial++)+".mat");clones.Add(m,clone);}
                    slots[i]=clone;changed=true;
                }
                if(changed)renderer.sharedMaterials=slots;
            }
        }
        static T Save<T>(T asset,string name) where T:UnityEngine.Object
        {
            string path=Root+"/"+name;var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        public static void ValidateClearance(ProductionWorld world)
        {
            var currentRoot=world.transform.Cast<Transform>().FirstOrDefault(t=>t.name=="Full-lap advertising / supported compositions");
            if(!currentRoot)throw new InvalidDataException("Missing full-lap advertising root");
            var renderers=currentRoot.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();
            for(int k=0;k<2400;k++) {
                var f=world.track.Evaluate(k/2400f);
                for(int side=-6;side<=6;side++) {
                    Vector3 p=f.Position+f.Right*(side/6f)*(world.track.Width*.5f+3);
                    foreach(var r in renderers) {
                        var filter=r.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;
                        var a=r.transform.InverseTransformPoint(p-Vector3.up*2);
                        var delta=r.transform.InverseTransformVector(Vector3.up*10);
                        if(IntersectsSegment(filter.sharedMesh.bounds,a,delta))throw new InvalidDataException(r.name+" intersects driving corridor at "+k/2400f);
                    }
                }
            }
        }
        static bool IntersectsSegment(Bounds bounds,Vector3 origin,Vector3 delta)
        {
            float enter=0,exit=1;
            for(int axis=0;axis<3;axis++) {
                if(Mathf.Abs(delta[axis])<1e-6f){if(origin[axis]<bounds.min[axis]||origin[axis]>bounds.max[axis])return false;continue;}
                float a=(bounds.min[axis]-origin[axis])/delta[axis],b=(bounds.max[axis]-origin[axis])/delta[axis];
                if(a>b){float temp=a;a=b;b=temp;}enter=Mathf.Max(enter,a);exit=Mathf.Min(exit,b);if(enter>exit)return false;
            }
            return true;
        }
        static string PhysicsSignature()=>string.Join("\n",UnityEngine.Object.FindFirstObjectByType<ProductionWorld>().GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(x=>x));
    }
}
