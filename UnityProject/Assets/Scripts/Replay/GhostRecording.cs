using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VectorRush
{
    public static class GhostRecordingSchema
    {
        public const int Version = 1;
        public const int SamplesPerSecond = 20;
        public const float MaximumLapSeconds = 300f;
        public const int MaximumSamples = (int)(MaximumLapSeconds * SamplesPerSecond) + 2;
        public const int MaximumFileBytes = 2 * 1024 * 1024;
    }

    [Serializable]
    public sealed class GhostSample
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public float progress;
    }

    [Serializable]
    public sealed class GhostLapRecording
    {
        public int schemaVersion = GhostRecordingSchema.Version;
        public string courseIdentity;
        public string drivingRulesIdentity;
        public float lapTime;
        public float[] sectorTimes;
        public GhostSample[] samples;

        public bool IsCompatible(string course, string rules)
        {
            return schemaVersion == GhostRecordingSchema.Version &&
                string.Equals(courseIdentity, course, StringComparison.Ordinal) &&
                string.Equals(drivingRulesIdentity, rules, StringComparison.Ordinal);
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(courseIdentity) || string.IsNullOrWhiteSpace(drivingRulesIdentity) ||
                !FinitePositive(lapTime) || lapTime > GhostRecordingSchema.MaximumLapSeconds ||
                sectorTimes == null || sectorTimes.Length != 3 || samples == null || samples.Length < 2 ||
                samples.Length > GhostRecordingSchema.MaximumSamples) return false;

            float previousSector = 0f;
            for (int i = 0; i < sectorTimes.Length; i++)
            {
                if (!FinitePositive(sectorTimes[i]) || sectorTimes[i] <= previousSector || sectorTimes[i] > lapTime + .001f) return false;
                previousSector = sectorTimes[i];
            }

            float previousTime = -1f;
            for (int i = 0; i < samples.Length; i++)
            {
                GhostSample sample = samples[i];
                if (sample == null || !Finite(sample.time) || sample.time < 0f || sample.time <= previousTime ||
                    sample.time > lapTime + .001f || !Finite(sample.progress) || !Finite(sample.position) || !Finite(sample.rotation)) return false;
                previousTime = sample.time;
            }
            return samples[0].time <= .051f && Mathf.Abs(samples[samples.Length - 1].time - lapTime) <= .051f;
        }

        static bool FinitePositive(float value) => Finite(value) && value > 0f;
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        static bool Finite(Quaternion value) => Finite(value.x) && Finite(value.y) && Finite(value.z) && Finite(value.w) &&
            value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > .0001f;
    }

    public enum GhostLoadStatus { Loaded, NotFound, Incompatible, Unreadable }
    public enum GhostSaveStatus { Saved, NotFaster, Failed }

    public sealed class GhostLoadResult
    {
        public GhostLoadStatus Status { get; }
        public GhostLapRecording Recording { get; }
        public string Message { get; }

        internal GhostLoadResult(GhostLoadStatus status, GhostLapRecording recording = null, string message = null)
        {
            Status = status;
            Recording = recording;
            Message = message ?? string.Empty;
        }
    }

    public sealed class GhostSaveResult
    {
        public GhostSaveStatus Status { get; }
        public string Message { get; }
        public bool Saved => Status == GhostSaveStatus.Saved;

        internal GhostSaveResult(GhostSaveStatus status, string message = null)
        {
            Status = status;
            Message = message ?? string.Empty;
        }
    }

    public sealed class PersonalBestGhostStore
    {
        const int StoreVersion = 1;
        readonly string filePath;
        public string FilePath => filePath;

        public PersonalBestGhostStore(string directory = null)
        {
            string root = string.IsNullOrWhiteSpace(directory) ? Application.persistentDataPath : Path.GetFullPath(directory);
            filePath = Path.Combine(root, "personal-best-ghosts.json");
        }

        public GhostLoadResult Load(string course, string rules)
        {
            RequireIdentity(course, rules);
            if (!File.Exists(filePath)) return new GhostLoadResult(GhostLoadStatus.NotFound, message: "Complete an eligible lap to create your first ghost.");
            try
            {
                if (new FileInfo(filePath).Length > GhostRecordingSchema.MaximumFileBytes)
                    return new GhostLoadResult(GhostLoadStatus.Unreadable, message: "The saved ghost exceeds the storage limit and was ignored.");
                GhostStoreFile store = Parse(File.ReadAllText(filePath));
                if (store == null || store.version != StoreVersion || store.recordings == null)
                    return new GhostLoadResult(GhostLoadStatus.Unreadable, message: "The saved ghost could not be read.");
                GhostLapRecording match = store.recordings.FirstOrDefault(value => value != null &&
                    string.Equals(value.courseIdentity, course, StringComparison.Ordinal) &&
                    string.Equals(value.drivingRulesIdentity, rules, StringComparison.Ordinal));
                if (match == null)
                    return new GhostLoadResult(GhostLoadStatus.Incompatible, message: "The saved ghost belongs to a different course or driving setup.");
                if (!match.IsCompatible(course, rules))
                    return new GhostLoadResult(GhostLoadStatus.Incompatible, message: "The saved ghost uses an older recording format.");
                if (!match.IsValid())
                    return new GhostLoadResult(GhostLoadStatus.Unreadable, message: "The saved ghost is incomplete or unreadable.");
                return new GhostLoadResult(GhostLoadStatus.Loaded, match);
            }
            catch (Exception error)
            {
                return new GhostLoadResult(GhostLoadStatus.Unreadable, message: "The saved ghost could not be read: " + error.Message);
            }
        }

        public GhostSaveResult TrySaveIfFaster(GhostLapRecording recording)
        {
            if (recording == null || !recording.IsValid()) return new GhostSaveResult(GhostSaveStatus.Failed, "The lap recording was incomplete and was not saved.");
            try
            {
                GhostStoreFile store = ReadStoreForWrite();
                int index = Array.FindIndex(store.recordings, value => value != null &&
                    string.Equals(value.courseIdentity, recording.courseIdentity, StringComparison.Ordinal) &&
                    string.Equals(value.drivingRulesIdentity, recording.drivingRulesIdentity, StringComparison.Ordinal) &&
                    value.schemaVersion == GhostRecordingSchema.Version);
                if (index >= 0 && store.recordings[index].IsValid() && store.recordings[index].lapTime <= recording.lapTime)
                    return new GhostSaveResult(GhostSaveStatus.NotFaster, "The existing personal-best ghost is faster.");

                var recordings = store.recordings.Where(value => value != null &&
                    !(string.Equals(value.courseIdentity, recording.courseIdentity, StringComparison.Ordinal) &&
                      string.Equals(value.drivingRulesIdentity, recording.drivingRulesIdentity, StringComparison.Ordinal))).ToList();
                recordings.Add(recording);
                store.recordings = recordings.OrderBy(value => value.courseIdentity, StringComparer.Ordinal)
                    .ThenBy(value => value.drivingRulesIdentity, StringComparer.Ordinal).ToArray();
                byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(store, false));
                if (bytes.Length > GhostRecordingSchema.MaximumFileBytes)
                    return new GhostSaveResult(GhostSaveStatus.Failed, "The lap recording exceeded the storage limit and was not saved.");
                SaveAtomic(bytes);
                return new GhostSaveResult(GhostSaveStatus.Saved, "Personal-best ghost saved.");
            }
            catch (Exception error)
            {
                return new GhostSaveResult(GhostSaveStatus.Failed, "The personal-best ghost could not be saved: " + error.Message);
            }
        }

        GhostStoreFile ReadStoreForWrite()
        {
            if (!File.Exists(filePath)) return EmptyStore();
            try
            {
                GhostStoreFile store = Parse(File.ReadAllText(filePath));
                return store != null && store.version == StoreVersion && store.recordings != null ? store : EmptyStore();
            }
            catch { return EmptyStore(); }
        }

        static GhostStoreFile Parse(string json) => JsonUtility.FromJson<GhostStoreFile>(json);
        static GhostStoreFile EmptyStore() => new GhostStoreFile { version = StoreVersion, recordings = Array.Empty<GhostLapRecording>() };

        void SaveAtomic(byte[] bytes)
        {
            string directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            string temporary = filePath + ".tmp-" + Guid.NewGuid().ToString("N");
            string backup = filePath + ".backup-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(filePath))
                {
                    File.Replace(temporary, filePath, backup);
                    File.Delete(backup);
                }
                else File.Move(temporary, filePath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                if (File.Exists(backup)) File.Delete(backup);
            }
        }

        static void RequireIdentity(string course, string rules)
        {
            if (string.IsNullOrWhiteSpace(course)) throw new ArgumentException("Course identity is required", nameof(course));
            if (string.IsNullOrWhiteSpace(rules)) throw new ArgumentException("Driving-rules identity is required", nameof(rules));
        }

        [Serializable]
        sealed class GhostStoreFile
        {
            public int version;
            public GhostLapRecording[] recordings;
        }
    }

    public sealed class GhostLapBuilder
    {
        readonly List<GhostSample> samples = new List<GhostSample>();
        float nextSampleTime;
        bool overflowed;
        public bool HasSamples => samples.Count > 0;
        public int SampleCount => samples.Count;

        public void Reset()
        {
            samples.Clear();
            nextSampleTime = 0f;
            overflowed = false;
        }

        public bool TrySample(float lapTime, Vector3 position, Quaternion rotation, float progress, bool force = false)
        {
            if (!Finite(lapTime) || lapTime < 0f || lapTime > GhostRecordingSchema.MaximumLapSeconds) { overflowed = true; return false; }
            if (!force && samples.Count > 0 && lapTime + .0001f < nextSampleTime) return false;
            if (samples.Count >= GhostRecordingSchema.MaximumSamples) { overflowed = true; return false; }
            if (samples.Count > 0 && lapTime <= samples[samples.Count - 1].time) return false;
            samples.Add(new GhostSample { time = lapTime, position = position, rotation = rotation, progress = Mathf.Repeat(progress, 1f) });
            nextSampleTime = lapTime + 1f / GhostRecordingSchema.SamplesPerSecond;
            return true;
        }

        public GhostLapRecording Finish(string course, string rules, float lapTime, IReadOnlyList<float> sectorTimes,
            Vector3 finalPosition, Quaternion finalRotation, float finalProgress)
        {
            if (overflowed || sectorTimes == null || sectorTimes.Count != 3 || !Finite(lapTime) || lapTime <= 0f) return null;
            if (samples.Count == 0 || samples[samples.Count - 1].time < lapTime)
                TrySample(lapTime, finalPosition, finalRotation, finalProgress, true);
            var recording = new GhostLapRecording
            {
                schemaVersion = GhostRecordingSchema.Version,
                courseIdentity = course,
                drivingRulesIdentity = rules,
                lapTime = lapTime,
                sectorTimes = sectorTimes.ToArray(),
                samples = samples.ToArray()
            };
            return recording.IsValid() ? recording : null;
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public readonly struct GhostPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float Progress;

        public GhostPose(Vector3 position, Quaternion rotation, float progress)
        {
            Position = position;
            Rotation = rotation;
            Progress = progress;
        }
    }

    public static class GhostPlayback
    {
        public static bool TryEvaluate(GhostLapRecording recording, float lapTime, out GhostPose pose)
        {
            pose = default;
            if (recording == null || !recording.IsValid() || lapTime < 0f || lapTime > recording.lapTime) return false;
            GhostSample[] samples = recording.samples;
            if (lapTime <= samples[0].time) { pose = Pose(samples[0]); return true; }
            int low = 0, high = samples.Length - 1;
            while (low + 1 < high)
            {
                int mid = (low + high) / 2;
                if (samples[mid].time <= lapTime) low = mid;
                else high = mid;
            }
            GhostSample before = samples[low], after = samples[high];
            float blend = Mathf.InverseLerp(before.time, after.time, lapTime);
            float progressDelta = Mathf.Repeat(after.progress - before.progress + .5f, 1f) - .5f;
            pose = new GhostPose(Vector3.Lerp(before.position, after.position, blend),
                Quaternion.Slerp(before.rotation, after.rotation, blend), Mathf.Repeat(before.progress + progressDelta * blend, 1f));
            return true;
        }

        static GhostPose Pose(GhostSample sample) => new GhostPose(sample.position, sample.rotation, sample.progress);
    }

    public sealed class SectorComparisonTracker
    {
        readonly float[] reference;
        readonly float[] current = new float[3];
        int nextBoundary;
        bool eligible;
        public bool Eligible => eligible;
        public int CompletedSectors => nextBoundary;
        public IReadOnlyList<float> CurrentSectorTimes => current;

        public SectorComparisonTracker(IReadOnlyList<float> referenceSectorTimes = null)
        {
            reference = referenceSectorTimes != null && referenceSectorTimes.Count == 3 ? referenceSectorTimes.ToArray() : null;
            BeginLap(true);
        }

        public void BeginLap(bool lapEligible)
        {
            Array.Clear(current, 0, current.Length);
            nextBoundary = 0;
            eligible = lapEligible;
        }

        public void InvalidateLap() => eligible = false;

        public bool TryRecord(int boundaryIndex, float lapLocalTime, bool crossingEligible, out float delta)
        {
            delta = 0f;
            if (!eligible || !crossingEligible || boundaryIndex != nextBoundary || boundaryIndex < 0 || boundaryIndex >= 3 ||
                float.IsNaN(lapLocalTime) || float.IsInfinity(lapLocalTime) || lapLocalTime <= 0f ||
                (boundaryIndex > 0 && lapLocalTime <= current[boundaryIndex - 1])) return false;
            current[boundaryIndex] = lapLocalTime;
            nextBoundary++;
            if (reference != null) delta = lapLocalTime - reference[boundaryIndex];
            return true;
        }
    }
}
