using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Graphics;
using REPRODUCTOR_MUSICAL.Models;
using REPRODUCTOR_MUSICAL.Services;
using REPRODUCTOR_MUSICAL.Views;

namespace REPRODUCTOR_MUSICAL.Controllers
{
    public class HomeController
    {
        private readonly IHomeView view;
        private readonly IAudioPlayerService audioPlayer;
        private readonly IAudioAnalysisService audioAnalysis;
        private readonly PlayerState playerState;
        private readonly Timer playbackTimer;
        private readonly Timer animationTimer;
        private readonly Dictionary<string, IVisualizer> visualizers;
        private IVisualizer currentVisualizer;

        public HomeController(IHomeView view, IAudioPlayerService audioPlayer, IAudioAnalysisService audioAnalysis, PlayerState playerState)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
            this.audioAnalysis = audioAnalysis ?? throw new ArgumentNullException(nameof(audioAnalysis));
            this.playerState = playerState ?? throw new ArgumentNullException(nameof(playerState));
            playbackTimer = new Timer { Interval = 250 };
            animationTimer = new Timer { Interval = 33 };
            visualizers = new Dictionary<string, IVisualizer>
            {
                { "Barras de espectro", new SpectrumBarsVisualizer() },
                { "Ondas circulares", new CircularWavesVisualizer() },
                { "Particulas ritmicas", new ParticleVisualizer() },
                { "Escena geometrica", new GeometrySceneVisualizer() }
            };
        }

        public void Initialize()
        {
            view.ViewLoaded += HandleViewLoaded;
            view.LoadSongRequested += HandleLoadSongRequested;
            view.PlayRequested += HandlePlayRequested;
            view.PauseRequested += HandlePauseRequested;
            view.StopRequested += HandleStopRequested;
            view.SeekRequested += HandleSeekRequested;
            view.VolumeChanged += HandleVolumeChanged;
            view.VisualizationModeChanged += HandleVisualizationModeChanged;
            view.ExitRequested += HandleExitRequested;
            view.VisualizerPaintRequested += HandleVisualizerPaintRequested;
            playbackTimer.Tick += HandlePlaybackTimerTick;
            animationTimer.Tick += HandleAnimationTimerTick;
        }

        private void HandleViewLoaded(object sender, EventArgs e)
        {
            view.ShowStatus("Detenido");
            view.ShowAnalysisMode("Analisis: esperando audio");
            view.ShowVolume(view.Volume);
            view.ShowPlaybackTime(TimeSpan.Zero, TimeSpan.Zero);
            audioPlayer.Volume = view.Volume;
            SelectVisualizer(view.SelectedVisualizationMode);
            view.ShowVisualizerPlaceholder(false);
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
                audioPlayer.Load(filePath);
                audioAnalysis.Load(filePath);
                audioPlayer.Volume = view.Volume;
                playerState.LoadFile(filePath);
                view.ShowSongInfo(Path.GetFileName(filePath));
                view.ShowStatus("Cancion cargada");
                view.ShowAnalysisMode(audioAnalysis.HasRealSamples ? "Analisis: FFT en tiempo real" : "Analisis: sin FFT real");
                view.ShowPlaybackTime(TimeSpan.Zero, audioPlayer.Duration);
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

            audioPlayer.Pause();
            playerState.MarkPaused();
            playbackTimer.Stop();
            view.ShowStatus("Pausado");
        }

        private void HandleStopRequested(object sender, EventArgs e)
        {
            if (!HasLoadedSong())
            {
                return;
            }

            audioPlayer.Stop();
            playerState.MarkStopped();
            playbackTimer.Stop();
            view.ShowStatus("Detenido");
            view.ShowPlaybackTime(TimeSpan.Zero, audioPlayer.Duration);
        }

        private void HandleSeekRequested(object sender, SeekRequestedEventArgs e)
        {
            if (!HasLoadedSong())
            {
                return;
            }

            audioPlayer.Seek(e.Position);
            view.ShowPlaybackTime(e.Position, audioPlayer.Duration);
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
            view.RefreshVisualizer();
        }

        private void HandleExitRequested(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void HandlePlaybackTimerTick(object sender, EventArgs e)
        {
            view.ShowPlaybackTime(audioPlayer.CurrentTime, audioPlayer.Duration);
        }

        private void HandleAnimationTimerTick(object sender, EventArgs e)
        {
            if (currentVisualizer == null)
            {
                return;
            }

            currentVisualizer.Update(CalculateAudioFrame());
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

        private AudioFrame CalculateAudioFrame()
        {
            if (playerState.Status != PlayerStatus.Playing)
            {
                return new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);
            }

            return audioAnalysis.Analyze(audioPlayer.CurrentTime, view.Volume);
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
