using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VectorRush.Editor
{
    /// <summary>Builds the checked-in Solstice scene without preparing, opening or saving any scene.</summary>
    public static class AAAReviewBuild
    {
        const string ScenePath = "Assets/Scenes/Solstice.unity";
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        [Serializable]
        sealed class BuildIdentity
        {
            public string status;
            public string visualStatus = "BLOCKED_VISUAL_REVIEW";
            public string unityVersion;
            public string result;
            public string buildGuid;
            public string outputPath;
            public string scenePath;
            public string sceneAssetGuid;
            public string sceneSha256;
            public string courseHash;
            public string openingResourceAssetGuid;
            public string galleryResourceAssetGuid;
            public string openingRevision;
            public string galleryRevision;
            public string gallerySourceCourseSha256;
            public bool galleryAtomicReplacement;
            public int galleryFixtureCount;
            public string buildMethod = "VectorRush.Editor.AAAReviewBuild.BuildSolstice";
            public string scenePreparation = "NOT_RUN";
            public string startedUtc;
            public string finishedUtc;
            public double totalSeconds;
            public string totalSizeBytes;
            public string error;
        }

        public static void BuildSolstice()
        {
            string output = RequiredFreshPath("-aaaBuildOutput");
            string evidence = RequiredFreshPath("-aaaEvidence");
            string project = Directory.GetParent(Application.dataPath).FullName;
            string sceneSource = Path.Combine(project, ScenePath);
            if (!output.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("-aaaBuildOutput must name a fresh .app bundle");
            if (!File.Exists(sceneSource)) throw new FileNotFoundException("Existing Solstice scene is missing", sceneSource);
            if (Overlaps(output, evidence)) throw new IOException("Build output and evidence paths must not overlap");
            if (Overlaps(output, project) || Overlaps(evidence, project))
                throw new IOException("Build output and evidence must be outside the Unity project");

            Directory.CreateDirectory(evidence);
            var identity = new BuildIdentity {
                status = "FAILED",
                unityVersion = Application.unityVersion,
                outputPath = output,
                scenePath = ScenePath,
                sceneAssetGuid = AssetDatabase.AssetPathToGUID(ScenePath),
                sceneSha256 = FileHash(sceneSource),
                openingResourceAssetGuid = ResourceGuid(AAABaselineIntegration.OpeningResource),
                galleryResourceAssetGuid = ResourceGuid(AAABaselineIntegration.GalleryResource),
                startedUtc = DateTime.UtcNow.ToString("o")
            };

            var courseObject = new GameObject("AAA review build course identity") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var track = courseObject.AddComponent<TrackPath>();
                identity.courseHash = AAACourseIdentity.Compute(track);
                AAAExemplarValidation.ValidateRequiredGalleryStaging();
                AAAExemplar opening = Resources.Load<AAAExemplar>(AAABaselineIntegration.OpeningResource);
                AAAExemplar gallery = Resources.Load<AAAExemplar>(AAABaselineIntegration.GalleryResource);
                if (opening) RequireForTrack(opening, track, AAABaselineIntegration.OpeningResource);
                RequireForTrack(gallery, track, AAABaselineIntegration.GalleryResource);
                identity.openingRevision = opening ? opening.revision : null;
                identity.galleryRevision = gallery.revision;
                identity.gallerySourceCourseSha256 = gallery.sourceCourseSha256;
                identity.galleryAtomicReplacement = gallery.atomicReplacement;
                identity.galleryFixtureCount = gallery.lights == null ? 0 : gallery.lights.Length;
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { ScenePath },
                    locationPathName = output,
                    target = BuildTarget.StandaloneOSX,
                    options = BuildOptions.None
                });
                identity.result = report.summary.result.ToString();
                identity.buildGuid = report.summary.guid.ToString();
                identity.totalSeconds = report.summary.totalTime.TotalSeconds;
                identity.totalSizeBytes = report.summary.totalSize.ToString();
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Solstice review build failed: " + report.summary.result);
                identity.status = "NATIVE_BUILD_COMPLETE_NOT_QUALITY_ACCEPTED";
                Debug.Log("VR_AAA_BUILD complete without Prepare; visual review still blocked; output=" + output);
            }
            catch (Exception exception)
            {
                identity.error = exception.ToString();
                Debug.LogError("VR_AAA_BUILD failed: " + exception.Message);
                throw;
            }
            finally
            {
                identity.finishedUtc = DateTime.UtcNow.ToString("o");
                File.WriteAllText(Path.Combine(evidence, "build-identity.json"), JsonUtility.ToJson(identity, true), Utf8);
                UnityEngine.Object.DestroyImmediate(courseObject);
            }
        }

        static string RequiredFreshPath(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            int count = 0, index = -1;
            for (int i = 0; i < args.Length; i++) if (args[i] == flag) { count++; index = i; }
            if (count != 1 || index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1]))
                throw new ArgumentException("Require exactly one " + flag + " <absolute path>");
            string path = Path.GetFullPath(args[index + 1]).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (File.Exists(path) || Directory.Exists(path)) throw new IOException(flag + " must be fresh: " + path);
            return path;
        }

        static bool Overlaps(string first, string second)
        {
            string a = first.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string b = second.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || b.StartsWith(a, StringComparison.OrdinalIgnoreCase);
        }

        static string ResourceGuid(string resource)
        {
            return AssetDatabase.AssetPathToGUID("Assets/Resources/" + resource + ".asset");
        }

        static void RequireForTrack(AAAExemplar payload, TrackPath track, string resource)
        {
            string issue = AAABaselineIntegration.ValidateForTrack(payload, track);
            if (issue != null) throw new InvalidOperationException(resource + ": " + issue);
        }

        static string FileHash(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }
}
