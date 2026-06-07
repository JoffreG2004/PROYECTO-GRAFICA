namespace REPRODUCTOR_MUSICAL.Models
{
    public class AudioFrame
    {
        public AudioFrame(float intensity, float bass, float mid, float treble, bool usesRealSamples)
            : this(intensity, bass, mid, treble, 0, new float[0], usesRealSamples)
        {
        }

        public AudioFrame(float intensity, float bass, float mid, float treble, float[] spectrum, bool usesRealSamples)
            : this(intensity, bass, mid, treble, 0, spectrum, usesRealSamples)
        {
        }

        public AudioFrame(float intensity, float bass, float mid, float treble, float pulse, float[] spectrum, bool usesRealSamples)
        {
            Intensity = Clamp(intensity);
            Bass = Clamp(bass);
            Mid = Clamp(mid);
            Treble = Clamp(treble);
            Pulse = Clamp(pulse);
            Spectrum = spectrum ?? new float[0];
            UsesRealSamples = usesRealSamples;
        }

        public float Intensity { get; }

        public float Bass { get; }

        public float Mid { get; }

        public float Treble { get; }

        public float Pulse { get; }

        public float[] Spectrum { get; }

        public bool UsesRealSamples { get; }

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
