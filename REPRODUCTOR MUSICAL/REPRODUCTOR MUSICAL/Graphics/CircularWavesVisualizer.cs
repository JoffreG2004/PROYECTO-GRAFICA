using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class CircularWavesVisualizer : IVisualizer
    {
        private const int RadialBars = 128;
        private float phase;
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);

        public string Name => "Ondas circulares";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            phase += 0.045f + audioFrame.Intensity * 0.09f + audioFrame.Pulse * 0.16f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            DrawBackground(graphics, bounds);
            DrawAmbientRings(graphics, bounds);
            DrawCircularSpectrum(graphics, bounds);
            DrawRadialBars(graphics, bounds);
            DrawOrbitNodes(graphics, bounds);
            DrawCore(graphics, bounds);
        }

        private void DrawBackground(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            using (var brush = new LinearGradientBrush(
                bounds,
                // Color fondo arriba/izquierda.
                Color.FromArgb(5, 8, 16),
                // Color fondo abajo/derecha.
                Color.FromArgb(18, 13, 34),
                LinearGradientMode.ForwardDiagonal))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
            var radius = Math.Min(bounds.Width, bounds.Height) * (0.32f + currentFrame.Intensity * 0.08f + currentFrame.Pulse * 0.08f);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                using (var glow = new PathGradientBrush(path))
                {
                    glow.CenterPoint = center;
                    glow.CenterColor = Color.FromArgb(90 + (int)(currentFrame.Pulse * 45), 42, 222, 205); // Color brillo grande del fondo.
                    glow.SurroundColors = new[] { Color.FromArgb(0, 42, 222, 205) }; // Color borde del brillo de fondo.
                    graphics.FillPath(glow, path);
                }
            }

            DrawTinyStars(graphics, bounds);
        }

        private void DrawAmbientRings(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var centerX = bounds.Width / 2f;
            var centerY = bounds.Height / 2f;
            var baseRadius = Math.Min(bounds.Width, bounds.Height) * 0.16f;

            for (var i = 0; i < 6; i++)
            {
                var radius = baseRadius + i * 48 + currentFrame.Pulse * (18 + i * 7) + (float)Math.Sin(phase * 1.6 + i) * 6;
                var alpha = ClampAlpha(48 - i * 5 + currentFrame.Pulse * 55);
                var color = i % 2 == 0
                    ? Color.FromArgb(alpha, 41, 221, 218) // Color anillos pares.
                    : Color.FromArgb(alpha, 126, 118, 255); // Color anillos impares.

                using (var pen = new Pen(color, 1.3f + currentFrame.Pulse * 1.6f))
                {
                    graphics.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }
        }

        private void DrawCircularSpectrum(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
            var baseRadius = Math.Min(bounds.Width, bounds.Height) * 0.23f;

            for (var layer = 0; layer < 3; layer++)
            {
                var points = new PointF[RadialBars + 1];
                var layerOffset = layer * 0.08f;
                var layerRadius = baseRadius + layer * 26;

                for (var i = 0; i <= RadialBars; i++)
                {
                    var t = i / (float)RadialBars;
                    var band = GetSpectrumValue(t);
                    var wave = (float)Math.Sin(phase * (1.25f + layer * 0.28f) + i * 0.12f + layer * 1.7f);
                    var radius = layerRadius + band * (80 - layer * 12) + currentFrame.Pulse * (22 + layer * 8) + wave * (8 + band * 16);
                    var angle = t * Math.PI * 2 + phase * (0.20f + layerOffset);

                    points[i] = new PointF(
                        center.X + (float)Math.Cos(angle) * radius,
                        center.Y + (float)Math.Sin(angle) * radius);
                }

                // Color ondas circulares: capa 1 rosa, capa 2 celeste, capa 3 amarillo.
                var color = layer == 0
                    ? Color.FromArgb(185, 255, 95, 170)
                    : layer == 1
                        ? Color.FromArgb(205, 41, 221, 218)
                        : Color.FromArgb(150, 255, 210, 86);

                using (var glowPen = new Pen(Color.FromArgb(55, color), 8 - layer * 1.5f) { LineJoin = LineJoin.Round })
                using (var pen = new Pen(color, 2.2f - layer * 0.25f) { LineJoin = LineJoin.Round })
                {
                    graphics.DrawLines(glowPen, points);
                    graphics.DrawLines(pen, points);
                }
            }
        }

        private void DrawRadialBars(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
            var innerRadius = Math.Min(bounds.Width, bounds.Height) * 0.30f;

            for (var i = 0; i < RadialBars; i++)
            {
                var t = i / (float)RadialBars;
                var band = GetSpectrumValue(t);
                var angle = t * Math.PI * 2 - phase * 0.30f;
                var energy = Clamp(band * 0.90f + currentFrame.Intensity * 0.10f + currentFrame.Pulse * 0.16f);
                var length = 8 + energy * 82;
                var inner = innerRadius + (float)Math.Sin(phase + i * 0.2f) * 5;
                var outer = inner + length;

                var x1 = center.X + (float)Math.Cos(angle) * inner;
                var y1 = center.Y + (float)Math.Sin(angle) * inner;
                var x2 = center.X + (float)Math.Cos(angle) * outer;
                var y2 = center.Y + (float)Math.Sin(angle) * outer;
                var color = GetSpectrumColor(t); // Color barras radiales; se cambia en GetSpectrumColor.
                var alpha = ClampAlpha(80 + energy * 155);

                using (var pen = new Pen(Color.FromArgb(alpha, color), 1.2f + energy * 3.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        private void DrawOrbitNodes(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
            var nodeCount = 18;

            for (var i = 0; i < nodeCount; i++)
            {
                var t = i / (float)nodeCount;
                var band = GetSpectrumValue(t);
                var angle = t * Math.PI * 2 + phase * (0.52f + i % 3 * 0.09f);
                var radius = Math.Min(bounds.Width, bounds.Height) * (0.35f + 0.035f * (i % 3)) + band * 72 + currentFrame.Pulse * 34;
                var x = center.X + (float)Math.Cos(angle) * radius;
                var y = center.Y + (float)Math.Sin(angle) * radius;
                var size = 4 + band * 11 + currentFrame.Pulse * 5;
                var color = GetSpectrumColor(t); // Color bolitas alrededor; se cambia en GetSpectrumColor.

                using (var glowBrush = new SolidBrush(Color.FromArgb(38, color)))
                using (var brush = new SolidBrush(Color.FromArgb(210, color)))
                {
                    graphics.FillEllipse(glowBrush, x - size * 1.9f, y - size * 1.9f, size * 3.8f, size * 3.8f);
                    graphics.FillEllipse(brush, x - size / 2f, y - size / 2f, size, size);
                }
            }
        }

        private void DrawCore(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
            var coreRadius = 26 + currentFrame.Bass * 58 + currentFrame.Pulse * 44;

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(center.X - coreRadius, center.Y - coreRadius, coreRadius * 2, coreRadius * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(238, 255, 255, 245); // Color bola blanca central.
                    brush.SurroundColors = new[] { Color.FromArgb(30, 255, 95, 170) }; // Color brillo alrededor de la bola.
                    graphics.FillPath(brush, path);
                }
            }

            using (var pen = new Pen(Color.FromArgb(210, 41, 221, 218), 2.2f + currentFrame.Pulse * 3f)) // Color aro alrededor de la bola.
            {
                var radius = coreRadius + 22 + currentFrame.Pulse * 24;
                graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
            }
        }

        private void DrawTinyStars(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            for (var i = 0; i < 90; i++)
            {
                var x = (float)((Math.Sin(i * 12.9898) * 43758.5453) % 1);
                if (x < 0) x += 1;
                var y = (float)((Math.Sin(i * 78.233) * 24634.6345) % 1);
                if (y < 0) y += 1;

                var twinkle = 0.45f + 0.55f * (float)Math.Sin(phase * 2.2f + i);
                var size = 1f + twinkle * 2f + currentFrame.Treble * 1.8f;
                var alpha = ClampAlpha(22 + twinkle * 80 + currentFrame.Treble * 70);
                var color = i % 2 == 0 ? Color.FromArgb(alpha, 41, 221, 218) : Color.FromArgb(alpha, 255, 95, 170); // Color estrellas/fonditos.

                using (var brush = new SolidBrush(color))
                {
                    graphics.FillEllipse(brush, bounds.Left + x * bounds.Width, bounds.Top + y * bounds.Height, size, size);
                }
            }
        }

        private float GetSpectrumValue(float normalizedIndex)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return currentFrame.Intensity * 0.20f;
            }

            var exactIndex = normalizedIndex * (currentFrame.Spectrum.Length - 1);
            var left = Math.Max(0, (int)Math.Floor(exactIndex));
            var right = Math.Min(currentFrame.Spectrum.Length - 1, left + 1);
            var fraction = exactIndex - left;
            return Clamp((float)(currentFrame.Spectrum[left] * (1 - fraction) + currentFrame.Spectrum[right] * fraction));
        }

        private static Color GetSpectrumColor(float t)
        {
            if (t < 0.20f)
            {
                // Color gradiente 1: rosa fuerte a rojo.
                return Blend(Color.FromArgb(255, 70, 170), Color.FromArgb(255, 95, 115), t / 0.20f);
            }

            if (t < 0.45f)
            {
                // Color gradiente 2: rojo a amarillo.
                return Blend(Color.FromArgb(255, 95, 115), Color.FromArgb(255, 215, 86), (t - 0.20f) / 0.25f);
            }

            if (t < 0.70f)
            {
                // Color gradiente 3: amarillo a celeste.
                return Blend(Color.FromArgb(255, 215, 86), Color.FromArgb(41, 221, 218), (t - 0.45f) / 0.25f);
            }

            // Color gradiente 4: celeste a morado.
            return Blend(Color.FromArgb(41, 221, 218), Color.FromArgb(126, 118, 255), (t - 0.70f) / 0.30f);
        }

        private static Color Blend(Color from, Color to, float amount)
        {
            amount = Clamp(amount);
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * amount),
                (int)(from.G + (to.G - from.G) * amount),
                (int)(from.B + (to.B - from.B) * amount));
        }

        private static int ClampAlpha(float value)
        {
            return Math.Max(0, Math.Min(255, (int)value));
        }

        private static float Clamp(float value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > 1 ? 1 : value;
        }
    }
}
