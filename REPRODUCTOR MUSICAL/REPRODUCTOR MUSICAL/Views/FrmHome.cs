using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Models;
using REPRODUCTOR_MUSICAL.Views;

namespace REPRODUCTOR_MUSICAL
{
    public partial class FrmHome : Form, IHomeView
    {
        private const int SongTitleStartPauseTicks = 28;
        private const int SongTitleEndPauseTicks = 22;
        private const int SongTitleStep = 1;

        private readonly Timer songTitleTimer = new Timer { Interval = 45 };
        private readonly Timer transitionTimer = new Timer { Interval = 33 };
        private readonly HashSet<string> favoriteSongs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MoodPalette> moods = new Dictionary<string, MoodPalette>(StringComparer.OrdinalIgnoreCase)
        {
            { "Neon Tokyo", new MoodPalette(Color.FromArgb(8, 9, 22), Color.FromArgb(16, 18, 42), Color.FromArgb(41, 221, 218), Color.FromArgb(255, 72, 176), Color.FromArgb(255, 204, 80)) },
            { "Lava Pop", new MoodPalette(Color.FromArgb(24, 6, 9), Color.FromArgb(50, 13, 18), Color.FromArgb(255, 72, 38), Color.FromArgb(255, 205, 45), Color.FromArgb(61, 240, 255)) },
            { "Toxic Candy", new MoodPalette(Color.FromArgb(8, 18, 4), Color.FromArgb(22, 42, 9), Color.FromArgb(184, 255, 28), Color.FromArgb(255, 45, 212), Color.FromArgb(255, 245, 82)) },
            { "Ice Carnival", new MoodPalette(Color.FromArgb(4, 13, 24), Color.FromArgb(8, 32, 54), Color.FromArgb(42, 230, 255), Color.FromArgb(255, 112, 238), Color.FromArgb(170, 255, 82)) },
            { "Dark Galaxy", new MoodPalette(Color.FromArgb(7, 7, 13), Color.FromArgb(18, 16, 29), Color.FromArgb(185, 192, 255), Color.FromArgb(144, 82, 255), Color.FromArgb(255, 84, 126)) }
        };

        private RoundedPanel panelPlaylistShell;
        private FlowLayoutPanel flowPlaylist;
        private Label lblPlaylistTitle;
        private Label lblPlaylistCount;
        private Label lblMood;
        private NeonComboBox cmbMood;
        private Label lblNowPlayingCaption;
        private Label lblNowPlayingDuration;
        private PictureBox picAlbumCover;
        private PictureBox picHeaderCover;
        private Panel panelMiniWave;
        private RoundedPanel panelPresentacionInfo;
        private Label lblPresentacionCancion;
        private Label lblPresentacionTiempo;
        private GradientButton btnPresentacion;
        private GradientButton btnAutoMode;
        private MoodPalette currentMood;
        private AudioFrame lastAudioFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);
        private TimeSpan lastDuration = TimeSpan.Zero;
        private FormBorderStyle previousBorderStyle;
        private FormWindowState previousWindowState;
        private Rectangle previousBounds;
        private Padding previousPrincipalPadding;
        private bool isPresentationMode;
        private bool suppressSelectionEvents;
        private int transitionFrame;
        private int songTitleHomeLeft;
        private int songTitleViewportWidth;
        private int songTitleMinimumLeft;
        private int songTitlePauseTicks;

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

        public event EventHandler MoodChanged;

        public event EventHandler AutoModeChanged;

        public event EventHandler<int> PlaylistSongSelected;

        public event EventHandler<int> PlaylistSongRemoveRequested;

        public event EventHandler<int> PlaylistFavoriteToggled;

        public event EventHandler ExitRequested;

        public FrmHome()
        {
            InitializeComponent();
            BuildUxUpgrade();
            InitializeSongTitleMarquee();
            WireEvents();
            ApplyMood("Neon Tokyo");
            ShowVolume(Volume);
        }

        public int Volume => trackVolumen.Value;

        public bool IsShuffleEnabled => chkAleatorio.Checked;

        public bool IsAutoModeEnabled => btnAutoMode != null && btnAutoMode.IsActive;

        public string SelectedVisualizationMode => cmbModoVisualizacion.Text;

        public string SelectedMood => cmbMood == null ? "Neon Tokyo" : cmbMood.Text;

