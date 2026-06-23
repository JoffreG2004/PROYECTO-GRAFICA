using System;
using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class ShapePanelControl : StudioCard
    {
        public event Action<PaintTool> ToolSelected;
        public event Action<int> RegularPolygonSelected;

        public ShapePanelControl()
        {
            BackColor = ThemeColors.Panel;
            Build();
        }

        private void Build()
        {
            Controls.Add(new Label { Text = "Formas", ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            var shapes = new (StudioIcon icon, PaintTool tool)[]
            {
                (StudioIcon.Rectangle,   PaintTool.Rectangle),
                (StudioIcon.RoundedRect, PaintTool.RoundedRectangle),
                (StudioIcon.Ellipse,     PaintTool.Ellipse),
                (StudioIcon.Polygon,     PaintTool.Polygon),
                (StudioIcon.Line,        PaintTool.Line),
                (StudioIcon.Arrow,       PaintTool.Arrow),
                (StudioIcon.Star,        PaintTool.Star),
                (StudioIcon.Curve,       PaintTool.Bezier),
                (StudioIcon.Blob,        PaintTool.Blob),
            };

            for (int i = 0; i < shapes.Length; i++)
            {
                int col = i % 3, row = i / 3;
                var shape = shapes[i];
                StudioToolButton btn = new StudioToolButton
                {
                    Icon = shape.icon, Caption = "", Tag = shape.tool,
                    Location = new Point(8 + col * 50, 28 + row * 46), Size = new Size(44, 42)
                };
                PaintTool tool = shape.tool;
                btn.Click += (s, e) => ToolSelected?.Invoke(tool);
                Controls.Add(btn);
            }

            Label polygonLabel = new Label { Text = "Polígonos regulares", ForeColor = ThemeColors.TextSecondary, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(10, 210) };
            polygonLabel.Location = new Point(10, 170);
            Controls.Add(polygonLabel);
            for (int sides = 3; sides <= 10; sides++)
            {
                int value = sides, index = sides - 3;
                Button button = new Button { Text = value.ToString(), FlatStyle = FlatStyle.Flat, ForeColor = ThemeColors.TextPrimary, BackColor = ThemeColors.Background, Font = new Font("Segoe UI Semibold", 8F), Size = new Size(34, 24), Location = new Point(8 + (index % 4) * 38, 188 + (index / 4) * 27), Cursor = Cursors.Hand };
                button.FlatAppearance.BorderColor = ThemeColors.Border;
                button.FlatAppearance.MouseOverBackColor = ThemeColors.Hover;
                button.Click += (s, e) => RegularPolygonSelected?.Invoke(value);
                Controls.Add(button);
            }
        }
    }
}
