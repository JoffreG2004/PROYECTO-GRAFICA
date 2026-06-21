using proyectoPaint.GraphicsCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private readonly Stack<DrawableShape> redoShapes = new Stack<DrawableShape>();
        private PaintCanvas canvas;
        private NumericUpDown thickness;
        private TrackBar brushSize;
        private Panel strokePreview;
        private Panel fillPreview;
        private Label status;
        private CheckBox chkFill;
        private Color strokeColor = Color.White;
        private Color fillColor = Color.FromArgb(103, 82, 255);
        private PaintTool currentTool = PaintTool.Pencil;
        private DrawableShape previewShape;
        private DrawableShape selectedShape;
        private Point startPoint;
        private Point lastPoint;
        private bool isDrawing;
        private bool isMovingSelection;

        public Form1()
        {
            InitializeComponent();
            document.Width = 760;
            document.Height = 520;
            BuildStudio();
            RefreshCanvas();
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
            AddCommand(bar, "Brush", StudioIcon.Brush, 692, delegate { ActivateTool(PaintTool.Pencil); });
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
            AddTool(list, "Brush", StudioIcon.Brush, PaintTool.Pencil);
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
            if (tool == PaintTool.Pencil && caption == "Brush") btn.Selected = true;
        }

        // ─── Right Inspector (two columns) ─────────────────────────────────────

        private Panel BuildInspector()
        {
            Panel right = new Panel { BackColor = Color.FromArgb(10, 16, 28) };
            Panel leftCol = new Panel { BackColor = Color.FromArgb(10, 16, 28) };
            Panel rightCol = new Panel { BackColor = Color.FromArgb(10, 16, 28) };

            StudioCard colorCard = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            BuildColorCard(colorCard);
            StudioCard layersCard = BuildLayersCard();
            StudioCard brushCard = BuildBrushCard();
            StudioCard shapeCard = BuildShapePanel();
            StudioCard propsCard = BuildPropertiesPanel();

            leftCol.Controls.Add(colorCard);
            leftCol.Controls.Add(layersCard);
            leftCol.Controls.Add(brushCard);
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
                brushCard.SetBounds(4, 410, cardWidth, Math.Max(202, right.Height - 416));
                shapeCard.SetBounds(4, 4, cardWidth, 216);
                propsCard.SetBounds(4, 226, cardWidth, Math.Max(330, right.Height - 232));
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
            wheel.ColorChanged += delegate { fillColor = wheel.SelectedColor; fillPreview.BackColor = fillColor; };
            card.Controls.Add(wheel);

            // Swatches / Gradients tabs
            Panel tabRow = new Panel { Location = new Point(8, 168), Size = new Size(166, 22), BackColor = Color.FromArgb(12, 20, 34) };
            Button swTab = new Button { Text = "Swatches", FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 46, 68), ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), Size = new Size(80, 22), Location = new Point(0, 0) };
            swTab.FlatAppearance.BorderSize = 0;
            Button grTab = new Button { Text = "Gradients", FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.FromArgb(130, 155, 188), Font = new Font("Segoe UI", 7.5F), Size = new Size(82, 22), Location = new Point(82, 0) };
            grTab.FlatAppearance.BorderSize = 0;
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
                p.Click += (s, e) => { fillColor = c; fillPreview.BackColor = c; };
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
            card.Controls.Add(addBtn); card.Controls.Add(moreBtn);

            var layers = new[] {
                ("T",  "Title",        true,  false),
                ("○",  "Shape 2",      false, false),
                ("△",  "Shape 1",      false, false),
                ("⬤", "Gradient Blob", false, false),
                ("▬",  "Background",   false, true ),
            };

            for (int i = 0; i < layers.Length; i++)
            {
                var (icon, name, active, locked) = layers[i];
                Panel row = new Panel
                {
                    Location = new Point(6, 34 + i * 28),
                    Size = new Size(170, 25),
                    BackColor = active ? Color.FromArgb(32, 50, 100) : Color.FromArgb(20, 32, 50),
                    Cursor = Cursors.Hand
                };
                row.Controls.Add(new Label { Text = icon, ForeColor = Color.FromArgb(160, 185, 220), Font = new Font("Segoe UI", 7.5F), AutoSize = false, Size = new Size(20, 20), Location = new Point(4, 3), TextAlign = ContentAlignment.MiddleCenter });
                row.Controls.Add(new Label { Text = name, AutoSize = true, Location = new Point(26, 5), ForeColor = active ? Color.White : Color.FromArgb(195, 212, 236), Font = new Font("Segoe UI", 7.5F) });
                if (locked) row.Controls.Add(new Label { Text = "🔒", ForeColor = Color.FromArgb(120, 148, 185), Font = new Font("Segoe UI", 6.5F), AutoSize = true, Location = new Point(126, 6) });
                row.Controls.Add(new Label { Text = "◉", ForeColor = Color.FromArgb(110, 140, 180), Font = new Font("Segoe UI", 7F), AutoSize = true, Location = new Point(150, 6), Cursor = Cursors.Hand });
                card.Controls.Add(row);
            }
            return card;
        }

        // ─── Brush card ────────────────────────────────────────────────────────

        private StudioCard BuildBrushCard()
        {
            StudioCard card = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            card.Controls.Add(new Label { Text = "Brush Settings", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            Button closeBtn = new Button { Text = "×", ForeColor = Color.FromArgb(140, 165, 200), Font = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(18, 18), Location = new Point(158, 8) };
            closeBtn.FlatAppearance.BorderSize = 0;
            card.Controls.Add(closeBtn);

            // Brush stroke preview
            Panel previewStrip = new Panel { Location = new Point(8, 32), Size = new Size(166, 36), BackColor = Color.FromArgb(11, 19, 32) };
            previewStrip.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.White, 3F) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    e.Graphics.DrawBezier(p, new PointF(10, 24), new PointF(52, 6), new PointF(112, 32), new PointF(158, 12));
            };
            card.Controls.Add(previewStrip);

            // Size row
            thickness = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 3, Visible = false };
            card.Controls.Add(new Label { Text = "Size", ForeColor = Color.FromArgb(175, 195, 225), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, 76) });
            brushSize = new TrackBar { Minimum = 1, Maximum = 100, Value = 3, TickStyle = TickStyle.None, Location = new Point(40, 70), Size = new Size(106, 26) };
            Label sizeVal = new Label { Text = "3", ForeColor = Color.FromArgb(210, 225, 245), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(148, 76) };
            brushSize.ValueChanged += delegate { thickness.Value = brushSize.Value; sizeVal.Text = brushSize.Value.ToString(); };
            thickness.ValueChanged += delegate { if (brushSize.Value != (int)thickness.Value) brushSize.Value = (int)thickness.Value; };
            card.Controls.Add(brushSize); card.Controls.Add(sizeVal);

            chkFill = new CheckBox { Text = "Fill shapes", ForeColor = Color.FromArgb(195, 212, 236), Checked = true, Location = new Point(10, 100), AutoSize = true, Font = new Font("Segoe UI", 7.5F) };
            card.Controls.Add(chkFill);

            AddBrushSlider(card, "Opacity",    122, 100);
            AddBrushSlider(card, "Flow",       148, 80);
            AddBrushSlider(card, "Smoothing",  174, 60);

            Label moreLink = new Label { Text = "More Settings  ∨", ForeColor = Color.FromArgb(100, 130, 175), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, 202), Cursor = Cursors.Hand };
            card.Controls.Add(moreLink);
            return card;
        }

        private void AddBrushSlider(Control card, string label, int top, int value)
        {
            card.Controls.Add(new Label { Text = label, ForeColor = Color.FromArgb(170, 192, 222), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, top) });
            TrackBar bar = new TrackBar { Minimum = 0, Maximum = 100, Value = value, TickStyle = TickStyle.None, Location = new Point(62, top - 4), Size = new Size(86, 26) };
            card.Controls.Add(bar);
            card.Controls.Add(new Label { Text = value + "%", ForeColor = Color.FromArgb(165, 188, 220), Font = new Font("Segoe UI", 7F), AutoSize = true, Location = new Point(150, top) });
        }

        // ─── Shape panel ───────────────────────────────────────────────────────

        private StudioCard BuildShapePanel()
        {
            StudioCard card = new StudioCard { BackColor = Color.FromArgb(17, 27, 43) };
            card.Controls.Add(new Label { Text = "Shape", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });
            Button closeBtn = new Button { Text = "×", ForeColor = Color.FromArgb(140, 165, 200), Font = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(18, 18), Location = new Point(158, 8) };
            closeBtn.FlatAppearance.BorderSize = 0;
            card.Controls.Add(closeBtn);

            var shapes = new (StudioIcon icon, PaintTool tool)[]
            {
                (StudioIcon.Rectangle,   PaintTool.Rectangle),
                (StudioIcon.RoundedRect, PaintTool.Rectangle),
                (StudioIcon.Ellipse,     PaintTool.Ellipse),
                (StudioIcon.Polygon,     PaintTool.Polygon),
                (StudioIcon.Line,        PaintTool.Line),
                (StudioIcon.Arrow,       PaintTool.Line),
                (StudioIcon.Star,        PaintTool.Polygon),
                (StudioIcon.Curve,       PaintTool.Bezier),
                (StudioIcon.Blob,        PaintTool.Rectangle),
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
                btn.Click += (s, e) => ActivateTool(shape.tool);
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
            Panel grad = new Panel { Location = new Point(10, y + 16), Size = new Size(160, 22), Cursor = Cursors.Hand };
            grad.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gb = new LinearGradientBrush(new Rectangle(0, 0, 161, 23), Color.FromArgb(88, 48, 230), Color.FromArgb(34, 211, 238), LinearGradientMode.Horizontal))
                using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, 159, 21), 8))
                    e.Graphics.FillPath(gb, path);
            };
            card.Controls.Add(grad);
            y += 46;

            // Stroke
            card.Controls.Add(new Label { Text = "Stroke", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            Panel strokeSw = new Panel { Location = new Point(10, y + 16), Size = new Size(20, 20), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            strokeSw.Click += BtnStrokeColor_Click;
            card.Controls.Add(strokeSw);
            card.Controls.Add(new Label { Text = "2 px", ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(36, y + 18) });
            Button sDrop = new Button { Text = "▾", ForeColor = Color.FromArgb(150, 175, 210), Font = new Font("Segoe UI", 8F), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(24, 38, 58), Size = new Size(26, 20), Location = new Point(132, y + 15) };
            sDrop.FlatAppearance.BorderSize = 0;
            card.Controls.Add(sDrop);
            y += 46;

            // Stroke Style
            card.Controls.Add(new Label { Text = "Stroke Style", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            string[] styleLabels = { "—", "- -", "···" };
            for (int i = 0; i < 3; i++)
            {
                Button sb = new Button { Text = styleLabels[i], ForeColor = Color.White, Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat, BackColor = i == 0 ? Color.FromArgb(44, 64, 98) : Color.FromArgb(22, 36, 54), Size = new Size(48, 22), Location = new Point(10 + i * 52, y + 16) };
                sb.FlatAppearance.BorderColor = Color.FromArgb(55, 80, 118); sb.FlatAppearance.BorderSize = 1;
                card.Controls.Add(sb);
            }
            y += 46;

            // Corner Radius
            card.Controls.Add(new Label { Text = "Corner Radius", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            TrackBar cr = new TrackBar { Minimum = 0, Maximum = 100, Value = 16, TickStyle = TickStyle.None, Location = new Point(10, y + 14), Size = new Size(120, 26) };
            Label crVal = new Label { Text = "16 px", ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(134, y + 18) };
            cr.ValueChanged += (s, e) => crVal.Text = cr.Value + " px";
            card.Controls.Add(cr); card.Controls.Add(crVal);
            y += 48;

            // Opacity
            card.Controls.Add(new Label { Text = "Opacity", ForeColor = Color.FromArgb(165, 188, 218), Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(10, y) });
            TrackBar op = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, Location = new Point(10, y + 14), Size = new Size(120, 26) };
            Label opVal = new Label { Text = "100%", ForeColor = Color.White, Font = new Font("Segoe UI", 7.5F), AutoSize = true, Location = new Point(134, y + 18) };
            op.ValueChanged += (s, e) => opVal.Text = op.Value + "%";
            card.Controls.Add(op); card.Controls.Add(opVal);

            Button rotate = PropertyButton("Rotate 15", 10, 258);
            Button scaleUp = PropertyButton("Scale +", 66, 258);
            Button scaleDown = PropertyButton("Scale -", 122, 258);
            Button clear = PropertyButton("Clear canvas", 10, 286);
            clear.Size = new Size(160, 24);
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

            footer.Controls.Add(new Label { Text = "Artboard 1  ▾", ForeColor = Color.FromArgb(215, 228, 244), AutoSize = true, Location = new Point(18, 12), Font = new Font("Segoe UI", 8.5F) });
            footer.Controls.Add(new Label { Text = "1200 × 720 px", ForeColor = Color.FromArgb(110, 138, 175), AutoSize = true, Location = new Point(130, 12), Font = new Font("Segoe UI", 8.5F) });

            Button gridBtn = new Button { Text = "⊞", ForeColor = Color.FromArgb(150, 178, 215), Font = new Font("Segoe UI", 12F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(26, 26), Location = new Point(274, 7) };
            gridBtn.FlatAppearance.BorderSize = 0;
            footer.Controls.Add(gridBtn);
            footer.Controls.Add(new Label { Text = "Grid", ForeColor = Color.FromArgb(110, 138, 175), AutoSize = true, Location = new Point(306, 12), Font = new Font("Segoe UI", 8.5F) });

            status = new Label { Text = "", ForeColor = Color.FromArgb(130, 158, 198), AutoSize = true, Location = new Point(390, 12), Font = new Font("Segoe UI", 8.5F) };
            footer.Controls.Add(status);

            // Zoom controls – anchored right
            Label zOut = new Label { Text = "−", ForeColor = Color.FromArgb(170, 195, 228), Font = new Font("Segoe UI", 12F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1140, 10), Cursor = Cursors.Hand };
            Label zPct = new Label { Text = "84%", ForeColor = Color.FromArgb(215, 228, 244), Font = new Font("Segoe UI", 8.5F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1166, 12) };
            Label zIn  = new Label { Text = "+", ForeColor = Color.FromArgb(170, 195, 228), Font = new Font("Segoe UI", 12F), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1202, 10), Cursor = Cursors.Hand };
            Label help = new Label { Text = "?", ForeColor = Color.FromArgb(140, 168, 210), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1248, 12), Cursor = Cursors.Hand };
            footer.Controls.Add(zOut); footer.Controls.Add(zPct); footer.Controls.Add(zIn); footer.Controls.Add(help);
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

        private Button PropertyButton(string text, int x, int y)
        {
            Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(52, 24), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 55, 86), ForeColor = Color.White, Font = new Font("Segoe UI", 7F) };
            button.FlatAppearance.BorderColor = Color.FromArgb(72, 103, 151);
            return button;
        }

        private void AddShape(DrawableShape shape)
        {
            document.Shapes.Add(shape);
            redoShapes.Clear();
        }

        private void ClearDocument()
        {
            document.Clear();
            redoShapes.Clear();
            pendingPoints.Clear();
            previewShape = null;
            selectedShape = null;
            canvas.SelectedShape = null;
            status.Text = "Canvas cleared";
            RefreshCanvas();
        }

        private void Undo()
        {
            if (document.Shapes.Count == 0) { status.Text = "Nothing to undo"; return; }
            DrawableShape shape = document.Shapes[document.Shapes.Count - 1];
            document.Shapes.RemoveAt(document.Shapes.Count - 1);
            redoShapes.Push(shape);
            selectedShape = null;
            status.Text = "Undo";
            RefreshCanvas();
        }

        private void Redo()
        {
            if (redoShapes.Count == 0) { status.Text = "Nothing to redo"; return; }
            document.Shapes.Add(redoShapes.Pop());
            status.Text = "Redo";
            RefreshCanvas();
        }

        private void BringSelectedToFront()
        {
            if (selectedShape == null) { status.Text = "Select a shape first"; return; }
            document.Shapes.Remove(selectedShape);
            document.Shapes.Add(selectedShape);
            status.Text = "Shape moved to front";
            RefreshCanvas();
        }

        private void TransformSelected(Action<DrawableShape> transform)
        {
            if (selectedShape == null) { status.Text = "Select a shape first"; return; }
            transform(selectedShape);
            status.Text = "Transformation applied";
            RefreshCanvas();
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
            currentTool = tool; pendingPoints.Clear(); previewShape = null; isDrawing = false;
            status.Text = "Tool: " + tool;
            RefreshCanvas();
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
                AddShape(new FloodFillShape { Seed = startPoint, FillColor = fillColor, StrokeColor = strokeColor, Thickness = (int)thickness.Value, UseFill = true });
                RefreshCanvas(); return;
            }
            if (currentTool == PaintTool.Select)
            {
                selectedShape = document.Shapes.LastOrDefault(s => s.HitTest(startPoint));
                canvas.SelectedShape = selectedShape; isMovingSelection = selectedShape != null;
                status.Text = selectedShape == null ? "No shape selected" : "Selected: " + selectedShape.Kind;
                RefreshCanvas(); return;
            }
            isDrawing = true;
            if (currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser)
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
                lastPoint = p; RefreshCanvas(); return;
            }
            if (!isDrawing) return;
            if (currentTool == PaintTool.Pencil || currentTool == PaintTool.Eraser)
                ((PolylineShape)previewShape).Vertices.Add(p);
            else
                previewShape = BuildDragShape(startPoint, p);
            RefreshCanvas();
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

        private void CanvasDoubleClick(object sender, EventArgs e) { FinishPendingShape(); }

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
                curve.ControlPoints.AddRange(pendingPoints.Take(4)); document.Shapes.Add(curve);
            }
            pendingPoints.Clear(); previewShape = null; RefreshCanvas();
        }

        private DrawableShape BuildDragShape(Point a, Point b)
        {
            if (currentTool == PaintTool.Line) return NewShape(new LineShape { Start = a, End = b });
            if (currentTool == PaintTool.Rectangle) return NewShape(new RectangleShape(a, b));
            if (currentTool == PaintTool.Ellipse) return NewShape(new EllipseShape { A = a, B = b });
            return null;
        }

        private DrawableShape NewShape(DrawableShape shape)
        {
            shape.StrokeColor = currentTool == PaintTool.Eraser ? document.BackgroundColor : strokeColor;
            shape.FillColor = fillColor;
            shape.Thickness = currentTool == PaintTool.Eraser ? Math.Max(10, (int)thickness.Value) : (int)thickness.Value;
            shape.UseFill = chkFill.Checked && currentTool != PaintTool.Line && currentTool != PaintTool.Pencil && currentTool != PaintTool.Eraser;
            return shape;
        }

        private Point ClampPoint(Point p)
        {
            return new Point(Math.Max(0, Math.Min(document.Width - 1, p.X)), Math.Max(0, Math.Min(document.Height - 1, p.Y)));
        }

        private void RefreshCanvas()
        {
            if (pendingPoints.Count > 1)
            {
                PolylineShape guide = NewShape(new PolylineShape()) as PolylineShape;
                guide.Vertices.AddRange(pendingPoints); previewShape = guide;
            }
            canvas.SelectedShape = selectedShape;
            canvas.SetBitmap(document.Render(previewShape));
        }

        private void BtnStrokeColor_Click(object sender, EventArgs e) { PickColor(ref strokeColor, strokePreview); }
        private void BtnFillColor_Click(object sender, EventArgs e) { PickColor(ref fillColor, fillPreview); }

        private void PickColor(ref Color target, Panel preview)
        {
            using (ColorDialog dlg = new ColorDialog { Color = target, FullOpen = true })
                if (dlg.ShowDialog() == DialogResult.OK) { target = dlg.Color; preview.BackColor = target; }
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
                document.Clear(); document.Shapes.AddRange(loaded.Shapes);
                document.Width = loaded.Width; document.Height = loaded.Height;
                document.BackgroundColor = loaded.BackgroundColor;
                canvas.Size = new Size(document.Width, document.Height);
                selectedShape = null; RefreshCanvas();
                status.Text = "Project opened";
            }
        }
    }
}
