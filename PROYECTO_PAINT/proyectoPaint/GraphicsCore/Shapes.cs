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
        public abstract string Kind { get; }
        public abstract IEnumerable<Point> Points { get; }
        public abstract void Draw(Bitmap bmp);
        public abstract void Translate(int dx, int dy);
        public abstract void Rotate(float degrees);
        public abstract void Scale(float factor);

        public Rectangle Bounds
        {
            get
            {
                Point[] pts = Points.ToArray();
                if (pts.Length == 0) return Rectangle.Empty;
                int minX = pts.Min(p => p.X), minY = pts.Min(p => p.Y);
                int maxX = pts.Max(p => p.X), maxY = pts.Max(p => p.Y);
                return Rectangle.FromLTRB(minX, minY, maxX, maxY);
            }
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
    }

    public class PolylineShape : DrawableShape
    {
        public List<Point> Vertices { get; set; } = new List<Point>();
        public override string Kind { get { return "Lapiz"; } }
        public override IEnumerable<Point> Points { get { return Vertices; } }

        public override void Draw(Bitmap bmp)
        {
            for (int i = 1; i < Vertices.Count; i++)
                GraphicsAlgorithms.DrawLine(bmp, Vertices[i - 1], Vertices[i], StrokeColor, Thickness);
        }

        public override void Translate(int dx, int dy) { Transform(p => new Point(p.X + dx, p.Y + dy)); }
        public override void Rotate(float degrees) { Point c = Center(); Transform(p => GraphicsAlgorithms.RotatePoint(p, c, degrees)); }
        public override void Scale(float factor) { Point c = Center(); Transform(p => GraphicsAlgorithms.ScalePoint(p, c, factor)); }
        private void Transform(Func<Point, Point> fn) { for (int i = 0; i < Vertices.Count; i++) Vertices[i] = fn(Vertices[i]); }
    }

    public class LineShape : DrawableShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public override string Kind { get { return "Linea"; } }
        public override IEnumerable<Point> Points { get { yield return Start; yield return End; } }
        public override void Draw(Bitmap bmp) { GraphicsAlgorithms.DrawLine(bmp, Start, End, StrokeColor, Thickness); }
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
            if (UseFill) GraphicsAlgorithms.FillPolygon(bmp, Vertices, FillColor);
            for (int i = 0; i < Vertices.Count; i++)
                GraphicsAlgorithms.DrawLine(bmp, Vertices[i], Vertices[(i + 1) % Vertices.Count], StrokeColor, Thickness);
        }

        public override void Translate(int dx, int dy) { Transform(p => new Point(p.X + dx, p.Y + dy)); }
        public override void Rotate(float degrees) { Point c = Center(); Transform(p => GraphicsAlgorithms.RotatePoint(p, c, degrees)); }
        public override void Scale(float factor) { Point c = Center(); Transform(p => GraphicsAlgorithms.ScalePoint(p, c, factor)); }
        private void Transform(Func<Point, Point> fn) { for (int i = 0; i < Vertices.Count; i++) Vertices[i] = fn(Vertices[i]); }
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

    public class EllipseShape : DrawableShape
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public override string Kind { get { return "Elipse"; } }
        public override IEnumerable<Point> Points { get { yield return A; yield return B; } }

        public override void Draw(Bitmap bmp)
        {
            Rectangle r = BoundsFromCorners(A, B);
            if (UseFill)
            {
                for (int y = r.Top; y <= r.Bottom; y++)
                for (int x = r.Left; x <= r.Right; x++)
                {
                    double nx = (x - (r.Left + r.Width / 2.0)) / Math.Max(1, r.Width / 2.0);
                    double ny = (y - (r.Top + r.Height / 2.0)) / Math.Max(1, r.Height / 2.0);
                    if (nx * nx + ny * ny <= 1 && x >= 0 && y >= 0 && x < bmp.Width && y < bmp.Height)
                        bmp.SetPixel(x, y, FillColor);
                }
            }

            if (Math.Abs(r.Width - r.Height) < 6)
                GraphicsAlgorithms.DrawCircle(bmp, new Point(r.Left + r.Width / 2, r.Top + r.Height / 2), Math.Max(r.Width, r.Height) / 2, StrokeColor, Thickness);
            else
                DrawEllipseParametric(bmp, r);
        }

        public override void Translate(int dx, int dy) { A = new Point(A.X + dx, A.Y + dy); B = new Point(B.X + dx, B.Y + dy); }
        public override void Rotate(float degrees) { }
        public override void Scale(float factor) { Point c = Center(); A = GraphicsAlgorithms.ScalePoint(A, c, factor); B = GraphicsAlgorithms.ScalePoint(B, c, factor); }

        private void DrawEllipseParametric(Bitmap bmp, Rectangle r)
        {
            Point center = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
            double rx = Math.Max(1, r.Width / 2.0);
            double ry = Math.Max(1, r.Height / 2.0);
            Point prev = new Point(center.X + (int)rx, center.Y);
            for (int i = 1; i <= 360; i++)
            {
                double t = i * Math.PI / 180.0;
                Point next = new Point(center.X + (int)Math.Round(rx * Math.Cos(t)), center.Y + (int)Math.Round(ry * Math.Sin(t)));
                GraphicsAlgorithms.DrawLine(bmp, prev, next, StrokeColor, Thickness);
                prev = next;
            }
        }

        private static Rectangle BoundsFromCorners(Point a, Point b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
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
            Point prev = ControlPoints[0];
            for (int i = 1; i <= 100; i++)
            {
                double t = i / 100.0;
                Point p = Cubic(t);
                GraphicsAlgorithms.DrawLine(bmp, prev, p, StrokeColor, Thickness);
                prev = p;
            }
        }

        public override void Translate(int dx, int dy) { Transform(p => new Point(p.X + dx, p.Y + dy)); }
        public override void Rotate(float degrees) { Point c = Center(); Transform(p => GraphicsAlgorithms.RotatePoint(p, c, degrees)); }
        public override void Scale(float factor) { Point c = Center(); Transform(p => GraphicsAlgorithms.ScalePoint(p, c, factor)); }
        private void Transform(Func<Point, Point> fn) { for (int i = 0; i < ControlPoints.Count; i++) ControlPoints[i] = fn(ControlPoints[i]); }

        private Point Cubic(double t)
        {
            double u = 1 - t;
            Point p0 = ControlPoints[0], p1 = ControlPoints[1], p2 = ControlPoints[2], p3 = ControlPoints[3];
            int x = (int)Math.Round(u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X);
            int y = (int)Math.Round(u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y);
            return new Point(x, y);
        }
    }

    public class FloodFillShape : DrawableShape
    {
        public Point Seed { get; set; }
        public override string Kind { get { return "Relleno"; } }
        public override IEnumerable<Point> Points { get { yield return Seed; } }
        public override void Draw(Bitmap bmp) { GraphicsAlgorithms.FloodFill(bmp, Seed, FillColor); }
        public override void Translate(int dx, int dy) { Seed = new Point(Seed.X + dx, Seed.Y + dy); }
        public override void Rotate(float degrees) { }
        public override void Scale(float factor) { }
    }
}
