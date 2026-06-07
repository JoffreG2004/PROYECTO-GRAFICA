namespace REPRODUCTOR_MUSICAL
{
    partial class FrmHome
    {
        /// <summary>
        /// Variable del disenador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se esten usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Codigo generado por el Disenador de Windows Forms

        /// <summary>
        /// Metodo necesario para admitir el Disenador. No se puede modificar
        /// el contenido de este metodo con el editor de codigo.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuPrincipal = new System.Windows.Forms.MenuStrip();
            this.menuArchivo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCargarCancion = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSalir = new System.Windows.Forms.ToolStripMenuItem();
            this.menuVisualizacion = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBarras = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOndas = new System.Windows.Forms.ToolStripMenuItem();
            this.menuParticulas = new System.Windows.Forms.ToolStripMenuItem();
            this.menuGeometria = new System.Windows.Forms.ToolStripMenuItem();
            this.panelPrincipal = new System.Windows.Forms.Panel();
            this.panelContenido = new System.Windows.Forms.Panel();
            this.panelVisualizador = new REPRODUCTOR_MUSICAL.Views.DoubleBufferedPanel();
            this.lblVisualizador = new System.Windows.Forms.Label();
            this.panelControles = new REPRODUCTOR_MUSICAL.Views.RoundedPanel();
            this.panelCancion = new REPRODUCTOR_MUSICAL.Views.RoundedPanel();
            this.panelIconoCancion = new REPRODUCTOR_MUSICAL.Views.MusicIconPanel();
            this.lblIconoCancion = new System.Windows.Forms.Label();
            this.lblCancion = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.lblAnalisis = new System.Windows.Forms.Label();
            this.btnCargar = new REPRODUCTOR_MUSICAL.Views.GradientButton();
            this.btnReproducir = new REPRODUCTOR_MUSICAL.Views.GradientButton();
            this.btnPausar = new REPRODUCTOR_MUSICAL.Views.GradientButton();
            this.btnDetener = new REPRODUCTOR_MUSICAL.Views.GradientButton();
            this.lblTiempoActual = new System.Windows.Forms.Label();
            this.trackPosicion = new REPRODUCTOR_MUSICAL.Views.NeonSlider();
            this.lblTiempoTotal = new System.Windows.Forms.Label();
            this.lblVolumen = new System.Windows.Forms.Label();
            this.trackVolumen = new REPRODUCTOR_MUSICAL.Views.NeonSlider();
            this.lblModo = new System.Windows.Forms.Label();
            this.cmbModoVisualizacion = new REPRODUCTOR_MUSICAL.Views.NeonComboBox();
            this.panelEncabezado = new System.Windows.Forms.Panel();
            this.panelTituloIcono = new REPRODUCTOR_MUSICAL.Views.MusicIconPanel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.panelMarca = new REPRODUCTOR_MUSICAL.Views.RoundedPanel();
            this.lblAutores = new System.Windows.Forms.Label();
            this.lblMateria = new System.Windows.Forms.Label();
            this.lblNrc = new System.Windows.Forms.Label();
            this.picLogoEspe = new System.Windows.Forms.PictureBox();
            this.menuPrincipal.SuspendLayout();
            this.panelPrincipal.SuspendLayout();
            this.panelContenido.SuspendLayout();
            this.panelVisualizador.SuspendLayout();
            this.panelControles.SuspendLayout();
            this.panelCancion.SuspendLayout();
            this.panelIconoCancion.SuspendLayout();
            this.panelEncabezado.SuspendLayout();
            this.panelMarca.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogoEspe)).BeginInit();
            this.SuspendLayout();
            // 
            // menuPrincipal
            // 
            this.menuPrincipal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(13)))), ((int)(((byte)(25)))));
            this.menuPrincipal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.menuPrincipal.ForeColor = System.Drawing.Color.White;
            this.menuPrincipal.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuArchivo,
            this.menuVisualizacion});
            this.menuPrincipal.Location = new System.Drawing.Point(0, 0);
            this.menuPrincipal.Name = "menuPrincipal";
            this.menuPrincipal.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.menuPrincipal.Size = new System.Drawing.Size(1280, 33);
            this.menuPrincipal.TabIndex = 0;
            this.menuPrincipal.Text = "menuPrincipal";
            // 
            // menuArchivo
            // 
            this.menuArchivo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCargarCancion,
            this.menuSalir});
            this.menuArchivo.ForeColor = System.Drawing.Color.White;
            this.menuArchivo.Name = "menuArchivo";
            this.menuArchivo.Size = new System.Drawing.Size(85, 27);
            this.menuArchivo.Text = "Archivo";
            // 
            // menuCargarCancion
            // 
            this.menuCargarCancion.Name = "menuCargarCancion";
            this.menuCargarCancion.Size = new System.Drawing.Size(224, 28);
            this.menuCargarCancion.Text = "Cargar cancion";
            // 
            // menuSalir
            // 
            this.menuSalir.Name = "menuSalir";
            this.menuSalir.Size = new System.Drawing.Size(224, 28);
            this.menuSalir.Text = "Salir";
            // 
            // menuVisualizacion
            // 
            this.menuVisualizacion.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuBarras,
            this.menuOndas,
            this.menuParticulas,
            this.menuGeometria});
            this.menuVisualizacion.ForeColor = System.Drawing.Color.White;
            this.menuVisualizacion.Name = "menuVisualizacion";
            this.menuVisualizacion.Size = new System.Drawing.Size(126, 27);
            this.menuVisualizacion.Text = "Visualizacion";
            // 
            // menuBarras
            // 
            this.menuBarras.Name = "menuBarras";
            this.menuBarras.Size = new System.Drawing.Size(224, 28);
            this.menuBarras.Text = "Barras";
            // 
            // menuOndas
            // 
            this.menuOndas.Name = "menuOndas";
            this.menuOndas.Size = new System.Drawing.Size(224, 28);
            this.menuOndas.Text = "Ondas";
            // 
            // menuParticulas
            // 
            this.menuParticulas.Name = "menuParticulas";
            this.menuParticulas.Size = new System.Drawing.Size(224, 28);
            this.menuParticulas.Text = "Particulas";
            // 
            // menuGeometria
            // 
            this.menuGeometria.Name = "menuGeometria";
            this.menuGeometria.Size = new System.Drawing.Size(224, 28);
            this.menuGeometria.Text = "Geometria";
            // 
            // panelPrincipal
            // 
            this.panelPrincipal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(13)))), ((int)(((byte)(25)))));
            this.panelPrincipal.Controls.Add(this.panelContenido);
            this.panelPrincipal.Controls.Add(this.panelEncabezado);
            this.panelPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPrincipal.Location = new System.Drawing.Point(0, 33);
            this.panelPrincipal.Name = "panelPrincipal";
            this.panelPrincipal.Padding = new System.Windows.Forms.Padding(24);
            this.panelPrincipal.Size = new System.Drawing.Size(1280, 687);
            this.panelPrincipal.TabIndex = 1;
            // 
            // panelContenido
            // 
            this.panelContenido.Controls.Add(this.panelVisualizador);
            this.panelContenido.Controls.Add(this.panelControles);
            this.panelContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContenido.Location = new System.Drawing.Point(24, 148);
            this.panelContenido.Name = "panelContenido";
            this.panelContenido.Size = new System.Drawing.Size(1232, 515);
            this.panelContenido.TabIndex = 1;
            // 
            // panelVisualizador
            // 
            this.panelVisualizador.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelVisualizador.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
            this.panelVisualizador.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelVisualizador.Controls.Add(this.lblVisualizador);
            this.panelVisualizador.Location = new System.Drawing.Point(0, 0);
            this.panelVisualizador.Name = "panelVisualizador";
            this.panelVisualizador.Size = new System.Drawing.Size(845, 515);
            this.panelVisualizador.TabIndex = 0;
            // 
            // lblVisualizador
            // 
            this.lblVisualizador.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblVisualizador.AutoSize = true;
            this.lblVisualizador.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold);
            this.lblVisualizador.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(240)))), ((int)(((byte)(214)))));
            this.lblVisualizador.Location = new System.Drawing.Point(259, 233);
            this.lblVisualizador.Name = "lblVisualizador";
            this.lblVisualizador.Size = new System.Drawing.Size(306, 41);
            this.lblVisualizador.TabIndex = 0;
            this.lblVisualizador.Text = "ESCENA GRAFICA 2D";
            // 
            // panelControles
            // 
            this.panelControles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControles.BackColor = System.Drawing.Color.Transparent;
            this.panelControles.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(58)))), ((int)(((byte)(88)))));
            this.panelControles.Controls.Add(this.panelCancion);
            this.panelControles.Controls.Add(this.lblEstado);
            this.panelControles.Controls.Add(this.btnCargar);
            this.panelControles.Controls.Add(this.btnReproducir);
            this.panelControles.Controls.Add(this.btnPausar);
            this.panelControles.Controls.Add(this.btnDetener);
            this.panelControles.Controls.Add(this.lblTiempoActual);
            this.panelControles.Controls.Add(this.trackPosicion);
            this.panelControles.Controls.Add(this.lblTiempoTotal);
            this.panelControles.Controls.Add(this.lblVolumen);
            this.panelControles.Controls.Add(this.trackVolumen);
            this.panelControles.Controls.Add(this.lblModo);
            this.panelControles.Controls.Add(this.cmbModoVisualizacion);
            this.panelControles.Location = new System.Drawing.Point(869, 0);
            this.panelControles.Name = "panelControles";
            this.panelControles.Padding = new System.Windows.Forms.Padding(22);
            this.panelControles.Size = new System.Drawing.Size(363, 515);
            this.panelControles.TabIndex = 1;
            this.panelControles.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(22)))), ((int)(((byte)(39)))));
            // 
            // panelCancion
            // 
            this.panelCancion.BackColor = System.Drawing.Color.Transparent;
            this.panelCancion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(61)))), ((int)(((byte)(96)))));
            this.panelCancion.Controls.Add(this.lblCancion);
            this.panelCancion.Controls.Add(this.lblAnalisis);
            this.panelCancion.Controls.Add(this.panelIconoCancion);
            this.panelCancion.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(27)))), ((int)(((byte)(48)))));
            this.panelCancion.Location = new System.Drawing.Point(25, 23);
            this.panelCancion.Name = "panelCancion";
            this.panelCancion.Size = new System.Drawing.Size(313, 105);
            this.panelCancion.TabIndex = 14;
            // 
            // panelIconoCancion
            // 
            this.panelIconoCancion.BackColor = System.Drawing.Color.Transparent;
            this.panelIconoCancion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(64)))), ((int)(((byte)(210)))));
            this.panelIconoCancion.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(26)))), ((int)(((byte)(77)))));
            this.panelIconoCancion.Location = new System.Drawing.Point(15, 17);
            this.panelIconoCancion.Name = "panelIconoCancion";
            this.panelIconoCancion.Size = new System.Drawing.Size(70, 70);
            this.panelIconoCancion.TabIndex = 2;
            // 
            // lblIconoCancion
            // 
            this.lblIconoCancion.Font = new System.Drawing.Font("Segoe UI Semibold", 28F, System.Drawing.FontStyle.Bold);
            this.lblIconoCancion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(95)))), ((int)(((byte)(170)))));
            this.lblIconoCancion.Location = new System.Drawing.Point(0, 1);
            this.lblIconoCancion.Name = "lblIconoCancion";
            this.lblIconoCancion.Size = new System.Drawing.Size(70, 66);
            this.lblIconoCancion.TabIndex = 0;
            this.lblIconoCancion.Text = "";
            this.lblIconoCancion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCancion
            // 
            this.lblCancion.AutoEllipsis = true;
            this.lblCancion.Font = new System.Drawing.Font("Segoe UI Semibold", 11.5F, System.Drawing.FontStyle.Bold);
            this.lblCancion.ForeColor = System.Drawing.Color.White;
            this.lblCancion.Location = new System.Drawing.Point(99, 19);
            this.lblCancion.Name = "lblCancion";
            this.lblCancion.Size = new System.Drawing.Size(198, 43);
            this.lblCancion.TabIndex = 0;
            this.lblCancion.Text = "Ninguna cancion cargada";
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(200)))), ((int)(((byte)(87)))));
            this.lblEstado.Location = new System.Drawing.Point(30, 137);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(80, 23);
            this.lblEstado.TabIndex = 1;
            this.lblEstado.Text = "Detenido";
            // 
            // lblAnalisis
            // 
            this.lblAnalisis.AutoSize = true;
            this.lblAnalisis.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAnalisis.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(190)))), ((int)(((byte)(210)))));
            this.lblAnalisis.Location = new System.Drawing.Point(100, 66);
            this.lblAnalisis.Name = "lblAnalisis";
            this.lblAnalisis.Size = new System.Drawing.Size(167, 20);
            this.lblAnalisis.TabIndex = 13;
            this.lblAnalisis.Text = "Analisis: esperando audio";
            // 
            // btnCargar
            // 
            this.btnCargar.BackColor = System.Drawing.Color.Transparent;
            this.btnCargar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(229)))), ((int)(((byte)(239)))));
            this.btnCargar.BorderRadius = 12;
            this.btnCargar.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(117)))), ((int)(((byte)(185)))));
            this.btnCargar.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnCargar.ForeColor = System.Drawing.Color.White;
            this.btnCargar.IconColor = System.Drawing.Color.White;
            this.btnCargar.IconKind = REPRODUCTOR_MUSICAL.Views.ButtonIconKind.Upload;
            this.btnCargar.Location = new System.Drawing.Point(30, 166);
            this.btnCargar.Name = "btnCargar";
            this.btnCargar.Size = new System.Drawing.Size(308, 54);
            this.btnCargar.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(229)))), ((int)(((byte)(220)))));
            this.btnCargar.TabIndex = 2;
            this.btnCargar.Text = "Cargar cancion";
            this.btnCargar.UseVisualStyleBackColor = false;
            // 
            // btnReproducir
            // 
            this.btnReproducir.BackColor = System.Drawing.Color.Transparent;
            this.btnReproducir.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(210)))), ((int)(((byte)(170)))));
            this.btnReproducir.BorderRadius = 10;
            this.btnReproducir.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(53)))), ((int)(((byte)(66)))));
            this.btnReproducir.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnReproducir.ForeColor = System.Drawing.Color.White;
            this.btnReproducir.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.btnReproducir.IconKind = REPRODUCTOR_MUSICAL.Views.ButtonIconKind.Play;
            this.btnReproducir.Location = new System.Drawing.Point(30, 238);
            this.btnReproducir.Name = "btnReproducir";
            this.btnReproducir.Size = new System.Drawing.Size(98, 68);
            this.btnReproducir.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(105)))), ((int)(((byte)(94)))));
            this.btnReproducir.TabIndex = 3;
            this.btnReproducir.Text = "Play";
            this.btnReproducir.UseVisualStyleBackColor = false;
            // 
            // btnPausar
            // 
            this.btnPausar.BackColor = System.Drawing.Color.Transparent;
            this.btnPausar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(160)))), ((int)(((byte)(65)))));
            this.btnPausar.BorderRadius = 10;
            this.btnPausar.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(43)))), ((int)(((byte)(30)))));
            this.btnPausar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnPausar.ForeColor = System.Drawing.Color.White;
            this.btnPausar.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(190)))), ((int)(((byte)(72)))));
            this.btnPausar.IconKind = REPRODUCTOR_MUSICAL.Views.ButtonIconKind.Pause;
            this.btnPausar.Location = new System.Drawing.Point(135, 238);
            this.btnPausar.Name = "btnPausar";
            this.btnPausar.Size = new System.Drawing.Size(98, 68);
            this.btnPausar.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(80)))), ((int)(((byte)(34)))));
            this.btnPausar.TabIndex = 4;
            this.btnPausar.Text = "Pausa";
            this.btnPausar.UseVisualStyleBackColor = false;
            // 
            // btnDetener
            // 
            this.btnDetener.BackColor = System.Drawing.Color.Transparent;
            this.btnDetener.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(73)))), ((int)(((byte)(103)))));
            this.btnDetener.BorderRadius = 10;
            this.btnDetener.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(27)))), ((int)(((byte)(45)))));
            this.btnDetener.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDetener.ForeColor = System.Drawing.Color.White;
            this.btnDetener.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(83)))), ((int)(((byte)(111)))));
            this.btnDetener.IconKind = REPRODUCTOR_MUSICAL.Views.ButtonIconKind.Stop;
            this.btnDetener.Location = new System.Drawing.Point(240, 238);
            this.btnDetener.Name = "btnDetener";
            this.btnDetener.Size = new System.Drawing.Size(98, 68);
            this.btnDetener.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(37)))), ((int)(((byte)(61)))));
            this.btnDetener.TabIndex = 5;
            this.btnDetener.Text = "Stop";
            this.btnDetener.UseVisualStyleBackColor = false;
            // 
            // lblTiempoActual
            // 
            this.lblTiempoActual.AutoSize = true;
            this.lblTiempoActual.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblTiempoActual.ForeColor = System.Drawing.Color.White;
            this.lblTiempoActual.Location = new System.Drawing.Point(27, 317);
            this.lblTiempoActual.Name = "lblTiempoActual";
            this.lblTiempoActual.Size = new System.Drawing.Size(50, 23);
            this.lblTiempoActual.TabIndex = 6;
            this.lblTiempoActual.Text = "00:00";
            // 
            // trackPosicion
            // 
            this.trackPosicion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(22)))), ((int)(((byte)(39)))));
            this.trackPosicion.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.trackPosicion.Location = new System.Drawing.Point(30, 342);
            this.trackPosicion.Maximum = 1;
            this.trackPosicion.Name = "trackPosicion";
            this.trackPosicion.Size = new System.Drawing.Size(308, 34);
            this.trackPosicion.TabIndex = 7;
            this.trackPosicion.ThumbColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.trackPosicion.TickFrequency = 0;
            this.trackPosicion.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackPosicion.TrackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(77)))));
            // 
            // lblTiempoTotal
            // 
            this.lblTiempoTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblTiempoTotal.ForeColor = System.Drawing.Color.White;
            this.lblTiempoTotal.Location = new System.Drawing.Point(257, 317);
            this.lblTiempoTotal.Name = "lblTiempoTotal";
            this.lblTiempoTotal.Size = new System.Drawing.Size(81, 23);
            this.lblTiempoTotal.TabIndex = 8;
            this.lblTiempoTotal.Text = "00:00";
            this.lblTiempoTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblVolumen
            // 
            this.lblVolumen.AutoSize = true;
            this.lblVolumen.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblVolumen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(240)))), ((int)(((byte)(214)))));
            this.lblVolumen.Location = new System.Drawing.Point(27, 388);
            this.lblVolumen.Name = "lblVolumen";
            this.lblVolumen.Size = new System.Drawing.Size(101, 23);
            this.lblVolumen.TabIndex = 9;
            this.lblVolumen.Text = "Volumen 70";
            // 
            // trackVolumen
            // 
            this.trackVolumen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(22)))), ((int)(((byte)(39)))));
            this.trackVolumen.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.trackVolumen.Location = new System.Drawing.Point(30, 419);
            this.trackVolumen.Maximum = 100;
            this.trackVolumen.Name = "trackVolumen";
            this.trackVolumen.Size = new System.Drawing.Size(308, 34);
            this.trackVolumen.TabIndex = 10;
            this.trackVolumen.ThumbColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.trackVolumen.TickFrequency = 10;
            this.trackVolumen.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackVolumen.TrackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(77)))));
            this.trackVolumen.Value = 70;
            // 
            // lblModo
            // 
            this.lblModo.AutoSize = true;
            this.lblModo.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblModo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(200)))), ((int)(((byte)(87)))));
            this.lblModo.Location = new System.Drawing.Point(27, 477);
            this.lblModo.Name = "lblModo";
            this.lblModo.Size = new System.Drawing.Size(151, 23);
            this.lblModo.TabIndex = 11;
            this.lblModo.Text = "Modo visualizador";
            // 
            // cmbModoVisualizacion
            // 
            this.cmbModoVisualizacion.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.cmbModoVisualizacion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(18)))), ((int)(((byte)(32)))));
            this.cmbModoVisualizacion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(221)))), ((int)(((byte)(218)))));
            this.cmbModoVisualizacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModoVisualizacion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbModoVisualizacion.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.cmbModoVisualizacion.ForeColor = System.Drawing.Color.White;
            this.cmbModoVisualizacion.FormattingEnabled = true;
            this.cmbModoVisualizacion.ItemHeight = 30;
            this.cmbModoVisualizacion.Items.AddRange(new object[] {
            "Barras de espectro",
            "Ondas circulares",
            "Particulas ritmicas",
            "Escena geometrica"});
            this.cmbModoVisualizacion.Location = new System.Drawing.Point(30, 502);
            this.cmbModoVisualizacion.Name = "cmbModoVisualizacion";
            this.cmbModoVisualizacion.SelectedIndex = 3;
            this.cmbModoVisualizacion.Size = new System.Drawing.Size(308, 36);
            this.cmbModoVisualizacion.TabIndex = 12;
            // 
            // panelEncabezado
            // 
            this.panelEncabezado.Controls.Add(this.panelMarca);
            this.panelEncabezado.Controls.Add(this.panelTituloIcono);
            this.panelEncabezado.Controls.Add(this.lblTitulo);
            this.panelEncabezado.Controls.Add(this.lblSubtitulo);
            this.panelEncabezado.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelEncabezado.Location = new System.Drawing.Point(24, 24);
            this.panelEncabezado.Name = "panelEncabezado";
            this.panelEncabezado.Size = new System.Drawing.Size(1232, 124);
            this.panelEncabezado.TabIndex = 0;
            // 
            // panelTituloIcono
            // 
            this.panelTituloIcono.BackColor = System.Drawing.Color.Transparent;
            this.panelTituloIcono.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(72)))), ((int)(((byte)(220)))));
            this.panelTituloIcono.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(26)))), ((int)(((byte)(77)))));
            this.panelTituloIcono.Location = new System.Drawing.Point(0, 30);
            this.panelTituloIcono.Name = "panelTituloIcono";
            this.panelTituloIcono.Size = new System.Drawing.Size(70, 70);
            this.panelTituloIcono.TabIndex = 4;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 26F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(91, 22);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(441, 60);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Reproductor Musical";
            // 
            // lblSubtitulo
            // 
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(190)))), ((int)(((byte)(210)))));
            this.lblSubtitulo.Location = new System.Drawing.Point(95, 78);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(456, 25);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Animaciones sincronizadas con audio en tiempo real";
            // 
            // panelMarca
            // 
            this.panelMarca.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMarca.BackColor = System.Drawing.Color.Transparent;
            this.panelMarca.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(62)))), ((int)(((byte)(92)))));
            this.panelMarca.Controls.Add(this.lblAutores);
            this.panelMarca.Controls.Add(this.lblMateria);
            this.panelMarca.Controls.Add(this.lblNrc);
            this.panelMarca.Controls.Add(this.picLogoEspe);
            this.panelMarca.Location = new System.Drawing.Point(618, 0);
            this.panelMarca.Name = "panelMarca";
            this.panelMarca.Size = new System.Drawing.Size(614, 104);
            this.panelMarca.TabIndex = 2;
            this.panelMarca.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(24)))), ((int)(((byte)(41)))));
            // 
            // lblAutores
            // 
            this.lblAutores.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblAutores.ForeColor = System.Drawing.Color.White;
            this.lblAutores.Location = new System.Drawing.Point(17, 13);
            this.lblAutores.Name = "lblAutores";
            this.lblAutores.Size = new System.Drawing.Size(376, 24);
            this.lblAutores.TabIndex = 0;
            this.lblAutores.Text = "AUTORES: GOMEZ JOFFRE, BLACIO JULIO";
            // 
            // lblMateria
            // 
            this.lblMateria.AutoSize = true;
            this.lblMateria.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblMateria.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(190)))), ((int)(((byte)(210)))));
            this.lblMateria.Location = new System.Drawing.Point(17, 42);
            this.lblMateria.Name = "lblMateria";
            this.lblMateria.Size = new System.Drawing.Size(192, 21);
            this.lblMateria.TabIndex = 1;
            this.lblMateria.Text = "COMPUTACION GRAFICA";
            // 
            // lblNrc
            // 
            this.lblNrc.AutoSize = true;
            this.lblNrc.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNrc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(240)))), ((int)(((byte)(214)))));
            this.lblNrc.Location = new System.Drawing.Point(17, 70);
            this.lblNrc.Name = "lblNrc";
            this.lblNrc.Size = new System.Drawing.Size(88, 21);
            this.lblNrc.TabIndex = 2;
            this.lblNrc.Text = "NRC: 29505";
            // 
            // picLogoEspe
            // 
            this.picLogoEspe.BackColor = System.Drawing.Color.White;
            this.picLogoEspe.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picLogoEspe.ImageLocation = "Assets\\logo1.png";
            this.picLogoEspe.Location = new System.Drawing.Point(414, 11);
            this.picLogoEspe.Name = "picLogoEspe";
            this.picLogoEspe.Size = new System.Drawing.Size(180, 82);
            this.picLogoEspe.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogoEspe.TabIndex = 3;
            this.picLogoEspe.TabStop = false;
            // 
            // FrmHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(13)))), ((int)(((byte)(25)))));
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.panelPrincipal);
            this.Controls.Add(this.menuPrincipal);
            this.MainMenuStrip = this.menuPrincipal;
            this.MinimumSize = new System.Drawing.Size(1100, 680);
            this.Name = "FrmHome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reproductor Musical";
            this.menuPrincipal.ResumeLayout(false);
            this.menuPrincipal.PerformLayout();
            this.panelPrincipal.ResumeLayout(false);
            this.panelContenido.ResumeLayout(false);
            this.panelVisualizador.ResumeLayout(false);
            this.panelVisualizador.PerformLayout();
            this.panelControles.ResumeLayout(false);
            this.panelControles.PerformLayout();
            this.panelCancion.ResumeLayout(false);
            this.panelCancion.PerformLayout();
            this.panelIconoCancion.ResumeLayout(false);
            this.panelEncabezado.ResumeLayout(false);
            this.panelEncabezado.PerformLayout();
            this.panelMarca.ResumeLayout(false);
            this.panelMarca.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogoEspe)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuPrincipal;
        private System.Windows.Forms.ToolStripMenuItem menuArchivo;
        private System.Windows.Forms.ToolStripMenuItem menuCargarCancion;
        private System.Windows.Forms.ToolStripMenuItem menuSalir;
        private System.Windows.Forms.ToolStripMenuItem menuVisualizacion;
        private System.Windows.Forms.ToolStripMenuItem menuBarras;
        private System.Windows.Forms.ToolStripMenuItem menuOndas;
        private System.Windows.Forms.ToolStripMenuItem menuParticulas;
        private System.Windows.Forms.ToolStripMenuItem menuGeometria;
        private System.Windows.Forms.Panel panelPrincipal;
        private System.Windows.Forms.Panel panelEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private REPRODUCTOR_MUSICAL.Views.RoundedPanel panelMarca;
        private System.Windows.Forms.Label lblAutores;
        private System.Windows.Forms.Label lblMateria;
        private System.Windows.Forms.Label lblNrc;
        private System.Windows.Forms.PictureBox picLogoEspe;
        private System.Windows.Forms.Panel panelContenido;
        private REPRODUCTOR_MUSICAL.Views.DoubleBufferedPanel panelVisualizador;
        private System.Windows.Forms.Label lblVisualizador;
        private REPRODUCTOR_MUSICAL.Views.RoundedPanel panelControles;
        private REPRODUCTOR_MUSICAL.Views.RoundedPanel panelCancion;
        private REPRODUCTOR_MUSICAL.Views.MusicIconPanel panelIconoCancion;
        private System.Windows.Forms.Label lblIconoCancion;
        private System.Windows.Forms.Label lblCancion;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblAnalisis;
        private REPRODUCTOR_MUSICAL.Views.GradientButton btnCargar;
        private REPRODUCTOR_MUSICAL.Views.GradientButton btnReproducir;
        private REPRODUCTOR_MUSICAL.Views.GradientButton btnPausar;
        private REPRODUCTOR_MUSICAL.Views.GradientButton btnDetener;
        private System.Windows.Forms.Label lblTiempoActual;
        private REPRODUCTOR_MUSICAL.Views.NeonSlider trackPosicion;
        private System.Windows.Forms.Label lblTiempoTotal;
        private System.Windows.Forms.Label lblVolumen;
        private REPRODUCTOR_MUSICAL.Views.NeonSlider trackVolumen;
        private System.Windows.Forms.Label lblModo;
        private REPRODUCTOR_MUSICAL.Views.NeonComboBox cmbModoVisualizacion;
        private REPRODUCTOR_MUSICAL.Views.MusicIconPanel panelTituloIcono;
    }
}
