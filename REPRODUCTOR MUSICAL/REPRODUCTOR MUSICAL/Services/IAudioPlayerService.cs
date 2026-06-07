using System;

namespace REPRODUCTOR_MUSICAL.Services
{
    public interface IAudioPlayerService
    {
        TimeSpan CurrentTime { get; }

        TimeSpan Duration { get; }

        int Volume { get; set; }

        bool IsLoaded { get; }

        void Load(string filePath);

        void Play();

        void Pause();

        void Stop();

        void Seek(TimeSpan position);
    }
}
