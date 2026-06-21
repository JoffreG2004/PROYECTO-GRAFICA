using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class ColorPanelControl : StudioCard
    {
        private Panel _strokePreview;
        private Panel _fillPreview;

        public event Action StrokeColorPickRequested;
        public event Action FillColorPickRequested;
        public event Action<Color> FillColorChanged;
        public event Action<string> StatusChanged;

        public ColorPanelControl(Color initialStroke, Color initialFill)
        {
            BackColor = Color.FromArgb(17, 27, 43);
            Build(initialStroke, initialFill);
        }

        public void SetStrokeColor(Color color) { if (_strokePreview != null) _strokePreview.BackColor = color; }
        public void SetFillColor(Color color)   { if (_fillPreview   != null) _fillPreview.BackColor   = color; }

        private void Build(Color stroke, Color fill)
        {
            Controls.Add(new Label { Text = "Color", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            _strokePreview = AddColorTarget("Borde",   10, 34, stroke, () => StrokeColorPickRequested?.Invoke());
            _fillPreview   = AddColorTarget("Relleno", 98, 34, fill,   () => FillColorPickRequested?.Invoke());

            ColorWheelControl wheel = new ColorWheelControl { Location = new Point(42, 64), Size = new Size(98, 98) };
            wheel.ColorChanged += delegate { FillColorChanged?.Invoke(wheel.SelectedColor); };
            Controls.Add(wheel);

            Panel tabRow = new Panel { Location = new Point(8, 168), Size = new Size(166, 22), BackColor = Color.FromArgb(12, 20, 34) };
            Button swTab = new Button { Text = "Muestras",   FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 46, 68), ForeColor = Color.White,                   Font = new Font("Segoe UI", 7.5F), Size = new Size(80, 22), Location = new Point(0, 0) };
            Button grTab = new Button { Text = "Degradados", FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent,          ForeColor = Color.FromArgb(130, 155, 188), Font = new Font("Segoe UI", 7.5F), Size = new Size(82, 22), Location = new Point(82, 0) };
            swTab.FlatAppearance.BorderSize = 0;
            grTab.FlatAppearance.BorderSize = 0;
            swTab.Click += (s, e) => { swTab.BackColor = Color.FromArgb(30, 46, 68); grTab.BackColor = Color.Transparent; StatusChanged?.Invoke("Muestras seleccionadas"); };
            grTab.Click += (s, e) => { grTab.BackColor = Color.FromArgb(30, 46, 68); swTab.BackColor = Color.Transparent; FillColorChanged?.Invoke(Color.FromArgb(34, 211, 238)); StatusChanged?.Invoke("Paleta de degradados seleccionada"); };
            tabRow.Controls.Add(swTab); tabRow.Controls.Add(grTab);
            Controls.Add(tabRow);

            Color[] sw = { Color.FromArgb(139, 92, 246), Color.FromArgb(59, 130, 246), Color.FromArgb(34, 211, 238), Color.FromArgb(74, 222, 128), Color.FromArgb(250, 204, 21), Color.FromArgb(251, 113, 133), Color.White };
            for (int i = 0; i < sw.Length; i++)
            {
                Color c = sw[i];
                Panel p = new Panel { Location = new Point(8 + i * 23, 196), Size = new Size(18, 18), Cursor = Cursors.Hand };
                p.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, 17, 17), 9))
                    using (var b = new SolidBrush(c)) e.Graphics.FillPath(b, path);
                };
                p.Click += (s, e) => FillColorChanged?.Invoke(c);
                Controls.Add(p);
            }
        }

        private Panel AddColorTarget(string label, int x, int y, Color color, Action onClick)
        {
            Button btn = new Button { Text = label, Location = new Point(x, y), Size = new Size(54, 24), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(26, 40, 60), ForeColor = Color.FromArgb(215, 228, 246), Font = new Font("Segoe UI", 7.5F) };
            btn.FlatAppearance.BorderColor = Color.FromArgb(52, 74, 106);
            btn.Click += (s, e) => onClick?.Invoke();
            Panel preview = new Panel { Location = new Point(x + 58, y), Size = new Size(22, 24), BackColor = color, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            preview.Click += (s, e) => onClick?.Invoke();
            Controls.Add(btn); Controls.Add(preview);
            return preview;
        }
    }
}
