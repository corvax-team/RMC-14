using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Audio
{
    public static class AudioHelpers
    {
        private const float MaxSafeGain = 64f;
        private const float MinSafeVolume = -80f;
        private const float MaxSafeVolume = 24f;

        public static float SanitizeGain(float gain, float fallback = 1f)
        {
            if (!float.IsFinite(gain) || gain < 0f)
                gain = fallback;

            if (!float.IsFinite(gain) || gain < 0f)
                return 0f;

            return Math.Clamp(gain, 0f, MaxSafeGain);
        }

        public static float SanitizeVolume(float volume, float fallback = 0f)
        {
            if (float.IsNegativeInfinity(volume))
                return float.NegativeInfinity;

            if (!float.IsFinite(volume))
                volume = fallback;

            if (float.IsNegativeInfinity(volume))
                return float.NegativeInfinity;

            if (!float.IsFinite(volume))
                return 0f;

            return Math.Clamp(volume, MinSafeVolume, MaxSafeVolume);
        }

        public static float SafeGainToVolume(float gain, float fallback = 1f)
        {
            var sanitizedGain = SanitizeGain(gain, fallback);
            return sanitizedGain <= 0f
                ? float.NegativeInfinity
                : SharedAudioSystem.GainToVolume(sanitizedGain);
        }

        public static float SafeVolumeToGain(float volume, float fallback = 0f)
        {
            var sanitizedVolume = SanitizeVolume(volume, fallback);
            return float.IsNegativeInfinity(sanitizedVolume)
                ? 0f
                : SharedAudioSystem.VolumeToGain(sanitizedVolume);
        }

        public static AudioParams SanitizeAudioParams(AudioParams audioParams, AudioParams? fallback = null)
        {
            var safeFallback = fallback ?? AudioParams.Default;

            audioParams = audioParams
                .WithVolume(SanitizeVolume(audioParams.Volume, safeFallback.Volume))
                .WithPitchScale(SanitizeNonNegative(audioParams.Pitch, safeFallback.Pitch))
                .WithMaxDistance(SanitizeNonNegative(audioParams.MaxDistance, safeFallback.MaxDistance))
                .WithRolloffFactor(SanitizeNonNegative(audioParams.RolloffFactor, safeFallback.RolloffFactor))
                .WithReferenceDistance(SanitizeNonNegative(audioParams.ReferenceDistance, safeFallback.ReferenceDistance))
                .WithPlayOffset(SanitizeNonNegative(audioParams.PlayOffsetSeconds, safeFallback.PlayOffsetSeconds))
                .WithVariation(SanitizeVariation(audioParams.Variation, safeFallback.Variation));

            return audioParams;
        }

        private static float SanitizeNonNegative(float value, float fallback)
        {
            if (!float.IsFinite(value) || value < 0f)
                value = fallback;

            if (!float.IsFinite(value) || value < 0f)
                return 0f;

            return value;
        }

        private static float? SanitizeVariation(float? value, float? fallback)
        {
            if (value is null)
                return null;

            if (!float.IsFinite(value.Value) || value.Value < 0f)
                return fallback is { } fallbackValue && float.IsFinite(fallbackValue) && fallbackValue >= 0f
                    ? fallbackValue
                    : null;

            return value.Value;
        }

        /// <summary>
        ///     Returns a random pitch.
        /// </summary>
        [Obsolete("Use AudioParams.Variation data-field")]
        public static AudioParams WithVariation(float amplitude)
        {
            return WithVariation(amplitude, null);
        }

        /// <summary>
        ///     Returns a random pitch.
        /// </summary>
        [Obsolete("Use AudioParams.Variation data-field")]
        public static AudioParams WithVariation(float amplitude, IRobustRandom? rand)
        {
            IoCManager.Resolve(ref rand);
            var scale = (float) rand.NextGaussian(1, amplitude);
            return AudioParams.Default.WithPitchScale(scale);
        }

        // Might as well just hardcode these because the audio system is limited to pitching up and down
        // by 12 semitones anyway (ie. 0.5 to 2.0 multiplier).
        private static readonly float[] SemitoneMultipliers =
        {
            0.5f, 233.08f/440f, 246.94f/440f, 261.63f/440f,
            277.18f/440f, 293.66f/440f, 311.13f/440f, 329.63f/440f,
            349.23f/440f, 369.99f/440f, 392.00f/440f, 415.30f/440f,
            1.0f,
            466.16f/440f, 493.88f/440f, 523.25f/440f, 554.37f/440f,
            587.33f/440f, 622.25f/440f, 659.26f/440f, 698.46f/440f,
            739.99f/440f, 783.99f/440f, 830.61f/440f, 2.0f
        };

        /// <summary>
        /// Returns a pitch multiplier that shifts by the given number of semitones.
        /// </summary>
        /// <param name="shift">Number of semitones to shift, positive or negative. Clamped between -12 and 12
        /// which correspond to a pitch multiplier of 0.5 and 2.0 respectively.</param>
        public static AudioParams ShiftSemitone(AudioParams @params, int shift)
        {
            shift = MathHelper.Clamp(shift, -12, 12);
            float pitchMult = SemitoneMultipliers[shift + 12];
            return @params.WithPitchScale(pitchMult);
        }

        /// <summary>
        /// Returns a pitch multiplier shifted by a random number of semitones within variation.
        /// </summary>
        /// <param name="variation">Max number of semitones to shift in either direction. Values above 12 have no effect.</param>
        public static AudioParams WithSemitoneVariation(AudioParams @params, int variation, IRobustRandom rand)
        {
            IoCManager.Resolve(ref rand);
            variation = Math.Clamp(variation, 0, 12);
            return ShiftSemitone(@params, rand.Next(-variation, variation));
        }
    }
}
