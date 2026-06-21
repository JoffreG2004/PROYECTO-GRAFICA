using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class PaintCanvas : Panel
    {
        public Bitmap CurrentBitmap { get; private set; }
        public DrawableShape SelectedShape { get; set; }

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
                e.Graphics.DrawImageUnscaled(CurrentBitmap, 0, 0);
            }

            if (SelectedShape != null)
            {
                Rectangle b = SelectedShape.Bounds;
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
