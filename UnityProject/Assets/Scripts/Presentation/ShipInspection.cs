using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    /// <summary>
    /// Opt-in native ship inspection. Bootstrap hook: ShipInspection.TryStart(this).
    /// Launch with -inspectShip /absolute/output/folder; optional -quitAfterInspection.
    /// Uses real runtime geometry/materials and ScreenCapture, never a replacement render.
    /// </summary>
    public sealed class ShipInspection : MonoBehaviour
    {
        const int ReflectionCardLayer = 9;
        VectorBootstrap bootstrap;
        HoverVehicle player;
        Camera camera;
        string folder;
        bool quitAfter, studioActive, completed;
        bool originalAutopilot;
        float originalTimeScale, originalReflectionIntensity;
        int originalWidth, originalHeight, originalCaptureRate;
        FullScreenMode originalScreenMode;
        bool originalFog;
        AmbientMode originalAmbientMode;
        SphericalHarmonicsL2 originalAmbient;
        Material originalSkybox;
        Light originalSun;
        CameraState originalCamera;
        GameObject studio;
        Transform studioVisual, originalVisualParent;
        Vector3 originalVisualPosition, originalVisualScale, studioVisualScale;
        Quaternion originalVisualRotation;
        int originalVisualSibling;
        bool visualDetached;
        Bounds expectedStudioBounds;
        VolumeProfile studioProfile;
        ReflectionProbe studioProbe;
        readonly List<Behaviour> disabled = new List<Behaviour>();
        readonly List<GameObject> hidden = new List<GameObject>();
        readonly List<BodyState> bodies = new List<BodyState>();
        readonly List<Material> ownedMaterials = new List<Material>();
        readonly Dictionary<Renderer,Material[]> emissionOriginals = new Dictionary<Renderer,Material[]>();
        readonly List<RaceEvidence> pausedEvidence = new List<RaceEvidence>();
        readonly InspectionReport report = new InspectionReport();

        public static bool TryStart(VectorBootstrap owner)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i=0;i<args.Length;i++)
            {
                if (args[i] != "-inspectShip") continue;
                if (i+1 >= args.Length || !Path.IsPathRooted(args[i+1]))
                { Debug.LogError("Ship inspection requires -inspectShip <absolute evidence folder>."); return false; }
                if (!owner || !owner.Director || !owner.Director.Player || !owner.Camera)
                { Debug.LogError("Call ShipInspection.TryStart after bootstrap has initialized racers, camera and director."); return false; }
                if (owner.GetComponent<ShipInspection>()) return true;
                var runner = owner.gameObject.AddComponent<ShipInspection>();
                runner.bootstrap = owner; runner.player = owner.Director.Player; runner.camera = owner.Camera;
                runner.folder = Path.GetFullPath(args[i+1]);
                runner.quitAfter = Array.IndexOf(args,"-quitAfterInspection") >= 0;
                foreach(var evidence in owner.GetComponents<RaceEvidence>())
                    if(evidence.enabled) { evidence.StopAllCoroutines(); evidence.enabled=false; runner.pausedEvidence.Add(evidence); }
                runner.StartCoroutine(runner.Run());
                return true;
            }
            return false;
        }

        IEnumerator Run()
        {
            Directory.CreateDirectory(folder);
            originalWidth=Screen.width; originalHeight=Screen.height; originalScreenMode=Screen.fullScreenMode;
            originalCaptureRate=Time.captureFramerate; originalAutopilot=player.AutopilotForTesting;
            report.startedUtc=DateTime.UtcNow.ToString("o"); report.unityVersion=Application.unityVersion;
            report.gpu=SystemInfo.graphicsDeviceName; report.resourcePath="Art/HeroShip";
            report.scope="Actual Unity screenshots of the currently imported ship and runtime materials. 00 is ordinary native chase/HUD with automated input through normal physics. Studio views use fixed neutral light, ACES, exposure 0, no bloom/vignette/fog and no propulsion effects. The explicitly named engine emission-OFF frame alone uses temporary material copies with emission disabled. All other ship materials remain unchanged. These views assess modeling and material response, not race performance or AAA acceptance.";
            File.WriteAllText(Path.Combine(folder,"inspection-status.txt"),"RUNNING — native rendering inspection\n");
            try
            {
                Time.captureFramerate=0;
                Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(2.5f);
                // Context is captured before any studio lighting, camera, material or HUD change.
                player.AutopilotForTesting=true;
                bootstrap.Director.StartRace();
                float deadline=Time.realtimeSinceStartup+12f;
                while((bootstrap.Director.Phase!=RacePhase.Racing || bootstrap.Director.RaceTime<1.35f) && Time.realtimeSinceStartup<deadline) yield return null;
                if(bootstrap.Director.Phase!=RacePhase.Racing) throw new InvalidOperationException("The native race did not start; chase context was not captured.");
                report.contextRaceTime=bootstrap.Director.RaceTime; report.contextSpeedKph=player.SpeedKph;
                yield return Capture("00-native-gameplay-chase.png",false);
                PrepareStudio();
                yield return null;
                ValidateStudioCalibration();
                Bounds bounds=ShipBounds();
                BuildStudio(bounds);
                RecordShip(bounds);
                int probeRender=studioProbe.RenderProbe();
                deadline=Time.realtimeSinceStartup+15f;
                while(!studioProbe.IsFinishedRendering(probeRender) && Time.realtimeSinceStartup<deadline) yield return null;
                report.reflectionProbeCompleted=studioProbe.IsFinishedRendering(probeRender);
                if(!report.reflectionProbeCompleted) throw new InvalidOperationException("Studio reflection probe did not finish; inspection stopped rather than capture unlit materials.");
                for(int i=0;i<6;i++) yield return null;
                float span=Mathf.Max(bounds.size.x,bounds.size.z); Vector3 center=bounds.center;
                SetPerspective(center+new Vector3(span*1.20f,span*.65f,-span*1.65f),center,50f);
                yield return Capture("01-studio-rear-three-quarter.png",true);
                SetPerspective(center+new Vector3(span*1.20f,span*.65f,span*1.65f),center,50f);
                yield return Capture("02-studio-front-three-quarter.png",true);
                SetPerspective(center+new Vector3(span*2.10f,span*.22f,0),center,55f);
                yield return Capture("03-studio-starboard-side.png",true);
                camera.usePhysicalProperties=false; camera.orthographic=true;
                camera.orthographicSize=Mathf.Max(bounds.size.z*.62f,bounds.size.x/camera.aspect*.62f);
                camera.transform.SetPositionAndRotation(center+Vector3.up*span*3f,Quaternion.LookRotation(Vector3.down,Vector3.forward));
                yield return Capture("04-studio-top-ORTHOGRAPHIC.png",true);
                Vector3 aperture=FindAperture();
                report.inspectedNozzleApertureWorld=aperture;
                SetPerspective(aperture+new Vector3(.25f,.18f,-4.2f),aperture+Vector3.forward*.25f,55f);
                DisableShipEmission();
                yield return Capture("05-studio-engine-emission-OFF.png",true);
                RestoreShipEmission();
                yield return Capture("06-studio-engine-emission-ON.png",true);
                SetPerspective(center+new Vector3(span*.60f,span*.50f,span*.25f),center+Vector3.up*.35f,60f);
                yield return Capture("07-studio-canopy-close.png",true);
                completed=true; report.finishedUtc=DateTime.UtcNow.ToString("o");
                File.WriteAllText(Path.Combine(folder,"inspection-scope.json"),JsonUtility.ToJson(report,true));
                File.WriteAllText(Path.Combine(folder,"inspection-status.txt"),"COMPLETE — eight actual native screenshots. Top view is explicitly orthographic; other studio views use a 50–60 mm perspective lens. Mesh payloads and native material mapping preserved; the labeled emission-OFF comparison uses temporary material copies.\n");
            }
            finally
            {
                Restore();
                if(!completed) File.WriteAllText(Path.Combine(folder,"inspection-status.txt"),"INCOMPLETE — see native Player.log; do not treat partial images as a passed inspection.\n");
            }
            if(quitAfter) { yield return new WaitForSecondsRealtime(1f); Application.Quit(); }
        }

        void PrepareStudio()
        {
            originalTimeScale=Time.timeScale; originalCamera=new CameraState(camera);
            originalFog=RenderSettings.fog; originalAmbientMode=RenderSettings.ambientMode; originalAmbient=RenderSettings.ambientProbe;
            originalSkybox=RenderSettings.skybox; originalSun=RenderSettings.sun; originalReflectionIntensity=RenderSettings.reflectionIntensity;
            studioActive=true; Time.timeScale=0;
            foreach(var light in FindObjectsByType<Light>(FindObjectsSortMode.None)) Disable(light);
            foreach(var probe in FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)) Disable(probe);
            foreach(var volume in FindObjectsByType<Volume>(FindObjectsSortMode.None)) Disable(volume);
            foreach(var hud in FindObjectsByType<RaceHUD>(FindObjectsSortMode.None)) Disable(hud);
            foreach(var audio in FindObjectsByType<RaceAudio>(FindObjectsSortMode.None)) Disable(audio);
            Disable(camera.GetComponent<ChaseCamera>()); Disable(bootstrap.Director);
            foreach(var racer in bootstrap.Director.Racers)
            {
                if(!racer) continue;
                bodies.Add(new BodyState(racer.Body)); racer.Body.interpolation=RigidbodyInterpolation.None; racer.Body.isKinematic=true;
                Disable(racer); Disable(racer.GetComponent<IonPropulsion>()); Disable(racer.GetComponent<VehicleVFX>());
                if(racer!=player) Hide(racer.gameObject);
            }
            foreach(var world in FindObjectsByType<WorldBuilder>(FindObjectsSortMode.None)) Hide(world.gameObject);
            Transform visual=player.VisualRoot ? player.VisualRoot : player.transform;
            foreach(Transform child in visual)
                if(child.name=="Port ion aperture" || child.name=="Starboard ion aperture" || child.name=="Central ion aperture") Hide(child.gameObject);
            foreach(var particles in player.GetComponentsInChildren<ParticleSystem>()) Hide(particles.gameObject);
            if(!player.VisualRoot || visual==player.transform || visual.GetComponent<Rigidbody>())
                throw new InvalidOperationException("Inspection requires the separate imported visual root, without a Rigidbody.");
            // An interpolated race Rigidbody can replace a teleported pose during rendering.
            // Detach the actual visual hierarchy so the studio pose has no physics ancestor.
            studioVisual=visual; originalVisualParent=visual.parent; originalVisualSibling=visual.GetSiblingIndex();
            originalVisualPosition=visual.localPosition; originalVisualRotation=visual.localRotation; originalVisualScale=visual.localScale;
            studioVisualScale=visual.lossyScale; report.originalVisualRotation=originalVisualRotation;
            expectedStudioBounds=CanonicalShipBounds(visual,studioVisualScale);
            report.canonicalBoundsCenter=expectedStudioBounds.center; report.canonicalBoundsSize=expectedStudioBounds.size;
            visual.SetParent(null,false); visualDetached=true;
            visual.SetPositionAndRotation(Vector3.zero,Quaternion.identity); visual.localScale=studioVisualScale;
            Physics.SyncTransforms();
            RenderSettings.fog=false; RenderSettings.skybox=null; RenderSettings.ambientMode=AmbientMode.Custom;
            var ambient=new SphericalHarmonicsL2(); ambient.AddAmbientLight(new Color(.20f,.20f,.20f)); RenderSettings.ambientProbe=ambient;
            RenderSettings.reflectionIntensity=1f;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.16f,.17f,.19f);
            camera.cullingMask=originalCamera.cullingMask & ~(1<<ReflectionCardLayer);
            camera.nearClipPlane=.08f; camera.farClipPlane=150f;
        }

        void BuildStudio(Bounds bounds)
        {
            studio=new GameObject("OPT IN • neutral ship inspection studio");
            var lit=Shader.Find("Universal Render Pipeline/Lit"); var unlit=Shader.Find("Universal Render Pipeline/Unlit");
            if(!lit) throw new InvalidOperationException("Studio requires the existing URP Lit shader.");
            var floorMaterial=new Material(lit){name="Inspection neutral gray floor"}; floorMaterial.SetColor("_BaseColor",new Color(.18f,.18f,.18f)); floorMaterial.SetFloat("_Smoothness",.22f); floorMaterial.SetFloat("_Metallic",0f); ownedMaterials.Add(floorMaterial);
            Box("Studio floor",new Vector3(0,bounds.min.y-.55f,0),new Vector3(70,.20f,70),floorMaterial,0);
            var cardMaterial=new Material(unlit ? unlit : lit){name="Inspection white reflection cards"};
            if(unlit) cardMaterial.SetColor("_BaseColor",new Color(2f,2f,2f));
            else { cardMaterial.SetColor("_BaseColor",Color.black);cardMaterial.SetColor("_EmissionColor",new Color(2f,2f,2f));cardMaterial.EnableKeyword("_EMISSION"); }
            ownedMaterials.Add(cardMaterial);
            Box("Overhead reflection softbox",new Vector3(-1,9,0),new Vector3(8,.08f,14),cardMaterial,ReflectionCardLayer);
            Box("Port reflection softbox",new Vector3(-7,3,0),new Vector3(.08f,5,12),cardMaterial,ReflectionCardLayer);
            Box("Starboard reflection strip",new Vector3(8,4,1),new Vector3(.08f,7,4),cardMaterial,ReflectionCardLayer);
            var key=new GameObject("Neutral fixed key"); key.transform.SetParent(studio.transform,false); key.transform.rotation=Quaternion.Euler(42,-35,0);
            var sun=key.AddComponent<Light>(); sun.type=LightType.Directional; sun.color=Color.white; sun.intensity=2.2f;
            sun.shadows=LightShadows.Soft; sun.shadowStrength=.55f; sun.shadowBias=.025f; sun.shadowNormalBias=.04f; sun.shadowCustomResolution=2048; RenderSettings.sun=sun;
            StudioFill("Rear cavity fill",new Vector3(-4,3,-9),bounds.center,42f,new Color(1f,.98f,.94f));
            StudioFill("Front broad fill",new Vector3(6,4,9),bounds.center,34f,new Color(.94f,.97f,1f));
            StudioFill("Port low fill",new Vector3(-8,2,2),bounds.center,22f,Color.white);
            var probeObject=new GameObject("Neutral studio reflection probe"); probeObject.transform.SetParent(studio.transform,false); probeObject.transform.position=bounds.center;
            studioProbe=probeObject.AddComponent<ReflectionProbe>(); studioProbe.mode=ReflectionProbeMode.Realtime; studioProbe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;
            studioProbe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce; studioProbe.resolution=512; studioProbe.hdr=true; studioProbe.size=Vector3.one*80;
            studioProbe.clearFlags=ReflectionProbeClearFlags.SolidColor; studioProbe.backgroundColor=new Color(.18f,.18f,.18f);
            studioProbe.nearClipPlane=.1f; studioProbe.farClipPlane=80; studioProbe.intensity=1f; studioProbe.importance=100; studioProbe.blendDistance=0; studioProbe.cullingMask=~(1<<8);
            var volumeObject=new GameObject("Neutral fixed inspection grade"); volumeObject.transform.SetParent(studio.transform,false);
            var volume=volumeObject.AddComponent<Volume>(); volume.isGlobal=true; volume.priority=1000; studioProfile=ScriptableObject.CreateInstance<VolumeProfile>(); volume.sharedProfile=studioProfile;
            studioProfile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
            studioProfile.Add<Bloom>().intensity.Override(0f);
            var grade=studioProfile.Add<ColorAdjustments>(); grade.postExposure.Override(0f); grade.contrast.Override(0f); grade.saturation.Override(0f);
            studioProfile.Add<Vignette>().intensity.Override(0f);
            report.studioLighting="Fixed key: white directional 2.2, soft shadows strength0.55. Shadowless spot fills: rear42/front34/port22. Neutral diffuse SH ambient0.20. HDR512 reflection probe sees three broad white cards on layer9 and excludes ship8. ACES; exposure0; bloom0; vignette0; fog off. No per-view lighting adjustments.";
        }

        void StudioFill(string name,Vector3 position,Vector3 target,float intensity,Color color)
        {
            var go=new GameObject(name); go.transform.SetParent(studio.transform,false); go.transform.position=position; go.transform.LookAt(target);
            var light=go.AddComponent<Light>(); light.type=LightType.Spot; light.color=color; light.intensity=intensity; light.range=30f; light.spotAngle=115f; light.innerSpotAngle=85f; light.shadows=LightShadows.None;
        }
        void Box(string name,Vector3 position,Vector3 size,Material material,int layer)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(studio.transform,false); go.transform.position=position; go.transform.localScale=size; go.layer=layer;
            Destroy(go.GetComponent<Collider>()); go.GetComponent<Renderer>().sharedMaterial=material;
            if(layer==ReflectionCardLayer) go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
        }
        void SetPerspective(Vector3 position,Vector3 target,float focalLength)
        {
            camera.orthographic=false; camera.usePhysicalProperties=true; camera.sensorSize=new Vector2(36f,20.25f);
            camera.lensShift=Vector2.zero; camera.gateFit=Camera.GateFitMode.Vertical; camera.focalLength=focalLength;
            camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position,Vector3.up));
        }
        Bounds ShipBounds()
        {
            var renderers=(player.VisualRoot ? player.VisualRoot : player.transform).GetComponentsInChildren<MeshRenderer>();
            bool first=true; Bounds result=new Bounds(player.transform.position,Vector3.zero);
            foreach(var renderer in renderers) if(renderer.enabled) { if(first){result=renderer.bounds;first=false;} else result.Encapsulate(renderer.bounds); }
            if(first) throw new InvalidOperationException("No visible ship render mesh found."); return result;
        }
        Bounds CanonicalShipBounds(Transform visual,Vector3 scale)
        {
            bool first=true; Bounds result=new Bounds(Vector3.zero,Vector3.zero);
            foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>())
            {
                if(!renderer.enabled) continue;
                Bounds local=renderer.localBounds; Matrix4x4 matrix=Matrix4x4.identity;
                for(Transform node=renderer.transform;node && node!=visual;node=node.parent)
                    matrix=Matrix4x4.TRS(node.localPosition,node.localRotation,node.localScale)*matrix;
                matrix=Matrix4x4.Scale(scale)*matrix;
                for(int i=0;i<8;i++)
                {
                    Vector3 corner=local.center+Vector3.Scale(local.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    Vector3 point=matrix.MultiplyPoint3x4(corner);
                    if(first) { result=new Bounds(point,Vector3.zero); first=false; } else result.Encapsulate(point);
                }
            }
            if(first) throw new InvalidOperationException("No visible ship mesh available for canonical calibration.");
            return result;
        }
        void ValidateStudioCalibration()
        {
            if(!studioVisual || !visualDetached || studioVisual.parent || studioVisual.GetComponentInParent<Rigidbody>() ||
               studioVisual.position.sqrMagnitude>.000001f || Quaternion.Angle(studioVisual.rotation,Quaternion.identity)>.01f ||
               (studioVisual.lossyScale-studioVisualScale).sqrMagnitude>.000001f)
                throw new InvalidOperationException("Studio calibration failed: ship must be detached, at world origin, with identity orientation and its original scale.");
            Bounds actual=ShipBounds();
            if((actual.center-expectedStudioBounds.center).magnitude>.01f || (actual.size-expectedStudioBounds.size).magnitude>.01f || actual.size.sqrMagnitude<.01f)
                throw new InvalidOperationException("Studio calibration failed: rendered bounds "+actual+" differ from canonical imported bounds "+expectedStudioBounds+".");
            report.studioCalibrationChecks++;
        }
        Vector3 FindAperture()
        {
            Transform root=player.VisualRoot ? player.VisualRoot : player.transform;
            foreach(var child in root.GetComponentsInChildren<Transform>(true)) if(child.name=="Starboard ion aperture") return child.position;
            return root.TransformPoint(new Vector3(1.68f,-.035f,-3.405f));
        }
        void RecordShip(Bounds bounds)
        {
            report.shipBoundsCenter=bounds.center; report.shipBoundsSize=bounds.size;
            var filters=(player.VisualRoot ? player.VisualRoot : player.transform).GetComponentsInChildren<MeshFilter>();
            var seenMaterials=new HashSet<Material>();
            foreach(var filter in filters)
            {
                Mesh mesh=filter.sharedMesh; if(!mesh) continue; long triangles=0;
                for(int i=0;i<mesh.subMeshCount;i++) if(mesh.GetTopology(i)==MeshTopology.Triangles) triangles+=(long)mesh.GetIndexCount(i)/3;
                report.meshes.Add(new MeshRecord{name=mesh.name,vertices=mesh.vertexCount,triangles=triangles}); report.totalTriangles+=triangles;
                var renderer=filter.GetComponent<Renderer>(); if(!renderer) continue;
                foreach(var material in renderer.sharedMaterials)
                {
                    if(!material || !seenMaterials.Add(material)) continue;
                    report.materials.Add(new MaterialRecord{name=material.name,shader=material.shader?material.shader.name:"missing",baseColor=material.HasProperty("_BaseColor")?material.GetColor("_BaseColor"):Color.white,metallic=material.HasProperty("_Metallic")?material.GetFloat("_Metallic"):0,smoothness=material.HasProperty("_Smoothness")?material.GetFloat("_Smoothness"):0,emission=material.HasProperty("_EmissionColor")?material.GetColor("_EmissionColor"):Color.black});
                }
            }
        }
        void DisableShipEmission()
        {
            Transform visual=player.VisualRoot ? player.VisualRoot : player.transform;
            foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>())
            {
                Material[] originals=renderer.sharedMaterials; Material[] copies=(Material[])originals.Clone(); bool changed=false;
                for(int i=0;i<copies.Length;i++)
                {
                    Material material=copies[i];
                    if(!material || !material.HasProperty("_EmissionColor") || material.GetColor("_EmissionColor").maxColorComponent<=0f) continue;
                    var copy=new Material(material){name=material.name+" [inspection emission OFF]"};copy.SetColor("_EmissionColor",Color.black);copy.DisableKeyword("_EMISSION");copies[i]=copy;ownedMaterials.Add(copy);changed=true;
                }
                if(changed) { emissionOriginals.Add(renderer,originals); renderer.sharedMaterials=copies; }
            }
        }
        void RestoreShipEmission() { foreach(var entry in emissionOriginals) if(entry.Key) entry.Key.sharedMaterials=entry.Value; emissionOriginals.Clear(); }
        IEnumerator Capture(string name,bool studioView)
        {
            for(int i=0;i<4;i++) yield return null;
            string path=Path.Combine(folder,name); DateTime requested=DateTime.UtcNow;
            yield return new WaitForEndOfFrame();
            if(studioView) ValidateStudioCalibration();
            var view=new ViewRecord{file=name,studio=studioView,width=Screen.width,height=Screen.height,position=camera.transform.position,rotation=camera.transform.rotation,orthographic=camera.orthographic,orthographicSize=camera.orthographicSize,focalLengthMm=camera.focalLength,verticalFov=camera.fieldOfView,sensorSize=camera.sensorSize,raceTime=bootstrap.Director.RaceTime,speedKph=player.SpeedKph};
            if(!studioView) { report.contextRaceTime=view.raceTime;report.contextSpeedKph=view.speedKph; }
            ScreenCapture.CaptureScreenshot(path);
            float deadline=Time.realtimeSinceStartup+12f;
            do { yield return null; } while((!File.Exists(path)||File.GetLastWriteTimeUtc(path)<requested) && Time.realtimeSinceStartup<deadline);
            if(!File.Exists(path)||File.GetLastWriteTimeUtc(path)<requested) throw new IOException("Screenshot was not written: "+path);
            report.views.Add(view);
            File.WriteAllText(Path.Combine(folder,"inspection-scope.json"),JsonUtility.ToJson(report,true));
        }
        void Disable(Behaviour component) { if(component && component.enabled) { disabled.Add(component); component.enabled=false; } }
        void Hide(GameObject go) { if(go && go.activeSelf) { hidden.Add(go); go.SetActive(false); } }
        void Restore()
        {
            RestoreShipEmission();
            if(studioActive)
            {
                studioActive=false;
                if(studio) studio.SetActive(false);
                if(visualDetached && studioVisual)
                {
                    studioVisual.SetParent(originalVisualParent,false); studioVisual.localPosition=originalVisualPosition;
                    studioVisual.localRotation=originalVisualRotation; studioVisual.localScale=originalVisualScale;
                    studioVisual.SetSiblingIndex(originalVisualSibling); visualDetached=false;
                }
                foreach(var go in hidden) if(go) go.SetActive(true);
                foreach(var body in bodies) body.Restore();
                Physics.SyncTransforms();
                foreach(var component in disabled) if(component) component.enabled=true;
                RenderSettings.fog=originalFog; RenderSettings.ambientMode=originalAmbientMode; RenderSettings.ambientProbe=originalAmbient;
                RenderSettings.skybox=originalSkybox; RenderSettings.sun=originalSun; RenderSettings.reflectionIntensity=originalReflectionIntensity;
                if(camera && originalCamera!=null) originalCamera.Restore(camera);
                Time.timeScale=originalTimeScale;
            }
            if(player) player.AutopilotForTesting=originalAutopilot;
            Time.captureFramerate=originalCaptureRate;
            if(originalWidth>0&&originalHeight>0) Screen.SetResolution(originalWidth,originalHeight,originalScreenMode);
            foreach(var evidence in pausedEvidence) if(evidence) evidence.enabled=true;
            if(studio) Destroy(studio); if(studioProfile) Destroy(studioProfile);
            foreach(var material in ownedMaterials) if(material) Destroy(material); ownedMaterials.Clear();
        }
        void OnDestroy() { if(studioActive) Restore(); }

        sealed class BodyState
        {
            readonly Rigidbody body; readonly bool kinematic; readonly Vector3 position,velocity,angularVelocity; readonly Quaternion rotation;
            readonly RigidbodyInterpolation interpolation;
            public BodyState(Rigidbody target) { body=target;kinematic=target.isKinematic;position=target.position;rotation=target.rotation;velocity=target.linearVelocity;angularVelocity=target.angularVelocity;interpolation=target.interpolation; }
            public void Restore() { if(!body)return;body.isKinematic=false;body.position=position;body.rotation=rotation;body.linearVelocity=velocity;body.angularVelocity=angularVelocity;body.isKinematic=kinematic;body.transform.SetPositionAndRotation(position,rotation);body.interpolation=interpolation; }
        }
        sealed class CameraState
        {
            public readonly int cullingMask;
            readonly Vector3 position; readonly Quaternion rotation; readonly float fov,near,far,orthoSize,focal; readonly bool physical,orthographic;
            readonly Vector2 sensor,lensShift; readonly Camera.GateFitMode gate; readonly CameraClearFlags flags; readonly Color background;
            public CameraState(Camera value) { cullingMask=value.cullingMask;position=value.transform.position;rotation=value.transform.rotation;fov=value.fieldOfView;near=value.nearClipPlane;far=value.farClipPlane;orthoSize=value.orthographicSize;focal=value.focalLength;physical=value.usePhysicalProperties;orthographic=value.orthographic;sensor=value.sensorSize;lensShift=value.lensShift;gate=value.gateFit;flags=value.clearFlags;background=value.backgroundColor; }
            public void Restore(Camera value) { value.transform.SetPositionAndRotation(position,rotation);value.cullingMask=cullingMask;value.clearFlags=flags;value.backgroundColor=background;value.nearClipPlane=near;value.farClipPlane=far;value.sensorSize=sensor;value.lensShift=lensShift;value.gateFit=gate;value.focalLength=focal;value.usePhysicalProperties=physical;value.orthographic=orthographic;value.orthographicSize=orthoSize;if(!physical)value.fieldOfView=fov; }
        }
        [Serializable] sealed class InspectionReport
        {
            public string scope,startedUtc,finishedUtc,unityVersion,gpu,resourcePath,studioLighting;
            public bool reflectionProbeCompleted; public float contextRaceTime,contextSpeedKph; public long totalTriangles; public int studioCalibrationChecks;
            public Vector3 shipBoundsCenter,shipBoundsSize,canonicalBoundsCenter,canonicalBoundsSize,inspectedNozzleApertureWorld; public Quaternion originalVisualRotation;
            public List<MeshRecord> meshes=new List<MeshRecord>(); public List<MaterialRecord> materials=new List<MaterialRecord>(); public List<ViewRecord> views=new List<ViewRecord>();
        }
        [Serializable] sealed class MeshRecord { public string name; public int vertices; public long triangles; }
        [Serializable] sealed class MaterialRecord { public string name,shader; public Color baseColor,emission; public float metallic,smoothness; }
        [Serializable] sealed class ViewRecord { public string file;public bool studio,orthographic;public int width,height;public Vector3 position;public Quaternion rotation;public float orthographicSize,focalLengthMm,verticalFov,raceTime,speedKph;public Vector2 sensorSize; }
    }
}
