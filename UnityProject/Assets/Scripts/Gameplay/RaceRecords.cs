using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VectorRush
{
    [Serializable]
    public sealed class RaceRecordData
    {
        public string course;
        public string rules;
        public float lap;
        public float race;
    }

    public sealed class RaceBest
    {
        public bool HasBestLap { get; }
        public float BestLap { get; }
        public bool HasBestRace { get; }
        public float BestRace { get; }

        internal RaceBest(float bestLap, float bestRace)
        {
            HasBestLap = ValidTime(bestLap);
            BestLap = HasBestLap ? bestLap : 0f;
            HasBestRace = ValidTime(bestRace);
            BestRace = HasBestRace ? bestRace : 0f;
        }

        internal static bool ValidTime(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class RaceRecordComparison
    {
        public float CurrentLap { get; }
        public float CurrentRace { get; }
        public bool HadPreviousLapBest { get; }
        public float PreviousBestLap { get; }
        public float LapDelta { get; }
        public bool HadPreviousRaceBest { get; }
        public float PreviousBestRace { get; }
        public float RaceDelta { get; }
        public bool UpdatedLapBest { get; }
        public bool UpdatedRaceBest { get; }
        public bool ExcludedAutomatedRun { get; }

        internal RaceRecordComparison(float lap, float race, RaceBest previous, bool updatedLap, bool updatedRace, bool excluded)
        {
            CurrentLap = lap;
            CurrentRace = race;
            HadPreviousLapBest = previous.HasBestLap;
            PreviousBestLap = previous.BestLap;
            LapDelta = previous.HasBestLap ? lap - previous.BestLap : 0f;
            HadPreviousRaceBest = previous.HasBestRace;
            PreviousBestRace = previous.BestRace;
            RaceDelta = previous.HasBestRace ? race - previous.BestRace : 0f;
            UpdatedLapBest = updatedLap;
            UpdatedRaceBest = updatedRace;
            ExcludedAutomatedRun = excluded;
        }
    }

    public sealed class RaceRecords
    {
        const int FileVersion = 1;
        readonly string filePath;
        readonly Action<string, string> writeText;
        readonly Action<string> deleteFile;
        readonly Dictionary<string, RecordEntry> entries = new Dictionary<string, RecordEntry>(StringComparer.Ordinal);

        public const string DrivingRules = RaceDirector.DrivingRulesId;
        public string FilePath => filePath;
        public string LastSaveError { get; private set; } = "";

        public RaceRecords(string directory = null)
            : this(ResolveFilePath(directory), WriteDurably, File.Delete)
        {
        }

        RaceRecords(string path, Action<string, string> writer, Action<string> cleanup)
        {
            filePath = Path.GetFullPath(path);
            writeText = writer ?? throw new ArgumentNullException(nameof(writer));
            deleteFile = cleanup ?? throw new ArgumentNullException(nameof(cleanup));
            Load();
        }

        public RaceBest GetBest(string courseHash, string drivingRulesId)
        {
            string key = Key(courseHash, drivingRulesId);
            return entries.TryGetValue(key, out RecordEntry entry) ? new RaceBest(entry.bestLap, entry.bestRace) : new RaceBest(0f, 0f);
        }

        public RaceRecordData Get(string courseHash, string drivingRulesId)
        {
            string key = Key(courseHash, drivingRulesId);
            if (!entries.TryGetValue(key, out RecordEntry entry)) return null;
            return new RaceRecordData { course = entry.courseHash, rules = entry.drivingRulesId, lap = entry.bestLap, race = entry.bestRace };
        }

        public bool Record(string courseHash, string drivingRulesId, float bestLap, float raceTime, bool automatedRun)
        {
            RaceRecordComparison comparison = Submit(courseHash, drivingRulesId, bestLap, raceTime, automatedRun);
            return comparison.UpdatedLapBest || comparison.UpdatedRaceBest;
        }

        public RaceRecordComparison Submit(string courseHash, string drivingRulesId, float bestLap, float raceTime, bool automatedRun)
        {
            LastSaveError = "";
            if (!RaceBest.ValidTime(bestLap)) throw new ArgumentOutOfRangeException(nameof(bestLap), "Best lap must be finite and positive");
            if (!RaceBest.ValidTime(raceTime)) throw new ArgumentOutOfRangeException(nameof(raceTime), "Race time must be finite and positive");
            string key = Key(courseHash, drivingRulesId);
            RaceBest previous = GetBest(courseHash, drivingRulesId);
            if (automatedRun) return new RaceRecordComparison(bestLap, raceTime, previous, false, false, true);

            bool updatedLap = !previous.HasBestLap || bestLap < previous.BestLap;
            bool updatedRace = !previous.HasBestRace || raceTime < previous.BestRace;
            if (updatedLap || updatedRace)
            {
                bool hadEntry = entries.TryGetValue(key, out RecordEntry entry);
                float oldLap = hadEntry ? entry.bestLap : 0f;
                float oldRace = hadEntry ? entry.bestRace : 0f;
                if (!hadEntry)
                {
                    entry = new RecordEntry { courseHash = courseHash, drivingRulesId = drivingRulesId };
                    entries.Add(key, entry);
                }
                if (updatedLap) entry.bestLap = bestLap;
                if (updatedRace) entry.bestRace = raceTime;
                try { SaveAtomic(); }
                catch (Exception error)
                {
                    LastSaveError = error.Message;
                    if (hadEntry) { entry.bestLap = oldLap; entry.bestRace = oldRace; }
                    else entries.Remove(key);
                    if (!(error is SaveFailureLoggedException)) Debug.LogWarning("Could not save personal best: " + error.Message);
                    return new RaceRecordComparison(bestLap, raceTime, previous, false, false, false);
                }
            }
            return new RaceRecordComparison(bestLap, raceTime, previous, updatedLap, updatedRace, false);
        }

        void Load()
        {
            entries.Clear();
            if (!File.Exists(filePath)) return;
            try
            {
                string json = File.ReadAllText(filePath);
                RecordStore current = JsonUtility.FromJson<RecordStore>(json);
                LegacyRecordStore legacy = JsonUtility.FromJson<LegacyRecordStore>(json);
                if ((current == null || current.version != FileVersion || current.entries == null) &&
                    (legacy == null || legacy.version != FileVersion || legacy.records == null)) return;
                if (current != null && current.version == FileVersion && current.entries != null)
                    foreach (RecordEntry entry in current.entries)
                        if (entry != null) Merge(entry.courseHash, entry.drivingRulesId, entry.bestLap, entry.bestRace);
                if (legacy != null && legacy.version == FileVersion && legacy.records != null)
                    foreach (RaceRecordData entry in legacy.records)
                        if (entry != null) Merge(entry.course, entry.rules, entry.lap, entry.race);
            }
            catch (Exception error)
            {
                Debug.LogWarning("Race records are malformed and were ignored without modifying the file: " + error.Message);
                entries.Clear();
            }
        }

        void SaveAtomic()
        {
            string directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            string temporary = filePath + ".tmp-" + Guid.NewGuid().ToString("N");
            string backup = filePath + ".backup-" + Guid.NewGuid().ToString("N");
            var store = new LegacyRecordStore
            {
                version = FileVersion,
                records = entries.Values.OrderBy(value => value.courseHash, StringComparer.Ordinal)
                    .ThenBy(value => value.drivingRulesId, StringComparer.Ordinal)
                    .Select(value => new RaceRecordData
                    {
                        course = value.courseHash,
                        rules = value.drivingRulesId,
                        lap = value.bestLap,
                        race = value.bestRace
                    }).ToArray()
            };
            try
            {
                writeText(temporary, JsonUtility.ToJson(store, false));
                if (File.Exists(filePath))
                {
                    File.Replace(temporary, filePath, backup);
                    TryCleanup(backup);
                }
                else File.Move(temporary, filePath);
            }
            catch (Exception error)
            {
                Debug.LogWarning("Could not save personal best: " + error.Message);
                TryCleanup(temporary);
                throw new SaveFailureLoggedException(error);
            }
        }

        void Merge(string courseHash, string drivingRulesId, float bestLap, float bestRace)
        {
            if (string.IsNullOrWhiteSpace(courseHash) || string.IsNullOrWhiteSpace(drivingRulesId)) return;
            if (!RaceBest.ValidTime(bestLap) || !RaceBest.ValidTime(bestRace)) return;
            string key = Key(courseHash, drivingRulesId);
            if (!entries.TryGetValue(key, out RecordEntry current))
            {
                entries.Add(key, new RecordEntry
                {
                    courseHash = courseHash,
                    drivingRulesId = drivingRulesId,
                    bestLap = RaceBest.ValidTime(bestLap) ? bestLap : 0f,
                    bestRace = RaceBest.ValidTime(bestRace) ? bestRace : 0f
                });
                return;
            }
            if (RaceBest.ValidTime(bestLap) && (!RaceBest.ValidTime(current.bestLap) || bestLap < current.bestLap)) current.bestLap = bestLap;
            if (RaceBest.ValidTime(bestRace) && (!RaceBest.ValidTime(current.bestRace) || bestRace < current.bestRace)) current.bestRace = bestRace;
        }

        void TryCleanup(string path)
        {
            if (!File.Exists(path)) return;
            try { deleteFile(path); }
            catch (Exception error) { Debug.LogWarning("Could not clean up personal best temporary file: " + error.Message); }
        }

        static string ResolveFilePath(string directoryOrPath)
        {
            string value = string.IsNullOrWhiteSpace(directoryOrPath) ? Application.persistentDataPath : Path.GetFullPath(directoryOrPath);
            return string.Equals(Path.GetExtension(value), ".json", StringComparison.OrdinalIgnoreCase)
                ? value
                : Path.Combine(value, "race-records.json");
        }

        static void WriteDurably(string path, string json)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(json);
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        static string Key(string courseHash, string drivingRulesId)
        {
            if (string.IsNullOrWhiteSpace(courseHash)) throw new ArgumentException("Course hash is required", nameof(courseHash));
            if (string.IsNullOrWhiteSpace(drivingRulesId)) throw new ArgumentException("Driving-rules ID is required", nameof(drivingRulesId));
            return courseHash + "\n" + drivingRulesId;
        }

        [Serializable]
        sealed class RecordStore
        {
            public int version;
            public RecordEntry[] entries;
        }

        [Serializable]
        sealed class LegacyRecordStore
        {
            public int version;
            public RaceRecordData[] records;
        }

        [Serializable]
        sealed class RecordEntry
        {
            public string courseHash;
            public string drivingRulesId;
            public float bestLap;
            public float bestRace;
        }

        sealed class SaveFailureLoggedException : Exception
        {
            public SaveFailureLoggedException(Exception inner) : base(inner.Message, inner) { }
        }
    }
}
