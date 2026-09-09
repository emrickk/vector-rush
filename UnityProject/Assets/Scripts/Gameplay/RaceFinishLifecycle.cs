using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VectorRush
{
    public enum RacerResultStatus { Finished, DidNotFinish }

    public sealed class RacerIdentity
    {
        public string RacerId { get; }
        public string DisplayName { get; }
        public bool IsPlayer { get; }

        public RacerIdentity(string racerId, string displayName, bool isPlayer)
        {
            if (string.IsNullOrWhiteSpace(racerId)) throw new ArgumentException("A racer ID is required", nameof(racerId));
            RacerId = racerId;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? racerId : displayName;
            IsPlayer = isPlayer;
        }
    }

    public sealed class RacerFinishRecord
    {
        public string RacerId { get; }
        public string DisplayName { get; }
        public bool IsPlayer { get; }
        public RacerResultStatus Status { get; }
        public int Position { get; }
        public bool HasFinishTime { get; }
        public float FinishTime { get; }

        internal RacerFinishRecord(RacerIdentity identity, RacerResultStatus status, int position, float finishTime)
        {
            RacerId = identity.RacerId;
            DisplayName = identity.DisplayName;
            IsPlayer = identity.IsPlayer;
            Status = status;
            Position = position;
            HasFinishTime = status == RacerResultStatus.Finished;
            FinishTime = HasFinishTime ? finishTime : 0f;
        }
    }

    /// <summary>Simulation-clock finish state, independent of HUD clocks and Unity object lifetimes.</summary>
    public sealed class RaceFinishLifecycle
    {
        readonly List<RacerIdentity> racers;
        readonly Dictionary<string, RacerIdentity> identities;
        readonly List<RacerFinishRecord> records = new List<RacerFinishRecord>();
        readonly ReadOnlyCollection<RacerFinishRecord> readOnlyRecords;
        readonly float timeoutSeconds;
        readonly string playerId;
        float playerFinishTime;

        public float SimulationTime { get; private set; }
        public bool PlayerFinished => !float.IsNaN(playerFinishTime);
        public float PlayerFinishTime => PlayerFinished ? playerFinishTime : 0f;
        public float PlayerPresentationTime => PlayerFinished ? playerFinishTime : SimulationTime;
        public IReadOnlyList<RacerFinishRecord> Records => readOnlyRecords;
        public bool HasPendingRivals => PlayerFinished && racers.Any(value => !value.IsPlayer && !IsResolved(value.RacerId));

        public RaceFinishLifecycle(IEnumerable<RacerIdentity> racerIdentities, float timeoutSeconds = 60f)
        {
            if (racerIdentities == null) throw new ArgumentNullException(nameof(racerIdentities));
            racers = racerIdentities.ToList();
            if (racers.Count == 0) throw new ArgumentException("At least one racer is required", nameof(racerIdentities));
            identities = new Dictionary<string, RacerIdentity>(StringComparer.Ordinal);
            foreach (RacerIdentity identity in racers)
            {
                if (identity == null) throw new ArgumentException("Racer identities cannot contain null", nameof(racerIdentities));
                if (!identities.TryAdd(identity.RacerId, identity)) throw new ArgumentException("Duplicate racer ID: " + identity.RacerId, nameof(racerIdentities));
            }
            RacerIdentity[] players = racers.Where(value => value.IsPlayer).ToArray();
            if (players.Length != 1) throw new ArgumentException("Exactly one player racer is required", nameof(racerIdentities));
            if (float.IsNaN(timeoutSeconds) || float.IsInfinity(timeoutSeconds) || timeoutSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            playerId = players[0].RacerId;
            this.timeoutSeconds = timeoutSeconds;
            readOnlyRecords = records.AsReadOnly();
            Reset();
        }

        public void Reset()
        {
            SimulationTime = 0f;
            playerFinishTime = float.NaN;
            records.Clear();
        }

        public void Advance(float deltaSeconds, bool paused = false)
        {
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (paused || deltaSeconds == 0f || (PlayerFinished && !HasPendingRivals)) return;
            SimulationTime += deltaSeconds;
            if (PlayerFinished && SimulationTime - playerFinishTime >= timeoutSeconds) ResolveTimeout();
        }

        public RacerFinishRecord RecordFinish(string racerId, float crossingTime)
        {
            if (!identities.TryGetValue(racerId, out RacerIdentity identity)) throw new ArgumentException("Unknown racer ID: " + racerId, nameof(racerId));
            RacerFinishRecord existing = records.FirstOrDefault(value => value.RacerId == racerId);
            if (existing != null) return existing;
            if (float.IsNaN(crossingTime) || float.IsInfinity(crossingTime) || crossingTime < 0f || crossingTime > SimulationTime + .001f)
                throw new ArgumentOutOfRangeException(nameof(crossingTime), "Finish timestamp must be finite and no later than the simulation clock");
            var record = new RacerFinishRecord(identity, RacerResultStatus.Finished, records.Count(value => value.Status == RacerResultStatus.Finished) + 1, crossingTime);
            records.Add(record);
            if (racerId == playerId) playerFinishTime = crossingTime;
            return record;
        }

        public bool CanSimulate(string racerId)
        {
            if (!identities.TryGetValue(racerId, out RacerIdentity identity)) return false;
            if (IsResolved(racerId)) return false;
            return !PlayerFinished || !identity.IsPlayer;
        }

        public bool IsResolved(string racerId) => records.Any(value => value.RacerId == racerId);

        void ResolveTimeout()
        {
            foreach (RacerIdentity identity in racers)
            {
                if (IsResolved(identity.RacerId)) continue;
                records.Add(new RacerFinishRecord(identity, RacerResultStatus.DidNotFinish, 0, 0f));
            }
        }
    }
}
