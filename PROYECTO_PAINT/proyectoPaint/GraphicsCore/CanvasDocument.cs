using System.Collections.Generic;
using System.Drawing;

namespace proyectoPaint.GraphicsCore
{
    public class CanvasDocument
    {
        public List<DrawableShape> Shapes { get; private set; } = new List<DrawableShape>();
        public Color BackgroundColor { get; set; } = Color.White;
        public int Width { get; set; } = 1200;
        public int Height { get; set; } = 720;

        public Bitmap Render(DrawableShape preview = null)
        {
            Bitmap bmp = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(bmp))
                g.Clear(BackgroundColor);
            foreach (DrawableShape shape in Shapes)
                shape.Draw(bmp);
            if (preview != null) preview.Draw(bmp);
            return bmp;
        }

        public void Clear()
        {
            Shapes.Clear();
        }
    }
}
