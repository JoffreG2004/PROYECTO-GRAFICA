using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace proyectoPaint.GraphicsCore
{
    // Buffer de píxeles para rasterización manual rápida (sin Bitmap.SetPixel).
    internal sealed class RasterSurface : IDisposable
    {
        private readonly Bitmap bitmap;
        private readonly BitmapData data;
        private readonly int[] pixels;
        private readonly int stride;

        public int Width { get { return bitmap.Width; } }
        public int Height { get { return bitmap.Height; } }

        public RasterSurface(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            stride = Math.Abs(data.Stride) / 4;
            pixels = new int[stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
        }

        public bool Contains(int x, int y) { return x >= 0 && y >= 0 && x < Width && y < Height; }
        public Color Get(int x, int y) { return Color.FromArgb(pixels[y * stride + x]); }
        public int GetArgb(int x, int y) { return pixels[y * stride + x]; }
        public void Set(int x, int y, Color color) { if (Contains(x, y)) pixels[y * stride + x] = color.ToArgb(); }

        public void Blend(int x, int y, Color color)
        {
            if (!Contains(x, y)) return;
            if (color.A >= 255) { pixels[y * stride + x] = Color.FromArgb(255, color.R, color.G, color.B).ToArgb(); return; }
            Color dst = Get(x, y); float a = color.A / 255F;
            int r = (int)Math.Round(color.R * a + dst.R * (1F - a));
            int g = (int)Math.Round(color.G * a + dst.G * (1F - a));
            int b = (int)Math.Round(color.B * a + dst.B * (1F - a));
            pixels[y * stride + x] = Color.FromArgb(255, r, g, b).ToArgb();
        }

        public void BlendSpan(int y, int left, int right, Color color)
        {
            if (y < 0 || y >= Height) return;
            left = Math.Max(0, left); right = Math.Min(Width - 1, right);
            if (left > right) return;
            int offset = y * stride + left;
            if (color.A >= 255)
            {
                int source = Color.FromArgb(255, color.R, color.G, color.B).ToArgb();
                for (int x = left; x <= right; x++) pixels[offset++] = source;
                return;
            }
            float a = color.A / 255F;
            for (int x = left; x <= right; x++, offset++)
            {
                Color dst = Color.FromArgb(pixels[offset]);
                pixels[offset] = Color.FromArgb(255, (int)Math.Round(color.R * a + dst.R * (1F - a)), (int)Math.Round(color.G * a + dst.G * (1F - a)), (int)Math.Round(color.B * a + dst.B * (1F - a))).ToArgb();
            }
        }

        public void Dispose() { Marshal.Copy(pixels, 0, data.Scan0, pixels.Length); bitmap.UnlockBits(data); }
    }
}
