using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    /// <summary>
    /// Panel "Propiedades": SOLO ajustes propios de la forma seleccionada que no
    /// se ofrecen en otro panel. El color vive en "Colores", el grosor en
    /// "Tamaño del pincel" y la opacidad en "Opacidad" (sin botones repetidos).
    /// </summary>
    public class PropertiesPanelControl : StudioCard
    {
        private readonly List<Button> _strokeStyleButtons = new List<Button>();
        private TrackBar _cornerBar;
        private Label _cornerVal;

        public event Action<StrokeRenderStyle> StrokeStyleChanged;
        public event Action<int> CornerRadiusChanged;
        public event Action RotateRequested;
        public event Action ScaleUpRequested;
        public event Action ScaleDownRequested;

        public PropertiesPanelControl(Color initialStroke, Color initialFill, StrokeRenderStyle initialStyle, int initialCorner)
        {
            BackColor = ThemeColors.Panel;
            Build(initialStyle, initialCorner);
        }

        public void SetStrokeStyle(StrokeRenderStyle style)
        {
            for (int i = 0; i < _strokeStyleButtons.Count; i++)
                _strokeStyleButtons[i].BackColor = i == (int)style ? ThemeColors.Selected : ThemeColors.Background;
        }

        public void SyncFromShape(DrawableShape shape)
        {
            if (shape == null) return;
            SetStrokeStyle(shape.StrokeStyle);
            RoundedRectangleShape rr = shape as RoundedRectangleShape;
            if (_cornerBar != null && rr != null)
            {
                _cornerBar.Value = Math.Max(_cornerBar.Minimum, Math.Min(_cornerBar.Maximum, rr.CornerRadius));
                if (_cornerVal != null) _cornerVal.Text = _cornerBar.Value + " px";
            }
        }

        private void Build(StrokeRenderStyle style, int cornerRadius)
        {
            Font lbl = new Font("Segoe UI", 8.5F);
            Font val = new Font("Segoe UI", 8.5F);
            int y = 36;

            Controls.Add(new Label { Text = "Propiedades", ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            // Estilo de borde
            Controls.Add(new Label { Text = "Estilo de borde", ForeColor = ThemeColors.TextSecondary, Font = lbl, AutoSize = true, Location = new Point(10, y) });
            string[] styleLabels = { "—", "- -", "···" };
            _strokeStyleButtons.Clear();
            for (int i = 0; i < 3; i++)
            {
                Button sb = new Button { Text = styleLabels[i], ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat, BackColor = i == (int)style ? ThemeColors.Selected : ThemeColors.Background, Size = new Size(44, 26), Location = new Point(10 + i * 48, y + 18) };
                sb.FlatAppearance.BorderColor = ThemeColors.Border;
                sb.FlatAppearance.BorderSize  = 1;
                sb.FlatAppearance.MouseOverBackColor = ThemeColors.Hover;
                int styleIndex = i;
                sb.Click += (se, ev) => StrokeStyleChanged?.Invoke((StrokeRenderStyle)styleIndex);
                _strokeStyleButtons.Add(sb);
                Controls.Add(sb);
            }
            y += 50;

            // Radio de esquina
            Controls.Add(new Label { Text = "Radio de esquina", ForeColor = ThemeColors.TextSecondary, Font = lbl, AutoSize = true, Location = new Point(10, y) });
            _cornerBar = new TrackBar { Minimum = 0, Maximum = 100, Value = cornerRadius, TickStyle = TickStyle.None, BackColor = ThemeColors.Panel, Location = new Point(6, y + 16), Size = new Size(118, 38) };
            _cornerVal = new Label { Text = cornerRadius + " px", ForeColor = ThemeColors.TextPrimary, Font = val, AutoSize = true, Location = new Point(126, y + 24) };
            _cornerBar.ValueChanged += (s, e) => { _cornerVal.Text = _cornerBar.Value + " px"; CornerRadiusChanged?.Invoke(_cornerBar.Value); };
            Controls.Add(_cornerBar); Controls.Add(_cornerVal);
            y += 62;

            // Transformaciones
            Controls.Add(new Label { Text = "Transformar", ForeColor = ThemeColors.TextSecondary, Font = lbl, AutoSize = true, Location = new Point(10, y) });
            y += 20;
            Button rotate    = PropertyButton("Rotar",    10, y, 100);
            Button scaleUp   = PropertyButton("Escala +", 116, y, 64);
            y += 30;
            Button scaleDown = PropertyButton("Escala −", 10, y, 100);
            rotate.Click    += (s, e) => RotateRequested?.Invoke();
            scaleUp.Click   += (s, e) => ScaleUpRequested?.Invoke();
            scaleDown.Click += (s, e) => ScaleDownRequested?.Invoke();
            Controls.Add(rotate); Controls.Add(scaleUp); Controls.Add(scaleDown);
        }

        private static Button PropertyButton(string text, int x, int y, int width = 52)
        {
            Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 26), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Background, ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI", 8.5F) };
            button.FlatAppearance.BorderColor = ThemeColors.Border;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.Hover;
            return button;
        }
    }
}
