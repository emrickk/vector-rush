using UnityEngine;

namespace VectorRush
{
    /// <summary>Integration-owned adapter from the authoritative VFX contact stream to race audio.</summary>
    public sealed class VehicleContactAudioBridge : MonoBehaviour
    {
        VehicleVFX source;
        RaceAudio audio;
        RaceDirector director;
        HoverVehicle vehicle;
        bool subscribed;

        public void Initialize(VehicleVFX contactSource, RaceAudio raceAudio, RaceDirector raceDirector)
        {
            Unsubscribe(true);
            source = contactSource;
            audio = raceAudio;
            director = raceDirector;
            vehicle = source ? source.GetComponent<HoverVehicle>() : null;
            Subscribe();
        }

        public static ContactSignalPhase MapPhase(VehicleContactPhase phase)
        {
            switch (phase)
            {
                case VehicleContactPhase.Enter: return ContactSignalPhase.Enter;
                case VehicleContactPhase.Stay: return ContactSignalPhase.Stay;
                default: return ContactSignalPhase.Exit;
            }
        }

        public static string ContactKey(VehicleContactKey key) =>
            key.VehicleId.ToString() + ":" + key.OtherColliderId.ToString();

        void Subscribe()
        {
            if (subscribed || !isActiveAndEnabled || !source || !audio || !director) return;
            source.ContactFeedback += HandleContact;
            director.RaceRestarted += HandleRaceRestarted;
            audio.ConfigureExternalContactFeed(true);
            subscribed = true;
        }

        void Unsubscribe(bool deactivateExternalFeed)
        {
            if (subscribed)
            {
                if (source) source.ContactFeedback -= HandleContact;
                if (director) director.RaceRestarted -= HandleRaceRestarted;
                subscribed = false;
            }
            if (!audio) return;
            if (vehicle) audio.CancelContacts(vehicle);
            if (deactivateExternalFeed) audio.ConfigureExternalContactFeed(false);
        }

        void HandleContact(VehicleContactSignal signal)
        {
            if (!audio) return;
            audio.ApplyContactSignal(signal.Vehicle, ContactKey(signal.StableContactKey), MapPhase(signal.Phase),
                signal.NormalIntensity01, signal.TangentialIntensity01);
        }

        void HandleRaceRestarted()
        {
            if (audio) audio.ConfigureExternalContactFeed(true);
        }

        void OnEnable() { Subscribe(); }
        void OnDisable() { Unsubscribe(true); }
        void OnDestroy() { Unsubscribe(true); }
    }
}
