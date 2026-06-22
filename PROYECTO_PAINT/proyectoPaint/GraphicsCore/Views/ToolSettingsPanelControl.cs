using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class ToolSettingsPanelControl : StudioCard
    {
        private Label _title;
        private Panel _preview;
        private TrackBar _sizeBar;
        private CheckBox _chkFill;
        private Label _algorithmInfo;
        private readonly List<Control> _brushOnlyControls = new List<Control>();
        private PaintTool _currentTool = PaintTool.Pencil;
        private Color _fillColor = Color.FromArgb(59, 130, 246);
        private StrokeRenderStyle _strokeStyle = StrokeRenderStyle.Solid;
        private int _opacityPercent = 100;
        private int _flowPercent = 80;
        private int _smoothingValue = 60;
        private int _pencilSize = 2;
        private int _brushSize = 12;
        private bool _changingToolSize;

        public int BrushSize => _sizeBar?.Value ?? 3;
        public bool FillShapes => _chkFill?.Checked ?? true;
        public int OpacityPercent => _opacityPercent;
        public int FlowPercent    => _flowPercent;
        public int SmoothingValue => _smoothingValue;

        public event Action<int>  OpacityChanged;
        public event Action<int>  FlowChanged;
        public event Action<int>  SmoothingChanged;
        public event Action<bool> FillToggled;

        public ToolSettingsPanelControl()
        {
            BackColor = Color.FromArgb(17, 27, 43);
            Build();
        }

        public void UpdateForTool(PaintTool tool, Color fillColor, StrokeRenderStyle strokeStyle)
        {
            _currentTool  = tool;
            _fillColor    = fillColor;
            _strokeStyle  = strokeStyle;
            if (_sizeBar != null)
            {
                _changingToolSize = true;
                _sizeBar.Value = tool == PaintTool.Brush ? _brushSize : tool == PaintTool.Pencil ? _pencilSize : _sizeBar.Value;
                _changingToolSize = false;
            }
            if (_title != null) _title.Text = ToolDisplayName(tool) + " - Ajustes";
            bool showBrushOnly = tool == PaintTool.Brush;
            foreach (Control c in _brushOnlyControls) c.Visible = showBrushOnly;
            if (_algorithmInfo != null)
            {
                _algorithmInfo.Visible = tool == PaintTool.Fill;
                _algorithmInfo.Text = "Algoritmo: Flood Fill por scanlines\n1. Parte de la semilla.\n2. Encuentra extremos de la franja.\n3. Propaga a filas vecinas hasta la frontera.";
            }
            _preview?.Invalidate();
        }

        public void SetFillChecked(bool value) { if (_chkFill != null) _chkFill.Checked = value; }

        public void SetThickness(int value)
        {
            if (_sizeBar == null) return;
            _sizeBar.Value = Math.Max(_sizeBar.Minimum, Math.Min(_sizeBar.Maximum, value));
        }

        public void InvalidatePreview() { _preview?.Invalidate(); }

        private void Build()
        {
            _title = new Label { Text = "Lápiz - Ajustes", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
            Controls.Add(_title);

            _preview = new Panel { Location = new Point(8, 32), Size = new Size(166, 38), BackColor = Color.FromArgb(11, 19, 32) };
            _preview.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawToolPreview(e.Graphics, _preview.ClientRectangle);
            };
            Controls.Add(_preview);

            Controls.Add(new Label { Text = "Tamaño", ForeColor = Color.FromArgb(175, 195, 225), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(10, 78) });
            _sizeBar = new TrackBar { Minimum = 1, Maximum = 100, Value = 3, TickStyle = TickStyle.None, Location = new Point(8, 92), Size = new Size(138, 28) };
            Label sizeVal = new Label { Text = "3", ForeColor = Color.FromArgb(210, 225, 245), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(152, 98) };
            _sizeBar.ValueChanged += delegate
            {
                sizeVal.Text = _sizeBar.Value.ToString();
                if (!_changingToolSize)
                {
                    if (_currentTool == PaintTool.Brush) _brushSize = _sizeBar.Value;
                    else if (_currentTool == PaintTool.Pencil) _pencilSize = _sizeBar.Value;
                }
            };
            Controls.Add(_sizeBar); Controls.Add(sizeVal);

            _chkFill = new CheckBox { Text = "Rellenar formas", ForeColor = Color.FromArgb(195, 212, 236), Checked = true, Location = new Point(10, 122), AutoSize = true, Font = new Font("Segoe UI", 8F) };
            _chkFill.CheckedChanged += (s, e) => FillToggled?.Invoke(_chkFill.Checked);
            Controls.Add(_chkFill);

            AddBrushSlider("Opacidad", 146, 100, false, v => { _opacityPercent  = v; OpacityChanged?.Invoke(v); });
            AddBrushSlider("Flujo",    184, 80,  true,  v => { _flowPercent     = v; FlowChanged?.Invoke(v); });
            AddBrushSlider("Suavizado",222, 60,  true,  v => { _smoothingValue  = v; SmoothingChanged?.Invoke(v); });

            _algorithmInfo = new Label { ForeColor = Color.FromArgb(170, 195, 225), Font = new Font("Segoe UI", 7.2F), AutoSize = false, Size = new Size(166, 52), Location = new Point(10, 250), Visible = false };
            Controls.Add(_algorithmInfo);
            Label moreLink = new Label { Text = "Más ajustes  v", ForeColor = Color.FromArgb(100, 130, 175), Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(10, 306), Cursor = Cursors.Hand };
            moreLink.Click += (s, e) => { if (_chkFill != null) { _chkFill.Checked = !_chkFill.Checked; } };
            Controls.Add(moreLink);
        }

        private void AddBrushSlider(string label, int top, int value, bool brushOnly, Action<int> changed)
        {
            Label lbl    = new Label { Text = label,       ForeColor = Color.FromArgb(170, 192, 222), Font = new Font("Segoe UI", 8F), AutoSize = false, Size = new Size(100, 18), Location = new Point(10, top) };
            Label valLbl = new Label { Text = value + "%", ForeColor = Color.FromArgb(165, 188, 220), Font = new Font("Segoe UI", 8F), AutoSize = true,  Location = new Point(142, top) };
            TrackBar bar = new TrackBar { Minimum = 0, Maximum = 100, Value = value, TickStyle = TickStyle.None, Location = new Point(8, top + 14), Size = new Size(138, 28) };
            bar.ValueChanged += delegate { valLbl.Text = bar.Value + "%"; changed?.Invoke(bar.Value); };
            Controls.Add(bar); Controls.Add(lbl); Controls.Add(valLbl);
            if (brushOnly)
            {
                lbl.Visible = false; bar.Visible = false; valLbl.Visible = false;
                _brushOnlyControls.Add(lbl); _brushOnlyControls.Add(bar); _brushOnlyControls.Add(valLbl);
            }
        }

        private void DrawToolPreview(Graphics g, Rectangle bounds)
        {
            g.Clear(Color.FromArgb(11, 19, 32));
            Rectangle r = new Rectangle(bounds.Left + 10, bounds.Top + 7, bounds.Width - 20, bounds.Height - 14);
            Color ink = Color.White;
            using (Pen pen = new Pen(ink, _currentTool == PaintTool.Brush ? 3.2F : 2F))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(115, _fillColor.R, _fillColor.G, _fillColor.B)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap   = LineCap.Round;

                if (_currentTool == PaintTool.Brush)
                    g.DrawBezier(pen, new PointF(r.Left, r.Bottom - 4), new PointF(r.Left + 45, r.Top - 2), new PointF(r.Right - 42, r.Bottom + 6), new PointF(r.Right, r.Top + 3));
                else if (_currentTool == PaintTool.Pencil)
                    g.DrawLine(pen, r.Left + 6, r.Bottom - 3, r.Right - 8, r.Top + 4);
                else if (_currentTool == PaintTool.Eraser)
                {
                    using (Pen faded = new Pen(Color.FromArgb(80, 170, 190, 225), 2F))
                        g.DrawLine(faded, r.Left, r.Bottom - 3, r.Right, r.Top + 5);
                    Point[] eraser = { new Point(r.Left + 46, r.Bottom - 3), new Point(r.Left + 64, r.Top + 2), new Point(r.Left + 82, r.Top + 14), new Point(r.Left + 64, r.Bottom + 2) };
                    using (SolidBrush eraserFill = new SolidBrush(Color.FromArgb(225, 235, 242, 255)))
                        g.FillPolygon(eraserFill, eraser);
                }
                else if (_currentTool == PaintTool.Fill)
                {
                    g.DrawLine(pen, r.Left + 38, r.Top + 2, r.Left + 86, r.Bottom - 2);
                    g.DrawLine(pen, r.Left + 38, r.Top + 2, r.Left + 52, r.Top);
                    g.FillEllipse(fill, r.Right - 36, r.Bottom - 13, 16, 16);
                }
                else if (_currentTool == PaintTool.Ellipse)
                {
                    g.FillEllipse(fill, r.Left + 35, r.Top, 76, r.Height);
                    g.DrawEllipse(pen, r.Left + 35, r.Top, 76, r.Height);
                }
                else if (_currentTool == PaintTool.Line)
                {
                    ApplyPreviewDash(pen);
                    g.DrawLine(pen, r.Left + 10, r.Bottom - 2, r.Right - 8, r.Top + 3);
                }
                else if (_currentTool == PaintTool.Polygon)
                {
                    Point[] pts = { new Point(r.Left + 72, r.Top), new Point(r.Right - 20, r.Bottom), new Point(r.Left + 24, r.Bottom - 2) };
                    g.FillPolygon(fill, pts);
                    g.DrawPolygon(pen, pts);
                }
                else if (_currentTool == PaintTool.RoundedRectangle)
                {
                    using (GraphicsPath path = StudioCard.RoundedRect(new Rectangle(r.Left + 30, r.Top, 86, r.Height), 10))
                    {
                        g.FillPath(fill, path);
                        g.DrawPath(pen, path);
                    }
                }
                else if (_currentTool == PaintTool.Arrow)
                {
                    pen.CustomEndCap = new AdjustableArrowCap(5, 6);
                    g.DrawLine(pen, r.Left + 18, r.Bottom - 2, r.Right - 18, r.Top + 2);
                }
                else if (_currentTool == PaintTool.Star)
                {
                    Point[] star = BuildPreviewStar(new Point(r.Left + r.Width / 2, r.Top + r.Height / 2), Math.Min(r.Width, r.Height) / 2);
                    g.FillPolygon(fill, star);
                    g.DrawPolygon(pen, star);
                }
                else if (_currentTool == PaintTool.Blob)
                {
                    Point[] blob =
                    {
                        new Point(r.Left + 32, r.Top + 13), new Point(r.Left + 66, r.Top),
                        new Point(r.Right - 26, r.Top + 8), new Point(r.Right - 18, r.Bottom - 4),
                        new Point(r.Left + 72, r.Bottom),   new Point(r.Left + 25, r.Bottom - 8)
                    };
                    g.FillPolygon(fill, blob);
                    g.DrawPolygon(pen, blob);
                }
                else
                {
                    StudioToolButton.DrawIcon(g, new Rectangle(r.Left + r.Width / 2 - 12, r.Top, 24, 24), IconForTool(_currentTool), ink);
                }
            }
        }

        private static Point[] BuildPreviewStar(Point center, int radius)
        {
            Point[] pts = new Point[10];
            double inner = radius * 0.45;
            for (int i = 0; i < pts.Length; i++)
            {
                double angle    = -Math.PI / 2 + i * Math.PI / 5;
                double distance = i % 2 == 0 ? radius : inner;
                pts[i] = new Point(center.X + (int)Math.Round(Math.Cos(angle) * distance), center.Y + (int)Math.Round(Math.Sin(angle) * distance));
            }
            return pts;
        }

        private void ApplyPreviewDash(Pen pen)
        {
            if (_strokeStyle == StrokeRenderStyle.Dashed) pen.DashPattern = new[] { 5F, 4F };
            if (_strokeStyle == StrokeRenderStyle.Dotted) pen.DashPattern = new[] { 1F, 4F };
        }

        private static StudioIcon IconForTool(PaintTool tool)
        {
            if (tool == PaintTool.Select)    return StudioIcon.Select;
            if (tool == PaintTool.Rectangle) return StudioIcon.Rectangle;
            if (tool == PaintTool.Bezier)    return StudioIcon.Curve;
            return StudioIcon.Pencil;
        }

        private static string ToolDisplayName(PaintTool tool)
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
    }
}
