using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace proyectoPaint.GraphicsCore
{
    public class LayersPanelControl : StudioCard
    {
        private Panel _list;

        public event Action<DrawableShape> ShapeSelected;
        public event Action<DrawableShape> ShapeDeleted;
        public event Action<DrawableShape> VisibilityToggled;
        public event Action<Control> MoreMenuRequested;
        public event Action AddLayerRequested;

        public LayersPanelControl()
        {
            BackColor = Color.FromArgb(17, 27, 43);
            Build();
        }

        public void Refresh(IList<DrawableShape> shapes, DrawableShape selected)
        {
            if (_list == null || _list.IsDisposed) return;
            _list.SuspendLayout();
            _list.Controls.Clear();

            if (shapes.Count == 0)
            {
                _list.Controls.Add(new Label
                {
                    Text = "Sin capas todavía.\nDibuja algo para empezar.",
                    ForeColor = Color.FromArgb(110, 134, 170), Font = new Font("Segoe UI", 8F),
                    AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(4, 28), Size = new Size(_list.ClientSize.Width - 8, 48)
                });
                _list.ResumeLayout();
                return;
            }

            int rowWidth = _list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2;
            for (int i = 0; i < shapes.Count; i++)
            {
                DrawableShape shape = shapes[shapes.Count - 1 - i];
                bool isSelected = shape == selected;

                Panel row = new Panel
                {
                    Location = new Point(0, i * 28), Size = new Size(rowWidth, 25),
                    BackColor = isSelected ? Color.FromArgb(48, 40, 92) : Color.FromArgb(20, 32, 50),
                    Cursor = Cursors.Hand
                };
                if (isSelected)
                    row.Paint += (s, e) =>
                    {
                        using (var b = new SolidBrush(StudioToolButton.Accent))
                            e.Graphics.FillRectangle(b, 0, 0, 3, row.Height);
                    };

                DrawableShape captured = shape;

                Button eye = new Button
                {
                    Text = shape.Visible ? "◉" : "○",
                    ForeColor = shape.Visible ? Color.FromArgb(190, 208, 236) : Color.FromArgb(110, 130, 160),
                    Font = new Font("Segoe UI", 8F), FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent, Size = new Size(20, 20), Location = new Point(6, 2), Cursor = Cursors.Hand
                };
                eye.FlatAppearance.BorderSize = 0;
                new ToolTip().SetToolTip(eye, "Mostrar / ocultar capa");
                eye.Click += (s, e) =>
                {
                    captured.Visible = !captured.Visible;
                    VisibilityToggled?.Invoke(captured);
                };

                Color swatch = shape.UseFill ? shape.FillColor : shape.StrokeColor;
                Panel dot = new Panel { Size = new Size(12, 12), Location = new Point(28, 6), BackColor = swatch, Cursor = Cursors.Hand };

                Label name = new Label
                {
                    Text = shape.DisplayName, AutoSize = false, Size = new Size(rowWidth - 70, 20), Location = new Point(46, 3),
                    ForeColor = isSelected ? Color.White : (shape.Visible ? Color.FromArgb(205, 220, 242) : Color.FromArgb(130, 148, 175)),
                    Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
                };

                Button del = new Button
                {
                    Text = "×", ForeColor = Color.FromArgb(150, 172, 205),
                    Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent, Size = new Size(20, 20), Location = new Point(rowWidth - 22, 2), Cursor = Cursors.Hand
                };
                del.FlatAppearance.BorderSize = 0;
                del.FlatAppearance.MouseOverBackColor = Color.FromArgb(120, 40, 60);
                del.Click += (s, e) => ShapeDeleted?.Invoke(captured);

                EventHandler choose = (s, e) => ShapeSelected?.Invoke(captured);
                row.Click += choose; name.Click += choose; dot.Click += choose;
                row.Controls.Add(eye); row.Controls.Add(dot); row.Controls.Add(name); row.Controls.Add(del);
                _list.Controls.Add(row);
            }
            _list.ResumeLayout();
        }

        private void Build()
        {
            Controls.Add(new Label { Text = "Capas", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            Button addBtn  = new Button { Text = "+",   ForeColor = Color.FromArgb(170, 192, 224), Font = new Font("Segoe UI", 12F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(20, 20), Location = new Point(134, 7) };
            Button moreBtn = new Button { Text = "···", ForeColor = Color.FromArgb(150, 175, 210), Font = new Font("Segoe UI", 7F),  FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(24, 20), Location = new Point(154, 7) };
            addBtn.FlatAppearance.BorderSize  = 0;
            moreBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click  += (s, e) => AddLayerRequested?.Invoke();
            moreBtn.Click += (s, e) => MoreMenuRequested?.Invoke(moreBtn);
            Controls.Add(addBtn); Controls.Add(moreBtn);

            _list = new Panel { Location = new Point(6, 32), Size = new Size(160, 140), AutoScroll = true, BackColor = BackColor };
            Controls.Add(_list);
            Resize += (s, e) => _list.SetBounds(6, 32, Math.Max(40, Width - 12), Math.Max(40, Height - 40));
        }
    }
}
