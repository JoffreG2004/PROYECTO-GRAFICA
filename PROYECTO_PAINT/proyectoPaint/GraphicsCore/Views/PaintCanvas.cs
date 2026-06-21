using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class PaintCanvas : Panel
    {
        public Bitmap CurrentBitmap { get; private set; }
        public DrawableShape SelectedShape { get; set; }
        public bool ShowGrid { get; set; }
        public float Zoom { get; set; } = 1F;

        public PaintCanvas()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Cursor = Cursors.Cross;
        }

        public void SetBitmap(Bitmap bitmap)
        {
            if (CurrentBitmap != null) CurrentBitmap.Dispose();
            CurrentBitmap = bitmap;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (CurrentBitmap != null)
            {
                e.Graphics.DrawImage(CurrentBitmap, new Rectangle(0, 0, Width, Height));
            }

            if (ShowGrid)
            {
                using (Pen grid = new Pen(Color.FromArgb(35, 45, 100, 180)))
                {
                    for (int x = 0; x < Width; x += 20) e.Graphics.DrawLine(grid, x, 0, x, Height);
                    for (int y = 0; y < Height; y += 20) e.Graphics.DrawLine(grid, 0, y, Width, y);
                }
            }

            if (SelectedShape != null)
            {
                Rectangle raw = SelectedShape.Bounds;
                Rectangle b = new Rectangle((int)(raw.X * Zoom), (int)(raw.Y * Zoom), (int)(raw.Width * Zoom), (int)(raw.Height * Zoom));
                using (Pen pen = new Pen(Color.FromArgb(40, 120, 215), 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    b.Inflate(6, 6);
                    e.Graphics.DrawRectangle(pen, b);
                }
            }
        }
    }
}
