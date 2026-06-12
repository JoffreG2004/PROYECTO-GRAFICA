using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Graphics;
using REPRODUCTOR_MUSICAL.Models;
using REPRODUCTOR_MUSICAL.Services;
using REPRODUCTOR_MUSICAL.Views;

namespace REPRODUCTOR_MUSICAL.Controllers
{
    public class HomeController
    {
        private static readonly string[] SupportedAudioExtensions = { ".mp3", ".wav", ".wma", ".aac", ".m4a", ".flac" };
        private static readonly string[] AutoVisualizationModes =
        {
            "Barras de espectro",
            "Ondas circulares",
            "Autopista Neon",
            "Orbitas cosmicas"
        };
        private readonly IHomeView view;
        private readonly IAudioPlayerService audioPlayer;
        private readonly IAudioAnalysisService audioAnalysis;
        private readonly PlayerState playerState;
        private readonly Timer playbackTimer;
        private readonly Timer animationTimer;
        private readonly Timer autoModeTimer;
        private readonly Dictionary<string, IVisualizer> visualizers;
        private readonly List<string> playlist = new List<string>();
        private readonly HashSet<string> favoriteSongs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private IVisualizer currentVisualizer;
        private int playlistIndex = -1;
        private int autoVisualizationIndex;
        private bool isChangingSong;
        private readonly Random random = new Random();

        public HomeController(IHomeView view, IAudioPlayerService audioPlayer, IAudioAnalysisService audioAnalysis, PlayerState playerState)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
            this.audioAnalysis = audioAnalysis ?? throw new ArgumentNullException(nameof(audioAnalysis));
            this.playerState = playerState ?? throw new ArgumentNullException(nameof(playerState));
            playbackTimer = new Timer { Interval = 250 };
            animationTimer = new Timer { Interval = 33 };
            autoModeTimer = new Timer { Interval = 13000 };
            visualizers = new Dictionary<string, IVisualizer>
            {
                { "Barras de espectro", new SpectrumBarsVisualizer() },
                { "Ondas circulares", new CircularWavesVisualizer() },
                { "Autopista Neon", new ParticleVisualizer() },
                { "Orbitas cosmicas", new GeometrySceneVisualizer() }
            };
        }

        public void Initialize()
        {
            view.ViewLoaded += HandleViewLoaded;
            view.LoadSongRequested += HandleLoadSongRequested;
            view.PlayRequested += HandlePlayRequested;
            view.PauseRequested += HandlePauseRequested;
            view.StopRequested += HandleStopRequested;
            view.PreviousSongRequested += HandlePreviousSongRequested;
            view.NextSongRequested += HandleNextSongRequested;
            view.ShuffleModeChanged += HandleShuffleModeChanged;
            view.SeekRequested += HandleSeekRequested;
            view.VolumeChanged += HandleVolumeChanged;
            view.VisualizationModeChanged += HandleVisualizationModeChanged;
            view.MoodChanged += HandleMoodChanged;
            view.AutoModeChanged += HandleAutoModeChanged;
            view.PlaylistSongSelected += HandlePlaylistSongSelected;
            view.PlaylistSongRemoveRequested += HandlePlaylistSongRemoveRequested;
            view.PlaylistFavoriteToggled += HandlePlaylistFavoriteToggled;
            view.ExitRequested += HandleExitRequested;
            view.VisualizerPaintRequested += HandleVisualizerPaintRequested;
            playbackTimer.Tick += HandlePlaybackTimerTick;
            animationTimer.Tick += HandleAnimationTimerTick;
            autoModeTimer.Tick += HandleAutoModeTimerTick;
        }

        private void HandleViewLoaded(object sender, EventArgs e)
        {
            view.ShowStatus("Detenido");
            view.ShowAnalysisMode("Analisis: esperando audio");
            view.ShowPlaybackControls(PlayerStatus.Stopped);
            view.ShowVolume(view.Volume);
            view.ShowPlaybackTime(TimeSpan.Zero, TimeSpan.Zero);
            RefreshPlaylistView();
            audioPlayer.Volume = view.Volume;
            SelectVisualizer(view.SelectedVisualizationMode);
            view.ShowVisualizerPlaceholder(false);
            LoadDefaultPlaylist();
            animationTimer.Start();
        }