        public string ShowAudioFileDialog()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Seleccionar cancion";
                dialog.Filter = "Archivos de audio|*.mp3;*.wav;*.wma;*.aac;*.m4a;*.flac|Todos los archivos|*.*";
                dialog.Multiselect = false;

                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : string.Empty;
            }
        }

        public void ShowSongInfo(string songName)
        {
            var cleanTitle = string.IsNullOrWhiteSpace(songName)
                ? "Ninguna cancion cargada"
                : FormatSongTitle(Path.GetFileNameWithoutExtension(songName));

            lblCancion.Text = string.IsNullOrWhiteSpace(songName)
                ? "Ninguna cancion cargada"
                : cleanTitle;

            if (lblNowPlayingCaption != null)
            {
                lblNowPlayingCaption.Text = cleanTitle;
            }

            if (lblPresentacionCancion != null)
            {
                lblPresentacionCancion.Text = cleanTitle;
            }

            ResetSongTitleMarquee();
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
            lastDuration = duration;

            if (lblNowPlayingDuration != null)
            {
                lblNowPlayingDuration.Text = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
            }

            if (lblPresentacionTiempo != null)
            {
                lblPresentacionTiempo.Text = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
            }

            var totalSeconds = Math.Max(1, (int)duration.TotalSeconds);
            var currentSeconds = Math.Min(totalSeconds, Math.Max(0, (int)currentTime.TotalSeconds));

            if (!trackPosicion.Focused)
            {
                trackPosicion.Maximum = totalSeconds;
                trackPosicion.Value = currentSeconds;
            }
        }

        public void ShowPlaybackControls(PlayerStatus status)
        {
            btnReproducir.IsActive = status == PlayerStatus.Playing;
            btnPausar.IsActive = status == PlayerStatus.Paused;
            btnDetener.IsActive = status == PlayerStatus.Stopped;
        }

        public void ShowPlaylist(IReadOnlyList<string> songs, int activeIndex, ISet<string> favorites)
        {
            if (flowPlaylist == null)
            {
                return;
            }

            favoriteSongs.Clear();
            if (favorites != null)
            {
                foreach (var favorite in favorites)
                {
                    favoriteSongs.Add(favorite);
                }
            }

            flowPlaylist.SuspendLayout();
            flowPlaylist.Controls.Clear();
            var favoriteCount = songs.Count(song => favoriteSongs.Contains(song));
            lblPlaylistCount.Text = favoriteCount == 0
                ? $"{songs.Count} canciones"
                : $"{songs.Count} canciones | {favoriteCount} fav";

            if (songs.Count == 0)
            {
                flowPlaylist.Controls.Add(CreateEmptyPlaylistLabel());
            }
            else
            {
                var orderedIndexes = Enumerable.Range(0, songs.Count)
                    .OrderByDescending(index => favoriteSongs.Contains(songs[index]))
                    .ThenBy(index => index == activeIndex ? 0 : 1)
                    .ThenBy(index => index)
                    .ToList();

                foreach (var index in orderedIndexes)
                {
                    flowPlaylist.Controls.Add(CreatePlaylistRow(songs[index], index, index == activeIndex));
                }
            }

            flowPlaylist.ResumeLayout();
        }

        public void ShowAudioPulse(AudioFrame frame)
        {
            lastAudioFrame = frame ?? new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);
            panelMiniWave?.Invalidate();
            panelPresentacionInfo?.Invalidate();
        }

        public void SetVisualizationMode(string mode)
        {
            if (cmbModoVisualizacion.Items.Contains(mode))
            {
                suppressSelectionEvents = true;
                cmbModoVisualizacion.SelectedItem = mode;
                suppressSelectionEvents = false;
                StartVisualizerTransition();
                VisualizationModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetMood(string mood)
        {
            if (cmbMood.Items.Contains(mood))
            {
                suppressSelectionEvents = true;
                cmbMood.SelectedItem = mood;
                suppressSelectionEvents = false;
                ApplyMood(mood);
                MoodChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StartVisualizerTransition()
        {
            transitionFrame = 10;
            transitionTimer.Start();
            panelVisualizador.Invalidate();
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
            toolTipPrincipal.SetToolTip(chkAleatorio, "Aleatorio");
            chkAleatorio.CheckedChanged += (sender, args) => ShuffleModeChanged?.Invoke(this, EventArgs.Empty);
            trackPosicion.Scroll += (sender, args) => SeekRequested?.Invoke(this, new SeekRequestedEventArgs(TimeSpan.FromSeconds(trackPosicion.Value)));
            trackVolumen.Scroll += (sender, args) => VolumeChanged?.Invoke(this, EventArgs.Empty);
            cmbModoVisualizacion.SelectedIndexChanged += (sender, args) =>
            {
                if (suppressSelectionEvents)
                {
                    return;
                }

                StartVisualizerTransition();
                VisualizationModeChanged?.Invoke(this, EventArgs.Empty);
            };
            cmbMood.SelectedIndexChanged += (sender, args) =>
            {
                if (suppressSelectionEvents)
                {
                    return;
                }

                ApplyMood(SelectedMood);
                MoodChanged?.Invoke(this, EventArgs.Empty);
            };
            menuBarras.Click += (sender, args) => SelectVisualizationMode("Barras de espectro");
            menuOndas.Click += (sender, args) => SelectVisualizationMode("Ondas circulares");
            menuParticulas.Click += (sender, args) => SelectVisualizationMode("Autopista Neon");
            menuGeometria.Click += (sender, args) => SelectVisualizationMode("Orbitas cosmicas");
            menuSalir.Click += (sender, args) => ExitRequested?.Invoke(this, EventArgs.Empty);
            panelVisualizador.Paint += HandleVisualizerPanelPaint;
            panelMiniWave.Paint += HandleMiniWavePaint;
            panelPresentacionInfo.Paint += HandlePresentationInfoPaint;
            panelContenido.Resize += (sender, args) => LayoutUxUpgrade();
            transitionTimer.Tick += HandleTransitionTimerTick;
            btnPresentacion.Click += (sender, args) => TogglePresentationMode();
            btnAutoMode.Click += (sender, args) => ToggleAutoMode();
            KeyPreview = true;
            KeyDown += HandleFormKeyDown;
        }

        private void BuildUxUpgrade()
        {
            panelControles.Width = 390;
            panelControles.BorderRadius = 16;
            menuParticulas.Text = "Autopista";
            menuGeometria.Text = "Orbitas";
            panelTituloIcono.Location = new Point(0, 14);
            panelTituloIcono.Size = new Size(76, 76);
            panelTituloIcono.BorderRadius = 18;
            panelTituloIcono.Padding = new Padding(7);
            panelTituloIcono.ShowBarsIcon = false;
            lblTitulo.Location = new Point(116, 24);
            lblSubtitulo.Location = new Point(120, 82);

            picHeaderCover = new PictureBox
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                ImageLocation = "Assets\\album-cover-neon.png",
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelTituloIcono.Controls.Add(picHeaderCover);
            picHeaderCover.BringToFront();

            panelCancion.Size = new Size(346, 138);
            panelTituloCancion.Location = new Point(113, 31);
            panelTituloCancion.Size = new Size(208, 34);
            lblCancion.Size = new Size(208, 34);
            panelIconoCancion.Location = new Point(15, 24);
            panelIconoCancion.Size = new Size(82, 82);
            panelIconoCancion.BorderRadius = 16;
            panelIconoCancion.Padding = new Padding(4);
            panelIconoCancion.ShowBarsIcon = false;
            lblAnalisis.Location = new Point(113, 69);

            picAlbumCover = new PictureBox
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                ImageLocation = "Assets\\album-cover-neon.png",
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelIconoCancion.Controls.Add(picAlbumCover);
            picAlbumCover.BringToFront();

            lblNowPlayingCaption = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(109, 240, 214),
                Location = new Point(113, 14),
                Size = new Size(208, 18),
                Text = "Ahora sonando"
            };

            lblNowPlayingDuration = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(183, 190, 210),
                Location = new Point(113, 92),
                Size = new Size(105, 19),
                Text = "00:00 / 00:00"
            };

            panelMiniWave = new Panel
            {
                BackColor = Color.Transparent,
                Location = new Point(222, 91),
                Size = new Size(99, 24)
            };

            panelCancion.Controls.Add(lblNowPlayingCaption);
            panelCancion.Controls.Add(lblNowPlayingDuration);
            panelCancion.Controls.Add(panelMiniWave);

            lblEstado.Location = new Point(28, 164);
            btnCargar.Location = new Point(25, 191);
            btnCargar.Size = new Size(340, 48);
            btnReproducir.Location = new Point(25, 254);
            btnReproducir.Size = new Size(106, 66);
            btnPausar.Location = new Point(142, 254);
            btnPausar.Size = new Size(106, 66);
            btnDetener.Location = new Point(259, 254);
            btnDetener.Size = new Size(106, 66);
            lblTiempoActual.Location = new Point(25, 331);
            lblTiempoTotal.Location = new Point(284, 331);
            trackPosicion.Location = new Point(25, 356);
            trackPosicion.Size = new Size(340, 24);
            lblVolumen.Location = new Point(25, 386);
            trackVolumen.Location = new Point(25, 411);
            trackVolumen.Size = new Size(340, 24);
            btnAnterior.Location = new Point(25, 451);
            btnAnterior.Size = new Size(94, 38);
            chkAleatorio.Location = new Point(168, 451);
            btnSiguiente.Location = new Point(271, 451);
            btnSiguiente.Size = new Size(94, 38);

            lblMood = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 87),
                Location = new Point(25, 506),
                Text = "Ambiente"
            };

            cmbMood = new NeonComboBox
            {
                Location = new Point(25, 532),
                Size = new Size(157, 36),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            cmbMood.Items.AddRange(new object[] { "Neon Tokyo", "Lava Pop", "Toxic Candy", "Ice Carnival", "Dark Galaxy" });
            cmbMood.SelectedIndex = 0;

            lblModo.Location = new Point(199, 506);
            lblModo.Text = "Visual";
            cmbModoVisualizacion.Location = new Point(199, 532);
            cmbModoVisualizacion.Size = new Size(166, 36);
            cmbModoVisualizacion.Items.Clear();
            cmbModoVisualizacion.Items.AddRange(new object[]
            {
                "Barras de espectro",
                "Ondas circulares",
                "Autopista Neon",
                "Orbitas cosmicas"
            });
            cmbModoVisualizacion.SelectedIndex = 0;

            panelControles.Controls.Add(lblMood);
            panelControles.Controls.Add(cmbMood);

            btnPresentacion = new GradientButton
            {
                BackColor = Color.Transparent,
                BorderRadius = 10,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                IconKind = ButtonIconKind.None,
                Location = new Point(25, 590),
                Size = new Size(166, 40),
                Text = "Presentar"
            };

            btnAutoMode = new GradientButton
            {
                BackColor = Color.Transparent,
                BorderRadius = 10,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                IconKind = ButtonIconKind.None,
                Location = new Point(199, 590),
                Size = new Size(166, 40),
                Text = "Auto figuras"
            };

            panelControles.Controls.Add(btnPresentacion);
            panelControles.Controls.Add(btnAutoMode);

            panelPlaylistShell = new RoundedPanel
            {
                BackColor = Color.Transparent,
                BorderRadius = 14,
                Padding = new Padding(16),
                Name = "panelPlaylistShell"
            };

            lblPlaylistTitle = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 13),
                Size = new Size(240, 24),
                Text = "Playlist"
            };

            lblPlaylistCount = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(183, 190, 210),
                Location = new Point(682, 16),
                Size = new Size(180, 20),
                Text = "0 canciones",
                TextAlign = ContentAlignment.TopRight
            };

            flowPlaylist = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                AutoScrollMargin = new Size(0, 8),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                Location = new Point(16, 45),
                Size = new Size(846, 112),
                WrapContents = false
            };
            flowPlaylist.HorizontalScroll.Enabled = false;
            flowPlaylist.HorizontalScroll.Visible = false;

            panelPlaylistShell.Controls.Add(lblPlaylistTitle);
            panelPlaylistShell.Controls.Add(lblPlaylistCount);
            panelPlaylistShell.Controls.Add(flowPlaylist);
            panelContenido.Controls.Add(panelPlaylistShell);
            panelPlaylistShell.BringToFront();

            panelPresentacionInfo = new RoundedPanel
            {
                BackColor = Color.Transparent,
                BorderRadius = 16,
                FillColor = Color.FromArgb(170, 10, 16, 28),
                BorderColor = Color.FromArgb(120, 41, 221, 218),
                Size = new Size(720, 78),
                Visible = false
            };

            lblPresentacionCancion = new Label
            {
                AutoEllipsis = true,
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(26, 12),
                Size = new Size(520, 34),
                Text = "Ninguna cancion cargada"
            };

            lblPresentacionTiempo = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(183, 240, 238),
                Location = new Point(26, 47),
                Size = new Size(220, 24),
                Text = "00:00 / 00:00"
            };

            panelPresentacionInfo.Controls.Add(lblPresentacionCancion);
            panelPresentacionInfo.Controls.Add(lblPresentacionTiempo);
            panelContenido.Controls.Add(panelPresentacionInfo);
            panelPresentacionInfo.BringToFront();

            LayoutUxUpgrade();
        }

        private void LayoutUxUpgrade()
        {
            if (panelPlaylistShell == null)
            {
                return;
            }

            if (isPresentationMode)
            {
                panelVisualizador.Location = new Point(0, 0);
                panelVisualizador.Size = panelContenido.ClientSize;
                panelPresentacionInfo.Size = new Size(Math.Min(820, Math.Max(420, panelContenido.Width - 180)), 82);
                panelPresentacionInfo.Location = new Point(
                    (panelContenido.Width - panelPresentacionInfo.Width) / 2,
                    panelContenido.Height - panelPresentacionInfo.Height - 28);
                lblPresentacionCancion.Size = new Size(panelPresentacionInfo.Width - 52, 34);
                panelPresentacionInfo.BringToFront();
                return;
            }

            var controlWidth = panelControles.Width;
            panelControles.Left = Math.Max(0, panelContenido.Width - controlWidth);
            panelControles.Height = panelContenido.Height;

            var leftWidth = Math.Max(320, panelControles.Left - 18);
            var playlistHeight = Math.Min(176, Math.Max(132, panelContenido.Height / 4));
            panelPlaylistShell.Location = new Point(0, panelContenido.Height - playlistHeight);
            panelPlaylistShell.Size = new Size(leftWidth, playlistHeight);

            panelVisualizador.Location = new Point(0, 0);
            panelVisualizador.Size = new Size(leftWidth, Math.Max(280, panelPlaylistShell.Top - 16));

            lblPlaylistCount.Location = new Point(panelPlaylistShell.Width - 200, 16);
            flowPlaylist.Size = new Size(panelPlaylistShell.Width - 40, panelPlaylistShell.Height - 58);
            flowPlaylist.HorizontalScroll.Enabled = false;
            flowPlaylist.HorizontalScroll.Visible = false;
            ResizePlaylistRows();
        }

        private Control CreateEmptyPlaylistLabel()
        {
            return new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(183, 190, 210),
                Margin = new Padding(4, 8, 4, 0),
                Size = new Size(GetPlaylistRowWidth(), 36),
                Text = "Carga una cancion para armar la lista automaticamente."
            };
        }

        private Control CreatePlaylistRow(string filePath, int index, bool isActive)
        {
            var palette = currentMood ?? moods["Neon Tokyo"];
            var row = new RoundedPanel
            {
                BackColor = Color.Transparent,
                BorderRadius = 10,
                BorderColor = isActive ? palette.Accent : Color.FromArgb(38, 58, 88),
                FillColor = isActive ? Blend(palette.Panel, palette.Accent, 0.18f) : Color.FromArgb(13, 24, 41),
                Margin = new Padding(0, 4, 12, 4),
                Size = new Size(GetPlaylistRowWidth(), 50),
                Tag = index,
                Cursor = Cursors.Hand
            };

            var number = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = isActive ? palette.Accent : Color.FromArgb(183, 190, 210),
                Location = new Point(12, 14),
                Size = new Size(38, 22),
                Text = (index + 1).ToString("00")
            };

            var title = new Label
            {
                AutoEllipsis = true,
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(58, 8),
                Size = new Size(Math.Max(160, row.Width - 250), 22),
                Text = FormatSongTitle(Path.GetFileNameWithoutExtension(filePath))
            };

            var meta = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(183, 190, 210),
                Location = new Point(58, 29),
                Size = new Size(Math.Max(160, row.Width - 250), 17),
                Text = isActive ? "Reproduciendo ahora" : Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant()
            };

            var favorite = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = favoriteSongs.Contains(filePath) ? palette.Warm : Color.FromArgb(183, 190, 210),
                Location = new Point(row.Width - 112, 10),
                Size = new Size(52, 30),
                Text = favoriteSongs.Contains(filePath) ? "Fav" : "+ Fav",
                Tag = index,
                Cursor = Cursors.Hand
            };
            favorite.FlatAppearance.BorderSize = 0;
            favorite.FlatAppearance.MouseOverBackColor = Blend(palette.Panel, palette.Warm, 0.22f);

            var remove = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 120, 145),
                Location = new Point(row.Width - 54, 10),
                Size = new Size(44, 30),
                Text = "Del",
                Tag = index,
                Cursor = Cursors.Hand
            };
            remove.FlatAppearance.BorderSize = 0;
            remove.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 60, 24, 42);

            row.Click += (sender, args) => PlaylistSongSelected?.Invoke(this, (int)row.Tag);
            title.Click += (sender, args) => PlaylistSongSelected?.Invoke(this, (int)row.Tag);
            meta.Click += (sender, args) => PlaylistSongSelected?.Invoke(this, (int)row.Tag);
            number.Click += (sender, args) => PlaylistSongSelected?.Invoke(this, (int)row.Tag);
            favorite.Click += (sender, args) => PlaylistFavoriteToggled?.Invoke(this, (int)favorite.Tag);
            remove.Click += (sender, args) => PlaylistSongRemoveRequested?.Invoke(this, (int)remove.Tag);

            row.Controls.Add(number);
            row.Controls.Add(title);
            row.Controls.Add(meta);
            row.Controls.Add(favorite);
            row.Controls.Add(remove);

            return row;
        }

        private void ResizePlaylistRows()
        {
            if (flowPlaylist == null)
            {
                return;
            }

            foreach (Control row in flowPlaylist.Controls)
            {
                row.Width = GetPlaylistRowWidth();
            }
        }

        private int GetPlaylistRowWidth()
        {
            if (flowPlaylist == null)
            {
                return 280;
            }

            return Math.Max(280, flowPlaylist.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 18);
        }

        private void TogglePresentationMode()
        {
            if (!isPresentationMode)
            {
                previousBorderStyle = FormBorderStyle;
                previousWindowState = WindowState;
                previousBounds = Bounds;
                previousPrincipalPadding = panelPrincipal.Padding;

                isPresentationMode = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                panelPrincipal.Padding = new Padding(12);
                menuPrincipal.Visible = false;
                panelEncabezado.Visible = false;
                panelControles.Visible = false;
                panelPlaylistShell.Visible = false;
                panelPresentacionInfo.Visible = true;
                btnPresentacion.IsActive = true;
                LayoutUxUpgrade();
                panelVisualizador.Focus();
                return;
            }

            isPresentationMode = false;
            FormBorderStyle = previousBorderStyle;
            WindowState = previousWindowState;
            Bounds = previousBounds;
            panelPrincipal.Padding = previousPrincipalPadding;
            menuPrincipal.Visible = true;
            panelEncabezado.Visible = true;
            panelControles.Visible = true;
            panelPlaylistShell.Visible = true;
            panelPresentacionInfo.Visible = false;
            btnPresentacion.IsActive = false;
            LayoutUxUpgrade();
        }

        private void ToggleAutoMode()
        {
            btnAutoMode.IsActive = !btnAutoMode.IsActive;
            btnAutoMode.Text = btnAutoMode.IsActive ? "Auto figuras ON" : "Auto figuras";
            AutoModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleFormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && isPresentationMode)
            {
                TogglePresentationMode();
                e.Handled = true;
            }
        }

        private void HandleTransitionTimerTick(object sender, EventArgs e)
        {
            transitionFrame--;
            if (transitionFrame <= 0)
            {
                transitionFrame = 0;
                transitionTimer.Stop();
            }

            panelVisualizador.Invalidate();
        }

        private void ApplyMood(string moodName)
        {
            if (!moods.TryGetValue(moodName, out currentMood))
            {
                currentMood = moods["Neon Tokyo"];
            }

            BackColor = currentMood.Background;
            panelPrincipal.BackColor = currentMood.Background;
            menuPrincipal.BackColor = currentMood.Background;
            panelMarca.FillColor = Blend(currentMood.Panel, Color.White, 0.03f);
            panelMarca.BorderColor = Blend(currentMood.Panel, currentMood.Accent, 0.38f);
            panelControles.FillColor = currentMood.Panel;
            panelControles.BorderColor = Blend(currentMood.Panel, currentMood.Accent, 0.55f);
            panelCancion.FillColor = Blend(currentMood.Panel, currentMood.Secondary, 0.14f);
            panelCancion.BorderColor = Blend(currentMood.Panel, currentMood.Accent, 0.45f);
            panelIconoCancion.BorderColor = currentMood.Secondary;
            panelIconoCancion.FillColor = Blend(currentMood.Panel, currentMood.Secondary, 0.22f);
            panelTituloIcono.BorderColor = currentMood.Secondary;
            panelTituloIcono.FillColor = Blend(currentMood.Panel, currentMood.Secondary, 0.22f);
            panelPlaylistShell.FillColor = Blend(currentMood.Panel, Color.Black, 0.10f);
            panelPlaylistShell.BorderColor = Blend(currentMood.Panel, currentMood.Accent, 0.50f);
            panelVisualizador.BackColor = Blend(currentMood.Background, Color.Black, 0.18f);
            lblTitulo.ForeColor = Color.White;
            lblSubtitulo.ForeColor = Blend(Color.White, currentMood.Accent, 0.32f);
            lblAutores.ForeColor = Color.White;
            lblMateria.ForeColor = Blend(Color.White, currentMood.Secondary, 0.25f);
            lblVolumen.ForeColor = currentMood.Accent;
            lblNrc.ForeColor = currentMood.Accent;
            lblNowPlayingCaption.ForeColor = currentMood.Accent;
            lblEstado.ForeColor = currentMood.Warm;
            lblMood.ForeColor = currentMood.Warm;
            lblModo.ForeColor = currentMood.Warm;
            lblPlaylistTitle.ForeColor = Color.White;
            cmbMood.AccentColor = currentMood.Accent;
            cmbMood.BorderColor = currentMood.Accent;
            cmbMood.BackColor = Blend(currentMood.Panel, Color.Black, 0.20f);
            cmbModoVisualizacion.AccentColor = currentMood.Accent;
            cmbModoVisualizacion.BorderColor = currentMood.Accent;
            cmbModoVisualizacion.BackColor = Blend(currentMood.Panel, Color.Black, 0.20f);
            trackPosicion.FillColor = currentMood.Accent;
            trackPosicion.ThumbColor = currentMood.Accent;
            trackPosicion.TrackColor = Blend(currentMood.Panel, currentMood.Secondary, 0.20f);
            trackVolumen.FillColor = currentMood.Accent;
            trackVolumen.ThumbColor = currentMood.Accent;
            trackVolumen.TrackColor = Blend(currentMood.Panel, currentMood.Secondary, 0.20f);
            btnCargar.StartColor = Blend(currentMood.Accent, Color.White, 0.16f);
            btnCargar.EndColor = Blend(currentMood.Secondary, Color.Black, 0.35f);
            btnCargar.BorderColor = currentMood.Accent;
            btnReproducir.StartColor = Blend(currentMood.Accent, Color.Black, 0.45f);
            btnReproducir.EndColor = Blend(currentMood.Panel, currentMood.Accent, 0.25f);
            btnReproducir.BorderColor = currentMood.Accent;
            btnReproducir.IconColor = currentMood.Accent;
            btnPausar.StartColor = Blend(currentMood.Warm, Color.Black, 0.45f);
            btnPausar.EndColor = Blend(currentMood.Panel, currentMood.Warm, 0.20f);
            btnPausar.BorderColor = currentMood.Warm;
            btnPausar.IconColor = currentMood.Warm;
            btnDetener.StartColor = Blend(currentMood.Secondary, Color.Black, 0.45f);
            btnDetener.EndColor = Blend(currentMood.Panel, currentMood.Secondary, 0.24f);
            btnDetener.BorderColor = currentMood.Secondary;
            btnDetener.IconColor = currentMood.Secondary;
            btnAnterior.StartColor = Blend(currentMood.Panel, currentMood.Secondary, 0.16f);
            btnAnterior.EndColor = Blend(currentMood.Panel, Color.Black, 0.25f);
            btnAnterior.BorderColor = Blend(currentMood.Secondary, currentMood.Accent, 0.35f);
            btnSiguiente.StartColor = btnAnterior.StartColor;
            btnSiguiente.EndColor = btnAnterior.EndColor;
            btnSiguiente.BorderColor = btnAnterior.BorderColor;
            btnPresentacion.StartColor = Blend(currentMood.Panel, currentMood.Warm, 0.30f);
            btnPresentacion.EndColor = Blend(currentMood.Panel, Color.Black, 0.18f);
            btnPresentacion.BorderColor = currentMood.Warm;
            btnAutoMode.StartColor = Blend(currentMood.Panel, currentMood.Accent, 0.30f);
            btnAutoMode.EndColor = Blend(currentMood.Panel, Color.Black, 0.18f);
            btnAutoMode.BorderColor = currentMood.Accent;
            chkAleatorio.AccentColor = currentMood.Accent;
            chkAleatorio.SecondaryColor = currentMood.Secondary;
            panelPresentacionInfo.FillColor = Blend(Color.FromArgb(10, 16, 28), currentMood.Panel, 0.50f);
            panelPresentacionInfo.BorderColor = Blend(currentMood.Accent, currentMood.Secondary, 0.35f);
            lblPresentacionTiempo.ForeColor = currentMood.Accent;
            RefreshPlaylistTheme();
            panelPrincipal.Invalidate(true);
            panelVisualizador.Invalidate();
        }

        private void RefreshPlaylistTheme()
        {
            if (flowPlaylist == null)
            {
                return;
            }

            foreach (Control row in flowPlaylist.Controls)
            {
                row.Invalidate(true);
            }
        }

        private void HandleVisualizerPanelPaint(object sender, PaintEventArgs args)
        {
            VisualizerPaintRequested?.Invoke(this, args);
            DrawPulseFrame(args.Graphics, panelVisualizador.ClientRectangle);
            DrawBeatIndicator(args.Graphics, panelVisualizador.ClientRectangle);
            DrawTransitionFlash(args.Graphics, panelVisualizador.ClientRectangle);
        }

        private void HandleMiniWavePaint(object sender, PaintEventArgs args)
        {
            var palette = currentMood ?? moods["Neon Tokyo"];
            args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            args.Graphics.Clear(Color.Transparent);

            var bars = 18;
            var slot = panelMiniWave.Width / (float)bars;
            for (var i = 0; i < bars; i++)
            {
                var wave = 0.35f + 0.65f * (float)Math.Sin(Environment.TickCount * 0.004 + i * 0.7);
                var level = Math.Max(0.12f, lastAudioFrame.Bass * 0.65f + lastAudioFrame.Intensity * 0.35f);
                var height = 4 + wave * level * (panelMiniWave.Height - 6);
                var x = i * slot + 1;
                var y = (panelMiniWave.Height - height) / 2f;
                using (var pen = new Pen(i % 2 == 0 ? palette.Accent : palette.Secondary, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    args.Graphics.DrawLine(pen, x, y, x, y + height);
                }
            }
        }

        private void DrawPulseFrame(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var palette = currentMood ?? moods["Neon Tokyo"];
            var bass = Math.Max(lastAudioFrame.Bass, lastAudioFrame.Pulse);
            var alpha = Math.Min(210, 65 + (int)(bass * 145));
            var inset = 2 + (int)(bass * 8);
            var rect = Rectangle.Inflate(bounds, -inset, -inset);

            using (var pen = new Pen(Color.FromArgb(alpha, palette.Accent), 1.4f + bass * 3.2f))
            using (var glowPen = new Pen(Color.FromArgb(Math.Max(30, alpha / 3), palette.Secondary), 8 + bass * 16))
            {
                graphics.DrawRectangle(glowPen, rect);
                graphics.DrawRectangle(pen, rect);
            }
        }

        private void DrawBeatIndicator(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var palette = currentMood ?? moods["Neon Tokyo"];
            var beat = Math.Max(lastAudioFrame.Bass, lastAudioFrame.Pulse);
            if (beat < 0.10f)
            {
                return;
            }

            var radius = 22 + beat * 42;
            var center = new PointF(bounds.Right - 76, bounds.Bottom - 74);
            var alpha = Math.Min(220, 45 + (int)(beat * 160));

            using (var glowPen = new Pen(Color.FromArgb(alpha / 3, palette.Secondary), 12 + beat * 12))
            using (var pen = new Pen(Color.FromArgb(alpha, palette.Accent), 2.2f + beat * 3f))
            using (var fillBrush = new SolidBrush(Color.FromArgb(Math.Min(60, alpha / 4), palette.Accent)))
            {
                graphics.FillEllipse(fillBrush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
                graphics.DrawEllipse(glowPen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
                graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
            }

            using (var pen = new Pen(Color.FromArgb(alpha, palette.Warm), 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                graphics.DrawLine(pen, center.X - 16, center.Y, center.X - 5, center.Y + beat * 12);
                graphics.DrawLine(pen, center.X - 5, center.Y + beat * 12, center.X + 7, center.Y - beat * 18);
                graphics.DrawLine(pen, center.X + 7, center.Y - beat * 18, center.X + 18, center.Y);
            }
        }

        private void DrawTransitionFlash(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            if (transitionFrame <= 0)
            {
                return;
            }

            var palette = currentMood ?? moods["Neon Tokyo"];
            var progress = transitionFrame / 10f;
            var alpha = Math.Min(180, (int)(progress * 170));
            var inset = (int)(8 + (1 - progress) * 30);
            var rect = Rectangle.Inflate(bounds, -inset, -inset);
            var sweepWidth = Math.Max(120, bounds.Width / 5);
            var sweepX = bounds.Left + (int)((1 - progress) * (bounds.Width + sweepWidth)) - sweepWidth;
            var sweepRect = new Rectangle(sweepX, bounds.Top + 6, sweepWidth, bounds.Height - 12);

            using (var borderPen = new Pen(Color.FromArgb(alpha, palette.Accent), 2.4f + progress * 3f))
            using (var warmPen = new Pen(Color.FromArgb(alpha / 2, palette.Warm), 1.6f + progress * 2f))
            {
                graphics.DrawRectangle(borderPen, rect);
                graphics.DrawRectangle(warmPen, Rectangle.Inflate(rect, -10, -10));
            }

            using (var brush = new LinearGradientBrush(
                sweepRect,
                Color.FromArgb(0, palette.Accent),
                Color.FromArgb(Math.Min(70, alpha / 3), palette.Accent),
                LinearGradientMode.Horizontal))
            {
                graphics.FillRectangle(brush, sweepRect);
            }
        }

        private void HandlePresentationInfoPaint(object sender, PaintEventArgs args)
        {
            var palette = currentMood ?? moods["Neon Tokyo"];
            var beat = Math.Max(lastAudioFrame.Bass, lastAudioFrame.Pulse);
            args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(Color.FromArgb(90 + (int)(beat * 120), palette.Accent), 2f + beat * 3f))
            {
                args.Graphics.DrawLine(pen, 26, panelPresentacionInfo.Height - 8, panelPresentacionInfo.Width - 26, panelPresentacionInfo.Height - 8);
            }
        }

        private static Color Blend(Color first, Color second, float amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                (int)(first.R + (second.R - first.R) * amount),
                (int)(first.G + (second.G - first.G) * amount),
                (int)(first.B + (second.B - first.B) * amount));
        }

        private void InitializeSongTitleMarquee()
        {
            songTitleHomeLeft = lblCancion.Left;
            songTitleViewportWidth = lblCancion.Width;
            lblCancion.AutoEllipsis = false;
            songTitleTimer.Tick += HandleSongTitleTimerTick;
            panelTituloCancion.Resize += (sender, args) =>
            {
                songTitleViewportWidth = panelTituloCancion.Width;
                ResetSongTitleMarquee();
            };
            ResetSongTitleMarquee();
        }

        private void ResetSongTitleMarquee()
        {
            songTitleTimer.Stop();
            lblCancion.Left = songTitleHomeLeft;

            var measuredTitle = TextRenderer.MeasureText(
                lblCancion.Text,
                lblCancion.Font,
                new Size(int.MaxValue, lblCancion.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            lblCancion.Width = Math.Max(songTitleViewportWidth, measuredTitle.Width + 8);
            songTitleMinimumLeft = songTitleHomeLeft - Math.Max(0, lblCancion.Width - songTitleViewportWidth);
            songTitlePauseTicks = SongTitleStartPauseTicks;

            if (lblCancion.Width > songTitleViewportWidth)
            {
                songTitleTimer.Start();
            }
        }

        private void HandleSongTitleTimerTick(object sender, EventArgs e)
        {
            if (songTitlePauseTicks > 0)
            {
                songTitlePauseTicks--;
                return;
            }

            if (lblCancion.Left > songTitleMinimumLeft)
            {
                lblCancion.Left -= SongTitleStep;
                return;
            }

            lblCancion.Left = songTitleHomeLeft;
            songTitlePauseTicks = SongTitleStartPauseTicks + SongTitleEndPauseTicks;
        }

        private void SelectVisualizationMode(string mode)
        {
            SetVisualizationMode(mode);
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

        private class MoodPalette
        {
            public MoodPalette(Color background, Color panel, Color accent, Color secondary, Color warm)
            {
                Background = background;
                Panel = panel;
                Accent = accent;
                Secondary = secondary;
                Warm = warm;
            }

            public Color Background { get; }

            public Color Panel { get; }

            public Color Accent { get; }

            public Color Secondary { get; }

            public Color Warm { get; }
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

