using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace REPRODUCTOR_MUSICAL.Views
{
    public enum ButtonIconKind
    {
        None,
        Upload,
        Play,
        Pause,
        Stop
    }

    public class GradientButton : Button
    {
        private bool isHovered;
        private bool isPressed;
        private bool isActive;

        public GradientButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            StartColor = Color.FromArgb(41, 221, 218);
            EndColor = Color.FromArgb(18, 113, 185);
            BorderColor = Color.FromArgb(55, 225, 238);
            BorderRadius = 10;
            ForeColor = Color.White;
            IconColor = Color.White;
            IconKind = ButtonIconKind.None;
        }

        public Color StartColor { get; set; }

        public Color EndColor { get; set; }

        public Color BorderColor { get; set; }

        public Color IconColor { get; set; }

        public int BorderRadius { get; set; }

        public ButtonIconKind IconKind { get; set; }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value)
                {
                    return;
                }

                isActive = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var glowAlpha = IsActive ? 190 : isHovered ? 120 : 70;
            var glowWidth = IsActive ? 3.2f : isHovered ? 2f : 1.4f;
            var iconGlowAlpha = IsActive ? 88 : 42;

            using (var path = CreateRoundedRectangle(rect, BorderRadius))
            using (var brush = new LinearGradientBrush(rect, Adjust(StartColor, 1.05f), EndColor, LinearGradientMode.Vertical))
            using (var pen = new Pen(BorderColor, IsActive ? 1.4f : 1f))
            using (var glowPen = new Pen(Color.FromArgb(glowAlpha, BorderColor), glowWidth))
            using (var topBrush = new LinearGradientBrush(rect, Color.FromArgb(isPressed ? 18 : IsActive ? 72 : 46, Color.White), Color.Transparent, LinearGradientMode.Vertical))
            {
                pevent.Graphics.Clear(ResolveSurfaceColor());
                if (isHovered || IsActive)
                {
                    using (var glowBrush = new SolidBrush(Color.FromArgb(IsActive ? 44 : 26, BorderColor)))
                    {
                        pevent.Graphics.FillPath(glowBrush, path);
                    }
                }

                pevent.Graphics.FillPath(brush, path);
                pevent.Graphics.FillPath(topBrush, path);
                pevent.Graphics.DrawPath(glowPen, path);
                pevent.Graphics.DrawPath(pen, path);
                DrawIcon(pevent.Graphics, rect, iconGlowAlpha);
                DrawButtonText(pevent.Graphics, rect);
            }
        }

        private void DrawButtonText(System.Drawing.Graphics graphics, Rectangle rect)
        {
            var textRect = IconKind == ButtonIconKind.Upload
                ? new Rectangle(rect.Left + 64, rect.Top + 1, rect.Width - 88, rect.Height - 2)
                : new Rectangle(rect.Left + 4, rect.Top + 35, rect.Width - 8, rect.Height - 38);

            if (IconKind == ButtonIconKind.None)
            {
                textRect = rect;
            }

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                textRect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DrawIcon(System.Drawing.Graphics graphics, Rectangle rect, int glowAlpha)
        {
            if (IconKind == ButtonIconKind.None)
            {
                return;
            }

            var center = IconKind == ButtonIconKind.Upload
                ? new PointF(rect.Left + 40, rect.Top + rect.Height / 2f)
                : new PointF(rect.Left + rect.Width / 2f, rect.Top + 24);

            using (var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, IconColor)))
            using (var brush = new SolidBrush(IconColor))
            using (var pen = new Pen(IconColor, IconKind == ButtonIconKind.Upload ? 2.5f : 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                graphics.FillEllipse(glowBrush, center.X - 18, center.Y - 18, 36, 36);

                if (IconKind == ButtonIconKind.Play)
                {
                    graphics.FillPolygon(brush, new[]
                    {
                        new PointF(center.X - 7, center.Y - 10),
                        new PointF(center.X - 7, center.Y + 10),
                        new PointF(center.X + 11, center.Y)
                    });
                }
                else if (IconKind == ButtonIconKind.Pause)
                {
                    graphics.FillRectangle(brush, center.X - 9, center.Y - 10, 6, 20);
                    graphics.FillRectangle(brush, center.X + 3, center.Y - 10, 6, 20);
                }
                else if (IconKind == ButtonIconKind.Stop)
                {
                    graphics.FillRectangle(brush, center.X - 8, center.Y - 8, 16, 16);
                }
                else if (IconKind == ButtonIconKind.Upload)
                {
                    graphics.DrawLine(pen, center.X, center.Y + 7, center.X, center.Y - 8);
                    graphics.DrawLine(pen, center.X, center.Y - 8, center.X - 6, center.Y - 2);
                    graphics.DrawLine(pen, center.X, center.Y - 8, center.X + 6, center.Y - 2);
                    graphics.DrawArc(pen, center.X - 13, center.Y - 2, 26, 16, 20, 140);
                }
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

        private Color ResolveSurfaceColor()
        {
            var parent = Parent;
            while (parent != null && parent.BackColor == Color.Transparent)
            {
                parent = parent.Parent;
            }

            if (parent == null || parent.BackColor == Color.Empty)
            {
                return Color.FromArgb(12, 22, 39);
            }

            return parent.BackColor;
        }

        private static Color Adjust(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Clamp(color.R * factor),
                Clamp(color.G * factor),
                Clamp(color.B * factor));
        }

        private static int Clamp(float value)
        {
            return value < 0 ? 0 : value > 255 ? 255 : (int)value;
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
