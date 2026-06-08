using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class ParticleVisualizer : IVisualizer
    {
        private const int StreamCount = 7;
        private const int StreamParticles = 46;
        private const int ConstellationNodes = 54;
        private const int CrystalCount = 18;
        private float phase;
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);

        public string Name => "Particulas ritmicas";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            phase += 0.050f + audioFrame.Intensity * 0.10f + audioFrame.Pulse * 0.16f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            DrawBackground(graphics, bounds);
            DrawAuroraVeils(graphics, bounds);
            DrawConstellation(graphics, bounds);
            DrawEnergyStreams(graphics, bounds);
            DrawBeatCrystals(graphics, bounds);
            DrawFineSparkles(graphics, bounds);
        }

        private void DrawBackground(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            using (var brush = new LinearGradientBrush(
                bounds,
                Color.FromArgb(5, 8, 16),
                Color.FromArgb(18, 13, 34),
                LinearGradientMode.ForwardDiagonal))
            {
                graphics.FillRectangle(brush, bounds);
            }

            DrawGlow(graphics, bounds, bounds.Width * 0.25f, bounds.Height * 0.35f, Color.FromArgb(255, 95, 170), 0.34f);
            DrawGlow(graphics, bounds, bounds.Width * 0.78f, bounds.Height * 0.28f, Color.FromArgb(41, 221, 218), 0.34f);
            DrawGlow(graphics, bounds, bounds.Width * 0.50f, bounds.Height * 0.78f, Color.FromArgb(255, 215, 86), 0.28f);
        }

        private void DrawGlow(System.Drawing.Graphics graphics, Rectangle bounds, float x, float y, Color color, float scale)
        {
            var radius = Math.Min(bounds.Width, bounds.Height) * (scale + currentFrame.Intensity * 0.05f + currentFrame.Pulse * 0.06f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x - radius, y - radius, radius * 2, radius * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(34 + (int)(currentFrame.Pulse * 34), color);
                    brush.SurroundColors = new[] { Color.FromArgb(0, color) };
                    graphics.FillPath(brush, path);
                }
            }
        }

        private void DrawAuroraVeils(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            for (var layer = 0; layer < 4; layer++)
            {
                var points = new PointF[90];
                var baseY = bounds.Height * (0.28f + layer * 0.14f);
                var color = layer == 0 ? Color.FromArgb(255, 95, 170)
                    : layer == 1 ? Color.FromArgb(255, 215, 86)
                    : layer == 2 ? Color.FromArgb(41, 221, 218)
                    : Color.FromArgb(126, 118, 255);

                for (var i = 0; i < points.Length; i++)
                {
                    var t = i / (float)(points.Length - 1);
                    var band = GetSpectrumValue((t + layer * 0.17f) % 1f);
                    var x = t * bounds.Width;
                    var wave = Math.Sin(phase * (1.2f + layer * 0.22f) + t * Math.PI * (3.5f + layer));
                    var y = baseY + (float)wave * (22 + band * 95 + currentFrame.Pulse * 28);
                    points[i] = new PointF(x, y);
                }

                using (var glowPen = new Pen(Color.FromArgb(28 + layer * 4, color), 16 - layer * 2) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                using (var pen = new Pen(Color.FromArgb(110, color), 1.6f + currentFrame.Pulse * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                {
                    graphics.DrawLines(glowPen, points);
                    graphics.DrawLines(pen, points);
                }
            }
        }

        private void DrawEnergyStreams(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            for (var stream = 0; stream < StreamCount; stream++)
            {
                var offset = stream / (float)(StreamCount - 1);
                var streamColor = GetParticleColor(offset, 220);

                for (var i = 0; i < StreamParticles; i++)
                {
                    var t = i / (float)(StreamParticles - 1);
                    var band = GetSpectrumValue((t * 0.75f + offset * 0.38f) % 1f);
                    var flow = (t + phase * (0.045f + stream * 0.004f) + offset) % 1f;
                    var x = bounds.Width * flow;
                    var diagonal = bounds.Height * (0.18f + offset * 0.68f) + (flow - 0.5f) * bounds.Height * 0.30f;
                    var y = diagonal + (float)Math.Sin(phase * 2.0f + i * 0.34f + stream) * (24 + band * 90);
                    var size = 2.2f + band * 13 + currentFrame.Pulse * 3.5f + (stream % 3) * 0.8f;
                    var alpha = ClampAlpha(78 + band * 140 + currentFrame.Pulse * 35);
                    var color = BlendColor(streamColor, GetParticleColor(t, 255), 0.38f + band * 0.28f);

                    DrawGlowParticle(graphics, x, y, size, Color.FromArgb(alpha, color));

                    if (i > 0 && i % 4 == 0)
                    {
                        var trailX = x - bounds.Width * 0.035f;
                        var trailY = y - 10 - band * 20;
                        using (var pen = new Pen(Color.FromArgb(28 + (int)(band * 55), color), 1.1f + band * 1.3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            graphics.DrawLine(pen, trailX, trailY, x, y);
                        }
                    }
                }
            }
        }

        private void DrawConstellation(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var points = new PointF[ConstellationNodes];
            var energies = new float[ConstellationNodes];

            for (var i = 0; i < ConstellationNodes; i++)
            {
                var t = i / (float)ConstellationNodes;
                var band = GetSpectrumValue(t);
                var baseX = bounds.Width * (0.12f + 0.76f * Hash01(i * 11.7f));
                var baseY = bounds.Height * (0.16f + 0.68f * Hash01(i * 29.4f));
                var driftX = (float)Math.Sin(phase * 1.1f + i * 0.61f) * (12 + band * 45 + currentFrame.Treble * 20);
                var driftY = (float)Math.Cos(phase * 1.3f + i * 0.48f) * (10 + band * 38 + currentFrame.Mid * 18);
                points[i] = new PointF(baseX + driftX, baseY + driftY);
                energies[i] = band;
            }

            for (var i = 0; i < ConstellationNodes; i++)
            {
                for (var step = 1; step <= 2; step++)
                {
                    var other = (i + 9 * step) % ConstellationNodes;
                    var dx = points[i].X - points[other].X;
                    var dy = points[i].Y - points[other].Y;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance > bounds.Width * 0.20f)
                    {
                        continue;
                    }

                    var energy = (energies[i] + energies[other]) * 0.5f;
                    var alpha = ClampAlpha(14 + energy * 65 + currentFrame.Pulse * 16);
                    using (var pen = new Pen(Color.FromArgb(alpha, 109, 240, 214), 1f + energy * 1.1f))
                    {
                        graphics.DrawLine(pen, points[i], points[other]);
                    }
                }
            }

            for (var i = 0; i < ConstellationNodes; i++)
            {
                var color = GetParticleColor(i / (float)ConstellationNodes, ClampAlpha(82 + energies[i] * 120));
                DrawGlowParticle(graphics, points[i].X, points[i].Y, 3.5f + energies[i] * 12 + currentFrame.Pulse * 2.5f, color);
            }
        }

        private void DrawBeatCrystals(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            for (var i = 0; i < CrystalCount; i++)
            {
                var t = i / (float)CrystalCount;
                var band = GetSpectrumValue((t + 0.2f) % 1f);
                var x = bounds.Width * (0.10f + Hash01(i * 7.13f) * 0.80f);
                var y = bounds.Height * (0.14f + Hash01(i * 5.91f) * 0.72f);
                var radius = 8 + band * 28 + currentFrame.Pulse * 24;
                var rotation = phase * (0.8f + i * 0.02f) + i;
                var color = GetParticleColor(t, ClampAlpha(42 + band * 95 + currentFrame.Pulse * 115));

                DrawCrystal(graphics, new PointF(x, y), radius, rotation, color);
            }
        }

        private void DrawCrystal(System.Drawing.Graphics graphics, PointF center, float radius, float rotation, Color color)
        {
            var points = new PointF[4];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = rotation + Math.PI / 4 + i * Math.PI * 2 / points.Length;
                var r = i % 2 == 0 ? radius : radius * 0.48f;
                points[i] = new PointF(center.X + (float)Math.Cos(angle) * r, center.Y + (float)Math.Sin(angle) * r);
            }

            using (var brush = new SolidBrush(Color.FromArgb(Math.Min(50, color.A / 3), color)))
            using (var pen = new Pen(color, 1.2f + currentFrame.Pulse * 1.2f) { LineJoin = LineJoin.Round })
            {
                graphics.FillPolygon(brush, points);
                graphics.DrawPolygon(pen, points);
            }
        }

        private void DrawFineSparkles(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            for (var i = 0; i < 85; i++)
            {
                var t = i / 85f;
                var band = GetSpectrumValue(t);
                var x = bounds.Width * Hash01(i * 37.1f) + (float)Math.Sin(phase + i) * 12;
                var y = bounds.Height * Hash01(i * 53.8f) + (float)Math.Cos(phase * 1.4f + i) * 10;
                var size = 1f + band * 3.4f + currentFrame.Pulse * 1.4f;
                var color = GetParticleColor(t, ClampAlpha(18 + band * 95 + currentFrame.Treble * 60));

                using (var brush = new SolidBrush(color))
                {
                    graphics.FillEllipse(brush, x, y, size, size);
                }
            }
        }

        private void DrawGlowParticle(System.Drawing.Graphics graphics, float x, float y, float size, Color color)
        {
            using (var glowBrush = new SolidBrush(Color.FromArgb(Math.Min(72, (int)color.A / 3), color)))
            using (var brush = new SolidBrush(color))
            using (var hotBrush = new SolidBrush(Color.FromArgb(Math.Min(230, (int)color.A), 255, 255, 245)))
            {
                graphics.FillEllipse(glowBrush, x - size * 1.8f, y - size * 1.8f, size * 3.6f, size * 3.6f);
                graphics.FillEllipse(brush, x - size / 2f, y - size / 2f, size, size);
                var hot = Math.Max(1f, size * 0.26f);
                graphics.FillEllipse(hotBrush, x - hot / 2f, y - hot / 2f, hot, hot);
            }
        }

        private float GetSpectrumValue(float normalizedIndex)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return currentFrame.Intensity * 0.24f;
            }

            normalizedIndex = normalizedIndex - (float)Math.Floor(normalizedIndex);
            var exactIndex = normalizedIndex * (currentFrame.Spectrum.Length - 1);
            var left = Math.Max(0, (int)Math.Floor(exactIndex));
            var right = Math.Min(currentFrame.Spectrum.Length - 1, left + 1);
            var fraction = exactIndex - left;
            return Clamp((float)(currentFrame.Spectrum[left] * (1 - fraction) + currentFrame.Spectrum[right] * fraction));
        }

        private static Color GetParticleColor(float t, int alpha)
        {
            if (t < 0.20f)
            {
                return Color.FromArgb(alpha, BlendColor(Color.FromArgb(255, 95, 170), Color.FromArgb(255, 108, 105), t / 0.20f));
            }

            if (t < 0.44f)
            {
                return Color.FromArgb(alpha, BlendColor(Color.FromArgb(255, 108, 105), Color.FromArgb(255, 215, 86), (t - 0.20f) / 0.24f));
            }

            if (t < 0.72f)
            {
                return Color.FromArgb(alpha, BlendColor(Color.FromArgb(255, 215, 86), Color.FromArgb(41, 221, 218), (t - 0.44f) / 0.28f));
            }

            return Color.FromArgb(alpha, BlendColor(Color.FromArgb(41, 221, 218), Color.FromArgb(126, 118, 255), (t - 0.72f) / 0.28f));
        }

        private static Color BlendColor(Color from, Color to, float amount)
        {
            amount = Clamp(amount);
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * amount),
                (int)(from.G + (to.G - from.G) * amount),
                (int)(from.B + (to.B - from.B) * amount));
        }

        private static float Hash01(float value)
        {
            var raw = Math.Sin(value * 12.9898) * 43758.5453;
            return (float)(raw - Math.Floor(raw));
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
