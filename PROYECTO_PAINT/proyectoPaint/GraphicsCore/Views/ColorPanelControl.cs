using System;
using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    /// <summary>A compact, responsive color card for the right inspector.</summary>
    public class ColorPanelControl : StudioCard
    {
        private Panel _strokePreview;
        private Panel _fillPreview;
        private ColorWheelControl _wheel;
        private Panel _tabRow;
        private StudioGlyphButton _eyedropper;
        private StudioGlyphButton _menuBtn;
        private readonly Panel[] _swatches = new Panel[7];

        public event Action StrokeColorPickRequested;
        public event Action FillColorPickRequested;
        public event Action<Color> FillColorChanged;
        public event Action<string> StatusChanged;

        public ColorPanelControl(Color initialStroke, Color initialFill)
        {
            BackColor = Color.FromArgb(17, 27, 43);
            Build(initialStroke, initialFill);
            Resize += (s, e) => LayoutContent();
        }

        public void SetStrokeColor(Color color) { if (_strokePreview != null) _strokePreview.BackColor = color; }
        public void SetFillColor(Color color) { if (_fillPreview != null) _fillPreview.BackColor = color; }

        private void Build(Color stroke, Color fill)
        {
            Controls.Add(new Label { Text = "Color", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(12, 11) });
            _strokePreview = AddColorTarget("Stroke", stroke, () => StrokeColorPickRequested?.Invoke());
            _fillPreview = AddColorTarget("Fill", fill, () => FillColorPickRequested?.Invoke());

            _wheel = new ColorWheelControl { Size = new Size(126, 126) };
            _wheel.ColorChanged += delegate { FillColorChanged?.Invoke(_wheel.SelectedColor); };
            Controls.Add(_wheel);

            _tabRow = new Panel { BackColor = Color.FromArgb(12, 20, 34) };
            Button swatches = CreateTab("Swatches", true);
            Button gradients = CreateTab("Gradients", false);
            swatches.Click += (s, e) => { swatches.BackColor = Color.FromArgb(31, 47, 70); gradients.BackColor = Color.Transparent; StatusChanged?.Invoke("Swatches selected"); };
            gradients.Click += (s, e) => { gradients.BackColor = Color.FromArgb(31, 47, 70); swatches.BackColor = Color.Transparent; FillColorChanged?.Invoke(Color.FromArgb(34, 211, 238)); StatusChanged?.Invoke("Gradient palette selected"); };
            _tabRow.Controls.Add(swatches); _tabRow.Controls.Add(gradients);
            Controls.Add(_tabRow);

            Color[] colors = { Color.FromArgb(139, 92, 246), Color.FromArgb(236, 72, 153), Color.FromArgb(251, 146, 60), Color.FromArgb(250, 204, 21), Color.FromArgb(74, 222, 128), Color.FromArgb(34, 211, 238), Color.FromArgb(59, 130, 246) };
            for (int i = 0; i < colors.Length; i++)
            {
                Color swatchColor = colors[i];
                Panel swatch = new Panel { Size = new Size(18, 18), Cursor = Cursors.Hand };
                swatch.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, 17, 17), 8))
                    using (var brush = new SolidBrush(swatchColor)) e.Graphics.FillPath(brush, path);
                };
                swatch.Click += (s, e) => FillColorChanged?.Invoke(swatchColor);
                _swatches[i] = swatch;
                Controls.Add(swatch);
            }

            // Top-right card actions: eyedropper + overflow menu.
            _eyedropper = new StudioGlyphButton { Icon = StudioIcon.Eyedropper, Size = new Size(24, 24) };
            _eyedropper.Click += (s, e) => FillColorPickRequested?.Invoke();
            new ToolTip().SetToolTip(_eyedropper, "Seleccionar color");
            Controls.Add(_eyedropper);
            _menuBtn = new StudioGlyphButton { Icon = StudioIcon.More, Size = new Size(24, 24) };
            Controls.Add(_menuBtn);

            LayoutContent();
        }

        private Panel AddColorTarget(string label, Color color, Action click)
        {
            Button target = new Button { Text = label, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(27, 42, 63), ForeColor = Color.FromArgb(219, 230, 247), Font = new Font("Segoe UI", 7.5F), Cursor = Cursors.Hand };
            target.FlatAppearance.BorderColor = Color.FromArgb(55, 78, 110);
            target.Click += (s, e) => click();
            Panel preview = new Panel { BackColor = color, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            preview.Click += (s, e) => click();
            Controls.Add(target); Controls.Add(preview);
            return preview;
        }

        private static Button CreateTab(string text, bool selected)
        {
            Button button = new Button { Text = text, FlatStyle = FlatStyle.Flat, BackColor = selected ? Color.FromArgb(31, 47, 70) : Color.Transparent, ForeColor = selected ? Color.White : Color.FromArgb(140, 163, 196), Font = new Font("Segoe UI", 7.5F) };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void LayoutContent()
        {
            if (_wheel == null) return;
            int wheelSize = Math.Min(126, Math.Max(92, Height - 110));
            int wheelX = Math.Max(92, Width - wheelSize - 14);
            _wheel.SetBounds(wheelX, 28, wheelSize, wheelSize);

            Control[] targets = { Controls[1], _strokePreview, Controls[3], _fillPreview };
            targets[0].SetBounds(12, 46, 54, 23); targets[1].SetBounds(70, 46, 22, 23);
            targets[2].SetBounds(12, 75, 54, 23); targets[3].SetBounds(70, 75, 22, 23);

            int bottom = Math.Max(130, Height - 63);
            _tabRow.SetBounds(10, bottom, Math.Max(110, Width - 20), 23);
            if (_tabRow.Controls.Count == 2)
            {
                int half = _tabRow.Width / 2;
                _tabRow.Controls[0].SetBounds(0, 0, half, 23);
                _tabRow.Controls[1].SetBounds(half, 0, _tabRow.Width - half, 23);
            }
            int start = 12;
            int available = Math.Max(20, Width - 24);
            int spacing = _swatches.Length > 1 ? Math.Max(20, Math.Min(29, (available - 18) / (_swatches.Length - 1))) : 20;
            for (int i = 0; i < _swatches.Length; i++)
                if (_swatches[i] != null) _swatches[i].Location = new Point(start + i * spacing, bottom + 30);

            if (_eyedropper != null) _eyedropper.Location = new Point(Width - 34, 8);
            if (_menuBtn != null) _menuBtn.Location = new Point(Width - 60, 8);
        }
    }
}
