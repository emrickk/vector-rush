using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    public enum RacePhase { Menu, Countdown, Racing, Paused, Finished }

    public readonly struct OrderedRaceCrossing
    {
        public int LapIndex { get; }
        /// <summary>Zero-based comparison sector: 0 at one-third, 1 at two-thirds, 2 at finish.</summary>
        public int BoundaryIndex { get; }
        public float NormalizedBoundary { get; }
        public float SimulationTime { get; }
        public float LapTime { get; }
        public bool LapEligible { get; }

        public OrderedRaceCrossing(int lapIndex, int boundaryIndex, float normalizedBoundary,
            float simulationTime, float lapTime, bool lapEligible)
        {
            LapIndex = lapIndex;
            BoundaryIndex = boundaryIndex;
            NormalizedBoundary = normalizedBoundary;
            SimulationTime = simulationTime;
            LapTime = lapTime;
            LapEligible = lapEligible;
        }
    }

    public sealed class RaceDirector : MonoBehaviour
    {
        public static RaceDirector Instance { get; private set; }
        public RacePhase Phase { get; private set; } = RacePhase.Menu;
        public HoverVehicle Player { get; private set; }
        public List<HoverVehicle> Racers { get; private set; } = new List<HoverVehicle>();
        public int TotalLaps = 3;
        public float CountdownRemaining { get; private set; }
        public float RaceTime { get; private set; }
        public int Lap => Player == null || Player.ProgressTracker == null ? 1 : Mathf.Min(TotalLaps, Player.ProgressTracker.CompletedLaps + 1);
        public int Position { get; private set; } = 1;
        public float BestLap { get; private set; }
        public float LastLap { get; private set; }
        public float FinishTime => lifecycle == null ? RaceTime : lifecycle.PlayerPresentationTime;
        public float SimulationTime => lifecycle == null ? RaceTime : lifecycle.SimulationTime;
        public float CurrentLapTime => !playerCrossedStart ? 0f :
            Phase == RacePhase.Finished ? LastLap : Mathf.Max(0f, SimulationTime - playerLapStart);
        public float CurrentOrderedProgress => Player == null ? 0f : Player.RaceProgress;
        public float CurrentWrappedProgress => Player == null ? 0f : Player.TrackProgress;
        public string CourseIdentity => courseIdentity;
        public bool ManualRunEligible { get; private set; }
        public bool CurrentLapEligible { get; private set; }
        public bool AutomatedRecordExcluded => !ManualRunEligible;
        public bool NewPersonalBest => CurrentRecordComparison != null &&
            (CurrentRecordComparison.UpdatedLapBest || CurrentRecordComparison.UpdatedRaceBest) &&
            !CurrentRecordComparison.ExcludedAutomatedRun;
        public bool HasPendingRivals => lifecycle != null && lifecycle.HasPendingRivals;
        public IReadOnlyList<RacerFinishRecord> FinishRecords => lifecycle == null ? Array.Empty<RacerFinishRecord>() : lifecycle.Records;
        public RaceRecordComparison CurrentRecordComparison { get; private set; }
        public RaceBest PersonalBest { get; private set; }
        public string RecordSaveError { get; private set; } = "";
        public const string DrivingRulesId = "vector-rush-rules-v2";
        public event Action<RacerFinishRecord> RacerResolved;
        public event Action RaceRestarted;
        public event Action<OrderedRaceCrossing> OrderedCrossed;

        TrackPath track;
        RacePhase beforePause;
        float playerLapStart;
        bool playerCrossedStart;
        readonly List<HoverVehicle> finishOrder = new List<HoverVehicle>();
        readonly List<HoverVehicle> stepFinishers = new List<HoverVehicle>();
        readonly Dictionary<HoverVehicle, string> racerIds = new Dictionary<HoverVehicle, string>();
        RaceFinishLifecycle lifecycle;
        RaceFinishLedger finishLedger;
        RaceRecords records;
        string courseIdentity;

        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(TrackPath circuit, List<HoverVehicle> vehicles, string courseHash = null, RaceRecords records = null)
        {
            track = circuit;
            Racers = vehicles ?? new List<HoverVehicle>();
            Player = Racers.Find(racer => racer && racer.IsPlayer);
            racerIds.Clear();
            var identities = new List<RacerIdentity>();
            for (int i = 0; i < Racers.Count; i++)
            {
                HoverVehicle racer = Racers[i];
                if (!racer) continue;
                string id = "racer-" + i;
                racerIds.Add(racer, id);
                identities.Add(new RacerIdentity(id, racer.DisplayName, racer.IsPlayer));
            }
            lifecycle = identities.Count > 0 && identities.Count(value => value.IsPlayer) == 1 ? new RaceFinishLifecycle(identities, 60f) : null;
            finishLedger = lifecycle == null ? null : new RaceFinishLedger(lifecycle, identities);
            courseIdentity = string.IsNullOrWhiteSpace(courseHash) ? "legacy-solstice-course-v1" : courseHash;
            this.records = records ?? new RaceRecords();
            PersonalBest = this.records.GetBest(courseIdentity, DrivingRulesId);
            Time.timeScale = 1f;
            Phase = RacePhase.Menu;
            FreezeVehicles(true);
            UpdatePosition();
        }

        public void StartRace()
        {
            if (!track || !Player || Racers.Count == 0) return;
            Time.timeScale = 1f;
            FreezeVehicles(false);
            foreach (var racer in Racers) if (racer) racer.ResetForRace();
            FreezeVehicles(true);
            finishOrder.Clear();
            lifecycle?.Reset();
            CurrentRecordComparison = null;
            RecordSaveError = "";
            RaceTime = LastLap = BestLap = playerLapStart = 0f;
            playerCrossedStart = false;
            ManualRunEligible = !(Player.AutopilotForTesting || HasEvidenceArgument());
            CurrentLapEligible = true;
            CountdownRemaining = 3f;
            Phase = RacePhase.Countdown;
            UpdatePosition();
            RaceRestarted?.Invoke();
        }

        public void RestartRace() => StartRace();

        /// <summary>Leave the current race without recording a result or continuing rivals.</summary>
        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            FreezeVehicles(false);
            foreach (var racer in Racers) if (racer) racer.ResetForRace();
            FreezeVehicles(true);
            finishOrder.Clear();
            stepFinishers.Clear();
            lifecycle?.Reset();
            CurrentRecordComparison = null;
            RecordSaveError = "";
            RaceTime = LastLap = BestLap = playerLapStart = CountdownRemaining = 0f;
            playerCrossedStart = false;
            ManualRunEligible = CurrentLapEligible = false;
            Phase = RacePhase.Menu;
            beforePause = RacePhase.Menu;
            UpdatePosition();
            RaceRestarted?.Invoke();
        }

        public void TogglePause()
        {
            if (Phase == RacePhase.Paused)
            {
                Phase = beforePause;
                Time.timeScale = 1f;
            }
            else if (Phase == RacePhase.Racing || Phase == RacePhase.Countdown ||
                (Phase == RacePhase.Finished && HasPendingRivals))
            {
                beforePause = Phase;
                Phase = RacePhase.Paused;
                Time.timeScale = 0f;
            }
        }

        void Update()
        {
            ObserveAutomationProvenance();
            bool pausePressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
            pausePressed = PlayerPreferences.Current.WasPressedThisFrame(PlayerAction.Pause) || Input.GetKeyDown(KeyCode.P);
#endif
#if ENABLE_INPUT_SYSTEM
#if !ENABLE_LEGACY_INPUT_MANAGER
            pausePressed |= Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);
