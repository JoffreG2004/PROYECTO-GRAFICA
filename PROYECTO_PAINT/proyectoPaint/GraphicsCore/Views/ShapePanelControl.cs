using System;
using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class ShapePanelControl : StudioCard
    {
        public event Action<PaintTool> ToolSelected;

        public ShapePanelControl()
        {
            BackColor = Color.FromArgb(17, 27, 43);
            Build();
        }

        private void Build()
        {
            Controls.Add(new Label { Text = "Forma", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

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
                    Location = new Point(8 + col * 58, 34 + row * 58), Size = new Size(50, 50)
                };
                PaintTool tool = shape.tool;
                btn.Click += (s, e) => ToolSelected?.Invoke(tool);
                Controls.Add(btn);
            }
        }
    }
}
