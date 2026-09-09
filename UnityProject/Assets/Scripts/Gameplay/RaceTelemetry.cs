using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VectorRush
{
    public sealed class SectorTelemetryRecord
    {
        public int Lap { get; }
        public int Sector { get; }
        public float CrossingTime { get; }
        internal SectorTelemetryRecord(int lap, int sector, float crossingTime) { Lap = lap; Sector = sector; CrossingTime = crossingTime; }
    }

    public sealed class TrafficTelemetryRecord
    {
        public float SimulationTime { get; }
        public float NearestSignedGapMeters { get; }
        public int NearbyRivals { get; }
        public int ActiveCorridorGuards { get; }
        internal TrafficTelemetryRecord(float time, float nearestGap, int nearby, int guards)
        { SimulationTime = time; NearestSignedGapMeters = nearestGap; NearbyRivals = nearby; ActiveCorridorGuards = guards; }
    }

    public sealed class RaceTelemetry
    {
        readonly List<SectorTelemetryRecord> sectors = new List<SectorTelemetryRecord>();
        readonly List<TrafficTelemetryRecord> traffic = new List<TrafficTelemetryRecord>();
        readonly ReadOnlyCollection<SectorTelemetryRecord> readOnlySectors;
        readonly ReadOnlyCollection<TrafficTelemetryRecord> readOnlyTraffic;
        public IReadOnlyList<SectorTelemetryRecord> Sectors => readOnlySectors;
        public IReadOnlyList<TrafficTelemetryRecord> Traffic => readOnlyTraffic;
        public float RivalCorridorGuardSeconds { get; private set; }
        public int PlayerRecoveryCount { get; private set; }

        public RaceTelemetry()
        {
            readOnlySectors = sectors.AsReadOnly();
            readOnlyTraffic = traffic.AsReadOnly();
        }

        public void Reset()
        {
            sectors.Clear();
            traffic.Clear();
            RivalCorridorGuardSeconds = 0f;
            PlayerRecoveryCount = 0;
        }

        public void RecordSector(int lap, int sector, float crossingTime)
        {
            sectors.Add(new SectorTelemetryRecord(lap, sector, crossingTime));
        }

        public void RecordTraffic(float simulationTime, float nearestSignedGapMeters, int nearbyRivals, int activeCorridorGuards)
        {
            traffic.Add(new TrafficTelemetryRecord(simulationTime, nearestSignedGapMeters, nearbyRivals, activeCorridorGuards));
        }

        public void AccumulateAssistance(float deltaSeconds, int activeRivalCorridorGuards, int playerRecoveryCount)
        {
            RivalCorridorGuardSeconds += deltaSeconds * activeRivalCorridorGuards;
            PlayerRecoveryCount = playerRecoveryCount;
        }
    }
}
