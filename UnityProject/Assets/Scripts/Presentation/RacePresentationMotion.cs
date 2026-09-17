using UnityEngine;

namespace VectorRush
{
    public enum PresentationCuePriority { None, Boost, Position, Contact }

    public readonly struct MenuMotionSample
    {
        public float Visibility { get; }
        public float OffsetX { get; }

        public MenuMotionSample(float visibility, float offsetX)
        {
            Visibility = visibility;
            OffsetX = offsetX;
        }
    }

    /// <summary>Pure, frame-rate-independent presentation timing shared by the IMGUI HUD.</summary>
    public static class RacePresentationMotion
    {
        public const float MenuEnterSeconds = .34f;
        public const float MenuExitSeconds = .16f;
        public const float PositionAttackSeconds = .11f;
        public const float PositionHoldSeconds = .45f;
        public const float PositionExitSeconds = .17f;
        public const float PositionCueSeconds = PositionAttackSeconds + PositionHoldSeconds + PositionExitSeconds;

        public static MenuMotionSample Menu(float screenElapsed, float exitElapsed, bool exiting, bool reducedMotion)
        {
            if (reducedMotion) return new MenuMotionSample(1f, 0f);
            float enter = EaseOut(Mathf.InverseLerp(0f, MenuEnterSeconds, Mathf.Max(0f, screenElapsed)));
            float exit = exiting ? EaseIn(Mathf.InverseLerp(0f, MenuExitSeconds, Mathf.Max(0f, exitElapsed))) : 0f;
            return new MenuMotionSample(Mathf.Clamp01(enter * (1f - exit)), Mathf.Lerp(-32f, 0f, enter) + exit * 24f);
        }

        public static float SelectionEmphasis(float elapsed, bool reducedMotion)
        {
            if (reducedMotion) return 1f;
            return EaseOut(Mathf.InverseLerp(0f, .14f, Mathf.Max(0f, elapsed)));
        }

        public static float PositionEnvelope(float elapsed, bool reducedMotion)
        {
            return CueEnvelope(elapsed, PositionAttackSeconds, PositionHoldSeconds, PositionExitSeconds, reducedMotion);
        }

        public static float CueEnvelope(float elapsed, float attack, float hold, float release, bool reducedMotion)
        {
            if (elapsed < 0f) return 0f;
            attack = Mathf.Max(0f, attack);
            hold = Mathf.Max(0f, hold);
            release = Mathf.Max(0f, release);
            if (reducedMotion) return elapsed < attack + hold + release ? 1f : 0f;
            if (attack > 0f && elapsed < attack) return EaseOut(elapsed / attack);
            elapsed -= attack;
            if (elapsed < hold) return 1f;
            elapsed -= hold;
            return release > 0f && elapsed < release ? 1f - EaseIn(elapsed / release) : 0f;
        }

        public static float Approach(float current, float target, float deltaTime, float risePerSecond, float fallPerSecond, bool reducedMotion)
        {
            if (reducedMotion) return target;
            float rate = target > current ? risePerSecond : fallPerSecond;
            return Mathf.MoveTowards(current, target, Mathf.Max(0f, deltaTime) * Mathf.Max(0f, rate));
        }

        public static PresentationCuePriority SelectPriority(bool contact, bool position, bool boost)
        {
            if (contact) return PresentationCuePriority.Contact;
            if (position) return PresentationCuePriority.Position;
            return boost ? PresentationCuePriority.Boost : PresentationCuePriority.None;
        }

        static float EaseOut(float value)
        {
            float t = Mathf.Clamp01(value);
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        static float EaseIn(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * t;
        }
    }
}
