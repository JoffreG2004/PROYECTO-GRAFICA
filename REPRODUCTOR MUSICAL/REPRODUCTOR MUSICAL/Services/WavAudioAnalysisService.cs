using System;
using System.IO;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Services
{
    public class WavAudioAnalysisService : IAudioAnalysisService
    {
        private short[] samples = new short[0];
        private int sampleRate = 44100;
        private int channels = 1;
        private bool hasRealSamples;
        private readonly FftAnalyzer fftAnalyzer = new FftAnalyzer();
        private readonly AudioDynamicsProcessor dynamicsProcessor = new AudioDynamicsProcessor();

        public bool HasRealSamples => hasRealSamples;

        public void Load(string filePath)
        {
            samples = new short[0];
            sampleRate = 44100;
            channels = 1;
            hasRealSamples = false;
            dynamicsProcessor.Reset();

            if (!Path.GetExtension(filePath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TryLoadPcmWav(filePath);
        }

        public AudioFrame Analyze(TimeSpan currentTime, int volume)
        {
            var volumeFactor = volume / 100f;

            if (!hasRealSamples)
            {
                return CreateFallbackFrame(volumeFactor);
            }

            return dynamicsProcessor.Process(fftAnalyzer.Analyze(samples, sampleRate, channels, currentTime, volume));
        }

        private void TryLoadPcmWav(string filePath)
        {
            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                if (new string(reader.ReadChars(4)) != "RIFF")
                {
                    return;
                }

                reader.ReadInt32();

                if (new string(reader.ReadChars(4)) != "WAVE")
                {
                    return;
                }

                var bitsPerSample = 0;
                byte[] dataBytes = null;

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = new string(reader.ReadChars(4));
                    var chunkSize = reader.ReadInt32();
                    var nextChunk = reader.BaseStream.Position + chunkSize;

                    if (chunkId == "fmt ")
                    {
                        var audioFormat = reader.ReadInt16();
                        channels = reader.ReadInt16();
                        sampleRate = reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt16();
                        bitsPerSample = reader.ReadInt16();

                        if (audioFormat != 1 || bitsPerSample != 16)
                        {
                            return;
                        }
                    }
                    else if (chunkId == "data")
                    {
                        dataBytes = reader.ReadBytes(chunkSize);
                    }

                    reader.BaseStream.Position = Math.Min(nextChunk, reader.BaseStream.Length);
                }

                if (dataBytes == null || dataBytes.Length == 0 || bitsPerSample != 16)
                {
                    return;
                }

                samples = new short[dataBytes.Length / 2];
                Buffer.BlockCopy(dataBytes, 0, samples, 0, dataBytes.Length);
                hasRealSamples = samples.Length > 0;
            }
        }

        private static AudioFrame CreateFallbackFrame(float volumeFactor)
        {
            var idle = 0.10f * volumeFactor;

            return new AudioFrame(
                idle,
                idle,
                idle,
                idle,
                false);
        }

        private static double Normalize(double value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > 1 ? 1 : value;
        }
    }
}
