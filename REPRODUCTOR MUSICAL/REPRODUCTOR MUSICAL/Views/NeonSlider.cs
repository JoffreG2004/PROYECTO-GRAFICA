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
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            DoubleBuffered = true;
            Height = 34;
            BackColor = Color.FromArgb(12, 22, 39);
            TrackColor = Color.FromArgb(40, 53, 72);
            FillColor = Color.FromArgb(41, 221, 218);
            ThumbColor = Color.FromArgb(41, 221, 218);
            TickStyle = TickStyle.None;
            Cursor = Cursors.Hand;
            TabStop = true;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var background = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(background, ClientRectangle);
            }

            var trackRect = new RectangleF(12, Height / 2f - 3, Width - 24, 6);
            var ratio = (Value - Minimum) / (float)(Maximum - Minimum);
            var fillRect = new RectangleF(trackRect.X, trackRect.Y, trackRect.Width * ratio, trackRect.Height);
            var thumbX = trackRect.X + trackRect.Width * ratio;

            using (var trackBrush = new SolidBrush(TrackColor))
            using (var fillBrush = new LinearGradientBrush(trackRect, FillColor, Color.FromArgb(77, 178, 255), LinearGradientMode.Horizontal))
            using (var thumbBrush = new SolidBrush(ThumbColor))
            using (var glowBrush = new SolidBrush(Color.FromArgb(55, ThumbColor)))
            using (var focusPen = new Pen(Color.FromArgb(Focused ? 80 : 0, FillColor), 1))
            {
                FillRound(e.Graphics, trackBrush, trackRect, 3);
                FillRound(e.Graphics, fillBrush, fillRect, 3);
                e.Graphics.FillEllipse(glowBrush, thumbX - 9, Height / 2f - 9, 18, 18);
                e.Graphics.FillEllipse(thumbBrush, thumbX - 6, Height / 2f - 6, 12, 12);
                if (Focused)
                {
                    e.Graphics.DrawRectangle(focusPen, 1, 1, Width - 3, Height - 3);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
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
