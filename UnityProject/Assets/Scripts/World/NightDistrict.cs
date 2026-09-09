using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Original night city. Scenery boxes/pipes are batched by district/material.</summary>
    public sealed class NightDistrict : MonoBehaviour
    {
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Material> ownMaterials=new List<Material>();
        readonly List<Texture2D> ownTextures=new List<Texture2D>();
        readonly Dictionary<Material,Batch> batches=new Dictionary<Material,Batch>();
        readonly List<Vector3> accepted=new List<Vector3>();
        public IReadOnlyList<Vector3> DistrictCenters => accepted;
        Material concrete,steel,trim,asphalt,warm,cyan,windows;
        Material openingConcrete,openingDeck,openingSteel,openingOccupied;
        Material civicConcrete,civicSteel,civicGlass,civicWarm,civicWorkshopWarm,civicTowerWarm,farConcrete,farGlass;
        readonly Material[] facades=new Material[4];
        readonly Material[] skylineFacades=new Material[12];
        NightLandmarks landmarks;
        readonly List<Plot> benchmarkPlots=new List<Plot>();
        Vector3 benchmarkOrigin;
        Quaternion benchmarkRotation;
        float benchmarkClearance;
        Vector3[] samples;
        float clearance;
        readonly List<OpeningObstacle> openingObstacles=new List<OpeningObstacle>();
        Vector3 openingFrontage;
        Quaternion openingFrontageRotation;
        bool openingFrontageExists;


        public void Build(WorldBuilder world,TrackPath track)
        {
            concrete=world.MakeMaterial("Night / blue slate ceramic",new Color(.12f,.17f,.23f),.42f,.1f);
            steel=world.MakeMaterial("Night / industrial gunmetal",new Color(.032f,.049f,.075f),.55f,.45f);
            trim=world.MakeMaterial("Night / weathered aluminum",new Color(.29f,.36f,.42f),.65f,.5f);
            asphalt=world.MakeMaterial("Night / service asphalt",new Color(.025f,.035f,.049f),.5f,.08f);
            warm=world.MakeMaterial("Night / amber wayfinding",new Color(.55f,.22f,.055f),.25f,0,new Color(2.2f,.75f,.15f));
            cyan=world.MakeMaterial("Night / cool architectural light",new Color(.08f,.3f,.42f),.25f,0,new Color(.12f,.75f,1.2f));
            var shader=Shader.Find("VectorRush/NightWindows");
            windows=shader?new Material(shader):world.MakeMaterial("Night / window fallback",new Color(.08f,.15f,.22f),.75f,.1f,new Color(.15f,.24f,.35f));
            if(shader){ownMaterials.Add(windows);windows.SetFloat("_Density",.53f);}
            float[] cellWidths={2.1f,1.65f,3.3f,2.65f},floorHeights={3.4f,3.1f,3.8f,3.3f};
            float[] densities={.49f,.36f,.57f,.4f},intensities={.86f,.57f,.70f,.48f};
            for(int i=0;i<facades.Length;i++)
            {
                if(!shader){facades[i]=windows;continue;}
                var facade=new Material(windows){name="Night / facade character "+i};ownMaterials.Add(facade);facades[i]=facade;
                facade.SetFloat("_CellWidth",cellWidths[i]);facade.SetFloat("_FloorHeight",floorHeights[i]);
                facade.SetFloat("_Density",densities[i]);facade.SetFloat("_Intensity",intensities[i]);
                facade.SetFloat("_Seed",3+i*7);facade.SetFloat("_BandInterval",5+i*2);
                if(i==1){facade.SetColor("_WarmColor",new Color(.54f,.46f,.32f));facade.SetColor("_CoolColor",new Color(.32f,.53f,.7f));}
                if(i==3){facade.SetColor("_WarmColor",new Color(.62f,.38f,.22f));facade.SetColor("_CoolColor",new Color(.21f,.41f,.56f));}
            }
            // Distant occupied rooms recede in three layers, leaving the near landmarks dominant.
            for(int depth=0;depth<3;depth++)for(int character=0;character<4;character++)
            {
                var distant=new Material(facades[character]){name="Night / skyline depth "+depth+" character "+character};
                ownMaterials.Add(distant);skylineFacades[depth*4+character]=distant;
                if(shader){
                    distant.SetFloat("_Intensity",intensities[character]*(depth==0?.72f:depth==1?.48f:.30f));
                    distant.SetFloat("_Density",densities[character]*(depth==0?.95f:.8f));
                    distant.SetColor("_WarmColor",new Color(.56f,.35f,.22f));
                    distant.SetColor("_CoolColor",new Color(.28f,.40f,.56f));
                }
            }
            samples=new Vector3[720];for(int i=0;i<samples.Length;i++)samples[i]=track.Evaluate((float)i/samples.Length).Position;
            clearance=track.Width*.5f+30f;
            Box(Vector3.zero,Quaternion.identity,new Vector3(0,-1.15f,0),new Vector3(2400,.5f,2400),asphalt);
            Flush("Night city ground");
            var a=Resources.Load<GameObject>("Art/Environment/Solstice_TerraceTower_A");
            var b=Resources.Load<GameObject>("Art/Environment/Solstice_SplitTower_B");
            BuildTransitBenchmark(world,track,a,b);
            landmarks=gameObject.AddComponent<NightLandmarks>();landmarks.Build(world,track);
            float[] anchors={.13f,.40f,.67f,.88f};
            for(int i=0;i<anchors.Length;i++)
            {
                // The first-sector plinth obscured the signal mast approach with a blank wall.
                // Authored landmark/service clusters now own this view and the final station sector.
                if(i==0||i==3)continue;
                var frame=track.Evaluate(anchors[i]);Vector3 outward=Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
                Quaternion q=Quaternion.LookRotation(-outward,Vector3.up);Vector3 center=frame.Position+outward*135f;center.y=0;
                for(int n=0;n<12&&(!Clear(center,q,new Vector2(82,77))||OverlapsBenchmark(center,q,new Vector2(82,77)));n++)center+=outward*15f;
                if(!Clear(center,q,new Vector2(82,77))||OverlapsBenchmark(center,q,new Vector2(82,77)))continue;
                accepted.Add(center);
                District(center,q,i,a,b);
                Flush("Night district "+(i+1));
            }
            // Low industrial service blocks fill selected middle-distance gaps.
            for(int i=0;i<6;i++)
            {
                if(i==5)continue;
                var frame=track.Evaluate(.04f+i*.16f);Vector3 direction=Vector3.ProjectOnPlane(frame.Right,Vector3.up).normalized;
                Vector3 center=frame.Position-direction*115f;center.y=0;Quaternion q=Quaternion.LookRotation(direction,Vector3.up);
                if(!Clear(center,q,new Vector2(43,32))||OverlapsBenchmark(center,q,new Vector2(43,32)))continue;
                bool occupied=false;foreach(var other in accepted)if((center-other).sqrMagnitude<120*120)occupied=true;
                if(occupied)continue;
                Industrial(center,q,i);
                if(i==1){openingFrontage=center;openingFrontageRotation=q;openingFrontageExists=true;}
                Flush("Inner service works "+i);
            }
            BuildUrbanFabric();
            if(OpeningFinishPreview.ConstructionEnabled)BuildOpeningConstruction(track);
        }

        void BuildUrbanFabric()
        {
            var random=new System.Random(64281);
            // Continuous surrounding skyline: staggered rings make parallax and depth,
            // with taller silhouettes behind lower buildings instead of four isolated islands.
            int[] counts={22,26,30};float[] radii={445,625,855};
            for(int ring=0;ring<3;ring++)
            {
                for(int i=0;i<counts[ring];i++)
                {
                    float angle=(i+.27f*ring)*Mathf.PI*2/counts[ring];
                    float r=radii[ring]+((float)random.NextDouble()-.5f)*48;
                    Vector3 center=new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r);
                    Quaternion q=Quaternion.Euler(0,-angle*Mathf.Rad2Deg+90,0);
                    float w=24+(float)random.NextDouble()*20,d=22+(float)random.NextDouble()*21;
                    float cluster=.5f+.5f*Mathf.Sin(angle*3f+.65f);
                    float height=42+(float)random.NextDouble()*44+cluster*48+ring*24;
                    if(i%9==0)height+=28;
                    bool nearDistrict=false;
                    foreach(var other in accepted)if((center-other).sqrMagnitude<105f*105f)nearDistrict=true;
                    if(nearDistrict||InBenchmarkArea(center)||OverlapsBenchmark(center,q,new Vector2(w*.7f+5,d*.7f+5))||!Clear(center,q,new Vector2(w*.7f+5,d*.7f+5)))continue;
                    SkylineTower(center,q,w,d,height,(i+ring)%5,ring);
                }
                Flush("Layered skyline / depth "+ring);
            }
            // Mid-rise districts bridge the space between the race and the skyline.
            for(int i=0;i<26;i++)
            {
                float a=(i+.4f)*Mathf.PI*2/26;float r=345+(i%3)*17;
                Vector3 c=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
                Quaternion q=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);
                bool nearDistrict=false;foreach(var other in accepted)if((c-other).sqrMagnitude<120*120)nearDistrict=true;
                if(nearDistrict||InBenchmarkArea(c)||OverlapsBenchmark(c,q,new Vector2(32,30))||!Clear(c,q,new Vector2(32,30)))continue;
                SkylineTower(c,q,43,38,25+(i%5)*10,i%5);
                Box(c,q,new Vector3(0,1,0),new Vector3(62,3,58),concrete);
                Box(c,q,new Vector3(0,4.8f,27),new Vector3(54,5,.25f),windows);
                Box(c,q,new Vector3(0,8,28),new Vector3(59,.45f,3),steel);
                Box(c,q,new Vector3(0,7.75f,29.6f),new Vector3(42,.12f,.16f),i%3==0?cyan:warm);
            }
            Flush("Middle city / mixed-use blocks");
            // Interior blocks give the descent an inhabited foreground below the track.
            for(int x=-2;x<=2;x++)for(int z=-2;z<=2;z++)
            {
                Vector3 c=new Vector3(x*70+12,0,z*70+14);Quaternion q=Quaternion.Euler(0,(x+z)%2*90,0);
                if(InBenchmarkArea(c)||OverlapsBenchmark(c,q,new Vector2(25,26))||!Clear(c,q,new Vector2(25,26)))continue;
                SkylineTower(c,q,36,40,19+Mathf.Abs(x*11+z*7)%36,Mathf.Abs(x+z)%5);
            }
            Flush("Inner city / low urban fabric");
            // A real ground-level street grid, seen through the elevated road supports.
            // These lanes and 8m streetlamps cannot reach the 28m minimum track height.
            for(int lane=-7;lane<=7;lane++)
            {
              for(int segment=-7;segment<=7;segment++)
              {
                float x=lane*112,z=segment*112;
                Box(Vector3.zero,Quaternion.identity,new Vector3(x,.02f,z),new Vector3(13,.12f,112),asphalt);
                Box(Vector3.zero,Quaternion.identity,new Vector3(x,.02f,z),new Vector3(112,.12f,13),asphalt);
                for(int side=-1;side<=1;side+=2)
                {
                    Box(Vector3.zero,Quaternion.identity,new Vector3(x+side*6.7f,.16f,z),new Vector3(.07f,.06f,78),warm);
                    Vector3 pole=new Vector3(x+side*8,4,z+35);
                    Box(Vector3.zero,Quaternion.identity,pole,new Vector3(.22f,8,.22f),trim);
                    Box(Vector3.zero,Quaternion.identity,pole+new Vector3(-side*.9f,4,0),new Vector3(2,.12f,.45f),warm);
                    // Tiny opposing traffic queues are architectural light detail, not gameplay AI.
                    for(int vehicle=0;vehicle<2;vehicle++)
                    {
                        Vector3 car=new Vector3(x+side*3,.45f,z-26+vehicle*12+side*9);
                        Box(Vector3.zero,Quaternion.identity,car,new Vector3(1.8f,.8f,4),steel);
                        Box(Vector3.zero,Quaternion.identity,car+new Vector3(0,0,side*2.05f),new Vector3(1.5f,.15f,.1f),side>0?warm:cyan);
                    }
                }
              }
              if(lane%3==0)Flush("Service avenues / sector "+lane);
            }
            Flush("Service avenues / remainder");
        }

        void BuildTransitBenchmark(WorldBuilder world,TrackPath track,GameObject terrace,GameObject split)
        {
            civicConcrete=world.MakeMaterial("Transit / blue gray concrete",new Color(.38f,.46f,.51f),.36f,.05f);
            civicSteel=world.MakeMaterial("Transit / charcoal structure",new Color(.055f,.075f,.091f),.32f,.18f);
            civicGlass=world.MakeMaterial("Transit / recessed dark glass",new Color(.027f,.068f,.089f),.66f,.25f);
            BuildOccupiedGlazing(world);
            farConcrete=world.MakeMaterial("Transit skyline / quiet blue mass",new Color(.22f,.28f,.32f),.24f,.03f);
            farGlass=world.MakeMaterial("Transit skyline / grouped dim rooms",new Color(.10f,.15f,.18f),.3f,0,new Color(.105f,.13f,.14f));
            var frame=track.Evaluate(.918f);var forward=Vector3.ProjectOnPlane(frame.Forward,Vector3.up).normalized;
            benchmarkRotation=Quaternion.LookRotation(forward,Vector3.up);benchmarkOrigin=frame.Position;benchmarkOrigin.y=0;
            // Add a full sample-step margin to the 18 m camera envelope, so the
            // coarse runtime sampling cannot accept a bend between sample points.
            benchmarkClearance=18f+track.Length/samples.Length;
            var station=Resources.Load<GameObject>("Art/Environment/Nocturne_TransitStation_A");
            var workshop=Resources.Load<GameObject>("Art/Environment/Nocturne_ServiceWorkshop_A");
            var buttress=Resources.Load<GameObject>("Art/Environment/Nocturne_PlatformButtress_A");
            // Candidate 01 placed this at .918: it was behind the chase camera
            // by the .947 reveal. Keep the same landmark ahead across both views.
            var stationFrame=track.Evaluate(.970f);
            var stationRoute=Quaternion.LookRotation(Vector3.ProjectOnPlane(stationFrame.Forward,Vector3.up).normalized,Vector3.up);
            // Native candidate 01 exposed the source's blank rear elevation.
            // This FBX arrives with its occupied side on local +X; turn that
            // side toward the road instead of trusting the source X convention.
            var stationFacing=stationRoute*Quaternion.Euler(0,180,0);
            Vector3 stationPosition=stationFrame.Position+stationRoute*Vector3.right*33f-Vector3.up*7f;
            if(station&&Reserve(stationPosition,stationFacing,new Vector2(11.6f,32)))
            {
                PlaceAuthored(station,"Benchmark / exit transit station",stationPosition,stationFacing,1);
                // Two unscaled structural modules meet the platform foundation;
                // their lower pedestals continue to the visible service level.
                foreach(float along in new[]{-21f,20f})
                {
                    Vector3 p=stationPosition+stationFacing*new Vector3(3,-11.135f,along);
                    PlaceAuthored(buttress,"Benchmark / grounded station buttress",p,stationFacing,1);
                    Box(p,stationFacing,new Vector3(0,-p.y*.5f,0),new Vector3(4.4f,p.y,7.2f),civicConcrete);
                    Box(p,stationFacing,new Vector3(0,-p.y+.30f,0),new Vector3(5.8f,.6f,8.4f),civicSteel);
                }
                Vector3 service=stationPosition;service.y=.16f;
                Box(service,stationFacing,Vector3.zero,new Vector3(22.8f,.3f,63.5f),civicSteel);
                PlaceAuthored(workshop,"Benchmark / ground service rooms",service+stationFacing*new Vector3(3,0,0),stationFacing,1);
                // A broad service apron and limited markings make the station
                // visibly belong to the ground, with no additional lamp cadence.
                for(int bay=0;bay<3;bay++)Box(service,stationFacing,new Vector3(-6.3f,.18f,-8+bay*8),new Vector3(3.2f,.04f,.16f),trim);
                foreach(float along in new[]{-17f,16f})
                    ArchitecturalFill("Transit canopy / warm concealed wash",stationPosition,stationFacing,new Vector3(8.0f,10.5f,along),new Vector3(1.5f,7.2f,along+3),new Color(1,.73f,.47f),210,27,116);
            }
            var other=track.Evaluate(.948f);var oq=Quaternion.LookRotation(Vector3.ProjectOnPlane(other.Forward,Vector3.up).normalized,Vector3.up);
            Vector3 opposite=other.Position-oq*Vector3.right*29f-Vector3.up*3f;
            var facing=oq;
            if(workshop&&Reserve(opposite,facing,new Vector2(4,9)))
            {
                PlaceAuthored(workshop,"Benchmark / opposite low service room",opposite,facing,1);
                // The elevated service room has a real structural base instead
                // of floating at the road datum.
                for(int side=-1;side<=1;side+=2)
                    Box(opposite,facing,new Vector3(0,-opposite.y*.5f,side*6),new Vector3(3.2f,opposite.y,2.0f),civicConcrete);
                Box(opposite,facing,new Vector3(0,-.35f,0),new Vector3(8,.7f,18),civicSteel);
            }
            // Reuse the authored terrace/split families as distinct middle
            // masses. The left side stays lower; bases and open courtyards show.
            var nightTerrace=Resources.Load<GameObject>("Art/Environment/Nocturne_TerraceTower_Night_A");
            var nightSplit=Resources.Load<GameObject>("Art/Environment/Nocturne_SplitTower_Night_B");
            BenchmarkTower(track,nightTerrace?nightTerrace:terrace,.913f,-86f,.75f,"low left terrace",false);
            BenchmarkTower(track,nightTerrace?nightTerrace:terrace,.926f,91f,1.0f,"station rear terrace",true);
            BenchmarkTower(track,nightSplit?nightSplit:split,.970f,122f,1.18f,"split landmark",true);
            BuildApproachServiceGroup(track,nightTerrace?nightTerrace:terrace,workshop);
            // Close but intermittent structural ledges give the approach real
            // parallax. Their complete bounds get the same all-course check.
            foreach(float progress in new[]{.792f,.826f,.848f})
            {
                var f=track.Evaluate(progress);var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up).normalized,Vector3.up);
                Vector3 c=f.Position+q*Vector3.right*26f-Vector3.up*13.2f;
                if(!buttress||!Reserve(c,q,new Vector2(3.2f,4.3f)))continue;
                PlaceAuthored(buttress,"Benchmark / approach structural ledge",c,q,1);
                Box(c,q,new Vector3(0,-c.y*.5f,0),new Vector3(3.6f,c.y,5.4f),civicConcrete);
            }
            // Quiet grouped silhouettes leave the vanishing-point corridor open.
            BenchmarkSkyline(new Vector3(172,0,305),28,33,126);
            BenchmarkSkyline(new Vector3(211,0,337),25,29,151);
            // Leave the opening mast's sky gap clear; this transformed placement crowded its approach.
            Flush("Benchmark / composed transit district");
        }

        void BuildOccupiedGlazing(WorldBuilder world)
        {
            // Low-frequency material artwork inside the already-authored panes:
            // a shaded lower room, one partial blind and a soft ceiling pool.
            // This is not interior geometry, parallax, or extra occupied windows.
            const int width=512,height=256;
            var baseMap=new Texture2D(width,height,TextureFormat.RGBA32,true,false){name="Transit glazing / glass and room tone",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            var emissionMap=new Texture2D(width,height,TextureFormat.RGBA32,true,true){name="Transit glazing / restrained room light",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            var basePixels=new Color[width*height];var emissionPixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                float u=(x+.5f)/width,v=(y+.5f)/height;
                float side=SmoothRange(0,.10f,u)*(1-SmoothRange(.90f,1,u));
                float top=SmoothRange(0,.07f,v)*(1-SmoothRange(.95f,1,v));
                float pool=Mathf.Exp(-Mathf.Pow((u-.62f)/.34f,2)-Mathf.Pow((v-.62f)/.39f,2));
                float ceiling=.27f*Mathf.Exp(-Mathf.Pow((v-.83f)/.055f,2))*SmoothRange(.16f,.30f,u)*(1-SmoothRange(.77f,.86f,u));
                float lower=Mathf.Lerp(.25f,1,SmoothRange(.15f,.38f,v));
                float blind=1-.35f*SmoothRange(.15f,.21f,u)*(1-SmoothRange(.37f,.43f,u))*SmoothRange(.45f,.55f,v);
                float light=(.17f+.27f*pool+ceiling)*side*top*lower*blind;
                // A cool reflected glass tint survives around the warmer room.
                basePixels[y*width+x]=new Color(.095f+light*.26f,.135f+light*.18f,.16f+light*.07f,1);
                emissionPixels[y*width+x]=new Color(light,light*.91f,light*.78f,1);
            }
            baseMap.SetPixels(basePixels);baseMap.Apply(true,true);emissionMap.SetPixels(emissionPixels);emissionMap.Apply(true,true);
            ownTextures.Add(baseMap);ownTextures.Add(emissionMap);
            civicWarm=world.MakeMaterial("Transit / glass over occupied rooms",Color.white,.73f,.10f,new Color(.55f,.32f,.16f));
            civicWarm.SetTexture("_BaseMap",baseMap);civicWarm.SetTexture("_EmissionMap",emissionMap);
            SetRoomUV(civicWarm,new Vector2(4f/6.3f,4f/5.7f),new Vector2(0,-2.75f/5.7f));
            // Workshop UV0 is the same 4 m metric projection, fitted to its
            // single existing 3.5 x 3 m recessed occupied pane.
            civicWorkshopWarm=new Material(civicWarm){name="Transit / workshop recessed glazing"};ownMaterials.Add(civicWorkshopWarm);
            SetRoomUV(civicWorkshopWarm,new Vector2(4f/3.5f,4f/3f),new Vector2(.5f,-1.4f/3f));
            // Tower glazing retains legacy UV0. Reuse the exact shader already
            // instantiated above for the native city; no new shader variant or
            // CPU mesh read is required. Apply only to existing WarmWindow faces.
            if(windows&&windows.shader&&windows.shader.name=="VectorRush/NightWindows")
            {
                civicTowerWarm=new Material(windows){name="Transit / authored warm floor glass"};ownMaterials.Add(civicTowerWarm);
                civicTowerWarm.SetColor("_BaseColor",new Color(.055f,.082f,.105f));
                civicTowerWarm.SetColor("_WarmColor",new Color(.44f,.27f,.13f));
                civicTowerWarm.SetColor("_CoolColor",new Color(.36f,.23f,.13f));
                civicTowerWarm.SetFloat("_Density",1f);civicTowerWarm.SetFloat("_CellWidth",7.4f);
                civicTowerWarm.SetFloat("_FloorHeight",7f);civicTowerWarm.SetFloat("_Intensity",.95f);
                civicTowerWarm.SetFloat("_BandInterval",999f);civicTowerWarm.SetFloat("_Seed",23f);
            }
            else civicTowerWarm=civicWarm;
        }

        static float SmoothRange(float a,float b,float value)
        {
            float t=Mathf.Clamp01((value-a)/(b-a));return t*t*(3-2*t);
        }
        static void SetRoomUV(Material material,Vector2 scale,Vector2 offset)
        {
            material.SetTextureScale("_BaseMap",scale);material.SetTextureOffset("_BaseMap",offset);
            material.SetTextureScale("_EmissionMap",scale);material.SetTextureOffset("_EmissionMap",offset);
        }

        void BuildApproachServiceGroup(TrackPath track,GameObject terrace,GameObject workshop)
        {
            if(!terrace)return;
            var f=track.Evaluate(.735f);var route=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up).normalized,Vector3.up);
            Vector3 c=f.Position-route*Vector3.right*65f;c.y=9;
            // Aim this one composed frontage toward the recorded approach
            // camera. No camera or gameplay transform is changed.
            Vector3 towardCamera=new Vector3(-212.6821f,0,41.7193f)-new Vector3(c.x,0,c.z);
            var q=Quaternion.LookRotation(towardCamera.normalized,Vector3.up);
            if(!Reserve(c,q,new Vector2(18,20)))return;
            PlaceAuthored(terrace,"Benchmark / approach terrace over service court",c,q,.82f);
            Box(c,q,new Vector3(0,-.5f,0),new Vector3(31,1,34),civicConcrete);
            foreach(float x in new[]{-11f,11f})foreach(float z in new[]{-12f,12f})
                Box(c,q,new Vector3(x,-4.9f,z),new Vector3(2.2f,7.8f,2.8f),civicConcrete);
            Vector3 ground=c;ground.y=.2f;
            Box(ground,q,Vector3.zero,new Vector3(35,.4f,39),civicSteel);
            PlaceAuthored(workshop,"Benchmark / approach occupied service court",ground,q*Quaternion.Euler(0,-90,0),1);
        }

        void BenchmarkTower(TrackPath track,GameObject source,float progress,float offset,float scale,string name,bool lightFacade)
        {
            if(!source)return;
            var f=track.Evaluate(progress);var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f.Forward,Vector3.up).normalized,Vector3.up);
            Vector3 c=f.Position+q*Vector3.right*offset;c.y=3.9f;
            // Reorient the authored entrance toward the service courtyard.
            var orientation=q*Quaternion.Euler(0,offset>0?-90:90,0);
            var half=new Vector2(19*scale,21*scale);
            if(!Reserve(c,orientation,half))return;
            PlaceAuthored(source,"Benchmark / "+name,c,orientation,scale);
            Box(c,orientation,new Vector3(0,-2.1f,0),new Vector3(half.x*2,4.2f,half.y*2),civicSteel);
            Box(c,orientation,new Vector3(0,.03f,0),new Vector3(half.x*2,.16f,half.y*2),civicConcrete);
            // A separate street-facing service room is supported by the podium.
            Box(c,orientation,new Vector3(0,-1.0f,half.y+.03f),new Vector3(half.x*1.3f,1.4f,.12f),civicGlass);
            if(lightFacade)
                ArchitecturalFill("Transit facade / cool structural wash",c,orientation,new Vector3(-8*scale,5,half.y+11),new Vector3(0,31*scale,0),new Color(.65f,.77f,1),1850,105,66);
        }

        void ArchitecturalFill(string name,Vector3 c,Quaternion q,Vector3 localPosition,Vector3 localTarget,Color color,float intensity,float range,float cone)
        {
            Vector3 position=c+q*localPosition,target=c+q*localTarget;
            var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.position=position;
            go.transform.rotation=Quaternion.LookRotation(target-position,Vector3.up);
            var light=go.AddComponent<Light>();light.type=LightType.Spot;light.color=color;light.intensity=intensity;
            light.range=range;light.spotAngle=cone;light.innerSpotAngle=cone*.62f;light.shadows=LightShadows.None;
            // Physical dark housings, with no extra neon strips or road lamp cadence.
            Box(position,go.transform.rotation,Vector3.zero,new Vector3(.52f,.26f,.48f),civicSteel);
        }

        void BenchmarkSkyline(Vector3 local,float w,float d,float height)
        {
            Vector3 c=benchmarkOrigin+benchmarkRotation*local;c.y=0;
            if(!Reserve(c,benchmarkRotation,new Vector2(w*.6f,d*.6f)))return;
            Box(c,benchmarkRotation,new Vector3(0,3,0),new Vector3(w*1.15f,6,d*1.15f),farConcrete);
            Box(c,benchmarkRotation,new Vector3(0,height*.36f,0),new Vector3(w,height*.72f,d),farConcrete);
            Box(c,benchmarkRotation,new Vector3(w*.12f,height*.84f,0),new Vector3(w*.68f,height*.24f,d*.74f),farConcrete);
            Box(c,benchmarkRotation,new Vector3(w*.18f,height*.97f,0),new Vector3(w*.36f,height*.06f,d*.46f),civicSteel);
            // A few large occupied groups have lower contrast than the route;
            // gaps and height changes carry the distant identity.
            for(int group=0;group<3;group++)for(int floor=0;floor<2;floor++)
                Box(c,benchmarkRotation,new Vector3(-w*.13f,height*(.22f+group*.18f)+floor*3.3f,-d*.502f),new Vector3(w*.46f,1.15f,.12f),farGlass);
        }

        GameObject PlaceAuthored(GameObject source,string name,Vector3 p,Quaternion q,float scale)
        {
            if(!source)return null;
            var go=Instantiate(source,transform);go.name=name;go.transform.SetPositionAndRotation(p,q);go.transform.localScale=Vector3.one*scale;
            bool tower=source.name.Contains("Tower"),workshop=source.name.Contains("Workshop");
            Material occupied=tower?civicTowerWarm:workshop?civicWorkshopWarm:civicWarm;
            foreach(var renderer in go.GetComponentsInChildren<Renderer>())
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    string material=mats[i]?mats[i].name:"Concrete";
                    mats[i]=material.Contains("WarmWindow")?occupied:material.Contains("Glass")?civicGlass:
                        material.Contains("Metal")?trim:material.Contains("Concrete")||material.Contains("Ivory")?civicConcrete:civicSteel;
                }
                renderer.sharedMaterials=mats;
            }
            return go;
        }

        bool Reserve(Vector3 c,Quaternion q,Vector2 half)
        {
            var inverse=Quaternion.Inverse(q);
            foreach(var p in samples)
            {
                Vector3 d=inverse*(p-c);float x=Mathf.Max(0,Mathf.Abs(d.x)-half.x),z=Mathf.Max(0,Mathf.Abs(d.z)-half.y);
                if(x*x+z*z<benchmarkClearance*benchmarkClearance)return false;
            }
            if(OverlapsBenchmark(c,q,half))return false;
            benchmarkPlots.Add(new Plot{center=c,rotation=q,half=half});return true;
        }
        bool InBenchmarkArea(Vector3 c)
        {
            Vector3 local=Quaternion.Inverse(benchmarkRotation)*(c-benchmarkOrigin);
            return local.z>-145&&local.z<350&&Mathf.Abs(local.x)<157;
        }
        bool OverlapsBenchmark(Vector3 c,Quaternion q,Vector2 half)
        {
            Vector3 a=q*Vector3.right,b=q*Vector3.forward;
            foreach(var plot in benchmarkPlots)
            {
                Vector3 d=c-plot.center,u=plot.rotation*Vector3.right,v=plot.rotation*Vector3.forward;
                bool separate=false;
                foreach(var axis in new[]{a,b,u,v})
                {
                    float radius=half.x*Mathf.Abs(Vector3.Dot(a,axis))+half.y*Mathf.Abs(Vector3.Dot(b,axis))+
                        plot.half.x*Mathf.Abs(Vector3.Dot(u,axis))+plot.half.y*Mathf.Abs(Vector3.Dot(v,axis))+4f;
                    if(Mathf.Abs(Vector3.Dot(d,axis))>radius){separate=true;break;}
                }
                if(!separate)return true;
            }
            return false;
        }
        struct Plot{public Vector3 center;public Quaternion rotation;public Vector2 half;}

        void SkylineTower(Vector3 c,Quaternion q,float w,float d,float h,int style,int depth=-1)
        {
            RecordOpeningObstacle(c,q,new Vector2(w*.5f+4,d*.5f+4),h*1.05f+9);
            int character=(Mathf.FloorToInt(Mathf.Abs(c.x+c.z)*.037f)+style)%4;
            Material facade=depth>=0?skylineFacades[Mathf.Clamp(depth,0,2)*4+character]:facades[character];
            bool detailed=c.x*c.x+c.z*c.z<420f*420f;
            Box(c,q,new Vector3(0,4,0),new Vector3(w+8,8,d+8),steel);
            if(style==1)
            {
                // Unequal paired blades with a deliberate vertical gap and linking floor.
                Box(c,q,new Vector3(-w*.29f,h*.5f,0),new Vector3(w*.43f,h,d),facade);
                Box(c,q,new Vector3(w*.29f,h*.38f,0),new Vector3(w*.43f,h*.76f,d*.87f),facade);
                Box(c,q,new Vector3(0,h*.63f,0),new Vector3(w*.65f,5,d*.57f),steel);
                Box(c,q,new Vector3(-w*.29f,h+.8f,0),new Vector3(w*.44f,1.6f,d+1),concrete);
            }
            else
            {
                float lower=h*.64f,upper=h*.24f;
                Box(c,q,new Vector3(0,lower*.5f,0),new Vector3(w,lower,d),facade);
                Box(c,q,new Vector3(style==2?w*.12f:0,lower+upper*.5f,0),new Vector3(w*.79f,upper,d*.81f),facade);
                if(style==3)
                {
                    // An open mechanical crown replaces the repeated opaque roof block.
                    Box(c,q,new Vector3(0,h*.88f+1,0),new Vector3(w*.64f,2,d*.64f),steel);
                    for(int side=-1;side<=1;side+=2)Box(c,q,new Vector3(side*w*.27f,h*.945f,0),new Vector3(.65f,h*.13f,.65f),trim);
                    Box(c,q,new Vector3(0,h*1.015f,0),new Vector3(w*.57f,.65f,1.2f),concrete);
                }
                else if(style==4)
                    Box(c,q,new Vector3(w*.15f,h*.895f,-d*.15f),new Vector3(w*.5f,3,d*.48f),concrete);
                else
                    Box(c,q,new Vector3(style==2?w*.22f:-w*.12f,h*.94f,-d*.08f),new Vector3(w*.46f,h*.12f,d*.53f),steel);
                Box(c,q,new Vector3(0,lower+.4f,0),new Vector3(w+1,.8f,d+1),concrete);
                Box(c,q,new Vector3(style==2?w*.12f:0,lower+upper+.4f,0),new Vector3(w*.8f,.8f,d*.83f),trim);
            }
            if(detailed)
            {
                // Close buildings have a matte service elevation and deep construction
                // breaks, so the camera reads volumes instead of equally luminous boxes.
                Box(c,q,new Vector3(w*.505f,h*.32f,0),new Vector3(.42f,h*.64f,d*.96f),concrete);
                Box(c,q,new Vector3(-w*.14f,h*.32f,d*.512f),new Vector3(w*.13f,h*.64f,.7f),steel);
                for(int band=1;band<=2;band++)
                    Box(c,q,new Vector3(0,h*.2f*band,0),new Vector3(w+.7f,2.3f,d+.7f),steel);
                for(int fin=0;fin<5;fin++)
                {
                    float finX=-w*.42f+fin*w*.21f;
                    if(style==1&&Mathf.Abs(finX)<w*.13f)continue;
                    Box(c,q,new Vector3(finX,h*.31f,d*.54f),new Vector3(.38f,h*.6f,1.1f),concrete);
                }
                if(style==1)
                {
                    Box(c,q,new Vector3(-w*.29f,h*.84f,d*.51f),new Vector3(w*.4f,1.2f,.8f),trim);
                    Box(c,q,new Vector3(w*.29f,h*.77f,0),new Vector3(w*.44f,1.5f,d*.9f),concrete);
                }
            }
            for(int side=-1;side<=1;side+=2)
            {
                Box(c,q,new Vector3(side*w*.48f,h*.32f,-d*.49f),new Vector3(.8f,h*.64f,1.2f),concrete);
                Box(c,q,new Vector3(side*w*.48f,h*.32f,d*.49f),new Vector3(.8f,h*.64f,1.2f),concrete);
            }
            if(style==0||style==3)
            {
                // A few architectural edge/crown lights establish readable district identity.
                Box(c,q,new Vector3(w*.24f,h*.7f,d*.42f),new Vector3(.18f,h*.22f,.2f),style==0?cyan:warm);
                Box(c,q,new Vector3(0,h+1,d*.29f),new Vector3(w*.52f,.18f,.22f),style==0?cyan:warm);
                Pipe(c,q,new Vector3(0,h,0),new Vector3(0,h+9,0),.3f,trim);
            }
            if(style==4)
            {
                // Vertical utility sign is a restrained grouped light mark, not a bright slab.
                Box(c,q,new Vector3(-w*.5f-1,h*.59f,0),new Vector3(1.8f,19,3.5f),steel);
                for(int mark=0;mark<4;mark++)Box(c,q,new Vector3(-w*.5f-2,h*.59f-6+mark*4,0),new Vector3(.15f,1.2f,2.4f),warm);
            }
        }

        void District(Vector3 c,Quaternion q,int index,GameObject a,GameObject b)
        {
            RecordOpeningObstacle(c,q,new Vector2(82,82),100);
            Box(c,q,new Vector3(0,1.2f,0),new Vector3(154,4.4f,140),steel);
            Box(c,q,new Vector3(0,3.55f,0),new Vector3(151,.3f,137),concrete);
            // Occupied street-edge podium: distinct deep piers, recessed storefronts and canopy.
            Box(c,q,new Vector3(0,8,53),new Vector3(130,9,17),steel);
            Box(c,q,new Vector3(0,7.7f,61.6f),new Vector3(125,6,.16f),windows);
            Box(c,q,new Vector3(0,12.7f,55),new Vector3(134,.65f,22),concrete);
            Box(c,q,new Vector3(0,12.25f,65.9f),new Vector3(115,.14f,.2f),index%2==0?warm:cyan);
            for(int i=0;i<14;i++)Box(c,q,new Vector3(-63+i*9.6f,8,62),new Vector3(.75f,9,1.4f),trim);
            Vector3[] positions={new Vector3(-39,3.7f,-17),new Vector3(28,3.7f,-28),new Vector3(9,3.7f,23)};
            float[] scales={1.1f,.84f,.99f};
            for(int i=0;i<positions.Length;i++)
            {
                var source=(index+i)%2==0?a:b;if(!source)continue;
                var tower=Instantiate(source,transform);tower.name="Night / occupied authored tower "+index+"-"+i;
                tower.transform.SetPositionAndRotation(c+q*positions[i],q);tower.transform.localScale=Vector3.one*scales[i];
                foreach(var renderer in tower.GetComponentsInChildren<Renderer>())
                {
                    var mats=renderer.sharedMaterials;
                    for(int m=0;m<mats.Length;m++)
                    {
                        string name=mats[m]?mats[m].name:"Ivory";
                        mats[m]=name.Contains("Glass")?facades[(index+i)%4]:name.Contains("Metal")?trim:name.Contains("Ivory")?concrete:steel;
                    }
                    renderer.sharedMaterials=mats;
                }
                // Narrow crown lights identify silhouettes without illuminating complete slabs.
                Box(c,q,positions[i]+new Vector3(0,73.5f*scales[i],0),new Vector3(12*scales[i],.22f,.3f),i==1?warm:cyan);
            }
            // Separated service viaduct occupies district frontage, safely outside race footprint.
            Box(c,q,new Vector3(0,6.2f,73),new Vector3(140,1.3f,10),concrete);
            Box(c,q,new Vector3(0,6.89f,73),new Vector3(140,.08f,8.4f),asphalt);
            for(int x=-60;x<=60;x+=24)
            {
                Box(c,q,new Vector3(x,2.1f,73),new Vector3(1.7f,7,3),steel);
                Box(c,q,new Vector3(x,6.97f,73),new Vector3(5,.03f,.16f),warm);
            }
            Box(c,q,new Vector3(0,7.7f,77.6f),new Vector3(140,1,.3f),steel);
            Box(c,q,new Vector3(0,8.25f,77.6f),new Vector3(140,.08f,.16f),warm);
            // Human-scale access furniture and linked utilities, batched into very few draws.
            for(int i=0;i<7;i++)
            {
                float x=-59+i*19;
                Box(c,q,new Vector3(x,5,66),new Vector3(.18f,3,.18f),trim);
                Box(c,q,new Vector3(x,6.6f,66),new Vector3(.45f,.25f,.45f),warm);
            }
            Industrial(c+q*new Vector3(-38,3.7f,-45),q,index,true);
        }

        void Industrial(Vector3 c,Quaternion q,int index,bool compact=false)
        {
            float sx=compact?.52f:1f;
            Vector3 dims=new Vector3(72*sx,12,40*sx);
            RecordOpeningObstacle(c,q,new Vector2(dims.x*.5f+1,dims.z*.5f+1),20);
            Box(c,q,new Vector3(0,6,0),dims,steel);
            Box(c,q,new Vector3(0,12.4f,0),new Vector3(dims.x+2,.7f,dims.z+2),concrete);
            Box(c,q,new Vector3(0,6.3f,dims.z*.5f+.06f),new Vector3(dims.x-3,9,.12f),windows);
            for(int i=0;i<9;i++)
            {
                float x=-dims.x*.47f+i*dims.x*.1175f;
                Box(c,q,new Vector3(x,6.2f,dims.z*.5f+.7f),new Vector3(.55f,12,1.4f),concrete);
                if(i%3==0)Box(c,q,new Vector3(x+.4f,9.8f,dims.z*.5f+1.5f),new Vector3(3,.13f,.15f),warm);
            }
            // Roof plant: cooling banks with fins, risers and connected bent pipe runs.
            for(int i=0;i<3;i++)
            {
                Vector3 unit=new Vector3(-dims.x*.3f+i*dims.x*.3f,14,0);
                Box(c,q,unit,new Vector3(8*sx,3,9*sx),concrete);
                for(int f=0;f<6;f++)Box(c,q,unit+new Vector3(0,-1+f*.4f,4.6f*sx),new Vector3(7*sx,.11f,.25f),steel);
                Pipe(c,q,new Vector3(unit.x,12.9f,-dims.z*.25f),new Vector3(unit.x,18,-dims.z*.25f),.65f,trim);
                Pipe(c,q,new Vector3(unit.x,18,-dims.z*.25f),new Vector3(unit.x+5*sx,18,-dims.z*.25f),.65f,trim);
                Box(c,q,new Vector3(unit.x,18.8f,-dims.z*.25f),new Vector3(.2f,.2f,.2f),warm);
            }
            Pipe(c,q,new Vector3(-dims.x*.48f,13.2f,-dims.z*.32f),new Vector3(dims.x*.48f,13.2f,-dims.z*.32f),.8f,trim);
            Box(c,q,new Vector3(0,.05f,dims.z*.5f+7),new Vector3(dims.x+10,.15f,11),asphalt);
        }

        // Editable rough construction: two existing-frontage connections, three
        // module types, four housed practical lights. Native acceptance is pending.
        void BuildOpeningConstruction(TrackPath track)
        {
            if(!openingFrontageExists)
            {
                Debug.LogWarning("VECTOR_RUSH_OPENING_CONSTRUCTION groups=0 reason=existing-frontage-unavailable",this);
                return;
            }
            // These are the ACTUAL accepted .20 Industrial frontage and the
            // existing .200/.275 WorldBuilder feet, not generated target pixels.
            Vector3 east=openingFrontage+openingFrontageRotation*new Vector3(0,0,20);
            // The existing inner-grid tower shares this industrial footprint;
            // its east facade is at X=172. Connect at that existing face too.
            east.x=Mathf.Max(east.x,172f);
            // North blade of the existing co-located 34 m grid tower: its
            // upper full-height blade reaches Z=-37.82 at this 22 m datum.
            Vector3 north=new Vector3(152,0,-37.82f);
            BuildOpeningMaterials();
            int count=0;
            if(BuildOpeningGroup(track,"crossing service court",.200f,east))count++;
            if(BuildOpeningGroup(track,"inner frontage return",.275f,north,22f/12.75f))count++;
            Debug.Log("VECTOR_RUSH_OPENING_CONSTRUCTION groups="+count+" addedLights="+(count*2)+" moduleTypes=3 colliders=0",this);
        }

        bool BuildOpeningGroup(TrackPath track,string name,float progress,Vector3 frontage,float heightScale=1f)
        {
            Vector3 routePosition=track.Evaluate(progress).Position;
            Transform foot=null;float nearest=1f;
            foreach(var candidate in GetComponentsInChildren<Transform>())
            {
                if(candidate.name!="Pier foot")continue;
                float distance=Vector2.Distance(new Vector2(candidate.position.x,candidate.position.z),new Vector2(routePosition.x,routePosition.z));
                if(distance<nearest){nearest=distance;foot=candidate;}
            }
            if(!foot)
            {
                Debug.LogWarning("VECTOR_RUSH_OPENING_GROUP name="+name+" accepted=false reason=actual-pier-foot-missing",this);return false;
            }
            Vector3 socket=foot.TransformPoint(new Vector3(-.33f,.46f,0));
            Vector3 direction=Vector3.ProjectOnPlane(socket-frontage,Vector3.up);
            float length=direction.magnitude;Quaternion q=Quaternion.LookRotation(direction.normalized,Vector3.up);
            Vector3 c=frontage;c.y=0;
            float floor=12.75f*heightScale;
            float room=length*(heightScale>1f?.70f:.32f);
            var pieces=new List<OpeningPiece>();
            var lamps=new List<OpeningLamp>();
            // Type 1: a seven-metre service walk with dark running surface,
            // a mineral edge and two actual deep longitudinal steel girders.
            // Split each layer at its exact podium boundary. Distinct local
            // UV origins must never create two coplanar mapped top surfaces.
            foreach(bool after in new[]{false,true})
            {
                float start=after?room+12.9f:0,end=after?length:room-12.9f;
                OpeningBox(pieces,new Vector3(0,floor-.50f,(start+end)*.5f),new Vector3(7,1,end-start),openingConcrete);
                foreach(float side in new[]{-1f,1f})
                    OpeningBox(pieces,new Vector3(side*3.36f,floor+.28f,(start+end)*.5f),new Vector3(.28f,.56f,end-start),openingSteel);
                start=after?room+12.6f:0;end=after?length:room-12.6f;
                OpeningBox(pieces,new Vector3(0,floor+.025f,(start+end)*.5f),new Vector3(6.5f,.05f,end-start),openingDeck);
            }
            foreach(float side in new[]{-1f,1f})
                OpeningBox(pieces,new Vector3(side*2.6f,floor-1.35f,length*.5f),new Vector3(.5f,1.65f,length),openingSteel);
            // Type 2: the room now sits ON the connected service datum. Its
            // 8.5 m front is visible above the .22 barrier; the old 22 m tall
            // empty ground-level cubby and its broad pale roof are removed.
            OpeningBox(pieces,new Vector3(0,floor-.5f,room),new Vector3(13.8f,1,25.8f),openingConcrete);
            OpeningBox(pieces,new Vector3(0,floor+.025f,room),new Vector3(13.2f,.05f,25.2f),openingDeck);
            OpeningBox(pieces,new Vector3(-5.45f,floor+3.8f,room),new Vector3(.7f,7.6f,23.2f),openingSteel);
            foreach(float end in new[]{-11.2f,11.2f})
            {
                OpeningBox(pieces,new Vector3(-1,floor+3.8f,room+end),new Vector3(9.6f,7.6f,.9f),openingConcrete);
                OpeningBox(pieces,new Vector3(3.55f,floor+3.8f,room+end),new Vector3(1.1f,7.6f,1.7f),openingSteel);
            }
            // Recessed occupied cassettes, a central service door and a solid
            // lower return establish use/scale without rows of tiny detailing.
            OpeningBox(pieces,new Vector3(1.9f,floor+3.65f,room),new Vector3(.35f,7.3f,21.4f),openingSteel);
            foreach(float bay in new[]{-6f,6f})
            {
                OpeningBox(pieces,new Vector3(2.12f,floor+4.15f,room+bay),new Vector3(.14f,3.8f,5.4f),civicGlass);
                OpeningBox(pieces,new Vector3(2.22f,floor+4.15f,room+bay),new Vector3(.08f,3.2f,4.8f),openingOccupied);
                OpeningBox(pieces,new Vector3(2.5f,floor+1.15f,room+bay),new Vector3(.7f,2.3f,5.5f),openingConcrete);
                OpeningBox(pieces,new Vector3(3.1f,floor+6.5f,room+bay),new Vector3(2.1f,.45f,5.6f),openingSteel);
            }
            OpeningBox(pieces,new Vector3(2.18f,floor+2.6f,room),new Vector3(.18f,5.2f,3.2f),civicGlass);
            OpeningBox(pieces,new Vector3(-.45f,floor+8.05f,room),new Vector3(11.1f,.9f,24.9f),openingDeck);
            OpeningBox(pieces,new Vector3(4.7f,floor+7.55f,room),new Vector3(.6f,.7f,24.9f),openingSteel);
            // Type 3: grounded room frames and intermittent span supports.
            // The four room columns replace the former opaque ground walls.
            // Their feet, caps and steel girders make the bearing path explicit.
            foreach(float end in new[]{-10f,10f})
            {
                foreach(float side in new[]{-4.7f,4.7f})
                {
                    OpeningBox(pieces,new Vector3(side,(floor-1)*.5f,room+end),new Vector3(1.8f,floor-1,2.2f),openingConcrete);
                    OpeningBox(pieces,new Vector3(side,.4f,room+end),new Vector3(3.3f,.8f,3.7f),openingConcrete);
                }
                OpeningBox(pieces,new Vector3(0,floor-1.35f,room+end),new Vector3(12.8f,1.3f,2.5f),openingSteel);
            }
            foreach(float along in new[]{length*.18f,length*.46f,length*.75f})
            {
                if(Mathf.Abs(along-room)<13)continue;
                OpeningBox(pieces,new Vector3(0,(floor-1)*.5f,along),new Vector3(2.6f,floor-1,3.6f),openingConcrete);
                OpeningBox(pieces,new Vector3(0,.35f,along),new Vector3(4.2f,.7f,5.2f),openingConcrete);
                OpeningBox(pieces,new Vector3(0,floor-1.35f,along),new Vector3(6.8f,1.2f,2.8f),openingSteel);
            }
            float socketTop=floor-.65f;
            OpeningBox(pieces,new Vector3(0,(socket.y+socketTop)*.5f,length),new Vector3(1.8f,socketTop-socket.y,2.2f),openingConcrete);
            OpeningBox(pieces,new Vector3(0,floor-1.3f,length-.6f),new Vector3(6.8f,1.2f,2.4f),openingSteel);
            // One visible under-canopy source models the room's recessed front.
            // The other follows the support to a real base, not another roof pool.
            OpeningLampPlan(pieces,lamps,new Vector3(4.65f,floor+6.98f,room-7),new Vector3(1.5f,floor+2.8f,room+2),new Color(1,.81f,.62f),240,27,108);
            if(heightScale>1f)
            {
                OpeningBox(pieces,new Vector3(3.05f,floor-1.15f,length-2.3f),new Vector3(1.5f,.45f,1.3f),openingSteel);
                OpeningLampPlan(pieces,lamps,new Vector3(3.55f,floor-1.5f,length-2.3f),new Vector3(0,1.25f,length),new Color(1,.78f,.57f),750,34,80);
            }
            else
            {
                OpeningBox(pieces,new Vector3(5.55f,floor+2.3f,room+10),new Vector3(2.3f,.4f,.6f),openingSteel);
                OpeningLampPlan(pieces,lamps,new Vector3(6.3f,floor+2.15f,room+10),new Vector3(4.7f,.6f,room+10),new Color(1,.78f,.57f),430,25,82);
            }
            // Every emitted mesh box is in this plan, including the rotated
            // lamp housings/lenses. No unaudited geometry is appended afterward.
            Bounds bounds=new Bounds(c+q*pieces[0].center,Vector3.zero);
            float clearanceBelowDeck=float.PositiveInfinity;string failure=null;
            foreach(var piece in pieces)bounds.Encapsulate(OpeningWorldBounds(c,q,piece));
            foreach(var piece in pieces)
                if(!OpeningClear(track,c,q,piece,ref clearanceBelowDeck,out failure))break;
            if(failure!=null)
            {
                Debug.LogWarning("VECTOR_RUSH_OPENING_GROUP name="+name+" accepted=false bounds="+bounds+" reason="+failure,this);return false;
            }
            foreach(var piece in pieces)Box(c+q*piece.center,q*piece.rotation,Vector3.zero,piece.size,piece.material,true);
            for(int i=0;i<lamps.Count;i++)OpeningPractical(name+" / "+(i==0?"occupied front":"support contact"),c,q,lamps[i]);
            Flush("Opening construction / "+name);
            Debug.Log("VECTOR_RUSH_OPENING_GROUP name="+name+" accepted=true supportProgress="+progress.ToString("F3")+" supportFoot="+foot.position.ToString("F3")+" socket="+socket.ToString("F3")+" frontage="+c.ToString("F3")+" serviceFloor="+floor.ToString("F3")+" boundsMin="+bounds.min.ToString("F3")+" boundsMax="+bounds.max.ToString("F3")+" minimumBelowProtectedDeck="+clearanceBelowDeck.ToString("F3")+" boxes="+pieces.Count+" addedLights=2 auditedHousingBoxes=4 fullCourseSamples=1200 cityFootprintsChecked="+openingObstacles.Count,this);
            return true;
        }

        void BuildOpeningMaterials()
        {
            openingConcrete=OpeningFinish("Opening / cast structural returns",new Color(.32f,.355f,.37f),"Concrete",.72f);
            openingDeck=OpeningFinish("Opening / dark mineral service tops",new Color(.10f,.135f,.155f),"Concrete",.76f);
            openingSteel=OpeningFinish("Opening / coated steel recess and girders",new Color(.075f,.095f,.11f),"Structure",.82f);
            openingOccupied=new Material(civicWorkshopWarm){name="Opening / recessed occupied cassette"};ownMaterials.Add(openingOccupied);
            // The front is 4.8 x 3.2 m and each box's UV origin is its centre.
            SetRoomUV(openingOccupied,new Vector2(4f/4.8f,4f/3.2f),new Vector2(.5f,.5f));
        }
        Material OpeningFinish(string name,Color color,string family,float smoothness)
        {
            var material=new Material(civicConcrete){name=name};ownMaterials.Add(material);
            material.SetColor("_BaseColor",color);material.color=color;
            bool mapped=NightArchitectureFinishes.Apply(material,family);material.SetFloat("_Smoothness",smoothness);
            Debug.Log("VECTOR_RUSH_OPENING_FINISH material="+name+" existingMaps="+mapped+" uvMetresPerTile=4 normalMap=false",this);
            return material;
        }

        void OpeningLampPlan(List<OpeningPiece> pieces,List<OpeningLamp> lamps,Vector3 position,Vector3 target,Color color,float intensity,float range,float angle)
        {
            Quaternion aim=Quaternion.LookRotation(target-position,Vector3.up);
            // The Light origin is the front lens. The housing sits behind it.
            pieces.Add(new OpeningPiece{center=position+aim*new Vector3(0,0,-.28f),size=new Vector3(.85f,.34f,.6f),rotation=aim,material=openingSteel});
            pieces.Add(new OpeningPiece{center=position,size=new Vector3(.68f,.22f,.035f),rotation=aim,material=warm});
            lamps.Add(new OpeningLamp{position=position,target=target,color=color,intensity=intensity,range=range,angle=angle});
        }
        void OpeningPractical(string name,Vector3 c,Quaternion q,OpeningLamp lamp)
        {
            var housing=new GameObject("Opening practical / "+name);housing.transform.SetParent(transform,false);
            housing.transform.SetPositionAndRotation(c+q*lamp.position,Quaternion.LookRotation(q*(lamp.target-lamp.position),Vector3.up));
            var light=housing.AddComponent<Light>();light.type=LightType.Spot;light.color=lamp.color;light.intensity=lamp.intensity;
            light.range=lamp.range;light.spotAngle=lamp.angle;light.innerSpotAngle=lamp.angle*.64f;light.shadows=LightShadows.None;
            light.cullingMask=~(1<<8);light.renderMode=LightRenderMode.Auto;
        }

        bool OpeningClear(TrackPath track,Vector3 c,Quaternion q,OpeningPiece piece,ref float minimum,out string failure)
        {
            failure=null;Vector3 center=c+q*piece.center;
            // Rotated fixture boxes use their conservative group-local AABB
            // for the yaw-only landmark/city footprint checks too.
            Bounds local=OpeningWorldBounds(Vector3.zero,Quaternion.identity,piece);
            var half=new Vector2(local.extents.x,local.extents.z);
            if(landmarks&&landmarks.Overlaps(center,q,half)){failure="landmark-reservation";return false;}
            if(OverlapsBenchmark(center,q,half)){failure="transit-reservation";return false;}
            Bounds b=OpeningWorldBounds(c,q,piece);
            // Under-road construction cannot use Clear's 2D city-building test:
            // a physical pier-foot contact has zero lateral clearance. Require
            // the entire new box below the banked underbody plus 3 m margin,
            // on EVERY branch that enters the conservative camera envelope.
            const int count=1200;float step=track.Length/count;
            float horizontal=18f+step;
            float underbody=13.3f*Mathf.Sin(17f*Mathf.Deg2Rad)+1.8f+3f+step;
            for(int i=0;i<count;i++)
            {
                Vector3 p=track.Evaluate((float)i/count).Position;
                float x=Mathf.Max(b.min.x-p.x,0,p.x-b.max.x),z=Mathf.Max(b.min.z-p.z,0,p.z-b.max.z);
                if(x*x+z*z>=horizontal*horizontal)continue;
                float gap=p.y-underbody-b.max.y;minimum=Mathf.Min(minimum,gap);
                if(gap<=0){failure="whole-course-protected-envelope@"+((float)i/count).ToString("F4");return false;}
            }
            foreach(var obstacle in openingObstacles)
            {
                // The .20 industrial block and its already-overlapping grid
                // tower are the two existing contact surfaces. The recipe
                // begins at their exterior faces; it adds no duplicate base.
                if(Vector3.ProjectOnPlane(obstacle.center-openingFrontage,Vector3.up).sqrMagnitude<9)continue;
                if(b.max.y<=obstacle.center.y||b.min.y>=obstacle.center.y+obstacle.height)continue;
                if(OpeningOverlap(center,q,half,obstacle)){failure="existing-city-footprint@"+obstacle.center;return false;}
            }
            return true;
        }

        static bool OpeningOverlap(Vector3 c,Quaternion q,Vector2 half,OpeningObstacle other)
        {
            Vector3 a=q*Vector3.right,b=q*Vector3.forward,u=other.rotation*Vector3.right,v=other.rotation*Vector3.forward,d=c-other.center;
            foreach(var axis in new[]{a,b,u,v})
            {
                float extent=half.x*Mathf.Abs(Vector3.Dot(a,axis))+half.y*Mathf.Abs(Vector3.Dot(b,axis))+other.half.x*Mathf.Abs(Vector3.Dot(u,axis))+other.half.y*Mathf.Abs(Vector3.Dot(v,axis));
                if(Mathf.Abs(Vector3.Dot(d,axis))>=extent)return false;
            }
            return true;
        }

        void RecordOpeningObstacle(Vector3 c,Quaternion q,Vector2 half,float height)
        {
            if(OpeningFinishPreview.ConstructionEnabled)openingObstacles.Add(new OpeningObstacle{center=c,rotation=q,half=half,height=height});
        }
        static void OpeningBox(List<OpeningPiece> pieces,Vector3 center,Vector3 size,Material material)
        {
            pieces.Add(new OpeningPiece{center=center,size=size,rotation=Quaternion.identity,material=material});
        }
        static Bounds OpeningWorldBounds(Vector3 c,Quaternion q,OpeningPiece piece)
        {
            Vector3 h=piece.size*.5f;
            Bounds bounds=new Bounds(c+q*piece.center,Vector3.zero);
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                bounds.Encapsulate(c+q*(piece.center+piece.rotation*Vector3.Scale(h,new Vector3(x,y,z))));
            return bounds;
        }
        struct OpeningPiece{public Vector3 center,size;public Quaternion rotation;public Material material;}
        struct OpeningLamp{public Vector3 position,target;public Color color;public float intensity,range,angle;}
        struct OpeningObstacle{public Vector3 center;public Quaternion rotation;public Vector2 half;public float height;}

        bool Clear(Vector3 center,Quaternion q,Vector2 half)
        {
            if(landmarks && landmarks.Overlaps(center,q,half))return false;
            var inverse=Quaternion.Inverse(q);
            foreach(var p in samples)
            {
                Vector3 d=inverse*(p-center);float x=Mathf.Max(0,Mathf.Abs(d.x)-half.x),z=Mathf.Max(0,Mathf.Abs(d.z)-half.y);
                if(x*x+z*z<clearance*clearance)return false;
            }
            return true;
        }
        Batch Get(Material material){if(!batches.TryGetValue(material,out var batch)){batch=new Batch();batches.Add(material,batch);}return batch;}
        void Box(Vector3 c,Quaternion q,Vector3 p,Vector3 size,Material mat,bool metricUV=false)
        {
            var batch=Get(mat);Vector3 h=size*.5f;
            Vector3[] corners={new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)};
            int[] faces={0,3,2,1,5,6,7,4,4,7,3,0,1,2,6,5,3,7,6,2,4,0,1,5};
            for(int f=0;f<6;f++)
            {
                batch.Quad(c+q*(p+corners[faces[f*4]]),c+q*(p+corners[faces[f*4+1]]),c+q*(p+corners[faces[f*4+2]]),c+q*(p+corners[faces[f*4+3]]));
                if(!metricUV)continue;
                for(int k=0;k<4;k++)
                {
                    Vector3 v=p+corners[faces[f*4+k]];
                    batch.uv.Add((f<2?new Vector2(v.x,v.y):f<4?new Vector2(v.z,v.y):new Vector2(v.x,v.z))*.25f);
                }
            }
        }
        void Pipe(Vector3 c,Quaternion q,Vector3 a,Vector3 b,float radius,Material material)
        {
            Vector3 axis=(b-a).normalized;Vector3 side=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;
            Vector3 up=Vector3.Cross(axis,side);var batch=Get(material);
            for(int i=0;i<8;i++)
            {
                float t=i*Mathf.PI*.25f,u=(i+1)*Mathf.PI*.25f;Vector3 p=(side*Mathf.Cos(t)+up*Mathf.Sin(t))*radius,r=(side*Mathf.Cos(u)+up*Mathf.Sin(u))*radius;
                batch.Quad(c+q*(a+p),c+q*(a+r),c+q*(b+r),c+q*(b+p));
            }
        }
        void Flush(string name)
        {
            foreach(var pair in batches)
            {
                var mesh=new Mesh{name=name+" / "+pair.Key.name};if(pair.Value.vertices.Count>65535)mesh.indexFormat=IndexFormat.UInt32;
                mesh.SetVertices(pair.Value.vertices);mesh.SetTriangles(pair.Value.triangles,0);
                if(pair.Value.uv.Count>0&&pair.Value.uv.Count==pair.Value.vertices.Count)mesh.SetUVs(0,pair.Value.uv);
                mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
                var go=new GameObject(mesh.name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            batches.Clear();
        }
        sealed class Batch
        {
            public readonly List<Vector3> vertices=new List<Vector3>();public readonly List<int> triangles=new List<int>();public readonly List<Vector2> uv=new List<Vector2>();
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);foreach(var mat in ownMaterials)if(mat)Destroy(mat);foreach(var texture in ownTextures)if(texture)Destroy(texture);}
    }
}
