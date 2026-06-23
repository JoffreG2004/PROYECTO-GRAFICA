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
            BackColor = ThemeColors.Panel;
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
                    ForeColor = ThemeColors.TextSecondary, Font = new Font("Segoe UI", 8F),
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
                    BackColor = isSelected ? ThemeColors.Selected : ThemeColors.Background,
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
                    ForeColor = shape.Visible ? ThemeColors.Icon : ThemeColors.TextSecondary,
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
                    ForeColor = isSelected ? ThemeColors.TextPrimary : (shape.Visible ? ThemeColors.TextPrimary : ThemeColors.TextSecondary),
                    Font = new Font("Segoe UI", 8F), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
                };

                Button del = new Button
                {
                    Text = "×", ForeColor = ThemeColors.TextSecondary,
                    Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent, Size = new Size(20, 20), Location = new Point(rowWidth - 22, 2), Cursor = Cursors.Hand
                };
                del.FlatAppearance.BorderSize = 0;
                del.FlatAppearance.MouseOverBackColor = ThemeColors.Hover;
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
            Controls.Add(new Label { Text = "Capas", ForeColor = ThemeColors.TextPrimary, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) });

            Button addBtn  = new Button { Text = "+",   ForeColor = ThemeColors.Accent, Font = new Font("Segoe UI", 12F), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(20, 20), Location = new Point(134, 7), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            Button moreBtn = new Button { Text = "···", ForeColor = ThemeColors.Icon, Font = new Font("Segoe UI", 7F),  FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Size = new Size(24, 20), Location = new Point(154, 7), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            addBtn.FlatAppearance.BorderSize  = 0;
            moreBtn.FlatAppearance.BorderSize = 0;
            addBtn.FlatAppearance.MouseOverBackColor = ThemeColors.Hover;
            moreBtn.FlatAppearance.MouseOverBackColor = ThemeColors.Hover;
            addBtn.Click  += (s, e) => AddLayerRequested?.Invoke();
            moreBtn.Click += (s, e) => MoreMenuRequested?.Invoke(moreBtn);
            Controls.Add(addBtn); Controls.Add(moreBtn);

            _list = new Panel { Location = new Point(6, 32), Size = new Size(160, 140), AutoScroll = true, BackColor = BackColor };
            Controls.Add(_list);
            Resize += (s, e) =>
            {
                _list.SetBounds(6, 32, Math.Max(40, Width - 12), Math.Max(40, Height - 40));
                moreBtn.Location = new Point(Width - 30, 7);
                addBtn.Location = new Point(Width - 54, 7);
            };
        }
    }
}
