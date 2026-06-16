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
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(filePath)))
                {
                    if (ReadFourCc(reader) != "RIFF")
                    {
                        return;
                    }

                    reader.ReadUInt32();

                    if (ReadFourCc(reader) != "WAVE")
                    {
                        return;
                    }

                    var bitsPerSample = 0;
                    byte[] dataBytes = null;

                    while (TryReadChunkHeader(reader, out var chunkId, out var chunkSize))
                    {
                        var chunkStart = reader.BaseStream.Position;
                        var bytesRemaining = reader.BaseStream.Length - chunkStart;
                        var actualChunkSize = GetReadableChunkSize(chunkSize, bytesRemaining);
                        var nextChunk = chunkStart + actualChunkSize + (actualChunkSize % 2);

                        if (actualChunkSize < 0)
                        {
                            return;
                        }

                        if (chunkId == "fmt ")
                        {
                            if (actualChunkSize < 16)
                            {
                                return;
                            }

                            var audioFormat = reader.ReadInt16();
                            channels = reader.ReadInt16();
                            sampleRate = reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt16();
                            bitsPerSample = reader.ReadInt16();

                            if (audioFormat != 1 || bitsPerSample != 16 || channels <= 0 || sampleRate <= 0)
                            {
                                return;
                            }
                        }
                        else if (chunkId == "data")
                        {
                            if (actualChunkSize > int.MaxValue)
                            {
                                return;
                            }

                            var bytesToRead = (int)(actualChunkSize - (actualChunkSize % 2));
                            dataBytes = reader.ReadBytes(bytesToRead);
                        }

                        reader.BaseStream.Position = Math.Min(nextChunk, reader.BaseStream.Length);
                    }

                    if (dataBytes == null || dataBytes.Length == 0 || bitsPerSample != 16)
                    {
                        return;
                    }

                    samples = new short[dataBytes.Length / 2];
                    Buffer.BlockCopy(dataBytes, 0, samples, 0, samples.Length * 2);
                    hasRealSamples = samples.Length > 0;
                }
            }
            catch (IOException)
            {
            }
            catch (ArgumentException)
            {
            }
        }

        private static string ReadFourCc(BinaryReader reader)
        {
            return new string(reader.ReadChars(4));
        }

        private static bool TryReadChunkHeader(BinaryReader reader, out string chunkId, out uint chunkSize)
        {
            chunkId = null;
            chunkSize = 0;

            if (reader.BaseStream.Length - reader.BaseStream.Position < 8)
            {
                return false;
            }

            chunkId = ReadFourCc(reader);
            chunkSize = reader.ReadUInt32();
            return true;
        }

        private static long GetReadableChunkSize(uint declaredSize, long bytesRemaining)
        {
            if (bytesRemaining < 0)
            {
                return -1;
            }

            if (declaredSize == uint.MaxValue)
            {
                return bytesRemaining;
            }

            return Math.Min((long)declaredSize, bytesRemaining);
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
