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
                    new XAttribute("strokeArgb", shape.StrokeColor.ToArgb().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("fillArgb", shape.FillColor.ToArgb().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("thickness", shape.Thickness),
                    new XAttribute("useFill", shape.UseFill),
                    new XAttribute("strokeStyle", shape.StrokeStyle),
                    new XAttribute("opacity", shape.Opacity.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("visible", shape.Visible));
                if (!string.IsNullOrEmpty(shape.LayerName))
                    node.Add(new XAttribute("layerName", shape.LayerName));
                if (shape is RoundedRectangleShape)
                    node.Add(new XAttribute("cornerRadius", ((RoundedRectangleShape)shape).CornerRadius));
                if (shape is EllipseShape)
                    node.Add(new XAttribute("rotation", ((EllipseShape)shape).RotationDegrees.ToString(CultureInfo.InvariantCulture)));
                if (shape is TextShape)
                {
                    TextShape text = (TextShape)shape;
                    node.Add(new XAttribute("text", text.Text ?? string.Empty));
                    node.Add(new XAttribute("fontSize", text.FontSize));
                }
                if (shape is PolylineShape)
                {
                    PolylineShape line = (PolylineShape)shape;
                    node.Add(new XAttribute("flow", line.Flow));
                    node.Add(new XAttribute("smoothing", line.Smoothing));
                }
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
                shape.StrokeColor = ReadColor(node, "strokeArgb", "stroke", Color.Black);
                shape.FillColor = ReadColor(node, "fillArgb", "fill", Color.White);
                shape.Thickness = ReadInt(node, "thickness", 1);
                shape.UseFill = ReadBool(node, "useFill", false);
                shape.StrokeStyle = ReadStrokeStyle(node, "strokeStyle", StrokeRenderStyle.Solid);
                shape.Opacity = ReadFloat(node, "opacity", 1F);
                shape.Visible = ReadBool(node, "visible", true);
                shape.LayerName = ReadString(node, "layerName", null);
                if (shape is RoundedRectangleShape)
                    ((RoundedRectangleShape)shape).CornerRadius = ReadInt(node, "cornerRadius", 0);
                if (shape is EllipseShape)
                    ((EllipseShape)shape).RotationDegrees = ReadFloat(node, "rotation", 0F);
                if (shape is TextShape)
                {
                    ((TextShape)shape).Text = ReadString(node, "text", "Texto");
                    ((TextShape)shape).FontSize = ReadInt(node, "fontSize", 20);
                }
                if (shape is PolylineShape)
                {
                    PolylineShape line = (PolylineShape)shape;
                    line.Flow = ReadInt(node, "flow", 100);
                    line.Smoothing = ReadInt(node, "smoothing", 0);
                }
                document.Shapes.Add(shape);
            }
            // Un relleno se guarda inmediatamente después de la figura sobre la
            // que fue aplicado. Al cargarlo se restablece ese vínculo para que
            // ambos se transformen juntos.
            for (int i = 0; i < document.Shapes.Count; i++)
            {
                FloodFillShape fill = document.Shapes[i] as FloodFillShape;
                if (fill != null)
                    fill.LinkedShape = document.Shapes.Take(i).LastOrDefault(shape => !(shape is FloodFillShape));
            }
            return document;
        }

        private static DrawableShape CreateShape(string kind, List<Point> points)
        {
            if (kind == "Linea" && points.Count >= 2) return new LineShape { Start = points[0], End = points[1] };
            if (kind == "Rectangulo" && points.Count >= 4) return new RectangleShape { Vertices = points };
            if (kind == "RectanguloRedondeado" && points.Count >= 4) return new RoundedRectangleShape { Vertices = points };
            if (kind == "Elipse" && points.Count >= 2) return new EllipseShape { A = points[0], B = points[1] };
            if (kind == "Poligono" && points.Count >= 3) return new PolygonShape { Vertices = points };
            if (kind == "Flecha" && points.Count >= 3) return new ArrowShape { Vertices = points };
            if (kind == "Estrella" && points.Count >= 3) return new StarShape { Vertices = points };
            if (kind == "Blob" && points.Count >= 3) return new BlobShape { Vertices = points };
            if (kind == "Lapiz" && points.Count >= 2) return new PolylineShape { Vertices = points };
            if (kind == "CurvaBezier" && points.Count >= 4) return new BezierShape { ControlPoints = points };
            if (kind == "Relleno" && points.Count >= 1) return new FloodFillShape { Seed = points[0] };
            if (kind == "Texto" && points.Count >= 1) return new TextShape { Position = points[0] };
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

        private static Color ReadColor(XElement node, string argbName, string htmlName, Color fallback)
        {
            XAttribute argb = node.Attribute(argbName);
            if (argb != null)
            {
                int value;
                if (int.TryParse((string)argb, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    return Color.FromArgb(value);
            }

            XAttribute html = node.Attribute(htmlName);
            if (html != null) return ColorTranslator.FromHtml((string)html);
            return fallback;
        }

        private static int ReadInt(XElement node, string name, int fallback)
        {
            XAttribute attr = node.Attribute(name);
            if (attr == null) return fallback;
            int value;
            return int.TryParse((string)attr, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        private static float ReadFloat(XElement node, string name, float fallback)
        {
            XAttribute attr = node.Attribute(name);
            if (attr == null) return fallback;
            float value;
            return float.TryParse((string)attr, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        private static bool ReadBool(XElement node, string name, bool fallback)
        {
            XAttribute attr = node.Attribute(name);
            if (attr == null) return fallback;
            bool value;
            return bool.TryParse((string)attr, out value) ? value : fallback;
        }

        private static string ReadString(XElement node, string name, string fallback)
        {
            XAttribute attr = node.Attribute(name);
            return attr == null ? fallback : (string)attr;
        }

        private static StrokeRenderStyle ReadStrokeStyle(XElement node, string name, StrokeRenderStyle fallback)
        {
            XAttribute attr = node.Attribute(name);
            if (attr == null) return fallback;
            try
            {
                return (StrokeRenderStyle)Enum.Parse(typeof(StrokeRenderStyle), (string)attr);
            }
            catch
            {
                return fallback;
            }
        }
    }
}
