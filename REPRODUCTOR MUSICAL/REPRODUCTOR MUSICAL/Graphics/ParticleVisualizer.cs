using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public class ParticleVisualizer : IVisualizer
    {
        private const int ParticleCount = 86;
        private float phase;
        private AudioFrame currentFrame = new AudioFrame(0.16f, 0.16f, 0.16f, 0.16f, false);

        public string Name => "Particulas ritmicas";

        public void Update(AudioFrame audioFrame)
        {
            currentFrame = audioFrame;
            phase += 0.09f + audioFrame.Intensity * 0.14f + audioFrame.Pulse * 0.18f;
        }

        public void Render(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(
                bounds,
                Color.FromArgb(7, 9, 15),
                Color.FromArgb(28, 21, 33),
                LinearGradientMode.BackwardDiagonal))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var centerX = bounds.Width / 2f;
            var centerY = bounds.Height / 2f;
            var baseRadius = Math.Min(bounds.Width, bounds.Height) * (0.13f + currentFrame.Pulse * 0.08f);

            for (var index = 0; index < ParticleCount; index++)
            {
                var angle = index * Math.PI * 2 / ParticleCount + phase * 0.35;
                var band = GetSpectrumValue(index, ParticleCount);
                var orbit = baseRadius + (index % 9) * 18 + band * 155 + currentFrame.Intensity * 55 + currentFrame.Pulse * 70;
                var distortion = Math.Sin(phase + index * 0.48) * (36 * band + currentFrame.Pulse * 46);
                var x = centerX + Math.Cos(angle) * (orbit + distortion);
                var y = centerY + Math.Sin(angle) * (orbit - distortion);
                var size = 3 + (index % 5) + band * 20 + currentFrame.Pulse * 7;
                var alpha = Math.Min(245, 120 + (int)(band * 115) + (int)(currentFrame.Pulse * 40));
                var color = index % 3 == 0
                    ? Color.FromArgb(alpha, 109, 240, 214)
                    : index % 3 == 1
                        ? Color.FromArgb(alpha, 255, 200, 87)
                        : Color.FromArgb(alpha, 239, 94, 115);

                using (var brush = new SolidBrush(color))
                {
                    graphics.FillEllipse(brush, (float)x, (float)y, size, size);
                }
            }
        }

        private float GetSpectrumValue(int index, int count)
        {
            if (currentFrame.Spectrum.Length == 0)
            {
                return currentFrame.Intensity * 0.25f;
            }

            var spectrumIndex = Math.Min(
                currentFrame.Spectrum.Length - 1,
                Math.Max(0, index * currentFrame.Spectrum.Length / Math.Max(1, count)));

            return currentFrame.Spectrum[spectrumIndex];
        }
    }
}
