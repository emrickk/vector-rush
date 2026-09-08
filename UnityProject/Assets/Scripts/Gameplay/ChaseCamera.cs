using UnityEngine;

namespace VectorRush
{
    [RequireComponent(typeof(Camera))]
    public sealed class ChaseCamera : MonoBehaviour
    {
        public bool ShakeEnabled = true;
        public float Distance = 10.5f;
        public float Height = 4.6f;
        HoverVehicle target;
        Camera lens;
        Vector3 positionVelocity;
        Vector3 smoothedForward, smoothedUp;
        bool snap = true;

        public void Initialize(HoverVehicle vehicle)
        {
            target = vehicle;
            lens = GetComponent<Camera>();
            lens.nearClipPlane = .18f;
            lens.farClipPlane = 5000f;
            smoothedForward = vehicle.transform.forward;
            smoothedUp = vehicle.transform.up;
            snap = true;
        }

        void LateUpdate()
        {
            if (!target) return;
            float delta = Time.deltaTime;
            if (delta <= 0f) return;
            Transform craft = target.transform;
            float speed = Mathf.Clamp01(target.SpeedKph / 380f);
            Vector3 heading = craft.forward;
            if (target.Body && target.Body.linearVelocity.sqrMagnitude > 400f)
                heading = Vector3.Slerp(heading, target.Body.linearVelocity.normalized, .15f);
            float blend = 1f - Mathf.Exp(-7f * delta);
            smoothedForward = Vector3.Slerp(smoothedForward, heading, blend);
            smoothedUp = Vector3.Slerp(smoothedUp, craft.up, blend);
            Vector3 focus = craft.position + smoothedUp * 1.0f + smoothedForward * (8f + speed * 5f);
            Vector3 desired = craft.position - smoothedForward * (Distance + speed * .75f) + smoothedUp * Height;
            if (snap || Vector3.Distance(transform.position, desired) > 70f)
            {
                transform.position = desired;
                positionVelocity = Vector3.zero;
                snap = false;
            }
            else transform.position = Vector3.SmoothDamp(transform.position, desired, ref positionVelocity, .075f);
            Quaternion rotation = Quaternion.LookRotation(focus - transform.position, smoothedUp);
            if (ShakeEnabled)
            {
                float impact = Mathf.Clamp01(1f - (Time.time - target.LastImpactTime) / .35f) * Mathf.Min(target.LastImpactStrength / 20f, 1f);
                float amplitude = (target.IsBoosting ? .22f : .04f * speed) + impact * .6f;
                rotation *= Quaternion.Euler(Mathf.Sin(Time.time * 71f) * amplitude, Mathf.Sin(Time.time * 53f) * amplitude * .6f, 0f);
            }
            transform.rotation = rotation;
            lens.fieldOfView = Mathf.Lerp(lens.fieldOfView, 65f + speed * 10f + (target.IsBoosting ? 5f : 0f), 1f - Mathf.Exp(-3f * delta));
        }
    }
}
