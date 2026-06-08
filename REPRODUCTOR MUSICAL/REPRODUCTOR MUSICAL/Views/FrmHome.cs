using System;
using System.IO;
using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Models;
using REPRODUCTOR_MUSICAL.Views;

namespace REPRODUCTOR_MUSICAL
{
    public partial class FrmHome : Form, IHomeView
    {
        public event EventHandler ViewLoaded;

        public event EventHandler<PaintEventArgs> VisualizerPaintRequested;

        public event EventHandler LoadSongRequested;

        public event EventHandler PlayRequested;

        public event EventHandler PauseRequested;

        public event EventHandler StopRequested;

        public event EventHandler PreviousSongRequested;

        public event EventHandler NextSongRequested;

        public event EventHandler ShuffleModeChanged;

        public event EventHandler<SeekRequestedEventArgs> SeekRequested;

        public event EventHandler VolumeChanged;

        public event EventHandler VisualizationModeChanged;

        public event EventHandler ExitRequested;

        public FrmHome()
        {
            InitializeComponent();
            WireEvents();
            ShowVolume(Volume);
        }

        public int Volume => trackVolumen.Value;

        public bool IsShuffleEnabled => chkAleatorio.Checked;

        public string SelectedVisualizationMode => cmbModoVisualizacion.Text;

        public string ShowAudioFileDialog()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Seleccionar cancion";
                dialog.Filter = "Archivos de audio|*.mp3;*.wav;*.wma;*.aac|Todos los archivos|*.*";
                dialog.Multiselect = false;

                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : string.Empty;
            }
        }

        public void ShowSongInfo(string songName)
        {
            lblCancion.Text = string.IsNullOrWhiteSpace(songName)
                ? "Ninguna cancion cargada"
                : FormatSongTitle(Path.GetFileNameWithoutExtension(songName));
        }

        public void ShowStatus(string status)
        {
            lblEstado.Text = status;
        }

        public void ShowAnalysisMode(string analysisMode)
        {
            lblAnalisis.Text = analysisMode;
        }

        public void ShowPlaybackTime(TimeSpan currentTime, TimeSpan duration)
        {
            lblTiempoActual.Text = FormatTime(currentTime);
            lblTiempoTotal.Text = FormatTime(duration);

            var totalSeconds = Math.Max(1, (int)duration.TotalSeconds);
            var currentSeconds = Math.Min(totalSeconds, Math.Max(0, (int)currentTime.TotalSeconds));

            if (!trackPosicion.Focused)
            {
                trackPosicion.Maximum = totalSeconds;
                trackPosicion.Value = currentSeconds;
            }
        }

        public void ShowVolume(int volume)
        {
            lblVolumen.Text = $"Volumen {volume}";
        }

        public void RefreshVisualizer()
        {
            panelVisualizador.Invalidate();
        }

        public void ShowVisualizerPlaceholder(bool visible)
        {
            lblVisualizador.Visible = visible;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, "Reproductor Musical", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void WireEvents()
        {
            btnCargar.Click += (sender, args) => LoadSongRequested?.Invoke(this, EventArgs.Empty);
            menuCargarCancion.Click += (sender, args) => LoadSongRequested?.Invoke(this, EventArgs.Empty);
            btnReproducir.Click += (sender, args) => PlayRequested?.Invoke(this, EventArgs.Empty);
            btnPausar.Click += (sender, args) => PauseRequested?.Invoke(this, EventArgs.Empty);
            btnDetener.Click += (sender, args) => StopRequested?.Invoke(this, EventArgs.Empty);
            btnAnterior.Click += (sender, args) => PreviousSongRequested?.Invoke(this, EventArgs.Empty);
            btnSiguiente.Click += (sender, args) => NextSongRequested?.Invoke(this, EventArgs.Empty);
            chkAleatorio.CheckedChanged += (sender, args) => ShuffleModeChanged?.Invoke(this, EventArgs.Empty);
            trackPosicion.Scroll += (sender, args) => SeekRequested?.Invoke(this, new SeekRequestedEventArgs(TimeSpan.FromSeconds(trackPosicion.Value)));
            trackVolumen.Scroll += (sender, args) => VolumeChanged?.Invoke(this, EventArgs.Empty);
            cmbModoVisualizacion.SelectedIndexChanged += (sender, args) => VisualizationModeChanged?.Invoke(this, EventArgs.Empty);
            menuBarras.Click += (sender, args) => SelectVisualizationMode("Barras de espectro");
            menuOndas.Click += (sender, args) => SelectVisualizationMode("Ondas circulares");
            menuParticulas.Click += (sender, args) => SelectVisualizationMode("Particulas ritmicas");
            menuGeometria.Click += (sender, args) => SelectVisualizationMode("Escena geometrica");
            menuSalir.Click += (sender, args) => ExitRequested?.Invoke(this, EventArgs.Empty);
            panelVisualizador.Paint += (sender, args) => VisualizerPaintRequested?.Invoke(this, args);
        }

        private void SelectVisualizationMode(string mode)
        {
            cmbModoVisualizacion.SelectedItem = mode;
            VisualizationModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private static string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }

        private static string FormatSongTitle(string fileName)
        {
            return fileName
                .Replace('_', ' ')
                .Replace("  ", " ")
                .Trim();
        }

        private void UpdateSeekFromMouse(int mouseX)
        {
            if (trackPosicion.Maximum <= 0)
            {
                return;
            }

            var ratio = Math.Max(0, Math.Min(1, mouseX / (double)trackPosicion.Width));
            trackPosicion.Value = Math.Max(trackPosicion.Minimum, Math.Min(trackPosicion.Maximum, (int)(ratio * trackPosicion.Maximum)));
            SeekRequested?.Invoke(this, new SeekRequestedEventArgs(TimeSpan.FromSeconds(trackPosicion.Value)));
        }

        private void FrmHome_Load(object sender, EventArgs e)
        {

        }
    }
}