        private void HandleLoadSongRequested(object sender, EventArgs e)
        {
            var filePath = view.ShowAudioFileDialog();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                BuildPlaylistFromFolder(Path.GetDirectoryName(filePath), filePath);
                LoadSong(filePath, $"Cancion cargada ({playlistIndex + 1}/{Math.Max(1, playlist.Count)})");
            }
            catch (Exception exception)
            {
                view.ShowError(exception.Message);
            }
        }

        private void HandlePlayRequested(object sender, EventArgs e)
        {
            if (!HasLoadedSong())
            {
                return;
            }

            try
            {
                audioPlayer.Play();
                playerState.MarkPlaying();
                playbackTimer.Start();
                view.ShowPlaybackControls(playerState.Status);
                view.ShowStatus("Reproduciendo");
            }
            catch (Exception exception)
            {
                view.ShowError(exception.Message);
            }
        }

        private void HandlePauseRequested(object sender, EventArgs e)
        {
            if (!HasLoadedSong())
            {
                return;
            }

            try
            {
                audioPlayer.Pause();
                playerState.MarkPaused();
                playbackTimer.Stop();
                view.ShowPlaybackControls(playerState.Status);
                view.ShowStatus("Pausado");
            }
            catch (Exception exception)
            {
                view.ShowError(exception.Message);
            }
        }

        private void HandleStopRequested(object sender, EventArgs e)
        {
            if (!HasLoadedSong())
            {
                return;
            }

            try
            {
                audioPlayer.Stop();
                playerState.MarkStopped();
                playbackTimer.Stop();
                view.ShowPlaybackControls(playerState.Status);
                view.ShowStatus("Detenido");
                view.ShowPlaybackTime(TimeSpan.Zero, audioPlayer.Duration);
            }
            catch (Exception exception)
            {
                view.ShowError(exception.Message);
            }
        }

        private void HandlePreviousSongRequested(object sender, EventArgs e)
        {
            ChangePlaylistSong(-1, false);
        }

        private void HandleNextSongRequested(object sender, EventArgs e)
        {
            ChangePlaylistSong(1, view.IsShuffleEnabled);
        }

        private void HandleShuffleModeChanged(object sender, EventArgs e)
        {
            view.ShowStatus(view.IsShuffleEnabled ? "Aleatorio activado" : "Aleatorio desactivado");
        }

        private void HandleSeekRequested(object sender, SeekRequestedEventArgs e)
        {
            if (!HasLoadedSong())
            {
                return;
            }

            try
            {
                audioPlayer.Seek(e.Position);
                view.ShowPlaybackTime(e.Position, audioPlayer.Duration);
            }
            catch (Exception exception)
            {
                view.ShowError(exception.Message);
            }
        }

        private void HandleVolumeChanged(object sender, EventArgs e)
        {
            audioPlayer.Volume = view.Volume;
            view.ShowVolume(view.Volume);
        }

        private void HandleVisualizationModeChanged(object sender, EventArgs e)
        {
            view.ShowStatus($"Visualizacion: {view.SelectedVisualizationMode}");
            SelectVisualizer(view.SelectedVisualizationMode);
            SyncAutoIndexes();
            view.RefreshVisualizer();
        }

        private void HandleMoodChanged(object sender, EventArgs e)
        {
            view.ShowStatus($"Ambiente: {view.SelectedMood}");
            SyncAutoIndexes();
            view.RefreshVisualizer();
            RefreshPlaylistView();
        }

        private void HandleAutoModeChanged(object sender, EventArgs e)
        {
            if (view.IsAutoModeEnabled)
            {
                SyncAutoIndexes();
                autoModeTimer.Start();
                view.ShowStatus("Auto figuras activado");
                return;
            }

            autoModeTimer.Stop();
            view.ShowStatus("Auto figuras desactivado");
        }

        private void HandleAutoModeTimerTick(object sender, EventArgs e)
        {
            autoVisualizationIndex = (autoVisualizationIndex + 1) % AutoVisualizationModes.Length;
            view.SetVisualizationMode(AutoVisualizationModes[autoVisualizationIndex]);
            view.ShowStatus($"Auto figuras: {AutoVisualizationModes[autoVisualizationIndex]}");
        }

        private void HandlePlaylistSongSelected(object sender, int index)
        {
            if (index < 0 || index >= playlist.Count)
            {
                return;
            }

            var shouldPlay = playerState.Status == PlayerStatus.Playing;
            playlistIndex = index;
            LoadSong(playlist[playlistIndex], $"Cancion seleccionada ({playlistIndex + 1}/{playlist.Count})");

            if (shouldPlay)
            {
                audioPlayer.Play();
                playerState.MarkPlaying();
                playbackTimer.Start();
                view.ShowPlaybackControls(playerState.Status);
                view.ShowStatus($"Reproduciendo lista ({playlistIndex + 1}/{playlist.Count})");
            }
        }

        private void HandlePlaylistSongRemoveRequested(object sender, int index)
        {
            if (index < 0 || index >= playlist.Count)
            {
                return;
            }

            var removingCurrent = index == playlistIndex;
            var shouldKeepPlaying = playerState.Status == PlayerStatus.Playing;
            favoriteSongs.Remove(playlist[index]);
            playlist.RemoveAt(index);

            if (playlist.Count == 0)
            {
                playlistIndex = -1;
                audioPlayer.Stop();
                playerState.ClearFile();
                playbackTimer.Stop();
                view.ShowSongInfo(string.Empty);
                view.ShowStatus("Lista vacia");
                view.ShowAnalysisMode("Analisis: esperando audio");
                view.ShowPlaybackTime(TimeSpan.Zero, TimeSpan.Zero);
                view.ShowPlaybackControls(playerState.Status);
                RefreshPlaylistView();
                return;
            }

            if (index < playlistIndex)
            {
                playlistIndex--;
            }
            else if (playlistIndex >= playlist.Count)
            {
                playlistIndex = playlist.Count - 1;
            }

            if (removingCurrent)
            {
                LoadSong(playlist[playlistIndex], $"Cancion removida ({playlistIndex + 1}/{playlist.Count})");
                if (shouldKeepPlaying)
                {
                    audioPlayer.Play();
                    playerState.MarkPlaying();
                    playbackTimer.Start();
                    view.ShowPlaybackControls(playerState.Status);
                    view.ShowStatus($"Reproduciendo lista ({playlistIndex + 1}/{playlist.Count})");
                }
            }
            else
            {
                RefreshPlaylistView();
                view.ShowStatus($"Cancion removida ({playlist.Count} en lista)");
            }
        }

        private void HandlePlaylistFavoriteToggled(object sender, int index)
        {
            if (index < 0 || index >= playlist.Count)
            {
                return;
            }

            var song = playlist[index];
            if (!favoriteSongs.Add(song))
            {
                favoriteSongs.Remove(song);
            }

            RefreshPlaylistView();
        }

        private void HandleExitRequested(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void HandlePlaybackTimerTick(object sender, EventArgs e)
        {
            view.ShowPlaybackTime(audioPlayer.CurrentTime, audioPlayer.Duration);
            AdvancePlaylistIfSongFinished();
        }

        private void HandleAnimationTimerTick(object sender, EventArgs e)
        {
            if (currentVisualizer == null)
            {
                return;
            }

            var audioFrame = CalculateAudioFrame();
            currentVisualizer.Update(audioFrame);
            view.ShowAudioPulse(audioFrame);
            view.RefreshVisualizer();
        }

        private void HandleVisualizerPaintRequested(object sender, PaintEventArgs e)
        {
            currentVisualizer?.Render(e.Graphics, e.ClipRectangle);
        }

        private void SelectVisualizer(string mode)
        {
            if (!visualizers.TryGetValue(mode, out currentVisualizer))
            {
                currentVisualizer = visualizers["Barras de espectro"];
            }
        }

        private void SyncAutoIndexes()
        {
            var visualizationIndex = Array.IndexOf(AutoVisualizationModes, view.SelectedVisualizationMode);
            if (visualizationIndex >= 0)
            {
                autoVisualizationIndex = visualizationIndex;
            }
        }

        private AudioFrame CalculateAudioFrame()
        {
            if (!audioPlayer.IsLoaded || playerState.Status == PlayerStatus.Stopped)
            {
                return new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);
            }

            return audioAnalysis.Analyze(audioPlayer.CurrentTime, view.Volume);
        }

        private void LoadDefaultPlaylist()
        {
            var songsFolder = FindSongsFolder();
            if (string.IsNullOrWhiteSpace(songsFolder))
            {
                return;
            }

            BuildPlaylistFromFolder(songsFolder, null);
            if (playlist.Count == 0)
            {
                RefreshPlaylistView();
                return;
            }

            playlistIndex = 0;
            LoadSong(playlist[playlistIndex], $"Lista cargada ({playlist.Count} canciones)");
        }

        private void BuildPlaylistFromFolder(string folderPath, string selectedFile)
        {
            playlist.Clear();
            playlistIndex = -1;

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return;
            }

            playlist.AddRange(Directory.GetFiles(folderPath)
                .Where(IsSupportedAudioFile)
                .OrderBy(Path.GetFileName, StringComparer.CurrentCultureIgnoreCase));

            if (!string.IsNullOrWhiteSpace(selectedFile))
            {
                playlistIndex = playlist.FindIndex(file => string.Equals(file, selectedFile, StringComparison.OrdinalIgnoreCase));
            }

            if (playlistIndex < 0 && playlist.Count > 0)
            {
                playlistIndex = 0;
            }

            RefreshPlaylistView();
        }

        private void LoadSong(string filePath, string status)
        {
            audioPlayer.Load(filePath);
            audioAnalysis.Load(filePath);
            audioPlayer.Volume = view.Volume;
            playerState.LoadFile(filePath);
            view.ShowPlaybackControls(playerState.Status);
            view.ShowSongInfo(Path.GetFileName(filePath));
            view.ShowStatus(status);
            view.ShowAnalysisMode(audioAnalysis.HasRealSamples ? "Analisis: FFT en tiempo real" : "Analisis: sin FFT real");
            view.ShowPlaybackTime(TimeSpan.Zero, audioPlayer.Duration);
            RefreshPlaylistView();
        }

        private void RefreshPlaylistView()
        {
            view.ShowPlaylist(playlist, playlistIndex, favoriteSongs);
        }

        private void AdvancePlaylistIfSongFinished()
        {
            if (isChangingSong || playerState.Status != PlayerStatus.Playing || playlist.Count == 0 || audioPlayer.Duration <= TimeSpan.Zero)
            {
                return;
            }

            if (audioPlayer.CurrentTime < audioPlayer.Duration - TimeSpan.FromMilliseconds(650))
            {
                return;
            }

            PlayNextSong(view.IsShuffleEnabled);
        }

        private void PlayNextSong(bool useShuffle)
        {
            ChangePlaylistSong(1, useShuffle, true);
        }

        private void ChangePlaylistSong(int direction, bool useShuffle)
        {
            var shouldKeepPlaying = playerState.Status == PlayerStatus.Playing;
            ChangePlaylistSong(direction, useShuffle, shouldKeepPlaying);
        }

        private void ChangePlaylistSong(int direction, bool useShuffle, bool shouldPlay)
        {
            if (isChangingSong || playlist.Count == 0)
            {
                view.ShowError("No hay canciones en la lista.");
                return;
            }

            isChangingSong = true;

            try
            {
                playlistIndex = GetNextPlaylistIndex(direction, useShuffle);
                var song = playlist[playlistIndex];
                LoadSong(song, $"Lista {(view.IsShuffleEnabled ? "aleatoria" : "normal")} ({playlistIndex + 1}/{playlist.Count})");

                if (shouldPlay)
                {
                    audioPlayer.Play();
                    playerState.MarkPlaying();
                    playbackTimer.Start();
                    view.ShowPlaybackControls(playerState.Status);
                    view.ShowStatus($"Reproduciendo lista ({playlistIndex + 1}/{playlist.Count})");
                }
            }
            catch (Exception exception)
            {
                playbackTimer.Stop();
                playerState.MarkStopped();
                view.ShowPlaybackControls(playerState.Status);
                view.ShowError(exception.Message);
            }
            finally
            {
                isChangingSong = false;
            }
        }

        private int GetNextPlaylistIndex(int direction, bool useShuffle)
        {
            if (playlist.Count <= 1)
            {
                return 0;
            }

            if (useShuffle)
            {
                var nextIndex = playlistIndex;
                while (nextIndex == playlistIndex)
                {
                    nextIndex = random.Next(playlist.Count);
                }

                return nextIndex;
            }

            return (playlistIndex + direction + playlist.Count) % playlist.Count;
        }

        private static string FindSongsFolder()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "CANCIONES");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return string.Empty;
        }

        private static bool IsSupportedAudioFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return SupportedAudioExtensions.Any(item => string.Equals(item, extension, StringComparison.OrdinalIgnoreCase));
        }

        private bool HasLoadedSong()
        {
            if (!string.IsNullOrWhiteSpace(playerState.CurrentFilePath))
            {
                return true;
            }

            view.ShowError("Primero carga una cancion.");
            return false;
        }
    }
}


