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
        private Image headerLogo;
        private Point activeStrokeLastPoint;
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
            if (headerLogo != null) headerLogo.Dispose();
            base.OnFormClosed(e);
        }

        // ─── Layout ────────────────────────────────────────────────────────────

        private void BuildStudio()
        {
            Panel header = BuildHeader();
            Panel commandBar = BuildCommandBar();
            Panel footer = BuildFooter();

            TableLayoutPanel main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 14, 25),
                ColumnCount = 3, RowCount = 1,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 456));
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
            Controls.Add(commandBar);
            Controls.Add(header);
        }

        private Panel BuildHeader()
        {
            Panel header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(6, 11, 21) };

            Label brandMark = new Label { Text = "✦", ForeColor = Color.FromArgb(80, 210, 255), Font = new Font("Segoe UI", 25F), AutoSize = true, Location = new Point(18, 13) };
            Label brand = new Label { Text = "Paint ESPE", ForeColor = Color.FromArgb(244, 247, 255), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(82, 14) };
            Label edition = new Label { Text = "CREATE · LEARN · DRAW", ForeColor = Color.FromArgb(151, 169, 208), Font = new Font("Segoe UI", 7F, FontStyle.Bold), AutoSize = true, Location = new Point(76, 36) };
            Button drawNav = new Button { Text = "Dibujar", ForeColor = Color.White, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 64, 120), Font = new Font("Segoe UI Semibold", 8.5F), Size = new Size(70, 30), Location = new Point(235, 17) };
            Button labNav = new Button { Text = "Laboratorio", ForeColor = Color.FromArgb(204, 190, 255), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(20, 29, 51), Font = new Font("Segoe UI Semibold", 8.5F), Size = new Size(98, 30), Location = new Point(309, 17), Cursor = Cursors.Hand };
            drawNav.FlatAppearance.BorderColor = Color.FromArgb(105, 100, 255); labNav.FlatAppearance.BorderColor = Color.FromArgb(67, 83, 125);
            labNav.Click += (s, e) => { using (AlgorithmLabForm lab = new AlgorithmLabForm()) lab.ShowDialog(this); };
            brandMark.Visible = false;
            drawNav.Visible = false;
            labNav.Visible = false;

            PictureBox logo = new PictureBox { Location = new Point(25, 10), Size = new Size(46, 46), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent };
            string logoPath = Path.Combine(Application.StartupPath, "Assets", "paint-espe-logo-v2-key.png");
            if (File.Exists(logoPath))
            {
                using (Bitmap source = new Bitmap(logoPath))
                {
                    headerLogo = new Bitmap(source);
                    ((Bitmap)headerLogo).MakeTransparent(Color.FromArgb(0, 255, 0));
                }
                logo.Image = headerLogo;
            }

            StudioCard tab = new StudioCard { Location = new Point(286, 9), Size = new Size(198, 44), BackColor = Color.FromArgb(16, 27, 48) };
            Label tabTitle = new Label { Text = "Sin título", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 8.5F), AutoSize = true, Location = new Point(12, 7) };
            Label tabSaved = new Label { Text = "Guardado automáticamente", ForeColor = Color.FromArgb(120, 145, 178), Font = new Font("Segoe UI", 7F), AutoSize = true, Location = new Point(12, 22) };
            Button closeTab = new Button { Text = "×", ForeColor = Color.FromArgb(140, 160, 196), Font = new Font("Segoe UI", 11F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(22, 22), Location = new Point(165, 10), Cursor = Cursors.Hand };
            closeTab.FlatAppearance.BorderSize = 0;
            closeTab.Click += (s, e) => ClearDocument();
            tab.Controls.Add(tabTitle); tab.Controls.Add(tabSaved); tab.Controls.Add(closeTab);

            Button newTab = new Button { Text = "+", ForeColor = Color.FromArgb(170, 188, 220), Font = new Font("Segoe UI", 13F), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(13, 22, 38), Size = new Size(28, 28), Location = new Point(494, 17), Cursor = Cursors.Hand };
            newTab.FlatAppearance.BorderSize = 0;
            newTab.Click += (s, e) => ClearDocument();

            // "Pro" badge next to the brand name.
            Panel proBadge = new Panel { Size = new Size(38, 19), Location = new Point(193, 18), BackColor = Color.FromArgb(6, 11, 21), Cursor = Cursors.Default };
            proBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, proBadge.Width - 1, proBadge.Height - 1), 9))
                using (var b = new LinearGradientBrush(proBadge.ClientRectangle, Color.FromArgb(168, 85, 247), Color.FromArgb(99, 102, 241), 0F))
                    e.Graphics.FillPath(b, path);
                TextRenderer.DrawText(e.Graphics, "Pro", new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold), proBadge.ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            header.Controls.Add(logo); header.Controls.Add(brandMark); header.Controls.Add(brand); header.Controls.Add(edition); header.Controls.Add(proBadge); header.Controls.Add(drawNav); header.Controls.Add(labNav);

            // Right-side utilities: theme, notifications, avatar, account chevron.
            StudioGlyphButton themeBtn = new StudioGlyphButton { Icon = StudioIcon.ThemeSun, Size = new Size(30, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1740, 17) };
            StudioGlyphButton bellBtn = new StudioGlyphButton { Icon = StudioIcon.Bell, ShowDot = true, Size = new Size(30, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1774, 17) };
            new ToolTip().SetToolTip(themeBtn, "Tema");
            new ToolTip().SetToolTip(bellBtn, "Alertas");
            themeBtn.Click += (s, e) => { if (status != null) status.Text = "Tema claro / oscuro"; };
            bellBtn.Click += (s, e) => { if (status != null) status.Text = "Sin alertas nuevas"; };
            Panel profile = new Panel { BackColor = Color.FromArgb(6, 11, 21), Size = new Size(30, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1812, 17), Cursor = Cursors.Hand };
            profile.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var b = new LinearGradientBrush(profile.ClientRectangle, Color.FromArgb(230, 66, 201), Color.FromArgb(66, 124, 255), 35F)) e.Graphics.FillEllipse(b, 0, 0, profile.Width - 1, profile.Height - 1); };
            Label chevron = new Label { Text = "⌄", ForeColor = Color.FromArgb(150, 170, 205), Font = new Font("Segoe UI", 10F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1846, 19), Cursor = Cursors.Hand };

            header.Controls.Add(tab); header.Controls.Add(newTab); header.Controls.Add(themeBtn); header.Controls.Add(bellBtn); header.Controls.Add(profile); header.Controls.Add(chevron);
            header.Resize += (s, e) =>
            {
                int r = header.Width;
                chevron.Location = new Point(r - 26, 19);
                profile.Location = new Point(r - 56, 17);
                bellBtn.Location = new Point(r - 92, 17);
                themeBtn.Location = new Point(r - 126, 17);
            };
            return header;
        }

        private Panel BuildCommandBar()
        {
            Panel bar = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Color.FromArgb(8, 14, 25) };
            bar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var card = StudioCard.RoundedRect(new Rectangle(16, 4, Math.Max(0, bar.Width - 32), 68), 14))
                using (var fill = new SolidBrush(Color.FromArgb(15, 25, 42)))
                using (Pen sep = new Pen(Color.FromArgb(42, 58, 82)))
                {
                    e.Graphics.FillPath(fill, card);
                    e.Graphics.DrawPath(sep, card);
                    e.Graphics.DrawLine(sep, 264, 16, 264, 59);
                    e.Graphics.DrawLine(sep, 410, 16, 410, 59);
                    e.Graphics.DrawLine(sep, 618, 16, 618, 59);
                }
            };

            AddCommand(bar, "Nuevo",       StudioIcon.NewFile,   18,  delegate { ClearDocument(); });
            AddCommand(bar, "Abrir",       StudioIcon.Open,      78,  LoadProject);
            AddCommand(bar, "Guardar",     StudioIcon.Save,      138, SaveProject);
            AddCommand(bar, "Exportar",    StudioIcon.Export,    198, SaveImage);
            AddCommand(bar, "Deshacer",    StudioIcon.Undo,      284, delegate { Undo(); });
            AddCommand(bar, "Rehacer",     StudioIcon.Redo,      344, delegate { Redo(); });
            AddCommand(bar, "Seleccionar", StudioIcon.Select,    430, delegate { ActivateTool(PaintTool.Select); });
            AddCommand(bar, "Transformar", StudioIcon.Transform, 492, delegate { ActivateTool(PaintTool.Select); status.Text = "Selecciona una forma y usa Rotar o Escala en Propiedades."; });
            AddCommand(bar, "Ordenar",     StudioIcon.Arrange,   554, delegate { BringSelectedToFront(); });
            AddToolCommand(bar, "Pincel",  StudioIcon.Brush,     636, PaintTool.Brush);
            AddToolCommand(bar, "Forma",   StudioIcon.Rectangle, 696, PaintTool.Rectangle);
            AddCommand(bar, "Texto",       StudioIcon.Text,      756, delegate { status.Text = "Herramienta de texto (próximamente)"; });
            AddToolCommand(bar, "Relleno", StudioIcon.Fill,      816, PaintTool.Fill);
            AddCommand(bar, "Imagen",      StudioIcon.Image,     876, delegate { status.Text = "Insertar imagen (próximamente)"; });

            // Prominent Export button on the right of the command bar (matches the mockup).
            Panel exportBtn = new Panel { Size = new Size(134, 40), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1736, 18), Cursor = Cursors.Hand, BackColor = Color.FromArgb(8, 14, 25) };
            exportBtn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, exportBtn.Width - 1, exportBtn.Height - 1), 11))
                using (var b = new LinearGradientBrush(exportBtn.ClientRectangle, Color.FromArgb(139, 92, 246), Color.FromArgb(99, 102, 241), 0F))
                    e.Graphics.FillPath(b, path);
                TextRenderer.DrawText(e.Graphics, "Exportar", new Font("Segoe UI Semibold", 9.5F), new Rectangle(0, 0, exportBtn.Width - 24, exportBtn.Height), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255)))
                    e.Graphics.DrawLine(pen, exportBtn.Width - 26, 9, exportBtn.Width - 26, exportBtn.Height - 9);
                TextRenderer.DrawText(e.Graphics, "▾", new Font("Segoe UI", 8F), new Rectangle(exportBtn.Width - 24, 0, 22, exportBtn.Height), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            exportBtn.Click += SaveImage;
            bar.Controls.Add(exportBtn);
            bar.Resize += (s, e) => exportBtn.Location = new Point(bar.Width - 150, 18);

            return bar;
        }

        private void AddCommand(Control parent, string caption, StudioIcon icon, int left, EventHandler action)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Location = new Point(left, 10), Size = new Size(58, 54) };
            btn.Click += action;
            parent.Controls.Add(btn);
        }

        private void AddToolCommand(Control parent, string caption, StudioIcon icon, int left, PaintTool tool)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Tag = tool, Location = new Point(left, 10), Size = new Size(58, 54) };
            btn.Click += (s, e) => ActivateTool(tool);
            toolButtons.Add(btn);              // share the active-tool highlight (purple by default)
            parent.Controls.Add(btn);
        }

        private Panel BuildWorkspace()
        {
            Panel workspace = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 14, 25), Padding = new Padding(2, 0, 2, 0) };
            StudioCard artboard = new StudioCard { BackColor = Color.FromArgb(13, 22, 39), Padding = new Padding(9) };
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
                RefreshCanvas();
            };
            workspace.Resize += (s, e) => fitCanvas();
            workspace.HandleCreated += (s, e) => fitCanvas();
            return workspace;
        }

        // ─── Left toolbar ──────────────────────────────────────────────────────

        private Panel BuildToolbar()
        {
            Panel left = new Panel { Width = 126, BackColor = Color.FromArgb(8, 14, 25), Padding = new Padding(16, 12, 16, 12) };
            StudioCard tools = new StudioCard { Dock = DockStyle.Fill, BackColor = Color.FromArgb(17, 27, 43), Padding = new Padding(4) };
            FlowLayoutPanel list = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.FromArgb(17, 27, 43) };
            AddTool(list, "Selec.",   StudioIcon.Select,    PaintTool.Select);
            AddTool(list, "Pincel",   StudioIcon.Brush,     PaintTool.Brush);
            AddTool(list, "Lápiz",    StudioIcon.Pencil,    PaintTool.Pencil);
            AddTool(list, "Borrador", StudioIcon.Eraser,    PaintTool.Eraser);
            AddTool(list, "Forma",    StudioIcon.Rectangle, PaintTool.Rectangle);
            AddTool(list, "Elipse",   StudioIcon.Ellipse,   PaintTool.Ellipse);
            AddTool(list, "Relleno",  StudioIcon.Fill,      PaintTool.Fill);
            AddTool(list, "Polígono", StudioIcon.Polygon,   PaintTool.Polygon);
            AddTool(list, "Bézier",   StudioIcon.Curve,     PaintTool.Bezier);
            AddTool(list, "Línea",    StudioIcon.Line,      PaintTool.Line);
            tools.Controls.Add(list);
            left.Controls.Add(tools);
            return left;
        }

        private void AddTool(FlowLayoutPanel list, string caption, StudioIcon icon, PaintTool tool)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Tag = tool, Margin = new Padding(0, 2, 0, 2), Size = new Size(86, 57), SelectedFill = Color.FromArgb(37, 92, 196), SelectedBorder = Color.FromArgb(110, 168, 255) };
            btn.Click += ToolButton_Click;
            toolButtons.Add(btn);
            list.Controls.Add(btn);
            if (tool == currentTool) btn.Selected = true;
        }

        // ─── Right Inspector (dos columnas, cada card es un UserControl) ────────

        private Panel BuildInspector()
        {
            Panel right    = new Panel { BackColor = Color.FromArgb(10, 16, 28) };
            Panel leftCol  = new Panel { BackColor = Color.FromArgb(10, 16, 28) };
            Panel rightCol = new Panel { BackColor = Color.FromArgb(10, 16, 28) };

            colorPanel        = new ColorPanelControl(strokeColor, fillColor);
            layersPanel       = new LayersPanelControl();
            toolSettingsPanel = new ToolSettingsPanelControl();
            ShapePanelControl shapePanel = new ShapePanelControl();
            propertiesPanel   = new PropertiesPanelControl(strokeColor, fillColor, currentStrokeStyle, cornerRadius);

            // Color
            colorPanel.FillColorPickRequested   += () => PickColor(fillColor, SetFillColor);
            colorPanel.StrokeColorPickRequested += () => PickColor(strokeColor, SetStrokeColor);
            colorPanel.FillColorChanged         += color => SetFillColor(color);
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

            // Ajustes de herramienta
            toolSettingsPanel.OpacityChanged   += v => status.Text = "Opacidad: " + v + "%";
            toolSettingsPanel.FlowChanged      += v => status.Text = "Flujo del pincel: " + v + "%";
            toolSettingsPanel.SmoothingChanged += v => status.Text = "Suavizado del pincel: " + v + "%";
            toolSettingsPanel.FillToggled      += v => status.Text = v ? "Relleno de forma activado" : "Relleno de forma desactivado";
            toolSettingsPanel.UpdateForTool(currentTool, fillColor, currentStrokeStyle);

            // Selector de forma
            shapePanel.ToolSelected += tool => ActivateTool(tool);
            shapePanel.RegularPolygonSelected += sides => { regularPolygonSides = sides; ActivateTool(PaintTool.RegularPolygon); status.Text = "Polígono de " + sides + " lados: arrastra desde el centro."; };

            // Propiedades
            propertiesPanel.FillColorPickRequested   += () => PickColor(fillColor, SetFillColor);
            propertiesPanel.StrokeColorPickRequested += () => PickColor(strokeColor, SetStrokeColor);
            propertiesPanel.StrokeStyleChanged       += style => SetStrokeStyle(style);
            propertiesPanel.ThicknessChanged         += w => { toolSettingsPanel.SetThickness(w); ApplySelectedThickness(w); status.Text = "Borde: " + w + " px"; };
            propertiesPanel.CornerRadiusChanged      += v => { cornerRadius = v; ApplySelectedCornerRadius(v); status.Text = "Radio de esquina: " + v + " px"; };
            propertiesPanel.OpacityChanged           += v => { ApplySelectedOpacity(v / 100F); status.Text = "Opacidad: " + v + "%"; };
            propertiesPanel.RotateRequested          += () => TransformSelected(shape => shape.Rotate(15));
            propertiesPanel.ScaleUpRequested         += () => TransformSelected(shape => shape.Scale(1.1F));
            propertiesPanel.ScaleDownRequested       += () => TransformSelected(shape => shape.Scale(0.9F));
            propertiesPanel.ClearRequested           += () => ClearDocument();

            leftCol.Controls.Add(colorPanel);
            leftCol.Controls.Add(layersPanel);
            leftCol.Controls.Add(toolSettingsPanel);
            rightCol.Controls.Add(shapePanel);
            rightCol.Controls.Add(propertiesPanel);
            right.Controls.Add(leftCol);
            right.Controls.Add(rightCol);

            Action layout = () =>
            {
                int gap = 8;
                int leftWidth = Math.Max(238, (int)((right.Width - gap) * 0.60F));
                int rightWidth = Math.Max(166, right.Width - leftWidth - gap);
                leftCol.SetBounds(0, 0, leftWidth, right.Height);
                rightCol.SetBounds(leftWidth + gap, 0, rightWidth, right.Height);

                int leftCardWidth = Math.Max(120, leftWidth - 8);
                int rightCardWidth = Math.Max(120, rightWidth - 8);
                colorPanel.SetBounds(4, 4, leftCardWidth, 270);
                layersPanel.SetBounds(4, 282, leftCardWidth, 204);
                toolSettingsPanel.SetBounds(4, 494, leftCardWidth, Math.Max(320, right.Height - 500));
                shapePanel.SetBounds(4, 4, rightCardWidth, 240);
                propertiesPanel.SetBounds(4, 252, rightCardWidth, Math.Max(340, right.Height - 258));
            };
            right.Resize += (s, e) => layout();
            right.HandleCreated += (s, e) => layout();
            return right;
        }

        // ─── Footer ────────────────────────────────────────────────────────────

        private Panel BuildFooter()
        {
            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(9, 16, 28) };
            footer.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(38, 54, 76)), 0, 0, footer.Width, 0);

            Button artboardMenu = new Button { Text = "Artboard 1  v", ForeColor = Color.FromArgb(222, 230, 246), Font = new Font("Segoe UI Semibold", 8.5F), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(15, 25, 42), Size = new Size(132, 30), Location = new Point(16, 9), Cursor = Cursors.Hand };
            artboardMenu.FlatAppearance.BorderColor = Color.FromArgb(37, 54, 79);
            Label documentSize = new Label { Text = "1920 x 1080 px", ForeColor = Color.FromArgb(135, 156, 191), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(164, 17) };
            footer.Controls.Add(artboardMenu); footer.Controls.Add(documentSize);

            // Grid: icon + label + pill toggle (matches the mockup).
            Label gridIcon = new Label { Text = "▦", ForeColor = Color.FromArgb(150, 178, 215), Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(300, 13) };
            Label gridLabel = new Label { Text = "Cuadrícula", ForeColor = Color.FromArgb(150, 172, 205), AutoSize = true, Location = new Point(324, 16), Font = new Font("Segoe UI", 8.5F) };
            ToggleSwitch gridToggle = new ToggleSwitch { Location = new Point(400, 14) };
            gridToggle.Toggled += (s, e) => { gridVisible = gridToggle.On; canvas.ShowGrid = gridVisible; status.Text = gridVisible ? "Cuadrícula activada" : "Cuadrícula desactivada"; canvas.Invalidate(); };
            footer.Controls.Add(gridIcon); footer.Controls.Add(gridLabel); footer.Controls.Add(gridToggle);

            status = new Label { Text = "", ForeColor = Color.FromArgb(130, 158, 198), AutoSize = true, Location = new Point(470, 17), Font = new Font("Segoe UI", 8.5F) };
            footer.Controls.Add(status);

            Label zOut = new Label { Text = "−", ForeColor = Color.FromArgb(170, 195, 228), Font = new Font("Segoe UI", 12F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1140, 10), Cursor = Cursors.Hand };
            Label zPct = new Label { Text = "100%", ForeColor = Color.FromArgb(215, 228, 244), Font = new Font("Segoe UI", 8.5F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1166, 12) };
            Label zIn  = new Label { Text = "+", ForeColor = Color.FromArgb(170, 195, 228), Font = new Font("Segoe UI", 12F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1202, 10), Cursor = Cursors.Hand };
            Label help = new Label { Text = "?", ForeColor = Color.FromArgb(140, 168, 210), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1248, 12), Cursor = Cursors.Hand };
            footer.Controls.Add(zOut); footer.Controls.Add(zPct); footer.Controls.Add(zIn); footer.Controls.Add(help);
            zOut.Click += (s, e) => SetZoom(zPct, -10);
            zIn.Click  += (s, e) => SetZoom(zPct, 10);
            help.Click += (s, e) => MessageBox.Show("Selecciona una herramienta, dibuja en el lienzo y usa Seleccionar para mover o transformar formas.\n\nPolígono: haz clic en los puntos y doble clic para cerrar.\nBézier: haz clic en cuatro puntos.", "Ayuda - proyectoPaint", MessageBoxButtons.OK, MessageBoxIcon.Information);
            footer.Resize += (s, e) =>
            {
                int r = footer.Width;
                help.Location = new Point(r - 26, 12);
                zIn.Location  = new Point(r - 54, 10);
                zPct.Location = new Point(r - 88, 12);
                zOut.Location = new Point(r - 116, 10);
            };
            return footer;
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

        private void TransformSelected(Action<DrawableShape> transform)
        {
            if (selectedShape == null) { status.Text = "Selecciona una forma primero"; return; }
            transform(selectedShape);
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
            propertiesPanel.SetStrokeColor(color);
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
            propertiesPanel.SetFillColor(color);
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
            propertiesPanel.SetStrokeColor(strokeColor);
            propertiesPanel.SetFillColor(fillColor);
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
            if (currentTool == PaintTool.Fill)
            {
                // Un relleno del mismo color que el fondo parece que "no hace nada".
                // Elegimos un azul visible y lo reflejamos en los selectores.
                if (fillColor.ToArgb() == document.BackgroundColor.ToArgb())
                {
                    fillColor = Color.FromArgb(59, 130, 246);
                    colorPanel.SetFillColor(fillColor);
                    propertiesPanel.SetFillColor(fillColor);
                }
                FloodFillShape fill = NewShape(new FloodFillShape { Seed = startPoint }) as FloodFillShape;
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
            status.Text = "X " + p.X + "  Y " + p.Y + "  " + ToolDisplayName(currentTool);
            if (isMovingSelection && selectedShape != null)
            {
                selectedShape.Translate(p.X - lastPoint.X, p.Y - lastPoint.Y);
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

        private void SetZoom(Label label, int change)
        {
            int current = int.Parse(label.Text.TrimEnd('%'));
            int next = Math.Max(50, Math.Min(150, current + change));
            if (next == current) return;
            zoom = next / 100F;
            canvas.Zoom = zoom;
            canvas.Size = new Size((int)(document.Width * zoom), (int)(document.Height * zoom));
            label.Text = next + "%";
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
