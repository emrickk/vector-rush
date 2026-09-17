using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VectorRush
{
    public static class SpeedMotionBlurModel
    {
        public static float PresentationIntensity(float speed01, float boost01, bool reducedMotion)
        {
            if (reducedMotion) return 0;
            return Mathf.Clamp(Mathf.SmoothStep(0f, .32f, Mathf.InverseLerp(.12f, .72f, speed01)) + Mathf.Clamp01(boost01) * .08f, 0f, .4f);
        }

        public static float TargetIntensity(float speed01, float boost01, bool reducedMotion)
        {
            if (reducedMotion) return 0f;
            float peripheralEnergy = Mathf.SmoothStep(0f, .22f, Mathf.InverseLerp(.42f, 1f, speed01));
            return Mathf.Clamp(peripheralEnergy + Mathf.Clamp01(boost01) * .10f, 0f, .32f);
        }
    }

    /// <summary>
    /// Owns the speed/boost envelope for an integration-owned URP MotionBlur override.
    /// Task 01 creates the runtime Volume and calls Initialize; this component never
    /// changes renderer settings, motion-vector settings, or persistent preferences.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class SpeedMotionBlur : MonoBehaviour
    {
        HoverVehicle vehicle;
        MotionBlur blur;
        float boostEnvelope;
        ChaseCamera chase;

        public float CurrentIntensity { get; private set; }

        public void Initialize(HoverVehicle craft, MotionBlur motionBlur)
        {
            vehicle = craft;
            blur = motionBlur;
            chase = GetComponent<ChaseCamera>();
            if (chase && chase.MotionPresentationEnabled && !chase.LegacyComparisonMode && blur != null)
            {
                blur.mode.Override(MotionBlurMode.CameraAndObjects);
                blur.quality.Override(MotionBlurQuality.High);
                blur.clamp.Override(.035f);
            }
            ResetState();
        }

        void LateUpdate()
        {
            if (!vehicle || blur == null) return;
            RacePhase phase = RaceDirector.Instance ? RaceDirector.Instance.Phase : RacePhase.Menu;
            if (phase == RacePhase.Paused) return;
            if (phase != RacePhase.Racing)
            {
                ResetState();
                return;
            }

            boostEnvelope = Mathf.Lerp(boostEnvelope, vehicle.IsBoosting ? 1f : 0f,
                1f - Mathf.Exp(-(vehicle.IsBoosting ? 7f : 3.8f) * Time.deltaTime));
            float speed = ChaseCameraMotion.Speed01(vehicle.SpeedKph);
            float target = SpeedMotionBlurModel.TargetIntensity(speed, boostEnvelope,
                PlayerPreferences.Current.ReducedInterfaceMotion);
            if (chase && chase.MotionPresentationEnabled && !chase.LegacyComparisonMode)
            {
                target = SpeedMotionBlurModel.PresentationIntensity(speed, boostEnvelope, PlayerPreferences.Current.ReducedInterfaceMotion);
                if (chase.IsViewTransitioning) { CurrentIntensity = 0; Apply(); return; }
            }
            CurrentIntensity = Mathf.Lerp(CurrentIntensity, target,
                1f - Mathf.Exp(-(target > CurrentIntensity ? 5.5f : 3.5f) * Time.deltaTime));
            Apply();
        }

        public void ResetState()
        {
            boostEnvelope = CurrentIntensity = 0f;
            Apply();
        }

        void Apply()
        {
            if (blur == null) return;
            blur.active = CurrentIntensity > .001f;
            blur.intensity.Override(CurrentIntensity);
        }

        void OnDisable() { ResetState(); }
    }
}
