using System;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Services
{
    public interface IAudioAnalysisService
    {
        bool HasRealSamples { get; }

        void Load(string filePath);

        AudioFrame Analyze(TimeSpan currentTime, int volume);
    }
}
