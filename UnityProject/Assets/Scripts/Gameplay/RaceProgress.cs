using System;

namespace VectorRush
{
    /// <summary>Ordered gates on a closed circuit. Position alone never awards a lap.</summary>
    public sealed class RaceProgress
    {
        public const int SectorCount = 12;
        public int CompletedLaps { get; private set; }
        public int NextCheckpoint { get; private set; }
        public bool HasStarted { get; private set; }
        public float LastProgress { get; private set; }
        public float LastCrossingFraction { get; private set; }
        public float RecoveryProgress => HasStarted ? (NextCheckpoint - 1f) / SectorCount : startingProgress;
        float startingProgress;
        bool previousSampleValid;
        public float Distance
        {
            get
            {
                if (!HasStarted) return LastProgress > .5f ? LastProgress - 1f : 0f;
                // An unvalidated shortcut cannot improve race position beyond the next gate.
                float sectorStart = (NextCheckpoint - 1f) / SectorCount;
                float position = LastProgress;
                if (NextCheckpoint == SectorCount && position < .1f) position = 1f;
                return CompletedLaps + Math.Max(sectorStart, Math.Min(position, NextCheckpoint / (float)SectorCount));
            }
        }

        public RaceProgress(float startingProgress = .995f) { Reset(startingProgress); }

        public void Reset(float startingProgress)
        {
            CompletedLaps = 0;
            NextCheckpoint = 1;
            HasStarted = false;
            this.startingProgress = IsFinite(startingProgress) ? Wrap(startingProgress) : .995f;
            LastProgress = this.startingProgress;
            LastCrossingFraction = 0f;
            previousSampleValid = true;
        }

        /// <summary>Reposition without crossing a gate; does not erase already validated gates.</summary>
        public void NotifyRespawn(float progress)
        {
            if (IsFinite(progress)) LastProgress = Wrap(progress);
            previousSampleValid = false;
        }

        /// <returns>True only on a complete forward lap through all ordered gates.</returns>
        public bool Sample(float progress, bool validTrackSample = true)
        {
            if (!IsFinite(progress)) { previousSampleValid = false; return false; }
            progress = Wrap(progress);
            float previous = LastProgress;
            LastProgress = progress;
            bool mayCross = previousSampleValid && validTrackSample;
            previousSampleValid = validTrackSample;
            // Re-entry and recovery establish position first; neither can synthesize a crossing.
            if (!mayCross) return false;
            float delta = progress - previous;
            if (delta < -.5f) delta += 1f;
            else if (delta > .5f) delta -= 1f;
            // Reverse motion, unchanged samples, and discontinuities award nothing.
            if (delta <= 0f || delta > 1f / SectorCount) return false;
            float end = previous + delta;
            bool crossedStart = end >= 1f && previous < 1f;
            if (!HasStarted)
            {
                if (crossedStart)
                {
                    HasStarted = true;
                    LastCrossingFraction = (1f - previous) / delta;
                }
                return false;
            }
            float gate = NextCheckpoint / (float)SectorCount;
            if (previous < gate && end >= gate)
            {
                LastCrossingFraction = (gate - previous) / delta;
                if (NextCheckpoint == SectorCount)
                {
                    CompletedLaps++;
                    NextCheckpoint = 1;
                    return true;
                }
                NextCheckpoint++;
            }
            return false;
        }

        static float Wrap(float value) => value - (float)Math.Floor(value);
        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
