using System;
using System.Collections.Generic;
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

        event EventHandler MoodChanged;

        event EventHandler AutoModeChanged;

        event EventHandler<int> PlaylistSongSelected;

        event EventHandler<int> PlaylistSongRemoveRequested;

        event EventHandler<int> PlaylistFavoriteToggled;

        event EventHandler ExitRequested;

        int Volume { get; }

        bool IsShuffleEnabled { get; }

        bool IsAutoModeEnabled { get; }

        string SelectedVisualizationMode { get; }

        string SelectedMood { get; }

        string ShowAudioFileDialog();

        void ShowSongInfo(string songName);

        void ShowStatus(string status);

        void ShowAnalysisMode(string analysisMode);

        void ShowPlaybackTime(TimeSpan currentTime, TimeSpan duration);

        void ShowPlaybackControls(PlayerStatus status);

        void ShowPlaylist(IReadOnlyList<string> songs, int activeIndex, ISet<string> favorites);

        void ShowAudioPulse(AudioFrame frame);

        void SetVisualizationMode(string mode);

        void SetMood(string mood);

        void StartVisualizerTransition();

        void ShowVolume(int volume);

        void RefreshVisualizer();

        void ShowVisualizerPlaceholder(bool visible);

        void ShowError(string message);
    }
}
