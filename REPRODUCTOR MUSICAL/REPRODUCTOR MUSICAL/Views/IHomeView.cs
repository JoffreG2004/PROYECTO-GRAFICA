using System;
using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Views
{
    public interface IHomeView
    {
        event EventHandler ViewLoaded;

        event EventHandler<PaintEventArgs> VisualizerPaintRequested;

        event EventHandler LoadSongRequested;

        event EventHandler PlayRequested;

        event EventHandler PauseRequested;

        event EventHandler StopRequested;

        event EventHandler PreviousSongRequested;

        event EventHandler NextSongRequested;

        event EventHandler ShuffleModeChanged;

        event EventHandler<SeekRequestedEventArgs> SeekRequested;

        event EventHandler VolumeChanged;

        event EventHandler VisualizationModeChanged;

        event EventHandler ExitRequested;

        int Volume { get; }

        bool IsShuffleEnabled { get; }

        string SelectedVisualizationMode { get; }

        string ShowAudioFileDialog();

        void ShowSongInfo(string songName);

        void ShowStatus(string status);

        void ShowAnalysisMode(string analysisMode);

        void ShowPlaybackTime(TimeSpan currentTime, TimeSpan duration);

        void ShowPlaybackControls(PlayerStatus status);

        void ShowVolume(int volume);

        void RefreshVisualizer();

        void ShowVisualizerPlaceholder(bool visible);

        void ShowError(string message);
    }
}

