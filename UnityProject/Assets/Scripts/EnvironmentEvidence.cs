using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Actual fixed-simulation race footage with complete camera/vehicle pose records.</summary>
    public sealed class EnvironmentEvidence : MonoBehaviour
    {
        VectorBootstrap owner;
        string folder, referencePath;
        int originalCaptureRate;
        float originalTimeScale;
        bool originalAutopilot, initialized;
        bool fullLap,stillsOnly;
        readonly Report report = new Report();
        readonly float[] anchors = { .71626f, .84775f, .87269f, .89735f, .94585f };
        readonly string[] labels = { "01-approach", "02-entry", "03-middle", "04-exit", "05-reveal" };

        public static bool TryStart(VectorBootstrap bootstrap)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-environmentEvidence");
            if (i < 0) return false;
            if (i + 1 >= args.Length || !Path.IsPathRooted(args[i + 1])) {
                Debug.LogError("Environment evidence requires an absolute output directory."); return false;
            }
            var evidence = bootstrap.gameObject.AddComponent<EnvironmentEvidence>();
            evidence.owner = bootstrap; evidence.folder = args[i + 1];
            evidence.fullLap=Array.IndexOf(args,"-environmentFullLap")>=0;
            evidence.stillsOnly=Array.IndexOf(args,"-environmentStillsOnly")>=0;
            int reference = Array.IndexOf(args, "-environmentReference");
            if (reference >= 0 && reference + 1 < args.Length) evidence.referencePath = args[reference + 1];
            foreach (var old in bootstrap.GetComponents<RaceEvidence>()) { old.StopAllCoroutines(); old.enabled = false; }
            evidence.StartCoroutine(evidence.Run()); return true;
        }

        IEnumerator Run()
        {
            Directory.CreateDirectory(folder); Directory.CreateDirectory(Path.Combine(folder, "frames"));
            var director = owner.Director; var player = director.Player;
            originalCaptureRate = Time.captureFramerate; originalTimeScale = Time.timeScale;
            originalAutopilot = player.AutopilotForTesting; initialized = true;
            report.startedUtc = DateTime.UtcNow.ToString("o");
            report.unityVersion=Application.unityVersion;report.buildGuid=Application.buildGUID;report.gpu=SystemInfo.graphicsDeviceName;
            report.sceneRenderers=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length;
            foreach(var light in FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.enabled)report.activeLights++;
            report.scope = "Actual native racing camera and normal hover physics. Existing test autopilot drives; no camera, pose, lap, energy, speed or race phase is injected. Time.captureFramerate=24 is set before StartRace for repeatable simulation. The 360-frame passage is 15 simulation seconds, not a frame-time/performance measurement. Camera transforms, FOV, all racer rigidbody poses/velocities and player visual transform are recorded every frame. Five anchors are the first recorded frames crossing fixed track-progress thresholds. An optional prior report compares actual anchor poses; it never overwrites them. PNGs and metadata are rendering evidence, not independent visual/motion acceptance. The exported picture sequence contains no audio.";
            int frameCount=fullLap?1440:360;
            if(fullLap)report.scope=report.scope.Replace("The 360-frame passage is 15 simulation seconds","The 1440-frame full-circuit traversal is 60 simulation seconds");
            if(stillsOnly)report.scope+=" Stills-only mode saves the five anchor PNGs, while retaining every simulated frame's pose metadata; it is not a motion recording.";
            bool success = false;
            try {
                report.roadSurface=ReadRoadSurface();
                player.AutopilotForTesting = true; Time.timeScale = 1; Time.captureFramerate = 24;
                yield return new WaitForSecondsRealtime(2);
                director.StartRace();
                float deadline = Time.realtimeSinceStartup + 150;
                float startProgress=fullLap?.005f:anchors[0],startTime=fullLap?.1f:5;
                while ((director.Phase != RacePhase.Racing || director.RaceTime < startTime || player.TrackProgress < startProgress) && Time.realtimeSinceStartup < deadline) yield return null;
                if (director.Phase != RacePhase.Racing || player.TrackProgress < startProgress) throw new InvalidOperationException("Could not reach the benchmark through normal physics.");
                int nextAnchor = 0;
                // Grid staging can report .99 before the start-line crossing. Arm
                // full-lap anchors only after entering the early circuit.
                bool anchorsArmed=!fullLap || player.TrackProgress<anchors[0];
                for (int index = 0; index < frameCount; index++) {
                    yield return new WaitForEndOfFrame();
                    var frame = ReadFrame(index);
                    report.frames.Add(frame);
                    if(player.TrackProgress<anchors[0])anchorsArmed=true;
                    if (anchorsArmed && nextAnchor < anchors.Length && player.TrackProgress >= anchors[nextAnchor] && player.TrackProgress < .999f) {
                        frame.anchor = labels[nextAnchor]; frame.anchorFile = labels[nextAnchor] + ".png";
                        report.anchorFrames.Add(index); nextAnchor++;
                    }
                    frame.captureRequested=!stillsOnly || !string.IsNullOrEmpty(frame.anchor);
                    if(frame.captureRequested)ScreenCapture.CaptureScreenshot(Path.Combine(folder, frame.file));
                }
                // Allow the native screenshot writer to finish before validating files/copying anchors.
                yield return new WaitForSecondsRealtime(2);
                foreach (var frame in report.frames) {
                    if(!frame.captureRequested)continue;
                    string path = Path.Combine(folder, frame.file);
                    if (!File.Exists(path) || new FileInfo(path).Length < 24) throw new IOException("Missing native frame " + frame.file);
                    if (!string.IsNullOrEmpty(frame.anchorFile)) File.Copy(path, Path.Combine(folder, frame.anchorFile), true);
                }
                if(fullLap && (report.frames[report.frames.Count-1].lap<=report.frames[0].lap || report.frames[report.frames.Count-1].raceTime-report.frames[0].raceTime<40)) throw new InvalidOperationException("Full circuit was not completed during the traversal.");
                if (nextAnchor != 5) throw new InvalidOperationException("Only " + nextAnchor + " of five natural progress anchors reached.");
                if (!string.IsNullOrEmpty(referencePath)) CompareReference();
                report.complete = true; success = true;
            }
            finally {
                report.finishedUtc = DateTime.UtcNow.ToString("o");
                File.WriteAllText(Path.Combine(folder, "environment-evidence.json"), JsonUtility.ToJson(report, true));
                File.WriteAllText(Path.Combine(folder, "status.txt"), success ? "COMPLETE: "+frameCount+" simulated frames / 5 anchors; stillsOnly="+stillsOnly+"; review required.\n" : "INCOMPLETE: inspect native log.\n");
                Restore();
                if(!success)Application.Quit(1);
            }
            Application.Quit(success ? 0 : 1);
        }

        Frame ReadFrame(int index)
        {
            var player = owner.Director.Player; var cam = owner.Camera;
            var f = new Frame {
                index = index, file = "frames/frame-" + index.ToString("D4") + ".png",
                width = Screen.width, height = Screen.height, raceTime = owner.Director.RaceTime,
                progress = player.TrackProgress, speedKph = player.SpeedKph, energy = player.Boost01,
                boost = player.IsBoosting, grounded = player.IsGrounded, lap = owner.Director.Lap,
                cameraPosition = cam.transform.position, cameraRotation = cam.transform.rotation, fieldOfView = cam.fieldOfView,
                playerVisualPosition = player.VisualRoot.position, playerVisualRotation = player.VisualRoot.rotation,
                racers = new RacerPose[owner.Director.Racers.Count]
            };
            for (int i = 0; i < f.racers.Length; i++) {
                var racer = owner.Director.Racers[i];
                f.racers[i] = new RacerPose { name = racer.name, position = racer.Body.position, rotation = racer.Body.rotation,
                    velocity = racer.Body.linearVelocity, angularVelocity = racer.Body.angularVelocity, progress = racer.TrackProgress };
            }
            return f;
        }

        void CompareReference()
        {
            Report baseline = JsonUtility.FromJson<Report>(File.ReadAllText(referencePath));
            report.reference = referencePath;
            if (baseline.anchorFrames.Count != 5) throw new InvalidDataException("Reference needs five anchor frames.");
            report.matchedWithinTolerance = true;
            for (int i = 0; i < 5; i++) {
                Frame a = baseline.frames[baseline.anchorFrames[i]], b = report.frames[report.anchorFrames[i]];
                var comparison = new PoseComparison { anchor = b.anchor,
                    cameraDistance = Vector3.Distance(a.cameraPosition, b.cameraPosition),
                    cameraAngle = Quaternion.Angle(a.cameraRotation, b.cameraRotation),
                    fovDifference = Mathf.Abs(a.fieldOfView - b.fieldOfView),
                    playerDistance = Vector3.Distance(a.racers[0].position, b.racers[0].position),
                    progressDifference = Mathf.Abs(a.progress - b.progress) };
                comparison.matched = comparison.cameraDistance <= .05f && comparison.cameraAngle <= .1f && comparison.fovDifference <= .05f && comparison.playerDistance <= .05f;
                report.matchedWithinTolerance &= comparison.matched; report.comparisons.Add(comparison);
            }
        }

        RoadSurfaceInfo ReadRoadSurface()
        {
            var surface=GameObject.Find("Running surface");
            var renderer=surface?surface.GetComponent<MeshRenderer>():null;
            if(!renderer || !renderer.sharedMaterial)throw new InvalidOperationException("Running surface material is missing.");
            var material=renderer.sharedMaterial;
            var mask=material.GetTexture("_MetallicGlossMap");
            if(!mask)throw new InvalidOperationException("Running surface smoothness mask is missing.");
            var result=new RoadSurfaceInfo { material=material.name,shader=material.shader.name,
                keywords=material.shaderKeywords,smoothnessMultiplier=material.GetFloat("_Smoothness"),
                environmentReflections=material.GetFloat("_EnvironmentReflections"),
                bumpScale=material.GetFloat("_BumpScale"),metallic=material.GetFloat("_Metallic"),
                baseColor=material.GetColor("_BaseColor"),maskWidth=mask.width,maskHeight=mask.height };
            // Read the uploaded linear mask before racing; restore the active target and
            // release all temporary resources. No material or source texture is modified.
            var previous=RenderTexture.active;
            var target=RenderTexture.GetTemporary(mask.width,mask.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            var pixels=new Texture2D(mask.width,mask.height,TextureFormat.RGBA32,false,true);
            try {
                Graphics.Blit(mask,target);RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,mask.width,mask.height),0,0,false);
                int minimum=255,maximum=0;
                foreach(var pixel in pixels.GetPixels32()){minimum=Mathf.Min(minimum,pixel.a);maximum=Mathf.Max(maximum,pixel.a);}
                result.uploadedAlphaMinimum=minimum/255f;result.uploadedAlphaMaximum=maximum/255f;
                result.effectiveSmoothnessMinimum=result.uploadedAlphaMinimum*result.smoothnessMultiplier;
                result.effectiveSmoothnessMaximum=result.uploadedAlphaMaximum*result.smoothnessMultiplier;
            } finally { RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Destroy(pixels); }
            return result;
        }

        void Restore()
        {
            if (!initialized) return; initialized = false;
            if (owner && owner.Director && owner.Director.Player) owner.Director.Player.AutopilotForTesting = originalAutopilot;
            Time.captureFramerate = originalCaptureRate; Time.timeScale = originalTimeScale;
        }
        void OnDestroy() { Restore(); }
        [Serializable] public sealed class Report {
            public string scope, startedUtc, finishedUtc, reference,unityVersion,buildGuid,gpu;
            public int sceneRenderers,activeLights;
            public bool complete, matchedWithinTolerance;
            public RoadSurfaceInfo roadSurface;
            public List<int> anchorFrames = new List<int>(); public List<Frame> frames = new List<Frame>();
            public List<PoseComparison> comparisons = new List<PoseComparison>();
        }
        [Serializable] public sealed class RoadSurfaceInfo {
            public string material,shader;public string[] keywords;public Color baseColor;
            public int maskWidth,maskHeight;
            public float smoothnessMultiplier,environmentReflections,bumpScale,metallic,
                uploadedAlphaMinimum,uploadedAlphaMaximum,effectiveSmoothnessMinimum,effectiveSmoothnessMaximum;
        }
        [Serializable] public sealed class Frame {
            public int index, width, height, lap; public string file, anchor, anchorFile;
            public float raceTime, progress, speedKph, energy, fieldOfView; public bool boost, grounded,captureRequested;
            public Vector3 cameraPosition, playerVisualPosition; public Quaternion cameraRotation, playerVisualRotation;
            public RacerPose[] racers;
        }
        [Serializable] public sealed class RacerPose {
            public string name; public Vector3 position, velocity, angularVelocity; public Quaternion rotation; public float progress;
        }
        [Serializable] public sealed class PoseComparison {
            public string anchor; public float cameraDistance, cameraAngle, fovDifference, playerDistance, progressDifference; public bool matched;
        }
    }
}
