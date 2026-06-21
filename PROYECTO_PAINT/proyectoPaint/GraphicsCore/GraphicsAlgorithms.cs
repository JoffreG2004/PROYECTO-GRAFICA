using System;
using System.Collections.Generic;
using System.Drawing;

namespace proyectoPaint.GraphicsCore
{
    public static class GraphicsAlgorithms
    {
        public static void PutPixel(Bitmap bmp, int x, int y, Color color, int thickness)
        {
            int radius = Math.Max(0, thickness / 2);
            for (int yy = y - radius; yy <= y + radius; yy++)
            {
                for (int xx = x - radius; xx <= x + radius; xx++)
                {
                    if (xx >= 0 && yy >= 0 && xx < bmp.Width && yy < bmp.Height)
                    {
                        bmp.SetPixel(xx, yy, color);
                    }
                }
            }
        }

        public static void DrawLine(Bitmap bmp, Point p0, Point p1, Color color, int thickness)
        {
            int x0 = p0.X, y0 = p0.Y, x1 = p1.X, y1 = p1.Y;
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                PutPixel(bmp, x0, y0, color, thickness);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public static void DrawCircle(Bitmap bmp, Point center, int radius, Color color, int thickness)
        {
            int x = radius;
            int y = 0;
            int err = 0;
            while (x >= y)
            {
                PlotCirclePoints(bmp, center, x, y, color, thickness);
                y++;
                if (err <= 0) err += 2 * y + 1;
                if (err > 0)
                {
                    x--;
                    err -= 2 * x + 1;
                }
            }
        }

        public static void FillPolygon(Bitmap bmp, IList<Point> points, Color fill)
        {
            if (points == null || points.Count < 3) return;
            int minY = points[0].Y, maxY = points[0].Y;
            foreach (Point p in points)
            {
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }

            for (int y = Math.Max(0, minY); y <= Math.Min(bmp.Height - 1, maxY); y++)
            {
                List<int> nodes = new List<int>();
                int j = points.Count - 1;
                for (int i = 0; i < points.Count; i++)
                {
                    Point pi = points[i];
                    Point pj = points[j];
                    if ((pi.Y < y && pj.Y >= y) || (pj.Y < y && pi.Y >= y))
                    {
                        int x = pi.X + (y - pi.Y) * (pj.X - pi.X) / Math.Max(1, pj.Y - pi.Y);
                        nodes.Add(x);
                    }
                    j = i;
                }
                nodes.Sort();
                for (int i = 0; i + 1 < nodes.Count; i += 2)
                {
                    int start = Math.Max(0, nodes[i]);
                    int end = Math.Min(bmp.Width - 1, nodes[i + 1]);
                    for (int x = start; x <= end; x++) bmp.SetPixel(x, y, fill);
                }
            }
        }

        public static void FloodFill(Bitmap bmp, Point start, Color replacement)
        {
            if (start.X < 0 || start.Y < 0 || start.X >= bmp.Width || start.Y >= bmp.Height) return;
            Color target = bmp.GetPixel(start.X, start.Y);
            if (target.ToArgb() == replacement.ToArgb()) return;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                if (p.X < 0 || p.Y < 0 || p.X >= bmp.Width || p.Y >= bmp.Height) continue;
                if (bmp.GetPixel(p.X, p.Y).ToArgb() != target.ToArgb()) continue;
                bmp.SetPixel(p.X, p.Y, replacement);
                queue.Enqueue(new Point(p.X + 1, p.Y));
                queue.Enqueue(new Point(p.X - 1, p.Y));
                queue.Enqueue(new Point(p.X, p.Y + 1));
                queue.Enqueue(new Point(p.X, p.Y - 1));
            }
        }

        public static Point RotatePoint(Point p, Point center, float degrees)
        {
            double rad = degrees * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            int x = (int)Math.Round(center.X + (p.X - center.X) * cos - (p.Y - center.Y) * sin);
            int y = (int)Math.Round(center.Y + (p.X - center.X) * sin + (p.Y - center.Y) * cos);
            return new Point(x, y);
        }

        public static Point ScalePoint(Point p, Point center, float factor)
        {
            int x = (int)Math.Round(center.X + (p.X - center.X) * factor);
            int y = (int)Math.Round(center.Y + (p.Y - center.Y) * factor);
            return new Point(x, y);
        }

        private static void PlotCirclePoints(Bitmap bmp, Point c, int x, int y, Color color, int thickness)
        {
            PutPixel(bmp, c.X + x, c.Y + y, color, thickness);
            PutPixel(bmp, c.X + y, c.Y + x, color, thickness);
            PutPixel(bmp, c.X - y, c.Y + x, color, thickness);
            PutPixel(bmp, c.X - x, c.Y + y, color, thickness);
            PutPixel(bmp, c.X - x, c.Y - y, color, thickness);
            PutPixel(bmp, c.X - y, c.Y - x, color, thickness);
            PutPixel(bmp, c.X + y, c.Y - x, color, thickness);
            PutPixel(bmp, c.X + x, c.Y - y, color, thickness);
        }
    }
}
