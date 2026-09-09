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
        public bool HasPendingRivals => lifecycle != null && lifecycle.HasPendingRivals;
        public IReadOnlyList<RacerFinishRecord> FinishRecords => lifecycle == null ? Array.Empty<RacerFinishRecord>() : lifecycle.Records;
        public RaceRecordComparison CurrentRecordComparison { get; private set; }
        public const string DrivingRulesId = "vector-rush-rules-v1";
        public event Action<RacerFinishRecord> RacerResolved;
        public event Action RaceRestarted;

        TrackPath track;
        RacePhase beforePause;
        float playerLapStart;
        bool playerCrossedStart;
        readonly List<HoverVehicle> finishOrder = new List<HoverVehicle>();
        readonly List<HoverVehicle> stepFinishers = new List<HoverVehicle>();
        readonly Dictionary<HoverVehicle, string> racerIds = new Dictionary<HoverVehicle, string>();
        RaceFinishLifecycle lifecycle;
        RaceRecords raceRecords;
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
            courseIdentity = string.IsNullOrWhiteSpace(courseHash) ? "legacy-solstice-course-v1" : courseHash;
            raceRecords = records;
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
            RaceTime = LastLap = BestLap = playerLapStart = 0f;
            playerCrossedStart = false;
            CountdownRemaining = 3f;
            Phase = RacePhase.Countdown;
            UpdatePosition();
            RaceRestarted?.Invoke();
        }

        public void RestartRace() => StartRace();

        public void TogglePause()
        {
            if (Phase == RacePhase.Paused)
            {
                Phase = beforePause;
                Time.timeScale = 1f;
            }
            else if (Phase == RacePhase.Racing || Phase == RacePhase.Countdown)
            {
                beforePause = Phase;
                Phase = RacePhase.Paused;
                Time.timeScale = 0f;
            }
        }

        void Update()
        {
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
            if (pausePressed && !RaceHUD.KeyboardCaptureActive) TogglePause();
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
            lifecycle.Advance(Time.fixedDeltaTime);
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
                    }
                    if (lapComplete)
                    {
                        LastLap = crossingTime - playerLapStart;
                        if (BestLap <= 0f || LastLap < BestLap) BestLap = LastLap;
                        playerLapStart = crossingTime;
                    }
                }
                if (racer.ProgressTracker.CompletedLaps >= TotalLaps) stepFinishers.Add(racer);
            }
            stepFinishers.Sort((a, b) => a.ProgressTracker.LastCrossingFraction.CompareTo(b.ProgressTracker.LastCrossingFraction));
            bool playerFinishedThisStep = false;
            foreach (HoverVehicle racer in stepFinishers)
            {
                float crossingTime = tickStart + racer.ProgressTracker.LastCrossingFraction * Time.fixedDeltaTime;
                RacerFinishRecord record = lifecycle.RecordFinish(racerIds[racer], crossingTime);
                finishOrder.Add(racer);
                if (racer.Body) racer.Body.isKinematic = true;
                RacerResolved?.Invoke(record);
                if (racer == Player)
                {
                    playerFinishedThisStep = true;
                    TryStorePlayerRecord(record.FinishTime);
                }
            }
            UpdatePosition();
            if (playerFinishedThisStep)
            {
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
                        RacerResolved?.Invoke(record);
                    }
                }
            }
        }

        void TryStorePlayerRecord(float finishTime)
        {
            if (BestLap <= 0f || finishTime <= 0f) return;
            bool automated = Player && Player.AutopilotForTesting;
            foreach (string argument in Environment.GetCommandLineArgs())
                if (argument.IndexOf("evidence", StringComparison.OrdinalIgnoreCase) >= 0) { automated = true; break; }
            try
            {
                if (raceRecords == null) raceRecords = new RaceRecords();
                CurrentRecordComparison = raceRecords.Submit(courseIdentity, DrivingRulesId, BestLap, finishTime, automated);
            }
            catch (Exception error) { Debug.LogError("Unable to persist race record: " + error.Message); }
        }

        public bool CanSimulate(HoverVehicle racer)
        {
            if (!racer || lifecycle == null || !racerIds.TryGetValue(racer, out string id)) return false;
            if (Phase != RacePhase.Racing && Phase != RacePhase.Finished) return false;
            return lifecycle.CanSimulate(id);
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
                if (racer && racer.Body) racer.Body.isKinematic = freeze;
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
