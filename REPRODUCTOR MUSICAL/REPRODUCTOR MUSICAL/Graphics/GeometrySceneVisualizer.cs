using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class GeometrySceneVisualizer : IVisualizer
    {
        private const int NodeCount = 32;
        private readonly PointF[] nodes = new PointF[NodeCount];
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);
        private float angle;

        public string Name => "Escena geometrica";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            angle += 0.014f + audioFrame.Intensity * 0.045f + audioFrame.Pulse * 0.075f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.Clear(Color.FromArgb(7, 9, 15));

            DrawBackdrop(graphics, bounds);
            CalculateNodes(bounds);
            DrawConstellation(graphics);
            DrawOrbitingPolygons(graphics, bounds);
            DrawCentralCore(graphics, bounds);
            DrawPulseRings(graphics, bounds);
        }

        private void DrawBackdrop(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            using (var brush = new LinearGradientBrush(
                bounds,
                Color.FromArgb(8, 10, 16),
                Color.FromArgb(24, 20, 38),
                LinearGradientMode.ForwardDiagonal))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var center = GetCenter(bounds);
            var glowRadius = Math.Min(bounds.Width, bounds.Height) * (0.25f + currentFrame.Intensity * 0.12f + currentFrame.Pulse * 0.18f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(center.X - glowRadius, center.Y - glowRadius, glowRadius * 2, glowRadius * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(80 + (int)(currentFrame.Pulse * 90), 109, 240, 214);
                    brush.SurroundColors = new[] { Color.FromArgb(0, 109, 240, 214) };
                    graphics.FillPath(brush, path);
                }
            }
        }

        private void CalculateNodes(Rectangle bounds)
        {
            var center = GetCenter(bounds);
            var radiusX = bounds.Width * (0.23f + currentFrame.Bass * 0.08f + currentFrame.Pulse * 0.07f);
            var radiusY = bounds.Height * (0.18f + currentFrame.Mid * 0.07f + currentFrame.Pulse * 0.05f);

            for (var i = 0; i < NodeCount; i++)
            {
                var band = GetSpectrumValue(i, NodeCount);
                var nodeAngle = angle + i * Math.PI * 2 / NodeCount;
                var ripple = 1 + band * 0.72f + currentFrame.Pulse * 0.20f;
                nodes[i] = new PointF(
                    center.X + (float)(Math.Cos(nodeAngle) * radiusX * ripple),
                    center.Y + (float)(Math.Sin(nodeAngle) * radiusY * ripple));
            }
        }

        private void DrawConstellation(System.Drawing.Graphics graphics)
        {
            for (var i = 0; i < NodeCount; i++)
            {
                var next = (i + 1) % NodeCount;
                var jump = (i + 5) % NodeCount;
                var band = GetSpectrumValue(i, NodeCount);
                var nextBand = GetSpectrumValue(next, NodeCount);
                var lineEnergy = Math.Max(band, nextBand);
                var cyanAlpha = Math.Min(240, 80 + (int)(lineEnergy * 135) + (int)(currentFrame.Pulse * 45));
                var roseAlpha = Math.Min(220, 45 + (int)(lineEnergy * 105) + (int)(currentFrame.Pulse * 45));

                DrawGlowLine(graphics, nodes[i], nodes[next], Color.FromArgb(cyanAlpha, 109, 240, 214), 1.1f + lineEnergy * 4.2f + currentFrame.Pulse * 1.4f);

                if (i % 2 == 0)
                {
                    DrawGlowLine(graphics, nodes[i], nodes[jump], Color.FromArgb(roseAlpha, 239, 94, 115), 0.8f + lineEnergy * 2.7f + currentFrame.Pulse * 1.2f);
                }
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var band = GetSpectrumValue(i, NodeCount);
                var size = 3.5f + band * 18 + currentFrame.Pulse * 7;
                var alpha = Math.Min(255, 150 + (int)(band * 95));
                using (var brush = new SolidBrush(Color.FromArgb(alpha, 255, 200, 87)))
                {
                    graphics.FillEllipse(brush, nodes[i].X - size / 2, nodes[i].Y - size / 2, size, size);
                }
            }
        }

        private void DrawOrbitingPolygons(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = GetCenter(bounds);
            var orbitRadius = Math.Min(bounds.Width, bounds.Height) * (0.22f + currentFrame.Bass * 0.13f + currentFrame.Pulse * 0.18f);

            for (var i = 0; i < 7; i++)
            {
                var band = GetSpectrumValue(i * 5, NodeCount);
                var orbitAngle = angle * (1.2f + i * 0.08f) + i * (float)Math.PI * 2 / 7;
                var polygonCenter = new PointF(
                    center.X + (float)Math.Cos(orbitAngle) * orbitRadius * (1 + band * 0.38f),
                    center.Y + (float)Math.Sin(orbitAngle) * orbitRadius * (0.72f + band * 0.18f));
                var sides = i % 2 == 0 ? 6 : 3;
                var size = 18 + i * 4 + band * 70 + currentFrame.Intensity * 18 + currentFrame.Pulse * 32;
                var rotation = -angle * (1.2f + i * 0.16f + band * 1.4f + currentFrame.Pulse * 1.1f);
                var color = i % 3 == 0
                    ? Color.FromArgb(60 + (int)(band * 105), 109, 240, 214)
                    : i % 3 == 1
                        ? Color.FromArgb(60 + (int)(band * 105), 255, 200, 87)
                        : Color.FromArgb(60 + (int)(band * 105), 239, 94, 115);

                DrawPolygon(graphics, polygonCenter, sides, size, rotation, color);
            }
        }

        private void DrawCentralCore(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = GetCenter(bounds);
            var lowBand = AverageSpectrum(0, 12);
            var midBand = AverageSpectrum(12, 30);
            var outer = 56 + lowBand * 130 + currentFrame.Pulse * 70;
            var inner = 24 + midBand * 78 + currentFrame.Intensity * 20 + currentFrame.Pulse * 38;

            DrawPolygon(graphics, center, 6, outer, angle, Color.FromArgb(86, 109, 240, 214));
            DrawPolygon(graphics, center, 3, outer * 0.72f, -angle * 1.8f, Color.FromArgb(90, 239, 94, 115));

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(center.X - inner, center.Y - inner, inner * 2, inner * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(230, 255, 255, 255);
                    brush.SurroundColors = new[] { Color.FromArgb(35, 109, 240, 214) };
                    graphics.FillPath(brush, path);
                }
            }
        }

        private void DrawPulseRings(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = GetCenter(bounds);
            var maxRadius = Math.Min(bounds.Width, bounds.Height) * 0.44f;

            for (var i = 0; i < 4; i++)
            {
                var band = AverageSpectrum(i * 10, 10);
                var radius = maxRadius * (0.18f + i * 0.18f) + band * 110 + currentFrame.Pulse * (38 + i * 14);
                var alpha = Math.Min(210, 35 + (int)(band * 130) + (int)(currentFrame.Pulse * 50));
                using (var pen = new Pen(Color.FromArgb(alpha, 109, 240, 214), 1.2f + band * 5 + currentFrame.Pulse * 2.2f))
                {
                    graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
                }
            }
        }

        private static void DrawGlowLine(System.Drawing.Graphics graphics, PointF from, PointF to, Color color, float width)
        {
            using (var glowPen = new Pen(Color.FromArgb(Math.Min(255, color.A / 2), color.R, color.G, color.B), width + 4))
            using (var pen = new Pen(color, width))
            {
                graphics.DrawLine(glowPen, from, to);
                graphics.DrawLine(pen, from, to);
            }
        }

        private static void DrawPolygon(System.Drawing.Graphics graphics, PointF center, int sides, float radius, float rotation, Color color)
        {
            var points = new PointF[sides];

            for (var i = 0; i < sides; i++)
            {
                var pointAngle = rotation + i * Math.PI * 2 / sides;
                points[i] = new PointF(
                    center.X + (float)Math.Cos(pointAngle) * radius,
                    center.Y + (float)Math.Sin(pointAngle) * radius);
            }

            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(Color.FromArgb(Math.Min(255, color.A + 90), color.R, color.G, color.B), 2))
            {
                graphics.FillPolygon(brush, points);
                graphics.DrawPolygon(pen, points);
            }
        }

        private static PointF GetCenter(Rectangle bounds)
        {
            return new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
        }

        private float GetSpectrumValue(int index, int count)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return Math.Max(currentFrame.Intensity * 0.35f, 0.05f);
            }

            var spectrumIndex = Math.Min(
                currentFrame.Spectrum.Length - 1,
                Math.Max(0, index * currentFrame.Spectrum.Length / Math.Max(1, count)));

            return currentFrame.Spectrum[spectrumIndex];
        }

        private float AverageSpectrum(int start, int length)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return currentFrame.Intensity * 0.35f;
            }

            var total = 0f;
            var count = 0;

            for (var i = start; i < start + length && i < currentFrame.Spectrum.Length; i++)
            {
                total += currentFrame.Spectrum[i];
                count++;
            }

            return count == 0 ? 0 : total / count;
        }
    }
}
