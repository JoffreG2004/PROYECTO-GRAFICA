using System.Drawing;
using System.Drawing.Drawing2D;

namespace REPRODUCTOR_MUSICAL.Views
{
    public class MusicIconPanel : RoundedPanel
    {
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            FillColor = Color.FromArgb(39, 26, 77);
            BorderColor = Color.FromArgb(126, 72, 220);
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(41, 221, 218), 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pinkPen = new Pen(Color.FromArgb(255, 95, 170), 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                var centerY = Height / 2;
                for (var i = 0; i < 7; i++)
                {
                    var x = 17 + i * 7;
                    var height = 10 + (i % 3) * 8;
                    var activePen = i % 2 == 0 ? pinkPen : pen;
                    e.Graphics.DrawLine(activePen, x, centerY - height / 2, x, centerY + height / 2);
                }
            }
        }
    }
}
