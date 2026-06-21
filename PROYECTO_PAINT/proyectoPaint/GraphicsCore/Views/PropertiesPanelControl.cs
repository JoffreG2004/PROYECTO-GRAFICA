using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class PropertiesPanelControl : StudioCard
    {
        private Panel _fillPreview;
        private Panel _strokePreview;
        private readonly List<Button> _strokeStyleButtons = new List<Button>();
        private TrackBar _cornerBar;
        private Label _cornerVal;
        private TrackBar _opacityBar;
        private Label _opacityVal;

        public event Action FillColorPickRequested;
        public event Action StrokeColorPickRequested;
        public event Action<StrokeRenderStyle> StrokeStyleChanged;
        public event Action<int> ThicknessChanged;
        public event Action<int> CornerRadiusChanged;
        public event Action<int> OpacityChanged;
        public event Action RotateRequested;
        public event Action ScaleUpRequested;
        public event Action ScaleDownRequested;
        public event Action ClearRequested;

        public PropertiesPanelControl(Color initialStroke, Color initialFill, StrokeRenderStyle initialStyle, int initialCorner)
        {
            BackColor = Color.FromArgb(17, 27, 43);
            Build(initialStroke, initialFill, initialStyle, initialCorner);
        }

        public void SetFillColor(Color color)   { if (_fillPreview   != null) _fillPreview.BackColor   = color; }
        public void SetStrokeColor(Color color) { if (_strokePreview != null) _strokePreview.BackColor = color; }

        public void SetStrokeStyle(StrokeRenderStyle style)
        {
            for (int i = 0; i < _strokeStyleButtons.Count; i++)
                _strokeStyleButtons[i].BackColor = i == (int)style ? Color.FromArgb(44, 64, 98) : Color.FromArgb(22, 36, 54);
        }

        public void SyncFromShape(DrawableShape shape)
        {
            if (shape == null) return;
            SetFillColor(shape.FillColor);
            SetStrokeColor(shape.StrokeColor);
            SetStrokeStyle(shape.StrokeStyle);
            if (_opacityBar != null)
            {
                int pct = (int)Math.Round(shape.Opacity * 100F);
                _opacityBar.Value = Math.Max(_opacityBar.Minimum, Math.Min(_opacityBar.Maximum, pct));
                if (_opacityVal != null) _opacityVal.Text = _opacityBar.Value + "%";
            }
            RoundedRectangleShape rr = shape as RoundedRectangleShape;
            if (_cornerBar != null && rr != null)
            {
                _cornerBar.Value = Math.Max(_cornerBar.Minimum, Math.Min(_cornerBar.Maximum, rr.CornerRadius));
                if (_cornerVal != null) _cornerVal.Text = _cornerBar.Value + " px";
            }
        }

        private void Build(Color stroke, Color fill, StrokeRenderStyle style, int cornerRadius)
        {
            Font lbl = new Font("Segoe UI", 8.5F);
            Font val = new Font("Segoe UI", 8.5F);
            int y = 32;

            Controls.Add(new Label { Text = "Propiedades", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            // Relleno
            Controls.Add(new Label { Text = "Relleno", ForeColor = Color.FromArgb(165, 188, 218), Font = lbl, AutoSize = true, Location = new Point(10, y) });
            Button fillButton = new Button { Text = "Relleno", ForeColor = Color.FromArgb(205, 220, 242), Font = lbl, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(24, 38, 58), Size = new Size(76, 26), Location = new Point(10, y + 18), Cursor = Cursors.Hand };
            fillButton.FlatAppearance.BorderColor = Color.FromArgb(45, 70, 105);
            fillButton.FlatAppearance.BorderSize  = 1;
            fillButton.Click += (s, e) => FillColorPickRequested?.Invoke();
            _fillPreview = new Panel { Location = new Point(92, y + 18), Size = new Size(26, 26), BackColor = fill, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            _fillPreview.Click += (s, e) => FillColorPickRequested?.Invoke();
            Controls.Add(fillButton); Controls.Add(_fillPreview);
            y += 52;

            // Borde
            Controls.Add(new Label { Text = "Borde", ForeColor = Color.FromArgb(165, 188, 218), Font = lbl, AutoSize = true, Location = new Point(10, y) });
            _strokePreview = new Panel { Location = new Point(10, y + 18), Size = new Size(22, 22), BackColor = stroke, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            _strokePreview.Click += (s, e) => StrokeColorPickRequested?.Invoke();
            Controls.Add(_strokePreview);
            Controls.Add(new Label { Text = "3 px", ForeColor = Color.White, Font = val, AutoSize = true, Location = new Point(38, y + 20) });
            Button sDrop = new Button { Text = "▾", ForeColor = Color.FromArgb(150, 175, 210), Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(24, 38, 58), Size = new Size(28, 22), Location = new Point(120, y + 17) };
            sDrop.FlatAppearance.BorderSize = 0;
            sDrop.Click += (s, e) =>
            {
                ContextMenuStrip menu = new ContextMenuStrip();
                foreach (int width in new[] { 1, 2, 4, 8, 12 })
                {
                    int w = width;
                    menu.Items.Add(w + " px", null, (a, b) => ThicknessChanged?.Invoke(w));
                }
                menu.Show(sDrop, new Point(0, sDrop.Height));
            };
            Controls.Add(sDrop);
            y += 52;

            // Estilo de borde
            Controls.Add(new Label { Text = "Estilo de borde", ForeColor = Color.FromArgb(165, 188, 218), Font = lbl, AutoSize = true, Location = new Point(10, y) });
            string[] styleLabels = { "—", "- -", "···" };
            _strokeStyleButtons.Clear();
            for (int i = 0; i < 3; i++)
            {
                Button sb = new Button { Text = styleLabels[i], ForeColor = Color.White, Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat, BackColor = i == (int)style ? Color.FromArgb(44, 64, 98) : Color.FromArgb(22, 36, 54), Size = new Size(44, 24), Location = new Point(10 + i * 48, y + 18) };
                sb.FlatAppearance.BorderColor = Color.FromArgb(55, 80, 118);
                sb.FlatAppearance.BorderSize  = 1;
                int styleIndex = i;
                sb.Click += (se, ev) => StrokeStyleChanged?.Invoke((StrokeRenderStyle)styleIndex);
                _strokeStyleButtons.Add(sb);
                Controls.Add(sb);
            }
            y += 52;

            // Radio de esquina
            Controls.Add(new Label { Text = "Radio de esquina", ForeColor = Color.FromArgb(165, 188, 218), Font = lbl, AutoSize = true, Location = new Point(10, y) });
            _cornerBar = new TrackBar { Minimum = 0, Maximum = 100, Value = cornerRadius, TickStyle = TickStyle.None, Location = new Point(6, y + 18), Size = new Size(118, 45) };
            _cornerVal = new Label { Text = cornerRadius + " px", ForeColor = Color.White, Font = val, AutoSize = true, Location = new Point(126, y + 28) };
            _cornerBar.ValueChanged += (s, e) => { _cornerVal.Text = _cornerBar.Value + " px"; CornerRadiusChanged?.Invoke(_cornerBar.Value); };
            Controls.Add(_cornerBar); Controls.Add(_cornerVal);
            y += 70;

            // Opacidad
            Controls.Add(new Label { Text = "Opacidad", ForeColor = Color.FromArgb(165, 188, 218), Font = lbl, AutoSize = false, Size = new Size(110, 18), Location = new Point(10, y) });
            _opacityBar = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, Location = new Point(6, y + 18), Size = new Size(118, 45) };
            _opacityVal = new Label { Text = "100%", ForeColor = Color.White, Font = val, AutoSize = true, Location = new Point(126, y + 28) };
            _opacityBar.ValueChanged += (s, e) => { _opacityVal.Text = _opacityBar.Value + "%"; OpacityChanged?.Invoke(_opacityBar.Value); };
            Controls.Add(_opacityBar); Controls.Add(_opacityVal);
            y += 72;

            // Transform buttons
            Button rotate    = PropertyButton("Rotar",    10, y, 72);
            Button scaleUp   = PropertyButton("Escala +", 88, y, 72);
            y += 34;
            Button scaleDown = PropertyButton("Escala -", 10, y, 72);
            Button clear     = PropertyButton("Limpiar",  88, y, 72);
            rotate.Click    += (s, e) => RotateRequested?.Invoke();
            scaleUp.Click   += (s, e) => ScaleUpRequested?.Invoke();
            scaleDown.Click += (s, e) => ScaleDownRequested?.Invoke();
            clear.Click     += (s, e) => ClearRequested?.Invoke();
            Controls.Add(rotate); Controls.Add(scaleUp); Controls.Add(scaleDown); Controls.Add(clear);
        }

        private static Button PropertyButton(string text, int x, int y, int width = 52)
        {
            Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 26), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 55, 86), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F) };
            button.FlatAppearance.BorderColor = Color.FromArgb(72, 103, 151);
            return button;
        }
    }
}
