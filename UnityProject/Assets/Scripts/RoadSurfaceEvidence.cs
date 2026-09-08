using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    /// <summary>Opt-in matched native road-material/light diagnosis; never modifies the authored road.</summary>
    public sealed class RoadSurfaceEvidence : MonoBehaviour
    {
        const float TargetProgress=.88475f, ConstantSmoothness=.56f;
        VectorBootstrap bootstrap;
        HoverVehicle player;
        MeshRenderer road;
        Material[] originalMaterials;
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
            if(index+1>=args.Length || !Path.IsPathRooted(args[index+1]))
            { Debug.LogError("Road surface evidence requires -roadSurfaceEvidence <absolute output folder>."); return false; }
            if(!owner || !owner.Director || !owner.Director.Player || !owner.Camera)
            { Debug.LogError("Road surface evidence requires the initialized native race."); return false; }
            foreach(var evidence in owner.GetComponents<RaceEvidence>()) { evidence.StopAllCoroutines(); evidence.enabled=false; }
            var runner=owner.gameObject.AddComponent<RoadSurfaceEvidence>(); runner.bootstrap=owner;
            runner.player=owner.Director.Player; runner.folder=Path.GetFullPath(args[index+1]);
            runner.StartCoroutine(runner.Run()); return true;
        }

        IEnumerator Run()
        {
            Directory.CreateDirectory(folder);
            originalTimeScale=Time.timeScale; originalCaptureRate=Time.captureFramerate; originalAutopilot=player.AutopilotForTesting;
            originalWidth=Screen.width; originalHeight=Screen.height; originalScreenMode=Screen.fullScreenMode; initialized=true;
            report.startedUtc=DateTime.UtcNow.ToString("o"); report.unityVersion=Application.unityVersion;
            report.gpu=SystemInfo.graphicsDeviceName; report.graphicsApi=SystemInfo.graphicsDeviceType.ToString();
            report.scope="Actual native ScreenCapture images at the first gallery passage near progress0.88475. Existing player testing autopilot reaches the pose through normal physics, with captureFramerate0. After LateUpdate/end-of-frame, the real chase camera and all racers are frozen in their rendered poses; Rigidbody interpolation is disabled only for stable matching. Each material/light condition restores the original before the next. Geometry, UVs, normals, tangents, camera, postprocessing, light positions and intensities stay fixed. The calibrated control is URP Lit with no maps, base color equal to the original, metallic0, smoothness0.56 and environment reflections off. Single-light controls retain the original directional lights and ambient, changing only which originally enabled non-directional lights are enabled. Selection is an approximate attenuation ranking at three right-side road points, not a measured BRDF contribution. These images diagnose the artifact; capture completion does not identify its cause or establish visual acceptance.";
            report.normalEncodingSourceNote="WorldBuilder supplies a linear runtime RGBA32 normal texture with R/G near0.5, B1 and A1, rather than an imported compressed normal asset. The installed URP UnpackNormalMapRGorAG path multiplies A by R before AG decoding, which supports that packing. Its ASTC-only AG path would interpret alpha differently. The compiled native variant is not proven by this source note. Compare normal-disabled against BumpScale0 with original keywords; do not infer a normal-encoding cause from the source alone.";
            report.variantScope="Map-disabled and fresh-material conditions change shader keywords and can be affected by build-time stripping. The original-keyword normal-scale-zero and constant-mask controls preserve the baseline combinations. Material.shader.isSupported and recorded keywords do not certify availability of every compiled variant; native images must be reviewed for fallback or missing-shader output.";
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
                    if(bootstrap.Director.Phase==RacePhase.Racing && bootstrap.Director.RaceTime>10f && player.TrackProgress>=TargetProgress && player.TrackProgress<=TargetProgress+.006f)
                    { reached=true; break; }
                    if(bootstrap.Director.Phase==RacePhase.Finished) break;
                }
                if(!reached) Fail("The native player did not reach the requested gallery window.");
                else
                {
                    var roadObject=GameObject.Find("Running surface"); road=roadObject?roadObject.GetComponent<MeshRenderer>():null;
                    if(!road || road.sharedMaterials.Length!=1 || !road.sharedMaterial) Fail("Expected the existing single-material Running surface renderer.");
                    else
                    {
                        originalMaterials=road.sharedMaterials;
                        FreezeRenderedPose();
                        for(int i=0;i<4;i++) yield return null;
                        RecordSetup();
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
                Restore(); report.finishedUtc=DateTime.UtcNow.ToString("o");
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
                if(condition.material) road.sharedMaterial=condition.material;
                if(condition.lights!=null)
                    foreach(var state in lightStates) if(state.light && state.light.type!=LightType.Directional)
                        state.light.enabled=state.enabled && Array.IndexOf(condition.lights,state.light)>=0;
                for(int i=0;i<5;i++) yield return null;
                yield return new WaitForEndOfFrame();
                if((bootstrap.Camera.transform.position-cameraPosition).sqrMagnitude>.000001f || Quaternion.Angle(bootstrap.Camera.transform.rotation,cameraRotation)>.001f)
                { Fail("Frozen normal chase camera moved before "+condition.name); yield break; }
                foreach(var body in bodies) if(!body.IsFrozen()) { Fail("Frozen racer pose moved before "+condition.name); yield break; }
                var view=new View { file=condition.name+".png",material=ReadMaterial(road.sharedMaterial),cameraPosition=bootstrap.Camera.transform.position,
                    cameraRotation=bootstrap.Camera.transform.rotation,width=Screen.width,height=Screen.height,raceTime=bootstrap.Director.RaceTime,progress=player.TrackProgress };
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
            report.meshSourceNote="WorldBuilder creates a shared-vertex 960-row,12-column running ribbon, then RecalculateNormals/RecalculateTangents. This harness does not change mesh attributes. Counts and UV bounds above are read from the actual runtime mesh.";
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
        void RestoreCondition() { if(road && originalMaterials!=null) road.sharedMaterials=originalMaterials; foreach(var state in lightStates) if(state.light) state.light.enabled=state.enabled; }
        void Restore()
        {
            if(!initialized) return; initialized=false; RestoreCondition();
            foreach(var body in bodies) body.Restore(); Physics.SyncTransforms();
            foreach(var value in disabled) if(value) value.enabled=true;
            if(player) player.AutopilotForTesting=originalAutopilot;
            Time.captureFramerate=originalCaptureRate; Time.timeScale=originalTimeScale;
            Screen.SetResolution(originalWidth,originalHeight,originalScreenMode);
            foreach(var material in ownedMaterials) if(material) Destroy(material); if(constantMask) Destroy(constantMask);
        }
        void OnDestroy() { Restore(); }
        void Fail(string message) { if(report.error==null) report.error=message; Debug.LogError("Road surface evidence: "+message); }
        void Save(string status) { File.WriteAllText(Path.Combine(folder,"road-surface-evidence.json"),JsonUtility.ToJson(report,true)); File.WriteAllText(Path.Combine(folder,"road-surface-status.txt"),status); }
        sealed class Condition { public readonly string name; public readonly Material material; public readonly Light[] lights; public Condition(string name,Material material,Light[] lights=null) { this.name=name;this.material=material;this.lights=lights; } }
        sealed class LightState { public readonly Light light; public readonly bool enabled; public LightState(Light value) { light=value;enabled=value.enabled; } }
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
            public string scope,normalEncodingSourceNote,variantScope,meshSourceNote,startedUtc,finishedUtc,unityVersion,gpu,graphicsApi,error,pipelineJson,cameraUrpJson,roadPath,meshName;
            public bool complete; public int expectedViews,vertexCount,normalCount,tangentCount,uvCount,subMeshCount; public long triangleCount;
            public float raceTime,progress,speedKphBeforeFreeze,fieldOfView,nearClip,farClip,cameraAspect;
            public Vector3 playerPosition,cameraPosition,roadPosition,roadScale,meshBoundsCenter,meshBoundsSize; public Quaternion playerRotation,cameraRotation,roadRotation;
            public Vector2 uvMin,uvMax; public float[] projectionMatrix,worldToCameraMatrix; public Vector3[] lightSamplePoints;
            public MaterialInfo originalMaterial;
            public List<string> rendererSettings=new List<string>(),rendererFeatures=new List<string>(),selectedLights=new List<string>();
            public List<LightInfo> originalLights=new List<LightInfo>(); public List<View> views=new List<View>();
        }
        [Serializable] sealed class View { public string file;public MaterialInfo material;public Vector3 cameraPosition;public Quaternion cameraRotation;public int width,height;public float raceTime,progress;public List<string> enabledAdditionalLights=new List<string>(); }
        [Serializable] sealed class MaterialInfo { public string name,shader;public bool shaderSupported;public string[] keywords;public Color baseColor,emission;public List<FloatInfo> floats=new List<FloatInfo>();public List<TextureInfo> textures=new List<TextureInfo>(); }
        [Serializable] sealed class FloatInfo { public string property;public float value; }
        [Serializable] sealed class TextureInfo { public string property,name,graphicsFormat,format,filter,wrap;public int width,height,mipCount,aniso;public bool readable;public Vector2 scale,offset; }
        [Serializable] sealed class LightInfo { public string path,type,shadows;public bool enabled;public Vector3 position;public Quaternion rotation;public Color color;public float intensity,range,spotAngle,innerSpotAngle;public int cullingMask; }
    }
}
