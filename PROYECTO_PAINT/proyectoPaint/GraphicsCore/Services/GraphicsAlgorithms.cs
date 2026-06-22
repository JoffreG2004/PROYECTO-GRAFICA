using System;
using System.Collections.Generic;
using System.Drawing;

namespace proyectoPaint.GraphicsCore
{
    public static class GraphicsAlgorithms
    {
        public static void DrawLine(Bitmap bmp, Point a, Point b, Color color, int thickness, StrokeRenderStyle style = StrokeRenderStyle.Solid)
        { using (RasterSurface s = new RasterSurface(bmp)) DrawLine(s, a, b, color, thickness, style); }

        public static void DrawPolyline(Bitmap bmp, IList<Point> points, Color color, int thickness, StrokeRenderStyle style = StrokeRenderStyle.Solid)
        {
            if (points == null || points.Count < 2) return;
            using (RasterSurface s = new RasterSurface(bmp))
                for (int i = 1; i < points.Count; i++) DrawLine(s, points[i - 1], points[i], color, thickness, style);
        }

        public static void DrawPolygon(Bitmap bmp, IList<Point> points, Color stroke, int thickness, Color fill, bool useFill, StrokeRenderStyle style)
        {
            if (points == null || points.Count < 2) return;
            using (RasterSurface s = new RasterSurface(bmp))
            {
                if (useFill && points.Count >= 3) FillPolygon(s, points, fill);
                for (int i = 0; i < points.Count; i++) DrawLine(s, points[i], points[(i + 1) % points.Count], stroke, thickness, style);
            }
        }

        public static void DrawCircle(Bitmap bmp, Point center, int radius, Color color, int thickness)
        { using (RasterSurface s = new RasterSurface(bmp)) DrawCircle(s, center, radius, color, thickness); }

        public static void DrawEllipse(Bitmap bmp, Point center, double rx, double ry, double cos, double sin, Rectangle bounds, Color stroke, int thickness, Color fill, bool useFill, StrokeRenderStyle style)
        {
            using (RasterSurface s = new RasterSurface(bmp))
            {
                if (useFill)
                    for (int y = Math.Max(0, bounds.Top - 1); y <= Math.Min(s.Height - 1, bounds.Bottom + 1); y++)
                    for (int x = Math.Max(0, bounds.Left - 1); x <= Math.Min(s.Width - 1, bounds.Right + 1); x++)
                    {
                        double tx = x - center.X, ty = y - center.Y, ux = tx * cos + ty * sin, uy = -tx * sin + ty * cos;
                        if ((ux * ux) / (rx * rx) + (uy * uy) / (ry * ry) <= 1D) s.Blend(x, y, fill);
                    }
                List<Point> points = new List<Point>();
                for (int i = 0; i <= 180; i++)
                {
                    double t = i * 2D * Math.PI / 180D, x = rx * Math.Cos(t), y = ry * Math.Sin(t);
                    points.Add(new Point((int)Math.Round(center.X + x * cos - y * sin), (int)Math.Round(center.Y + x * sin + y * cos)));
                }
                for (int i = 1; i < points.Count; i++) DrawLine(s, points[i - 1], points[i], stroke, thickness, style);
            }
        }

        public static void FillPolygon(Bitmap bmp, IList<Point> points, Color fill)
        { using (RasterSurface s = new RasterSurface(bmp)) FillPolygon(s, points, fill); }

        public static bool FloodFill(Bitmap bmp, Point start, Color replacement, int maxSpans = int.MaxValue)
        {
            using (RasterSurface s = new RasterSurface(bmp))
            {
                if (!s.Contains(start.X, start.Y)) return true;
                int target = s.GetArgb(start.X, start.Y);
                if (target == replacement.ToArgb()) return true;
                Stack<Point> pending = new Stack<Point>(); pending.Push(start);
                int processed = 0;
                while (pending.Count > 0)
                {
                    if (processed++ >= maxSpans) return false;
                    Point seed = pending.Pop();
                    if (!s.Contains(seed.X, seed.Y) || s.GetArgb(seed.X, seed.Y) != target) continue;
                    int left = seed.X, right = seed.X;
                    while (left > 0 && s.GetArgb(left - 1, seed.Y) == target) left--;
                    while (right < s.Width - 1 && s.GetArgb(right + 1, seed.Y) == target) right++;
                    s.BlendSpan(seed.Y, left, right, replacement);
                    AddNeighborRuns(s, pending, left, right, seed.Y - 1, target);
                    AddNeighborRuns(s, pending, left, right, seed.Y + 1, target);
                }
                return true;
            }
        }

        public static List<Point> BuildSeedOrder(Bitmap bmp, Point seed)
        {
            List<Point> order = new List<Point>();
            using (RasterSurface s = new RasterSurface(bmp))
            {
                if (!s.Contains(seed.X, seed.Y)) return order;
                int target = s.GetArgb(seed.X, seed.Y); bool[] seen = new bool[s.Width * s.Height]; Stack<Point> stack = new Stack<Point>(); stack.Push(seed);
                while (stack.Count > 0)
                {
                    Point p = stack.Pop();
                    if (!s.Contains(p.X, p.Y)) continue;
                    int index = p.Y * s.Width + p.X;
                    if (seen[index] || s.GetArgb(p.X, p.Y) != target) continue;
                    seen[index] = true; order.Add(p);
                    // Se apila al revés: al sacar siempre se visita arriba, derecha, abajo, izquierda.
                    stack.Push(new Point(p.X - 1, p.Y)); stack.Push(new Point(p.X, p.Y + 1));
                    stack.Push(new Point(p.X + 1, p.Y)); stack.Push(new Point(p.X, p.Y - 1));
                }
            }
            return order;
        }

