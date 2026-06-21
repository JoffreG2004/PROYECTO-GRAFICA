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
        private NumericUpDown thickness;
        private TrackBar brushSize;
        private Panel strokePreview;
        private Panel fillPreview;
        private Panel propertyFillPreview;
        private Panel propertyStrokePreview;
        private Label status;
        private CheckBox chkFill;
        private Color strokeColor = Color.Black;
        private Color fillColor = Color.FromArgb(59, 130, 246);
        private StrokeRenderStyle currentStrokeStyle = StrokeRenderStyle.Solid;
        private int opacityPercent = 100;
        private int flowPercent = 80;
        private int smoothingValue = 60;
        private int cornerRadius = 16;
        private PaintTool currentTool = PaintTool.Pencil;
        private DrawableShape previewShape;
        private DrawableShape selectedShape;
        private Point startPoint;
        private Point lastPoint;
        private bool isDrawing;
        private bool isMovingSelection;
        private bool documentDirty = true;
        private bool hasHoverPoint;
        private bool gridVisible;
        private float zoom = 1F;
        private Point hoverPoint;
        private Bitmap cachedDocumentBitmap;
        private readonly Stopwatch repaintClock = Stopwatch.StartNew();
        private StudioCard layersCard;
        private StudioCard toolSettingsCard;
        private Label toolSettingsTitle;
        private Panel toolPreviewStrip;
        private readonly List<Button> strokeStyleButtons = new List<Button>();
        private readonly List<Control> brushOnlyControls = new List<Control>();

        public Form1()
        {
            InitializeComponent();
            document.Width = 760;
            document.Height = 520;
            documentController = new DocumentController(document);
            BuildStudio();
            RefreshCanvas();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (cachedDocumentBitmap != null) cachedDocumentBitmap.Dispose();
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
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 394));
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
            Panel header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(10, 18, 31) };

            Label brandMark = new Label { Text = "*", ForeColor = Color.FromArgb(137, 82, 255), Font = new Font("Segoe UI", 24F), AutoSize = true, Location = new Point(18, 8) };
            Label brand = new Label { Text = "proyectoPaint", ForeColor = Color.FromArgb(244, 247, 255), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(56, 17) };
            Label edition = new Label { Text = "STUDIO", ForeColor = Color.FromArgb(159, 130, 255), Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), AutoSize = true, Location = new Point(198, 23) };

            StudioCard tab = new StudioCard { Location = new Point(265, 7), Size = new Size(192, 42), BackColor = Color.FromArgb(22, 32, 48) };
            Label tabTitle = new Label { Text = "Untitled artwork", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 8.5F), AutoSize = true, Location = new Point(12, 7) };
            Label tabSaved = new Label { Text = "Autosaved just now", ForeColor = Color.FromArgb(120, 145, 178), Font = new Font("Segoe UI", 7F), AutoSize = true, Location = new Point(12, 22) };
            Button closeTab = new Button { Text = "×", ForeColor = Color.FromArgb(140, 160, 196), Font = new Font("Segoe UI", 11F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(22, 22), Location = new Point(165, 10), Cursor = Cursors.Hand };
            closeTab.FlatAppearance.BorderSize = 0;
            closeTab.Click += (s, e) => ClearDocument();
            tab.Controls.Add(tabTitle); tab.Controls.Add(tabSaved); tab.Controls.Add(closeTab);

            Button newTab = new Button { Text = "+", ForeColor = Color.FromArgb(140, 160, 196), Font = new Font("Segoe UI", 13F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(26, 26), Location = new Point(464, 15), Cursor = Cursors.Hand };
            newTab.FlatAppearance.BorderSize = 0;
            newTab.Click += (s, e) => ClearDocument();

            // Export button – top-right, purple accent
            Button exportBtn = new Button
            {
                Text = "Export  ▾",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(88, 48, 230),
                Size = new Size(108, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(1316, 10),
                Cursor = Cursors.Hand
            };
            exportBtn.FlatAppearance.BorderColor = Color.FromArgb(110, 70, 255);
            exportBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(105, 65, 245);
            exportBtn.Click += SaveImage;

            header.Controls.Add(brandMark); header.Controls.Add(brand); header.Controls.Add(edition);
            header.Controls.Add(tab); header.Controls.Add(newTab); header.Controls.Add(exportBtn);
            header.Resize += (s, e) => exportBtn.Location = new Point(header.Width - 122, 10);
            return header;
        }

        private Panel BuildCommandBar()
        {
            Panel bar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.FromArgb(15, 24, 38) };
            bar.Paint += (s, e) =>
            {
                using (Pen sep = new Pen(Color.FromArgb(42, 58, 82)))
                {
                    e.Graphics.DrawLine(sep, 0, bar.Height - 1, bar.Width, bar.Height - 1);
                    e.Graphics.DrawLine(sep, 280, 10, 280, 56);
                    e.Graphics.DrawLine(sep, 408, 10, 408, 56);
                    e.Graphics.DrawLine(sep, 680, 10, 680, 56);
                }
            };

            // File ops
            AddCommand(bar, "New", StudioIcon.NewFile, 8, delegate { ClearDocument(); });
            AddCommand(bar, "Open", StudioIcon.Open, 64, LoadProject);
            AddCommand(bar, "Save", StudioIcon.Save, 120, SaveProject);
            AddCommand(bar, "Export", StudioIcon.Export, 196, SaveImage);

            // Undo / Redo
            AddCommand(bar, "Undo", StudioIcon.Undo, 292, delegate { Undo(); });
            AddCommand(bar, "Redo", StudioIcon.Redo, 348, delegate { Redo(); });

            // Tool shortcuts
            AddCommand(bar, "Select", StudioIcon.Select, 420, delegate { ActivateTool(PaintTool.Select); });
            AddCommand(bar, "Transform", StudioIcon.Transform, 476, delegate { ActivateTool(PaintTool.Select); status.Text = "Select a shape, then use Rotate or Scale in Properties."; });
            AddCommand(bar, "Arrange", StudioIcon.Arrange, 532, delegate { BringSelectedToFront(); });
            AddCommand(bar, "Brush", StudioIcon.Brush, 692, delegate { ActivateTool(PaintTool.Brush); });
            AddCommand(bar, "Shape", StudioIcon.Rectangle, 748, delegate { ActivateTool(PaintTool.Rectangle); });
            AddCommand(bar, "Ellipse", StudioIcon.Ellipse, 804, delegate { ActivateTool(PaintTool.Ellipse); });
            AddCommand(bar, "Fill", StudioIcon.Fill, 860, delegate { ActivateTool(PaintTool.Fill); });
            AddCommand(bar, "Polygon", StudioIcon.Polygon, 916, delegate { ActivateTool(PaintTool.Polygon); });

            return bar;
        }

        private void AddCommand(Control parent, string caption, StudioIcon icon, int left, EventHandler action)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Location = new Point(left, 5), Size = new Size(56, 58) };
            btn.Click += action;
            parent.Controls.Add(btn);
        }

        private Panel BuildWorkspace()
        {
            Panel workspace = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 14, 25), AutoScroll = true, Padding = new Padding(28) };
            StudioCard artboard = new StudioCard { Location = new Point(34, 22), Size = new Size(document.Width + 18, document.Height + 18), BackColor = Color.FromArgb(13, 22, 39), Padding = new Padding(9) };
            canvas = new PaintCanvas { Location = new Point(9, 9), Size = new Size(document.Width, document.Height), BorderStyle = BorderStyle.None, BackColor = Color.White };
            canvas.MouseDown += CanvasMouseDown;
            canvas.MouseMove += CanvasMouseMove;
            canvas.MouseUp += CanvasMouseUp;
            canvas.DoubleClick += CanvasDoubleClick;
            artboard.Controls.Add(canvas);
            workspace.Controls.Add(artboard);
            return workspace;
        }

        // ─── Left toolbar ──────────────────────────────────────────────────────

        private Panel BuildToolbar()
        {
            Panel left = new Panel { Width = 90, BackColor = Color.FromArgb(8, 14, 25), Padding = new Padding(8, 10, 8, 10) };
            StudioCard tools = new StudioCard { Dock = DockStyle.Fill, BackColor = Color.FromArgb(17, 27, 43), Padding = new Padding(4) };
            FlowLayoutPanel list = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.FromArgb(17, 27, 43) };
            AddTool(list, "Select", StudioIcon.Select, PaintTool.Select);
            AddTool(list, "Brush", StudioIcon.Brush, PaintTool.Brush);
            AddTool(list, "Pencil", StudioIcon.Pencil, PaintTool.Pencil);
            AddTool(list, "Eraser", StudioIcon.Eraser, PaintTool.Eraser);
            AddTool(list, "Shape", StudioIcon.Rectangle, PaintTool.Rectangle);
            AddTool(list, "Ellipse", StudioIcon.Ellipse, PaintTool.Ellipse);
            AddTool(list, "Fill", StudioIcon.Fill, PaintTool.Fill);
            AddTool(list, "Polygon", StudioIcon.Polygon, PaintTool.Polygon);
            AddTool(list, "Bezier", StudioIcon.Curve, PaintTool.Bezier);
            AddTool(list, "Line", StudioIcon.Line, PaintTool.Line);
            tools.Controls.Add(list);
            left.Controls.Add(tools);
            return left;
        }

        private void AddTool(FlowLayoutPanel list, string caption, StudioIcon icon, PaintTool tool)
        {
            StudioToolButton btn = new StudioToolButton { Caption = caption, Icon = icon, Tag = tool, Margin = new Padding(0, 2, 0, 2), Size = new Size(64, 55) };
            btn.Click += ToolButton_Click;
            toolButtons.Add(btn);
            list.Controls.Add(btn);
            if (tool == currentTool) btn.Selected = true;
        }

        // ─── Right Inspector (two columns) ─────────────────────────────────────

        private Panel BuildInspector()
        {
            Panel right = new Panel { BackColor = Color.FromArgb(10, 16, 28) };
            Panel leftCol = new Panel { BackColor = Color.FromArgb(10, 16, 28) };
            Panel rightCol = new Panel { BackColor = Color.FromArgb(10, 16, 28) };

            StudioCard colorCard = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            BuildColorCard(colorCard);
            layersCard = BuildLayersCard();
            toolSettingsCard = BuildBrushCard();
            StudioCard shapeCard = BuildShapePanel();
            StudioCard propsCard = BuildPropertiesPanel();

            leftCol.Controls.Add(colorCard);
            leftCol.Controls.Add(layersCard);
            leftCol.Controls.Add(toolSettingsCard);
            rightCol.Controls.Add(shapeCard);
            rightCol.Controls.Add(propsCard);
            right.Controls.Add(leftCol);
            right.Controls.Add(rightCol);

            Action layout = () =>
            {
                int gap = 6;
                int columnWidth = Math.Max(185, (right.Width - gap) / 2);
                int cardWidth = columnWidth - 8;
                leftCol.SetBounds(0, 0, columnWidth, right.Height);
                rightCol.SetBounds(columnWidth + gap, 0, right.Width - columnWidth - gap, right.Height);

                colorCard.SetBounds(4, 4, cardWidth, 224);
                layersCard.SetBounds(4, 234, cardWidth, 170);
                toolSettingsCard.SetBounds(4, 410, cardWidth, Math.Max(254, right.Height - 416));
                shapeCard.SetBounds(4, 4, cardWidth, 216);
                propsCard.SetBounds(4, 226, cardWidth, Math.Max(390, right.Height - 232));
            };
            right.Resize += (s, e) => layout();
            right.HandleCreated += (s, e) => layout();
            return right;
        }

        private void BuildColorCard(StudioCard card)
        {
            card.Controls.Add(new Label { Text = "Color", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            strokePreview = AddColorTarget(card, "Stroke", 10, 34, strokeColor, BtnStrokeColor_Click);
            fillPreview   = AddColorTarget(card, "Fill",   98, 34, fillColor,   BtnFillColor_Click);

            ColorWheelControl wheel = new ColorWheelControl { Location = new Point(42, 64), Size = new Size(98, 98) };
            wheel.ColorChanged += delegate { SetFillColor(wheel.SelectedColor); };
            card.Controls.Add(wheel);

            // Swatches / Gradients tabs
            Panel tabRow = new Panel { Location = new Point(8, 168), Size = new Size(166, 22), BackColor = Color.FromArgb(12, 20, 34) };
            Button swTab = new Button { Text = "Swatches", FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 46, 68), ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), Size = new Size(80, 22), Location = new Point(0, 0) };
            swTab.FlatAppearance.BorderSize = 0;
            Button grTab = new Button { Text = "Gradients", FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.FromArgb(130, 155, 188), Font = new Font("Segoe UI", 7.5F), Size = new Size(82, 22), Location = new Point(82, 0) };
            grTab.FlatAppearance.BorderSize = 0;
            swTab.Click += (s, e) => { swTab.BackColor = Color.FromArgb(30, 46, 68); grTab.BackColor = Color.Transparent; status.Text = "Swatches selected"; };
            grTab.Click += (s, e) => { grTab.BackColor = Color.FromArgb(30, 46, 68); swTab.BackColor = Color.Transparent; SetFillColor(Color.FromArgb(34, 211, 238)); status.Text = "Gradient palette selected"; };
            tabRow.Controls.Add(swTab); tabRow.Controls.Add(grTab);
            card.Controls.Add(tabRow);

            // Swatch circles
            Color[] sw = { Color.FromArgb(139, 92, 246), Color.FromArgb(59, 130, 246), Color.FromArgb(34, 211, 238), Color.FromArgb(74, 222, 128), Color.FromArgb(250, 204, 21), Color.FromArgb(251, 113, 133), Color.White };
            for (int i = 0; i < sw.Length; i++)
            {
                Color c = sw[i];
                Panel p = new Panel { Location = new Point(8 + i * 23, 196), Size = new Size(18, 18), Cursor = Cursors.Hand };
                p.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, 17, 17), 9))
                    using (var b = new SolidBrush(c)) e.Graphics.FillPath(b, path);
                };
                p.Click += (s, e) => SetFillColor(c);
                card.Controls.Add(p);
            }
        }

        private Panel AddColorTarget(Control card, string label, int x, int y, Color color, EventHandler click)
        {
            Button btn = new Button { Text = label, Location = new Point(x, y), Size = new Size(54, 24), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(26, 40, 60), ForeColor = Color.FromArgb(215, 228, 246), Font = new Font("Segoe UI", 7.5F) };
            btn.FlatAppearance.BorderColor = Color.FromArgb(52, 74, 106); btn.Click += click;
            Panel preview = new Panel { Location = new Point(x + 58, y), Size = new Size(22, 24), BackColor = color, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            preview.Click += click;
            card.Controls.Add(btn); card.Controls.Add(preview);
            return preview;
        }

        // ─── Layers card ───────────────────────────────────────────────────────

        private StudioCard BuildLayersCard()
        {
            StudioCard card = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            card.Controls.Add(new Label { Text = "Layers", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            Button addBtn = new Button { Text = "+", ForeColor = Color.FromArgb(170, 192, 224), Font = new Font("Segoe UI", 12F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(20, 20), Location = new Point(134, 7) };
            addBtn.FlatAppearance.BorderSize = 0;
            Button moreBtn = new Button { Text = "···", ForeColor = Color.FromArgb(150, 175, 210), Font = new Font("Segoe UI", 7F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(24, 20), Location = new Point(154, 7) };
            moreBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += (s, e) => { ActivateTool(PaintTool.Rectangle); status.Text = "Draw a rectangle to add a new layer"; };
            moreBtn.Click += (s, e) => ShowLayerMenu(moreBtn);
            card.Controls.Add(addBtn); card.Controls.Add(moreBtn);
            return card;
        }

        // ─── Brush card ────────────────────────────────────────────────────────

        private StudioCard BuildBrushCard()
        {
            StudioCard card = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            toolSettingsTitle = new Label { Text = "Pencil Settings", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
            card.Controls.Add(toolSettingsTitle);

            Button closeBtn = new Button { Text = "×", ForeColor = Color.FromArgb(140, 165, 200), Font = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(18, 18), Location = new Point(158, 8) };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => { card.Visible = false; status.Text = "Tool settings hidden"; };
            card.Controls.Add(closeBtn);

            toolPreviewStrip = new Panel { Location = new Point(8, 32), Size = new Size(166, 38), BackColor = Color.FromArgb(11, 19, 32) };
            toolPreviewStrip.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawToolPreview(e.Graphics, toolPreviewStrip.ClientRectangle);
            };
            card.Controls.Add(toolPreviewStrip);

            // Size row
            thickness = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 3, Visible = false };
            Label sizeLabel = new Label { Text = "Size", ForeColor = Color.FromArgb(175, 195, 225), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(10, 78) };
            card.Controls.Add(sizeLabel);
            brushSize = new TrackBar { Minimum = 1, Maximum = 100, Value = 3, TickStyle = TickStyle.None, Location = new Point(8, 92), Size = new Size(138, 28) };
            Label sizeVal = new Label { Text = "3", ForeColor = Color.FromArgb(210, 225, 245), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(152, 98) };
            brushSize.ValueChanged += delegate { thickness.Value = brushSize.Value; sizeVal.Text = brushSize.Value.ToString(); };
            thickness.ValueChanged += delegate { if (brushSize.Value != (int)thickness.Value) brushSize.Value = (int)thickness.Value; };
            card.Controls.Add(brushSize); card.Controls.Add(sizeVal);

            chkFill = new CheckBox { Text = "Fill shapes", ForeColor = Color.FromArgb(195, 212, 236), Checked = true, Location = new Point(10, 122), AutoSize = true, Font = new Font("Segoe UI", 8F) };
            card.Controls.Add(chkFill);

            AddBrushSlider(card, "Opacity", 146, opacityPercent, delegate(int value)
            {
                opacityPercent = value;
                status.Text = "Opacity: " + value + "%";
            });
            AddBrushSlider(card, "Flow", 184, flowPercent, delegate(int value)
            {
                flowPercent = value;
                status.Text = "Brush flow: " + value + "%";
            }, true);
            AddBrushSlider(card, "Smoothing", 222, smoothingValue, delegate(int value)
            {
                smoothingValue = value;
                status.Text = "Brush smoothing: " + value + "%";
            }, true);

            Label moreLink = new Label { Text = "More Settings  v", ForeColor = Color.FromArgb(100, 130, 175), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(10, 250), Cursor = Cursors.Hand };
            moreLink.Click += (s, e) => { chkFill.Checked = !chkFill.Checked; status.Text = chkFill.Checked ? "Shape fill enabled" : "Shape fill disabled"; };
            card.Controls.Add(moreLink);
            UpdateToolSettingsTitle();
            return card;
        }

        private void AddBrushSlider(Control card, string label, int top, int value, Action<int> changed, bool brushOnly = false)
        {
            Label labelControl = new Label { Text = label, ForeColor = Color.FromArgb(170, 192, 222), Font = new Font("Segoe UI", 8F), AutoSize = false, Size = new Size(100, 18), Location = new Point(10, top) };
            card.Controls.Add(labelControl);
            Label valueLabel = new Label { Text = value + "%", ForeColor = Color.FromArgb(165, 188, 220), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(142, top) };
            TrackBar bar = new TrackBar { Minimum = 0, Maximum = 100, Value = value, TickStyle = TickStyle.None, Location = new Point(8, top + 14), Size = new Size(138, 28) };
            bar.ValueChanged += delegate
            {
                valueLabel.Text = bar.Value + "%";
                if (changed != null) changed(bar.Value);
            };
            card.Controls.Add(bar);
            card.Controls.Add(valueLabel);
            if (brushOnly)
            {
                brushOnlyControls.Add(labelControl);
                brushOnlyControls.Add(bar);
                brushOnlyControls.Add(valueLabel);
            }
        }

        private void DrawToolPreview(Graphics g, Rectangle bounds)
        {
            g.Clear(Color.FromArgb(11, 19, 32));
            Rectangle r = new Rectangle(bounds.Left + 10, bounds.Top + 7, bounds.Width - 20, bounds.Height - 14);
            Color ink = Color.White;
            using (Pen pen = new Pen(ink, currentTool == PaintTool.Brush ? 3.2F : 2F))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(115, fillColor.R, fillColor.G, fillColor.B)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                if (currentTool == PaintTool.Brush)
                {
                    g.DrawBezier(pen, new PointF(r.Left, r.Bottom - 4), new PointF(r.Left + 45, r.Top - 2), new PointF(r.Right - 42, r.Bottom + 6), new PointF(r.Right, r.Top + 3));
                }
                else if (currentTool == PaintTool.Pencil)
                {
                    g.DrawLine(pen, r.Left + 6, r.Bottom - 3, r.Right - 8, r.Top + 4);
                }
                else if (currentTool == PaintTool.Eraser)
                {
                    using (Pen faded = new Pen(Color.FromArgb(80, 170, 190, 225), 2F))
                        g.DrawLine(faded, r.Left, r.Bottom - 3, r.Right, r.Top + 5);
                    Point[] eraser =
                    {
                        new Point(r.Left + 46, r.Bottom - 3),
                        new Point(r.Left + 64, r.Top + 2),
                        new Point(r.Left + 82, r.Top + 14),
                        new Point(r.Left + 64, r.Bottom + 2)
                    };
                    using (SolidBrush eraserFill = new SolidBrush(Color.FromArgb(225, 235, 242, 255)))
                        g.FillPolygon(eraserFill, eraser);
                }
                else if (currentTool == PaintTool.Fill)
                {
                    g.DrawLine(pen, r.Left + 38, r.Top + 2, r.Left + 86, r.Bottom - 2);
                    g.DrawLine(pen, r.Left + 38, r.Top + 2, r.Left + 52, r.Top);
                    g.FillEllipse(fill, r.Right - 36, r.Bottom - 13, 16, 16);
                }
                else if (currentTool == PaintTool.Ellipse)
                {
                    g.FillEllipse(fill, r.Left + 35, r.Top, 76, r.Height);
                    g.DrawEllipse(pen, r.Left + 35, r.Top, 76, r.Height);
                }
                else if (currentTool == PaintTool.Line)
                {
                    ApplyPreviewDash(pen);
                    g.DrawLine(pen, r.Left + 10, r.Bottom - 2, r.Right - 8, r.Top + 3);
                }
                else if (currentTool == PaintTool.Polygon)
                {
                    Point[] pts = { new Point(r.Left + 72, r.Top), new Point(r.Right - 20, r.Bottom), new Point(r.Left + 24, r.Bottom - 2) };
                    g.FillPolygon(fill, pts);
                    g.DrawPolygon(pen, pts);
                }
                else if (currentTool == PaintTool.RoundedRectangle)
                {
                    using (GraphicsPath path = StudioCard.RoundedRect(new Rectangle(r.Left + 30, r.Top, 86, r.Height), 10))
                    {
                        g.FillPath(fill, path);
                        g.DrawPath(pen, path);
                    }
                }
                else if (currentTool == PaintTool.Arrow)
                {
                    pen.CustomEndCap = new AdjustableArrowCap(5, 6);
                    g.DrawLine(pen, r.Left + 18, r.Bottom - 2, r.Right - 18, r.Top + 2);
                }
                else if (currentTool == PaintTool.Star)
                {
                    Point[] star = BuildPreviewStar(new Point(r.Left + r.Width / 2, r.Top + r.Height / 2), Math.Min(r.Width, r.Height) / 2);
                    g.FillPolygon(fill, star);
                    g.DrawPolygon(pen, star);
                }
                else if (currentTool == PaintTool.Blob)
                {
                    Point[] blob =
                    {
                        new Point(r.Left + 32, r.Top + 13),
                        new Point(r.Left + 66, r.Top),
                        new Point(r.Right - 26, r.Top + 8),
                        new Point(r.Right - 18, r.Bottom - 4),
                        new Point(r.Left + 72, r.Bottom),
                        new Point(r.Left + 25, r.Bottom - 8)
                    };
                    g.FillPolygon(fill, blob);
                    g.DrawPolygon(pen, blob);
                }
                else
                {
                    StudioToolButton.DrawIcon(g, new Rectangle(r.Left + r.Width / 2 - 12, r.Top, 24, 24), IconForTool(currentTool), ink);
                }
            }
        }

        private static Point[] BuildPreviewStar(Point center, int radius)
        {
            Point[] pts = new Point[10];
            double inner = radius * 0.45;
            for (int i = 0; i < pts.Length; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                double distance = i % 2 == 0 ? radius : inner;
                pts[i] = new Point(center.X + (int)Math.Round(Math.Cos(angle) * distance), center.Y + (int)Math.Round(Math.Sin(angle) * distance));
            }
            return pts;
        }

        private void ApplyPreviewDash(Pen pen)
        {
            if (currentStrokeStyle == StrokeRenderStyle.Dashed) pen.DashPattern = new[] { 5F, 4F };
            if (currentStrokeStyle == StrokeRenderStyle.Dotted) pen.DashPattern = new[] { 1F, 4F };
        }

        private StudioIcon IconForTool(PaintTool tool)
        {
            if (tool == PaintTool.Select) return StudioIcon.Select;
            if (tool == PaintTool.Rectangle) return StudioIcon.Rectangle;
            if (tool == PaintTool.Bezier) return StudioIcon.Curve;
            return StudioIcon.Pencil;
        }

        // ─── Shape panel ───────────────────────────────────────────────────────

        private StudioCard BuildShapePanel()
        {
            StudioCard card = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            card.Controls.Add(new Label { Text = "Shape", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });
            Button closeBtn = new Button { Text = "×", ForeColor = Color.FromArgb(140, 165, 200), Font = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(18, 18), Location = new Point(158, 8) };
            closeBtn.FlatAppearance.BorderSize = 0;
            card.Controls.Add(closeBtn);
            closeBtn.Click += (s, e) => { card.Visible = false; status.Text = "Shape panel hidden"; };

            var shapes = new (StudioIcon icon, PaintTool tool)[]
            {
                (StudioIcon.Rectangle,   PaintTool.Rectangle),
                (StudioIcon.RoundedRect, PaintTool.RoundedRectangle),
                (StudioIcon.Ellipse,     PaintTool.Ellipse),
                (StudioIcon.Polygon,     PaintTool.Polygon),
                (StudioIcon.Line,        PaintTool.Line),
                (StudioIcon.Arrow,       PaintTool.Arrow),
                (StudioIcon.Star,        PaintTool.Star),
                (StudioIcon.Curve,       PaintTool.Bezier),
                (StudioIcon.Blob,        PaintTool.Blob),
            };

            for (int i = 0; i < shapes.Length; i++)
            {
                int col = i % 3, row = i / 3;
                var shape = shapes[i];
                StudioToolButton btn = new StudioToolButton
                {
                    Icon = shape.icon,
                    Caption = "",
                    Tag = shape.tool,
                    Location = new Point(8 + col * 58, 34 + row * 58),
                    Size = new Size(50, 50)
                };
                PaintTool selectedTool = shape.tool;
                btn.Click += (s, e) => ActivateTool(selectedTool);
                card.Controls.Add(btn);
            }
            return card;
        }

        // ─── Properties panel ──────────────────────────────────────────────────

        private StudioCard BuildPropertiesPanel()
        {
            StudioCard card = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            card.Controls.Add(new Label { Text = "Properties", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            int y = 32;

            // Fill
            card.Controls.Add(new Label { Text = "Fill", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            Button fillButton = new Button { Text = "Fill", ForeColor = Color.FromArgb(205, 220, 242), Font = new Font("Segoe UI", 7.5F), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(24, 38, 58), Size = new Size(70, 24), Location = new Point(10, y + 16), Cursor = Cursors.Hand };
            fillButton.FlatAppearance.BorderColor = Color.FromArgb(45, 70, 105);
            fillButton.FlatAppearance.BorderSize = 1;
            fillButton.Click += BtnFillColor_Click;
            propertyFillPreview = new Panel { Location = new Point(86, y + 16), Size = new Size(24, 24), BackColor = fillColor, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            propertyFillPreview.Click += BtnFillColor_Click;
            card.Controls.Add(fillButton);
            card.Controls.Add(propertyFillPreview);
            y += 46;

            // Stroke
            card.Controls.Add(new Label { Text = "Stroke", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            propertyStrokePreview = new Panel { Location = new Point(10, y + 16), Size = new Size(20, 20), BackColor = strokeColor, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            propertyStrokePreview.Click += BtnStrokeColor_Click;
            card.Controls.Add(propertyStrokePreview);
            card.Controls.Add(new Label { Text = "3 px", ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(36, y + 18) });
            Button sDrop = new Button { Text = "▾", ForeColor = Color.FromArgb(150, 175, 210), Font = new Font("Segoe UI", 8F), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(24, 38, 58), Size = new Size(26, 20), Location = new Point(128, y + 15) };
            sDrop.FlatAppearance.BorderSize = 0;
            sDrop.Click += (s, e) =>
            {
                ContextMenuStrip menu = new ContextMenuStrip();
                foreach (int value in new[] { 1, 2, 4, 8, 12 })
                {
                    int width = value;
                    menu.Items.Add(width + " px", null, (a, b) => { thickness.Value = width; ApplySelectedThickness(width); status.Text = "Stroke: " + width + " px"; });
                }
                menu.Show(sDrop, new Point(0, sDrop.Height));
            };
            card.Controls.Add(sDrop);
            y += 46;

            // Stroke Style
            card.Controls.Add(new Label { Text = "Stroke Style", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            string[] styleLabels = { "—", "- -", "···" };
            strokeStyleButtons.Clear();
            for (int i = 0; i < 3; i++)
            {
                Button sb = new Button { Text = styleLabels[i], ForeColor = Color.White, Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat, BackColor = i == 0 ? Color.FromArgb(44, 64, 98) : Color.FromArgb(22, 36, 54), Size = new Size(44, 22), Location = new Point(10 + i * 48, y + 16) };
                sb.FlatAppearance.BorderColor = Color.FromArgb(55, 80, 118); sb.FlatAppearance.BorderSize = 1;
                int styleIndex = i;
                sb.Click += (s, e) => SetStrokeStyle((StrokeRenderStyle)styleIndex);
                strokeStyleButtons.Add(sb);
                card.Controls.Add(sb);
            }
            y += 46;

            // Corner Radius
            card.Controls.Add(new Label { Text = "Corner Radius", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            TrackBar cr = new TrackBar { Minimum = 0, Maximum = 100, Value = 16, TickStyle = TickStyle.None, Location = new Point(10, y + 14), Size = new Size(108, 26) };
            Label crVal = new Label { Text = "16 px", ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(124, y + 18) };
            cr.ValueChanged += (s, e) => { cornerRadius = cr.Value; crVal.Text = cr.Value + " px"; ApplySelectedCornerRadius(cr.Value); status.Text = "Corner radius: " + cr.Value + " px"; };
            card.Controls.Add(cr); card.Controls.Add(crVal);
            y += 48;

            // Opacity
            y += 8;
            card.Controls.Add(new Label { Text = "Opacity", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = false, Size = new Size(100, 16), Location = new Point(10, y) });
            TrackBar op = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, Location = new Point(10, y + 14), Size = new Size(108, 26) };
            Label opVal = new Label { Text = "100%", ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(124, y + 18) };
            op.ValueChanged += (s, e) =>
            {
                opVal.Text = op.Value + "%";
                opacityPercent = op.Value;
                fillPreview.BackColor = Color.FromArgb(fillColor.R, fillColor.G, fillColor.B);
                ApplySelectedOpacity(op.Value / 100F);
                status.Text = "Opacity: " + op.Value + "%";
            };
            card.Controls.Add(op); card.Controls.Add(opVal);

            y += 66;

            Button rotate = PropertyButton("Rotate", 10, y, 70);
            Button scaleUp = PropertyButton("Scale +", 86, y, 70);
            y += 32;
            Button scaleDown = PropertyButton("Scale -", 10, y, 70);
            Button clear = PropertyButton("Clear", 86, y, 70);
            rotate.Click += (s, e) => TransformSelected(shape => shape.Rotate(15));
            scaleUp.Click += (s, e) => TransformSelected(shape => shape.Scale(1.1F));
            scaleDown.Click += (s, e) => TransformSelected(shape => shape.Scale(0.9F));
            clear.Click += (s, e) => ClearDocument();
            card.Controls.Add(rotate); card.Controls.Add(scaleUp); card.Controls.Add(scaleDown); card.Controls.Add(clear);

            return card;
        }

        // ─── Footer ────────────────────────────────────────────────────────────

        private Panel BuildFooter()
        {
            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(9, 16, 28) };
            footer.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(38, 54, 76)), 0, 0, footer.Width, 0);

            Button gridBtn = new Button { Text = "⊞", ForeColor = Color.FromArgb(150, 178, 215), Font = new Font("Segoe UI", 12F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(26, 26), Location = new Point(18, 7) };
            gridBtn.FlatAppearance.BorderSize = 0;
            gridBtn.Click += (s, e) => { gridVisible = !gridVisible; canvas.ShowGrid = gridVisible; status.Text = gridVisible ? "Grid enabled" : "Grid disabled"; canvas.Invalidate(); };
            footer.Controls.Add(gridBtn);
            footer.Controls.Add(new Label { Text = "Grid", ForeColor = Color.FromArgb(110, 138, 175), AutoSize = true, Location = new Point(50, 12), Font = new Font("Segoe UI", 8.5F) });

            status = new Label { Text = "", ForeColor = Color.FromArgb(130, 158, 198), AutoSize = true, Location = new Point(120, 12), Font = new Font("Segoe UI", 8.5F) };
            footer.Controls.Add(status);

            // Zoom controls – anchored right
            Label zOut = new Label { Text = "−", ForeColor = Color.FromArgb(170, 195, 228), Font = new Font("Segoe UI", 12F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1140, 10), Cursor = Cursors.Hand };
            Label zPct = new Label { Text = "100%", ForeColor = Color.FromArgb(215, 228, 244), Font = new Font("Segoe UI", 8.5F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1166, 12) };
            Label zIn  = new Label { Text = "+", ForeColor = Color.FromArgb(170, 195, 228), Font = new Font("Segoe UI", 12F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1202, 10), Cursor = Cursors.Hand };
            Label help = new Label { Text = "?", ForeColor = Color.FromArgb(140, 168, 210), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1248, 12), Cursor = Cursors.Hand };
            footer.Controls.Add(zOut); footer.Controls.Add(zPct); footer.Controls.Add(zIn); footer.Controls.Add(help);
            zOut.Click += (s, e) => SetZoom(zPct, -10);
            zIn.Click += (s, e) => SetZoom(zPct, 10);
            help.Click += (s, e) => MessageBox.Show("Select a tool, draw on the canvas, and use Select to move or transform shapes.\n\nPolygon: click points and double-click to finish.\nBezier: click four points.", "proyectoPaint help", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private Button PropertyButton(string text, int x, int y, int width = 52)
        {
            Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 24), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 55, 86), ForeColor = Color.White, Font = new Font("Segoe UI", 7F) };
            button.FlatAppearance.BorderColor = Color.FromArgb(72, 103, 151);
            return button;
        }

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
            status.Text = "Canvas cleared";
            RefreshCanvas();
        }

        private void Undo()
        {
            if (!documentController.Undo()) { status.Text = "Nothing to undo"; return; }
            selectedShape = null;
            documentDirty = true;
            status.Text = "Undo";
            RefreshCanvas();
            UpdateLayers();
        }

        private void Redo()
        {
            if (!documentController.Redo()) { status.Text = "Nothing to redo"; return; }
            documentDirty = true;
            status.Text = "Redo";
            RefreshCanvas();
            UpdateLayers();
        }

        private void BringSelectedToFront()
        {
            if (selectedShape == null) { status.Text = "Select a shape first"; return; }
            documentController.BringToFront(selectedShape);
            documentDirty = true;
            status.Text = "Shape moved to front";
            RefreshCanvas();
            UpdateLayers();
        }

        private void TransformSelected(Action<DrawableShape> transform)
        {
            if (selectedShape == null) { status.Text = "Select a shape first"; return; }
            transform(selectedShape);
            documentDirty = true;
            status.Text = "Transformation applied";
            RefreshCanvas();
        }

        private void SetStrokeStyle(StrokeRenderStyle style)
        {
            currentStrokeStyle = style;
            for (int i = 0; i < strokeStyleButtons.Count; i++)
                strokeStyleButtons[i].BackColor = i == (int)style ? Color.FromArgb(44, 64, 98) : Color.FromArgb(22, 36, 54);
            ApplySelectedShapeChange(shape => shape.StrokeStyle = style);
            if (toolPreviewStrip != null) toolPreviewStrip.Invalidate();
            status.Text = "Stroke style: " + style;
        }

        private void SetStrokeColor(Color color)
        {
            strokeColor = color;
            strokePreview.BackColor = color;
            if (propertyStrokePreview != null) propertyStrokePreview.BackColor = color;
            ApplySelectedShapeChange(shape =>
            {
                if (!(shape is FloodFillShape)) shape.StrokeColor = color;
            });
            if (toolPreviewStrip != null) toolPreviewStrip.Invalidate();
        }

        private void SetFillColor(Color color)
        {
            fillColor = color;
            fillPreview.BackColor = color;
            if (propertyFillPreview != null) propertyFillPreview.BackColor = color;
            ApplySelectedShapeChange(shape =>
            {
                if (CanUseFill(shape))
                {
                    shape.FillColor = color;
                    shape.UseFill = true;
                }
            });
            if (toolPreviewStrip != null) toolPreviewStrip.Invalidate();
        }

        private void ApplySelectedThickness(int value)
        {
            ApplySelectedShapeChange(shape => shape.Thickness = value);
        }

        private void ApplySelectedOpacity(float value)
        {
            ApplySelectedShapeChange(shape => shape.Opacity = value);
        }

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
            fillColor = selectedShape.FillColor;
            currentStrokeStyle = selectedShape.StrokeStyle;
            opacityPercent = (int)Math.Round(selectedShape.Opacity * 100F);
            chkFill.Checked = selectedShape.UseFill;
            if (strokePreview != null) strokePreview.BackColor = strokeColor;
            if (propertyStrokePreview != null) propertyStrokePreview.BackColor = strokeColor;
            if (fillPreview != null) fillPreview.BackColor = Color.FromArgb(fillColor.R, fillColor.G, fillColor.B);
            if (propertyFillPreview != null) propertyFillPreview.BackColor = Color.FromArgb(fillColor.R, fillColor.G, fillColor.B);
            if (thickness != null)
            {
                int value = Math.Max((int)thickness.Minimum, Math.Min((int)thickness.Maximum, selectedShape.Thickness));
                thickness.Value = value;
            }
            for (int i = 0; i < strokeStyleButtons.Count; i++)
                strokeStyleButtons[i].BackColor = i == (int)currentStrokeStyle ? Color.FromArgb(44, 64, 98) : Color.FromArgb(22, 36, 54);
            if (toolPreviewStrip != null) toolPreviewStrip.Invalidate();
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            StudioToolButton btn = sender as StudioToolButton;
            if (btn == null || !(btn.Tag is PaintTool)) return;
            ActivateTool((PaintTool)btn.Tag);
            SetActiveToolButton(btn);
        }

        private void ActivateTool(PaintTool tool)
        {
            currentTool = tool; pendingPoints.Clear(); previewShape = null; isDrawing = false; hasHoverPoint = false;
            StudioToolButton button = toolButtons.FirstOrDefault(b => b.Tag is PaintTool && (PaintTool)b.Tag == tool);
            if (button != null) SetActiveToolButton(button);
            UpdateToolSettingsTitle();
            status.Text = "Tool: " + ToolDisplayName(tool);
            RefreshCanvas();
        }

        private void UpdateToolSettingsTitle()
        {
            if (toolSettingsTitle == null) return;
            toolSettingsTitle.Text = ToolDisplayName(currentTool) + " Settings";
            bool showBrushOnly = currentTool == PaintTool.Brush;
            foreach (Control control in brushOnlyControls) control.Visible = showBrushOnly;
            if (toolPreviewStrip != null) toolPreviewStrip.Invalidate();
        }

        private string ToolDisplayName(PaintTool tool)
        {
            if (tool == PaintTool.RoundedRectangle) return "Rounded Rect";
            return tool.ToString();
        }

        private void SetActiveToolButton(StudioToolButton active)
        {
            foreach (StudioToolButton btn in toolButtons) btn.Selected = btn == active;
        }

        private void CanvasMouseDown(object sender, MouseEventArgs e)
        {
            startPoint = ClampPoint(e.Location); lastPoint = startPoint;
            if (currentTool == PaintTool.Fill)
            {
                AddShape(NewShape(new FloodFillShape { Seed = startPoint }) as FloodFillShape);
                RefreshCanvas(); return;
            }
            if (currentTool == PaintTool.Select)
            {
                selectedShape = document.Shapes.LastOrDefault(s => s.HitTest(startPoint));
                canvas.SelectedShape = selectedShape; isMovingSelection = selectedShape != null;
                if (selectedShape != null) SyncPropertiesFromSelection();
                status.Text = selectedShape == null ? "No shape selected" : "Selected: " + selectedShape.DisplayName;
                RefreshCanvas(); return;
            }
            if (currentTool == PaintTool.Bezier)
            {
                AddBezierPoint(startPoint);
                return;
            }
            isDrawing = true;
            if (currentTool == PaintTool.Brush || currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser)
            {
                PolylineShape line = NewShape(new PolylineShape()) as PolylineShape;
                line.Vertices.Add(startPoint); previewShape = line;
            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            Point p = ClampPoint(e.Location);
            status.Text = "X " + p.X + "  Y " + p.Y + "  " + currentTool;
            if (isMovingSelection && selectedShape != null)
            {
                selectedShape.Translate(p.X - lastPoint.X, p.Y - lastPoint.Y);
                documentDirty = true;
                lastPoint = p;
                if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
                return;
            }
            if (currentTool == PaintTool.Bezier && pendingPoints.Count > 0)
            {
                hoverPoint = p;
                hasHoverPoint = true;
                if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
                return;
            }
            if (!isDrawing) return;
            if (currentTool == PaintTool.Brush || currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser)
                ((PolylineShape)previewShape).Vertices.Add(p);
            else
                previewShape = BuildDragShape(startPoint, p);
            if (repaintClock.ElapsedMilliseconds >= 16) { RefreshCanvas(); repaintClock.Restart(); }
        }

        private void CanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (isMovingSelection) { isMovingSelection = false; RefreshCanvas(); return; }
            if (!isDrawing) return;
            isDrawing = false;
            Point end = ClampPoint(e.Location);
            if (currentTool == PaintTool.Polygon || currentTool == PaintTool.Bezier)
            {
                pendingPoints.Add(end);
                status.Text = currentTool == PaintTool.Polygon
                    ? "Polygon points: " + pendingPoints.Count + ". Double click to close."
                    : "Bezier points: " + pendingPoints.Count + "/4";
                if (currentTool == PaintTool.Bezier && pendingPoints.Count == 4) FinishPendingShape();
                return;
            }
            if (previewShape == null) previewShape = BuildDragShape(startPoint, end);
            if (previewShape != null) AddShape(previewShape);
            previewShape = null; RefreshCanvas();
        }

        private void CanvasDoubleClick(object sender, EventArgs e)
        {
            if (currentTool == PaintTool.Bezier) return;
            FinishPendingShape();
        }

        private void AddBezierPoint(Point point)
        {
            pendingPoints.Add(point);
            hasHoverPoint = false;
            status.Text = "Bezier point " + pendingPoints.Count + "/4";
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
            if (currentTool == PaintTool.Line) return NewShape(new LineShape { Start = a, End = b });
            if (currentTool == PaintTool.Rectangle) return NewShape(new RectangleShape(a, b));
            if (currentTool == PaintTool.RoundedRectangle) return NewShape(new RoundedRectangleShape(a, b, cornerRadius));
            if (currentTool == PaintTool.Ellipse) return NewShape(new EllipseShape { A = a, B = b });
            if (currentTool == PaintTool.Arrow) return NewShape(new ArrowShape(a, b, (int)thickness.Value));
            if (currentTool == PaintTool.Star) return NewShape(new StarShape(a, b));
            if (currentTool == PaintTool.Blob) return NewShape(new BlobShape(a, b));
            return null;
        }

        private DrawableShape NewShape(DrawableShape shape)
        {
            shape.StrokeColor = currentTool == PaintTool.Eraser ? document.BackgroundColor : strokeColor;
            shape.FillColor = fillColor;
            shape.Thickness = currentTool == PaintTool.Eraser ? Math.Max(10, (int)thickness.Value) : (int)thickness.Value;
            shape.StrokeStyle = currentStrokeStyle;
            shape.Opacity = opacityPercent / 100F;
            shape.UseFill = chkFill.Checked && CanUseFill(shape);
            if (currentTool == PaintTool.Eraser) shape.LayerName = "Borrador";
            else if (currentTool == PaintTool.Brush) shape.LayerName = "Pincel";
            else if (currentTool == PaintTool.Pencil) shape.LayerName = "Lapiz";
            if (shape is PolylineShape)
            {
                PolylineShape line = (PolylineShape)shape;
                line.Flow = currentTool == PaintTool.Brush ? flowPercent : 100;
                line.Smoothing = currentTool == PaintTool.Brush ? smoothingValue : 0;
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
            {
                previewShape = BuildBezierPreview();
            }
            else if (pendingPoints.Count > 1)
            {
                PolylineShape guide = NewShape(new PolylineShape()) as PolylineShape;
                guide.Vertices.AddRange(pendingPoints); previewShape = guide;
            }
            canvas.SelectedShape = selectedShape;
            if (cachedDocumentBitmap == null || documentDirty)
            {
                if (cachedDocumentBitmap != null) cachedDocumentBitmap.Dispose();
                cachedDocumentBitmap = document.Render();
                documentDirty = false;
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
            menu.Items.Add("Bring selected to front", null, (s, e) => BringSelectedToFront());
            menu.Items.Add("Delete selected", null, (s, e) =>
            {
                if (selectedShape == null) { status.Text = "Select a shape first"; return; }
                documentController.Remove(selectedShape); selectedShape = null; canvas.SelectedShape = null;
                documentDirty = true;
                status.Text = "Layer deleted"; RefreshCanvas(); UpdateLayers();
            });
            menu.Show(owner, new Point(0, owner.Height));
        }

        private void UpdateLayers()
        {
            if (layersCard == null || layersCard.IsDisposed) return;
            for (int i = layersCard.Controls.Count - 1; i >= 0; i--)
                if (layersCard.Controls[i].Tag as string == "layer-row") layersCard.Controls.RemoveAt(i);
            int count = Math.Min(5, document.Shapes.Count);
            for (int i = 0; i < count; i++)
            {
                DrawableShape shape = document.Shapes[document.Shapes.Count - 1 - i];
                Panel row = new Panel { Tag = "layer-row", Location = new Point(6, 34 + i * 28), Size = new Size(170, 25), BackColor = shape == selectedShape ? Color.FromArgb(32, 50, 100) : Color.FromArgb(20, 32, 50), Cursor = Cursors.Hand };
                Label name = new Label { Text = shape.DisplayName, Tag = shape, AutoSize = false, Size = new Size(135, 20), Location = new Point(8, 3), ForeColor = Color.FromArgb(205, 220, 242), Font = new Font("Segoe UI", 7.5F) };
                EventHandler choose = (s, e) => { selectedShape = shape; canvas.SelectedShape = shape; status.Text = "Selected: " + shape.DisplayName; SyncPropertiesFromSelection(); RefreshCanvas(); };
                row.Click += choose; name.Click += choose; row.Controls.Add(name); layersCard.Controls.Add(row);
            }
        }

        private void BtnStrokeColor_Click(object sender, EventArgs e) { PickColor(strokeColor, SetStrokeColor); }
        private void BtnFillColor_Click(object sender, EventArgs e) { PickColor(fillColor, SetFillColor); }

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
                status.Text = "Exported image";
            }
        }

        private void SaveProject(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog { Filter = "Proyecto Paint (*.ppaint)|*.ppaint", FileName = "proyectoPaint.ppaint" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                ProjectStorage.Save(document, dlg.FileName);
                status.Text = "Project saved";
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
                status.Text = "Project opened";
            }
        }
    }
}
