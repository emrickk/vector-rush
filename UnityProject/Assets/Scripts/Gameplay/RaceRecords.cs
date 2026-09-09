using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VectorRush
{
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
        readonly Dictionary<string, RecordEntry> entries = new Dictionary<string, RecordEntry>(StringComparer.Ordinal);

        public string FilePath => filePath;

        public RaceRecords(string directory = null)
        {
            string root = string.IsNullOrWhiteSpace(directory) ? Application.persistentDataPath : Path.GetFullPath(directory);
            filePath = Path.Combine(root, "race-records.json");
            Load();
        }

        public RaceBest GetBest(string courseHash, string drivingRulesId)
        {
            string key = Key(courseHash, drivingRulesId);
            return entries.TryGetValue(key, out RecordEntry entry) ? new RaceBest(entry.bestLap, entry.bestRace) : new RaceBest(0f, 0f);
        }

        public RaceRecordComparison Submit(string courseHash, string drivingRulesId, float bestLap, float raceTime, bool automatedRun)
        {
            if (!RaceBest.ValidTime(bestLap)) throw new ArgumentOutOfRangeException(nameof(bestLap), "Best lap must be finite and positive");
            if (!RaceBest.ValidTime(raceTime)) throw new ArgumentOutOfRangeException(nameof(raceTime), "Race time must be finite and positive");
            string key = Key(courseHash, drivingRulesId);
            RaceBest previous = GetBest(courseHash, drivingRulesId);
            if (automatedRun) return new RaceRecordComparison(bestLap, raceTime, previous, false, false, true);

            bool updatedLap = !previous.HasBestLap || bestLap < previous.BestLap;
            bool updatedRace = !previous.HasBestRace || raceTime < previous.BestRace;
            if (updatedLap || updatedRace)
            {
                if (!entries.TryGetValue(key, out RecordEntry entry))
                {
                    entry = new RecordEntry { courseHash = courseHash, drivingRulesId = drivingRulesId };
                    entries.Add(key, entry);
                }
                if (updatedLap) entry.bestLap = bestLap;
                if (updatedRace) entry.bestRace = raceTime;
                SaveAtomic();
            }
            return new RaceRecordComparison(bestLap, raceTime, previous, updatedLap, updatedRace, false);
        }

        void Load()
        {
            entries.Clear();
            if (!File.Exists(filePath)) return;
            try
            {
                RecordStore store = JsonUtility.FromJson<RecordStore>(File.ReadAllText(filePath));
                if (store == null || store.version != FileVersion || store.entries == null) return;
                foreach (RecordEntry entry in store.entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.courseHash) || string.IsNullOrWhiteSpace(entry.drivingRulesId)) continue;
                    if (!RaceBest.ValidTime(entry.bestLap) && !RaceBest.ValidTime(entry.bestRace)) continue;
                    entries[Key(entry.courseHash, entry.drivingRulesId)] = entry;
                }
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
            var store = new RecordStore
            {
                version = FileVersion,
                entries = entries.Values.OrderBy(value => value.courseHash, StringComparer.Ordinal)
                    .ThenBy(value => value.drivingRulesId, StringComparer.Ordinal).ToArray()
            };
            try
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(store, false));
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
        sealed class RecordEntry
        {
            public string courseHash;
            public string drivingRulesId;
            public float bestLap;
            public float bestRace;
        }
    }
}
