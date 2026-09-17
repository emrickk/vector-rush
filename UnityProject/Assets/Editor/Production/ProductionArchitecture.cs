using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VectorRush.Editor
{
    // Deterministic source recipe. Every batch is an editable saved mesh in the production scene.
    public sealed class ProductionArchitecture
    {
        readonly TrackPath track;readonly Transform root;
        readonly Dictionary<string,ProductionGeometry> batches=new Dictionary<string,ProductionGeometry>();
        readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        Transform zone;int meshId;
        readonly Color amber=new Color(1f,.48f,.14f),cool=new Color(.2f,.7f,1f);
        public ProductionArchitecture(TrackPath track,Transform root){this.track=track;this.root=root;}
        public Material Material(string name,Color color,float smooth=.45f,float metallic=0,Color? emission=null)
        {
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",smooth);material.SetFloat("_Metallic",metallic);
            if(emission.HasValue){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",emission.Value);}
            AssetDatabase.CreateAsset(material,ProductionSceneSetup.Root+"/Materials/"+name+".mat");return material;
        }
        public void Build()
        {
            materials["structure"]=Material("Blue steel",new Color(.055f,.09f,.125f),.46f,.65f);
            materials["concrete"]=Material("Pale cast concrete",new Color(.32f,.38f,.43f),.28f);
            materials["ceramic"]=Material("Porcelain panels",new Color(.58f,.65f,.67f),.5f,.15f);
            materials["metal"]=Material("Titanium trim",new Color(.22f,.29f,.34f),.62f,.8f);
            materials["glass"]=Material("Recessed blue glazing",new Color(.015f,.065f,.095f),.83f,.45f,new Color(.015f,.035f,.045f));
            materials["warm"]=Material("Occupied amber interiors",new Color(.6f,.29f,.085f),.35f,0,amber*.75f);
            materials["cool"]=Material("Cool service interiors",new Color(.12f,.3f,.42f),.4f,0,cool*.45f);
            materials["light"]=Material("Cyan diffusers",cool,.35f,0,cool*2.5f);
            materials["amber"]=Material("Amber diffusers",amber,.35f,0,amber*2.5f);
            materials["dark"]=Material("Deep recess",new Color(.015f,.022f,.03f),.25f);
            materials["road"]=Material("Directional satin road",new Color(.16f,.185f,.21f),.43f,.08f);
            CreateRoadTexture(materials["road"]);
            NewZone("00 / continuous track construction");
            SaveMesh("Running surface",ProductionGeometry.Ribbon(track,-11,11,0),materials["road"],true);
            SaveMesh("Track underbody",ProductionGeometry.Ribbon(track,-13.3f,13.3f,-1.7f,true),materials["structure"]);
            for(int side=-1;side<=1;side+=2)
            {
                SaveMesh("Safety barrier",ProductionGeometry.Barrier(track,side),materials["concrete"],true);
                SaveMesh("Shoulder",ProductionGeometry.Ribbon(track,side*11,side*12,-.02f),materials["concrete"],true);
                SaveMesh("Edge datum",ProductionGeometry.Ribbon(track,side*10.2f,side*10.48f,.025f),materials["ceramic"]);
                SaveMesh("Drain",ProductionGeometry.Ribbon(track,side*10.6f,side*10.85f,.03f),materials["metal"]);
                SaveMesh("Barrier cap",ProductionGeometry.Ribbon(track,side*11.78f,side*11.91f,2.03f),materials["light"]);
                SaveMesh("Service ledge",ProductionGeometry.Ribbon(track,side*12.1f,side*13.3f,-.12f),materials["structure"]);
                var fascia=new ProductionGeometry();
                for(int i=0;i<960;i++){var a=track.Evaluate(i/960f);var b=track.Evaluate((i+1)/960f);
                    Vector3 a0=a.Position+a.Right*(13.3f*side)-a.Up*1.7f,a1=a.Position+a.Right*(13.3f*side)-a.Up*.12f;
                    Vector3 b0=b.Position+b.Right*(13.3f*side)-b.Up*1.7f,b1=b.Position+b.Right*(13.3f*side)-b.Up*.12f;
                    if(side>0)fascia.Quad(a0,a1,b1,b0);else fascia.Quad(a0,b0,b1,a1);}
                SaveMesh("Continuous deck fascia",fascia.Mesh("Deck fascia"),materials["ceramic"]);
            }
            for(int i=0;i<112;i++){var f=track.Evaluate(i/112f);var q=Quaternion.LookRotation(f.Forward,f.Up);
                Box("ceramic",f.Position+f.Up*.026f,new Vector3(.14f,.016f,3),q);
                if(i%2==0)Pier(f);
                if(i%4==0)RoadLight(f,i%8==0?1:-1);
            }
            Flush();
            NewZone("01 / inhabited viaduct");
            for(int i=0;i<12;i++){float p=.012f+i*.024f;Facade(p,i%2==0?-1:1,20,18+i%3*6,28,i%3==0);}
            SignalMast(.12f);Flush();
            NewZone("02 / canyon and cool gallery");
            for(int i=0;i<7;i++){Facade(.302f+i*.03f,-1,18,42+i%2*12,33,false);if(i<4)Facade(.30f+i*.035f,1,18,35,29,true);}
            Gallery(.35f,.435f,false);Flush();
            NewZone("03 / thermal works");
            Thermal(.59f);for(int i=0;i<4;i++)Facade(.535f+i*.047f,-1,18,16,26,false);Flush();
            NewZone("04 / station and civic approach");
            Gallery(.79f,.88f,true);Station(.934f);Facade(.73f,1,22,24,42,true);Facade(.975f,-1,18,30,32,true);Flush();
            NewZone("05 / distant city");
            for(int i=0;i<28;i++)
            {
                float angle=i*Mathf.PI*2/28;Vector3 p=new Vector3(Mathf.Cos(angle)*620,0,Mathf.Sin(angle)*620);
                float h=65+(i*37%95);var q=Quaternion.Euler(0,-i*360f/28,0);
                Box("structure",p+Vector3.up*h*.5f,new Vector3(48,h,48),q);
                Box("metal",p+Vector3.up*(h+2),new Vector3(36,4,36),q);
                for(int floor=12;floor<h-6;floor+=12)for(int col=-1;col<=1;col++)if((floor+i+col)%4!=0)
                    Box(i%3==0?"warm":"cool",p+q*new Vector3(col*13,floor,-24.1f),new Vector3(8,2.3f,.12f),q);
            }
            Flush();AssetDatabase.SaveAssets();
        }
        void NewZone(string name){zone=new GameObject(name).transform;zone.SetParent(root,false);}
        ProductionGeometry Batch(string material){if(!batches.TryGetValue(material,out var value)){value=new ProductionGeometry();batches.Add(material,value);}return value;}
        void Box(string material,Vector3 center,Vector3 size,Quaternion rotation)=>Batch(material).Box(center,size,rotation);
        void Beam(string material,Vector3 a,Vector3 b,float width,float depth)=>Batch(material).Beam(a,b,width,depth);
        void Flush(){foreach(var item in batches)SaveMesh(zone.name+" / "+item.Key,item.Value.Mesh(item.Key),materials[item.Key]);batches.Clear();}
        void SaveMesh(string name,Mesh mesh,Material material,bool collider=false)
        {
            AssetDatabase.CreateAsset(mesh,ProductionSceneSetup.Root+"/Meshes/mesh-"+(meshId++).ToString("D3")+".asset");
            var go=new GameObject(name);go.transform.SetParent(zone,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
        void Pier(TrackFrame f)
        {
            var q=Quaternion.LookRotation(f.Forward,Vector3.up);float height=f.Position.y-5;
            Box("concrete",new Vector3(f.Position.x,2,f.Position.z),new Vector3(10,4,10),q);
            Box("structure",new Vector3(f.Position.x,5+height*.5f-3,f.Position.z),new Vector3(4.5f,height-6,5.5f),q);
            Box("metal",f.Position-f.Up*3,new Vector3(24,1.7f,4),Quaternion.LookRotation(f.Forward,f.Up));
            for(int side=-1;side<=1;side+=2)Beam("structure",f.Position-Vector3.up*13,f.Position+f.Right*side*10-f.Up*4,1.2f,1.2f);
        }
        void RoadLight(TrackFrame f,int side)
        {
            var q=Quaternion.LookRotation(f.Forward,f.Up);Vector3 foot=f.Position+f.Right*side*14;
            Box("metal",foot+f.Up*5,new Vector3(.32f,10,.45f),q);
            Box("metal",foot+f.Up*10-f.Right*side*2,new Vector3(4.5f,.3f,.8f),q);
            Vector3 source=foot+f.Up*9.8f-f.Right*side*3.5f;
            Box("light",source,new Vector3(1.5f,.08f,.6f),q);
            Spot("Road pool",source,f.Position,cool,4,28,105);
        }
        void Spot(string name,Vector3 p,Vector3 target,Color color,float intensity,float range,float angle)
        {
            var light=new GameObject(name).AddComponent<Light>();light.transform.SetParent(zone,false);light.transform.position=p;light.transform.rotation=Quaternion.LookRotation(target-p);
            light.type=LightType.Spot;light.color=color;light.intensity=intensity;light.range=range;light.spotAngle=angle;light.innerSpotAngle=angle*.62f;light.shadows=LightShadows.None;
        }
        void Facade(float progress,int side,float depth,float height,float length,bool warm)
        {
            var f=track.Evaluate(progress);var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up),Vector3.up);
            Vector3 origin=f.Position+f.Right*side*(17+depth*.5f)-Vector3.up*7;
            // Check all branches before placing near architecture, not only its nominal anchor.
            if(!Clear(origin,q,new Vector3(depth,height,length)))return;
            Vector3 P(float x,float y,float z)=>origin+q*new Vector3(x,y,z);
            float face=-side*depth*.5f;
            Box("structure",P(0,height*.5f,0),new Vector3(depth,height,length),q);
            Box("concrete",P(0,1,0),new Vector3(depth+1,2,length+1),q);
            Box("ceramic",P(0,height+.6f,0),new Vector3(depth+1.4f,1.2f,length+1.4f),q);
            int bays=Mathf.FloorToInt(length/5),floors=Mathf.FloorToInt((height-3)/5);
            for(int floor=0;floor<floors;floor++)
            {
                float y=4+floor*5;
                Box("ceramic",P(face-side*.45f,y-1.8f,0),new Vector3(.9f,.65f,length),q);
                for(int bay=0;bay<bays;bay++)
                {
                    float z=(bay-(bays-1)*.5f)*5;
                    Box("dark",P(face-side*.08f,y,z),new Vector3(.2f,3.5f,4.6f),q);
                    Box((floor+bay)%5==0?"glass":warm?"warm":"cool",P(face-side*.2f,y,z),new Vector3(.15f,2.6f,3.7f),q);
                    Box("concrete",P(face-side*.65f,y,z-2.3f),new Vector3(1.3f,4.4f,.42f),q);
                    Box("metal",P(face-side*.75f,y+1.6f,z),new Vector3(1.5f,.25f,4.5f),q);
                }
            }
            Box("structure",P(face-side*2,3,0),new Vector3(4,.7f,length+2),q);
            for(int s=-1;s<=1;s+=2)Box("metal",P(face-side*3.6f,1.4f,s*(length*.5f-2)),new Vector3(.32f,2.8f,.32f),q);
            Spot("Facade canopy wash",P(face-side*1.5f,8,0),P(face-side*3,0,0),warm?amber:cool,5,22,100);
        }
        bool Clear(Vector3 origin,Quaternion q,Vector3 size)
        {
            var inverse=Quaternion.Inverse(q);Vector3 center=origin+Vector3.up*size.y*.5f;
            for(int i=0;i<1200;i++){var f=track.Evaluate(i/1200f);var local=inverse*(f.Position+f.Up*4-center);
                if(Mathf.Abs(local.x)<size.x*.5f+13.5f&&Mathf.Abs(local.z)<size.z*.5f+13.5f&&Mathf.Abs(local.y)<size.y*.5f+5)return false;}
            return true;
        }
        void Gallery(float start,float end,bool warm)
        {
            int count=Mathf.CeilToInt((end-start)*track.Length/6);string glow=warm?"amber":"light";
            for(int i=0;i<=count;i++)
            {
                var f=track.Evaluate(Mathf.Lerp(start,end,i/(float)count));var q=Quaternion.LookRotation(f.Forward,f.Up);
                Vector3 P(float x,float y)=>f.Position+f.Right*x+f.Up*y;
                for(int s=-1;s<=1;s+=2)
                {
                    Box("concrete",P(s*15.4f,6.3f),new Vector3(1.5f,15,1.1f),q);
                    Beam("ceramic",P(s*15.4f,10),P(s*11,14),.9f,1.1f);
                    Box("metal",P(s*14,4),new Vector3(.7f,.8f,6.6f),q);
                    Box(glow,P(s*13.7f,7.8f),new Vector3(.18f,.32f,4.2f),q);
                    if(i<count)
                    {
                        Box("structure",P(s*16.4f,6),new Vector3(.65f,14,6.8f),q);
                        Box("ceramic",P(s*16,3),new Vector3(.35f,4.5f,5.1f),q);
                        Box("dark",P(s*15.95f,9),new Vector3(.25f,3.5f,4.8f),q);
                        for(int vent=0;vent<5;vent++)Box("metal",P(s*15.7f,7.8f+vent*.5f),new Vector3(.35f,.12f,4.3f),q);
                    }
                }
                Box("structure",P(0,14.8f),new Vector3(33,1,6.8f),q);
                Box("ceramic",P(0,14.1f),new Vector3(22,.4f,4.8f),q);
                Box("metal",P(0,13.7f),new Vector3(27,.65f,1.1f),q);
                if(i%2==0){Box(glow,P(0,13.3f),new Vector3(8,.15f,.6f),q);Spot("Gallery wash",P(0,13.1f),P(0,0),warm?amber:cool,9,26,115);}
                if(i==0||i==count)
                {
                    Box("concrete",P(0,15.4f),new Vector3(35,2.2f,4),q);
                    for(int s=-1;s<=1;s+=2)Box("concrete",P(s*16.4f,6),new Vector3(3.2f,15,4),q);
                    Box(glow,P(0,14.1f)-f.Forward*2.1f,new Vector3(23,.26f,.15f),q);
                }
            }
        }
        void SignalMast(float p)
        {
            var f=track.Evaluate(p);var q=Quaternion.LookRotation(f.Forward,Vector3.up);Vector3 origin=f.Position+f.Right*42-Vector3.up*12;
            Vector3 P(float x,float y,float z)=>origin+q*new Vector3(x,y,z);
            Box("concrete",P(0,1,0),new Vector3(20,2,20),q);
            for(int side=-1;side<=1;side+=2){Box("structure",P(side*5,28,0),new Vector3(2,56,4),q);
                Box("ceramic",P(side*5,47,0),new Vector3(4,14,5),q);Box("light",P(side*5,48,-2.6f),new Vector3(1.4f,10,.15f),q);}
            for(int y=8;y<42;y+=8){Beam("metal",P(-5,y,0),P(5,y+8,0),.55f,.55f);Box("metal",P(0,y,0),new Vector3(11,.65f,4),q);}
            Box("structure",P(0,29,0),new Vector3(16,6,10),q);Box("warm",P(0,29,-5.05f),new Vector3(13,2.5f,.12f),q);
        }
        void Thermal(float p)
        {
            var f=track.Evaluate(p);var q=Quaternion.LookRotation(f.Forward,Vector3.up);Vector3 origin=f.Position+f.Right*50-Vector3.up*24;
            Vector3 P(float x,float y,float z)=>origin+q*new Vector3(x,y,z);
            Box("concrete",P(0,2,0),new Vector3(44,4,94),q);
            for(int i=-1;i<=1;i++)
            {
                Vector3 c=P(0,25,i*28);Batch("concrete").Cylinder(c,10,40);Batch("metal").Cylinder(c+Vector3.up*21,10.6f,2);
                for(int y=12;y<=38;y+=13){Batch("structure").Cylinder(P(0,y,i*28),10.4f,1.2f);Box("structure",P(-14,y,i*28),new Vector3(5,.8f,24),q);
                    for(int z=-10;z<=10;z+=5)Box("metal",P(-16,y+1,z+i*28),new Vector3(.12f,2,.12f),q);Box("metal",P(-16,y+2,i*28),new Vector3(.14f,.14f,24),q);}
                for(int side=-1;side<=1;side+=2)Box("structure",P(side*11,8,i*28),new Vector3(2,16,8),q);
                Box("amber",P(-10.2f,28,i*28),new Vector3(.2f,15,1),q);
                Spot("Process pool",P(-13,30,i*28),P(-22,4,i*28),amber,12,45,90);
            }
            for(int y=9;y<=18;y+=9){Beam("metal",P(-12,y,-36),P(-12,y,36),2,2);
                for(int z=-32;z<=32;z+=16)Box("structure",P(-12,y*.5f,z),new Vector3(1,y,1),q);}
        }
        void Station(float p)
        {
            Facade(p,1,27,31,78,true);
            var f=track.Evaluate(p);var q=Quaternion.LookRotation(f.Forward,Vector3.up);
            for(int i=-4;i<=4;i++)
            {
                Vector3 c=f.Position+f.Right*26+f.Forward*i*8;
                Box("concrete",c+Vector3.up*4,new Vector3(1.1f,15,1.1f),q);
                Box("ceramic",c+Vector3.up*12-f.Right*3,new Vector3(15,.8f,7.2f),q);
                Beam("metal",c+Vector3.up*7,c+Vector3.up*11-f.Right*8,.35f,.35f);
                Box("amber",c+Vector3.up*11.5f-f.Right*5,new Vector3(6,.1f,.3f),q);
                if(i%2==0)Spot("Concourse warm pool",c+Vector3.up*11-f.Right*5,c-f.Right*6,amber,7,25,100);
            }
        }
        void CreateRoadTexture(Material road)
        {
            const int n=256;var texture=new Texture2D(n,n,TextureFormat.RGBA32,true){name="Fine directional aggregate",wrapMode=TextureWrapMode.Repeat,anisoLevel=8};
            var colors=new Color[n*n];var random=new System.Random(419);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){float shade=.88f+(float)random.NextDouble()*.07f+.015f*Mathf.Sin(x*.8f);colors[y*n+x]=new Color(shade,shade,shade,1);}
            texture.SetPixels(colors);texture.Apply();AssetDatabase.CreateAsset(texture,ProductionSceneSetup.Root+"/Materials/RoadGrain.asset");road.SetTexture("_BaseMap",texture);road.SetTextureScale("_BaseMap",new Vector2(3,1));EditorUtility.SetDirty(road);
        }
    }
}
