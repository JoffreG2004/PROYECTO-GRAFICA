using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace REPRODUCTOR_MUSICAL.Views
{
    public class NeonComboBox : ComboBox
    {
        private const int WmPaint = 0x000F;

        public NeonComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.FromArgb(10, 18, 32);
            ForeColor = Color.White;
            BorderColor = Color.FromArgb(41, 221, 218);
            AccentColor = Color.FromArgb(41, 221, 218);
            ItemHeight = 30;
            IntegralHeight = false;
        }

        public Color BorderColor { get; set; }

        public Color AccentColor { get; set; }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var brush = new SolidBrush(selected ? Color.FromArgb(19, 40, 61) : BackColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            DrawBarsIcon(e.Graphics, new Rectangle(e.Bounds.Left + 10, e.Bounds.Top + 8, 20, 16));

            TextRenderer.DrawText(
                e.Graphics,
                Items[e.Index].ToString(),
                Font,
                new Rectangle(e.Bounds.Left + 38, e.Bounds.Top, e.Bounds.Width - 42, e.Bounds.Height),
                ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg != WmPaint)
            {
                return;
            }

            using (var graphics = System.Drawing.Graphics.FromHwnd(Handle))
            using (var borderPen = new Pen(BorderColor, 1))
            using (var arrowPen = new Pen(Color.FromArgb(210, 235, 245), 2))
            using (var fillBrush = new SolidBrush(BackColor))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                graphics.FillRectangle(fillBrush, rect);
                DrawBarsIcon(graphics, new Rectangle(13, Height / 2 - 8, 20, 16));
                TextRenderer.DrawText(
                    graphics,
                    Text,
                    Font,
                    new Rectangle(43, 0, Width - 76, Height),
                    ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                graphics.DrawRectangle(borderPen, rect);
                graphics.DrawLine(arrowPen, Width - 23, Height / 2 - 3, Width - 17, Height / 2 + 3);
                graphics.DrawLine(arrowPen, Width - 17, Height / 2 + 3, Width - 11, Height / 2 - 3);
            }
        }

        private void DrawBarsIcon(System.Drawing.Graphics graphics, Rectangle bounds)
        {
            using (var cyan = new Pen(AccentColor, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var blue = new Pen(Color.FromArgb(77, 178, 255), 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                var baseY = bounds.Top + bounds.Height;
                graphics.DrawLine(cyan, bounds.Left, baseY - 2, bounds.Left, baseY - 9);
                graphics.DrawLine(blue, bounds.Left + 5, baseY - 2, bounds.Left + 5, baseY - 14);
                graphics.DrawLine(cyan, bounds.Left + 10, baseY - 2, bounds.Left + 10, baseY - 6);
                graphics.DrawLine(blue, bounds.Left + 15, baseY - 2, bounds.Left + 15, baseY - 11);
            }
        }
    }
}
