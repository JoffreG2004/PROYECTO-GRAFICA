using System;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Services
{
    public class FftAnalyzer
    {
        private const int FftSize = 2048;
        private const int VisualBins = 44;
        private readonly double[] real = new double[FftSize];
        private readonly double[] imaginary = new double[FftSize];
        private readonly float[] smoothedSpectrum = new float[VisualBins];

        public AudioFrame Analyze(short[] samples, int sampleRate, int channels, TimeSpan currentTime, int volume)
        {
            var compensatedTime = currentTime.TotalSeconds + 0.055;
            var startSample = Math.Max(0, (int)(compensatedTime * sampleRate));
            var sourceIndex = startSample * channels;
            var visualVolumeFactor = 0.82f;

            if (sourceIndex >= samples.Length)
            {
                return new AudioFrame(0, 0, 0, 0, new float[VisualBins], true);
            }

            FillWindow(samples, channels, sourceIndex);
            Transform(real, imaginary);

            var spectrum = BuildVisualSpectrum(sampleRate, visualVolumeFactor);
            var bass = AverageRange(sampleRate, 35, 180) * visualVolumeFactor;
            var mid = AverageRange(sampleRate, 180, 2400) * visualVolumeFactor;
            var treble = AverageRange(sampleRate, 2400, 9000) * visualVolumeFactor;
            var intensity = Math.Max(bass * 0.58, Math.Max(mid * 0.36, treble * 0.28)) + (bass * 0.22 + mid * 0.12);

            return new AudioFrame(
                Compress(intensity, 0.95),
                Compress(bass, 1.12),
                Compress(mid, 1.00),
                Compress(treble, 0.92),
                spectrum,
                true);
        }

        private void FillWindow(short[] samples, int channels, int sourceIndex)
        {
            for (var i = 0; i < FftSize; i++)
            {
                var sampleIndex = sourceIndex + i * channels;
                var sample = sampleIndex < samples.Length ? samples[sampleIndex] / 32768.0 : 0;
                var window = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (FftSize - 1));

                real[i] = sample * window;
                imaginary[i] = 0;
            }
        }

        private float[] BuildVisualSpectrum(int sampleRate, float volumeFactor)
        {
            var spectrum = new float[VisualBins];
            var rawSpectrum = new double[VisualBins];
            var minFrequency = 35.0;
            var maxFrequency = Math.Min(12000.0, sampleRate / 2.0);
            var peak = 0.0001;

            for (var bin = 0; bin < VisualBins; bin++)
            {
                var t0 = bin / (double)VisualBins;
                var t1 = (bin + 1) / (double)VisualBins;
                var startFrequency = minFrequency * Math.Pow(maxFrequency / minFrequency, t0);
                var endFrequency = minFrequency * Math.Pow(maxFrequency / minFrequency, t1);
                var frequencyCompensation = 0.85 + Math.Pow(bin / (double)(VisualBins - 1), 0.65) * 1.85;
                rawSpectrum[bin] = AverageRange(sampleRate, startFrequency, endFrequency) * frequencyCompensation * volumeFactor;
                peak = Math.Max(peak, rawSpectrum[bin]);
            }

            for (var bin = 0; bin < VisualBins; bin++)
            {
                var absoluteValue = Compress(rawSpectrum[bin], 0.92);
                var relativeValue = Compress(rawSpectrum[bin] / peak, 1.45);
                var target = (float)(absoluteValue * 0.62 + relativeValue * 0.38);
                var attack = target > smoothedSpectrum[bin] ? 0.62f : 0.18f;

                smoothedSpectrum[bin] += (target - smoothedSpectrum[bin]) * attack;
                spectrum[bin] = smoothedSpectrum[bin];
            }

            return spectrum;
        }

        private double AverageRange(int sampleRate, double startFrequency, double endFrequency)
        {
            var startIndex = Math.Max(1, (int)(startFrequency * FftSize / sampleRate));
            var endIndex = Math.Min(FftSize / 2 - 1, (int)(endFrequency * FftSize / sampleRate));

            if (endIndex < startIndex)
            {
                return 0;
            }

            double total = 0;
            var count = 0;

            for (var i = startIndex; i <= endIndex; i++)
            {
                var magnitude = Math.Sqrt(real[i] * real[i] + imaginary[i] * imaginary[i]);
                total += magnitude;
                count++;
            }

            return count == 0 ? 0 : total / count / 58.0;
        }

        private static float Compress(double value, double gain)
        {
            var compressed = 1 - Math.Exp(-Math.Max(0, value) * gain);
            return (float)Math.Max(0, Math.Min(1, compressed));
        }

        private static void Transform(double[] real, double[] imaginary)
        {
            var n = real.Length;
            var j = 0;

            for (var i = 1; i < n; i++)
            {
                var bit = n >> 1;

                while ((j & bit) != 0)
                {
                    j ^= bit;
                    bit >>= 1;
                }

                j ^= bit;

                if (i < j)
                {
                    Swap(real, i, j);
                    Swap(imaginary, i, j);
                }
            }

            for (var length = 2; length <= n; length <<= 1)
            {
                var angle = -2 * Math.PI / length;
                var wLengthReal = Math.Cos(angle);
                var wLengthImaginary = Math.Sin(angle);

                for (var i = 0; i < n; i += length)
                {
                    var wReal = 1.0;
                    var wImaginary = 0.0;

                    for (var k = 0; k < length / 2; k++)
                    {
                        var evenIndex = i + k;
                        var oddIndex = evenIndex + length / 2;
                        var oddReal = real[oddIndex] * wReal - imaginary[oddIndex] * wImaginary;
                        var oddImaginary = real[oddIndex] * wImaginary + imaginary[oddIndex] * wReal;

                        real[oddIndex] = real[evenIndex] - oddReal;
                        imaginary[oddIndex] = imaginary[evenIndex] - oddImaginary;
                        real[evenIndex] += oddReal;
                        imaginary[evenIndex] += oddImaginary;

                        var nextReal = wReal * wLengthReal - wImaginary * wLengthImaginary;
                        wImaginary = wReal * wLengthImaginary + wImaginary * wLengthReal;
                        wReal = nextReal;
                    }
                }
            }
        }

        private static void Swap(double[] values, int first, int second)
        {
            var temporary = values[first];
            values[first] = values[second];
            values[second] = temporary;
        }
    }
}
