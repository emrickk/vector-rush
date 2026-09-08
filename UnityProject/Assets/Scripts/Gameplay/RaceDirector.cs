using System.Collections.Generic;
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
        public float FinishTime => RaceTime;

        TrackPath track;
        RacePhase beforePause;
        float playerLapStart;
        bool playerCrossedStart;
        readonly List<HoverVehicle> finishOrder = new List<HoverVehicle>();
        readonly List<HoverVehicle> stepFinishers = new List<HoverVehicle>();

        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(TrackPath circuit, List<HoverVehicle> vehicles)
        {
            track = circuit;
            Racers = vehicles ?? new List<HoverVehicle>();
            Player = Racers.Find(racer => racer && racer.IsPlayer);
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
            RaceTime = LastLap = BestLap = playerLapStart = 0f;
            playerCrossedStart = false;
            CountdownRemaining = 3f;
            Phase = RacePhase.Countdown;
            UpdatePosition();
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
            pausePressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
#if ENABLE_INPUT_SYSTEM
#if !ENABLE_LEGACY_INPUT_MANAGER
            pausePressed |= Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);
#endif
            pausePressed |= Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
#endif
            if (pausePressed) TogglePause();
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
            if (Phase != RacePhase.Racing || !track) return;
            float tickStart = RaceTime;
            RaceTime += Time.fixedDeltaTime;
            stepFinishers.Clear();
            foreach (var racer in Racers)
            {
                if (!racer || finishOrder.Contains(racer)) continue;
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
            finishOrder.AddRange(stepFinishers);
            UpdatePosition();
            if (Player && finishOrder.Contains(Player))
            {
                Phase = RacePhase.Finished;
                RaceTime = tickStart + Player.ProgressTracker.LastCrossingFraction * Time.fixedDeltaTime;
                FreezeVehicles(true);
            }
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

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; Time.timeScale = 1f; }
        }
    }
}
