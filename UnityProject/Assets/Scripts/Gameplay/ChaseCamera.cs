using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    public static class ChaseCameraMotion
    {
        public static float Speed01(float speedKph) => Mathf.Clamp01(speedKph / 380f);

        public static float TargetFov(float speed01, float boost01, bool legacy)
        {
            float speedRange = legacy ? 10f : 12.5f;
            return 65f + Mathf.Clamp01(speed01) * speedRange + Mathf.Clamp01(boost01) * (legacy ? 5f : 5.5f);
        }

        public static float FocusLead(float speed01, float turn01, bool legacy)
        {
            if (legacy) return 8f + Mathf.Clamp01(speed01) * 5f;
            return 10f + Mathf.Clamp01(speed01) * 7.5f - Mathf.Abs(Mathf.Clamp(turn01, -1f, 1f)) * 1.25f;
        }

        public static float FollowSmoothTime(float speed01, float turn01)
        {
            return Mathf.Lerp(.105f, .072f, Mathf.Clamp01(speed01)) + Mathf.Abs(Mathf.Clamp(turn01, -1f, 1f)) * .018f;
        }
    }

    [RequireComponent(typeof(Camera))]
    public sealed class ChaseCamera : MonoBehaviour
    {
        public bool ShakeEnabled = true;
        public bool MotionPresentationEnabled { get; set; }
        public bool LookBackRequested { get; set; }
        public float ViewBlend { get; private set; }
        public bool IsViewTransitioning => viewCooldown > 0f;
        float filteredTurn, viewCooldown;

        public static Vector3 ViewOffset(float viewBlend, float distance, float height)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(viewBlend));
            return new Vector3(Mathf.Sin(t * Mathf.PI) * 6f, height + Mathf.Sin(t * Mathf.PI) * 2f, -Mathf.Cos(t * Mathf.PI) * distance);
        }
        public float Distance = 10.5f;
        public float Height = 4.6f;
        public bool LegacyComparisonMode { get; private set; }
        HoverVehicle target;
        Camera lens;
        VehicleVFX vehicleVfx;
        Vector3 positionVelocity;
        Vector3 smoothedForward, smoothedUp, previousHeading;
        float boostEnvelope, boostImpulse, impactImpulse;
        bool wasBoosting, subscribed;
        bool snap = true;

        public void Initialize(HoverVehicle vehicle)
        {
            target = vehicle;
            ShakeEnabled = PlayerPreferences.Current.ShakeEnabled;
            lens = GetComponent<Camera>();
            lens.nearClipPlane = .18f;
            lens.farClipPlane = 5000f;
            smoothedForward = vehicle.transform.forward;
            smoothedUp = vehicle.transform.up;
            previousHeading = smoothedForward;
            boostEnvelope = boostImpulse = impactImpulse = 0f;
            wasBoosting = vehicle.IsBoosting;
            vehicleVfx = vehicle.GetComponent<VehicleVFX>();
            Subscribe();
            snap = true;
        }

        void OnEnable() { Subscribe(); }
        void OnDisable() { Unsubscribe(); }
        void OnDestroy() { Unsubscribe(); }

        void Subscribe()
        {
            if (subscribed || !vehicleVfx) return;
            vehicleVfx.ContactFeedback += OnContactFeedback;
            subscribed = true;
        }

        void Unsubscribe()
        {
            if (subscribed && vehicleVfx) vehicleVfx.ContactFeedback -= OnContactFeedback;
            subscribed = false;
        }

        void OnContactFeedback(VehicleContactSignal signal)
        {
            if (signal.Phase == VehicleContactPhase.Exit) return;
            impactImpulse = Mathf.Max(impactImpulse, Mathf.Max(signal.NormalIntensity01, signal.TangentialIntensity01 * .55f));
        }

        public void SetLegacyComparisonMode(bool enabled)
        {
            LegacyComparisonMode = enabled;
            snap = true;
        }

        void LateUpdate()
        {
            if (!target) return;
            float delta = Time.deltaTime;
            if (delta <= 0f) return;
            Transform craft = target.transform;
            float speed = ChaseCameraMotion.Speed01(target.SpeedKph);
            Vector3 heading = craft.forward;
            if (target.Body && target.Body.linearVelocity.sqrMagnitude > 400f)
                heading = Vector3.Slerp(heading, target.Body.linearVelocity.normalized, .15f);
            float signedTurn = Vector3.SignedAngle(previousHeading, heading, craft.up) / Mathf.Max(delta, .0001f);
            float turn01 = Mathf.Clamp(signedTurn / 95f, -1f, 1f);
            bool reduced = PlayerPreferences.Current.ReducedInterfaceMotion;
            bool presentation = MotionPresentationEnabled && !LegacyComparisonMode;
            if (presentation)
            {
                filteredTurn = Mathf.Lerp(filteredTurn, turn01, 1f - Mathf.Exp(-7f * delta));
                turn01 = filteredTurn;
                bool held = LookBackRequested;
#if ENABLE_LEGACY_INPUT_MANAGER
                held |= Input.GetKey(KeyCode.Tab);
#endif
#if ENABLE_INPUT_SYSTEM
                held |= Keyboard.current != null && Keyboard.current.tabKey.isPressed;
                held |= Gamepad.current != null && Gamepad.current.rightStickButton.isPressed;
#endif
                bool racing = RaceDirector.Instance && RaceDirector.Instance.Phase == RacePhase.Racing;
                float before = ViewBlend;
                ViewBlend = racing ? (reduced ? (held ? 1f : 0f) : Mathf.MoveTowards(ViewBlend, held ? 1f : 0f, delta / .38f)) : 0f;
                viewCooldown = Mathf.Abs(before - ViewBlend) > .0001f ? .18f : Mathf.Max(0, viewCooldown - delta);
            }
            previousHeading = heading;
            float headingBlend = 1f - Mathf.Exp(-(LegacyComparisonMode ? 7f : 6.2f) * delta);
            float upBlend = 1f - Mathf.Exp(-(LegacyComparisonMode ? 7f : 4.8f) * delta);
            smoothedForward = Vector3.Slerp(smoothedForward, heading, headingBlend);
            // The rigidbody follows road banking while the ship's extra steering bank lives on
            // VisualRoot. Reading craft.up here therefore applies the road roll exactly once.
            Vector3 desiredUp = presentation ? Vector3.Slerp(Vector3.up, craft.up, reduced ? .65f : .82f) : craft.up;
            smoothedUp = Vector3.Slerp(smoothedUp, desiredUp, upBlend);
            bool boosting = target.IsBoosting;
            if (boosting && !wasBoosting) boostImpulse = 1f;
            wasBoosting = boosting;
            boostEnvelope = Mathf.Lerp(boostEnvelope, boosting ? 1f : 0f,
                1f - Mathf.Exp(-(boosting ? 8.5f : 4.2f) * delta));
            boostImpulse = Mathf.MoveTowards(boostImpulse, 0f, delta / .28f);
            impactImpulse = Mathf.MoveTowards(impactImpulse, 0f, delta / .38f);
            float focusLead = ChaseCameraMotion.FocusLead(speed, turn01, LegacyComparisonMode);
            Vector3 turnLook = craft.right * turn01 * (LegacyComparisonMode ? 0f : 2.2f);
            Vector3 focus = craft.position + smoothedUp * 1.0f + smoothedForward * focusLead + turnLook;
            Vector3 desired = craft.position - smoothedForward * (Distance + speed * .75f) + smoothedUp * Height;
            if (presentation)
            {
                float look = Mathf.SmoothStep(0, 1, ViewBlend);
                Vector3 offset = ViewOffset(ViewBlend, 9.2f + speed * 1.1f + boostEnvelope * .6f, 3.25f);
                desired = craft.position + smoothedForward * offset.z + smoothedUp * offset.y + Vector3.Cross(smoothedUp, smoothedForward).normalized * offset.x;
                focus = craft.position + smoothedUp * 1.1f + smoothedForward * Mathf.Lerp(9f + speed * 5f, 0f, Mathf.SmoothStep(0, 1, Mathf.Min(1, ViewBlend * 4)))
                    + craft.right * turn01 * (reduced ? 0 : 1.5f) * (1 - look);
            }
            if (snap || Vector3.Distance(transform.position, desired) > 70f)
            {
                transform.position = desired;
                positionVelocity = Vector3.zero;
                snap = false;
            }
            else if (presentation && IsViewTransitioning) { transform.position = desired; positionVelocity = Vector3.zero; }
            else transform.position = Vector3.SmoothDamp(transform.position, desired, ref positionVelocity,
                presentation ? .055f : LegacyComparisonMode ? .075f : ChaseCameraMotion.FollowSmoothTime(speed, turn01));
            Quaternion rotation = Quaternion.LookRotation(focus - transform.position, smoothedUp);
            if (ShakeEnabled && !(presentation && reduced) && !IsViewTransitioning)
            {
                // Short event impulses replace continuous boost shake. The route stays readable
                // at sustained speed and disabling shake removes both boost and impact impulses.
                float impulse = boostImpulse * .16f + impactImpulse * .72f;
                rotation *= Quaternion.Euler(Mathf.Sin(Time.time * 67f) * impulse,
                    Mathf.Sin(Time.time * 47f + 1.3f) * impulse * .62f, 0f);
            }
            transform.rotation = rotation;
            float targetFov = ChaseCameraMotion.TargetFov(speed, boostEnvelope, LegacyComparisonMode);
            if (presentation) targetFov = reduced ? 70f : 65f + speed * 16f + boostEnvelope * 4f;
            lens.fieldOfView = Mathf.Lerp(lens.fieldOfView, targetFov,
                1f - Mathf.Exp(-(targetFov > lens.fieldOfView ? 4.8f : 3.3f) * delta));
        }
    }
}
