using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Editor
{
    public static class NightCityIntegrationSetup
    {
        public const string Candidate = "Assets/Scenes/NightCityStage7.unity";
        const string Kit = "Assets/Art/NightCityKit";
        const string Root = "Assets/Art/NightCityStage7";
        struct Triangle { public Vector3 a,b,c; }
        [Serializable] public class Site { public float progress; public int side; public Vector3 road,wall,normal; public float distance; }
        [Serializable] class Survey { public List<Site> sites = new(); }
        static List<Triangle> Facades(ProductionWorld world)
        {
            var result = new List<Triangle>();
            foreach(var f in world.GetComponentsInChildren<MeshFilter>()) {
                var r=f.GetComponent<MeshRenderer>(); if(!r||!r.enabled||!f.sharedMesh)continue;
                var m=f.sharedMesh; var vertices=m.vertices;
                for(int s=0;s<m.subMeshCount;s++) {
                    var mat=r.sharedMaterials[Mathf.Min(s,r.sharedMaterials.Length-1)];
                    if(!mat||!mat.name.StartsWith("Office facade"))continue;
                    var ix=m.GetTriangles(s);
                    for(int k=0;k<ix.Length;k+=3)result.Add(new Triangle{a=f.transform.TransformPoint(vertices[ix[k]]),b=f.transform.TransformPoint(vertices[ix[k+1]]),c=f.transform.TransformPoint(vertices[ix[k+2]])});
                }
            }
            return result;
        }
        static bool Hit(Vector3 origin,Vector3 dir,List<Triangle> triangles,out Vector3 point,out Vector3 normal)
        {
            float closest=100;point=normal=Vector3.zero;
            foreach(var t in triangles) {
                var e1=t.b-t.a;var e2=t.c-t.a;var n=Vector3.Cross(e1,e2).normalized;
                if(Mathf.Abs(Vector3.Dot(n,dir))<.4f)continue;
                var p=Vector3.Cross(dir,e2);float det=Vector3.Dot(e1,p);if(Mathf.Abs(det)<1e-6f)continue;
                var delta=origin-t.a;float u=Vector3.Dot(delta,p)/det;if(u<0||u>1)continue;
                var q=Vector3.Cross(delta,e1);float v=Vector3.Dot(dir,q)/det;if(v<0||v+u>1)continue;
                float d=Vector3.Dot(e2,q)/det;if(d<.05f||d>closest)continue;
                closest=d;point=origin+dir*d;normal=Vector3.Dot(n,dir)>0?-n:n;
            }
            return closest<100;
        }
        public static void Inspect()
        {
            EditorSceneManager.OpenScene(BillboardSceneSetup.Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();world.ValidateReady();
            var faces=Facades(world);var survey=new Survey();
            for(float p=.03f;p<.305f;p+=.0075f)foreach(int side in new[]{-1,1}) {
                var f=world.track.Evaluate(p);var right=Vector3.ProjectOnPlane(f.Right,Vector3.up).normalized;
                if(Hit(f.Position-Vector3.up*3,right*side,faces,out var hit,out var n))survey.sites.Add(new Site{progress=p,side=side,road=f.Position,wall=hit,normal=n,distance=Vector3.Distance(hit,f.Position-Vector3.up*3)});
            }
            File.WriteAllText(ProductionSceneSetup.RequiredFlag("-productionEvidence")+"/sites.json",JsonUtility.ToJson(survey,true));
            Debug.Log("NIGHT_CITY_SITE_SURVEY_COMPLETE");
        }
        [Serializable] class IntegrationReport
        {
            public string sourceRevision="animated-billboards-stage6-06",revision="night-city-stage7-01",courseHash;
            public bool sourceSceneUnchanged,collidersUnchanged,billboardsUnchanged;
            public int prefabs;public List<Site> sites=new();
        }
        static Transform district;
        static TrackPath track;
        static int placed;
        static readonly List<Bounds> footprints=new();
        static List<Triangle> siteFacades;
        public static void Prepare()
        {
            string output=ProductionSceneSetup.RequiredFlag("-productionEvidence");Directory.CreateDirectory(output);
            string original=ProductionSceneSetup.Hash(File.ReadAllBytes(BillboardSceneSetup.Candidate));
            var scene=EditorSceneManager.OpenScene(BillboardSceneSetup.Candidate,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();world.ValidateReady();
            string colliderState=ColliderState(world),displayState=DisplayState();
            EditorSceneManager.SaveScene(scene,Candidate,true);scene=EditorSceneManager.OpenScene(Candidate,OpenSceneMode.Single);
            world=UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();track=world.track;
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            district=new GameObject("Night city / occupied service terraces").transform;district.SetParent(world.transform,false);
            placed=0;footprints.Clear();var faces=Facades(world);siteFacades=faces;
            var report=new IntegrationReport{courseHash=world.courseHash};
            // Alternate sides and leave gaps around P4's large animated compositions.
            float[] targets={.055f,.105f,.167f,.230f,.280f};int[] sides={-1,1,-1,1,-1};
            for(int i=0;i<targets.Length;i++) {
                Site site=null;
                foreach(float delta in new[]{0f,-.004f,.004f,-.008f,.008f}) {
                    var f=track.Evaluate(targets[i]+delta);var right=Vector3.ProjectOnPlane(f.Right,Vector3.up).normalized;
                    if(!Hit(f.Position-Vector3.up*3,right*sides[i],faces,out var wall,out var normal))continue;
                    // The frontage has an architectural heading, inherited from the real facade.
                    float gap=Vector3.Distance(wall,f.Position-Vector3.up*3);
                    if(gap<26||gap>60)continue;
                    var proposed=new Site{progress=targets[i]+delta,side=sides[i],road=f.Position,wall=wall,normal=normal,distance=gap};
                    if(CanPlace(proposed,out _,out _)){site=proposed;break;}
                }
                if(site==null)throw new InvalidDataException("No clear facade site for terrace "+i+". Inspect survey before changing layout.");
                BuildTerrace(site,i);report.sites.Add(site);
            }
            foreach(var f in district.GetComponentsInChildren<MeshFilter>()) {
                if(!f.sharedMesh)throw new InvalidDataException("Missing integrated mesh");
                var r=f.GetComponent<Renderer>();if(r&&r.sharedMaterials.Any(m=>!m))throw new InvalidDataException("Missing integrated material");
            }
            if(district.GetComponentsInChildren<Collider>(true).Length!=0)throw new InvalidDataException("Art must not add racing colliders");
            report.prefabs=placed;report.collidersUnchanged=colliderState==ColliderState(world);report.billboardsUnchanged=displayState==DisplayState();
            if(!report.collidersUnchanged||!report.billboardsUnchanged)throw new InvalidDataException("P4 gameplay or displays changed");
            world.artRevision=report.revision;world.ValidateReady();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            report.sourceSceneUnchanged=original==ProductionSceneSetup.Hash(File.ReadAllBytes(BillboardSceneSetup.Candidate));
            if(!report.sourceSceneUnchanged)throw new InvalidDataException("P4 source scene changed");
            File.WriteAllText(output+"/integration.json",JsonUtility.ToJson(report,true));Debug.Log("NIGHT_CITY_INTEGRATION_AUTHORED "+placed+" prefabs");
        }
        static bool CanPlace(Site site,out Vector3 center,out float depth)
        {
            depth=Mathf.Clamp(site.distance-23,6,11);center=site.wall+site.normal*(depth*.5f+.15f);center.y=site.road.y-4.5f;
            var q=Quaternion.LookRotation(site.normal);Vector3 size=new Vector3(15,1,depth);
            var bounds=new Bounds(center,new Vector3(Mathf.Abs(site.normal.z)*15+Mathf.Abs(site.normal.x)*depth,1,Mathf.Abs(site.normal.x)*15+Mathf.Abs(site.normal.z)*depth));
            if(footprints.Any(b=>b.Intersects(bounds)))return false;
            // Both frontage ends must meet the same facade plane, not hang beyond a building corner.
            foreach(float x in new[]{-6.8f,6.8f}) {
                var origin=site.wall+site.normal*.7f+q*Vector3.right*x;
                if(!Hit(origin,-site.normal,siteFacades,out var attachment,out var n)||Vector3.Distance(origin,attachment)>1.0f||Vector3.Dot(n,site.normal)<.95f)return false;
            }
            var occupied=bounds;occupied.center+=Vector3.up*3.5f;occupied.size=new Vector3(bounds.size.x+2,10,bounds.size.z+2);
            foreach(var screen in UnityEngine.Object.FindFirstObjectByType<AnimatedBillboards>().displays)
                if(occupied.Intersects(screen.GetComponent<Renderer>().bounds))return false;
            return ClearRoad(center,q,size);
        }
        static bool ClearRoad(Vector3 center,Quaternion rotation,Vector3 size)
        {
            var inverse=Quaternion.Inverse(rotation);
            for(int k=0;k<2400;k++) {
                var f=track.Evaluate(k/2400f);
                for(int side=-1;side<=1;side++) {
                    var sample=inverse*(f.Position+f.Right*side*(track.Width*.5f+3)-center);
                    if(Mathf.Abs(sample.x)<size.x*.5f+2&&Mathf.Abs(sample.z)<size.z*.5f+2)return false;
                }
            }
            return true;
        }
        static void BuildTerrace(Site site,int index)
        {
            CanPlace(site,out var center,out float depth);var q=Quaternion.LookRotation(site.normal);
            var node=new GameObject("Terrace "+index+" / facade connected").transform;node.SetParent(district,false);node.SetPositionAndRotation(center,q);
            float bottom=-39;float height=center.y-bottom;
            Block(node,"Occupied podium",new Vector3(0,-height*.5f,0),new Vector3(15,height,depth),"cast_concrete");
            Block(node,"Wet roof coping",new Vector3(0,.10f,0),new Vector3(15.6f,.20f,depth+.6f),"corrugated_steel_wet");
            // Source units are metres. The slightly enlarged service architecture reads beside the viaduct.
            float back=-depth*.5f;
            Place(node,"Shopfront",new Vector3(-2.1f,.2f,back+2.1f),1.25f);
            Place(node,"CoolingUnit",new Vector3(4.1f,.2f,back+2.8f),1.15f);
            Place(node,index%2==0?"WaterTank":"RoofExtractor",new Vector3(4.6f,.2f,depth*.5f-2),1.15f);
            Place(node,"DuctBank",new Vector3(-3.5f,4.95f,back+1.8f),.8f);
            Place(node,"PowerCabinet",new Vector3(1.5f,.2f,depth*.5f-1.4f),1);
            if(index==1||index==3)Place(node,"NeonRoofSign",new Vector3(-2.2f,4.95f,back+3.2f),.85f);
            else Place(node,"Sign_square",new Vector3(-5.5f,2.7f,back+4.5f),.75f);
            // Broken parapet runs leave the shop entry and service access readable.
            foreach(float x in new[]{-7.3f,7.3f})Block(node,"Raised side parapet",new Vector3(x,.7f,0),new Vector3(.32f,1.0f,depth+.15f),"painted_graphite");
            Block(node,"Front parapet left",new Vector3(-5.1f,.7f,depth*.5f),new Vector3(4.2f,1,.3f),"painted_graphite");
            Block(node,"Front parapet right",new Vector3(5.1f,.7f,depth*.5f),new Vector3(4.2f,1,.3f),"painted_graphite");
            // A warm practical belongs to the doorway; the original world lighting is untouched.
            var lamp=new GameObject("Shop doorway practical");lamp.transform.SetParent(node,false);lamp.transform.localPosition=new Vector3(-2,3.5f,back+4.2f);
            var l=lamp.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.57f,.29f);l.intensity=18;l.range=8;l.shadows=LightShadows.None;
            footprints.Add(new Bounds(center,new Vector3(Mathf.Abs(site.normal.z)*15+Mathf.Abs(site.normal.x)*depth,1,Mathf.Abs(site.normal.x)*15+Mathf.Abs(site.normal.z)*depth)));
        }
        static void Place(Transform parent,string id,Vector3 position,float scale)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Prefabs/NC_"+id+".prefab");if(!prefab)throw new FileNotFoundException(id);
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=Vector3.one*scale;
            foreach(var r in g.GetComponentsInChildren<Renderer>()) {
                var slots=r.sharedMaterials;
                for(int k=0;k<slots.Length;k++){var wet=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/"+slots[k].name+"_wet.mat");if(wet)slots[k]=wet;}
                r.sharedMaterials=slots;
            }
            placed++;
        }
        static void Block(Transform parent,string label,Vector3 position,Vector3 size,string material)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=label;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(parent,false);g.transform.localPosition=position;
            var mesh=UnityEngine.Object.Instantiate(g.GetComponent<MeshFilter>().sharedMesh);var vertices=mesh.vertices;var uv=mesh.uv;var normals=mesh.normals;
            for(int i=0;i<vertices.Length;i++){vertices[i]=Vector3.Scale(vertices[i],size);var v=vertices[i];uv[i]=Mathf.Abs(normals[i].y)>.5f?new Vector2(v.x,v.z)/4:Mathf.Abs(normals[i].x)>.5f?new Vector2(v.z,v.y)/4:new Vector2(v.x,v.y)/4;}
            mesh.vertices=vertices;mesh.uv=uv;mesh.RecalculateBounds();string path=Root+"/"+parent.name.Split('/')[0].Trim().Replace(" ","-")+"-"+label.Replace(" ","-")+"-"+parent.childCount+".asset";
            var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
            g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/"+material+".mat");
        }
        static string ColliderState(ProductionWorld world)=>string.Join("\n",world.GetComponentsInChildren<Collider>(true).Select(c=>c.name+"|"+c.GetType().Name+"|"+c.transform.localToWorldMatrix+"|"+(c is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):"")).OrderBy(x=>x));
        static string DisplayState()=>string.Join("\n",UnityEngine.Object.FindFirstObjectByType<AnimatedBillboards>().displays.Select(t=>t.name+"|"+t.localToWorldMatrix+"|"+AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial)));
    }
}
