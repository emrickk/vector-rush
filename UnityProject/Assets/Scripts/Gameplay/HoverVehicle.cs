using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class HoverVehicle : MonoBehaviour
    {
        [Header("Flight")]
        public float HoverHeight = 1.55f;
        public float CruiseSpeed = 78f;
        public float BoostSpeed = 108f;
        public float Acceleration = 34f;
        public Transform VisualRoot;
        [Tooltip("Automated validation only: use AI inputs through the same player physics.")]
        public bool AutopilotForTesting = false;
        public string DisplayName { get; set; } = "PILOT";
        public bool IsPlayer { get; private set; }
        public float SpeedKph => Body ? Body.linearVelocity.magnitude * 3.6f : 0f;
        public float Boost01 { get; private set; } = 1f;
        public bool IsBoosting { get; private set; }
        public float RaceProgress => ProgressTracker != null ? ProgressTracker.Distance : 0f;
        public RaceProgress ProgressTracker { get; private set; }
        public Rigidbody Body { get; private set; }
        public float TrackProgress { get; private set; }
        public bool IsGrounded { get; private set; }
        public float LastImpactTime { get; private set; } = -100f;
        public float LastImpactStrength { get; private set; }
        public int RecoveryCount { get; private set; }

        TrackPath track;
        int gridIndex;
        float steering, throttle, brake, leftBrake, rightBrake;
        bool boostHeld, recoveryRequested, boostExhausted, aiBoostLatch;
        float lostTime, stoppedTime, visualBank;
        float aiLane, nextLaneDecision;
        Quaternion originalVisualRotation;
        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly Vector3[] pads = { new Vector3(-1.5f,0,2.2f), new Vector3(1.5f,0,2.2f), new Vector3(-1.5f,0,-2.2f), new Vector3(1.5f,0,-2.2f) };

        public void Initialize(TrackPath circuit, bool isPlayer, int index)
        {
            track = circuit;
            IsPlayer = isPlayer;
            gridIndex = index;
            Body = GetComponent<Rigidbody>();
            Body.mass = 850f;
            Body.useGravity = false;
            Body.linearDamping = .05f;
            Body.angularDamping = .5f;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.centerOfMass = new Vector3(0,-.4f,0);
            if (!VisualRoot && transform.childCount > 0) VisualRoot = transform.GetChild(0);
            if (VisualRoot) originalVisualRotation = VisualRoot.localRotation;
            DisplayName = isPlayer ? "YOU" : new[] { "KIRA", "NOVA", "RENN", "AXIOM", "SOL" }[Mathf.Clamp(index - 1, 0, 4)];
            ResetForRace();
        }

        public void ResetForRace()
        {
            if (!track || !Body) return;
            float progress = Mathf.Repeat(-(14f + (gridIndex / 2) * 12f) / track.Length, 1f);
            ProgressTracker = new RaceProgress(progress);
            Boost01 = 1f;
            IsBoosting = false;
            boostExhausted = aiBoostLatch = recoveryRequested = false;
            lostTime = stoppedTime = 0f;
            RecoveryCount = 0;
            aiLane = (gridIndex % 2 == 0 ? -1f : 1f) * Mathf.Min(5.8f, track.Width * .265f);
            nextLaneDecision = 0f;
            Reposition(progress, gridIndex % 2 == 0 ? -4f : 4f);
        }

        void Update()
        {
            // Update still runs while paused; FixedUpdate does not. Never retain a
            // recover press (or held drive controls) from a non-racing screen.
            var director = RaceDirector.Instance;
            if (!director || director.Phase != RacePhase.Racing)
            {
                steering = throttle = brake = leftBrake = rightBrake = 0f;
                boostHeld = recoveryRequested = false;
                return;
            }
            if (!IsPlayer || AutopilotForTesting) return;
            steering = throttle = brake = leftBrake = rightBrake = 0f;
            boostHeld = false;
#if ENABLE_LEGACY_INPUT_MANAGER
            steering = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            throttle = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f;
            brake = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f;
            leftBrake = Input.GetKey(KeyCode.Q) ? 1f : 0f;
            rightBrake = Input.GetKey(KeyCode.E) ? 1f : 0f;
            boostHeld = Input.GetKey(KeyCode.Space);
            recoveryRequested |= Input.GetKeyDown(KeyCode.R);
#endif
#if ENABLE_INPUT_SYSTEM
#if !ENABLE_LEGACY_INPUT_MANAGER
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                steering = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
                throttle = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f;
                brake = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f;
                leftBrake = keyboard.qKey.isPressed ? 1f : 0f;
                rightBrake = keyboard.eKey.isPressed ? 1f : 0f;
                boostHeld = keyboard.spaceKey.isPressed;
                recoveryRequested |= keyboard.rKey.wasPressedThisFrame;
            }
#endif
            var pad = Gamepad.current;
            if (pad != null)
            {
                if (Mathf.Abs(pad.leftStick.x.ReadValue()) > Mathf.Abs(steering)) steering = pad.leftStick.x.ReadValue();
                throttle = Mathf.Max(throttle, pad.rightTrigger.ReadValue());
                brake = Mathf.Max(brake, pad.leftTrigger.ReadValue());
                leftBrake = Mathf.Max(leftBrake, pad.leftShoulder.ReadValue());
                rightBrake = Mathf.Max(rightBrake, pad.rightShoulder.ReadValue());
                boostHeld |= pad.buttonSouth.isPressed;
                recoveryRequested |= pad.buttonNorth.wasPressedThisFrame;
            }
#endif
        }

        void FixedUpdate()
        {
            if (!track || !Body) return;
            var director = RaceDirector.Instance;
            bool racing = director && director.Phase == RacePhase.Racing;
            if (!racing)
            {
                IsBoosting = false;
                recoveryRequested = false;
                return;
            }
            TrackProgress = track.ClosestProgress(Body.position);
            var frame = track.Evaluate(TrackProgress);
            if (!IsPlayer || AutopilotForTesting) DriveAI(frame);
            Vector3 velocity = Body.linearVelocity;
            Vector3 up = frame.Up;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, up).normalized;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            float speed = Vector3.Dot(velocity, forward);

            int groundedPads = 0;
            foreach (var point in pads)
            {
                Vector3 origin = transform.TransformPoint(point) + up * .4f;
                int count = Physics.RaycastNonAlloc(origin, -up, hits, HoverHeight + 3.2f, ~0, QueryTriggerInteraction.Ignore);
                float distance = float.PositiveInfinity;
                for (int i = 0; i < count; i++)
                    if (hits[i].rigidbody == null && Vector3.Dot(hits[i].normal, up) > .35f && hits[i].distance < distance)
                        distance = hits[i].distance;
                if (float.IsPositiveInfinity(distance)) continue;
                groundedPads++;
                float verticalSpeed = Vector3.Dot(Body.GetPointVelocity(origin), up);
                float force = Mathf.Clamp((HoverHeight + .4f - distance) * 65f - verticalSpeed * 11f + 22f, -18f, 130f);
                Body.AddForceAtPosition(up * force * .25f, origin, ForceMode.Acceleration);
            }
            IsGrounded = groundedPads >= 2;
            Body.AddForce(-up * 22f, ForceMode.Acceleration);
            Vector3 tiltError = Vector3.Cross(transform.up, up);
            Vector3 tiltVelocity = Vector3.ProjectOnPlane(Body.angularVelocity, up);
            Body.AddTorque(tiltError * 28f - tiltVelocity * 8f, ForceMode.Acceleration);
            float turn = Mathf.Clamp(steering + (rightBrake - leftBrake) * .65f, -1.65f, 1.65f);
            float targetYaw = turn * 1.12f * Mathf.Lerp(.25f, 1f, Mathf.Clamp01(Mathf.Abs(speed) / 26f));
            Body.AddTorque(up * (targetYaw - Vector3.Dot(Body.angularVelocity, up)) * 7f, ForceMode.Acceleration);

            if (!boostHeld) boostExhausted = false;
            if (Boost01 <= .01f) boostExhausted = true;
            IsBoosting = boostHeld && !boostExhausted && Boost01 > .01f && throttle > .1f && IsGrounded;
            Boost01 = Mathf.Clamp01(Boost01 + (IsBoosting ? -.23f : .12f) * Time.fixedDeltaTime);
            float topSpeed = IsBoosting ? BoostSpeed : CruiseSpeed;
            float engine = throttle * Acceleration * (IsBoosting ? 1.65f : 1f) * Mathf.Clamp01((topSpeed - speed) / 16f);
            float drag = 2f + speed * speed * .0017f;
            if (IsGrounded)
            {
                Body.AddForce(forward * (engine - Mathf.Sign(speed) * (drag + brake * 47f + (leftBrake + rightBrake) * 7f)), ForceMode.Acceleration);
                float slip = Vector3.Dot(velocity, right);
                Body.AddForce(-right * slip * (leftBrake + rightBrake > .1f ? 2.6f : 4.2f), ForceMode.Acceleration);
            }
            else Body.AddForce(forward * engine * .15f, ForceMode.Acceleration);
            if (Body.linearVelocity.magnitude > BoostSpeed * 1.15f) Body.linearVelocity = Body.linearVelocity.normalized * BoostSpeed * 1.15f;
            lostTime = !IsGrounded ? lostTime + Time.fixedDeltaTime : 0f;
            stoppedTime = speed < 5f && throttle > .5f ? stoppedTime + Time.fixedDeltaTime : 0f;
            if (recoveryRequested) Recover("manual");
            else if (lostTime > 2.7f) Recover("airborne");
            else if (stoppedTime > 4.5f) Recover("stalled");
            else if (Vector3.Distance(Body.position, frame.Position) > track.Width * 2.2f) Recover("outside-track");
            recoveryRequested = false;
        }

        void DriveAI(TrackFrame frame)
        {
            float speed = Body.linearVelocity.magnitude;
            // Steering follows a short horizon; braking examines farther ahead independently.
            float lookahead = 12f + speed * .33f;
            var target = track.Evaluate(TrackProgress + lookahead / track.Length);
            float currentLane = Vector3.Dot(Body.position - frame.Position, frame.Right);
            float desiredSpeed = 75f + gridIndex * .5f;
            float previewStep = Mathf.Max(12f, speed * .42f);
            Vector3 previousHeading = frame.Forward;
            float strongestCurve = 0f;
            for (int sample = 1; sample <= 3; sample++)
            {
                Vector3 heading = track.Evaluate(TrackProgress + previewStep * sample / track.Length).Forward;
                strongestCurve = Mathf.Max(strongestCurve, Vector3.Angle(previousHeading, heading) * Mathf.Deg2Rad / previewStep);
                previousHeading = heading;
            }
            if (strongestCurve > .0001f)
                desiredSpeed = Mathf.Min(desiredSpeed, Mathf.Sqrt(30f / strongestCurve));

            var racers = RaceDirector.Instance.Racers;
            float nearestBlocker = float.PositiveInfinity;
            foreach (var other in racers)
            {
                if (!other || other == this || !other.Body) continue;
                float gap = SignedTrackGap(other);
                if (gap <= 0f || gap > 65f) continue;
                float otherLane = OtherLane(other);
                if (Mathf.Abs(otherLane - currentLane) > 5.5f && Mathf.Abs(otherLane - aiLane) > 5.5f) continue;
                nearestBlocker = Mathf.Min(nearestBlocker, gap);
                float leaderSpeed = Mathf.Max(0f, Vector3.Dot(other.Body.linearVelocity, track.Evaluate(other.TrackProgress).Forward));
                float followingGap = 11f + speed * .32f;
                desiredSpeed = Mathf.Min(desiredSpeed, Mathf.Max(0f, leaderSpeed + (gap - followingGap) * 1.6f));
            }
            if (nearestBlocker < 45f && Time.time >= nextLaneDecision)
            {
                float spacing = Mathf.Min(5.8f, track.Width * .265f);
                for (int laneIndex = -1; laneIndex <= 1; laneIndex++)
                {
                    float candidate = laneIndex * spacing;
                    if (Mathf.Abs(candidate - aiLane) < 1f || Mathf.Abs(candidate - aiLane) > spacing + .1f) continue;
                    bool clear = true;
                    foreach (var other in racers)
                    {
                        if (!other || other == this || !other.Body) continue;
                        float gap = SignedTrackGap(other);
                        if (gap < -24f || gap > 45f) continue;
                        float occupiedLane = OtherLane(other);
                        float reservedLane = !other.IsPlayer || other.AutopilotForTesting ? other.aiLane : occupiedLane;
                        if (Mathf.Abs(occupiedLane - candidate) < 5.5f || Mathf.Abs(reservedLane - candidate) < 5.5f)
                        { clear = false; break; }
                    }
                    if (!clear) continue;
                    aiLane = candidate;
                    nextLaneDecision = Time.time + 3f;
                    break;
                }
            }
            Vector3 destination = target.Position + target.Right * aiLane;
            Vector3 direction = Vector3.ProjectOnPlane(destination - Body.position, frame.Up).normalized;
            float angle = Vector3.SignedAngle(transform.forward, direction, frame.Up);
            steering = Mathf.Clamp(angle / 26f, -1f, 1f);
            float bend = Vector3.Angle(frame.Forward, target.Forward);
            // Reduce speed while badly misaligned, including after glancing wall contact.
            desiredSpeed *= Mathf.Lerp(1f, .45f, Mathf.InverseLerp(15f, 60f, Mathf.Abs(angle)));
            bool clearStraight = strongestCurve < .002f && bend < 7f && Mathf.Abs(angle) < 8f && nearestBlocker > 65f;
            if (!clearStraight || Boost01 < .1f) aiBoostLatch = false;
            else if (Boost01 > .65f) aiBoostLatch = true;
            boostHeld = aiBoostLatch;
            if (boostHeld) desiredSpeed = Mathf.Max(desiredSpeed, 96f);
            throttle = Mathf.Clamp01((desiredSpeed - speed) / 5f);
            brake = Mathf.Clamp01((speed - desiredSpeed) / 10f);
            leftBrake = angle < -32f ? .45f : 0f;
            rightBrake = angle > 32f ? .45f : 0f;
        }

        float SignedTrackGap(HoverVehicle other) => (Mathf.Repeat(other.TrackProgress - TrackProgress + .5f, 1f) - .5f) * track.Length;

        float OtherLane(HoverVehicle other)
        {
            var otherFrame = track.Evaluate(other.TrackProgress);
            return Vector3.Dot(other.Body.position - otherFrame.Position, otherFrame.Right);
        }

        public void Recover() => Recover("manual-or-test");

        void Recover(string reason)
        {
            if (!track || ProgressTracker == null) return;
            RecoveryCount++;
            float progress = ProgressTracker.RecoveryProgress;
            var oldFrame = track.Evaluate(TrackProgress);
            Vector3 offset = Body.position - oldFrame.Position;
            float raceTime = RaceDirector.Instance ? RaceDirector.Instance.RaceTime : 0f;
            Debug.Log($"VECTOR_RUSH_RECOVERY racer={DisplayName} reason={reason} raceTime={raceTime:F2} lap={ProgressTracker.CompletedLaps} from={TrackProgress:F5} to={progress:F5} speedKph={SpeedKph:F1} lateral={Vector3.Dot(offset, oldFrame.Right):F2} height={Vector3.Dot(offset, oldFrame.Up):F2} grounded={IsGrounded} airborneSeconds={lostTime:F2} stalledSeconds={stoppedTime:F2} impactAgo={Time.time - LastImpactTime:F2} impactSpeed={LastImpactStrength:F1} count={RecoveryCount}", this);
            Reposition(progress, IsPlayer && !AutopilotForTesting ? 0f : aiLane);
            ProgressTracker.NotifyRespawn(progress);
            lostTime = stoppedTime = 0f;
            Boost01 = Mathf.Max(0f, Boost01 - .15f);
        }

        void Reposition(float progress, float lateral)
        {
            var frame = track.Evaluate(progress);
            Body.position = frame.Position + frame.Up * HoverHeight + frame.Right * lateral;
            Body.rotation = Quaternion.LookRotation(frame.Forward, frame.Up);
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(Body.position, Body.rotation);
            TrackProgress = Mathf.Repeat(progress, 1f);
        }

        void LateUpdate()
        {
            if (!VisualRoot) return;
            visualBank = Mathf.Lerp(visualBank, -steering * 13f - (rightBrake - leftBrake) * 7f, 1f - Mathf.Exp(-8f * Time.deltaTime));
            VisualRoot.localRotation = originalVisualRotation * Quaternion.Euler(0,0,visualBank);
        }

        void OnCollisionEnter(Collision collision)
        {
            LastImpactTime = Time.time;
            LastImpactStrength = collision.relativeVelocity.magnitude;
        }
    }
}
