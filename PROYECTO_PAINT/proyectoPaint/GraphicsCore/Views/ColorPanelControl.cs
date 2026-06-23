using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    /// <summary>
    /// Panel "Colores": cuadrícula de swatches + selección de Trazo y Relleno.
    /// Único lugar de la app donde se elige color (no se repite en Propiedades).
    /// </summary>
    public class ColorPanelControl : StudioCard
    {
        private Panel _strokeChip;
        private Panel _fillChip;
        private StudioGlyphButton _eyedropper;
        private Button _addBtn;
        private readonly Panel[] _swatches = new Panel[ThemeColors.Swatches.Length];

        public event Action StrokeColorPickRequested;
        public event Action FillColorPickRequested;
        /// <summary>Color elegido desde un swatch (se aplica a trazo y relleno).</summary>
        public event Action<Color> SwatchPicked;
        public event Action<string> StatusChanged;

        public ColorPanelControl(Color initialStroke, Color initialFill)
        {
            BackColor = ThemeColors.Panel;
            Build(initialStroke, initialFill);
            Resize += (s, e) => LayoutContent();
        }

        public void SetStrokeColor(Color color) { if (_strokeChip != null) _strokeChip.BackColor = color; }
        public void SetFillColor(Color color)   { if (_fillChip   != null) _fillChip.BackColor   = color; }

        private void Build(Color stroke, Color fill)
        {
            Controls.Add(new Label { Text = "Colores", ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 12) });

            _strokeChip = AddColorTarget("Trazo", stroke, () => StrokeColorPickRequested?.Invoke());
            _fillChip   = AddColorTarget("Relleno", fill, () => FillColorPickRequested?.Invoke());

            for (int i = 0; i < _swatches.Length; i++)
            {
                Color swatchColor = ThemeColors.Swatches[i];
                Panel swatch = new Panel { Size = new Size(22, 22), Cursor = Cursors.Hand };
                new ToolTip().SetToolTip(swatch, ColorTranslator.ToHtml(swatchColor));
                swatch.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = StudioCard.RoundedRect(new Rectangle(0, 0, swatch.Width - 1, swatch.Height - 1), 7))
                    using (var brush = new SolidBrush(swatchColor))
                    using (var pen = new Pen(ThemeColors.Border))
                    {
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                };
                swatch.Click += (s, e) => { SwatchPicked?.Invoke(swatchColor); StatusChanged?.Invoke("Color: " + ColorTranslator.ToHtml(swatchColor)); };
                _swatches[i] = swatch;
                Controls.Add(swatch);
            }

            // Acciones: cuentagotas (abrir diálogo) y "+" color personalizado.
            _eyedropper = new StudioGlyphButton { Icon = StudioIcon.Eyedropper, Size = new Size(26, 26) };
            _eyedropper.Click += (s, e) => StrokeColorPickRequested?.Invoke();
            new ToolTip().SetToolTip(_eyedropper, "Seleccionar color del trazo");
            Controls.Add(_eyedropper);

            _addBtn = new Button { Text = "+", ForeColor = ThemeColors.Accent, Font = new Font("Segoe UI", 13F), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Panel, Size = new Size(26, 26), Cursor = Cursors.Hand };
            _addBtn.FlatAppearance.BorderColor = ThemeColors.Border;
            _addBtn.Click += (s, e) => FillColorPickRequested?.Invoke();
            new ToolTip().SetToolTip(_addBtn, "Color personalizado de relleno");
            Controls.Add(_addBtn);

            LayoutContent();
        }

        private Panel AddColorTarget(string label, Color color, Action click)
        {
            Label caption = new Label { Text = label, ForeColor = ThemeColors.TextSecondary, Font = new Font("Segoe UI", 8F), AutoSize = true, Tag = "cap-" + label };
            Panel chip = new Panel { BackColor = color, Cursor = Cursors.Hand, Tag = "chip-" + label };
            chip.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeColors.Border)) e.Graphics.DrawRectangle(pen, 0, 0, chip.Width - 1, chip.Height - 1);
            };
            chip.Click += (s, e) => click();
            caption.Click += (s, e) => click();
            Controls.Add(caption); Controls.Add(chip);
            return chip;
        }

        private void LayoutContent()
        {
            int pad = 14;
            // Chips Trazo / Relleno
            int chipY = 40;
            Label strokeCap = FindTagged("cap-Trazo") as Label;
            Label fillCap   = FindTagged("cap-Relleno") as Label;
            if (strokeCap != null) strokeCap.Location = new Point(pad, chipY + 5);
            if (_strokeChip != null) _strokeChip.SetBounds(pad + 46, chipY, 28, 22);
            int half = Width / 2;
            if (fillCap != null) fillCap.Location = new Point(half + 2, chipY + 5);
            if (_fillChip != null) _fillChip.SetBounds(half + 50, chipY, 28, 22);

            // Cuadrícula de swatches (6 por fila)
            int cols = 6;
            int gridTop = chipY + 36;
            int available = Math.Max(60, Width - pad * 2);
            int cell = Math.Max(20, Math.Min(26, available / cols));
            int gap = (available - cell * cols) / Math.Max(1, cols - 1);
            for (int i = 0; i < _swatches.Length; i++)
            {
                if (_swatches[i] == null) continue;
                int c = i % cols, r = i / cols;
                _swatches[i].SetBounds(pad + c * (cell + gap), gridTop + r * (cell + 6), cell - 2, cell - 2);
            }

            if (_eyedropper != null) _eyedropper.Location = new Point(Width - 36, 8);
            if (_addBtn != null) _addBtn.Location = new Point(Width - 66, 8);
        }

        private Control FindTagged(string tag)
        {
            foreach (Control c in Controls)
                if (c.Tag is string && (string)c.Tag == tag) return c;
            return null;
        }
    }
}
