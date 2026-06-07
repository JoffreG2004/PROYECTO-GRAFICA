using System;

namespace REPRODUCTOR_MUSICAL.Models
{
    public class SeekRequestedEventArgs : EventArgs
    {
        public SeekRequestedEventArgs(TimeSpan position)
        {
            Position = position;
        }

        public TimeSpan Position { get; }
    }
}
