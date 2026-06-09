using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace REPRODUCTOR_MUSICAL.Views
{
    public class ShuffleToggle : CheckBox
    {
        private bool isHovered;
        private bool isPressed;

        public ShuffleToggle()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            Appearance = Appearance.Button;
            AutoSize = false;
            Cursor = Cursors.Hand;
            ForeColor = Color.FromArgb(109, 240, 214);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            Size = new Size(52, 38);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var borderColor = Checked ? Color.FromArgb(41, 221, 218) : Color.FromArgb(74, 126, 214);
            var startColor = Checked ? Color.FromArgb(18, 104, 111) : Color.FromArgb(16, 45, 72);
            var endColor = Checked ? Color.FromArgb(13, 58, 75) : Color.FromArgb(18, 34, 58);
            var iconColor = Checked ? Color.White : Color.FromArgb(226, 241, 255);

            using (var path = CreateRoundedRectangle(rect, 10))
            using (var brush = new LinearGradientBrush(rect, startColor, endColor, LinearGradientMode.Vertical))
            using (var topBrush = new LinearGradientBrush(rect, Color.FromArgb(isPressed ? 16 : 38, Color.White), Color.Transparent, LinearGradientMode.Vertical))
            using (var borderPen = new Pen(borderColor, Checked || isHovered ? 1.7f : 1.2f))
            {
                pevent.Graphics.Clear(ResolveSurfaceColor());
                pevent.Graphics.FillPath(brush, path);
                pevent.Graphics.FillPath(topBrush, path);
                pevent.Graphics.DrawPath(borderPen, path);
                DrawShuffleIcon(pevent.Graphics, rect, iconColor, borderColor);
            }
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            isHovered = false;
            isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            isPressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnCheckedChanged(System.EventArgs e)
        {
            Invalidate();
            base.OnCheckedChanged(e);
        }

        private static void DrawShuffleIcon(System.Drawing.Graphics graphics, Rectangle rect, Color iconColor, Color glowColor)
        {
            var iconSize = Math.Min(rect.Width - 18, rect.Height - 10);
            var scale = iconSize / 24f;
            var offsetX = rect.Left + (rect.Width - iconSize) / 2f;
            var offsetY = rect.Top + (rect.Height - iconSize) / 2f;

            using (var iconPath = CreateLucideShufflePath())
            using (var transform = new Matrix())
            using (var glowPen = new Pen(Color.FromArgb(72, glowColor), 4.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var pen = new Pen(iconColor, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                transform.Translate(offsetX, offsetY);
                transform.Scale(scale, scale);
                iconPath.Transform(transform);
                glowPen.Width *= scale;
                pen.Width *= scale;

                graphics.DrawPath(glowPen, iconPath);
                graphics.DrawPath(pen, iconPath);
            }
        }

        private static GraphicsPath CreateLucideShufflePath()
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddLines(new[]
            {
                new PointF(18, 14),
                new PointF(22, 18),
                new PointF(18, 22)
            });

            path.StartFigure();
            path.AddLines(new[]
            {
                new PointF(18, 2),
                new PointF(22, 6),
                new PointF(18, 10)
            });

            path.StartFigure();
            path.AddLine(2, 18, 3.973f, 18);
            path.AddBezier(3.973f, 18, 5.35f, 18, 6.6f, 17.35f, 7.273f, 16.3f);
            path.AddLine(7.273f, 16.3f, 12.727f, 7.7f);
            path.AddBezier(12.727f, 7.7f, 13.45f, 6.58f, 14.72f, 6, 16.027f, 6);
            path.AddLine(16.027f, 6, 22, 6);

            path.StartFigure();
            path.AddLine(2, 6, 3.972f, 6);
            path.AddBezier(3.972f, 6, 5.35f, 6, 6.75f, 6.88f, 7.572f, 8.2f);

            path.StartFigure();
            path.AddLine(22, 18, 15.959f, 18);
            path.AddBezier(15.959f, 18, 14.55f, 18, 13.3f, 17.32f, 12.659f, 16.2f);
            path.AddLine(12.659f, 16.2f, 12.3f, 15.75f);

            return path;
        }

        private Color ResolveSurfaceColor()
        {
            var parent = Parent;
            while (parent != null && parent.BackColor == Color.Transparent)
            {
                parent = parent.Parent;
            }

            return parent == null || parent.BackColor == Color.Empty
                ? Color.FromArgb(12, 22, 39)
                : parent.BackColor;
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
