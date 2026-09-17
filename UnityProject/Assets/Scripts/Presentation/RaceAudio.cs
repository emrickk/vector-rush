using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorRush
{
    public readonly struct RaceAudioMix
    {
        public float MotorLevel { get; }
        public float WindLevel { get; }
        public float BoostLevel { get; }

        public RaceAudioMix(float motorLevel, float windLevel, float boostLevel)
        {
            MotorLevel = motorLevel;
            WindLevel = windLevel;
            BoostLevel = boostLevel;
        }
    }

    public enum ImpactSeverity { None, Light, Medium, Heavy }

    public readonly struct ImpactFeedbackProfile
    {
        public ImpactSeverity Severity { get; }
        public float Volume { get; }
        public float VisualStrength { get; }

        public ImpactFeedbackProfile(ImpactSeverity severity, float volume, float visualStrength)
        {
            Severity = severity;
            Volume = volume;
            VisualStrength = visualStrength;
        }
    }

    public enum ContactSignalPhase { Enter, Stay, Exit, Cancel }

    public readonly struct ContactAudioMix
    {
        public float ImpactLevel { get; }
        public float ScrapeLevel { get; }
        public float Ducking { get; }

        public ContactAudioMix(float impactLevel, float scrapeLevel, float ducking)
        {
            ImpactLevel = impactLevel;
            ScrapeLevel = scrapeLevel;
            Ducking = ducking;
        }
    }

    /// <summary>Original synthesized craft, air, energy and race signals; no external recordings.</summary>
    public sealed class RaceAudio : MonoBehaviour
    {
        public static RaceAudio Instance { get; private set; }
        public float ImpactFeedback01 { get; private set; }
        public float ScrapeFeedback01 { get; private set; }
        public bool ExternalContactFeedActive { get; private set; }
        RaceDirector director;
        AudioSource motor, turbine, wind, boost, music, scrape, signals, impacts;
        AudioClip countTone, startTone, finishTone, boostTone, lightImpactTone, mediumImpactTone, heavyImpactTone;
        AudioClip rivalLoop, scrapeLoop, positionGainTone, positionLossTone, interfaceMoveTone, interfaceConfirmTone;
        readonly List<AudioClip> ownedClips = new List<AudioClip>();
        readonly List<RivalVoice> rivalVoices = new List<RivalVoice>();
        readonly List<HoverVehicle> rivalCandidates = new List<HoverVehicle>();
        readonly List<AudioReverbFilter> environmentFilters = new List<AudioReverbFilter>();
        readonly Dictionary<string, ContactState> contacts = new Dictionary<string, ContactState>();
        readonly List<string> expiredContacts = new List<string>();
        RacePhase previousPhase;
        int previousCount = -1, previousPlayerRecovery;
        int previousPosition;
        bool previousBoost, initialized;
        float lastImpactTime = -100f, lastPositionToneTime = -100f, lastInterfaceMoveTime = -100f;
        float level;
        const int SampleRate = 22050;
        const int RivalVoiceBudget = 3;

        public void Initialize(RaceDirector raceDirector)
        {
            director = raceDirector;
            if (initialized || director == null) return;
            Instance = this;
            initialized = true;
            motor = Voice("Craft / core", Loop("Core harmonics", 0), .70f);
            turbine = Voice("Craft / turbine", Loop("Turbine harmonics", 1), .80f);
            wind = Voice("Air / slipstream", Loop("Filtered airflow", 2), 1f);
            boost = Voice("Energy / thrust", Loop("Energy harmonics", 3), 1f);
            music = Voice("Music / original nocturne pulse", Loop("Original procedural nocturne score", 4), 1f);
            rivalLoop = Loop("Spatial rival propulsion", 5);
            scrapeLoop = Loop("Sustained hull contact", 6);
            scrape = Voice("Hull contact / sustained", scrapeLoop, .82f);
            signals = new GameObject("Race signals / distinct").AddComponent<AudioSource>();
            signals.transform.SetParent(transform, false);
            signals.playOnAwake = false;
            signals.spatialBlend = 0;
            signals.dopplerLevel = 0;
            signals.volume = .65f;
            impacts = new GameObject("Hull impacts / severity").AddComponent<AudioSource>();
            impacts.transform.SetParent(transform, false);
            impacts.playOnAwake = false;
            impacts.spatialBlend = 0f;
            impacts.dopplerLevel = 0f;
            countTone = Signal("Countdown", .14f, 440, 0);
            startTone = Signal("Race start", .48f, 880, 1);
            finishTone = Signal("Finish chord", 1.60f, 440, 2);
            boostTone = Signal("Boost engage", .34f, 180, 3);
            lightImpactTone = Signal("Hull scrape", .16f, 118, 4);
            mediumImpactTone = Signal("Hull impact", .24f, 82, 4);
            heavyImpactTone = Signal("Hull slam", .38f, 54, 4);
            positionGainTone = Signal("Position gained", .22f, 520, 6);
            positionLossTone = Signal("Position lost", .24f, 310, 7);
            interfaceMoveTone = Signal("Interface move", .055f, 720, 5);
            interfaceConfirmTone = Signal("Interface confirm", .12f, 540, 6);
            previousPhase = director.Phase;
            previousPosition = director.Position;
            previousPlayerRecovery = director.Player ? director.Player.RecoveryCount : 0;
            AttachCollisionListener();
            CreateRivalVoices();
            AddEnvironmentFilter(motor);
            AddEnvironmentFilter(turbine);
            AddEnvironmentFilter(wind);
        }

        AudioSource Voice(string label, AudioClip clip, float pitch)
        {
            var source = new GameObject(label).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.dopplerLevel = 0;
            source.volume = 0;
            source.pitch = pitch;
            source.Play();
            return source;
        }

        void AttachCollisionListener()
        {
            foreach (HoverVehicle racer in director.Racers)
            {
                if (!racer) continue;
                var listener = racer.GetComponent<RaceImpactListener>();
                if (listener == null) listener = racer.gameObject.AddComponent<RaceImpactListener>();
                listener.Owner = this;
                listener.Vehicle = racer;
            }
        }

        void Update()
        {
            if (!initialized || director == null) return;
            float dt = Time.unscaledDeltaTime;
            ImpactFeedback01 = Mathf.MoveTowards(ImpactFeedback01, 0f, dt * 2.8f);
            int playerRecovery = director.Player ? director.Player.RecoveryCount : previousPlayerRecovery;
            if (playerRecovery != previousPlayerRecovery)
            {
                previousPlayerRecovery = playerRecovery;
                ResetContactState();
            }
            UpdateContacts(dt);
            bool active = director.Phase == RacePhase.Racing || director.Phase == RacePhase.Countdown;
            float targetLevel = active ? 1f : 0f;
            level = Mathf.MoveTowards(level, targetLevel, dt * 3f);
            float speed = director.Player == null ? 0 : Mathf.Clamp01(director.Player.SpeedKph / 420f);
            bool boosting = director.Player != null && director.Player.IsBoosting && director.Phase == RacePhase.Racing;
            float throttle = director.Player == null ? 0f : director.Player.ThrottleInput;
            RaceAudioMix mix = ComputeMix(throttle, speed, boosting);
            PlayerPreferences preferences = PlayerPreferences.Current;
            float effectsLevel = preferences.EffectsVolume;
            float smoothing = 1 - Mathf.Exp(-dt * 6f);
            motor.pitch = Mathf.Lerp(motor.pitch, .72f + speed * 1.05f + mix.MotorLevel * .32f, smoothing);
            motor.volume = level * mix.MotorLevel * .15f * effectsLevel;
            turbine.pitch = Mathf.Lerp(turbine.pitch, .68f + speed * .85f + mix.MotorLevel * .42f, smoothing);
            turbine.volume = level * mix.MotorLevel * .068f * effectsLevel;
            wind.pitch = .8f + speed * .55f;
            float contactDucking = Mathf.Max(ImpactFeedback01 * .38f, ScrapeFeedback01 * .24f);
            float positionDucking = Mathf.Clamp01(1f - (Time.unscaledTime - lastPositionToneTime) / .35f) * .16f;
            float eventDucking = Mathf.Max(contactDucking, positionDucking);
            wind.volume = level * mix.WindLevel * .23f * effectsLevel * (1f - eventDucking);
            boost.pitch = 1f + speed * .55f;
            boost.volume = Mathf.Lerp(boost.volume, mix.BoostLevel * .11f * level * effectsLevel, smoothing);
            scrape.pitch = Mathf.Lerp(scrape.pitch, .76f + ScrapeFeedback01 * .48f, smoothing);
            scrape.volume = Mathf.Lerp(scrape.volume, ScrapeFeedback01 * .13f * level * effectsLevel, smoothing);
            music.volume = level * .16f * preferences.MusicVolume * (1f - eventDucking);
            signals.volume = .48f * effectsLevel;
            UpdateEnvironmentResponse();
            UpdateRivalVoices(effectsLevel, smoothing);

            if (director.Phase != previousPhase)
            {
                if (director.Phase == RacePhase.Countdown)
                {
                    previousCount = -1;
                    previousPosition = director.Position;
                    ResetContactState();
                    AttachCollisionListener();
                }
                if (director.Phase == RacePhase.Racing && previousPhase == RacePhase.Countdown) signals.PlayOneShot(startTone, .75f);
                if (director.Phase == RacePhase.Finished) signals.PlayOneShot(finishTone, .72f);
                if (director.Phase == RacePhase.Paused || director.Phase == RacePhase.Menu)
                {
                    signals.Stop();
                    impacts.Stop();
                    ResetContactState();
                }
                previousPhase = director.Phase;
            }
            if (director.Phase == RacePhase.Countdown)
            {
                int remaining = Mathf.CeilToInt(director.CountdownRemaining);
                if (remaining > 0 && remaining != previousCount)
                {
                    signals.PlayOneShot(countTone, .68f);
                    previousCount = remaining;
                }
            }
            if (director.Phase == RacePhase.Racing && previousPosition > 0 && director.Position != previousPosition && Time.unscaledTime - lastPositionToneTime >= .18f)
            {
                bool gained = director.Position < previousPosition;
                signals.PlayOneShot(gained ? positionGainTone : positionLossTone, gained ? .24f : .20f);
                lastPositionToneTime = Time.unscaledTime;
            }
            if (director.Phase == RacePhase.Racing) previousPosition = director.Position;
            if (boosting && !previousBoost) signals.PlayOneShot(boostTone, .32f);
            previousBoost = boosting;
        }

        public void PlayInterfaceMove()
        {
            if (!initialized || signals == null || Time.unscaledTime - lastInterfaceMoveTime < .045f) return;
            lastInterfaceMoveTime = Time.unscaledTime;
            signals.PlayOneShot(interfaceMoveTone, .11f);
        }

        public void PlayInterfaceConfirm()
        {
            if (!initialized || signals == null) return;
            signals.PlayOneShot(interfaceConfirmTone, .18f);
        }

        public void PlayImpact(float relativeSpeed)
        {
            PlayImpact(director == null ? null : director.Player, relativeSpeed);
        }

        public void PlayImpact(HoverVehicle vehicle, float relativeSpeed)
        {
            if (!initialized || director == null || !director.CanSimulate(vehicle) || relativeSpeed < 2f) return;
            ImpactFeedbackProfile profile = ImpactFeedbackForSpeed(relativeSpeed);
            AudioClip clip = profile.Severity == ImpactSeverity.Heavy ? heavyImpactTone :
                profile.Severity == ImpactSeverity.Medium ? mediumImpactTone : lightImpactTone;
            float volume = profile.Volume * PlayerPreferences.Current.EffectsVolume;
            if (vehicle == director.Player)
            {
                if (Time.unscaledTime - lastImpactTime < .18f) return;
                lastImpactTime = Time.unscaledTime;
                ImpactFeedback01 = Mathf.Max(ImpactFeedback01, profile.VisualStrength);
                impacts.PlayOneShot(clip, volume);
            }
            else
            {
                RivalVoice voice = rivalVoices.FirstOrDefault(value => value.vehicle == vehicle);
                if (voice != null && Time.unscaledTime - voice.lastImpactTime >= .22f)
                {
                    voice.lastImpactTime = Time.unscaledTime;
                    voice.source.PlayOneShot(clip, volume * .62f);
                }
            }
        }

        /// <summary>Consumes normalized contact values from the integration bridge. The listener below is a baseline adapter.</summary>
        public void ApplyContactSignal(HoverVehicle vehicle, string contactKey, ContactSignalPhase phase, float normalIntensity01, float tangentialIntensity01)
        {
            if (!initialized || director == null) return;
            if (string.IsNullOrEmpty(contactKey)) return;
            if (phase == ContactSignalPhase.Exit || phase == ContactSignalPhase.Cancel)
            {
                contacts.Remove(contactKey);
                return;
            }
            if (!director.CanSimulate(vehicle)) return;

            ContactAudioMix mix = ContactMix(normalIntensity01, tangentialIntensity01, phase == ContactSignalPhase.Stay);
            contacts[contactKey] = new ContactState
            {
                vehicle = vehicle,
                scrape = mix.ScrapeLevel,
                lastSeen = Time.unscaledTime
            };
            if (phase == ContactSignalPhase.Enter && mix.ImpactLevel > 0f)
                PlayImpact(vehicle, Mathf.Lerp(2f, 32f, mix.ImpactLevel));
        }

        public void ConfigureExternalContactFeed(bool active)
        {
            ExternalContactFeedActive = active;
            ResetContactState();
        }

        public static ContactAudioMix ContactMix(float normalIntensity01, float tangentialIntensity01, bool sustained)
        {
            float normal = Mathf.Clamp01(normalIntensity01);
            float tangential = Mathf.Clamp01(tangentialIntensity01);
            float impact = sustained ? 0f : normal;
            float scrape = sustained ? Mathf.Clamp01(tangential * Mathf.Lerp(1f, .42f, normal)) : 0f;
            return new ContactAudioMix(impact, scrape, Mathf.Clamp01(Mathf.Max(impact * .38f, scrape * .24f)));
        }

        public static void ContactIntensity(Vector3 relativeVelocity, Vector3 contactNormal, out float normal01, out float tangential01)
        {
            Vector3 normal = contactNormal.sqrMagnitude > .0001f ? contactNormal.normalized : Vector3.up;
            float signedNormal = Vector3.Dot(relativeVelocity, normal);
            float normalSpeed = Mathf.Abs(signedNormal);
            float tangentialSpeed = (relativeVelocity - normal * signedNormal).magnitude;
            normal01 = Mathf.InverseLerp(1.5f, 28f, normalSpeed);
            tangential01 = Mathf.InverseLerp(3f, 24f, tangentialSpeed);
        }

        public void CancelContacts(HoverVehicle vehicle)
        {
            expiredContacts.Clear();
            foreach (KeyValuePair<string, ContactState> entry in contacts)
                if (entry.Value.vehicle == vehicle) expiredContacts.Add(entry.Key);
            foreach (string key in expiredContacts) contacts.Remove(key);
        }

        public static ImpactFeedbackProfile ImpactFeedbackForSpeed(float relativeSpeed)
        {
            float speed = Mathf.Max(0f, relativeSpeed);
            if (speed < 2f) return new ImpactFeedbackProfile(ImpactSeverity.None, 0f, 0f);
            if (speed < 8f) return new ImpactFeedbackProfile(ImpactSeverity.Light, Mathf.Lerp(.08f, .18f, Mathf.InverseLerp(2f, 8f, speed)), .18f);
            if (speed < 18f) return new ImpactFeedbackProfile(ImpactSeverity.Medium, Mathf.Lerp(.20f, .34f, Mathf.InverseLerp(8f, 18f, speed)), .48f);
            return new ImpactFeedbackProfile(ImpactSeverity.Heavy, Mathf.Lerp(.36f, .52f, Mathf.InverseLerp(18f, 32f, speed)), .85f);
        }

        public static RaceAudioMix ComputeMix(float throttle, float speed01, bool boosting)
        {
            float speed = Mathf.Clamp01(speed01);
            float load = Mathf.Clamp01(throttle) * (boosting ? 1.25f : 1f);
            float motor = Mathf.Clamp01(.20f + speed * .22f + load * .58f + (boosting ? .18f : 0f));
            return new RaceAudioMix(motor, speed * speed, boosting ? Mathf.Clamp01(.35f + load * .65f) : 0f);
        }

        void UpdateContacts(float deltaTime)
        {
            bool active = director.Phase == RacePhase.Racing;
            float target = 0f;
            expiredContacts.Clear();
            foreach (KeyValuePair<string, ContactState> entry in contacts)
            {
                ContactState state = entry.Value;
                if (!active || state.vehicle == null || Time.unscaledTime - state.lastSeen > .12f)
                {
                    expiredContacts.Add(entry.Key);
                    continue;
                }
                if (state.vehicle == director.Player) target = Mathf.Max(target, state.scrape);
            }
            foreach (string key in expiredContacts) contacts.Remove(key);
            ScrapeFeedback01 = Mathf.MoveTowards(ScrapeFeedback01, target, deltaTime * (target > ScrapeFeedback01 ? 8f : 4.5f));
        }

        void ResetContactState()
        {
            contacts.Clear();
            ScrapeFeedback01 = 0f;
            if (scrape != null) scrape.volume = 0f;
        }

        public static int[] SelectRivalVoices(IReadOnlyList<float> squaredDistances, int budget)
        {
            if (squaredDistances == null || budget <= 0) return Array.Empty<int>();
            return Enumerable.Range(0, squaredDistances.Count)
                .Where(index => squaredDistances[index] >= 0f && !float.IsNaN(squaredDistances[index]) && !float.IsInfinity(squaredDistances[index]))
                .OrderBy(index => squaredDistances[index]).ThenBy(index => index).Take(budget).ToArray();
        }

        public static float AcousticWetness(float trackProgress)
        {
            float progress = Mathf.Repeat(trackProgress, 1f);
            float enter = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.31f, .35f, progress));
            float exit = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.42f, .46f, progress));
            return Mathf.Clamp01(enter * exit * .82f);
        }

        void CreateRivalVoices()
        {
            for (int i = 0; i < RivalVoiceBudget; i++)
            {
                AudioSource source = new GameObject("Rival voice " + (i + 1) + " / spatial budget").AddComponent<AudioSource>();
                source.transform.SetParent(transform, false);
                source.clip = rivalLoop;
                source.loop = true;
                source.playOnAwake = false;
                source.spatialBlend = 1f;
                source.dopplerLevel = 1.15f;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                source.minDistance = 7f;
                source.maxDistance = 130f;
                source.volume = 0f;
                source.Play();
                rivalVoices.Add(new RivalVoice { source = source });
            }
        }

        void UpdateRivalVoices(float effectsLevel, float smoothing)
        {
            rivalCandidates.Clear();
            if (director.Player && director.Player.Body)
                foreach (HoverVehicle racer in director.Racers)
                    if (racer && racer != director.Player && racer.Body && director.CanSimulate(racer)) rivalCandidates.Add(racer);
            Vector3 listener = director.Player && director.Player.Body ? director.Player.Body.position : Vector3.zero;
            float[] distances = rivalCandidates.Select(value => (value.Body.position - listener).sqrMagnitude).ToArray();
            int[] selected = SelectRivalVoices(distances, RivalVoiceBudget);
            for (int i = 0; i < rivalVoices.Count; i++)
            {
                RivalVoice voice = rivalVoices[i];
                if (i >= selected.Length)
                {
                    voice.vehicle = null;
                    voice.source.volume = Mathf.Lerp(voice.source.volume, 0f, smoothing);
                    continue;
                }
                HoverVehicle racer = rivalCandidates[selected[i]];
                voice.vehicle = racer;
                voice.source.transform.position = racer.Body.position;
                Vector3 offset = racer.Body.position - listener;
                Vector3 relativeVelocity = racer.Body.linearVelocity - director.Player.Body.linearVelocity;
                float approach = offset.sqrMagnitude > .01f ? -Vector3.Dot(relativeVelocity, offset.normalized) : 0f;
                RaceAudioMix mix = ComputeMix(racer.ThrottleInput, Mathf.Clamp01(racer.SpeedKph / 420f), racer.IsBoosting);
                float proximity = 1f - Mathf.Clamp01(Mathf.Sqrt(offset.sqrMagnitude) / 130f);
                voice.source.pitch = Mathf.Lerp(voice.source.pitch, .72f + mix.MotorLevel * .7f + Mathf.Clamp(approach / 100f, -.12f, .15f), smoothing);
                voice.source.volume = Mathf.Lerp(voice.source.volume, level * mix.MotorLevel * proximity * .095f * effectsLevel, smoothing);
            }
        }

        void AddEnvironmentFilter(AudioSource source)
        {
            AudioReverbFilter filter = source.gameObject.AddComponent<AudioReverbFilter>();
            filter.reverbPreset = AudioReverbPreset.User;
            filter.dryLevel = 0f;
            filter.room = -10000f;
            filter.roomHF = -10000f;
            filter.reverbLevel = -10000f;
            environmentFilters.Add(filter);
        }

        void UpdateEnvironmentResponse()
        {
            float wet = director.Player == null ? 0f : AcousticWetness(director.Player.TrackProgress);
            foreach (AudioReverbFilter filter in environmentFilters)
            {
                filter.room = Mathf.Lerp(-10000f, -1700f, wet);
                filter.roomHF = Mathf.Lerp(-10000f, -3200f, wet);
                filter.reverbLevel = Mathf.Lerp(-10000f, -650f, wet);
                filter.decayTime = Mathf.Lerp(.25f, 1.65f, wet);
                filter.reflectionsDelay = Mathf.Lerp(0f, .045f, wet);
            }
        }

        AudioClip Loop(string name, int type)
        {
            int count = SampleRate * 2;
            var samples = new float[count];
            var random = new System.Random(981 + type);
            float filteredNoise = 0;
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / SampleRate;
                double noise = random.NextDouble() * 2 - 1;
                filteredNoise = .89f * filteredNoise + .11f * (float)noise;
                double value;
                switch (type)
                {
                    case 0:
                        value = .47 * Wave(72, t) + .20 * Wave(144, t) + .11 * Wave(216, t) + .06 * Wave(360, t);
                        value *= .91 + .09 * Wave(8, t);
                        break;
                    case 1:
                        value = .32 * Wave(420, t) + .17 * Wave(630, t) + .09 * Wave(840, t);
                        value *= .94 + .06 * Wave(13, t);
                        break;
                    case 2: value = filteredNoise * 2.5; break;
                    case 4:
                        double beat = Math.Pow(Math.Max(0, 1 - (t * 2.5 % 1)), 7);
                        double root = t < 1 ? 55 : 65.406;
                        value = .19 * Wave(root, t) + .10 * Wave(root * 2, t) + .055 * Wave(root * 3, t) + beat * (.12 * Wave(88, t) + .08 * filteredNoise);
                        break;
                    case 5:
                        value = .40 * Wave(68, t) + .18 * Wave(136, t) + .08 * Wave(272, t) + .04 * filteredNoise;
                        break;
                    case 6:
                        value = .18 * Wave(96, t) + .12 * Wave(191, t) + .44 * ((float)noise - filteredNoise);
                        value *= .78 + .22 * Wave(23, t);
                        break;
                    default:
                        value = .27 * Wave(115, t) + .18 * Wave(230, t) + .16 * filteredNoise;
                        value *= .82 + .18 * Wave(19, t);
                        break;
                }
                samples[i] = (float)Math.Tanh(value);
            }
            // Crossfade random airflow at the seam; tonal frequencies have integral periods.
            if (type >= 2)
            {
                const int seam = 256;
                float start = samples[0];
                for (int i = 0; i < seam; i++)
                    samples[count - seam + i] = Mathf.Lerp(samples[count - seam + i], start, i / (float)(seam - 1));
            }
            return Clip(name, samples);
        }

        AudioClip Signal(string name, float duration, double frequency, int type)
        {
            var samples = new float[Mathf.CeilToInt(duration * SampleRate)];
            var random = new System.Random(117 + type);
            for (int i = 0; i < samples.Length; i++)
            {
                double t = (double)i / SampleRate;
                double progress = t / duration;
                double attack = Math.Min(1, t / .012);
                double release = Math.Pow(Math.Max(0, 1 - progress), type == 2 ? 1.5 : 2.4);
                double value;
                if (type == 2)
                    value = (Wave(frequency, t) + .65 * Wave(frequency * 1.25, t) + .55 * Wave(frequency * 1.5, t)) * .24;
                else if (type == 3)
                    value = Math.Sin(2 * Math.PI * (frequency * t + 650 * t * t)) * .42;
                else if (type == 4)
                    value = .35 * Wave(frequency * (1 - .35 * progress), t) + .42 * (random.NextDouble() * 2 - 1);
                else if (type == 5)
                    value = .32 * Wave(frequency, t) + .10 * Wave(frequency * 1.5, t);
                else if (type == 6)
                    value = .34 * Wave(frequency * (1 + .32 * progress), t) + .13 * Wave(frequency * 2, t);
                else if (type == 7)
                    value = .34 * Wave(frequency * (1.3 - .3 * progress), t) + .11 * Wave(frequency * 1.5, t);
                else
                    value = .50 * Wave(frequency, t) + .13 * Wave(frequency * 2, t);
                samples[i] = (float)(value * attack * release);
            }
            return Clip(name, samples);
        }

        static double Wave(double frequency, double time) => Math.Sin(2 * Math.PI * frequency * time);

        AudioClip Clip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            ownedClips.Add(clip);
            return clip;
        }

        void OnDestroy()
        {
            ResetContactState();
            if (Instance == this) Instance = null;
            foreach (AudioClip clip in ownedClips) if (clip != null) Destroy(clip);
        }

        void OnDisable()
        {
            ResetContactState();
            AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);
            foreach (AudioSource source in sources) source.Stop();
        }

        void OnEnable()
        {
            if (!initialized) return;
            AudioSource[] sources = { motor, turbine, wind, boost, music, scrape };
            foreach (AudioSource source in sources) if (source != null && !source.isPlaying) source.Play();
            foreach (RivalVoice voice in rivalVoices) if (voice.source != null && !voice.source.isPlaying) voice.source.Play();
        }

        sealed class ContactState
        {
            public HoverVehicle vehicle;
            public float scrape;
            public float lastSeen;
        }

        sealed class RivalVoice
        {
            public HoverVehicle vehicle;
            public AudioSource source;
            public float lastImpactTime = -100f;
        }
    }

    public sealed class RaceImpactListener : MonoBehaviour
    {
        public RaceAudio Owner;
        public HoverVehicle Vehicle;
        void OnCollisionEnter(Collision collision)
        {
            Send(collision, ContactSignalPhase.Enter);
        }
        void OnCollisionStay(Collision collision) => Send(collision, ContactSignalPhase.Stay);
        void OnCollisionExit(Collision collision)
        {
            if (Owner != null && !Owner.ExternalContactFeedActive) Owner.ApplyContactSignal(Vehicle, Key(collision), ContactSignalPhase.Exit, 0f, 0f);
        }
        void OnDisable()
        {
            if (Owner != null) Owner.CancelContacts(Vehicle);
        }
        void Send(Collision collision, ContactSignalPhase phase)
        {
            if (Owner == null || Owner.ExternalContactFeedActive || collision == null) return;
            Vector3 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector3.up;
            RaceAudio.ContactIntensity(collision.relativeVelocity, normal, out float normal01, out float tangential01);
            Owner.ApplyContactSignal(Vehicle, Key(collision), phase, normal01, tangential01);
        }
        string Key(Collision collision)
        {
            string other = collision != null && collision.collider != null ? collision.collider.GetEntityId().ToString() : "0";
            string vehicle = Vehicle ? Vehicle.GetEntityId().ToString() : GetEntityId().ToString();
            return vehicle + ":" + other;
        }
    }
}
