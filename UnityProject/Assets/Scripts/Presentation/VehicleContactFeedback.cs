using UnityEngine;

namespace VectorRush
{
    public enum VehicleContactPhase { Enter, Stay, Exit }

    public readonly struct VehicleContactKey : System.IEquatable<VehicleContactKey>
    {
        public EntityId VehicleId { get; }
        public EntityId OtherColliderId { get; }

        public VehicleContactKey(EntityId vehicleId, EntityId otherColliderId)
        {
            VehicleId = vehicleId;
            OtherColliderId = otherColliderId;
        }

        public bool Equals(VehicleContactKey other) =>
            VehicleId.Equals(other.VehicleId) && OtherColliderId.Equals(other.OtherColliderId);
        public override bool Equals(object value) => value is VehicleContactKey other && Equals(other);
        public override int GetHashCode()
        {
            unchecked { return VehicleId.GetHashCode() * 397 ^ OtherColliderId.GetHashCode(); }
        }
        public static bool operator ==(VehicleContactKey left, VehicleContactKey right) => left.Equals(right);
        public static bool operator !=(VehicleContactKey left, VehicleContactKey right) => !left.Equals(right);
    }

    public readonly struct VehicleContactProfile
    {
        public float NormalIntensity01 { get; }
        public float TangentialIntensity01 { get; }
        public float CombinedIntensity01 { get; }
        public bool IsScraping { get; }

        public VehicleContactProfile(float normal, float tangential, bool scraping)
        {
            NormalIntensity01 = Mathf.Clamp01(normal);
            TangentialIntensity01 = Mathf.Clamp01(tangential);
            CombinedIntensity01 = Mathf.Max(NormalIntensity01, TangentialIntensity01 * .72f);
            IsScraping = scraping;
        }
    }

    public readonly struct VehicleContactSignal
    {
        public HoverVehicle Vehicle { get; }
        public EntityId OtherColliderId { get; }
        public VehicleContactKey StableContactKey { get; }
        public VehicleContactPhase Phase { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public Vector3 TangentialDirection { get; }
        public float NormalIntensity01 { get; }
        public float TangentialIntensity01 { get; }
        public float ContactDuration { get; }
        public float UnscaledTimestamp { get; }

        public VehicleContactSignal(HoverVehicle vehicle, EntityId otherColliderId, VehicleContactPhase phase,
            Vector3 point, Vector3 normal, Vector3 tangentialDirection, VehicleContactProfile profile,
            float contactDuration, float unscaledTimestamp)
        {
            Vehicle = vehicle;
            OtherColliderId = otherColliderId;
            StableContactKey = new VehicleContactKey(vehicle ? vehicle.GetEntityId() : default, otherColliderId);
            Phase = phase;
            Point = point;
            Normal = normal.sqrMagnitude > .0001f ? normal.normalized : Vector3.up;
            TangentialDirection = tangentialDirection.sqrMagnitude > .0001f ? tangentialDirection.normalized : Vector3.zero;
            NormalIntensity01 = profile.NormalIntensity01;
            TangentialIntensity01 = profile.TangentialIntensity01;
            ContactDuration = Mathf.Max(0f, contactDuration);
            UnscaledTimestamp = unscaledTimestamp;
        }

    }

    public static class VehicleContactMath
    {
        public static VehicleContactProfile Evaluate(float normalSpeed, float impulsePerMass,
            float tangentialSpeed, float contactDuration)
        {
            float normalMeasure = Mathf.Max(Mathf.Abs(normalSpeed), Mathf.Max(0f, impulsePerMass));
            float normal = Mathf.InverseLerp(1.5f, 24f, normalMeasure);
            float slide = Mathf.InverseLerp(1.5f, 30f, Mathf.Max(0f, tangentialSpeed));
            float established = Mathf.InverseLerp(.04f, .18f, Mathf.Max(0f, contactDuration));
            bool scraping = tangentialSpeed >= 2.5f && contactDuration >= .04f;
            return new VehicleContactProfile(normal, slide * Mathf.Lerp(.45f, 1f, established), scraping);
        }
    }
}
