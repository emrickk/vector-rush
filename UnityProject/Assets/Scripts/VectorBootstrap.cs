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
            var bloom=profile.Add<Bloom>();bloom.threshold.Override(1.15f);bloom.intensity.Override(.2f);bloom.scatter.Override(.55f);
            var tonemap=profile.Add<Tonemapping>();tonemap.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.15f);color.contrast.Override(3);color.saturation.Override(3);
            var vignette=profile.Add<Vignette>();vignette.intensity.Override(.17f);vignette.smoothness.Override(.65f);
            var volume=new GameObject("Race grade").AddComponent<Volume>();volume.isGlobal=true;volume.profile=profile;
            var craftIvory=world.MakeMaterial("Craft pearl ceramic",new Color(.8f,.84f,.81f),.72f,.44f);
            var craftCeramic=world.MakeMaterial("Craft thermal ceramic",new Color(.055f,.08f,.088f),.53f,.35f);
            var racers=new List<HoverVehicle>();
            GameObject ship=Resources.Load<GameObject>("Art/HeroShip");
            for(int i=0;i<6;i++){
                Color teamColor=i==0?new Color(.56f,.75f,.006f):Color.HSVToRGB(i*.16f,.72f,.85f);
                var teamPaint=world.MakeMaterial("Team accent "+i,teamColor,.6f,.25f);
                var hullPaint=i==0?craftIvory:world.MakeMaterial("Team pearl hull "+i,Color.Lerp(new Color(.55f,.62f,.64f),teamColor,.48f),.65f,.3f);
                var root=new GameObject(i==0?"VESPER 01 • player":"Rival "+i);root.transform.SetParent(transform);
                root.AddComponent<Rigidbody>();var collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(5.2f,1.2f,7.2f);collider.center=new Vector3(0,.12f,.1f);
                GameObject art;
                if(ship){art=Instantiate(ship,root.transform);art.name="Craft visual";art.transform.localPosition=Vector3.zero;art.transform.localRotation=Quaternion.identity;art.transform.localScale=Vector3.one;}
                else {art=GameObject.CreatePrimitive(PrimitiveType.Cube);art.name="Missing craft asset — graybox";art.transform.SetParent(root.transform,false);art.transform.localScale=new Vector3(4,1,7);Destroy(art.GetComponent<Collider>());}
                foreach(var renderer in art.GetComponentsInChildren<Renderer>()){
                    var materials=renderer.sharedMaterials;
                    for(int k=0;k<materials.Length;k++){
                        string n=materials[k]?materials[k].name:"Ivory";
                        if(n.Contains("Engine"))materials[k]=world.Engine;
                        else if(n.Contains("Glass"))materials[k]=world.Glass;
                        else if(n.Contains("Graphite")||n.Contains("Ink"))materials[k]=world.Graphite;
                        else if(n.Contains("Ceramic"))materials[k]=craftCeramic;
                        else if(n.Contains("Metal"))materials[k]=world.Metal;
                        else if(n.Contains("WhiteMark"))materials[k]=world.MakeMaterial("White number",new Color(.97f,.99f,.94f),.3f);
                        else if(n.Contains("Signal"))materials[k]=teamPaint;
                        else materials[k]=hullPaint;
                    }renderer.sharedMaterials=materials;
                }
                var vehicle=root.AddComponent<HoverVehicle>();vehicle.VisualRoot=art.transform;vehicle.Initialize(Track,i==0,i);racers.Add(vehicle);
                AddTrails(root,world.Engine);
            }
            var chase=cameraObject.AddComponent<ChaseCamera>();chase.Initialize(racers[0]);
            Director=new GameObject("Race director").AddComponent<RaceDirector>();Director.Initialize(Track,racers);
            var hud=new GameObject("Vector interface").AddComponent<RaceHUD>();hud.Initialize(Director);
            var audio=new GameObject("Race sound").AddComponent<RaceAudio>();audio.Initialize(Director);
            gameObject.AddComponent<RaceEvidence>();
        }
        void AddTrails(GameObject root,Material mat)
        {
            for(int side=-1;side<=1;side+=2){
                var g=new GameObject("Ion wake");g.transform.SetParent(root.transform,false);g.transform.localPosition=new Vector3(side*1.9f,-.15f,-3.45f);
                var trail=g.AddComponent<TrailRenderer>();trail.sharedMaterial=mat;trail.time=.16f;trail.minVertexDistance=.15f;trail.startWidth=.18f;trail.endWidth=0;trail.shadowCastingMode=ShadowCastingMode.Off;
                trail.startColor=new Color(.15f,.9f,1,.55f);trail.endColor=new Color(.1f,.75f,.9f,0);
            }
        }
        void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;}
    }
}
