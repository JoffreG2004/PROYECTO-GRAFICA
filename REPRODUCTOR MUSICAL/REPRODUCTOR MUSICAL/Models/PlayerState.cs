namespace REPRODUCTOR_MUSICAL.Models
{
    public enum PlayerStatus
    {
        Stopped,
        Playing,
        Paused
    }

    public class PlayerState
    {
        public PlayerStatus Status { get; private set; } = PlayerStatus.Stopped;

        public string CurrentFilePath { get; private set; } = string.Empty;

        public void LoadFile(string filePath)
        {
            CurrentFilePath = filePath;
            Status = PlayerStatus.Stopped;
        }

        public void ClearFile()
        {
            CurrentFilePath = string.Empty;
            Status = PlayerStatus.Stopped;
        }

        public void MarkPlaying()
        {
            Status = PlayerStatus.Playing;
        }

        public void MarkPaused()
        {
            Status = PlayerStatus.Paused;
        }

        public void MarkStopped()
        {
            Status = PlayerStatus.Stopped;
        }
    }
}
