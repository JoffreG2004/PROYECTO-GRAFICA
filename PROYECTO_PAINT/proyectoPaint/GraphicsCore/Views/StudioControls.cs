using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public enum StudioIcon
    {
        None, NewFile, Open, Save, Export, Select, Brush, Pencil, Eraser, Fill,
        Line, Rectangle, Ellipse, Polygon, Curve, Rotate, ScaleUp, ScaleDown,
        Layers, Image, Zoom, Hand, Text, Undo, Redo, Arrange, Transform,
        RoundedRect, Arrow, Star, Blob
    }

    public class StudioCard : Panel
    {
        public StudioCard()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(21, 29, 43);
            Padding = new Padding(14);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 12))
            using (SolidBrush brush = new SolidBrush(BackColor))
            using (Pen pen = new Pen(Color.FromArgb(42, 58, 82), 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        public static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class StudioToolButton : Control
    {
        private bool hover;
        private bool selected;
        public StudioIcon Icon { get; set; }
        public string Caption { get; set; }
        public bool Selected
        {
            get { return selected; }
            set { selected = value; Invalidate(); }
        }

        public StudioToolButton()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(64, 70);
            Cursor = Cursors.Hand;
            ForeColor = Color.FromArgb(220, 229, 244);
            Font = new Font("Segoe UI", 7.5F);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color background = Parent == null ? Color.FromArgb(17, 27, 43) : Parent.BackColor;
            using (SolidBrush brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = selected ? Color.FromArgb(55, 72, 130) : (hover ? Color.FromArgb(35, 53, 74) : Color.Transparent);
            if (fill != Color.Transparent)
            {
                using (GraphicsPath path = StudioCard.RoundedRect(new Rectangle(2, 2, Width - 4, Height - 4), 10))
                using (SolidBrush brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
            }
            if (selected)
            {
                using (Pen accent = new Pen(Color.FromArgb(99, 130, 255), 2))
                using (GraphicsPath path = StudioCard.RoundedRect(new Rectangle(2, 2, Width - 4, Height - 4), 10))
                    e.Graphics.DrawPath(accent, path);
            }
            Color ink = selected ? Color.White : Color.FromArgb(200, 215, 238);
            int iconY = Caption == "" ? Height / 2 - 12 : 10;
            DrawIcon(e.Graphics, new Rectangle(Width / 2 - 12, iconY, 24, 24), Icon, ink);
            if (Caption != "")
                TextRenderer.DrawText(e.Graphics, Caption ?? Text, Font, new Rectangle(2, iconY + 26, Width - 4, 20), ink, TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }

        public static void DrawIcon(Graphics g, Rectangle r, StudioIcon icon, Color color)
        {
            using (Pen pen = new Pen(color, 1.8F))
            using (SolidBrush brush = new SolidBrush(color))
            {
                pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                int cx = r.Left + r.Width / 2, cy = r.Top + r.Height / 2;

                if (icon == StudioIcon.Select)
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(pen, r.Left + 3, r.Top + 3, r.Width - 6, r.Height - 6);
                }
                else if (icon == StudioIcon.Brush || icon == StudioIcon.Pencil)
                {
                    g.DrawLine(pen, r.Left + 5, r.Bottom - 5, r.Right - 5, r.Top + 5);
                    g.FillEllipse(brush, r.Left + 3, r.Bottom - 7, 5, 5);
                }
                else if (icon == StudioIcon.Eraser)
                {
                    g.FillPolygon(brush, new[] { new Point(r.Left + 5, r.Bottom - 8), new Point(r.Left + 13, r.Top + 4), new Point(r.Right - 4, r.Top + 12), new Point(r.Right - 12, r.Bottom - 4) });
                }
                else if (icon == StudioIcon.Fill)
                {
                    g.DrawLine(pen, r.Left + 7, r.Top + 7, r.Right - 6, r.Bottom - 7);
                    g.DrawLine(pen, r.Left + 7, r.Top + 7, r.Left + 13, r.Top + 4);
                    g.DrawLine(pen, r.Left + 7, r.Top + 7, r.Left + 4, r.Top + 13);
                    g.FillEllipse(brush, r.Right - 7, r.Bottom - 7, 4, 4);
                }
                else if (icon == StudioIcon.Line)
                {
                    g.DrawLine(pen, r.Left + 4, r.Bottom - 4, r.Right - 4, r.Top + 4);
                }
                else if (icon == StudioIcon.Rectangle)
                {
                    g.DrawRectangle(pen, r.Left + 4, r.Top + 5, r.Width - 8, r.Height - 10);
                }
                else if (icon == StudioIcon.RoundedRect)
                {
                    using (GraphicsPath rp = StudioCard.RoundedRect(new Rectangle(r.Left + 4, r.Top + 5, r.Width - 8, r.Height - 10), 5))
                        g.DrawPath(pen, rp);
                }
                else if (icon == StudioIcon.Ellipse)
                {
                    g.DrawEllipse(pen, r.Left + 4, r.Top + 5, r.Width - 8, r.Height - 10);
                }
                else if (icon == StudioIcon.Polygon)
                {
                    g.DrawPolygon(pen, new[] { new Point(cx, r.Top + 3), new Point(r.Right - 4, r.Bottom - 6), new Point(r.Left + 4, r.Bottom - 6) });
                }
                else if (icon == StudioIcon.Arrow)
                {
                    g.DrawLine(pen, r.Left + 4, r.Bottom - 5, r.Right - 5, r.Top + 5);
                    g.DrawLine(pen, r.Right - 5, r.Top + 5, r.Right - 12, r.Top + 6);
                    g.DrawLine(pen, r.Right - 5, r.Top + 5, r.Right - 6, r.Top + 12);
                }
                else if (icon == StudioIcon.Star)
                {
                    Point[] pts = new Point[10];
                    double outerR = 11, innerR = 5;
                    for (int i = 0; i < 10; i++)
                    {
                        double angle = i * Math.PI / 5 - Math.PI / 2;
                        double rad = (i % 2 == 0) ? outerR : innerR;
                        pts[i] = new Point(cx + (int)(rad * Math.Cos(angle)), cy + (int)(rad * Math.Sin(angle)));
                    }
                    g.DrawPolygon(pen, pts);
                }
                else if (icon == StudioIcon.Blob)
                {
                    g.DrawBezier(pen, new Point(cx, r.Top + 4), new Point(r.Right - 3, r.Top + 6), new Point(r.Right - 3, r.Bottom - 6), new Point(cx, r.Bottom - 4));
                    g.DrawBezier(pen, new Point(cx, r.Bottom - 4), new Point(r.Left + 5, r.Bottom - 5), new Point(r.Left + 5, r.Top + 7), new Point(cx, r.Top + 4));
                }
                else if (icon == StudioIcon.Curve)
                {
                    g.DrawBezier(pen, r.Left + 3, r.Bottom - 5, r.Left + 8, r.Top + 2, r.Right - 7, r.Bottom + 2, r.Right - 3, r.Top + 5);
                }
                else if (icon == StudioIcon.Text)
                {
                    using (Font tf = new Font("Segoe UI", 13F, FontStyle.Bold))
                        TextRenderer.DrawText(g, "T", tf, r, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                else if (icon == StudioIcon.Image)
                {
                    g.DrawRectangle(pen, r.Left + 3, r.Top + 4, r.Width - 6, r.Height - 8);
                    g.DrawLine(pen, r.Left + 5, r.Bottom - 7, r.Left + 11, r.Top + 10);
                    g.DrawLine(pen, r.Left + 11, r.Top + 10, r.Right - 5, r.Bottom - 7);
                    g.FillEllipse(brush, r.Right - 10, r.Top + 7, 5, 5);
                }
                else if (icon == StudioIcon.Undo)
                {
                    g.DrawArc(pen, r.Left + 5, r.Top + 5, 14, 14, 100, 220);
                    g.FillPolygon(brush, new[] { new Point(r.Left + 3, r.Top + 9), new Point(r.Left + 9, r.Top + 5), new Point(r.Left + 9, r.Top + 13) });
                }
                else if (icon == StudioIcon.Redo)
                {
                    g.DrawArc(pen, r.Left + 5, r.Top + 5, 14, 14, 280, 220);
                    g.FillPolygon(brush, new[] { new Point(r.Right - 3, r.Top + 9), new Point(r.Right - 9, r.Top + 5), new Point(r.Right - 9, r.Top + 13) });
                }
                else if (icon == StudioIcon.Transform)
                {
                    g.DrawRectangle(pen, r.Left + 5, r.Top + 5, r.Width - 10, r.Height - 10);
                    int hs = 3;
                    g.FillRectangle(brush, r.Left + 3, r.Top + 3, hs * 2, hs * 2);
                    g.FillRectangle(brush, r.Right - 3 - hs * 2, r.Top + 3, hs * 2, hs * 2);
                    g.FillRectangle(brush, r.Left + 3, r.Bottom - 3 - hs * 2, hs * 2, hs * 2);
                    g.FillRectangle(brush, r.Right - 3 - hs * 2, r.Bottom - 3 - hs * 2, hs * 2, hs * 2);
                    g.FillRectangle(brush, cx - hs, r.Top + 3, hs * 2, hs * 2);
                    g.FillRectangle(brush, cx - hs, r.Bottom - 3 - hs * 2, hs * 2, hs * 2);
                }
                else if (icon == StudioIcon.Arrange)
                {
                    g.DrawRectangle(pen, r.Left + 5, r.Top + 9, 14, 9);
                    g.DrawLine(pen, r.Left + 3, r.Top + 7, r.Right - 3, r.Top + 7);
                    g.DrawLine(pen, r.Left + 1, r.Top + 5, r.Right - 1, r.Top + 5);
                }
                else if (icon == StudioIcon.Hand)
                {
                    g.DrawLine(pen, cx, r.Bottom - 4, cx, r.Top + 9);
                    g.DrawLine(pen, cx - 4, r.Bottom - 4, cx - 4, r.Top + 7);
                    g.DrawLine(pen, cx + 4, r.Bottom - 4, cx + 4, r.Top + 10);
                    g.DrawArc(pen, cx - 7, r.Bottom - 10, 14, 8, 180, 180);
                }
                else if (icon == StudioIcon.NewFile)
                {
                    g.DrawRectangle(pen, r.Left + 6, r.Top + 3, 12, 18);
                    g.DrawLine(pen, r.Left + 9, cy, r.Right - 5, cy);
                    g.DrawLine(pen, cx, r.Top + 8, cx, r.Bottom - 5);
                }
                else if (icon == StudioIcon.Open)
                {
                    g.DrawLines(pen, new[] { new Point(r.Left + 3, r.Bottom - 6), new Point(r.Left + 6, r.Top + 9), new Point(r.Left + 12, r.Top + 9), new Point(r.Left + 15, r.Top + 6), new Point(r.Right - 3, r.Bottom - 6), new Point(r.Left + 3, r.Bottom - 6) });
                }
                else if (icon == StudioIcon.Save || icon == StudioIcon.Export)
                {
                    g.DrawRectangle(pen, r.Left + 5, r.Top + 3, 14, 18);
                    g.DrawRectangle(pen, r.Left + 8, r.Top + 5, 7, 5);
                    g.DrawLine(pen, r.Left + 8, r.Bottom - 7, r.Right - 7, r.Bottom - 7);
                }
                else if (icon == StudioIcon.Rotate)
                {
                    g.DrawArc(pen, r.Left + 4, r.Top + 4, 16, 16, 35, 290);
                    g.FillPolygon(brush, new[] { new Point(r.Right - 3, r.Top + 8), new Point(r.Right - 9, r.Top + 7), new Point(r.Right - 6, r.Top + 13) });
                }
                else if (icon == StudioIcon.Zoom)
                {
                    g.DrawEllipse(pen, r.Left + 4, r.Top + 4, 12, 12);
                    g.DrawLine(pen, r.Left + 15, r.Top + 15, r.Right - 3, r.Bottom - 3);
                }
                else
                {
                    g.FillEllipse(brush, cx - 3, cy - 3, 6, 6);
                }
            }
        }
    }

    public class ColorWheelControl : Control
    {
        private Bitmap wheel;
        public Color SelectedColor { get; private set; } = Color.FromArgb(103, 82, 255);
        public event EventHandler ColorChanged;

        public ColorWheelControl() { DoubleBuffered = true; Size = new Size(150, 150); Cursor = Cursors.Cross; }

        protected override void OnResize(EventArgs e) { base.OnResize(e); if (wheel != null) wheel.Dispose(); wheel = null; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 8 || Height < 8) return;
            int side = Math.Min(Width, Height);
            if (wheel == null) wheel = BuildWheel(side);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int ox = (Width - side) / 2, oy = (Height - side) / 2;
            g.DrawImage(wheel, ox, oy, side, side);

            // Gradient square inside
            float innerR = side * 0.40F;
            int sq = (int)(innerR * 1.28F);
            int sqX = ox + (side - sq) / 2, sqY = oy + (side - sq) / 2;
            double[] hsv = ToHsv(SelectedColor);
            Color hueColor = FromHsv(hsv[0], 1.0, 1.0);

            using (var hGrad = new LinearGradientBrush(new Rectangle(sqX, sqY, sq + 1, sq + 1), Color.White, hueColor, LinearGradientMode.Horizontal))
                g.FillRectangle(hGrad, sqX, sqY, sq, sq);
            using (var vGrad = new LinearGradientBrush(new Rectangle(sqX, sqY, sq + 1, sq + 1), Color.Transparent, Color.Black, LinearGradientMode.Vertical))
                g.FillRectangle(vGrad, sqX, sqY, sq, sq);

            // Selection marker
            int selX = sqX + (int)(Math.Max(0, Math.Min(1, hsv[1])) * sq);
            int selY = sqY + (int)((1.0 - Math.Max(0, Math.Min(1, hsv[2]))) * sq);
            using (Pen wp = new Pen(Color.White, 1.5F)) g.DrawEllipse(wp, selX - 5, selY - 5, 10, 10);
            using (Pen bp = new Pen(Color.Black, 0.8F)) g.DrawEllipse(bp, selX - 6, selY - 6, 12, 12);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            PickFromClick(e.X, e.Y);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) PickFromClick(e.X, e.Y);
            base.OnMouseMove(e);
        }

        private void PickFromClick(int mx, int my)
        {
            int side = Math.Min(Width, Height);
            int ox = (Width - side) / 2, oy = (Height - side) / 2;
            float innerR = side * 0.40F;
            int sq = (int)(innerR * 1.28F);
            int sqX = ox + (side - sq) / 2, sqY = oy + (side - sq) / 2;

            // Click in gradient square?
            if (mx >= sqX && mx <= sqX + sq && my >= sqY && my <= sqY + sq)
            {
                double[] hsv = ToHsv(SelectedColor);
                double sat = Math.Max(0, Math.Min(1, (double)(mx - sqX) / sq));
                double val = Math.Max(0, Math.Min(1, 1.0 - (double)(my - sqY) / sq));
                SelectedColor = FromHsv(hsv[0], sat, val);
                ColorChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
                return;
            }

            // Click in hue ring?
            float cx = Width / 2F, cy = Height / 2F;
            float dx = mx - cx, dy = my - cy;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist > innerR && dist < side / 2F)
            {
                double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                if (angle < 0) angle += 360;
                double[] hsv = ToHsv(SelectedColor);
                SelectedColor = FromHsv(angle, hsv[1], hsv[2]);
                ColorChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        private static Bitmap BuildWheel(int side)
        {
            Bitmap bmp = new Bitmap(side, side);
            float c = side / 2F, outer = c - 2, inner = c * 0.40F;
            for (int y = 0; y < side; y++)
                for (int x = 0; x < side; x++)
                {
                    float dx = x - c, dy = y - c, d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d >= inner && d <= outer)
                    {
                        double h = Math.Atan2(dy, dx) * 180 / Math.PI;
                        if (h < 0) h += 360;
                        bmp.SetPixel(x, y, FromHsv(h, 0.9, 1.0));
                    }
                    else bmp.SetPixel(x, y, Color.Transparent);
                }
            return bmp;
        }

        private static double[] ToHsv(Color color)
        {
            float r = color.R / 255F, gr = color.G / 255F, b = color.B / 255F;
            float max = Math.Max(r, Math.Max(gr, b)), min = Math.Min(r, Math.Min(gr, b));
            double h = 0, s = 0, v = max;
            float d = max - min;
            if (max != 0) s = d / max;
            if (d != 0)
            {
                if (max == r) h = (gr - b) / d + (gr < b ? 6 : 0);
                else if (max == gr) h = (b - r) / d + 2;
                else h = (r - gr) / d + 4;
                h *= 60;
            }
            return new double[] { h, s, v };
        }

        private static Color FromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
            if (hi == 0) return Color.FromArgb(255, v, t, p);
            if (hi == 1) return Color.FromArgb(255, q, v, p);
            if (hi == 2) return Color.FromArgb(255, p, v, t);
            if (hi == 3) return Color.FromArgb(255, p, q, v);
            if (hi == 4) return Color.FromArgb(255, t, p, v);
            return Color.FromArgb(255, v, p, q);
        }
    }
}
