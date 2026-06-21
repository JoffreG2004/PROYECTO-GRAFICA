using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace DeliverablesGenerator
{
    internal static class Program
    {
        private static readonly string Root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static readonly string Assets = Path.Combine(Root, "output", "assets");
        private static readonly string PdfOut = Path.Combine(Root, "output", "pdf", "Informe_tecnico_proyectoPaint.pdf");
        private static readonly string PptOut = Path.Combine(Root, "output", "presentations", "Presentacion_proyectoPaint.pptx");

        private static void Main()
        {
            Directory.CreateDirectory(Assets);
            Directory.CreateDirectory(Path.GetDirectoryName(PdfOut));
            Directory.CreateDirectory(Path.GetDirectoryName(PptOut));
            string screenshot = Path.Combine(Assets, "captura_proyectoPaint.png");
            string diagram = Path.Combine(Assets, "diagrama_clases_proyectoPaint.png");
            CreateScreenshot(screenshot);
            CreateDiagram(diagram);
            CreatePdf(PdfOut);
            CreatePptx(PptOut);
            Console.WriteLine(PdfOut);
            Console.WriteLine(PptOut);
        }

        private static void CreateScreenshot(string path)
        {
            using (Bitmap bmp = new Bitmap(1200, 720))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(218, 224, 232));
                using (Brush top = new SolidBrush(Color.FromArgb(28, 36, 52))) g.FillRectangle(top, 0, 0, 1200, 54);
                using (Brush side = new SolidBrush(Color.White)) g.FillRectangle(side, 0, 54, 214, 666);
                using (Brush canvas = new SolidBrush(Color.White)) g.FillRectangle(canvas, 235, 80, 920, 580);
                using (Pen border = new Pen(Color.FromArgb(160, 170, 185))) g.DrawRectangle(border, 235, 80, 920, 580);
                DrawText(g, "proyectoPaint", 18, 14, 24, Color.White, true);
                DrawText(g, "Lapiz  Linea  Rectangulo  Circulo  Poligono  Bezier", 20, 82, 16, Color.FromArgb(45, 55, 72), true);
                DrawText(g, "Color linea / relleno, grosor, borrador, seleccion", 20, 210, 14, Color.FromArgb(60, 70, 86), false);
                using (Pen p = new Pen(Color.FromArgb(20, 95, 180), 4))
                {
                    g.DrawLine(p, 310, 180, 550, 310);
                    g.DrawRectangle(p, 640, 165, 190, 130);
                    g.DrawEllipse(p, 880, 150, 160, 140);
                }
                using (Brush fill = new SolidBrush(Color.FromArgb(255, 230, 120)))
                    g.FillPolygon(fill, new[] { new Point(380, 430), new Point(520, 360), new Point(670, 440), new Point(610, 550), new Point(430, 540) });
                using (Pen p = new Pen(Color.FromArgb(34, 139, 94), 4))
                    g.DrawPolygon(p, new[] { new Point(380, 430), new Point(520, 360), new Point(670, 440), new Point(610, 550), new Point(430, 540) });
                using (Pen p = new Pen(Color.FromArgb(200, 60, 85), 4))
                    g.DrawBezier(p, 760, 455, 830, 350, 930, 610, 1040, 470);
                bmp.Save(path, ImageFormat.Png);
            }
        }

        private static void CreateDiagram(string path)
        {
            using (Bitmap bmp = new Bitmap(1200, 720))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                DrawText(g, "Estructura de clases - proyectoPaint", 40, 30, 28, Color.FromArgb(28, 36, 52), true);
                string[] boxes =
                {
                    "Form1\nUI, eventos, comandos",
                    "PaintCanvas\nPanel doble buffer",
                    "CanvasDocument\nLista de figuras y render",
                    "DrawableShape\nAbstraccion polimorfica",
                    "LineShape / RectangleShape\nEllipseShape / PolygonShape",
                    "PolylineShape / BezierShape\nFloodFillShape",
                    "GraphicsAlgorithms\nBresenham, scanline, fill",
                    "ProjectStorage\nGuardar y abrir .ppaint"
                };
                Point[] pts =
                {
                    new Point(60,110), new Point(430,110), new Point(780,110), new Point(430,280),
                    new Point(90,455), new Point(430,455), new Point(780,455), new Point(780,280)
                };
                for (int i = 0; i < boxes.Length; i++) DrawBox(g, pts[i], boxes[i]);
                using (Pen pen = new Pen(Color.FromArgb(48, 91, 145), 3))
                {
                    DrawArrow(g, pen, 300, 170, 430, 170);
                    DrawArrow(g, pen, 670, 170, 780, 170);
                    DrawArrow(g, pen, 870, 235, 870, 280);
                    DrawArrow(g, pen, 780, 340, 670, 340);
                    DrawArrow(g, pen, 560, 390, 240, 455);
                    DrawArrow(g, pen, 560, 390, 560, 455);
                    DrawArrow(g, pen, 670, 515, 780, 515);
                }
                bmp.Save(path, ImageFormat.Png);
            }
        }

        private static void CreatePdf(string path)
        {
            PdfDoc pdf = new PdfDoc();
            pdf.AddPage("Informe tecnico - proyectoPaint", new[]
            {
                "Aplicacion grafica interactiva tipo Paint desarrollada en C# Windows Forms y .NET Framework 4.7.2.",
                "Autor: Gomez Joffre. Materia: Computacion Grafica.",
                "El sistema permite crear dibujos digitales en un lienzo, manipular figuras y guardar el trabajo como imagen o proyecto editable.",
                "",
                "Funcionalidades implementadas:",
                "- Lienzo interactivo con eventos MouseDown, MouseMove, MouseUp y DoubleClick.",
                "- Herramientas: lapiz, linea, rectangulo, circulo/elipse, poligono, curva Bezier, relleno, borrador y seleccion.",
                "- Color de linea, color de relleno, grosor, limpieza completa, guardado PNG/JPG y archivo .ppaint.",
                "- Transformaciones: traslacion por arrastre, rotacion de 15 grados y escala +10% / -10% sobre la figura seleccionada."
            });
            pdf.AddPage("Arquitectura de software y POO", new[]
            {
                "La solucion se organiza en capas simples:",
                "- Form1: interfaz grafica, comandos, eventos y retroalimentacion visual.",
                "- PaintCanvas: componente visual de lienzo con doble buffer y seleccion.",
                "- CanvasDocument: documento que almacena figuras y renderiza el bitmap final.",
                "- DrawableShape: clase abstracta base para todas las figuras.",
                "- Shapes concretas: LineShape, RectangleShape, EllipseShape, PolygonShape, PolylineShape, BezierShape y FloodFillShape.",
                "- GraphicsAlgorithms: algoritmos de rasterizacion, relleno y transformacion.",
                "- ProjectStorage: persistencia XML del archivo .ppaint.",
                "",
                "Evidencia POO: encapsulamiento de propiedades, herencia desde DrawableShape, polimorfismo en Draw/Translate/Rotate/Scale y reutilizacion de algoritmos comunes."
            });
            pdf.AddPage("Algoritmos de computacion grafica", new[]
            {
                "1. Rasterizacion de lineas: algoritmo de Bresenham, usando coordenadas enteras y escritura de pixeles.",
                "2. Circulo: algoritmo de punto medio para el caso circular.",
                "3. Elipse: recorrido parametrico y relleno por ecuacion normalizada.",
                "4. Poligonos: rasterizado de bordes con Bresenham y relleno scanline.",
                "5. Relleno: flood fill con cola BFS desde una semilla, comparando el color objetivo.",
                "6. Curva Bezier cubica: evaluacion por parametro t y union de puntos consecutivos.",
                "7. Transformaciones: traslacion por delta del mouse, rotacion con matriz trigonometrica y escala respecto al centro de la figura.",
                "",
                "El proyecto no se limita a DrawLine/DrawRectangle: las figuras se convierten a pixeles mediante Bitmap.SetPixel y rutinas propias."
            });
            pdf.AddPage("UI/UX, evidencias y conclusiones", new[]
            {
                "Diseno UI/UX:",
                "- Barra superior para acciones frecuentes: guardar imagen, guardar proyecto, abrir y limpiar.",
                "- Panel lateral con herramientas, grosor, relleno y colores visibles.",
                "- Barra de estado con coordenadas y mensajes de uso.",
                "- Seleccion visual con rectangulo punteado y transformaciones accesibles.",
                "",
                "Problemas encontrados y soluciones:",
                "- Persistencia del relleno: se resolvio modelandolo como FloodFillShape para conservar la operacion en el documento.",
                "- Evitar parpadeo: PaintCanvas usa doble buffer y render centralizado.",
                "- Separacion de responsabilidades: se movio la logica grafica fuera del formulario.",
                "",
                "Conclusion: proyectoPaint integra interaccion, POO y algoritmos clasicos de Computacion Grafica en una aplicacion funcional y extensible."
            });
            pdf.Save(path);
        }

        private static void CreatePptx(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                Add(zip, "[Content_Types].xml", ContentTypes());
                Add(zip, "_rels/.rels", RootRels());
                Add(zip, "ppt/presentation.xml", PresentationXml(8));
                Add(zip, "ppt/_rels/presentation.xml.rels", PresentationRels(8));
                Add(zip, "ppt/slideMasters/slideMaster1.xml", SlideMaster());
                Add(zip, "ppt/slideMasters/_rels/slideMaster1.xml.rels", MasterRels());
                Add(zip, "ppt/slideLayouts/slideLayout1.xml", SlideLayout());
                Add(zip, "ppt/slideLayouts/_rels/slideLayout1.xml.rels", LayoutRels());
                Add(zip, "ppt/theme/theme1.xml", Theme());
                string[][] slides =
                {
                    new[] {"proyectoPaint", "Aplicacion grafica interactiva tipo Paint", "Computacion Grafica - Gomez Joffre"},
                    new[] {"Objetivo", "Crear dibujos digitales en un lienzo con herramientas basicas y algoritmos propios.", "Interaccion, rasterizacion, POO, transformaciones y persistencia."},
                    new[] {"Herramientas", "Lapiz, linea, rectangulo, circulo/elipse, poligono, curva Bezier, relleno, borrador y seleccion.", "Color de linea, color de relleno y control de grosor."},
                    new[] {"Arquitectura", "Form1, PaintCanvas, CanvasDocument, DrawableShape, GraphicsAlgorithms y ProjectStorage.", "Separacion entre interfaz, modelo, algoritmos y almacenamiento."},
                    new[] {"POO aplicada", "Abstraccion DrawableShape; herencia en figuras; polimorfismo en Draw, Translate, Rotate y Scale.", "Encapsulamiento de colores, grosor, puntos y estado de relleno."},
                    new[] {"Algoritmos", "Bresenham para lineas, punto medio para circulos, scanline para poligonos y flood fill BFS.", "Bezier cubica y transformaciones geometricas por coordenadas."},
                    new[] {"UI/UX", "Barra superior, panel lateral, lienzo central, barra de estado y seleccion visual.", "Flujo simple: elegir herramienta, dibujar, seleccionar, transformar y guardar."},
                    new[] {"Entregables", "Codigo fuente organizado, ejecutable Debug, informe tecnico PDF y presentacion PPTX.", "El proyecto compila como proyectoPaint.exe."}
                };
                for (int i = 0; i < slides.Length; i++)
                {
                    Add(zip, "ppt/slides/slide" + (i + 1) + ".xml", SlideXml(slides[i][0], slides[i][1], slides[i][2], i + 1));
                    Add(zip, "ppt/slides/_rels/slide" + (i + 1) + ".xml.rels", SlideRels());
                }
            }
        }

        private static void DrawText(Graphics g, string text, int x, int y, int size, Color color, bool bold)
        {
            using (Font f = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular))
            using (Brush b = new SolidBrush(color))
                g.DrawString(text, f, b, x, y);
        }

        private static void DrawBox(Graphics g, Point p, string text)
        {
            Rectangle r = new Rectangle(p.X, p.Y, 240, 110);
            using (Brush b = new SolidBrush(Color.FromArgb(241, 246, 252))) g.FillRectangle(b, r);
            using (Pen pen = new Pen(Color.FromArgb(48, 91, 145), 2)) g.DrawRectangle(pen, r);
            DrawText(g, text, p.X + 14, p.Y + 18, 14, Color.FromArgb(28, 36, 52), false);
        }

        private static void DrawArrow(Graphics g, Pen pen, int x1, int y1, int x2, int y2)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
            g.FillEllipse(Brushes.White, x2 - 5, y2 - 5, 10, 10);
            g.DrawEllipse(pen, x2 - 5, y2 - 5, 10, 10);
        }

        private static void Add(ZipArchive zip, string name, string content)
        {
            ZipArchiveEntry entry = zip.CreateEntry(name);
            using (Stream s = entry.Open())
            using (StreamWriter w = new StreamWriter(s, new UTF8Encoding(false))) w.Write(content);
        }

        private static string Esc(string s) { return SecurityElement(s); }
        private static string SecurityElement(string s) { return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;"); }

        private static string ContentTypes()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8""?><Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types""><Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/><Default Extension=""xml"" ContentType=""application/xml""/><Override PartName=""/ppt/presentation.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml""/><Override PartName=""/ppt/slideMasters/slideMaster1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml""/><Override PartName=""/ppt/slideLayouts/slideLayout1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml""/><Override PartName=""/ppt/theme/theme1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.theme+xml""/>");
            for (int i = 1; i <= 8; i++) sb.Append(@"<Override PartName=""/ppt/slides/slide" + i + @".xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>");
            sb.Append("</Types>");
            return sb.ToString();
        }

        private static string RootRels() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""ppt/presentation.xml""/></Relationships>"; }
        private static string PresentationRels(int count)
        {
            StringBuilder sb = new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster"" Target=""slideMasters/slideMaster1.xml""/>");
            for (int i = 1; i <= count; i++) sb.Append(@"<Relationship Id=""rId" + (i + 1) + @""" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide" + i + @".xml""/>");
            sb.Append("</Relationships>");
            return sb.ToString();
        }
        private static string PresentationXml(int count)
        {
            StringBuilder ids = new StringBuilder();
            for (int i = 1; i <= count; i++) ids.Append(@"<p:sldId id=""" + (255 + i) + @""" r:id=""rId" + (i + 1) + @"""/>");
            return @"<?xml version=""1.0"" encoding=""UTF-8""?><p:presentation xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main""><p:sldMasterIdLst><p:sldMasterId id=""2147483648"" r:id=""rId1""/></p:sldMasterIdLst><p:sldIdLst>" + ids + @"</p:sldIdLst><p:sldSz cx=""12192000"" cy=""6858000"" type=""screen16x9""/><p:notesSz cx=""6858000"" cy=""9144000""/></p:presentation>";
        }
        private static string MasterRels() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout"" Target=""../slideLayouts/slideLayout1.xml""/><Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme"" Target=""../theme/theme1.xml""/></Relationships>"; }
        private static string LayoutRels() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster"" Target=""../slideMasters/slideMaster1.xml""/></Relationships>"; }
        private static string SlideRels() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships""><Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout"" Target=""../slideLayouts/slideLayout1.xml""/></Relationships>"; }
        private static string SlideMaster() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><p:sldMaster xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main""><p:cSld><p:spTree><p:nvGrpSpPr><p:cNvPr id=""1"" name=""""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/></p:spTree></p:cSld><p:sldLayoutIdLst><p:sldLayoutId id=""2147483649"" r:id=""rId1""/></p:sldLayoutIdLst><p:txStyles><p:titleStyle/><p:bodyStyle/><p:otherStyle/></p:txStyles></p:sldMaster>"; }
        private static string SlideLayout() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><p:sldLayout xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"" type=""blank""><p:cSld name=""Blank""><p:spTree><p:nvGrpSpPr><p:cNvPr id=""1"" name=""""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/></p:spTree></p:cSld></p:sldLayout>"; }
        private static string Theme() { return @"<?xml version=""1.0"" encoding=""UTF-8""?><a:theme xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" name=""proyectoPaint""><a:themeElements><a:clrScheme name=""paint""><a:dk1><a:srgbClr val=""1C2434""/></a:dk1><a:lt1><a:srgbClr val=""FFFFFF""/></a:lt1><a:dk2><a:srgbClr val=""305B91""/></a:dk2><a:lt2><a:srgbClr val=""EEF1F5""/></a:lt2><a:accent1><a:srgbClr val=""D24B5A""/></a:accent1><a:accent2><a:srgbClr val=""228B5E""/></a:accent2><a:accent3><a:srgbClr val=""FFE678""/></a:accent3><a:accent4><a:srgbClr val=""145FB4""/></a:accent4><a:accent5><a:srgbClr val=""6B7280""/></a:accent5><a:accent6><a:srgbClr val=""C6CFDC""/></a:accent6><a:hlink><a:srgbClr val=""145FB4""/></a:hlink><a:folHlink><a:srgbClr val=""305B91""/></a:folHlink></a:clrScheme><a:fontScheme name=""Aptos""><a:majorFont><a:latin typeface=""Aptos Display""/></a:majorFont><a:minorFont><a:latin typeface=""Aptos""/></a:minorFont></a:fontScheme><a:fmtScheme name=""default""><a:fillStyleLst><a:solidFill><a:schemeClr val=""phClr""/></a:solidFill></a:fillStyleLst><a:lnStyleLst><a:ln w=""6350""><a:solidFill><a:schemeClr val=""phClr""/></a:solidFill></a:ln></a:lnStyleLst><a:effectStyleLst><a:effectStyle/></a:effectStyleLst><a:bgFillStyleLst><a:solidFill><a:schemeClr val=""phClr""/></a:solidFill></a:bgFillStyleLst></a:fmtScheme></a:themeElements></a:theme>"; }

        private static string SlideXml(string title, string body, string footer, int number)
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?><p:sld xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main""><p:cSld><p:bg><p:bgPr><a:solidFill><a:srgbClr val=""EEF1F5""/></a:solidFill></p:bgPr></p:bg><p:spTree><p:nvGrpSpPr><p:cNvPr id=""1"" name=""""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/>" +
                Shape(2, 0, 0, 12192000, 900000, "1C2434", "") +
                TextBox(3, 650000, 900000, 6800000, 900000, title, 3600, "1C2434", true) +
                TextBox(4, 650000, 2100000, 8900000, 1400000, body, 2100, "2D3748", false) +
                Shape(5, 720000, 4050000, 2200000, 1200000, "FFE678", "Linea, circulo, poligono") +
                Shape(6, 3300000, 4050000, 2200000, 1200000, "D8F3E6", "Relleno y pixeles") +
                Shape(7, 5880000, 4050000, 2200000, 1200000, "DCEBFF", "Transformaciones") +
                TextBox(8, 650000, 6000000, 8400000, 350000, footer, 1200, "6B7280", false) +
                TextBox(9, 11000000, 6100000, 500000, 300000, number.ToString(CultureInfo.InvariantCulture), 1200, "6B7280", false) +
                "</p:spTree></p:cSld><p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr></p:sld>";
        }

        private static string Shape(int id, int x, int y, int w, int h, string fill, string text)
        {
            string body = string.IsNullOrEmpty(text) ? "" : @"<p:txBody><a:bodyPr/><a:lstStyle/><a:p><a:r><a:rPr lang=""es-EC"" sz=""1500""/><a:t>" + Esc(text) + @"</a:t></a:r></a:p></p:txBody>";
            return @"<p:sp><p:nvSpPr><p:cNvPr id=""" + id + @""" name=""shape" + id + @"""/><p:cNvSpPr/><p:nvPr/></p:nvSpPr><p:spPr><a:xfrm><a:off x=""" + x + @""" y=""" + y + @"""/><a:ext cx=""" + w + @""" cy=""" + h + @"""/></a:xfrm><a:prstGeom prst=""roundRect""><a:avLst/></a:prstGeom><a:solidFill><a:srgbClr val=""" + fill + @"""/></a:solidFill><a:ln><a:solidFill><a:srgbClr val=""C6CFDC""/></a:solidFill></a:ln></p:spPr>" + body + "</p:sp>";
        }

        private static string TextBox(int id, int x, int y, int w, int h, string text, int size, string color, bool bold)
        {
            string b = bold ? @" b=""1""" : "";
            return @"<p:sp><p:nvSpPr><p:cNvPr id=""" + id + @""" name=""text" + id + @"""/><p:cNvSpPr txBox=""1""/><p:nvPr/></p:nvSpPr><p:spPr><a:xfrm><a:off x=""" + x + @""" y=""" + y + @"""/><a:ext cx=""" + w + @""" cy=""" + h + @"""/></a:xfrm><a:prstGeom prst=""rect""><a:avLst/></a:prstGeom><a:noFill/><a:ln><a:noFill/></a:ln></p:spPr><p:txBody><a:bodyPr wrap=""square""/><a:lstStyle/><a:p><a:r><a:rPr lang=""es-EC"" sz=""" + size + @"""" + b + @"><a:solidFill><a:srgbClr val=""" + color + @"""/></a:solidFill></a:rPr><a:t>" + Esc(text) + @"</a:t></a:r></a:p></p:txBody></p:sp>";
        }
    }

    internal class PdfDoc
    {
        private readonly List<string[]> pages = new List<string[]>();
        private readonly List<string> titles = new List<string>();
        public void AddPage(string title, string[] lines) { titles.Add(title); pages.Add(lines); }

        public void Save(string path)
        {
            List<string> objects = new List<string>();
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
            StringBuilder kids = new StringBuilder();
            for (int i = 0; i < pages.Count; i++) kids.Append((3 + i * 2) + " 0 R ");
            objects.Add("<< /Type /Pages /Kids [" + kids + "] /Count " + pages.Count + " >>");
            for (int i = 0; i < pages.Count; i++)
            {
                int pageObj = 3 + i * 2;
                int contentObj = pageObj + 1;
                objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> /F2 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >> >> >> /Contents " + contentObj + " 0 R >>");
                string stream = BuildPage(titles[i], pages[i], i + 1);
                objects.Add("<< /Length " + Encoding.ASCII.GetByteCount(stream) + " >>\nstream\n" + stream + "\nendstream");
            }
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms, "%PDF-1.4\n");
                List<long> offsets = new List<long> { 0 };
                for (int i = 0; i < objects.Count; i++)
                {
                    offsets.Add(ms.Position);
                    Write(ms, (i + 1) + " 0 obj\n" + objects[i] + "\nendobj\n");
                }
                long xref = ms.Position;
                Write(ms, "xref\n0 " + (objects.Count + 1) + "\n0000000000 65535 f \n");
                for (int i = 1; i < offsets.Count; i++) Write(ms, offsets[i].ToString("0000000000") + " 00000 n \n");
                Write(ms, "trailer\n<< /Size " + (objects.Count + 1) + " /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF");
                File.WriteAllBytes(path, ms.ToArray());
            }
        }

        private static string BuildPage(string title, string[] lines, int number)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BT /F2 22 Tf 50 790 Td (" + E(title) + ") Tj ET\n");
            int y = 742;
            foreach (string line in lines)
            {
                if (line == "") { y -= 14; continue; }
                string font = line.StartsWith("-") ? "/F1 10 Tf" : (line.EndsWith(":") ? "/F2 12 Tf" : "/F1 11 Tf");
                sb.Append("BT " + font + " 58 " + y + " Td (" + E(line) + ") Tj ET\n");
                y -= line.Length > 92 ? 28 : 18;
            }
            sb.Append("BT /F1 9 Tf 50 36 Td (proyectoPaint - Informe tecnico - pagina " + number + ") Tj ET\n");
            return sb.ToString();
        }

        private static string E(string s)
        {
            string clean = s.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n").Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U").Replace("Ñ", "N");
            return clean.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }
        private static void Write(Stream s, string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            s.Write(data, 0, data.Length);
        }
    }
}
