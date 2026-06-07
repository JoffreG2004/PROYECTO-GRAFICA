using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace REPRODUCTOR_MUSICAL.Views
{
    public class RoundedPanel : Panel
    {
        public RoundedPanel()
        {
            DoubleBuffered = true;
            FillColor = Color.FromArgb(15, 24, 41);
            BorderColor = Color.FromArgb(46, 62, 92);
            BorderRadius = 14;
        }

        public Color FillColor { get; set; }

        public Color BorderColor { get; set; }

        public int BorderRadius { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedRectangle(ClientRectangle, BorderRadius))
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }

            base.OnPaint(e);
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
