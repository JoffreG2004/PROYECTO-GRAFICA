using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class SpectrumBarsVisualizer : IVisualizer
    {
        private const int BarCount = 96;
        private float phase;
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);

        public string Name => "Barras de espectro";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            phase += 0.045f + audioFrame.Intensity * 0.08f + audioFrame.Pulse * 0.10f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            DrawBackground(graphics, bounds);
            DrawPulseRings(graphics, bounds);
            DrawParticles(graphics, bounds);
            DrawEnergyLine(graphics, bounds);
            DrawSpectrum(graphics, bounds);
        }

        private void DrawSpectrum(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var margin = 52;
            var availableWidth = Math.Max(1, bounds.Width - margin * 2);
            var centerY = bounds.Height / 2f;
            var slot = availableWidth / BarCount;
            var barWidth = Math.Max(3.5f, slot * 0.48f);

            for (var index = 0; index < BarCount; index++)
            {
                var x = margin + index * slot + (slot - barWidth) / 2f;
                var normalizedIndex = index / (float)(BarCount - 1);
                var spectrumValue = GetSpectrumValue(normalizedIndex);
                var musicalMotion = currentFrame.UsesRealSamples
                    ? spectrumValue
                    : 0.14f + (float)((Math.Sin(phase + index * 0.22) + 1) * 0.08);
                var contour = 0.52f + 0.48f * (float)Math.Sin(normalizedIndex * Math.PI);
                var energy = Clamp(musicalMotion * 0.86f + currentFrame.Intensity * 0.20f + currentFrame.Pulse * 0.18f);
                var height = 9 + energy * contour * bounds.Height * 0.42f;
                var color = GetSpectrumColor(normalizedIndex);

                DrawGlowBar(graphics, x, centerY, barWidth, height, color);
                DrawReflectionBar(graphics, x, centerY, barWidth, height, color);
            }
        }

        private void DrawGlowBar(System.Drawing.Graphics graphics, float x, float centerY, float width, float height, Color color)
        {
            var y = centerY - height;
            var mainRectangle = new RectangleF(x, y, width, height);
            var glowRectangle = new RectangleF(x - width * 0.65f, y - 8, width * 2.3f, height + 16);

            using (var glowBrush = new LinearGradientBrush(
                glowRectangle,
                Color.FromArgb(0, color),
                Color.FromArgb(90, color),
                LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(glowBrush, glowRectangle);
            }

            using (var brush = new LinearGradientBrush(
                mainRectangle,
                Color.FromArgb(255, 255, 255, 245),
                color,
                LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(brush, mainRectangle);
            }

            using (var pen = new Pen(Color.FromArgb(135, 255, 255, 255), 1))
            {
                graphics.DrawLine(pen, x + width / 2f, y, x + width / 2f, y + height);
            }
        }

        private void DrawReflectionBar(System.Drawing.Graphics graphics, float x, float centerY, float width, float height, Color color)
        {
            var reflectedHeight = height * 0.52f;
            var reflectionRectangle = new RectangleF(x, centerY + 4, width, reflectedHeight);

            using (var brush = new LinearGradientBrush(
                reflectionRectangle,
                Color.FromArgb(95, color),
                Color.FromArgb(0, color),
                LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(brush, reflectionRectangle);
            }
        }

        private void DrawEnergyLine(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var centerY = bounds.Height / 2f;

            using (var glowPen = new Pen(Color.FromArgb(115, 109, 240, 214), 5))
            using (var linePen = new Pen(Color.FromArgb(230, 255, 255, 255), 1.4f))
            {
                graphics.DrawLine(glowPen, 42, centerY, bounds.Width - 42, centerY);
                graphics.DrawLine(linePen, 42, centerY, bounds.Width - 42, centerY);
            }
        }

        private void DrawPulseRings(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var center = new PointF(bounds.Width * 0.50f, bounds.Height * 0.50f);
            var radiusBase = Math.Min(bounds.Width, bounds.Height) * (0.19f + currentFrame.Intensity * 0.08f);

            for (var i = 0; i < 4; i++)
            {
                var radius = radiusBase + i * 42 + currentFrame.Pulse * (44 + i * 12);
                var alpha = ClampAlpha(42 - i * 7 + currentFrame.Pulse * 55);

                using (var pen = new Pen(Color.FromArgb(alpha, 109, 240, 214), 1.2f + currentFrame.Pulse * 1.5f))
                {
                    graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
                }
            }
        }

        private void DrawParticles(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            var centerY = bounds.Height / 2f;
            var count = 130;

            for (var i = 0; i < count; i++)
            {
                var t = i / (float)(count - 1);
                var spectrum = GetSpectrumValue(t);
                var color = GetSpectrumColor(t);
                var wave = Math.Sin(phase * 1.8 + i * 0.44);
                var x = 48 + t * (bounds.Width - 96);
                var y = centerY + (float)wave * (26 + spectrum * 120) * (i % 2 == 0 ? 1 : -1);
                var size = 1.4f + spectrum * 4.2f + currentFrame.Pulse * 2.2f;
                var alpha = ClampAlpha(55 + spectrum * 135 + currentFrame.Pulse * 45);

                using (var brush = new SolidBrush(Color.FromArgb(alpha, color)))
                {
                    graphics.FillEllipse(brush, x - size / 2, y - size / 2, size, size);
                }
            }
        }

        private void DrawBackground(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            using (var brush = new LinearGradientBrush(
                bounds,
                Color.FromArgb(5, 8, 16),
                Color.FromArgb(17, 13, 34),
                LinearGradientMode.ForwardDiagonal))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var centerX = bounds.Width / 2f;
            var centerY = bounds.Height / 2f;
            var radius = Math.Min(bounds.Width, bounds.Height) * (0.28f + currentFrame.Intensity * 0.10f + currentFrame.Pulse * 0.12f);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(66 + (int)(currentFrame.Pulse * 45), 80, 50, 190);
                    brush.SurroundColors = new[] { Color.FromArgb(0, 80, 50, 190) };
                    graphics.FillPath(brush, path);
                }
            }
        }

        private float GetSpectrumValue(float normalizedIndex)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return currentFrame.Intensity * 0.18f;
            }

            var exactIndex = normalizedIndex * (currentFrame.Spectrum.Length - 1);
            var left = Math.Max(0, (int)Math.Floor(exactIndex));
            var right = Math.Min(currentFrame.Spectrum.Length - 1, left + 1);
            var fraction = exactIndex - left;
            var value = currentFrame.Spectrum[left] * (1 - fraction) + currentFrame.Spectrum[right] * fraction;

            return Clamp((float)value);
        }

        private static Color GetSpectrumColor(float t)
        {
            if (t < 0.18f)
            {
                return Blend(Color.FromArgb(255, 65, 170), Color.FromArgb(255, 95, 110), t / 0.18f);
            }

            if (t < 0.38f)
            {
                return Blend(Color.FromArgb(255, 95, 110), Color.FromArgb(255, 222, 88), (t - 0.18f) / 0.20f);
            }

            if (t < 0.62f)
            {
                return Blend(Color.FromArgb(255, 222, 88), Color.FromArgb(70, 245, 210), (t - 0.38f) / 0.24f);
            }

            if (t < 0.82f)
            {
                return Blend(Color.FromArgb(70, 245, 210), Color.FromArgb(70, 170, 255), (t - 0.62f) / 0.20f);
            }

            return Blend(Color.FromArgb(70, 170, 255), Color.FromArgb(205, 95, 255), (t - 0.82f) / 0.18f);
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
