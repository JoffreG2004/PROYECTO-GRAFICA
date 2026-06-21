using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace proyectoPaint.GraphicsCore
{
    public static class ProjectStorage
    {
        public static void Save(CanvasDocument document, string path)
        {
            XElement root = new XElement("ProyectoPaint",
                new XAttribute("width", document.Width),
                new XAttribute("height", document.Height),
                new XAttribute("background", ColorTranslator.ToHtml(document.BackgroundColor)));

            foreach (DrawableShape shape in document.Shapes)
            {
                XElement node = new XElement("Shape",
                    new XAttribute("kind", shape.Kind),
                    new XAttribute("stroke", ColorTranslator.ToHtml(shape.StrokeColor)),
                    new XAttribute("fill", ColorTranslator.ToHtml(shape.FillColor)),
                    new XAttribute("thickness", shape.Thickness),
                    new XAttribute("useFill", shape.UseFill));
                node.Value = string.Join(";", shape.Points.Select(p => p.X.ToString(CultureInfo.InvariantCulture) + "," + p.Y.ToString(CultureInfo.InvariantCulture)));
                root.Add(node);
            }

            new XDocument(root).Save(path);
        }

        public static CanvasDocument Load(string path)
        {
            XDocument xml = XDocument.Load(path);
            XElement root = xml.Root;
            CanvasDocument document = new CanvasDocument
            {
                Width = (int)root.Attribute("width"),
                Height = (int)root.Attribute("height"),
                BackgroundColor = ColorTranslator.FromHtml((string)root.Attribute("background"))
            };

            foreach (XElement node in root.Elements("Shape"))
            {
                string kind = (string)node.Attribute("kind");
                List<Point> points = ParsePoints(node.Value);
                DrawableShape shape = CreateShape(kind, points);
                if (shape == null) continue;
                shape.StrokeColor = ColorTranslator.FromHtml((string)node.Attribute("stroke"));
                shape.FillColor = ColorTranslator.FromHtml((string)node.Attribute("fill"));
                shape.Thickness = (int)node.Attribute("thickness");
                shape.UseFill = (bool)node.Attribute("useFill");
                document.Shapes.Add(shape);
            }
            return document;
        }

        private static DrawableShape CreateShape(string kind, List<Point> points)
        {
            if (kind == "Linea" && points.Count >= 2) return new LineShape { Start = points[0], End = points[1] };
            if (kind == "Rectangulo" && points.Count >= 4) return new RectangleShape { Vertices = points };
            if (kind == "Elipse" && points.Count >= 2) return new EllipseShape { A = points[0], B = points[1] };
            if (kind == "Poligono" && points.Count >= 3) return new PolygonShape { Vertices = points };
            if (kind == "Lapiz" && points.Count >= 2) return new PolylineShape { Vertices = points };
            if (kind == "CurvaBezier" && points.Count >= 4) return new BezierShape { ControlPoints = points };
            if (kind == "Relleno" && points.Count >= 1) return new FloodFillShape { Seed = points[0] };
            return null;
        }

        private static List<Point> ParsePoints(string value)
        {
            List<Point> points = new List<Point>();
            foreach (string pair in value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = pair.Split(',');
                if (parts.Length == 2)
                    points.Add(new Point(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture)));
            }
            return points;
        }
    }
}