        public static void PaintPoints(Bitmap bmp, IList<Point> points, int count, Color color)
        {
            if (points == null || count <= 0) return;
            using (RasterSurface s = new RasterSurface(bmp))
                for (int i = 0; i < Math.Min(count, points.Count); i++) s.Blend(points[i].X, points[i].Y, color);
        }

        private static void AddNeighborRuns(RasterSurface s, Stack<Point> pending, int left, int right, int y, int target)
        {
            if (y < 0 || y >= s.Height) return;
            int x = left;
            while (x <= right)
            {
                while (x <= right && s.GetArgb(x, y) != target) x++;
                if (x > right) return;
                pending.Push(new Point(x, y));
                while (x <= right && s.GetArgb(x, y) == target) x++;
            }
        }

        public static Point RotatePoint(Point p, Point center, float degrees)
        {
            double r = degrees * Math.PI / 180D, cos = Math.Cos(r), sin = Math.Sin(r);
            return new Point((int)Math.Round(center.X + (p.X - center.X) * cos - (p.Y - center.Y) * sin), (int)Math.Round(center.Y + (p.X - center.X) * sin + (p.Y - center.Y) * cos));
        }
        public static Point ScalePoint(Point p, Point center, float factor) { return new Point((int)Math.Round(center.X + (p.X - center.X) * factor), (int)Math.Round(center.Y + (p.Y - center.Y) * factor)); }

        private static void DrawLine(RasterSurface s, Point p0, Point p1, Color color, int thickness, StrokeRenderStyle style)
        {
            int x0 = p0.X, y0 = p0.Y, dx = Math.Abs(p1.X - x0), sx = x0 < p1.X ? 1 : -1, dy = -Math.Abs(p1.Y - y0), sy = y0 < p1.Y ? 1 : -1, err = dx + dy, step = 0;
            int stampEvery = Math.Max(1, thickness / 3);
            while (true)
            {
                bool pattern = style == StrokeRenderStyle.Solid || (style == StrokeRenderStyle.Dashed ? step % 18 < 11 : step % 10 < 2);
                if (pattern && (step % stampEvery == 0 || (x0 == p1.X && y0 == p1.Y))) Stamp(s, x0, y0, color, thickness);
                if (x0 == p1.X && y0 == p1.Y) break;
                int e2 = 2 * err; if (e2 >= dy) { err += dy; x0 += sx; } if (e2 <= dx) { err += dx; y0 += sy; } step++;
            }
        }

        private static void Stamp(RasterSurface s, int x, int y, Color color, int thickness)
        {
            int r = Math.Max(0, thickness / 2), rr = r * r;
            for (int yy = -r; yy <= r; yy++)
            {
                int half = (int)Math.Sqrt(Math.Max(0, rr - yy * yy));
                s.BlendSpan(y + yy, x - half, x + half, color);
            }
        }

        private static void DrawCircle(RasterSurface s, Point c, int radius, Color color, int thickness)
        {
            int x = radius, y = 0, err = 0;
            while (x >= y) { Plot(s, c, x, y, color, thickness); y++; if (err <= 0) err += 2 * y + 1; if (err > 0) { x--; err -= 2 * x + 1; } }
        }
        private static void Plot(RasterSurface s, Point c, int x, int y, Color color, int thickness)
        { Stamp(s,c.X+x,c.Y+y,color,thickness); Stamp(s,c.X+y,c.Y+x,color,thickness); Stamp(s,c.X-y,c.Y+x,color,thickness); Stamp(s,c.X-x,c.Y+y,color,thickness); Stamp(s,c.X-x,c.Y-y,color,thickness); Stamp(s,c.X-y,c.Y-x,color,thickness); Stamp(s,c.X+y,c.Y-x,color,thickness); Stamp(s,c.X+x,c.Y-y,color,thickness); }

        private static void FillPolygon(RasterSurface s, IList<Point> points, Color fill)
        {
            if (points == null || points.Count < 3) return;
            int min = points[0].Y, max = min; foreach (Point p in points) { min = Math.Min(min,p.Y); max = Math.Max(max,p.Y); }
            for (int y = Math.Max(0, min); y <= Math.Min(s.Height - 1, max); y++)
            {
                List<int> nodes = new List<int>(); int j = points.Count - 1;
                for (int i = 0; i < points.Count; i++) { Point a=points[i], b=points[j]; if ((a.Y < y && b.Y >= y) || (b.Y < y && a.Y >= y)) nodes.Add(a.X + (int)Math.Round((double)(y-a.Y)*(b.X-a.X)/(b.Y-a.Y))); j=i; }
                nodes.Sort(); for (int i = 0; i + 1 < nodes.Count; i += 2) for (int x = Math.Max(0,nodes[i]); x <= Math.Min(s.Width-1,nodes[i+1]); x++) s.Blend(x,y,fill);
            }
        }
    }
}
