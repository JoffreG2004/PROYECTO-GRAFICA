using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    /// <summary>
    /// Dos tarjetas minimalistas: "Tamaño del pincel" y "Opacidad", cada una con
    /// un slider delgado y un valor en formato pill. Sin vista previa ni sliders
    /// extra: única fuente de grosor y opacidad de toda la app.
    /// </summary>
    public class ToolSettingsPanelControl : Panel
    {
        private ThemeSlider _sizeBar;
        private ThemeSlider _opacityBar;
        private PillLabel _sizePill;
        private PillLabel _opacityPill;
        private CheckBox _chkFill;

        private Rectangle _cardA;
        private Rectangle _cardB;

        private PaintTool _currentTool = PaintTool.Pencil;
        private Color _fillColor = ColorTranslator.FromHtml("#C89C74");
        private StrokeRenderStyle _strokeStyle = StrokeRenderStyle.Solid;
        private int _opacityPercent = 100;
        private readonly int _flowPercent = 80;
        private readonly int _smoothingValue = 60;
        private int _pencilSize = 2;
        private int _brushSize = 12;
        private bool _changingToolSize;

        public int BrushSize     => _sizeBar?.Value ?? 3;
        public bool FillShapes   => _chkFill?.Checked ?? true;
        public int OpacityPercent => _opacityPercent;
        public int FlowPercent    => _flowPercent;
        public int SmoothingValue => _smoothingValue;

        public event Action<int>  BrushSizeChanged;
        public event Action<int>  OpacityChanged;
        public event Action<int>  FlowChanged;        // se conserva por compatibilidad
        public event Action<int>  SmoothingChanged;   // se conserva por compatibilidad
        public event Action<bool> FillToggled;

        public ToolSettingsPanelControl()
        {
            DoubleBuffered = true;
            BackColor = ThemeColors.Background;
            Build();
            Resize += (s, e) => LayoutContent();
        }

        public void UpdateForTool(PaintTool tool, Color fillColor, StrokeRenderStyle strokeStyle)
        {
            _currentTool = tool;
            _fillColor   = fillColor;
            _strokeStyle = strokeStyle;
            if (_sizeBar != null)
            {
                _changingToolSize = true;
                int v = tool == PaintTool.Brush ? _brushSize : tool == PaintTool.Pencil ? _pencilSize : _sizeBar.Value;
                _sizeBar.Value = v;
                if (_sizePill != null) _sizePill.Text = v + " px";
                _changingToolSize = false;
            }
            if (_chkFill != null) _chkFill.Visible = ToolCanFill(tool);
            Invalidate();
        }

        public void SetFillChecked(bool value) { if (_chkFill != null) _chkFill.Checked = value; }

        public void SetThickness(int value)
        {
            if (_sizeBar == null) return;
            _changingToolSize = true;
            _sizeBar.Value = value;
            if (_sizePill != null) _sizePill.Text = _sizeBar.Value + " px";
            _changingToolSize = false;
        }

        public void InvalidatePreview() { Invalidate(); }

        private static bool ToolCanFill(PaintTool tool)
        {
            switch (tool)
            {
                case PaintTool.Rectangle:
                case PaintTool.RoundedRectangle:
                case PaintTool.Ellipse:
                case PaintTool.Polygon:
                case PaintTool.RegularPolygon:
                case PaintTool.Arrow:
                case PaintTool.Star:
                case PaintTool.Blob:
                    return true;
                default:
                    return false;
            }
        }

        private void Build()
        {
            _sizeBar = new ThemeSlider { Minimum = 1, Maximum = 100, Value = _pencilSize };
            _sizePill = new PillLabel { Text = _pencilSize + " px" };
            _sizeBar.ValueChanged += v =>
            {
                _sizePill.Text = v + " px";
                if (_changingToolSize) return;
                if (_currentTool == PaintTool.Brush) _brushSize = v;
                else if (_currentTool == PaintTool.Pencil) _pencilSize = v;
                BrushSizeChanged?.Invoke(v);
            };
            Controls.Add(_sizeBar); Controls.Add(_sizePill);

            _opacityBar = new ThemeSlider { Minimum = 0, Maximum = 100, Value = 100 };
            _opacityPill = new PillLabel { Text = "100 %" };
            _opacityBar.ValueChanged += v =>
            {
                _opacityPercent = v;
                _opacityPill.Text = v + " %";
                OpacityChanged?.Invoke(v);
            };
            Controls.Add(_opacityBar); Controls.Add(_opacityPill);

            _chkFill = new CheckBox { Text = "Rellenar formas", ForeColor = ThemeColors.TextPrimary, BackColor = ThemeColors.Panel, Checked = true, AutoSize = true, Font = new Font("Segoe UI", 8.5F), Visible = false };
            _chkFill.CheckedChanged += (s, e) => FillToggled?.Invoke(_chkFill.Checked);
            Controls.Add(_chkFill);

            LayoutContent();
        }

        private void LayoutContent()
        {
            if (_sizeBar == null) return;
            int w = Width;
            _cardA = new Rectangle(0, 0, w - 1, 96);
            _cardB = new Rectangle(0, _cardA.Bottom + 8, w - 1, 80);

            int pillW = 58, pillH = 28;
            // Tarjeta A: Tamaño del pincel
            _sizePill.SetBounds(_cardA.Right - pillW - 14, _cardA.Top + 38, pillW, pillH);
            _sizeBar.SetBounds(16, _cardA.Top + 41, _sizePill.Left - 28, 24);
            _chkFill.Location = new Point(16, _cardA.Top + 70);

            // Tarjeta B: Opacidad
            _opacityPill.SetBounds(_cardB.Right - pillW - 14, _cardB.Top + 36, pillW, pillH);
            _opacityBar.SetBounds(16, _cardB.Top + 39, _opacityPill.Left - 28, 24);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawCard(e.Graphics, _cardA, "Tamaño del pincel", StudioIcon.Pencil, false);
            DrawCard(e.Graphics, _cardB, "Opacidad", StudioIcon.None, true);
        }

        private void DrawCard(Graphics g, Rectangle card, string title, StudioIcon icon, bool droplet)
        {
            if (card.Width <= 1) return;
            using (GraphicsPath path = StudioCard.RoundedRect(card, 14))
            using (SolidBrush fill = new SolidBrush(ThemeColors.Panel))
            using (Pen pen = new Pen(ThemeColors.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
            Rectangle iconRect = new Rectangle(card.Left + 14, card.Top + 12, 18, 18);
            if (droplet) DrawDroplet(g, iconRect);
            else StudioToolButton.DrawIcon(g, iconRect, icon, ThemeColors.Icon);
            TextRenderer.DrawText(g, title, new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                new Rectangle(card.Left + 38, card.Top + 11, card.Width - 48, 20), ThemeColors.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private static void DrawDroplet(Graphics g, Rectangle r)
        {
            using (GraphicsPath path = new GraphicsPath())
            using (Pen pen = new Pen(ThemeColors.Icon, 1.8F))
            {
                int cx = r.Left + r.Width / 2;
                path.AddLine(cx, r.Top + 1, r.Right - 3, r.Bottom - 5);
                path.AddArc(r.Left + 2, r.Bottom - 12, r.Width - 4, 11, 0, 180);
                path.AddLine(r.Left + 3, r.Bottom - 5, cx, r.Top + 1);
                path.CloseFigure();
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, path);
            }
        }
    }
}
