using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    public sealed class AuthoredCity : MonoBehaviour
    {
        // World-space quay centres at waterline, populated after accepted placement.
        // Landward is away from the race-facing promenade; cliffs can meet that edge.
        public Vector3[] HarborCenters { get; private set; } = new Vector3[0];
        public Vector3[] HarborLandwardDirections { get; private set; } = new Vector3[0];
        readonly List<Mesh> meshes = new List<Mesh>();

        public bool Build(WorldBuilder world, TrackPath track)
        {
            var terrace = Resources.Load<GameObject>("Art/Environment/Solstice_TerraceTower_A");
            var split = Resources.Load<GameObject>("Art/Environment/Solstice_SplitTower_B");
            if (!terrace || !split) return false;
            var vegetation = world.MakeMaterial("Terrace olive planting", new Color(.16f,.24f,.085f), .12f);
            // Preserve the reviewed lighter, low-metal coastal glass correction.
            var glazing = world.MakeMaterial("Recessed coastal glazing", new Color(.12f,.23f,.285f), .78f,.06f);
            var concrete = world.MakeMaterial("Harbor marine concrete", new Color(.39f,.43f,.40f), .26f);
            var paving = world.MakeMaterial("Harbor limestone paving", new Color(.65f,.66f,.57f), .28f);
            var wetStone = world.MakeMaterial("Quay tidal masonry", new Color(.19f,.26f,.25f), .38f);
            var centers = new List<Vector3>();
            var directions = new List<Vector3>();
            var samples = new Vector3[600];
            for (int i=0;i<samples.Length;i++) samples[i] = track.Evaluate((float)i/samples.Length).Position;
            float[] anchors = { .215f, .575f, .825f };
            float[][] scales = { new[] {1.02f,.84f,1.12f}, new[] {.94f,1.06f}, new[] {.88f,1.04f,.8f} };
            for (int district=0;district<anchors.Length;district++)
            {
                var frame = track.Evaluate(anchors[district]);
                Vector3 front = Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
                Quaternion rotation = Quaternion.LookRotation(front,Vector3.up);
                Vector3 side = rotation*Vector3.right;
                Vector3 center = Vector3.zero;
                bool placed = false;
                // Test the complete quay + seaward jetty, not just building origins.
                for (int attempt=0;attempt<30&&!placed;attempt++)
                {
                    float inward = 120f + (attempt/5)*12f;
                    float along = new[] {0f,-24f,24f,-48f,48f}[attempt%5];
                    center = frame.Position-front*inward+side*along; center.y=0;
                    if (!ClearOfCourse(center,rotation,new Vector2(68,68),samples,track.Width*.5f+19f)) continue;
                    if (!ClearOfCourse(center+front*83f-side*35f,rotation,new Vector2(20,25),samples,track.Width*.5f+12f)) continue;
                    bool overlaps = false;
                    foreach (var other in centers) if ((other-center).sqrMagnitude<165f*165f) overlaps=true;
                    if (!overlaps) placed=true;
                }
                if (!placed) continue;
                centers.Add(center); directions.Add(-front);
                string name = new[] {"EAST QUAY", "NORTH MARINA", "WEST LANDING"}[district];
                BuildQuay(world,name,center,rotation,concrete,paving,wetStone,vegetation);
                Vector3[] positions = scales[district].Length==3
                    ? new[] {new Vector3(-28,4.15f,-18),new Vector3(28,4.15f,-18),new Vector3(0,4.15f,29)}
                    : new[] {new Vector3(-28,4.15f,-10),new Vector3(28,4.15f,9)};
                for (int i=0;i<positions.Length;i++)
                {
                    var building=Instantiate((i+district)%2==0?terrace:split,transform);
                    building.name=name+" / "+((i+district)%2==0?"Terrace tower":"Split tower");
                    building.transform.SetPositionAndRotation(center+rotation*positions[i],rotation);
                    building.transform.localScale=Vector3.one*scales[district][i];
                    foreach(var renderer in building.GetComponentsInChildren<Renderer>())
                    {
                        var materials=renderer.sharedMaterials;
                        for(int m=0;m<materials.Length;m++)
                        {
                            string n=materials[m]?materials[m].name:"Ivory";
                            materials[m]=n.Contains("Vegetation")?vegetation:n.Contains("Glass")?glazing:n.Contains("Graphite")?world.Graphite:n.Contains("Metal")?world.Metal:world.Ivory;
                        }
                        renderer.sharedMaterials=materials;
                    }
                }
            }
            HarborCenters=centers.ToArray(); HarborLandwardDirections=directions.ToArray();
            return centers.Count>0;
        }

        void BuildQuay(WorldBuilder world,string name,Vector3 c,Quaternion q,Material concrete,Material paving,Material wet,Material vegetation)
        {
            // Chamfered, deep reclaimed quay with a tidal plinth and occupied upper deck.
            QuayPrism(name+" / submerged caisson",c,q,128,124,10,-9,1.2f,wet);
            QuayPrism(name+" / seawall mass",c,q,124,120,9,.4f,3.8f,concrete);
            QuayPrism(name+" / continuous pedestrian deck",c,q,120,116,8,3.8f,4.15f,paving);
            // Waterside walkway is continuous across all buildings and around corners.
            Box(world,name+" / front promenade",c,q,new Vector3(0,4.25f,53),new Vector3(106,.2f,8),world.Ivory);
            Box(world,name+" / shared service lane",c,q,new Vector3(0,4.22f,-46),new Vector3(104,.15f,7),world.Graphite);
            Box(world,name+" / west walkway",c,q,new Vector3(-53,4.24f,0),new Vector3(6,.18f,96),world.Ivory);
            Box(world,name+" / east walkway",c,q,new Vector3(53,4.24f,0),new Vector3(6,.18f,96),world.Ivory);
            for(int side=-1;side<=1;side+=2)
            {
                Box(world,name+" / waterfront parapet",c,q,new Vector3(side*37,4.9f,58),new Vector3(32,1.5f,.8f),concrete);
                Box(world,name+" / side seawall coping",c,q,new Vector3(side*60,4.6f,0),new Vector3(.85f,1.2f,96),concrete);
                // Low inhabited service wings connect the tower district to its quay.
                Box(world,name+" / service arcade",c,q,new Vector3(side*36,6.7f,-51),new Vector3(25,5,8),concrete);
                Box(world,name+" / arcade overhang",c,q,new Vector3(side*36,9.45f,-49),new Vector3(28,.45f,12),world.Ivory);
                for(int bay=0;bay<5;bay++)
                    Box(world,name+" / recessed service bay",c,q,new Vector3(side*36-10+bay*5,6.4f,-46.9f),new Vector3(3.7f,3.5f,.22f),world.Graphite);
                for(int i=0;i<4;i++)
                {
                    float z=-30+i*21;
                    Box(world,name+" / seawall buttress",c,q,new Vector3(side*62,1.4f,z),new Vector3(3.5f,5,2.2f),concrete);
                    Box(world,name+" / planted promenade bed",c,q,new Vector3(side*46,4.7f,z+3),new Vector3(4,1.1f,10),concrete);
                    Box(world,name+" / salt tolerant planting",c,q,new Vector3(side*46,5.45f,z+3),new Vector3(3.2f,.5f,9),vegetation);
                }
            }
            // A service jetty grows out of the promenade, with piles and a return arm.
            Box(world,name+" / jetty deck",c,q,new Vector3(-46,2.5f,82),new Vector3(9,1.1f,48),concrete);
            Box(world,name+" / jetty return",c,q,new Vector3(-35,2.5f,103),new Vector3(30,1.1f,7),paving);
            for(int i=0;i<4;i++)
            {
                Box(world,name+" / jetty pile",c,q,new Vector3(-49,-.8f,65+i*12),new Vector3(1.6f,7,1.6f),wet);
                Box(world,name+" / dock fender",c,q,new Vector3(-40.8f,1.1f,65+i*12),new Vector3(.7f,2.2f,2.5f),world.Graphite);
                Box(world,name+" / mooring bollard",c,q,new Vector3(-43,3.5f,65+i*12),new Vector3(.7f,1,.7f),world.Metal);
            }
            // Broad steps connect the upper promenade to the lower service landing.
            for(int step=0;step<4;step++)
                Box(world,name+" / landing stair",c,q,new Vector3(-46,3.8f-step*.4f,55+step*1.5f),new Vector3(9,.5f,1.6f),paving);
            for(int i=0;i<9;i++)
                Box(world,name+" / quay expansion joint",c,q,new Vector3(-48+i*12,4.37f,53),new Vector3(.09f,.015f,8),wet);
        }

        void Box(WorldBuilder world,string name,Vector3 center,Quaternion rotation,Vector3 local,Vector3 size,Material material)
        {
            var obj=world.Box(name,center+rotation*local,size,rotation,material);
            obj.transform.SetParent(transform,true);
        }

        void QuayPrism(string name,Vector3 center,Quaternion rotation,float width,float depth,float bevel,float bottom,float top,Material material)
        {
            float x=width*.5f,z=depth*.5f;
            Vector2[] ring={new Vector2(-x+bevel,-z),new Vector2(x-bevel,-z),new Vector2(x,-z+bevel),new Vector2(x,z-bevel),new Vector2(x-bevel,z),new Vector2(-x+bevel,z),new Vector2(-x,z-bevel),new Vector2(-x,-z+bevel)};
            var vertices=new List<Vector3>();var triangles=new List<int>();
            // Independent side vertices keep the civil-engineering edges sharp.
            for(int i=0;i<ring.Length;i++)
            {
                Vector2 a=ring[i],b=ring[(i+1)%ring.Length];int v=vertices.Count;
                vertices.Add(new Vector3(a.x,bottom,a.y));vertices.Add(new Vector3(a.x,top,a.y));
                vertices.Add(new Vector3(b.x,top,b.y));vertices.Add(new Vector3(b.x,bottom,b.y));
                triangles.AddRange(new[]{v,v+1,v+2,v,v+2,v+3});
            }
            int cap=vertices.Count;vertices.Add(new Vector3(0,top,0));
            foreach(var p in ring)vertices.Add(new Vector3(p.x,top,p.y));
            for(int i=0;i<ring.Length;i++)triangles.AddRange(new[]{cap,cap+1+(i+1)%ring.Length,cap+1+i});
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var obj=new GameObject(name);obj.transform.SetParent(transform,false);obj.transform.SetPositionAndRotation(center,rotation);
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;
        }

        static bool ClearOfCourse(Vector3 center,Quaternion rotation,Vector2 halfSize,Vector3[] samples,float clearance)
        {
            Quaternion inverse=Quaternion.Inverse(rotation);
            foreach(var sample in samples)
            {
                Vector3 p=inverse*(sample-center);
                float dx=Mathf.Max(0,Mathf.Abs(p.x)-halfSize.x),dz=Mathf.Max(0,Mathf.Abs(p.z)-halfSize.y);
                if(dx*dx+dz*dz<clearance*clearance)return false;
            }
            return true;
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);}
    }
}
