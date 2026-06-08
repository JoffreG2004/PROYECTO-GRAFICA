using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace REPRODUCTOR_MUSICAL.Views
{
    public class NeonSlider : Control
    {
        private int minimum;
        private int maximum = 100;
        private int value;

        public NeonSlider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
            DoubleBuffered = true;
            Height = 24;
            BackColor = Color.FromArgb(12, 22, 39);
            TrackColor = Color.FromArgb(43, 55, 77);
            FillColor = Color.FromArgb(41, 221, 218);
            ThumbColor = Color.FromArgb(41, 221, 218);
            TickStyle = TickStyle.None;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        public event EventHandler Scroll;

        public int Minimum
        {
            get => minimum;
            set
            {
                minimum = value;
                Value = this.value;
            }
        }

        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = Math.Max(minimum + 1, value);
                Value = this.value;
            }
        }

        public int Value
        {
            get => value;
            set
            {
                this.value = Math.Max(minimum, Math.Min(maximum, value));
                Invalidate();
            }
        }

        public int TickFrequency { get; set; }

        public TickStyle TickStyle { get; set; }

        public Color TrackColor { get; set; }

        public Color FillColor { get; set; }

        public Color ThumbColor { get; set; }

        protected override bool ShowFocusCues => false;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.Clear(BackColor);

            var trackRect = new RectangleF(12, Height / 2f - 3, Math.Max(1, Width - 24), 6);
            var ratio = (Value - Minimum) / (float)(Maximum - Minimum);
            var fillWidth = Math.Max(0, trackRect.Width * ratio);
            var thumbX = trackRect.X + fillWidth;

            using (var trackBrush = new SolidBrush(TrackColor))
            using (var fillBrush = new SolidBrush(FillColor))
            using (var thumbBrush = new SolidBrush(ThumbColor))
            using (var thumbGlow = new SolidBrush(Color.FromArgb(46, ThumbColor)))
            {
                FillRound(e.Graphics, trackBrush, trackRect, 3);

                if (fillWidth > 0.5f)
                {
                    FillRound(e.Graphics, fillBrush, new RectangleF(trackRect.X, trackRect.Y, fillWidth, trackRect.Height), 3);
                }

                e.Graphics.FillEllipse(thumbGlow, thumbX - 9, Height / 2f - 9, 18, 18);
                e.Graphics.FillEllipse(thumbBrush, thumbX - 6, Height / 2f - 6, 12, 12);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            SetValueFromMouse(e.X);
            Scroll?.Invoke(this, EventArgs.Empty);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetValueFromMouse(e.X);
                Scroll?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseMove(e);
        }

        private void SetValueFromMouse(int mouseX)
        {
            var ratio = Math.Max(0, Math.Min(1, (mouseX - 12) / (double)Math.Max(1, Width - 24)));
            Value = Minimum + (int)Math.Round(ratio * (Maximum - Minimum));
        }

        private static void FillRound(System.Drawing.Graphics graphics, Brush brush, RectangleF rect, float radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }
    }
}
