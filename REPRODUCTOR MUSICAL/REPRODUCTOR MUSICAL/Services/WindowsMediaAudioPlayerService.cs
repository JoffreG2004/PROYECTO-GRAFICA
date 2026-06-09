using System;
using System.IO;

namespace REPRODUCTOR_MUSICAL.Services
{
    public class WindowsMediaAudioPlayerService : IAudioPlayerService
    {
        private readonly dynamic player;
        private bool isLoaded;
        private int volume = 50;

        public WindowsMediaAudioPlayerService()
        {
            var playerType = Type.GetTypeFromProgID("WMPlayer.OCX");
            if (playerType == null)
            {
                throw new InvalidOperationException("Windows Media Player no esta disponible en este equipo.");
            }

            player = Activator.CreateInstance(playerType);
            player.settings.autoStart = false;
            player.settings.volume = volume;
        }

        public TimeSpan CurrentTime => isLoaded
            ? TimeSpan.FromSeconds(Math.Max(0, (double)player.controls.currentPosition))
            : TimeSpan.Zero;

        public TimeSpan Duration
        {
            get
            {
                if (!isLoaded || player.currentMedia == null)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromSeconds(Math.Max(0, (double)player.currentMedia.duration));
            }
        }

        public int Volume
        {
            get => volume;
            set
            {
                volume = Math.Max(0, Math.Min(100, value));
                player.settings.volume = volume;
            }
        }

        public bool IsLoaded => isLoaded;

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("No se encontro el archivo de audio.", filePath);
            }

            player.controls.stop();
            player.URL = filePath;
            isLoaded = true;
            Volume = volume;
        }

        public void Play()
        {
            EnsureLoaded();
            player.controls.play();
        }

        public void Pause()
        {
            EnsureLoaded();
            player.controls.pause();
        }

        public void Stop()
        {
            EnsureLoaded();
            player.controls.stop();
            Seek(TimeSpan.Zero);
        }

        public void Seek(TimeSpan position)
        {
            EnsureLoaded();
            player.controls.currentPosition = Math.Max(0, position.TotalSeconds);
        }

        private void EnsureLoaded()
        {
            if (!isLoaded)
            {
                throw new InvalidOperationException("Primero carga una cancion.");
            }
        }
    }
}
