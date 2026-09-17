using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorRush.Editor
{
    // Additive authoring only: never reconstructs the route or changes the finished HUD.
    public static class UndergroundGalleryFinish
    {
        public const string RootName = "Finish / inhabited city terraces";
        public const string AssetRoot = "Assets/Art/UndergroundGalleryFinish";
        public const string Revision = "underground-gallery-finish-03";
        static readonly Dictionary<Material, ProductionGeometry> batches = new();
        static readonly Dictionary<Material, List<Vector3>> foliageNormals = new();
        static TrackPath track, original;
        static Transform root;
        static Material concrete, trim, recess, metal, glass, warm, leaf, leafLight, earth;
        static int terraces, plants, podiums, lamps;

        public static void Apply()
        {
            string output = ProductionSceneSetup.RequiredFlag("-productionEvidence");
            if (Directory.Exists(output) && Directory.GetFileSystemEntries(output).Length > 0)
                throw new IOException("Use fresh authoring evidence");
            Directory.CreateDirectory(output);
            string baseline = Hash(EnvironmentStructureSetup.Candidate);
            var scene = EditorSceneManager.OpenScene(UndergroundGallerySetup.Candidate, OpenSceneMode.Single);
            var world = UnityEngine.Object.FindFirstObjectByType<ProductionWorld>();
            string course = ProductionSceneSetup.CourseHash(world.track);
            if (!world.track.UndergroundGallery || course != "e5b995eaedbda504165e40147acfbd697321b38a249fd1f08f770642b0a2dc4f")
                throw new InvalidDataException("Expected the published underground course");
            var enclosure = world.transform.Cast<Transform>().Single(t => t.name.StartsWith("Underground gallery /"));
            var prior = enclosure.Cast<Transform>().SingleOrDefault(t => t.name == RootName);
            if (prior) UnityEngine.Object.DestroyImmediate(prior.gameObject);
            // Fingerprint only preserved racing meshes, not the removable finish layer.
            string roads = RoadFingerprint(world);
            root = new GameObject(RootName).transform; root.SetParent(enclosure, false);
            track = world.track;
            var helper = new GameObject("Temporary reference parameterization"); original = helper.AddComponent<TrackPath>();
            batches.Clear(); foliageNormals.Clear(); terraces = plants = podiums = lamps = 0;
            Directory.CreateDirectory(AssetRoot); AssetDatabase.Refresh();
            try
            {
                Materials();
                BuildTerraces();
                BuildGallery();
                Flush();
                UnityEngine.Object.DestroyImmediate(helper);
                if (RoadFingerprint(world) != roads || ProductionSceneSetup.CourseHash(track) != course)
                    throw new InvalidDataException("Finish changed racing geometry");
                ValidateCorridor(world);
                world.artRevision = Revision;
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
                if (Hash(EnvironmentStructureSetup.Candidate) != baseline) throw new InvalidDataException("Baseline changed");
                File.WriteAllText(Path.Combine(output, "finish-validation.txt"),
                    $"revision={Revision}\ncourseUnchanged={course}\nbaselineUnchanged={baseline}\nroadMeshFingerprint={roads}\nterraceSpans={terraces}\nplantClusters={plants}\npodiumBays={podiums}\naddedLights={lamps}\nmaterialBatches={batches.Count}\nfinishCorridorChecks=7200\n");
            }
            finally { if (helper) UnityEngine.Object.DestroyImmediate(helper); }
        }

        static TrackFrame Frame(float p) => track.EvaluateParameter(original.ParameterAtProgress(p));
        static Vector3 Right(TrackFrame f) => Vector3.Cross(Vector3.up, f.Forward).normalized;
        static Vector3 At(TrackFrame f, float x, float y) => f.Position + Right(f) * x + Vector3.up * y;
        static Quaternion Rotation(TrackFrame f) => Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward, Vector3.up), Vector3.up);
        static float Height(float p)
        {
            if (p >= .791f && p <= .884f) return 21;
            return Mathf.Lerp(1,13, Mathf.Min(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.681f,.756f,p)),
                1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.895f,.972f,p))));
        }
        static float City(float p, TrackFrame f) => Mathf.Max(f.Position.y+Height(p)+2,
            Mathf.Lerp(f.Position.y+2,59,Mathf.Min(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.681f,.756f,p)),
                1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.895f,.972f,p)))))-f.Position.y;

        static void BuildTerraces()
        {
            int span = 0;
            for (float p=.708f; p<.958f; p+=.006f,span++)
            {
                if (p>=.785f && p<.89f) continue;
                float end=Mathf.Min(p+.00585f,.96f);
                var a=Frame(p);var b=Frame(end);var f=Frame((p+end)*.5f);var q=Rotation(f);
                float h=Height(p),hB=Height(end),city=City(p,a),cityB=City(end,b);
                float localHeight=(a.Position.y+h+b.Position.y+hB)*.5f-f.Position.y;
                float localCity=(a.Position.y+city+b.Position.y+cityB)*.5f-f.Position.y;
                float length=Vector3.Distance(a.Position,b.Position);
                foreach(int side in new[]{-1,1})
                {
                    terraces++;
                    // Slab, coping, handrails and reveals define a usable terrace above the road.
                    Strip(a,b,side*18.0f,side*22.0f,h+.46f,hB+.46f,h+.7f,hB+.7f,trim);
                    Strip(a,b,side*18.12f,side*18.26f,h+1.8f,hB+1.8f,h+1.87f,hB+1.87f,metal);
                    Strip(a,b,side*18.12f,side*18.22f,h+1.22f,hB+1.22f,h+1.28f,hB+1.28f,metal);
                    for(int j=0;j<4;j++)
                    {
                        var rail=Frame(Mathf.Lerp(p,end,j/3f));float y=Mathf.Lerp(a.Position.y+h,b.Position.y+hB,j/3f)-rail.Position.y;
                        Box(At(rail,side*18.2f,y+1.26f),new Vector3(.08f,1.2f,.08f),Rotation(rail),metal);
                    }
                    // Upper foundation: real relief joints and vertical buttresses, not paint alone.
                    float upper=localCity-localHeight;
                    if(upper>3)
                    {
                        for(float y=localHeight+3;y<localCity-1;y+=3.8f)
                            Box(At(f,side*21.82f,y),new Vector3(.16f,.065f,length*.97f),q,recess);
                        Box(At(f,side*21.72f,localHeight+upper*.5f),new Vector3(.42f,upper,.4f),q,trim);
                        // Deep framed ventilation / access bay occupies the upper face.
                        if(upper>6 && span%2==0)
                        {
                            float y=localHeight+Mathf.Min(upper*.5f,6.5f);
                            Box(At(f,side*21.7f,y),new Vector3(.28f,3.8f,5.7f),q,recess);
                            for(int j=0;j<9;j++)Box(At(f,side*21.47f,y-1.55f+j*.38f),new Vector3(.3f,.1f,5.25f),q,metal);
                            Box(At(f,side*21.5f,y+2.05f),new Vector3(.65f,.32f,6.0f),q,trim);
                            Box(At(f,side*21.12f,y+1.85f),new Vector3(.12f,.1f,4.8f),q,warm);
                        }
                    }
                    // Alternating planted bays leave a continuous walking route behind the railing.
                    if(span%3!=1)
                    {
                        Box(At(f,side*20.0f,localHeight+1.05f),new Vector3(1.7f,.7f,6.7f),q,concrete);
                        Box(At(f,side*20.0f,localHeight+1.42f),new Vector3(1.45f,.08f,6.35f),q,earth);
                        for(int j=0;j<5;j++)
                        {
                            var center=At(f,side*20.0f,localHeight+1.85f)+q*Vector3.forward*(j-2)*1.1f;
                            Shrub(center,new Vector3(.9f,.55f+Noise(span*17+j)*.45f,.85f),span*79+j+side*3);
                        }
                    }
                    // City-facing podium front physically rests on the existing upper platform.
                    if(span%3!=2 && upper>4 && p<.936f)
                    {
                        podiums++;
                        float tall=span%4==0?10:7;
                        Box(At(f,side*28.8f,localCity+tall*.5f+.5f),new Vector3(10,tall,length*.9f),q,glass);
                        Box(At(f,side*23.7f,localCity+tall*.5f+.5f),new Vector3(.25f,tall-.8f,length*.81f),q,glass);
                        for(int row=0;row<3;row++)
                        {
                            float y=localCity+1.1f+row*2.65f;if(y>localCity+tall)continue;
                            Box(At(f,side*28.7f,y),new Vector3(10.6f,.22f,length*.92f),q,trim);
                        }
                        for(int col=0;col<5;col++)
                        {
                            var at=At(f,side*23.42f,localCity+tall*.5f+.5f)+q*Vector3.forward*((col-2)*length*.2f);
                            Box(at,new Vector3(.3f,tall,.18f),q,metal);
                        }
                        Box(At(f,side*27.9f,localCity+tall+.68f),new Vector3(11.5f,.42f,length*.94f),q,trim);
                        Box(At(f,side*23.15f,localCity+.75f),new Vector3(2,.45f,length*.93f),q,trim);
                    }
                    if(span%3==0 && upper>4)
                    {
                        var pos=At(f,side*18.9f,localHeight+1.1f);
                        Box(pos,new Vector3(.6f,.12f,1.0f),q,warm);
                        Light("Terrace wall wash",pos+Vector3.up*.2f,At(f,side*22,localHeight+upper*.6f),360,26,95);
                    }
                    // Lower wall joints and columns remain clear of the physical shoulder.
                    for(float y=3.5f;y<localHeight-.5f;y+=3.4f)
                        Box(At(f,side*17.49f,y),new Vector3(.15f,.07f,length*.95f),q,recess);
                    Box(At(f,side*17.35f,localHeight*.5f),new Vector3(.3f,localHeight,.34f),q,trim);
                }
            }
        }

        static void BuildGallery()
        {
            for(float p=.792f;p<.883f;p+=.003f)
            {
                var a=Frame(p);var b=Frame(Mathf.Min(.884f,p+.003f));
                foreach(int side in new[]{-1,1})
                {
                    Strip(a,b,side*12.45f,side*13.2f,16.55f,16.55f,16.9f,16.9f,metal);
                    Strip(a,b,side*12.72f,side*12.94f,16.46f,16.46f,16.54f,16.54f,warm);
                    Strip(a,b,side*16.85f,side*17.1f,12.7f,12.7f,13.1f,13.1f,metal);
                }
            }
            foreach(float p in new[]{.791f,.884f})
            {
                var f=Frame(p);var q=Rotation(f);
                // Layered soffit and reveal frame follow the existing portal opening.
                foreach(int side in new[]{-1,1})
                {
                    Box(At(f,side*18.1f,8.5f),new Vector3(.45f,16.6f,7.5f),q,metal);
                    Box(At(f,side*18.5f,8.5f),new Vector3(.6f,17,8),q,concrete);
                }
                Box(At(f,0,18.15f),new Vector3(35,.38f,8),q,metal);
            }
        }

        static void Materials()
        {
            concrete=Mat("Terrace limestone",new Color(.37f,.38f,.36f),.27f);
            trim=Mat("Coping and reveals",new Color(.21f,.24f,.25f),.35f);
            recess=Mat("Recess depth",new Color(.016f,.024f,.027f),.16f);
            metal=Mat("Dark bronze metal",new Color(.13f,.17f,.18f),.52f);metal.SetFloat("_Metallic",.65f);
            glass=Save(new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/World/Stage1/city-s1/Materials/vrx_mat_officeglass_a.mat")){name="Podium glazing"},"Podium glazing.mat");
            glass.SetColor("_BaseColor",new Color(.65f,.72f,.73f));
            glass.SetTextureScale("_BaseMap",new Vector2(1.8f,1.8f));
            glass.SetTextureScale("_EmissionMap",new Vector2(1.8f,1.8f));
            glass.SetColor("_EmissionColor",new Color(.28f,.20f,.12f));
            warm=Mat("Occupied warm windows",new Color(.8f,.58f,.3f),.42f,new Color(1.35f,.79f,.3f));
            leaf=Mat("Terrace foliage shadow",new Color(.13f,.23f,.07f),.15f,new Color(.009f,.017f,.003f));
            leafLight=Mat("Terrace foliage tips",new Color(.22f,.32f,.095f),.2f,new Color(.016f,.029f,.005f));
            earth=Mat("Planter soil",new Color(.038f,.035f,.025f),.12f);
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UndergroundGalleryStage2/CastConcrete.asset");
            concrete.SetTexture("_BaseMap",texture);trim.SetTexture("_BaseMap",texture);
            // Candidate-only foundation gains surface relief. The baseline material is not referenced here.
            var foundation=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/UndergroundGalleryStage2/Foundation-mineral concrete.mat");
            var relief=ReliefTexture();
            foundation.SetTexture("_BumpMap",relief);foundation.SetFloat("_BumpScale",.5f);foundation.EnableKeyword("_NORMALMAP");
            foundation.SetColor("_BaseColor",new Color(.34f,.355f,.36f));
            EditorUtility.SetDirty(foundation);
            concrete.SetTexture("_BumpMap",relief);concrete.EnableKeyword("_NORMALMAP");concrete.SetFloat("_BumpScale",.45f);
            foreach(var m in new[]{concrete,trim,recess,metal,glass,warm,leaf,leafLight,earth})EditorUtility.SetDirty(m);
        }
        static Texture2D ReliefTexture()
        {
            const int n=256; var t=new Texture2D(n,n,TextureFormat.RGBA32,false,true);
            float Value(int x,int y)
            {
                x=(x+n)%n;y=(y+n)%n;
                return (x<2||y<2||Mathf.Abs(y-n/2)<1 ? .1f:.55f)+Mathf.PerlinNoise(x*.27f,y*.27f)*.06f;
            }
            var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                var v=new Vector3((Value(x-1,y)-Value(x+1,y))*1.7f,(Value(x,y-1)-Value(x,y+1))*1.7f,1).normalized;
                pixels[y*n+x]=new Color(v.x*.5f+.5f,v.y*.5f+.5f,v.z*.5f+.5f,1);
            }
            t.SetPixels(pixels);t.Apply();string path=AssetRoot+"/CastPanelNormal.png";
            File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.NormalMap;
            importer.sRGBTexture=false;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static float Noise(int seed)=>Mathf.Repeat(Mathf.Sin(seed*127.1f+311.7f)*43758.5453f,1);
        static void Shrub(Vector3 center,Vector3 size,int seed)
        {
            plants++;
            for(int lobe=0;lobe<12;lobe++)
            {
                var c=center+new Vector3((Noise(seed+lobe*17)-.5f)*size.x*1.6f,(Noise(seed+lobe*13)-.5f)*size.y,(Noise(seed+lobe*7)-.5f)*size.z*1.6f);
                Vector3 Point(int ring,int col)
                {
                    float phi=ring*Mathf.PI/6,theta=col*Mathf.PI*2/9;
                    float r=.88f+Noise(seed+lobe*31+ring*9+(col%9))*.12f;
                    return c+Vector3.Scale(new Vector3(Mathf.Sin(phi)*Mathf.Cos(theta),Mathf.Cos(phi),Mathf.Sin(phi)*Mathf.Sin(theta)),size)*r*.47f;
                }
                for(int ring=0;ring<6;ring++)for(int col=0;col<9;col++)
                {
                    var m=(lobe%3==0)?leafLight:leaf;
                    var points=new[]{Point(ring,col),Point(ring,col+1),Point(ring+1,col+1),Point(ring+1,col)};
                    Geo(m).Quad(points[0],points[1],points[2],points[3]);
                    if(!foliageNormals.TryGetValue(m,out var normals))foliageNormals[m]=normals=new List<Vector3>();
                    foreach(var point in points)
                    {var d=point-c;normals.Add(new Vector3(d.x/(size.x*size.x),d.y/(size.y*size.y),d.z/(size.z*size.z)).normalized);}
                }
            }
        }
        static void Light(string name,Vector3 from,Vector3 to,float intensity,float range,float angle)
        {
            var go=new GameObject(name+" "+lamps++);go.transform.SetParent(root,false);go.transform.position=from;go.transform.rotation=Quaternion.LookRotation(to-from);
            var l=go.AddComponent<Light>();l.type=LightType.Spot;l.color=new Color(1,.76f,.49f);l.intensity=intensity;l.range=range;l.spotAngle=angle;l.innerSpotAngle=angle*.5f;l.shadows=LightShadows.None;
        }
        static void Strip(TrackFrame a,TrackFrame b,float left,float right,float loA,float loB,float hiA,float hiB,Material m)
        {
            if(left>right)(left,right)=(right,left);
            var l0=At(a,left,loA);var r0=At(a,right,loA);var l1=At(b,left,loB);var r1=At(b,right,loB);
            var l2=At(a,left,hiA);var r2=At(a,right,hiA);var l3=At(b,left,hiB);var r3=At(b,right,hiB);var g=Geo(m);
            g.Quad(l2,l3,r3,r2);g.Quad(l0,r0,r1,l1);g.Quad(l0,l1,l3,l2);g.Quad(r0,r2,r3,r1);g.Quad(l0,l2,r2,r0);g.Quad(l1,r1,r3,l3);
        }
        static ProductionGeometry Geo(Material m){if(!batches.TryGetValue(m,out var g))batches[m]=g=new ProductionGeometry();return g;}
        static void Box(Vector3 center,Vector3 size,Quaternion rotation,Material m)=>Geo(m).Box(center,size,rotation);
        static Material Mat(string name,Color color,float smooth,Color emission=default)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,enableInstancing=true};m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);
            if(emission.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;}
            return Save(m,name+".mat");
        }
        static T Save<T>(T asset,string name) where T:UnityEngine.Object
        {
            string path=AssetRoot+"/"+name;var prior=AssetDatabase.LoadAssetAtPath<T>(path);
            if(prior){EditorUtility.CopySerialized(asset,prior);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(prior);return prior;}
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        static void Flush()
        {
            foreach(var pair in batches)
            {
                var go=new GameObject(pair.Key.name);go.transform.SetParent(root,false);go.isStatic=true;
                var generated=pair.Value.Mesh(pair.Key.name);
                if(foliageNormals.TryGetValue(pair.Key,out var normals)){generated.SetNormals(normals);generated.RecalculateTangents();}
                var mesh=Save(generated,pair.Key.name+".asset");go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
                // Non-solid foliage doesn't participate in camera/road collision checks.
                if(pair.Key!=leaf && pair.Key!=leafLight && pair.Key!=earth)go.AddComponent<MeshCollider>().sharedMesh=mesh;
            }
        }
        public static string RoadFingerprint(ProductionWorld world)
        {
            var text=new System.Text.StringBuilder();
            foreach(var c in world.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.Contains("authoritative collision")).OrderBy(c=>c.name))
            {string p=AssetDatabase.GetAssetPath(c.sharedMesh);text.Append(c.name).Append(':').Append(AssetDatabase.AssetPathToGUID(p)).Append(':').Append(Hash(p)).AppendLine();}
            return text.ToString();
        }
        public static void ValidateCorridor(ProductionWorld world)
        {
            var layer=world.transform.GetComponentsInChildren<Transform>().Single(t=>t.name==RootName);
            Physics.SyncTransforms();var colliders=layer.GetComponentsInChildren<MeshCollider>();
            for(int i=0;i<2400;i++)
            {
                var f=world.track.Evaluate(i/2400f);
                foreach(float x in new[]{-9f,0f,9f})
                    foreach(var c in colliders)
                        if(c.Raycast(new Ray(f.Position+f.Right*x+f.Up*10,-f.Up),out _,9))throw new InvalidDataException("Finish entered driving corridor at "+i);
            }
        }
        static string Hash(string path)=>BitConverter.ToString(SHA256.Create().ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();
    }
}
