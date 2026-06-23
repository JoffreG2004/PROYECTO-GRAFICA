using proyectoPaint.GraphicsCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace proyectoPaint
{
    public partial class Form1 : Form
    {
        private readonly CanvasDocument document = new CanvasDocument();
        private readonly List<Point> pendingPoints = new List<Point>();
        private readonly List<StudioToolButton> toolButtons = new List<StudioToolButton>();
        private readonly DocumentController documentController;
        private PaintCanvas canvas;
        private Label status;
        private Label zoomLabel;
        private Label canvasSizeLabel;
        private Color strokeColor = Color.Black;
        private Color fillColor = Color.FromArgb(59, 130, 246);
        private StrokeRenderStyle currentStrokeStyle = StrokeRenderStyle.Solid;
        private int cornerRadius = 16;
        private PaintTool currentTool = PaintTool.Brush;
        private DrawableShape previewShape;
        private DrawableShape selectedShape;
        private Point startPoint;
        private Point lastPoint;
        private bool isDrawing;
        private bool isMovingSelection;
        private PolygonShape editingPolygon;
        private int editingVertexIndex = -1;
        private int regularPolygonSides = 3;
        private bool documentDirty = true;
        private bool hasHoverPoint;
        private bool gridVisible;
        private float zoom = 1F;
        private Point hoverPoint;
        private Bitmap cachedDocumentBitmap;
        private Bitmap activeStrokeBitmap;
        private Point activeStrokeLastPoint;
        private bool colorPickMode;
        private bool handMode;
        private string pendingText;
        private Point handStart;
        private Point canvasStart;
        private StudioCard artboard;
        private readonly Stopwatch repaintClock = Stopwatch.StartNew();
        private readonly Stopwatch strokeClock = Stopwatch.StartNew();
        private readonly Timer fillAnimationTimer = new Timer { Interval = 16 };
        private FloodFillShape animatedFillShape;
        // Inspector panel controls (uno por card del panel derecho)
        private ColorPanelControl colorPanel;
        private LayersPanelControl layersPanel;
        private ToolSettingsPanelControl toolSettingsPanel;
        private PropertiesPanelControl propertiesPanel;

        public Form1()
        {
            InitializeComponent();
            document.Width = 920;
            document.Height = 600;
            documentController = new DocumentController(document);
            fillAnimationTimer.Tick += FillAnimationTick;
            BuildStudio();
            RefreshCanvas();
            UpdateLayers();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (cachedDocumentBitmap != null) cachedDocumentBitmap.Dispose();
            if (activeStrokeBitmap != null) activeStrokeBitmap.Dispose();
            base.OnFormClosed(e);
        }

        // ─── Layout ────────────────────────────────────────────────────────────

        private void BuildStudio()
        {
            BackColor = ThemeColors.Background;
            Panel header = BuildHeader();
            Panel footer = BuildFooter();

            TableLayoutPanel main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                ColumnCount = 3, RowCount = 1,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 162));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Panel left = BuildToolbar();
            Panel workspace = BuildWorkspace();
            Panel right = BuildInspector();

            left.Dock = DockStyle.Fill;
            workspace.Dock = DockStyle.Fill;
            right.Dock = DockStyle.Fill;

            main.Controls.Add(left, 0, 0);
            main.Controls.Add(workspace, 1, 0);
            main.Controls.Add(right, 2, 0);

            Controls.Add(main);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private Panel BuildHeader()
        {
            Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeColors.Panel };
            header.Paint += (s, e) =>
            {
                using (Pen divider = new Pen(ThemeColors.Divider))
                    e.Graphics.DrawLine(divider, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            // Marca creada en código: evita el fondo verde del recurso anterior y
            // se conserva nítida en cualquier resolución.
            Panel mark = new Panel { Location = new Point(18, 12), Size = new Size(34, 34), BackColor = Color.Transparent };
            mark.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = StudioCard.RoundedRect(new Rectangle(0, 0, 33, 33), 10))
                using (LinearGradientBrush brush = new LinearGradientBrush(mark.ClientRectangle, ThemeColors.Accent, ThemeColors.AccentDark, 45F))
                using (Font letter = new Font("Bahnschrift SemiBold", 18F, FontStyle.Bold))
                {
                    e.Graphics.FillPath(brush, path);
                    TextRenderer.DrawText(e.Graphics, "L", letter, new Rectangle(0, -1, 34, 35), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
            Label brand = new Label { Text = "PAINT-ESPE", ForeColor = ThemeColors.TextPrimary, Font = new Font("Bahnschrift SemiBold", 15F, FontStyle.Regular), AutoSize = true, Location = new Point(62, 9) };
            Label subtitle = new Label { Text = "COMPUTACIÓN GRÁFICA", ForeColor = ThemeColors.TextSecondary, Font = new Font("Segoe UI Semibold", 7F, FontStyle.Bold), AutoSize = true, Location = new Point(64, 33) };
            header.Controls.Add(mark); header.Controls.Add(brand); header.Controls.Add(subtitle);

            // Acciones de archivo (cada una aparece una sola vez en toda la app).
            int x = 232;
            header.Controls.Add(CreateActionButton("Nuevo",    StudioIcon.NewFile, false, ref x, (s, e) => ClearDocument()));
            header.Controls.Add(CreateActionButton("Abrir",    StudioIcon.Open,    false, ref x, LoadProject));
            header.Controls.Add(CreateActionButton("Guardar",  StudioIcon.Save,    false, ref x, SaveProject));
            header.Controls.Add(CreateActionButton("Exportar", StudioIcon.Export,  true,  ref x, SaveImage));

            // Deshacer / Rehacer (no se repiten en ningún otro lugar).
            x += 8;
            StudioGlyphButton undoBtn = new StudioGlyphButton { Icon = StudioIcon.Undo, Size = new Size(32, 32), Location = new Point(x, 14) };
            StudioGlyphButton redoBtn = new StudioGlyphButton { Icon = StudioIcon.Redo, Size = new Size(32, 32), Location = new Point(x + 34, 14) };
            undoBtn.Click += (s, e) => Undo();
            redoBtn.Click += (s, e) => Redo();
            new ToolTip().SetToolTip(undoBtn, "Deshacer");
            new ToolTip().SetToolTip(redoBtn, "Rehacer");
            header.Controls.Add(undoBtn); header.Controls.Add(redoBtn);

            // Utilidades a la derecha: tema, ayuda, ajustes.
            StudioGlyphButton themeBtn = new StudioGlyphButton { Icon = StudioIcon.ThemeSun, Size = new Size(32, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            StudioGlyphButton helpBtn  = new StudioGlyphButton { Icon = StudioIcon.More,    Size = new Size(32, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            StudioGlyphButton labBtn    = new StudioGlyphButton { Icon = StudioIcon.Transform, Size = new Size(32, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            new ToolTip().SetToolTip(themeBtn, "Tema");
            new ToolTip().SetToolTip(helpBtn, "Ayuda");
            new ToolTip().SetToolTip(labBtn, "Laboratorio de algoritmos");
            themeBtn.Click += (s, e) => ToggleTheme();
            helpBtn.Click  += (s, e) => MessageBox.Show("Elige una herramienta en la barra izquierda y dibuja en el lienzo.\n\nFormas: usa el panel Formas.\nPolígono: clic en cada punto y doble clic para cerrar.\nBézier: clic en cuatro puntos.", "Ayuda - Lumina Paint", MessageBoxButtons.OK, MessageBoxIcon.Information);
            labBtn.Click   += (s, e) => { using (AlgorithmLabForm lab = new AlgorithmLabForm()) lab.ShowDialog(this); };
            header.Controls.Add(themeBtn); header.Controls.Add(helpBtn); header.Controls.Add(labBtn);
            header.Resize += (s, e) =>
            {
                int r = header.Width;
                themeBtn.Location = new Point(r - 44, 14);
                helpBtn.Location  = new Point(r - 80, 14);
                labBtn.Location   = new Point(r - 116, 14);
            };
            return header;
        }

        /// <summary>Botón de acción del encabezado (icono + texto) con estilo del tema.</summary>
        private Panel CreateActionButton(string caption, StudioIcon icon, bool primary, ref int x, EventHandler onClick)
        {
            int width = Math.Max(86, TextRenderer.MeasureText(caption, new Font("Segoe UI Semibold", 9.5F)).Width + 56);
            Panel btn = new Panel { Size = new Size(width, 38), Location = new Point(x, 11), Cursor = Cursors.Hand, BackColor = ThemeColors.Panel };
            bool hovering = false;
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color fill = primary ? (hovering ? ThemeColors.Accent : ThemeColors.Export)
                                     : (hovering ? ThemeColors.Hover : ThemeColors.Canvas);
                Color ink  = primary ? Color.White : ThemeColors.TextPrimary;
                using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, btn.Width - 1, btn.Height - 1), 10))
                using (var brush = new SolidBrush(fill))
                using (var pen = new Pen(primary ? ThemeColors.Export : ThemeColors.Border))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
                StudioToolButton.DrawIcon(e.Graphics, new Rectangle(12, 8, 22, 22), icon, primary ? Color.White : ThemeColors.Icon);
                TextRenderer.DrawText(e.Graphics, caption, new Font("Segoe UI Semibold", 9.5F), new Rectangle(36, 0, btn.Width - 44, btn.Height), ink, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                if (primary)
                    TextRenderer.DrawText(e.Graphics, "▾", new Font("Segoe UI", 8F), new Rectangle(btn.Width - 20, 0, 16, btn.Height), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btn.MouseEnter += (s, e) => { hovering = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { hovering = false; btn.Invalidate(); };
            if (onClick != null) btn.Click += onClick;
            x += width + 8;
            return btn;
        }

        private void ToggleTheme()
        {
            ThemeColors.Toggle();
            toolButtons.Clear();
            Controls.Clear();
            BuildStudio();
            RefreshCanvas();
            UpdateLayers();
            status.Text = ThemeColors.IsDark ? "Tema oscuro activado" : "Tema claro activado";
        }

        private Panel BuildWorkspace()
        {
            Panel workspace = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(2, 0, 2, 0) };
            artboard = new StudioCard { BackColor = ThemeColors.Canvas, Padding = new Padding(9) };
            canvas = new PaintCanvas { Location = new Point(9, 9), BorderStyle = BorderStyle.None, BackColor = Color.White };
            canvas.MouseDown    += CanvasMouseDown;
            canvas.MouseMove    += CanvasMouseMove;
            canvas.MouseUp      += CanvasMouseUp;
            canvas.DoubleClick  += CanvasDoubleClick;
            artboard.Controls.Add(canvas);
            workspace.Controls.Add(artboard);

            // Canvas llena todo el espacio disponible; actualiza el documento al cambiar de tamaño.
            Action fitCanvas = () =>
            {
                int pad = 18;
                int w = Math.Max(200, workspace.ClientSize.Width - pad * 2);
                int h = Math.Max(200, workspace.ClientSize.Height - pad * 2);
                artboard.SetBounds(pad, pad, w, h);
                canvas.SetBounds(9, 9, w - 18, h - 18);
                if (document.Width == canvas.Width && document.Height == canvas.Height) return;
                document.Width = canvas.Width;
                document.Height = canvas.Height;
                if (cachedDocumentBitmap != null) { cachedDocumentBitmap.Dispose(); cachedDocumentBitmap = null; }
                documentDirty = true;
                UpdateCanvasSizeLabel();
                RefreshCanvas();
            };
            workspace.Resize += (s, e) => fitCanvas();
            workspace.HandleCreated += (s, e) => fitCanvas();
            return workspace;
        }

        // ─── Left toolbar ──────────────────────────────────────────────────────

        private Panel BuildToolbar()
        {
            Panel left = new Panel { Width = 162, BackColor = ThemeColors.Background, Padding = new Padding(12, 12, 10, 12) };
            StudioCard tools = new StudioCard { Dock = DockStyle.Fill, BackColor = ThemeColors.Panel, Padding = new Padding(6) };
            FlowLayoutPanel list = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = ThemeColors.Panel, Padding = new Padding(2, 4, 2, 4) };

            // Herramientas de dibujo (cada una existe una sola vez en toda la app).
            AddRailTool(list, "Lápiz",       StudioIcon.Pencil,    PaintTool.Pencil);
            AddRailTool(list, "Pincel",      StudioIcon.Brush,     PaintTool.Brush);
            AddRailTool(list, "Borrador",    StudioIcon.Eraser,    PaintTool.Eraser);
            AddRailTool(list, "Formas",      StudioIcon.Rectangle, PaintTool.Rectangle, "Elige la figura en el panel Formas →");
            AddRailTool(list, "Relleno",     StudioIcon.Fill,      PaintTool.Fill);
            AddRailAction(list, "Texto",       StudioIcon.Text,       (s, e) => BeginTextInsertion());
            AddRailAction(list, "Cuentagotas", StudioIcon.Eyedropper, (s, e) => BeginColorPick());

            // Grupo de navegación del lienzo (separado, como en el mockup).
            list.Controls.Add(new Panel { Size = new Size(132, 1), Margin = new Padding(6, 8, 6, 8), BackColor = ThemeColors.Divider });
            AddRailAction(list, "Zoom",        StudioIcon.Zoom,       (s, e) => AdjustZoom(10));
            AddRailAction(list, "Mano",        StudioIcon.Hand,       (s, e) => ActivateHandTool());
            AddRailTool(list, "Seleccionar", StudioIcon.Select,    PaintTool.Select);

            tools.Controls.Add(list);
            left.Controls.Add(tools);
            return left;
        }

        private void AddRailTool(FlowLayoutPanel list, string caption, StudioIcon icon, PaintTool tool, string hint = null)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Tag = tool, Horizontal = true, Margin = new Padding(0, 2, 0, 2), Size = new Size(132, 40) };
            btn.Click += (s, e) => { ActivateTool(tool); if (hint != null && status != null) status.Text = hint; };
            toolButtons.Add(btn);
            list.Controls.Add(btn);
            if (tool == currentTool) btn.Selected = true;
        }

        private void AddRailAction(FlowLayoutPanel list, string caption, StudioIcon icon, EventHandler action)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Horizontal = true, Margin = new Padding(0, 2, 0, 2), Size = new Size(132, 40) };
            btn.Click += action;
            list.Controls.Add(btn);
        }

        // ─── Right Inspector (dos columnas, cada card es un UserControl) ────────

        private Panel BuildInspector()
        {
            Panel right    = new Panel { BackColor = ThemeColors.Background };
            // Todas las tarjetas se mantienen dentro del alto disponible: no se
            // usan barras de desplazamiento en el inspector.
            Panel leftCol  = new Panel { BackColor = ThemeColors.Background };
            Panel rightCol = new Panel { BackColor = ThemeColors.Background };

            colorPanel        = new ColorPanelControl(strokeColor, fillColor);
            layersPanel       = new LayersPanelControl();
            toolSettingsPanel = new ToolSettingsPanelControl();
            ShapePanelControl shapePanel = new ShapePanelControl();
            propertiesPanel   = new PropertiesPanelControl(strokeColor, fillColor, currentStrokeStyle, cornerRadius);

            // Color (único lugar para elegir color en toda la app)
            colorPanel.FillColorPickRequested   += () => PickColor(fillColor, SetFillColor);
            colorPanel.StrokeColorPickRequested += () => PickColor(strokeColor, SetStrokeColor);
            colorPanel.SwatchPicked             += color => ApplyColorEverywhere(color);
            colorPanel.StatusChanged            += msg => status.Text = msg;

            // Capas
            layersPanel.ShapeSelected     += shape =>
            {
                selectedShape = shape; canvas.SelectedShape = shape;
                status.Text = "Seleccionado: " + shape.DisplayName;
                SyncPropertiesFromSelection(); RefreshCanvas();
                layersPanel.Refresh(document.Shapes, selectedShape);
            };
            layersPanel.ShapeDeleted += shape =>
            {
                documentController.Remove(shape);
                if (selectedShape == shape) { selectedShape = null; canvas.SelectedShape = null; }
                documentDirty = true;
                status.Text = "Capa eliminada: " + shape.DisplayName;
                RefreshCanvas(); layersPanel.Refresh(document.Shapes, selectedShape);
            };
            layersPanel.VisibilityToggled += shape =>
            {
                documentDirty = true;
                status.Text = (shape.Visible ? "Capa visible: " : "Capa oculta: ") + shape.DisplayName;
                RefreshCanvas(); layersPanel.Refresh(document.Shapes, selectedShape);
            };
            layersPanel.MoreMenuRequested += owner => ShowLayerMenu(owner);
            layersPanel.AddLayerRequested += () => { ActivateTool(PaintTool.Rectangle); status.Text = "Dibuja un rectángulo para añadir una capa"; };

            // Tamaño del pincel + Opacidad (únicos controles de grosor y opacidad)
            toolSettingsPanel.BrushSizeChanged += v => { ApplySelectedThickness(v); status.Text = "Tamaño: " + v + " px"; };
            toolSettingsPanel.OpacityChanged   += v => { ApplySelectedOpacity(v / 100F); status.Text = "Opacidad: " + v + "%"; };
            toolSettingsPanel.FlowChanged      += v => status.Text = "Flujo del pincel: " + v + "%";
            toolSettingsPanel.SmoothingChanged += v => status.Text = "Suavizado del pincel: " + v + "%";
            toolSettingsPanel.FillToggled      += v => status.Text = v ? "Relleno de forma activado" : "Relleno de forma desactivado";
            toolSettingsPanel.UpdateForTool(currentTool, fillColor, currentStrokeStyle);

            // Formas: único lugar donde viven todas las figuras
            shapePanel.ToolSelected += tool => ActivateTool(tool);
            shapePanel.RegularPolygonSelected += sides => { regularPolygonSides = sides; ActivateTool(PaintTool.RegularPolygon); status.Text = "Polígono de " + sides + " lados: arrastra desde el centro."; };

            // Propiedades: solo lo propio de la forma (sin color/grosor/opacidad repetidos)
            propertiesPanel.StrokeStyleChanged  += style => SetStrokeStyle(style);
            propertiesPanel.CornerRadiusChanged += v => { cornerRadius = v; ApplySelectedCornerRadius(v); status.Text = "Radio de esquina: " + v + " px"; };
            propertiesPanel.RotateRequested     += () => TransformSelected(shape => shape.Rotate(15), (fill, center) => fill.RotateWithTarget(center, 15));
            propertiesPanel.ScaleUpRequested    += () => TransformSelected(shape => shape.Scale(1.1F), (fill, center) => fill.ScaleWithTarget(center, 1.1F));
            propertiesPanel.ScaleDownRequested  += () => TransformSelected(shape => shape.Scale(0.9F), (fill, center) => fill.ScaleWithTarget(center, 0.9F));

            leftCol.Controls.Add(colorPanel);
            leftCol.Controls.Add(toolSettingsPanel);
            leftCol.Controls.Add(layersPanel);
            rightCol.Controls.Add(shapePanel);
            rightCol.Controls.Add(propertiesPanel);
            right.Controls.Add(leftCol);
            right.Controls.Add(rightCol);

            Action layout = () =>
            {
                int gap = 8;
                int leftWidth = Math.Max(248, (int)((right.Width - gap) * 0.58F));
                int rightWidth = Math.Max(176, right.Width - leftWidth - gap);
                leftCol.SetBounds(0, 0, leftWidth, right.Height);
                rightCol.SetBounds(leftWidth + gap, 0, rightWidth, right.Height);

                int leftCardWidth = Math.Max(120, leftWidth - 8);
                int rightCardWidth = Math.Max(120, rightWidth - 8);

                // Columna izquierda: Colores · Tamaño del pincel + Opacidad · Capas
                colorPanel.SetBounds(4, 4, leftCardWidth, 198);
                toolSettingsPanel.SetBounds(4, 208, leftCardWidth, 252);
                int layersTop = 468;
                layersPanel.SetBounds(4, layersTop, leftCardWidth, Math.Max(120, right.Height - layersTop - 4));

                // Columna derecha: Formas · Propiedades
                const int shapePanelHeight = 248;
                int propertiesTop = shapePanelHeight + 10;
                shapePanel.SetBounds(4, 4, rightCardWidth, shapePanelHeight);
                propertiesPanel.SetBounds(4, propertiesTop, rightCardWidth, Math.Max(280, right.Height - propertiesTop - 4));
            };
            right.Resize += (s, e) => layout();
            right.HandleCreated += (s, e) => layout();
            return right;
        }

        // ─── Footer ────────────────────────────────────────────────────────────

        private Panel BuildFooter()
        {
            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 38, BackColor = ThemeColors.Panel };
            footer.Paint += (s, e) => { using (Pen p = new Pen(ThemeColors.Divider)) e.Graphics.DrawLine(p, 0, 0, footer.Width, 0); };

            // Zoom −  100%  +   (izquierda)
            Label zOut = new Label { Text = "−", ForeColor = ThemeColors.Icon, Font = new Font("Segoe UI", 12F), AutoSize = true, Location = new Point(16, 8), Cursor = Cursors.Hand };
            zoomLabel  = new Label { Text = "100%", ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI Semibold", 8.5F), AutoSize = true, Location = new Point(40, 11) };
            Label zIn  = new Label { Text = "+", ForeColor = ThemeColors.Icon, Font = new Font("Segoe UI", 12F), AutoSize = true, Location = new Point(82, 8), Cursor = Cursors.Hand };
            zOut.Click += (s, e) => AdjustZoom(-10);
            zIn.Click  += (s, e) => AdjustZoom(10);
            footer.Controls.Add(zOut); footer.Controls.Add(zoomLabel); footer.Controls.Add(zIn);

            // Estado / coordenadas (área dinámica)
            status = new Label { Text = "", ForeColor = ThemeColors.TextSecondary, AutoSize = true, Location = new Point(132, 11), Font = new Font("Segoe UI", 8.5F) };
            footer.Controls.Add(status);

            // Lado derecho: Cuadrícula · Tamaño del lienzo · Modo RGB · Listo
            Label gridLabel = new Label { Text = "Cuadrícula", ForeColor = ThemeColors.TextSecondary, AutoSize = true, Location = new Point(0, 11), Font = new Font("Segoe UI", 8.5F), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            ToggleSwitch gridToggle = new ToggleSwitch { Location = new Point(0, 9), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            gridToggle.Toggled += (s, e) => { gridVisible = gridToggle.On; canvas.ShowGrid = gridVisible; status.Text = gridVisible ? "Cuadrícula activada" : "Cuadrícula desactivada"; canvas.Invalidate(); };
            canvasSizeLabel = new Label { Text = "Tamaño del lienzo: —", ForeColor = ThemeColors.TextSecondary, AutoSize = true, Location = new Point(0, 11), Font = new Font("Segoe UI", 8.5F), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            Label modo = new Label { Text = "Modo: RGB", ForeColor = ThemeColors.TextSecondary, AutoSize = true, Location = new Point(0, 11), Font = new Font("Segoe UI", 8.5F), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            Label ready = new Label { Text = "✓ Listo", ForeColor = ThemeColors.Accent, AutoSize = true, Location = new Point(0, 11), Font = new Font("Segoe UI Semibold", 8.5F), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            footer.Controls.Add(gridLabel); footer.Controls.Add(gridToggle); footer.Controls.Add(canvasSizeLabel); footer.Controls.Add(modo); footer.Controls.Add(ready);

            footer.Resize += (s, e) =>
            {
                int r = footer.Width;
                ready.Location = new Point(r - ready.Width - 16, 11);
                modo.Location = new Point(ready.Left - modo.Width - 24, 11);
                canvasSizeLabel.Location = new Point(modo.Left - canvasSizeLabel.Width - 24, 11);
                gridToggle.Location = new Point(canvasSizeLabel.Left - gridToggle.Width - 16, 9);
                gridLabel.Location = new Point(gridToggle.Left - gridLabel.Width - 6, 11);
            };
            return footer;
        }

        private void UpdateCanvasSizeLabel()
        {
            if (canvasSizeLabel == null) return;
            canvasSizeLabel.Text = "Tamaño del lienzo: " + document.Width + " x " + document.Height + " px";
        }

        private void BeginColorPick()
        {
            colorPickMode = true;
            handMode = false;
            pendingText = null;
            canvas.Cursor = Cursors.Cross;
            status.Text = "Cuentagotas: haz clic sobre un color del lienzo";
        }

        private void ActivateHandTool()
        {
            handMode = true;
            colorPickMode = false;
            pendingText = null;
            canvas.Cursor = Cursors.Hand;
            status.Text = "Mano: arrastra el lienzo cuando uses zoom";
        }

        private void BeginTextInsertion()
        {
            using (Form dialog = new Form())
            using (TextBox input = new TextBox())
            using (Button accept = new Button())
            using (Button cancel = new Button())
            {
                dialog.Text = "Insertar texto";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(330, 125);
                dialog.BackColor = ThemeColors.Panel;

                Label label = new Label { Text = "Escribe el texto que deseas insertar:", AutoSize = true, Location = new Point(16, 15), ForeColor = ThemeColors.TextPrimary };
                input.SetBounds(16, 40, 298, 25);
                input.Font = new Font("Segoe UI", 10F);
                accept.Text = "Insertar"; accept.DialogResult = DialogResult.OK; accept.SetBounds(145, 82, 82, 27);
                cancel.Text = "Cancelar"; cancel.DialogResult = DialogResult.Cancel; cancel.SetBounds(232, 82, 82, 27);
                dialog.Controls.Add(label); dialog.Controls.Add(input); dialog.Controls.Add(accept); dialog.Controls.Add(cancel);
                dialog.AcceptButton = accept; dialog.CancelButton = cancel;

                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(input.Text)) return;
                pendingText = input.Text.Trim();
                colorPickMode = false;
                handMode = false;
                canvas.Cursor = Cursors.IBeam;
                status.Text = "Texto: haz clic en el lienzo para colocarlo";
            }
        }

        private void PickColorFromCanvas(Point point)
        {
            EnsureDocumentCache();
            if (cachedDocumentBitmap == null) return;
            Color picked = cachedDocumentBitmap.GetPixel(point.X, point.Y);
            ApplyColorEverywhere(picked);
            colorPickMode = false;
            canvas.Cursor = Cursors.Cross;
            status.Text = "Color tomado: " + ColorTranslator.ToHtml(picked);
        }

        // ─── Tool / Canvas events ──────────────────────────────────────────────

        private void AddShape(DrawableShape shape)
        {
            documentController.Add(shape);
            documentDirty = true;
            UpdateLayers();
        }

        private void ClearDocument()
        {
            documentController.Clear();
            documentDirty = true;
            pendingPoints.Clear();
            previewShape = null;
            selectedShape = null;
            canvas.SelectedShape = null;
            UpdateLayers();
            status.Text = "Lienzo limpiado";
            RefreshCanvas();
        }

        private void Undo()
        {
            if (!documentController.Undo()) { status.Text = "Nada que deshacer"; return; }
            selectedShape = null;
            documentDirty = true;
            status.Text = "Deshacer";
            RefreshCanvas();
            UpdateLayers();
        }

        private void Redo()
        {
            if (!documentController.Redo()) { status.Text = "Nada que rehacer"; return; }
            documentDirty = true;
            status.Text = "Rehacer";
            RefreshCanvas();
            UpdateLayers();
        }

        private void BringSelectedToFront()
        {
            if (selectedShape == null) { status.Text = "Selecciona una forma primero"; return; }
            documentController.BringToFront(selectedShape);
            documentDirty = true;
            status.Text = "Forma enviada al frente";
            RefreshCanvas();
            UpdateLayers();
        }

        private void TransformSelected(Action<DrawableShape> transform, Action<FloodFillShape, Point> transformFill)
        {
            if (selectedShape == null) { status.Text = "Selecciona una forma primero"; return; }
            Rectangle bounds = selectedShape.Bounds;
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
            transform(selectedShape);
            foreach (FloodFillShape fill in document.Shapes.OfType<FloodFillShape>().Where(fill => fill.LinkedShape == selectedShape))
                transformFill(fill, center);
            documentDirty = true;
            status.Text = "Transformación aplicada";
            RefreshCanvas();
        }

        private void SetStrokeStyle(StrokeRenderStyle style)
        {
            currentStrokeStyle = style;
            propertiesPanel.SetStrokeStyle(style);
            ApplySelectedShapeChange(shape => shape.StrokeStyle = style);
            toolSettingsPanel.UpdateForTool(currentTool, fillColor, style);
            status.Text = "Estilo de borde: " + style;
        }

        private void SetStrokeColor(Color color)
        {
            strokeColor = color;
            colorPanel.SetStrokeColor(color);
            ApplySelectedShapeChange(shape =>
            {
                if (!(shape is FloodFillShape)) shape.StrokeColor = color;
            });
            toolSettingsPanel.InvalidatePreview();
        }

        private void SetFillColor(Color color)
        {
            fillColor = color;
            colorPanel.SetFillColor(color);
            ApplySelectedShapeChange(shape =>
            {
                if (CanUseFill(shape))
                {
                    shape.FillColor = color;
                    shape.UseFill = true;
                }
            });
            toolSettingsPanel.UpdateForTool(currentTool, color, currentStrokeStyle);
        }

        /// <summary>Aplica un color tanto al trazo como al relleno (clic en un swatch).</summary>
        private void ApplyColorEverywhere(Color color)
        {
            SetStrokeColor(color);
            SetFillColor(color);
            status.Text = "Color: " + ColorTranslator.ToHtml(color);
        }

        private void ApplySelectedThickness(int value) { ApplySelectedShapeChange(shape => shape.Thickness = value); }

        private void ApplySelectedOpacity(float value) { ApplySelectedShapeChange(shape => shape.Opacity = value); }

        private void ApplySelectedCornerRadius(int value)
        {
            RoundedRectangleShape rounded = selectedShape as RoundedRectangleShape;
            if (rounded == null) return;
            rounded.SetCornerRadius(value);
            documentDirty = true;
            RefreshCanvas();
        }

        private bool CanUseFill(DrawableShape shape)
        {
            return !(shape is LineShape) && !(shape is PolylineShape) && !(shape is BezierShape) && !(shape is FloodFillShape);
        }

        private void ApplySelectedShapeChange(Action<DrawableShape> change)
        {
            if (selectedShape == null || change == null) return;
            change(selectedShape);
            documentDirty = true;
            RefreshCanvas();
        }

        private void SyncPropertiesFromSelection()
        {
            if (selectedShape == null) return;
            strokeColor = selectedShape.StrokeColor;
            fillColor   = selectedShape.FillColor;
            currentStrokeStyle = selectedShape.StrokeStyle;
            colorPanel.SetStrokeColor(strokeColor);
            colorPanel.SetFillColor(fillColor);
            propertiesPanel.SyncFromShape(selectedShape);
            toolSettingsPanel.SetFillChecked(selectedShape.UseFill);
            toolSettingsPanel.SetThickness(selectedShape.Thickness);
            toolSettingsPanel.UpdateForTool(currentTool, fillColor, currentStrokeStyle);
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            StudioToolButton btn = sender as StudioToolButton;
            if (btn == null || !(btn.Tag is PaintTool)) return;
            ActivateTool((PaintTool)btn.Tag);
        }

        private void ActivateTool(PaintTool tool)
        {
            currentTool = tool; pendingPoints.Clear(); previewShape = null; isDrawing = false; hasHoverPoint = false;
            colorPickMode = false; handMode = false; pendingText = null; canvas.Cursor = Cursors.Cross;
            SetActiveToolButton(tool);
            toolSettingsPanel.UpdateForTool(tool, fillColor, currentStrokeStyle);
            status.Text = "Herramienta: " + ToolDisplayName(tool);
            RefreshCanvas();
        }

        private string ToolDisplayName(PaintTool tool)
        {
            switch (tool)
            {
                case PaintTool.Select:           return "Seleccionar";
                case PaintTool.Brush:            return "Pincel";
                case PaintTool.Pencil:           return "Lápiz";
                case PaintTool.Eraser:           return "Borrador";
                case PaintTool.Rectangle:        return "Rectángulo";
                case PaintTool.RoundedRectangle: return "Rect. redondeado";
                case PaintTool.Ellipse:          return "Elipse";
                case PaintTool.Fill:             return "Relleno";
                case PaintTool.Polygon:          return "Polígono";
                case PaintTool.Bezier:           return "Bézier";
                case PaintTool.Line:             return "Línea";
                case PaintTool.Arrow:            return "Flecha";
                case PaintTool.Star:             return "Estrella";
                case PaintTool.Blob:             return "Blob";
                default:                         return tool.ToString();
            }
        }

        private void SetActiveToolButton(PaintTool tool)
        {
            foreach (StudioToolButton btn in toolButtons)
                btn.Selected = (btn.Tag is PaintTool) && (PaintTool)btn.Tag == tool;
        }

        private void CanvasMouseDown(object sender, MouseEventArgs e)
        {
            startPoint = ClampPoint(e.Location); lastPoint = startPoint;
            if (colorPickMode)
            {
                PickColorFromCanvas(startPoint);
                return;
            }
            if (!string.IsNullOrEmpty(pendingText))
            {
                TextShape text = NewShape(new TextShape { Position = startPoint, Text = pendingText, FontSize = 22 }) as TextShape;
                text.LayerName = "Texto";
                AddShape(text);
                pendingText = null;
                canvas.Cursor = Cursors.Cross;
                status.Text = "Texto insertado";
                RefreshCanvas();
                return;
            }
            if (handMode)
            {
                handStart = e.Location;
                canvasStart = canvas.Location;
                canvas.Cursor = Cursors.SizeAll;
                return;
            }
            if (currentTool == PaintTool.Fill)
            {
                // Un relleno del mismo color que el fondo parece que "no hace nada".
                // Elegimos un azul visible y lo reflejamos en los selectores.
                if (fillColor.ToArgb() == document.BackgroundColor.ToArgb())
                {
                    fillColor = ThemeColors.Accent;
                    colorPanel.SetFillColor(fillColor);
                }
                DrawableShape target = document.Shapes.LastOrDefault(shape => !(shape is FloodFillShape) && shape.HitTest(startPoint));
                FloodFillShape fill = NewShape(new FloodFillShape { Seed = startPoint, LinkedShape = target }) as FloodFillShape;
                fill.MaxSpans = 0;
                AddShape(fill);
                animatedFillShape = fill;
                fillAnimationTimer.Start();
                status.Text = "Rellenando región…";
                RefreshCanvas(); return;
            }
            if (currentTool == PaintTool.Select)
            {
                selectedShape = document.Shapes.LastOrDefault(s => s.HitTest(startPoint));
                canvas.SelectedShape = selectedShape; isMovingSelection = selectedShape != null;
                PolygonShape polygon = selectedShape as PolygonShape;
                editingVertexIndex = FindVertex(polygon, startPoint);
                editingPolygon = editingVertexIndex >= 0 ? polygon : null;
                if (editingPolygon != null) isMovingSelection = false;
                if (selectedShape != null) SyncPropertiesFromSelection();
                status.Text = selectedShape == null ? "Ninguna forma seleccionada" : "Seleccionado: " + selectedShape.DisplayName;
                RefreshCanvas(); return;
            }
            if (currentTool == PaintTool.Bezier)
            {
                AddBezierPoint(startPoint);
                return;
            }
            if (currentTool == PaintTool.Polygon)
            {
                AddPolygonPoint(startPoint);
                return;
            }
            isDrawing = true;
            if (currentTool == PaintTool.Brush || currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser)
            {
                PolylineShape line = NewShape(new PolylineShape()) as PolylineShape;
                line.Vertices.Add(startPoint); previewShape = line;
                EnsureDocumentCache();
                activeStrokeBitmap = (Bitmap)cachedDocumentBitmap.Clone();
                activeStrokeLastPoint = startPoint;
                strokeClock.Restart();
            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            Point p = ClampPoint(e.Location);
            if (handMode && e.Button == MouseButtons.Left && artboard != null)
            {
                int minX = Math.Min(9, artboard.ClientSize.Width - canvas.Width - 9);
                int minY = Math.Min(9, artboard.ClientSize.Height - canvas.Height - 9);
                int x = Math.Max(minX, Math.Min(9, canvasStart.X + e.X - handStart.X));
                int y = Math.Max(minY, Math.Min(9, canvasStart.Y + e.Y - handStart.Y));
                canvas.Location = new Point(x, y);
                status.Text = "Moviendo lienzo";
                return;
            }
            status.Text = "X " + p.X + "  Y " + p.Y + "  " + ToolDisplayName(currentTool);
            if (isMovingSelection && selectedShape != null)
            {
                int dx = p.X - lastPoint.X, dy = p.Y - lastPoint.Y;
                selectedShape.Translate(dx, dy);
                foreach (FloodFillShape fill in document.Shapes.OfType<FloodFillShape>().Where(fill => fill.LinkedShape == selectedShape))
                    fill.TranslateWithTarget(dx, dy);
                documentDirty = true;
                lastPoint = p;
                if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
                return;
            }
            if (editingPolygon != null && editingVertexIndex >= 0)
            {
                editingPolygon.Vertices[editingVertexIndex] = p;
                documentDirty = true;
                if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
                return;
            }
            if ((currentTool == PaintTool.Bezier || currentTool == PaintTool.Polygon) && pendingPoints.Count > 0)
            {
                hoverPoint = p;
                hasHoverPoint = true;
                if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
                return;
            }
            if (!isDrawing) return;
            if (currentTool == PaintTool.Brush || currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser)
            {
                PolylineShape stroke = (PolylineShape)previewShape;
                stroke.Vertices.Add(p);
                if (strokeClock.ElapsedMilliseconds >= 8)
                {
                    GraphicsAlgorithms.DrawLine(activeStrokeBitmap, activeStrokeLastPoint, p, stroke.GetRenderPaint(), stroke.Thickness, stroke.StrokeStyle);
                    activeStrokeLastPoint = p;
                    strokeClock.Restart();
                }
            }
            else
                previewShape = BuildDragShape(startPoint, p);
            if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
        }

        private void CanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (handMode) { canvas.Cursor = Cursors.Hand; return; }
            if (editingPolygon != null) { editingPolygon = null; editingVertexIndex = -1; documentDirty = true; RefreshCanvas(); return; }
            if (isMovingSelection) { isMovingSelection = false; RefreshCanvas(); return; }
            if (!isDrawing) return;
            isDrawing = false;
            Point end = ClampPoint(e.Location);
            if (activeStrokeBitmap != null && (currentTool == PaintTool.Brush || currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser))
            {
                PolylineShape stroke = (PolylineShape)previewShape;
                if (activeStrokeLastPoint != end) { stroke.Vertices.Add(end); GraphicsAlgorithms.DrawLine(activeStrokeBitmap, activeStrokeLastPoint, end, stroke.GetRenderPaint(), stroke.Thickness, stroke.StrokeStyle); }
            }
            if (currentTool == PaintTool.Polygon || currentTool == PaintTool.Bezier)
            {
                pendingPoints.Add(end);
                status.Text = currentTool == PaintTool.Polygon
                    ? "Puntos del polígono: " + pendingPoints.Count + ". Doble clic para cerrar."
                    : "Puntos Bézier: " + pendingPoints.Count + "/4";
                if (currentTool == PaintTool.Bezier && pendingPoints.Count == 4) FinishPendingShape();
                return;
            }
            if (previewShape == null) previewShape = BuildDragShape(startPoint, end);
            if (previewShape != null) AddShape(previewShape);
            previewShape = null;
            if (activeStrokeBitmap != null) { activeStrokeBitmap.Dispose(); activeStrokeBitmap = null; }
            RefreshCanvas();
        }

        private void CanvasDoubleClick(object sender, EventArgs e)
        {
            if (currentTool == PaintTool.Bezier) return;
            FinishPendingShape();
        }

        private void AddPolygonPoint(Point point)
        {
            if (pendingPoints.Count > 0 && pendingPoints[pendingPoints.Count - 1] == point) return;
            pendingPoints.Add(point);
            hasHoverPoint = false;
            status.Text = "Polígono: " + pendingPoints.Count + " puntos. Doble clic para cerrar.";
            RefreshCanvas();
        }

        private void FillAnimationTick(object sender, EventArgs e)
        {
            if (animatedFillShape == null) { fillAnimationTimer.Stop(); return; }
            // ~100 cuadros de 30 ms: la ruta de la semilla se aprecia durante unos 3 segundos.
            int chunk = Math.Max(1, animatedFillShape.TotalSeedPoints / 35);
            animatedFillShape.MaxSpans += chunk;
            documentDirty = true;
            RefreshCanvas();
            if (animatedFillShape.IsComplete)
            {
                animatedFillShape.MaxSpans = int.MaxValue;
                animatedFillShape = null;
                fillAnimationTimer.Stop();
                status.Text = "Relleno completado";
            }
        }

        private void AddBezierPoint(Point point)
        {
            pendingPoints.Add(point);
            hasHoverPoint = false;
            status.Text = "Punto Bézier " + pendingPoints.Count + "/4";
            if (pendingPoints.Count >= 4) FinishPendingShape();
            else RefreshCanvas();
        }

        private void FinishPendingShape()
        {
            if (currentTool == PaintTool.Polygon && pendingPoints.Count >= 3)
            {
                PolygonShape polygon = NewShape(new PolygonShape()) as PolygonShape;
                polygon.Vertices.AddRange(pendingPoints); AddShape(polygon);
            }
            else if (currentTool == PaintTool.Bezier && pendingPoints.Count >= 4)
            {
                BezierShape curve = NewShape(new BezierShape()) as BezierShape;
                curve.ControlPoints.AddRange(pendingPoints.Take(4)); AddShape(curve);
            }
            pendingPoints.Clear(); previewShape = null; hasHoverPoint = false; RefreshCanvas();
        }

        private DrawableShape BuildDragShape(Point a, Point b)
        {
            if (currentTool == PaintTool.Line)             return NewShape(new LineShape { Start = a, End = b });
            if (currentTool == PaintTool.Rectangle)        return NewShape(new RectangleShape(a, b));
            if (currentTool == PaintTool.RoundedRectangle) return NewShape(new RoundedRectangleShape(a, b, cornerRadius));
            if (currentTool == PaintTool.Ellipse)          return NewShape(new EllipseShape { A = a, B = b });
            if (currentTool == PaintTool.Arrow)            return NewShape(new ArrowShape(a, b, toolSettingsPanel.BrushSize));
            if (currentTool == PaintTool.Star)             return NewShape(new StarShape(a, b));
            if (currentTool == PaintTool.Blob)             return NewShape(new BlobShape(a, b));
            if (currentTool == PaintTool.RegularPolygon)   return NewShape(new RegularPolygonShape(a, b, regularPolygonSides));
            return null;
        }

        private int FindVertex(PolygonShape polygon, Point point)
        {
            if (polygon == null) return -1;
            int radius = Math.Max(8, (int)(10 / Math.Max(.5F, zoom)));
            int vertex = -1, nearest = radius * radius;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                int dx = polygon.Vertices[i].X - point.X, dy = polygon.Vertices[i].Y - point.Y, distance = dx * dx + dy * dy;
                if (distance <= nearest) { vertex = i; nearest = distance; }
            }
            return vertex;
        }

        private DrawableShape NewShape(DrawableShape shape)
        {
            shape.StrokeColor = currentTool == PaintTool.Eraser ? document.BackgroundColor : strokeColor;
            shape.FillColor   = fillColor;
            shape.Thickness   = currentTool == PaintTool.Eraser ? Math.Max(10, toolSettingsPanel.BrushSize) : toolSettingsPanel.BrushSize;
            shape.StrokeStyle = currentStrokeStyle;
            shape.Opacity     = toolSettingsPanel.OpacityPercent / 100F;
            shape.UseFill     = toolSettingsPanel.FillShapes && CanUseFill(shape);
            if (currentTool == PaintTool.Eraser)      shape.LayerName = "Borrador";
            else if (currentTool == PaintTool.Brush)  shape.LayerName = "Pincel";
            else if (currentTool == PaintTool.Pencil) shape.LayerName = "Lapiz";
            if (shape is PolylineShape)
            {
                PolylineShape line = (PolylineShape)shape;
                line.Flow      = currentTool == PaintTool.Brush ? toolSettingsPanel.FlowPercent    : 100;
                line.Smoothing = currentTool == PaintTool.Brush ? toolSettingsPanel.SmoothingValue : 0;
            }
            return shape;
        }

        private Point ClampPoint(Point p)
        {
            int x = (int)(p.X / zoom), y = (int)(p.Y / zoom);
            return new Point(Math.Max(0, Math.Min(document.Width - 1, x)), Math.Max(0, Math.Min(document.Height - 1, y)));
        }

        private void RefreshCanvas()
        {
            if (currentTool == PaintTool.Bezier && pendingPoints.Count > 0)
                previewShape = BuildBezierPreview();
            else if (currentTool == PaintTool.Polygon && pendingPoints.Count > 0)
            {
                PolylineShape guide = NewShape(new PolylineShape()) as PolylineShape;
                guide.Thickness = 1;
                guide.Opacity = .7F;
                guide.Vertices.AddRange(pendingPoints); previewShape = guide;
                if (hasHoverPoint && hoverPoint != pendingPoints[pendingPoints.Count - 1]) guide.Vertices.Add(hoverPoint);
            }
            else if (pendingPoints.Count > 1)
            {
                PolylineShape guide = NewShape(new PolylineShape()) as PolylineShape;
                guide.Thickness = 1;
                guide.Opacity = .7F;
                guide.Vertices.AddRange(pendingPoints); previewShape = guide;
            }
            canvas.SelectedShape = selectedShape;
            EnsureDocumentCache();

            if (activeStrokeBitmap != null && previewShape is PolylineShape)
            {
                canvas.SetBitmap((Bitmap)activeStrokeBitmap.Clone());
                return;
            }
            if (previewShape == null)
            {
                canvas.SetBitmap((Bitmap)cachedDocumentBitmap.Clone());
                return;
            }

            Bitmap frame = (Bitmap)cachedDocumentBitmap.Clone();
            previewShape.Draw(frame);
            canvas.SetBitmap(frame);
        }

        private void EnsureDocumentCache()
        {
            if (cachedDocumentBitmap == null || documentDirty)
            {
                if (cachedDocumentBitmap != null) cachedDocumentBitmap.Dispose();
                cachedDocumentBitmap = document.Render();
                documentDirty = false;
            }
        }

        private DrawableShape BuildBezierPreview()
        {
            List<Point> points = new List<Point>(pendingPoints);
            if (hasHoverPoint && points.Count < 4) points.Add(hoverPoint);
            if (points.Count >= 4)
            {
                BezierShape curve = NewShape(new BezierShape()) as BezierShape;
                curve.ControlPoints.AddRange(points.Take(4));
                return curve;
            }
            PolylineShape guide = NewShape(new PolylineShape()) as PolylineShape;
            guide.Vertices.AddRange(points);
            return guide;
        }

        private void AdjustZoom(int change)
        {
            if (zoomLabel == null) return;
            int current = int.Parse(zoomLabel.Text.TrimEnd('%'));
            int next = Math.Max(50, Math.Min(150, current + change));
            if (next == current) return;
            zoom = next / 100F;
            canvas.Zoom = zoom;
            canvas.Size = new Size((int)(document.Width * zoom), (int)(document.Height * zoom));
            zoomLabel.Text = next + "%";
            status.Text = "Zoom: " + next + "%";
        }

        private void ShowLayerMenu(Control owner)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Traer al frente", null, (s, e) => BringSelectedToFront());
            menu.Items.Add("Eliminar seleccionada", null, (s, e) =>
            {
                if (selectedShape == null) { status.Text = "Selecciona una forma primero"; return; }
                documentController.Remove(selectedShape); selectedShape = null; canvas.SelectedShape = null;
                documentDirty = true;
                status.Text = "Capa eliminada"; RefreshCanvas(); UpdateLayers();
            });
            menu.Show(owner, new Point(0, owner.Height));
        }

        private void UpdateLayers()
        {
            layersPanel?.Refresh(document.Shapes, selectedShape);
        }

        private void PickColor(Color current, Action<Color> apply)
        {
            using (ColorDialog dlg = new ColorDialog { Color = current, FullOpen = true })
                if (dlg.ShowDialog() == DialogResult.OK && apply != null) apply(dlg.Color);
        }

        private void SaveImage(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg", FileName = "dibujo_proyectoPaint.png" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                using (Bitmap bmp = document.Render())
                    bmp.Save(dlg.FileName, Path.GetExtension(dlg.FileName).ToLower() == ".jpg" ? ImageFormat.Jpeg : ImageFormat.Png);
                status.Text = "Imagen exportada";
            }
        }

        private void SaveProject(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog { Filter = "Proyecto Paint (*.ppaint)|*.ppaint", FileName = "proyectoPaint.ppaint" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                ProjectStorage.Save(document, dlg.FileName);
                status.Text = "Proyecto guardado";
            }
        }

        private void LoadProject(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog { Filter = "Proyecto Paint (*.ppaint)|*.ppaint" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                CanvasDocument loaded = ProjectStorage.Load(dlg.FileName);
                documentController.Clear(); document.Shapes.AddRange(loaded.Shapes);
                document.Width = loaded.Width; document.Height = loaded.Height;
                document.BackgroundColor = loaded.BackgroundColor;
                canvas.Size = new Size(document.Width, document.Height);
                documentDirty = true;
                selectedShape = null; RefreshCanvas(); UpdateLayers();
                status.Text = "Proyecto abierto";
            }
        }
    }
}
