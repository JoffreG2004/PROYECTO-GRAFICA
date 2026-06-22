using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace proyectoPaint.GraphicsCore
{
    public abstract class DrawableShape
    {
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
        public int Thickness { get; set; }
        public bool UseFill { get; set; }
        public StrokeRenderStyle StrokeStyle { get; set; }
        public float Opacity { get; set; } = 1F;
        public bool Visible { get; set; } = true;
        public string LayerName { get; set; }
        public abstract string Kind { get; }
        public virtual string DisplayName { get { return string.IsNullOrEmpty(LayerName) ? Kind : LayerName; } }
        public abstract IEnumerable<Point> Points { get; }
        public abstract void Draw(Bitmap bmp);
        public abstract void Translate(int dx, int dy);
        public abstract void Rotate(float degrees);
        public abstract void Scale(float factor);

        public virtual Rectangle Bounds
        {
            get { return CalculateBounds(Points); }
        }

        public virtual bool HitTest(Point p)
        {
            Rectangle bounds = Bounds;
            bounds.Inflate(Math.Max(8, Thickness + 4), Math.Max(8, Thickness + 4));
            return bounds.Contains(p);
        }

        protected Point Center()
        {
            Rectangle b = Bounds;
            return new Point(b.Left + b.Width / 2, b.Top + b.Height / 2);
        }

        protected Color StrokePaint()
        {
            return ApplyOpacity(StrokeColor, Opacity);
        }

        public Color GetStrokePaint()
        {
            return StrokePaint();
        }

        protected Color FillPaint()
        {
            return ApplyOpacity(FillColor, Opacity);
        }

        protected static Color ApplyOpacity(Color color, float opacity)
        {
            float value = Math.Max(0F, Math.Min(1F, opacity));
            int alpha = (int)Math.Round(color.A * value);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        protected static Rectangle CalculateBounds(IEnumerable<Point> points)
        {
            Point[] pts = points == null ? new Point[0] : points.ToArray();
            if (pts.Length == 0) return Rectangle.Empty;
            int minX = pts.Min(p => p.X), minY = pts.Min(p => p.Y);
            int maxX = pts.Max(p => p.X), maxY = pts.Max(p => p.Y);
            return Rectangle.FromLTRB(minX, minY, maxX, maxY);
        }
    }

    public class PolylineShape : DrawableShape
    {
        public List<Point> Vertices { get; set; } = new List<Point>();
        public int Smoothing { get; set; }
        public int Flow { get; set; } = 100;
        public override string Kind { get { return "Lapiz"; } }
        public override IEnumerable<Point> Points { get { return Vertices; } }

        public override void Draw(Bitmap bmp)
        {
            List<Point> points = SmoothedVertices();
            Color paint = GetRenderPaint();
            GraphicsAlgorithms.DrawPolyline(bmp, points, paint, Thickness, StrokeStyle);
        }

        public Color GetRenderPaint()
        {
            return ApplyOpacity(StrokeColor, Opacity * Math.Max(0, Math.Min(100, Flow)) / 100F);
        }

        public override void Translate(int dx, int dy) { Transform(p => new Point(p.X + dx, p.Y + dy)); }
        public override void Rotate(float degrees) { Point c = Center(); Transform(p => GraphicsAlgorithms.RotatePoint(p, c, degrees)); }
        public override void Scale(float factor) { Point c = Center(); Transform(p => GraphicsAlgorithms.ScalePoint(p, c, factor)); }

        private List<Point> SmoothedVertices()
        {
            if (Smoothing <= 0 || Vertices.Count < 3) return new List<Point>(Vertices);
            int minDistance = 1 + Smoothing / 12;
            List<Point> filtered = new List<Point>();
            filtered.Add(Vertices[0]);
            for (int i = 1; i < Vertices.Count - 1; i++)
            {
                Point last = filtered[filtered.Count - 1];
                int dx = Vertices[i].X - last.X;
                int dy = Vertices[i].Y - last.Y;
                if (dx * dx + dy * dy >= minDistance * minDistance)
                    filtered.Add(Vertices[i]);
            }
            filtered.Add(Vertices[Vertices.Count - 1]);
            return filtered;
        }

        private void Transform(Func<Point, Point> fn)
        {
            for (int i = 0; i < Vertices.Count; i++) Vertices[i] = fn(Vertices[i]);
        }
    }

    public class LineShape : DrawableShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public override string Kind { get { return "Linea"; } }
        public override IEnumerable<Point> Points { get { yield return Start; yield return End; } }
        public override void Draw(Bitmap bmp) { GraphicsAlgorithms.DrawLine(bmp, Start, End, StrokePaint(), Thickness, StrokeStyle); }
        public override void Translate(int dx, int dy) { Start = new Point(Start.X + dx, Start.Y + dy); End = new Point(End.X + dx, End.Y + dy); }
        public override void Rotate(float degrees) { Point c = Center(); Start = GraphicsAlgorithms.RotatePoint(Start, c, degrees); End = GraphicsAlgorithms.RotatePoint(End, c, degrees); }
        public override void Scale(float factor) { Point c = Center(); Start = GraphicsAlgorithms.ScalePoint(Start, c, factor); End = GraphicsAlgorithms.ScalePoint(End, c, factor); }
    }

    public class PolygonShape : DrawableShape
    {
        public List<Point> Vertices { get; set; } = new List<Point>();
        public override string Kind { get { return "Poligono"; } }
        public override IEnumerable<Point> Points { get { return Vertices; } }

        public override void Draw(Bitmap bmp)
        {
            if (Vertices.Count < 2) return;
            GraphicsAlgorithms.DrawPolygon(bmp, Vertices, StrokePaint(), Thickness, FillPaint(), UseFill && Vertices.Count >= 3, StrokeStyle);
        }

        public override void Translate(int dx, int dy) { Transform(p => new Point(p.X + dx, p.Y + dy)); }
        public override void Rotate(float degrees) { Point c = Center(); Transform(p => GraphicsAlgorithms.RotatePoint(p, c, degrees)); }
        public override void Scale(float factor) { Point c = Center(); Transform(p => GraphicsAlgorithms.ScalePoint(p, c, factor)); }

        protected void Transform(Func<Point, Point> fn)
        {
            for (int i = 0; i < Vertices.Count; i++) Vertices[i] = fn(Vertices[i]);
        }
    }

    public class RectangleShape : PolygonShape
    {
        public override string Kind { get { return "Rectangulo"; } }
        public RectangleShape() { }
        public RectangleShape(Point a, Point b)
        {
            Vertices.Add(new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)));
            Vertices.Add(new Point(Math.Max(a.X, b.X), Math.Min(a.Y, b.Y)));
            Vertices.Add(new Point(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
            Vertices.Add(new Point(Math.Min(a.X, b.X), Math.Max(a.Y, b.Y)));
        }
    }

    public class RegularPolygonShape : PolygonShape
    {
        public int Sides { get; private set; }
        public override string Kind { get { return Sides + " lados"; } }

        public RegularPolygonShape() { Sides = 3; }
        public RegularPolygonShape(Point center, Point edge, int sides)
        {
            Sides = Math.Max(3, Math.Min(10, sides));
            double radius = Math.Max(2, Math.Sqrt((edge.X - center.X) * (edge.X - center.X) + (edge.Y - center.Y) * (edge.Y - center.Y)));
            double baseAngle = Math.Atan2(edge.Y - center.Y, edge.X - center.X) - Math.PI / 2;
            for (int i = 0; i < Sides; i++)
            {
                double angle = baseAngle + i * 2 * Math.PI / Sides;
                Vertices.Add(new Point(center.X + (int)Math.Round(Math.Cos(angle) * radius), center.Y + (int)Math.Round(Math.Sin(angle) * radius)));
            }
        }
    }

    public class RoundedRectangleShape : PolygonShape
    {
        public int CornerRadius { get; set; }
        public override string Kind { get { return "RectanguloRedondeado"; } }

        public RoundedRectangleShape() { }
        public RoundedRectangleShape(Point a, Point b, int radius)
        {
            CornerRadius = radius;
            Vertices.AddRange(BuildRoundedRectangle(a, b, radius));
        }

        public void SetCornerRadius(int radius)
        {
            Rectangle bounds = Bounds;
            CornerRadius = radius;
            Vertices.Clear();
            Vertices.AddRange(BuildRoundedRectangle(new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Bottom), radius));
        }

        private static IEnumerable<Point> BuildRoundedRectangle(Point a, Point b, int radius)
        {
            int left = Math.Min(a.X, b.X), top = Math.Min(a.Y, b.Y);
            int right = Math.Max(a.X, b.X), bottom = Math.Max(a.Y, b.Y);
            int width = Math.Max(1, right - left), height = Math.Max(1, bottom - top);
            int r = Math.Max(0, Math.Min(radius, Math.Min(width, height) / 2));
            if (r == 0) return new RectangleShape(a, b).Vertices;

            List<Point> pts = new List<Point>();
            AddArc(pts, right - r, top + r, r, -90, 0);
            AddArc(pts, right - r, bottom - r, r, 0, 90);
            AddArc(pts, left + r, bottom - r, r, 90, 180);
            AddArc(pts, left + r, top + r, r, 180, 270);
            return pts;
        }

        private static void AddArc(List<Point> pts, int cx, int cy, int radius, int start, int end)
        {
            for (int angle = start; angle <= end; angle += 10)
            {
                double rad = angle * Math.PI / 180.0;
                pts.Add(new Point(cx + (int)Math.Round(Math.Cos(rad) * radius), cy + (int)Math.Round(Math.Sin(rad) * radius)));
            }
        }
    }

    public class ArrowShape : PolygonShape
    {
        public override string Kind { get { return "Flecha"; } }

        public ArrowShape() { }
        public ArrowShape(Point start, Point end, int thickness)
        {
            Vertices.AddRange(BuildArrow(start, end, Math.Max(10, thickness * 4)));
        }

        private static IEnumerable<Point> BuildArrow(Point start, Point end, int bodyWidth)
        {
            double dx = end.X - start.X, dy = end.Y - start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1) length = 1;
            double ux = dx / length, uy = dy / length;
            double px = -uy, py = ux;
            double head = Math.Max(18, Math.Min(length * 0.45, bodyWidth * 2.2));
            double halfBody = bodyWidth / 2.0;
            double halfHead = bodyWidth;
            Point neck = new Point((int)Math.Round(end.X - ux * head), (int)Math.Round(end.Y - uy * head));

            return new[]
            {
                new Point((int)Math.Round(start.X + px * halfBody), (int)Math.Round(start.Y + py * halfBody)),
                new Point((int)Math.Round(neck.X + px * halfBody), (int)Math.Round(neck.Y + py * halfBody)),
                new Point((int)Math.Round(neck.X + px * halfHead), (int)Math.Round(neck.Y + py * halfHead)),
                end,
                new Point((int)Math.Round(neck.X - px * halfHead), (int)Math.Round(neck.Y - py * halfHead)),
                new Point((int)Math.Round(neck.X - px * halfBody), (int)Math.Round(neck.Y - py * halfBody)),
                new Point((int)Math.Round(start.X - px * halfBody), (int)Math.Round(start.Y - py * halfBody))
            };
        }
    }

    public class StarShape : PolygonShape
    {
        public override string Kind { get { return "Estrella"; } }

        public StarShape() { }
        public StarShape(Point a, Point b)
        {
            Vertices.AddRange(BuildStar(a, b));
        }

        private static IEnumerable<Point> BuildStar(Point a, Point b)
        {
            Rectangle r = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            Point c = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
            double outer = Math.Max(3, Math.Min(r.Width, r.Height) / 2.0);
            double inner = outer * 0.45;
            List<Point> pts = new List<Point>();
            for (int i = 0; i < 10; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                double radius = i % 2 == 0 ? outer : inner;
                pts.Add(new Point(c.X + (int)Math.Round(Math.Cos(angle) * radius), c.Y + (int)Math.Round(Math.Sin(angle) * radius)));
            }
            return pts;
        }
    }

    public class BlobShape : PolygonShape
    {
        public override string Kind { get { return "Blob"; } }

        public BlobShape() { }
        public BlobShape(Point a, Point b)
        {
            Vertices.AddRange(BuildBlob(a, b));
        }

        private static IEnumerable<Point> BuildBlob(Point a, Point b)
        {
            Rectangle r = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            Point c = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
            double rx = Math.Max(2, r.Width / 2.0);
            double ry = Math.Max(2, r.Height / 2.0);
            List<Point> pts = new List<Point>();
            for (int i = 0; i < 32; i++)
            {
                double t = i * 2 * Math.PI / 32.0;
                double wobble = 1.0 + 0.16 * Math.Sin(3 * t) + 0.09 * Math.Cos(5 * t);
                pts.Add(new Point(c.X + (int)Math.Round(Math.Cos(t) * rx * wobble), c.Y + (int)Math.Round(Math.Sin(t) * ry * wobble)));
            }
            return pts;
        }
    }

    public class EllipseShape : DrawableShape
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public float RotationDegrees { get; set; }
        public override string Kind { get { return "Elipse"; } }
        public override IEnumerable<Point> Points { get { yield return A; yield return B; } }
        public override Rectangle Bounds { get { return CalculateBounds(RotatedCorners()); } }

        public override void Draw(Bitmap bmp)
        {
            Rectangle r = AxisBounds();
            Point center = AxisCenter();
            double rx = Math.Max(1, r.Width / 2.0), ry = Math.Max(1, r.Height / 2.0);
            double rad = RotationDegrees * Math.PI / 180.0;
            double cos = Math.Cos(rad), sin = Math.Sin(rad);

            GraphicsAlgorithms.DrawEllipse(bmp, center, rx, ry, cos, sin, Bounds, StrokePaint(), Thickness, FillPaint(), UseFill, StrokeStyle);
        }

        public override void Translate(int dx, int dy) { A = new Point(A.X + dx, A.Y + dy); B = new Point(B.X + dx, B.Y + dy); }
        public override void Rotate(float degrees) { RotationDegrees = (RotationDegrees + degrees) % 360F; }
        public override void Scale(float factor) { Point c = AxisCenter(); A = GraphicsAlgorithms.ScalePoint(A, c, factor); B = GraphicsAlgorithms.ScalePoint(B, c, factor); }

        private Rectangle AxisBounds()
        {
            return Rectangle.FromLTRB(Math.Min(A.X, B.X), Math.Min(A.Y, B.Y), Math.Max(A.X, B.X), Math.Max(A.Y, B.Y));
        }

        private Point AxisCenter()
        {
            Rectangle r = AxisBounds();
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

        private IEnumerable<Point> RotatedCorners()
        {
            Rectangle r = AxisBounds();
            Point c = AxisCenter();
            Point[] corners =
            {
                new Point(r.Left, r.Top),
                new Point(r.Right, r.Top),
                new Point(r.Right, r.Bottom),
                new Point(r.Left, r.Bottom)
            };
            foreach (Point p in corners) yield return GraphicsAlgorithms.RotatePoint(p, c, RotationDegrees);
        }

    }

    public class BezierShape : DrawableShape
    {
        public List<Point> ControlPoints { get; set; } = new List<Point>();
        public override string Kind { get { return "CurvaBezier"; } }
        public override IEnumerable<Point> Points { get { return ControlPoints; } }

        public override void Draw(Bitmap bmp)
        {
            if (ControlPoints.Count < 4) return;
            Rectangle bounds = Bounds;
            int maxDimension = Math.Max(bounds.Width, bounds.Height);
            int steps = Math.Max(24, Math.Min(64, maxDimension / 8));
            List<Point> points = new List<Point> { ControlPoints[0] };
            for (int i = 1; i <= steps; i++)
            {
                double t = i / (double)steps;
                points.Add(Cubic(t));
            }
            GraphicsAlgorithms.DrawPolyline(bmp, points, StrokePaint(), Thickness, StrokeStyle);
        }

        public override void Translate(int dx, int dy) { Transform(p => new Point(p.X + dx, p.Y + dy)); }
        public override void Rotate(float degrees) { Point c = Center(); Transform(p => GraphicsAlgorithms.RotatePoint(p, c, degrees)); }
        public override void Scale(float factor) { Point c = Center(); Transform(p => GraphicsAlgorithms.ScalePoint(p, c, factor)); }

        private void Transform(Func<Point, Point> fn)
        {
            for (int i = 0; i < ControlPoints.Count; i++) ControlPoints[i] = fn(ControlPoints[i]);
        }

        private Point Cubic(double t)
        {
            double u = 1 - t;
            Point p0 = ControlPoints[0], p1 = ControlPoints[1], p2 = ControlPoints[2], p3 = ControlPoints[3];
            return new Point(
                (int)Math.Round(u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X),
                (int)Math.Round(u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y));
        }
    }

    public class FloodFillShape : DrawableShape
    {
        public Point Seed { get; set; }
        public int MaxSpans { get; set; } = int.MaxValue;
        public bool IsComplete { get; private set; } = true;
        public override string Kind { get { return "Relleno"; } }
        public override IEnumerable<Point> Points { get { yield return Seed; } }
        public override void Draw(Bitmap bmp) { IsComplete = GraphicsAlgorithms.FloodFill(bmp, Seed, FillPaint(), MaxSpans); }
        public override void Translate(int dx, int dy) { Seed = new Point(Seed.X + dx, Seed.Y + dy); }
        public override void Rotate(float degrees) { }
        public override void Scale(float factor) { }
    }
}
