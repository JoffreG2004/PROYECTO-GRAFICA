using System;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Services
{
    public class AudioDynamicsProcessor
    {
        private float previousEnergy;
        private float bassEnvelope;
        private float midEnvelope;
        private float trebleEnvelope;
        private float pulsePosition;
        private float pulseVelocity;

        public void Reset()
        {
            previousEnergy = 0;
            bassEnvelope = 0;
            midEnvelope = 0;
            trebleEnvelope = 0;
            pulsePosition = 0;
            pulseVelocity = 0;
        }

        public AudioFrame Process(AudioFrame rawFrame)
        {
            var energy = rawFrame.Bass * 0.62f + rawFrame.Mid * 0.26f + rawFrame.Treble * 0.12f;
            var onset = Math.Max(0, energy - previousEnergy * 1.08f);
            previousEnergy = Follow(previousEnergy, energy, 0.28f, 0.06f);

            bassEnvelope = Follow(bassEnvelope, rawFrame.Bass, 0.42f, 0.08f);
            midEnvelope = Follow(midEnvelope, rawFrame.Mid, 0.34f, 0.07f);
            trebleEnvelope = Follow(trebleEnvelope, rawFrame.Treble, 0.30f, 0.10f);

            var targetPulse = Compress(onset, 2.7f);

            // Respuesta discreta de un resorte amortiguado: x'' + c*x' + k*x = k*target.
            pulseVelocity += (targetPulse - pulsePosition) * 0.44f;
            pulseVelocity *= 0.64f;
            pulsePosition += pulseVelocity;
            pulsePosition = Clamp(pulsePosition);

            var impact = Math.Max(pulsePosition, targetPulse);
            var spectrum = BoostSpectrum(rawFrame.Spectrum, impact);

            return new AudioFrame(
                Clamp(rawFrame.Intensity * 0.72f + energy * 0.34f + impact * 0.32f),
                Clamp(bassEnvelope + impact * 0.34f),
                Clamp(midEnvelope + impact * 0.20f),
                Clamp(trebleEnvelope + impact * 0.14f),
                impact,
                spectrum,
                rawFrame.UsesRealSamples);
        }

        private static float[] BoostSpectrum(float[] spectrum, float impact)
        {
            if (spectrum == null || spectrum.Length == 0)
            {
                return new float[0];
            }

            var boosted = new float[spectrum.Length];

            for (var i = 0; i < spectrum.Length; i++)
            {
                var bassWeight = 1f - Math.Min(1f, i / 16f);
                var value = spectrum[i] * (0.96f + impact * 0.34f) + impact * 0.14f * bassWeight;
                boosted[i] = Clamp(value);
            }

            return boosted;
        }

        private static float Follow(float current, float target, float attack, float release)
        {
            var coefficient = target > current ? attack : release;
            return current + (target - current) * coefficient;
        }

        private static float Compress(float value, float gain)
        {
            return Clamp((float)(1 - Math.Exp(-Math.Max(0, value) * gain)));
        }

        private static float Clamp(float value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > 1 ? 1 : value;
        }
    }
}
