using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class CircularWavesVisualizer : IVisualizer
    {
        private float phase;
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);

        public string Name => "Ondas circulares";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            phase += 0.07f + audioFrame.Intensity * 0.08f + audioFrame.Pulse * 0.12f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(
                bounds,
                Color.FromArgb(7, 9, 15),
                Color.FromArgb(24, 19, 37),
                LinearGradientMode.ForwardDiagonal))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var centerX = bounds.Width / 2f;
            var centerY = bounds.Height / 2f;
            var maxRadius = Math.Min(bounds.Width, bounds.Height) * 0.42f;

            for (var index = 0; index < 9; index++)
            {
                var band = AverageSpectrum(index * 5, 5);
                var pulse = 0.35f + band * 0.65f;
                var radius = 28 + index * maxRadius / 9 + pulse * 72 * band + currentFrame.Pulse * (24 + index * 4);
                var alpha = Math.Min(240, Math.Max(35, 150 - index * 14 + (int)(currentFrame.Pulse * 120)));
                var color = index % 2 == 0
                    ? Color.FromArgb(alpha, 109, 240, 214)
                    : Color.FromArgb(alpha, 255, 200, 87);

                using (var pen = new Pen(color, 2 + band * 5 + currentFrame.Pulse * 4))
                {
                    graphics.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }

            var coreRadius = 22 + currentFrame.Intensity * 80 + currentFrame.Pulse * 70;
            using (var brush = new SolidBrush(Color.FromArgb(210, 239, 94, 115)))
            {
                graphics.FillEllipse(brush, centerX - coreRadius, centerY - coreRadius, coreRadius * 2, coreRadius * 2);
            }
        }

        private float AverageSpectrum(int start, int length)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return currentFrame.Intensity * 0.25f;
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
