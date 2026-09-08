using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    public sealed class VectorBootstrap : MonoBehaviour
    {
        public static VectorBootstrap Instance { get; private set; }
        public RaceDirector Director { get; private set; }
        public TrackPath Track { get; private set; }
        public Camera Camera { get; private set; }

        void Awake()
        {
            Instance=this;
            Application.targetFrameRate=120;QualitySettings.vSyncCount=1;
            Time.fixedDeltaTime=1f/100f;Time.maximumDeltaTime=.1f;
            Track=new GameObject("Solstice • race spline").AddComponent<TrackPath>();Track.transform.SetParent(transform);
            var world=new GameObject("Solstice • coastal infrastructure").AddComponent<WorldBuilder>();world.transform.SetParent(transform);world.Build(Track);
            var cameraObject=new GameObject("Race camera");cameraObject.tag="MainCamera";Camera=cameraObject.AddComponent<Camera>();
            Camera.nearClipPlane=.3f;Camera.farClipPlane=7000;Camera.fieldOfView=67;Camera.allowHDR=true;Camera.backgroundColor=new Color(.5f,.7f,.8f);
            cameraObject.AddComponent<AudioListener>();
            var cameraData=Camera.GetUniversalAdditionalCameraData();cameraData.renderPostProcessing=true;cameraData.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;cameraData.antialiasingQuality=AntialiasingQuality.High;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom=profile.Add<Bloom>();bloom.threshold.Override(1.3f);bloom.intensity.Override(.32f);bloom.scatter.Override(.55f);
            var tonemap=profile.Add<Tonemapping>();tonemap.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.3f);color.contrast.Override(3);color.saturation.Override(3);
            var vignette=profile.Add<Vignette>();vignette.intensity.Override(.16f);vignette.smoothness.Override(.65f);
            var volume=new GameObject("Race grade").AddComponent<Volume>();volume.isGlobal=true;volume.profile=profile;
            var craftIvory=ShipSurfaceMaps.Create(world,"Craft pearl ceramic","Ivory",ShipSurfaceMaps.HasSurface("Ivory")?Color.white:new Color(.72f,.78f,.83f),.58f,.18f);
            var craftCeramic=ShipSurfaceMaps.Create(world,"Craft thermal ceramic","Ceramic",ShipSurfaceMaps.HasSurface("Ceramic")?Color.white:new Color(.055f,.072f,.095f),.46f,.24f);
            var craftGraphite=ShipSurfaceMaps.Create(world,"Craft carbon structure","Graphite",ShipSurfaceMaps.HasSurface("Graphite")?Color.white:new Color(.018f,.034f,.045f),.54f,.35f);
            var craftMetal=ShipSurfaceMaps.HasSurface("Metal")?ShipSurfaceMaps.Create(world,"Craft exposed hardware","Metal",Color.white,.55f,.78f):world.Metal;
            var craftGlass=world.MakeMaterial("Craft optical canopy",new Color(.008f,.014f,.019f),.87f,0f);
            var craftEngine=world.MakeMaterial("Nozzle integrated accent",new Color(.045f,.19f,.25f),.62f,.35f,new Color(.015f,.46f,.68f));
            var craftEngineCore=world.MakeMaterial("Compact white ion core",new Color(.65f,.85f,.92f),.42f,0f,new Color(1.7f,2.6f,3.0f));
            var racers=new List<HoverVehicle>();
            GameObject ship=Resources.Load<GameObject>("Art/HeroShip");
            for(int i=0;i<6;i++){
                Color teamColor=i==0?new Color(.56f,.75f,.006f):Color.HSVToRGB(i*.16f,.72f,.85f);
                var teamPaint=world.MakeMaterial("Team accent "+i,teamColor,.6f,.25f);
                var hullPaint=i==0?craftIvory:ShipSurfaceMaps.Create(world,"Team pearl hull "+i,"Ivory",Color.Lerp(new Color(.55f,.62f,.64f),teamColor,.48f),.65f,.3f);
                var root=new GameObject(i==0?"VESPER 01 • player":"Rival "+i);root.transform.SetParent(transform);
                root.AddComponent<Rigidbody>();var collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(5.2f,1.2f,7.2f);collider.center=new Vector3(0,.12f,.1f);
                GameObject art;
                if(ship){art=Instantiate(ship,root.transform);art.name="Craft visual";art.transform.localPosition=Vector3.zero;art.transform.localRotation=Quaternion.identity;art.transform.localScale=Vector3.one;}
                else {art=GameObject.CreatePrimitive(PrimitiveType.Cube);art.name="Missing craft asset — graybox";art.transform.SetParent(root.transform,false);art.transform.localScale=new Vector3(4,1,7);Destroy(art.GetComponent<Collider>());}
                foreach(var renderer in art.GetComponentsInChildren<Renderer>()){
                    var materials=renderer.sharedMaterials;
                    for(int k=0;k<materials.Length;k++){
                        string n=materials[k]?materials[k].name:"Ivory";
                        if(n.Contains("EngineCore"))materials[k]=craftEngineCore;
                        else if(n.Contains("Engine"))materials[k]=craftEngine;
                        else if(n.Contains("Glass"))materials[k]=craftGlass;
                        else if(n.Contains("Graphite")||n.Contains("Ink"))materials[k]=craftGraphite;
                        else if(n.Contains("Ceramic"))materials[k]=craftCeramic;
                        else if(n.Contains("Metal"))materials[k]=craftMetal;
                        else if(n.Contains("WhiteMark"))materials[k]=world.MakeMaterial("White number",new Color(.97f,.99f,.94f),.3f);
                        else if(n.Contains("Signal"))materials[k]=teamPaint;
                        else materials[k]=hullPaint;
                    }renderer.sharedMaterials=materials;
                }
                foreach(var child in root.GetComponentsInChildren<Transform>())child.gameObject.layer=8;
                var vehicle=root.AddComponent<HoverVehicle>();vehicle.VisualRoot=art.transform;
                // The player uses the same pace preset in manual play and test driving.
                // Keep acceleration responsive while bringing its straight-line pace into the rival field.
                if(i==0){vehicle.CruiseSpeed=60f;vehicle.BoostSpeed=82f;}
                vehicle.Initialize(Track,i==0,i);racers.Add(vehicle);
                root.AddComponent<IonPropulsion>().Initialize(vehicle);
            }
            var chase=cameraObject.AddComponent<ChaseCamera>();chase.Initialize(racers[0]);
            Director=new GameObject("Race director").AddComponent<RaceDirector>();Director.Initialize(Track,racers);
            var hud=new GameObject("Vector interface").AddComponent<RaceHUD>();hud.Initialize(Director);
            var audio=new GameObject("Race sound").AddComponent<RaceAudio>();audio.Initialize(Director);
            gameObject.AddComponent<RaceEvidence>();
            if(!PaceEvidence.TryStart(this) && !EnvironmentEvidence.TryStart(this) && !RoadSurfaceEvidence.TryStart(this) && !HudEvidence.TryStart(this) && !ThrottleEvidence.TryStart(this)) ShipInspection.TryStart(this);
        }
        void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;}
    }
}
