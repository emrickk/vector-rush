using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    /// <summary>Opt-in matched native road-material/light diagnosis; never modifies the authored road.</summary>
    public sealed class RoadSurfaceEvidence : MonoBehaviour
    {
        const float DefaultTargetProgress=.88475f, ConstantSmoothness=.56f;
        static readonly string[] CasterNames={"Track lighting steelwork","Cool linear road lamps","Amber linear road lamps"};
        float targetProgress=DefaultTargetProgress;
        bool casterMode;
        readonly List<CasterState> casters=new List<CasterState>();
        MeshCollider roadCollider;
        Mesh originalColliderMesh;
        string originalLightSnapshot;
        VectorBootstrap bootstrap;
        HoverVehicle player;
        MeshRenderer road;
        Material[] originalMaterials;
        Mesh roadMesh;
        Vector3[] originalNormals;
        string folder;
        float originalTimeScale;
        int originalCaptureRate,originalWidth,originalHeight;
        FullScreenMode originalScreenMode;
        bool originalAutopilot,initialized;
        Vector3 cameraPosition;
        Quaternion cameraRotation;
        readonly List<Behaviour> disabled=new List<Behaviour>();
        readonly List<BodyState> bodies=new List<BodyState>();
        readonly List<LightState> lightStates=new List<LightState>();
        readonly List<Material> ownedMaterials=new List<Material>();
        Texture2D constantMask;
        readonly Report report=new Report();

        public static bool TryStart(VectorBootstrap owner)
        {
            string[] args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-roadSurfaceEvidence");
            if(index<0) return false;
            float target=DefaultTargetProgress; int targetIndex=Array.IndexOf(args,"-roadTargetProgress");
            if(targetIndex>=0 && (targetIndex+1>=args.Length || !float.TryParse(args[targetIndex+1],NumberStyles.Float,CultureInfo.InvariantCulture,out target) || float.IsNaN(target) || float.IsInfinity(target) || target<0f || target>=1f))
            { Debug.LogError("-roadTargetProgress requires a finite normalized value in [0,1)."); Application.Quit(2); return true; }
            if(index+1>=args.Length || !Path.IsPathRooted(args[index+1]))
            { Debug.LogError("Road surface evidence requires -roadSurfaceEvidence <absolute output folder>."); return false; }
            if(!owner || !owner.Director || !owner.Director.Player || !owner.Camera)
            { Debug.LogError("Road surface evidence requires the initialized native race."); return false; }
            foreach(var evidence in owner.GetComponents<RaceEvidence>()) { evidence.StopAllCoroutines(); evidence.enabled=false; }
            var runner=owner.gameObject.AddComponent<RoadSurfaceEvidence>(); runner.bootstrap=owner;
            runner.player=owner.Director.Player; runner.folder=Path.GetFullPath(args[index+1]);
            runner.targetProgress=target; runner.casterMode=Array.IndexOf(args,"-roadCasterEvidence")>=0;
            runner.StartCoroutine(runner.Run()); return true;
        }

        IEnumerator Run()
        {
            Directory.CreateDirectory(folder);
            originalTimeScale=Time.timeScale; originalCaptureRate=Time.captureFramerate; originalAutopilot=player.AutopilotForTesting;
            originalWidth=Screen.width; originalHeight=Screen.height; originalScreenMode=Screen.fullScreenMode; initialized=true;
            report.startedUtc=DateTime.UtcNow.ToString("o"); report.unityVersion=Application.unityVersion;
            report.buildGuid=Application.buildGUID; report.mode=casterMode?"fixture-caster-A-B-A":"legacy-road-controls"; report.targetProgress=targetProgress;
            report.gpu=SystemInfo.graphicsDeviceName; report.graphicsApi=SystemInfo.graphicsDeviceType.ToString();
            report.scope="Actual native ScreenCapture images at the first gallery passage near progress0.88475. Existing player testing autopilot reaches the pose through normal physics, with captureFramerate0. After LateUpdate/end-of-frame, the real chase camera and all racers are frozen in their rendered poses; Rigidbody interpolation is disabled only for stable matching. Each material/light condition restores the original before the next. Geometry, UVs, tangents, camera, postprocessing, light positions and intensities stay fixed. Named normal-mode controls change only mesh normals, and the named main-only shadow control changes only sun shadows relative to the no-additional-lights condition. The calibrated control is URP Lit with no maps, base color equal to the original, metallic0, smoothness0.56 and environment reflections off. Single-light controls retain the original directional lights and ambient, changing only which originally enabled non-directional lights are enabled. Selection is an approximate attenuation ranking at three right-side road points, not a measured BRDF contribution. These images diagnose the artifact; capture completion does not identify its cause or establish visual acceptance.";
            report.normalEncodingSourceNote="WorldBuilder supplies a linear runtime RGBA32 normal texture with R/G near0.5, B1 and A1, rather than an imported compressed normal asset. The installed URP UnpackNormalMapRGorAG path multiplies A by R before AG decoding, which supports that packing. Its ASTC-only AG path would interpret alpha differently. The compiled native variant is not proven by this source note. Compare normal-disabled against BumpScale0 with original keywords; do not infer a normal-encoding cause from the source alone.";
            report.variantScope="Map-disabled and fresh-material conditions change shader keywords and can be affected by build-time stripping. The original-keyword normal-scale-zero and constant-mask controls preserve the baseline combinations. Material.shader.isSupported and recorded keywords do not certify availability of every compiled variant; native images must be reviewed for fallback or missing-shader output.";
            if(casterMode)
            {
                report.scope="Actual normal-physics race reaches the recorded target-progress window, then the existing rendered-pose freeze is used. Only shadowCastingMode of the three named road-fixture renderers changes: baseline, fixture casters off, restored baseline. Visible geometry, meshes/colliders, mesh attributes, materials, all light settings, camera and postprocessing remain unchanged. Caster and light state is read back for each view and caster restoration is checked finally. This isolates a caster family at one native pose; completion is not root-cause or visual acceptance.";
                report.variantScope="Caster mode creates no material and changes no shader keyword.";
            }
            Save("RUNNING\n");
            try
            {
                Time.captureFramerate=0; Time.timeScale=1; player.AutopilotForTesting=true;
                Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(2.5f);
                bootstrap.Director.StartRace();
                float deadline=Time.realtimeSinceStartup+180f; bool reached=false;
                while(Time.realtimeSinceStartup<deadline)
                {
                    yield return new WaitForEndOfFrame();
                    if(bootstrap.Director.Phase==RacePhase.Racing && bootstrap.Director.RaceTime>10f && player.TrackProgress>=targetProgress && player.TrackProgress<=Mathf.Min(1f,targetProgress+.006f))
                    { reached=true; break; }
                    if(bootstrap.Director.Phase==RacePhase.Finished) break;
                }
                if(!reached) Fail("The native player did not reach the requested road-progress window.");
                else
                {
                    var roadObject=GameObject.Find("Running surface"); road=roadObject?roadObject.GetComponent<MeshRenderer>():null;
                    if(!road || road.sharedMaterials.Length!=1 || !road.sharedMaterial) Fail("Expected the existing single-material Running surface renderer.");
                    else
                    {
                        originalMaterials=road.sharedMaterials;
                        roadMesh=road.GetComponent<MeshFilter>().sharedMesh;originalNormals=roadMesh.normals;
                        roadCollider=road.GetComponent<MeshCollider>();originalColliderMesh=roadCollider?roadCollider.sharedMesh:null;
                        FreezeRenderedPose();
                        for(int i=0;i<4;i++) yield return null;
                        RecordSetup();
                        if(casterMode) ResolveCasters();
                        var conditions=BuildConditions(); report.expectedViews=conditions.Count;
                        foreach(var condition in conditions)
                        {
                            if(report.error!=null) break;
                            yield return CaptureCondition(condition);
                        }
                        report.complete=report.error==null && report.views.Count==report.expectedViews;
                    }
                }
            }
            finally
            {
                Restore(); report.complete=report.complete && report.error==null; report.finishedUtc=DateTime.UtcNow.ToString("o");
                Save(report.complete?"COMPLETE — matched native diagnostic images; cause and quality require review.\n":"INCOMPLETE — "+report.error+"\n");
            }
            yield return new WaitForSecondsRealtime(.75f); Application.Quit(report.complete?0:1);
        }

        void FreezeRenderedPose()
        {
            report.raceTime=bootstrap.Director.RaceTime; report.progress=player.TrackProgress; report.speedKphBeforeFreeze=player.SpeedKph;
            report.playerPosition=player.transform.position; report.playerRotation=player.transform.rotation;
            Time.timeScale=0; cameraPosition=bootstrap.Camera.transform.position; cameraRotation=bootstrap.Camera.transform.rotation;
            Disable(bootstrap.Camera.GetComponent<ChaseCamera>()); Disable(bootstrap.Director);
            foreach(var racer in bootstrap.Director.Racers)
            {
                if(!racer) continue;
                Disable(racer); Disable(racer.GetComponent<IonPropulsion>()); Disable(racer.GetComponent<VehicleVFX>());
                if(racer.Body) { var state=new BodyState(racer.Body); bodies.Add(state); state.Freeze(); }
            }
            foreach(var light in FindObjectsByType<Light>(FindObjectsSortMode.None)) lightStates.Add(new LightState(light));
            Physics.SyncTransforms();
        }

        List<Condition> BuildConditions()
        {
            if(casterMode) return new List<Condition>{new Condition("00-baseline",null),new Condition("01-fixture-casters-off",null,castersOff:true),new Condition("02-baseline-restored",null)};
            Material original=originalMaterials[0];
            var smooth=Copy("Constant smoothness, map disabled"); Constant(smooth);
            var normal=Copy("Normal mapping disabled"); NoNormal(normal);
            var both=Copy("Constant smoothness and no normal map"); Constant(both); NoNormal(both);
            var calibrated=new Material(original.shader){name="Calibrated simple URP Lit control"}; ownedMaterials.Add(calibrated);
            calibrated.SetColor("_BaseColor",original.GetColor("_BaseColor")); calibrated.SetColor("_EmissionColor",Color.black);
            calibrated.SetFloat("_Metallic",0); calibrated.SetFloat("_Smoothness",ConstantSmoothness);
            calibrated.SetFloat("_EnvironmentReflections",0); calibrated.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            calibrated.SetFloat("_SpecularHighlights",1); calibrated.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            var normalScale=Copy("Original keywords, normal scale zero"); normalScale.SetFloat("_BumpScale",0);
            var smoothMask=Copy("Original keywords, constant smoothness mask");
            constantMask=new Texture2D(1,1,TextureFormat.RGBA32,false,true){name="Diagnostic constant dampness mask"};
            float multiplier=original.GetFloat("_Smoothness");
            constantMask.SetPixel(0,0,new Color(0,0,0,ConstantSmoothness/Mathf.Max(multiplier,.0001f))); constantMask.Apply(false,true);
            smoothMask.SetTexture("_MetallicGlossMap",constantMask);
            var result=new List<Condition> {
                new Condition("00-baseline",null),new Condition("01-constant-smoothness-map-disabled",smooth),
                new Condition("02-normal-disabled",normal),new Condition("03-both-disabled",both),
                new Condition("04-calibrated-simple-Lit",calibrated),new Condition("05-normal-scale-zero-original-keywords",normalScale),
                new Condition("06-constant-mask-original-keywords",smoothMask),new Condition("07-no-additional-lights",null,new Light[0])
            };
            Light[] selected=SelectLights();
            for(int i=0;i<selected.Length;i++) result.Add(new Condition("08-selected-light-"+(i+1),null,new[]{selected[i]}));
            if(selected.Length>0) result.Add(new Condition("09-selected-lights-combined",null,selected));
            result.Add(new Condition("11-flat-normals-neutral-bump",normalScale,null,1));
            result.Add(new Condition("12-continuous-normals-neutral-bump",normalScale,null,2));
            result.Add(new Condition("13-flat-normals-calibrated",calibrated,null,1));
            result.Add(new Condition("14-continuous-normals-original-material",null,null,2));
            result.Add(new Condition("15-main-only-no-sun-shadows",null,new Light[0],0,true));
            result.Add(new Condition("10-baseline-restored",null)); return result;
        }
        Material Copy(string name) { var material=new Material(originalMaterials[0]){name=name}; ownedMaterials.Add(material); return material; }
        static void Constant(Material material) { material.SetTexture("_MetallicGlossMap",null); material.DisableKeyword("_METALLICSPECGLOSSMAP"); material.SetFloat("_Metallic",0); material.SetFloat("_Smoothness",ConstantSmoothness); }
        static void NoNormal(Material material) { material.SetTexture("_BumpMap",null); material.DisableKeyword("_NORMALMAP"); }

        IEnumerator CaptureCondition(Condition condition)
        {
            RestoreCondition();
            try
            {
                if(condition.castersOff) foreach(var state in casters) if(state.renderer)state.renderer.shadowCastingMode=ShadowCastingMode.Off;
                if(condition.material) road.sharedMaterial=condition.material;
                if(condition.noSunShadows && RenderSettings.sun)RenderSettings.sun.shadows=LightShadows.None;
                if(condition.normalMode!=0){
                    var normals=new Vector3[originalNormals.Length];
                    for(int index=0;index<normals.Length;index++){
                        if(condition.normalMode==1){normals[index]=Vector3.up;continue;}
                        int row=index/13,column=index%13;float progress=row/960f,lateral=Mathf.Lerp(-11,11,column/12f);
                        var before=bootstrap.Track.Evaluate(progress-.00005f);var after=bootstrap.Track.Evaluate(progress+.00005f);
                        var along=(after.Position+after.Right*lateral)-(before.Position+before.Right*lateral);
                        normals[index]=Vector3.Cross(along,bootstrap.Track.Evaluate(progress).Right).normalized;
                    }
                    roadMesh.normals=normals;
                }
                if(condition.lights!=null)
                    foreach(var state in lightStates) if(state.light && state.light.type!=LightType.Directional)
                        state.light.enabled=state.enabled && Array.IndexOf(condition.lights,state.light)>=0;
                for(int i=0;i<5;i++) yield return null;
                yield return new WaitForEndOfFrame();
                if((bootstrap.Camera.transform.position-cameraPosition).sqrMagnitude>.000001f || Quaternion.Angle(bootstrap.Camera.transform.rotation,cameraRotation)>.001f)
                { Fail("Frozen normal chase camera moved before "+condition.name); yield break; }
                foreach(var body in bodies) if(!body.IsFrozen()) { Fail("Frozen racer pose moved before "+condition.name); yield break; }
                if(casterMode && !VerifyCasterCondition(condition.castersOff))yield break;
                var view=new View { file=condition.name+".png",normalMode=condition.normalMode,noSunShadows=condition.noSunShadows,material=ReadMaterial(road.sharedMaterial),cameraPosition=bootstrap.Camera.transform.position,
                    cameraRotation=bootstrap.Camera.transform.rotation,width=Screen.width,height=Screen.height,raceTime=bootstrap.Director.RaceTime,progress=player.TrackProgress };
                view.fixtureCastersOff=condition.castersOff;
                if(casterMode){foreach(var state in casters)view.casters.Add(state.Read());view.lights=ReadLights();view.roadMeshEntityId=roadMesh.GetEntityId().ToString();view.roadColliderMeshEntityId=originalColliderMesh?originalColliderMesh.GetEntityId().ToString():string.Empty;}
                foreach(var state in lightStates) if(state.light && state.light.enabled && state.light.gameObject.activeInHierarchy && state.light.type!=LightType.Directional) view.enabledAdditionalLights.Add(Hierarchy(state.light.transform));
                string path=Path.Combine(folder,view.file); DateTime requested=DateTime.UtcNow;
                ScreenCapture.CaptureScreenshot(path); float deadline=Time.realtimeSinceStartup+8f;
                do { yield return null; } while((!File.Exists(path) || File.GetLastWriteTimeUtc(path)<requested) && Time.realtimeSinceStartup<deadline);
                if(!File.Exists(path) || File.GetLastWriteTimeUtc(path)<requested) { Fail("Screenshot was not written: "+view.file); yield break; }
                report.views.Add(view); Save("RUNNING\n");
            }
            finally { RestoreCondition(); }
        }

        Light[] SelectLights()
        {
            var frame=bootstrap.Track.Evaluate(report.progress);
            report.lightSamplePoints=new[]{frame.Position+frame.Right*7-frame.Forward*6,frame.Position+frame.Right*7+frame.Forward*8,frame.Position+frame.Right*7+frame.Forward*22};
            var ranked=new List<KeyValuePair<Light,float>>();
            foreach(var state in lightStates)
            {
                var light=state.light; if(!light || !state.enabled || !light.gameObject.activeInHierarchy || light.type==LightType.Directional) continue;
                float score=0;
                foreach(var point in report.lightSamplePoints)
                {
                    Vector3 delta=point-light.transform.position; float distance=delta.magnitude;
                    if(distance>=light.range || light.range<=0) continue;
                    float attenuation=Mathf.Pow(1-distance*distance/(light.range*light.range),2)/Mathf.Max(.25f,delta.sqrMagnitude);
                    if(light.type==LightType.Spot) attenuation*=Mathf.Pow(Mathf.InverseLerp(Mathf.Cos(light.spotAngle*.5f*Mathf.Deg2Rad),Mathf.Cos(light.innerSpotAngle*.5f*Mathf.Deg2Rad),Vector3.Dot(light.transform.forward,delta.normalized)),2);
                    score=Mathf.Max(score,attenuation*light.intensity*light.color.maxColorComponent);
                }
                if(score>0) ranked.Add(new KeyValuePair<Light,float>(light,score));
            }
            ranked.Sort((a,b)=>b.Value.CompareTo(a.Value)); int count=Mathf.Min(2,ranked.Count); var selected=new Light[count];
            for(int i=0;i<count;i++) { selected[i]=ranked[i].Key; report.selectedLights.Add(Hierarchy(selected[i].transform)); }
            return selected;
        }

        void RecordSetup()
        {
            var camera=bootstrap.Camera; report.cameraPosition=cameraPosition; report.cameraRotation=cameraRotation;
            report.fieldOfView=camera.fieldOfView; report.nearClip=camera.nearClipPlane; report.farClip=camera.farClipPlane; report.cameraAspect=camera.aspect;
            report.projectionMatrix=new float[16]; report.worldToCameraMatrix=new float[16];
            for(int i=0;i<16;i++) { report.projectionMatrix[i]=camera.projectionMatrix[i]; report.worldToCameraMatrix[i]=camera.worldToCameraMatrix[i]; }
            report.originalMaterial=ReadMaterial(originalMaterials[0]); report.roadPath=Hierarchy(road.transform);
            report.roadPosition=road.transform.position; report.roadRotation=road.transform.rotation; report.roadScale=road.transform.lossyScale;
            Mesh mesh=road.GetComponent<MeshFilter>().sharedMesh;
            report.meshName=mesh.name; report.vertexCount=mesh.vertexCount; report.subMeshCount=mesh.subMeshCount;
            report.normalCount=mesh.normals.Length; report.tangentCount=mesh.tangents.Length; report.uvCount=mesh.uv.Length;
            report.meshBoundsCenter=mesh.bounds.center; report.meshBoundsSize=mesh.bounds.size;
            for(int i=0;i<mesh.subMeshCount;i++) if(mesh.GetTopology(i)==MeshTopology.Triangles) report.triangleCount+=(long)mesh.GetIndexCount(i)/3;
            var uvs=mesh.uv; if(uvs.Length>0) { report.uvMin=report.uvMax=uvs[0]; foreach(var uv in uvs) { report.uvMin=Vector2.Min(report.uvMin,uv); report.uvMax=Vector2.Max(report.uvMax,uv); } }
            report.meshSourceNote="Original road: shared-vertex 960-row,12-column ribbon with recalculated normals/tangents. Diagnostic normalMode0 preserves original attributes; mode1 supplies global up normals (diagnostic only, wrong bank); mode2 estimates continuous ribbon normals from Track.Evaluate(progress +/-0.00005), including lateral offset, crossed with local right. Only normals change; vertices, topology, UVs and tangents stay fixed. Each condition restores original normals. Counts and UV bounds are read from the actual mesh.";
            if(GraphicsSettings.currentRenderPipeline) report.pipelineJson=JsonUtility.ToJson(GraphicsSettings.currentRenderPipeline,true);
            var cameraData=camera.GetUniversalAdditionalCameraData(); report.cameraUrpJson=JsonUtility.ToJson(cameraData,true);
            foreach(var data in Resources.FindObjectsOfTypeAll<UniversalRendererData>())
            {
                report.rendererSettings.Add(JsonUtility.ToJson(data,true));
                foreach(var feature in data.rendererFeatures) if(feature) report.rendererFeatures.Add(JsonUtility.ToJson(feature,true));
            }
            foreach(var state in lightStates)
            {
                var light=state.light; report.originalLights.Add(new LightInfo { path=Hierarchy(light.transform),type=light.type.ToString(),enabled=state.enabled,
                    position=light.transform.position,rotation=light.transform.rotation,color=light.color,intensity=light.intensity,range=light.range,
                    spotAngle=light.spotAngle,innerSpotAngle=light.innerSpotAngle,shadows=light.shadows.ToString(),cullingMask=light.cullingMask });
            }
            if(casterMode)originalLightSnapshot=JsonUtility.ToJson(ReadLights());
        }

        void ResolveCasters()
        {
            var renderers=FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(string name in CasterNames)
            {
                MeshRenderer found=null;int count=0;
                foreach(var candidate in renderers)if(candidate.name==name){found=candidate;count++;}
                if(count!=1 || !found || !found.enabled || !found.gameObject.activeInHierarchy)
                {Fail("Expected exactly one active enabled fixture renderer: "+name+"; found "+count);return;}
                var state=new CasterState(found);casters.Add(state);report.originalCasters.Add(state.Read());
            }
        }
        bool VerifyCasterCondition(bool off)
        {
            foreach(var state in casters)if(!state.Matches(off)){Fail("Fixture state changed beyond requested shadow casting: "+state.path);return false;}
            if(casters.Count!=CasterNames.Length){Fail("Incomplete fixture-caster family.");return false;}
            if(!road || road.GetComponent<MeshFilter>().sharedMesh!=roadMesh || road.sharedMaterial!=originalMaterials[0] ||
                (roadCollider && roadCollider.sharedMesh!=originalColliderMesh))
            {Fail("Road mesh, collider or material identity changed during caster control.");return false;}
            if(originalLightSnapshot!=JsonUtility.ToJson(ReadLights())){Fail("A light setting changed during caster control.");return false;}
            return true;
        }
        LightSnapshot ReadLights()
        {
            var snapshot=new LightSnapshot{sunEntityId=RenderSettings.sun?RenderSettings.sun.GetEntityId().ToString():string.Empty};
            foreach(var state in lightStates)if(state.light)
            {
                var light=state.light;snapshot.states.Add(new LightInfo{path=Hierarchy(light.transform),type=light.type.ToString(),enabled=light.enabled,
                    position=light.transform.position,rotation=light.transform.rotation,color=light.color,intensity=light.intensity,range=light.range,
                    spotAngle=light.spotAngle,innerSpotAngle=light.innerSpotAngle,shadows=light.shadows.ToString(),shadowStrength=light.shadowStrength,cullingMask=light.cullingMask});
            }
            return snapshot;
        }

        static MaterialInfo ReadMaterial(Material material)
        {
            var value=new MaterialInfo { name=material.name,shader=material.shader?material.shader.name:"missing",shaderSupported=material.shader && material.shader.isSupported,keywords=material.shaderKeywords };
            foreach(string property in new[]{"_Smoothness","_Metallic","_BumpScale","_EnvironmentReflections","_SpecularHighlights","_WorkflowMode","_SmoothnessTextureChannel","_Surface"})
                if(material.HasProperty(property)) value.floats.Add(new FloatInfo { property=property,value=material.GetFloat(property) });
            if(material.HasProperty("_BaseColor")) value.baseColor=material.GetColor("_BaseColor");
            if(material.HasProperty("_EmissionColor")) value.emission=material.GetColor("_EmissionColor");
            foreach(string property in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap"})
            {
                if(!material.HasProperty(property)) continue; var texture=material.GetTexture(property);
                var entry=new TextureInfo { property=property,name=texture?texture.name:"null",scale=material.GetTextureScale(property),offset=material.GetTextureOffset(property) };
                if(texture) { entry.width=texture.width; entry.height=texture.height; entry.graphicsFormat=texture.graphicsFormat.ToString(); entry.filter=texture.filterMode.ToString(); entry.wrap=texture.wrapMode.ToString(); entry.aniso=texture.anisoLevel; }
                if(texture is Texture2D image) { entry.mipCount=image.mipmapCount; entry.readable=image.isReadable; entry.format=image.format.ToString(); }
                value.textures.Add(entry);
            }
            return value;
        }
        static string Hierarchy(Transform value) { string path=value.name+"["+value.GetSiblingIndex()+"]"; for(var parent=value.parent;parent;parent=parent.parent) path=parent.name+"["+parent.GetSiblingIndex()+"]/"+path; return path; }
        void Disable(Behaviour value) { if(value && value.enabled) { disabled.Add(value); value.enabled=false; } }
        void RestoreCondition() { if(casterMode){foreach(var state in casters)state.Restore();return;} if(road && originalMaterials!=null) road.sharedMaterials=originalMaterials; if(roadMesh && originalNormals!=null)roadMesh.normals=originalNormals; foreach(var state in lightStates) if(state.light){state.light.enabled=state.enabled;state.light.shadows=state.shadows;} }
        void Restore()
        {
            if(!initialized) return; initialized=false; RestoreCondition();
            if(casterMode && casters.Count==CasterNames.Length)
            {report.casterRestorationVerified=VerifyCasterCondition(false);foreach(var state in casters)if(state.renderer)report.restoredCasters.Add(state.Read());}
            foreach(var body in bodies) body.Restore(); Physics.SyncTransforms();
            foreach(var value in disabled) if(value) value.enabled=true;
            if(player) player.AutopilotForTesting=originalAutopilot;
            Time.captureFramerate=originalCaptureRate; Time.timeScale=originalTimeScale;
            Screen.SetResolution(originalWidth,originalHeight,originalScreenMode);
            foreach(var material in ownedMaterials) if(material) Destroy(material); if(constantMask) Destroy(constantMask);
        }
        void OnDestroy() { Restore(); }
        void OnApplicationQuit() { Restore(); }
        void Fail(string message) { if(report.error==null) report.error=message; Debug.LogError("Road surface evidence: "+message); }
        void Save(string status) { File.WriteAllText(Path.Combine(folder,"road-surface-evidence.json"),JsonUtility.ToJson(report,true)); File.WriteAllText(Path.Combine(folder,"road-surface-status.txt"),status); }
        sealed class Condition { public readonly string name; public readonly Material material; public readonly Light[] lights;public readonly int normalMode;public readonly bool noSunShadows,castersOff; public Condition(string name,Material material,Light[] lights=null,int normalMode=0,bool noSunShadows=false,bool castersOff=false) { this.name=name;this.material=material;this.lights=lights;this.normalMode=normalMode;this.noSunShadows=noSunShadows;this.castersOff=castersOff; } }
        sealed class CasterState
        {
            public readonly MeshRenderer renderer;public readonly string path;
            readonly ShadowCastingMode mode;readonly Mesh mesh;readonly Material[] materials;readonly Vector3 position,scale;readonly Quaternion rotation;
            public CasterState(MeshRenderer value){renderer=value;path=Hierarchy(value.transform);mode=value.shadowCastingMode;mesh=value.GetComponent<MeshFilter>()?value.GetComponent<MeshFilter>().sharedMesh:null;materials=value.sharedMaterials;position=value.transform.position;rotation=value.transform.rotation;scale=value.transform.lossyScale;}
            public void Restore(){if(renderer)renderer.shadowCastingMode=mode;}
            public bool Matches(bool off)
            {
                if(!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy || renderer.shadowCastingMode!=(off?ShadowCastingMode.Off:mode) ||
                    renderer.transform.position!=position || Quaternion.Angle(renderer.transform.rotation,rotation)>.001f || renderer.transform.lossyScale!=scale ||
                    !renderer.GetComponent<MeshFilter>() || renderer.GetComponent<MeshFilter>().sharedMesh!=mesh)return false;
                var current=renderer.sharedMaterials;if(current.Length!=materials.Length)return false;for(int i=0;i<current.Length;i++)if(current[i]!=materials[i])return false;return true;
            }
            public CasterInfo Read(){var result=new CasterInfo{path=path,entityId=renderer.GetEntityId().ToString(),enabled=renderer.enabled,active=renderer.gameObject.activeInHierarchy,shadowCastingMode=renderer.shadowCastingMode.ToString(),position=renderer.transform.position,rotation=renderer.transform.rotation,scale=renderer.transform.lossyScale,meshEntityId=mesh?mesh.GetEntityId().ToString():string.Empty};foreach(var material in renderer.sharedMaterials)result.materialEntityIds.Add(material?material.GetEntityId().ToString():string.Empty);return result;}
        }
        sealed class LightState { public readonly Light light; public readonly bool enabled;public readonly LightShadows shadows; public LightState(Light value) { light=value;enabled=value.enabled;shadows=value.shadows; } }
        sealed class BodyState
        {
            readonly Rigidbody body; readonly Vector3 position,velocity,angularVelocity,frozenPosition; readonly Quaternion rotation,frozenRotation;
            readonly RigidbodyInterpolation interpolation; readonly bool kinematic;
            public BodyState(Rigidbody value) { body=value;position=value.position;rotation=value.rotation;velocity=value.linearVelocity;angularVelocity=value.angularVelocity;frozenPosition=value.transform.position;frozenRotation=value.transform.rotation;interpolation=value.interpolation;kinematic=value.isKinematic; }
            public void Freeze() { body.interpolation=RigidbodyInterpolation.None;body.isKinematic=true;body.position=frozenPosition;body.rotation=frozenRotation;body.transform.SetPositionAndRotation(frozenPosition,frozenRotation); }
            public bool IsFrozen() { return body && (body.transform.position-frozenPosition).sqrMagnitude<.000001f && Quaternion.Angle(body.transform.rotation,frozenRotation)<.001f; }
            public void Restore() { if(!body)return;body.isKinematic=false;body.position=position;body.rotation=rotation;body.linearVelocity=velocity;body.angularVelocity=angularVelocity;body.isKinematic=kinematic;body.transform.SetPositionAndRotation(position,rotation);body.interpolation=interpolation; }
        }
        [Serializable] sealed class Report
        {
            public string scope,normalEncodingSourceNote,variantScope,meshSourceNote,startedUtc,finishedUtc,unityVersion,gpu,graphicsApi,error,pipelineJson,cameraUrpJson,roadPath,meshName,buildGuid,mode;
            public float targetProgress;public bool casterRestorationVerified;
            public List<CasterInfo> originalCasters=new List<CasterInfo>(),restoredCasters=new List<CasterInfo>();
            public bool complete; public int expectedViews,vertexCount,normalCount,tangentCount,uvCount,subMeshCount; public long triangleCount;
            public float raceTime,progress,speedKphBeforeFreeze,fieldOfView,nearClip,farClip,cameraAspect;
            public Vector3 playerPosition,cameraPosition,roadPosition,roadScale,meshBoundsCenter,meshBoundsSize; public Quaternion playerRotation,cameraRotation,roadRotation;
            public Vector2 uvMin,uvMax; public float[] projectionMatrix,worldToCameraMatrix; public Vector3[] lightSamplePoints;
            public MaterialInfo originalMaterial;
            public List<string> rendererSettings=new List<string>(),rendererFeatures=new List<string>(),selectedLights=new List<string>();
            public List<LightInfo> originalLights=new List<LightInfo>(); public List<View> views=new List<View>();
        }
        [Serializable] sealed class View { public string file;public int normalMode;public string roadMeshEntityId,roadColliderMeshEntityId;public bool noSunShadows,fixtureCastersOff;public List<CasterInfo> casters=new List<CasterInfo>();public LightSnapshot lights;public MaterialInfo material;public Vector3 cameraPosition;public Quaternion cameraRotation;public int width,height;public float raceTime,progress;public List<string> enabledAdditionalLights=new List<string>(); }
        [Serializable] sealed class CasterInfo {public string path,shadowCastingMode;public string entityId,meshEntityId;public bool enabled,active;public Vector3 position,scale;public Quaternion rotation;public List<string> materialEntityIds=new List<string>();}
        [Serializable] sealed class LightSnapshot {public string sunEntityId;public List<LightInfo> states=new List<LightInfo>();}
        [Serializable] sealed class MaterialInfo { public string name,shader;public bool shaderSupported;public string[] keywords;public Color baseColor,emission;public List<FloatInfo> floats=new List<FloatInfo>();public List<TextureInfo> textures=new List<TextureInfo>(); }
        [Serializable] sealed class FloatInfo { public string property;public float value; }
        [Serializable] sealed class TextureInfo { public string property,name,graphicsFormat,format,filter,wrap;public int width,height,mipCount,aniso;public bool readable;public Vector2 scale,offset; }
        [Serializable] sealed class LightInfo { public string path,type,shadows;public bool enabled;public Vector3 position;public Quaternion rotation;public Color color;public float intensity,range,spotAngle,innerSpotAngle,shadowStrength;public int cullingMask; }
    }
}
