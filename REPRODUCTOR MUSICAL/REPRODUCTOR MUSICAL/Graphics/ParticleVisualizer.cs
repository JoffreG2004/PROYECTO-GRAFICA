using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class ParticleVisualizer : IVisualizer
    {
        private const int RoadLineCount = 26;
        private const int BuildingCount = 34;
        private const int StarCount = 80;
        private float phase;
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);

        public string Name => "Autopista Neon";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            phase += 0.030f + audioFrame.Intensity * 0.10f + audioFrame.Bass * 0.09f + audioFrame.Pulse * 0.16f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            DrawSky(graphics, bounds);
            DrawSunAndHorizon(graphics, bounds);
            DrawCity(graphics, bounds);
            DrawRoad(graphics, bounds);
            DrawLaneLines(graphics, bounds);
            DrawSpeedTrails(graphics, bounds);
            DrawBeatHeadlights(graphics, bounds);
        }

        private void DrawSky(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            using (var brush = new LinearGradientBrush(
                bounds,
                // Color fondo cielo arriba.
                Color.FromArgb(4, 6, 18),
                // Color fondo cielo abajo.
                Color.FromArgb(28, 8, 45),
                LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(brush, bounds);
            }

            DrawGlow(graphics, bounds, bounds.Width * 0.50f, bounds.Height * 0.36f, Color.FromArgb(255, 67, 181), 0.40f); // Color brillo rosa del cielo.
            DrawGlow(graphics, bounds, bounds.Width * 0.72f, bounds.Height * 0.18f, Color.FromArgb(48, 225, 255), 0.22f); // Color brillo celeste del cielo.

            for (var i = 0; i < StarCount; i++)
            {
                var t = i / (float)StarCount;
                var band = GetSpectrumValue(t);
                var x = bounds.Left + bounds.Width * Hash01(i * 18.17f);
                var y = bounds.Top + bounds.Height * (0.04f + 0.38f * Hash01(i * 43.91f));
                var size = 1f + currentFrame.Treble * 2.1f + band * 2.6f;
                var alpha = ClampAlpha(28 + band * 105 + currentFrame.Pulse * 60);
                using (var brush = new SolidBrush(Color.FromArgb(alpha, 210, 245, 255))) // Color estrellas.
                {
                    graphics.FillEllipse(brush, x, y, size, size);
                }
            }
        }

        private void DrawSunAndHorizon(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var horizonY = bounds.Top + bounds.Height * 0.43f;
            var centerX = bounds.Left + bounds.Width * 0.50f;
            var radius = Math.Min(bounds.Width, bounds.Height) * (0.12f + currentFrame.Bass * 0.035f + currentFrame.Pulse * 0.04f);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(centerX - radius, horizonY - radius * 0.95f, radius * 2, radius * 1.9f);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(210, 255, 184, 70); // Color centro del sol.
                    brush.SurroundColors = new[] { Color.FromArgb(0, 255, 75, 176) }; // Color borde/brillo del sol.
                    graphics.FillPath(brush, path);
                }
            }

            for (var i = 0; i < 7; i++)
            {
                var y = horizonY - radius * 0.7f + i * radius * 0.22f;
                using (var pen = new Pen(Color.FromArgb(70, 7, 9, 25), 3.5f)) // Color franjas oscuras del sol.
                {
                    graphics.DrawLine(pen, centerX - radius, y, centerX + radius, y);
                }
            }

            using (var horizonPen = new Pen(Color.FromArgb(170, 48, 225, 255), 2f + currentFrame.Pulse * 2f)) // Color linea horizonte.
            using (var glowPen = new Pen(Color.FromArgb(60, 255, 67, 181), 8f + currentFrame.Pulse * 8f)) // Color brillo horizonte.
            {
                graphics.DrawLine(glowPen, bounds.Left, horizonY, bounds.Right, horizonY);
                graphics.DrawLine(horizonPen, bounds.Left, horizonY, bounds.Right, horizonY);
            }
        }

        private void DrawCity(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var horizonY = bounds.Top + bounds.Height * 0.43f;
            var centerX = bounds.Left + bounds.Width * 0.50f;

            for (var i = 0; i < BuildingCount; i++)
            {
                var side = i % 2 == 0 ? -1 : 1;
                var depth = (i / 2) / (float)(BuildingCount / 2);
                var band = GetSpectrumValue(depth);
                var width = 18 + depth * 42;
                var height = 34 + band * 130 + currentFrame.Mid * 40 + Hash01(i * 7.2f) * 70;
                var gap = 70 + depth * bounds.Width * 0.42f;
                var x = centerX + side * gap - (side < 0 ? width : 0);
                var y = horizonY - height;
                // Color edificios: izquierda celeste, derecha rosa.
                var color = side < 0 ? Color.FromArgb(
69, 255, 28) : Color.FromArgb(65, 255, 67, 181);

                using (var brush = new LinearGradientBrush(
                    new RectangleF(x, y, width, height),
                    Color.FromArgb(20, 12, 18, 32), // Color base oscuro edificios.
                    Color.FromArgb(120, color),
                    LinearGradientMode.Vertical))
                using (var pen = new Pen(Color.FromArgb(110, color), 1.1f + band * 1.4f))
                {
                    graphics.FillRectangle(brush, x, y, width, height);
                    graphics.DrawRectangle(pen, x, y, width, height);
                }

                DrawWindows(graphics, x, y, width, height, band, color);
            }
        }

        private void DrawWindows(System.Drawing.Graphics graphics, float x, float y, float width, float height, float band, Color color)
        {
            var columns = Math.Max(2, (int)(width / 10));
            var rows = Math.Max(2, (int)(height / 18));
            using (var brush = new SolidBrush(Color.FromArgb(ClampAlpha(35 + band * 135 + currentFrame.Treble * 65), color))) // Color ventanas.
            {
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < columns; col++)
                    {
                        if (Hash01(row * 13.7f + col * 9.3f + width) < 0.42f + band * 0.18f)
                        {
                            var wx = x + 5 + col * (width - 10) / columns;
                            var wy = y + 8 + row * (height - 16) / rows;
                            graphics.FillRectangle(brush, wx, wy, 3.5f, 6f);
                        }
                    }
                }
            }
        }

        private void DrawRoad(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var horizonY = bounds.Top + bounds.Height * 0.43f;
            var centerX = bounds.Left + bounds.Width * 0.50f;
            var roadTopWidth = bounds.Width * (0.10f + currentFrame.Pulse * 0.015f);
            var roadBottomWidth = bounds.Width * (0.92f + currentFrame.Bass * 0.05f);
            var bottomY = bounds.Bottom;

            var road = new[]
            {
                new PointF(centerX - roadTopWidth / 2f, horizonY),
                new PointF(centerX + roadTopWidth / 2f, horizonY),
                new PointF(centerX + roadBottomWidth / 2f, bottomY),
                new PointF(centerX - roadBottomWidth / 2f, bottomY)
            };

            using (var brush = new LinearGradientBrush(
                bounds,
                // Color carretera arriba.
                Color.FromArgb(20, 10, 12, 24),
                // Color carretera abajo.
                Color.FromArgb(210, 8, 11, 24),
                LinearGradientMode.Vertical))
            {
                graphics.FillPolygon(brush, road);
            }

            DrawRoadEdge(graphics, centerX - roadTopWidth / 2f, horizonY, centerX - roadBottomWidth / 2f, bottomY, Color.FromArgb(48, 225, 255)); // Color borde izquierdo carretera.
            DrawRoadEdge(graphics, centerX + roadTopWidth / 2f, horizonY, centerX + roadBottomWidth / 2f, bottomY, Color.FromArgb(255, 67, 181)); // Color borde derecho carretera.
        }

        private void DrawRoadEdge(System.Drawing.Graphics graphics, float x1, float y1, float x2, float y2, Color color)
        {
            using (var glowPen = new Pen(Color.FromArgb(75, color), 10f + currentFrame.Pulse * 6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pen = new Pen(Color.FromArgb(220, color), 2.2f + currentFrame.Bass * 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                graphics.DrawLine(glowPen, x1, y1, x2, y2);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        private void DrawLaneLines(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var horizonY = bounds.Top + bounds.Height * 0.43f;
            var centerX = bounds.Left + bounds.Width * 0.50f;
            var speed = phase * (0.60f + currentFrame.Intensity * 1.6f + currentFrame.Pulse * 1.3f);

            for (var i = 0; i < RoadLineCount; i++)
            {
                var raw = (i / (float)RoadLineCount + speed) % 1f;
                var depth = raw * raw;
                var y = horizonY + depth * (bounds.Bottom - horizonY);
                var nextDepth = Math.Min(1f, depth + 0.035f + currentFrame.Bass * 0.025f);
                var y2 = horizonY + nextDepth * (bounds.Bottom - horizonY);
                var laneOffset = bounds.Width * (0.014f + depth * 0.15f);
                var alpha = ClampAlpha(35 + depth * 190 + currentFrame.Pulse * 50);
                var width = 1.2f + depth * 8f + currentFrame.Bass * 2f;

                DrawLaneSegment(graphics, centerX - laneOffset, y, centerX - laneOffset * 1.10f, y2, Color.FromArgb(alpha, 255, 244, 118), width); // Color linea amarilla izquierda.
                DrawLaneSegment(graphics, centerX + laneOffset, y, centerX + laneOffset * 1.10f, y2, Color.FromArgb(alpha, 255, 244, 118), width); // Color linea amarilla derecha.
                DrawLaneSegment(graphics, centerX, y, centerX, y2, Color.FromArgb(alpha, 255, 255, 255), Math.Max(1f, width * 0.65f)); // Color linea blanca central.
            }
        }

        private static void DrawLaneSegment(System.Drawing.Graphics graphics, float x1, float y1, float x2, float y2, Color color, float width)
        {
            using (var glowPen = new Pen(Color.FromArgb(Math.Min(80, color.A / 2), color), width + 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pen = new Pen(color, width) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                graphics.DrawLine(glowPen, x1, y1, x2, y2);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        private void DrawSpeedTrails(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var horizonY = bounds.Top + bounds.Height * 0.43f;
            var intensity = Math.Max(currentFrame.Intensity, currentFrame.Pulse);

            for (var i = 0; i < 28; i++)
            {
                var t = (Hash01(i * 12.31f) + phase * (0.35f + intensity)) % 1f;
                var side = i % 2 == 0 ? -1 : 1;
                var y = horizonY + t * (bounds.Bottom - horizonY);
                var x = bounds.Width * (side < 0 ? 0.04f + t * 0.20f : 0.96f - t * 0.20f);
                var length = 26 + t * 100 + intensity * 80;
                var color = side < 0 ? Color.FromArgb(48, 225, 255) : Color.FromArgb(255, 67, 181); // Color estelas laterales.
                var alpha = ClampAlpha(20 + t * 90 + intensity * 80);

                using (var pen = new Pen(Color.FromArgb(alpha, color), 1.2f + t * 3.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    graphics.DrawLine(pen, x, y, x - side * length, y + length * 0.15f);
                }
            }
        }

        private void DrawBeatHeadlights(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var beat = Math.Max(currentFrame.Bass, currentFrame.Pulse);
            var y = bounds.Bottom - bounds.Height * 0.16f;
            var centerX = bounds.Left + bounds.Width * 0.50f;
            var spread = bounds.Width * (0.055f + beat * 0.025f);
            var size = 16 + beat * 38;

            DrawGlow(graphics, bounds, centerX - spread, y, Color.FromArgb(48, 225, 255), 0.10f + beat * 0.06f); // Color brillo faro izquierdo.
            DrawGlow(graphics, bounds, centerX + spread, y, Color.FromArgb(255, 67, 181), 0.10f + beat * 0.06f); // Color brillo faro derecho.

            using (var cyan = new SolidBrush(Color.FromArgb(190, 48, 225, 255))) // Color faro izquierdo.
            using (var pink = new SolidBrush(Color.FromArgb(190, 255, 67, 181))) // Color faro derecho.
            {
                graphics.FillEllipse(cyan, centerX - spread - size / 2f, y - size / 2f, size, size * 0.55f);
                graphics.FillEllipse(pink, centerX + spread - size / 2f, y - size / 2f, size, size * 0.55f);
            }
        }

        private void DrawGlow(System.Drawing.Graphics graphics, Rectangle bounds, float x, float y, Color color, float scale)
        {
            var radius = Math.Min(bounds.Width, bounds.Height) * (scale + currentFrame.Intensity * 0.04f + currentFrame.Pulse * 0.05f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x - radius, y - radius, radius * 2, radius * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(32 + (int)(currentFrame.Pulse * 42), color);
                    brush.SurroundColors = new[] { Color.FromArgb(0, color) };
                    graphics.FillPath(brush, path);
                }
            }
        }

        private float GetSpectrumValue(float normalizedIndex)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return Math.Max(0.04f, currentFrame.Intensity * 0.30f);
            }

            normalizedIndex = normalizedIndex - (float)Math.Floor(normalizedIndex);
            var exactIndex = normalizedIndex * (currentFrame.Spectrum.Length - 1);
            var left = Math.Max(0, (int)Math.Floor(exactIndex));
            var right = Math.Min(currentFrame.Spectrum.Length - 1, left + 1);
            var fraction = exactIndex - left;
            return Clamp((float)(currentFrame.Spectrum[left] * (1 - fraction) + currentFrame.Spectrum[right] * fraction));
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