#endif
            pausePressed |= Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
#endif
            if (pausePressed && !RaceHUD.BlocksPauseInput) TogglePause();
            if (Phase != RacePhase.Countdown) return;
            CountdownRemaining = Mathf.Max(0f, CountdownRemaining - Time.deltaTime);
            if (CountdownRemaining <= 0f)
            {
                Phase = RacePhase.Racing;
                FreezeVehicles(false);
            }
        }

        void FixedUpdate()
        {
            if ((Phase != RacePhase.Racing && !(Phase == RacePhase.Finished && lifecycle != null && lifecycle.HasPendingRivals)) || !track || lifecycle == null) return;
            float tickStart = lifecycle.SimulationTime;
            lifecycle.BeginStep(Time.fixedDeltaTime);
            RaceTime = lifecycle.PlayerPresentationTime;
            stepFinishers.Clear();
            foreach (var racer in Racers)
            {
                if (!racer || !CanSimulate(racer)) continue;
                float progress = track.ClosestProgress(racer.Body.position);
                var frame = track.Evaluate(progress);
                bool valid = IsValidGateSample(frame, racer.Body.position, racer.Body.linearVelocity, track.Width, racer.HoverHeight);
                bool lapComplete = racer.ProgressTracker.Sample(progress, valid);
                float crossingTime = tickStart + racer.ProgressTracker.LastCrossingFraction * Time.fixedDeltaTime;
                if (racer == Player)
                {
                    if (!playerCrossedStart && racer.ProgressTracker.HasStarted)
                    {
                        playerCrossedStart = true;
                        playerLapStart = crossingTime;
                        CurrentLapEligible = true;
                    }
                    int checkpoint = racer.ProgressTracker.LastCrossedCheckpoint;
                    if (playerCrossedStart && (checkpoint == 4 || checkpoint == 8 || checkpoint == RaceProgress.SectorCount))
                    {
                        int lapIndex = Mathf.Max(1, racer.ProgressTracker.CompletedLaps + (lapComplete ? 0 : 1));
                        int boundaryIndex = checkpoint / (RaceProgress.SectorCount / 3) - 1;
                        float lapTime = Mathf.Max(0f, crossingTime - playerLapStart);
                        OrderedCrossed?.Invoke(new OrderedRaceCrossing(lapIndex, boundaryIndex,
                            checkpoint / (float)RaceProgress.SectorCount, crossingTime, lapTime, CurrentLapEligible));
                    }
                    if (lapComplete)
                    {
                        LastLap = crossingTime - playerLapStart;
                        if (CurrentLapEligible && (BestLap <= 0f || LastLap < BestLap)) BestLap = LastLap;
                        playerLapStart = crossingTime;
                        if (racer.ProgressTracker.CompletedLaps < TotalLaps) CurrentLapEligible = true;
                    }
                }
                if (racer.ProgressTracker.CompletedLaps >= TotalLaps) stepFinishers.Add(racer);
            }
            stepFinishers.Sort((a, b) => a.ProgressTracker.LastCrossingFraction.CompareTo(b.ProgressTracker.LastCrossingFraction));
            RacerFinishRecord playerFinish = lifecycle.Records.FirstOrDefault(value => value.IsPlayer && value.Status == RacerResultStatus.Finished);
            bool playerFinishedThisStep = playerFinish != null && Phase != RacePhase.Finished;
            foreach (HoverVehicle racer in stepFinishers)
            {
                float crossingTime = tickStart + racer.ProgressTracker.LastCrossingFraction * Time.fixedDeltaTime;
                RacerFinishRecord record = lifecycle.RecordFinish(racerIds[racer], crossingTime);
                if (record == null) continue;
                finishOrder.Add(racer);
                if (racer.Body) { racer.Body.isKinematic = true; racer.Body.detectCollisions = false; }
                RacerResolved?.Invoke(record);
                if (racer == Player)
                {
                    playerFinishedThisStep = true;
                    playerFinish = record;
                }
            }
            lifecycle.FinalizeTimeouts();
            UpdatePosition();
            if (playerFinishedThisStep)
            {
                TryStorePlayerRecord(playerFinish.FinishTime);
                Phase = RacePhase.Finished;
                RaceTime = lifecycle.PlayerPresentationTime;
                ApplySimulationLocks();
            }
            else if (Phase == RacePhase.Finished)
            {
                foreach (RacerFinishRecord record in lifecycle.Records)
                {
                    if (record.Status != RacerResultStatus.DidNotFinish) continue;
                    HoverVehicle racer = racerIds.FirstOrDefault(value => value.Value == record.RacerId).Key;
                    if (racer && racer.Body && !racer.Body.isKinematic)
                    {
                        racer.Body.isKinematic = true;
                        racer.Body.detectCollisions = false;
                        RacerResolved?.Invoke(record);
                    }
                }
            }
        }

        void TryStorePlayerRecord(float finishTime)
        {
            if (BestLap <= 0f || finishTime <= 0f) return;
            try
            {
                if (records == null) records = new RaceRecords();
                CurrentRecordComparison = records.Submit(courseIdentity, DrivingRulesId, BestLap, finishTime, !ManualRunEligible);
                PersonalBest = records.GetBest(courseIdentity, DrivingRulesId);
                RecordSaveError = string.IsNullOrEmpty(records.LastSaveError) ? "" : "Personal best could not be saved.";
            }
            catch (Exception error)
            {
                RecordSaveError = "Personal best could not be saved.";
                Debug.LogError("Unable to persist race record: " + error.Message);
            }
        }

        internal void NotifyRecovery(HoverVehicle racer)
        {
            if (racer == Player && (Phase == RacePhase.Racing || Phase == RacePhase.Paused)) CurrentLapEligible = false;
        }

        void ObserveAutomationProvenance()
        {
            if ((Phase == RacePhase.Countdown || Phase == RacePhase.Racing || Phase == RacePhase.Paused) &&
                Player && Player.AutopilotForTesting)
                ManualRunEligible = false;
        }

        static bool HasEvidenceArgument()
        {
            foreach (string argument in Environment.GetCommandLineArgs())
                if (argument.IndexOf("evidence", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        public bool CanSimulate(HoverVehicle racer)
        {
            ObserveAutomationProvenance();
            if (!racer || lifecycle == null || !racerIds.TryGetValue(racer, out string id)) return false;
            if (Phase != RacePhase.Racing && Phase != RacePhase.Finished) return false;
            return lifecycle.CanSimulate(id);
        }

        public bool IsFinished(HoverVehicle racer)
        {
            return racer && lifecycle != null && racerIds.TryGetValue(racer, out string id) && lifecycle.IsResolved(id);
        }

        public static bool IsValidGateSample(TrackFrame frame, Vector3 position, Vector3 velocity, float width, float hoverHeight)
        {
            Vector3 offset = position - frame.Position;
            float lateral = Vector3.Dot(offset, frame.Right);
            float height = Vector3.Dot(offset, frame.Up);
            float forwardSpeed = Vector3.Dot(velocity, frame.Forward);
            return Mathf.Abs(lateral) <= width * .5f + .75f && height >= .25f && height <= hoverHeight + 2f && forwardSpeed > .5f;
        }

        void UpdatePosition()
        {
            Position = 1;
            if (!Player) return;
            int playerFinish = finishOrder.IndexOf(Player);
            if (playerFinish >= 0) { Position = playerFinish + 1; return; }
            foreach (var racer in Racers)
                if (racer && racer != Player && (finishOrder.Contains(racer) || racer.RaceProgress > Player.RaceProgress + .00001f)) Position++;
        }

        void FreezeVehicles(bool freeze)
        {
            foreach (var racer in Racers)
                if (racer && racer.Body)
                {
                    racer.Body.isKinematic = freeze;
                    if (!freeze) racer.Body.detectCollisions = true;
                }
        }

        void ApplySimulationLocks()
        {
            foreach (HoverVehicle racer in Racers)
                if (racer && racer.Body) racer.Body.isKinematic = !CanSimulate(racer);
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; Time.timeScale = 1f; }
        }
    }
}
