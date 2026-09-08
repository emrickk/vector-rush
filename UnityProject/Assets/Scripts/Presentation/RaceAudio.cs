using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Original synthesized craft, air, energy and race signals; no external recordings.</summary>
    public sealed class RaceAudio : MonoBehaviour
    {
        RaceDirector director;
        AudioSource motor, turbine, wind, boost, signals;
        AudioClip countTone, startTone, finishTone, boostTone, impactTone;
        readonly List<AudioClip> ownedClips = new List<AudioClip>();
        RacePhase previousPhase;
        int previousCount = -1;
        bool previousBoost, initialized;
        float lastImpactTime = -100f;
        float level;
        const int SampleRate = 22050;

        public void Initialize(RaceDirector raceDirector)
        {
            director = raceDirector;
            if (initialized || director == null) return;
            initialized = true;
            motor = Voice("Craft / core", Loop("Core harmonics", 0), .70f);
            turbine = Voice("Craft / turbine", Loop("Turbine harmonics", 1), .80f);
            wind = Voice("Air / slipstream", Loop("Filtered airflow", 2), 1f);
            boost = Voice("Energy / thrust", Loop("Energy harmonics", 3), 1f);
            signals = gameObject.AddComponent<AudioSource>();
            signals.playOnAwake = false;
            signals.spatialBlend = 0;
            signals.dopplerLevel = 0;
            signals.volume = .65f;
            countTone = Signal("Countdown", .14f, 440, 0);
            startTone = Signal("Race start", .48f, 880, 1);
            finishTone = Signal("Finish chord", 1.60f, 440, 2);
            boostTone = Signal("Boost engage", .34f, 180, 3);
            impactTone = Signal("Hull contact", .22f, 75, 4);
            previousPhase = director.Phase;
            AttachCollisionListener();
        }

        AudioSource Voice(string label, AudioClip clip, float pitch)
        {
            var source = gameObject.AddComponent<AudioSource>();
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
            if (director.Player == null) return;
            var listener = director.Player.GetComponent<RaceImpactListener>();
            if (listener == null) listener = director.Player.gameObject.AddComponent<RaceImpactListener>();
            listener.Owner = this;
        }

        void Update()
        {
            if (!initialized || director == null) return;
            float dt = Time.unscaledDeltaTime;
            bool active = director.Phase == RacePhase.Racing || director.Phase == RacePhase.Countdown;
            float targetLevel = active ? 1f : 0f;
            level = Mathf.MoveTowards(level, targetLevel, dt * 3f);
            float speed = director.Player == null ? 0 : Mathf.Clamp01(director.Player.SpeedKph / 420f);
            bool boosting = director.Player != null && director.Player.IsBoosting && director.Phase == RacePhase.Racing;
            float smoothing = 1 - Mathf.Exp(-dt * 6f);
            motor.pitch = Mathf.Lerp(motor.pitch, .72f + speed * 1.36f, smoothing);
            motor.volume = level * Mathf.Lerp(.055f, .14f, speed);
            turbine.pitch = Mathf.Lerp(turbine.pitch, .68f + speed * 1.25f, smoothing);
            turbine.volume = level * Mathf.Lerp(.018f, .065f, speed);
            wind.pitch = .8f + speed * .55f;
            wind.volume = level * speed * speed * .23f;
            boost.pitch = 1f + speed * .55f;
            boost.volume = Mathf.Lerp(boost.volume, boosting ? .11f * level : 0, smoothing);

            if (director.Phase != previousPhase)
            {
                if (director.Phase == RacePhase.Countdown)
                {
                    previousCount = -1;
                    AttachCollisionListener();
                }
                if (director.Phase == RacePhase.Racing && previousPhase == RacePhase.Countdown) signals.PlayOneShot(startTone, .75f);
                if (director.Phase == RacePhase.Finished) signals.PlayOneShot(finishTone, .72f);
                if (director.Phase == RacePhase.Paused) signals.Stop();
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
            if (boosting && !previousBoost) signals.PlayOneShot(boostTone, .32f);
            previousBoost = boosting;
        }

        public void PlayImpact(float relativeSpeed)
        {
            if (!initialized || director.Phase != RacePhase.Racing || relativeSpeed < 2f || Time.unscaledTime - lastImpactTime < .18f) return;
            lastImpactTime = Time.unscaledTime;
            signals.PlayOneShot(impactTone, Mathf.Clamp(relativeSpeed / 28f, .08f, .48f));
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
            foreach (AudioClip clip in ownedClips) if (clip != null) Destroy(clip);
        }
    }

    public sealed class RaceImpactListener : MonoBehaviour
    {
        public RaceAudio Owner;
        void OnCollisionEnter(Collision collision)
        {
            if (Owner != null) Owner.PlayImpact(collision.relativeVelocity.magnitude);
        }
    }
}
