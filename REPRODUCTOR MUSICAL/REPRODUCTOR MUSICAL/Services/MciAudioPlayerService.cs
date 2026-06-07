using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace REPRODUCTOR_MUSICAL.Services
{
    public class MciAudioPlayerService : IAudioPlayerService
    {
        private const string Alias = "ReproductorMusicalAudio";
        private bool isLoaded;
        private int volume = 70;
        private string cachedAudioPath = string.Empty;

        public TimeSpan CurrentTime => isLoaded ? TimeSpan.FromMilliseconds(GetStatusValue("position")) : TimeSpan.Zero;

        public TimeSpan Duration => isLoaded ? TimeSpan.FromMilliseconds(GetStatusValue("length")) : TimeSpan.Zero;

        public int Volume
        {
            get => volume;
            set
            {
                volume = Math.Max(0, Math.Min(100, value));

                if (isLoaded)
                {
                    TrySendCommand($"setaudio {Alias} volume to {volume * 10}");
                    SetWaveOutputVolume(volume);
                }
            }
        }

        public bool IsLoaded => isLoaded;

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("No se encontro el archivo de audio.", filePath);
            }

            CloseCurrentAudio();
            var playablePath = PreparePlayablePath(filePath);
            OpenAudio(playablePath);
            SendCommand($"set {Alias} time format milliseconds");
            isLoaded = true;
            Volume = volume;
        }

        public void Play()
        {
            EnsureLoaded();
            SendCommand($"play {Alias}");
        }

        public void Pause()
        {
            EnsureLoaded();
            SendCommand($"pause {Alias}");
        }

        public void Stop()
        {
            EnsureLoaded();
            SendCommand($"stop {Alias}");
            SendCommand($"seek {Alias} to start");
        }

        public void Seek(TimeSpan position)
        {
            EnsureLoaded();
            var milliseconds = Math.Max(0, (int)position.TotalMilliseconds);
            SendCommand($"seek {Alias} to {milliseconds}");
        }

        private void CloseCurrentAudio()
        {
            if (!isLoaded)
            {
                return;
            }

            SendCommand($"close {Alias}");
            isLoaded = false;
        }

        private int GetStatusValue(string statusName)
        {
            var buffer = new StringBuilder(128);
            SendCommand($"status {Alias} {statusName}", buffer, buffer.Capacity);

            return int.TryParse(buffer.ToString(), out var value) ? value : 0;
        }

        private static string BuildOpenCommand(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".wav")
            {
                return $"open \"{filePath}\" type waveaudio alias {Alias}";
            }

            return $"open \"{filePath}\" alias {Alias}";
        }

        private string PreparePlayablePath(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var cacheDirectory = Path.Combine(Path.GetTempPath(), "ReproductorMusical");

            Directory.CreateDirectory(cacheDirectory);
            cachedAudioPath = Path.Combine(cacheDirectory, "current_audio" + extension);

            File.Copy(filePath, cachedAudioPath, true);

            return cachedAudioPath;
        }

        private static void OpenAudio(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension != ".wav")
            {
                SendCommand(BuildOpenCommand(filePath));
                return;
            }

            if (TrySendCommand(BuildOpenCommand(filePath)))
            {
                return;
            }

            SendCommand($"open \"{filePath}\" alias {Alias}");
        }

        private static void SendCommand(string command)
        {
            SendCommand(command, null, 0);
        }

        private static bool TrySendCommand(string command)
        {
            try
            {
                SendCommand(command);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static void SendCommand(string command, StringBuilder returnValue, int returnLength)
        {
            var error = mciSendString(command, returnValue, returnLength, IntPtr.Zero);

            if (error == 0)
            {
                return;
            }

            var errorText = new StringBuilder(256);
            mciGetErrorString(error, errorText, errorText.Capacity);
            throw new InvalidOperationException(errorText.ToString());
        }

        private void EnsureLoaded()
        {
            if (!isLoaded)
            {
                throw new InvalidOperationException("Primero carga una cancion.");
            }
        }

        private static void SetWaveOutputVolume(int volume)
        {
            var value = (uint)(Math.Max(0, Math.Min(100, volume)) * 0xFFFF / 100);
            var packedVolume = value | (value << 16);
            waveOutSetVolume(IntPtr.Zero, packedVolume);
        }

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr callback);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern bool mciGetErrorString(int errorCode, StringBuilder errorText, int errorTextLength);

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr device, uint volume);
    }
}
