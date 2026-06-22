using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace proyectoPaint
{
    public sealed class AlgorithmLabForm : Form
    {
        public AlgorithmLabForm()
        {
            Text = "proyectoPaint · Laboratorio de algoritmos";
            BackColor = Color.FromArgb(18, 29, 52); ClientSize = new Size(1240, 780); StartPosition = FormStartPosition.CenterParent;
            Panel nav = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(13, 23, 42) };
            nav.Controls.Add(new Label { Text = "LABORATORIO", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 11F), AutoSize = true, Location = new Point(20, 11) });
            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 9F) };
            tabs.TabPages.Add(BuildInfo("Líneas · DDA y Bresenham", "Selecciona dos puntos en el Paint y compara los algoritmos discretos.\n\nDDA: incrementa X e Y con Δx/k y Δy/k.\nBresenham: decide el siguiente píxel únicamente con enteros."));
            tabs.TabPages.Add(BuildInfo("Cónicas · Círculo por punto medio", "El círculo se calcula desde el primer octante.\nCada punto se refleja con las 8 simetrías respecto al centro.\n\np₀ = 1 − r; según p se avanza E o SE."));
            TabPage fill = new TabPage("Relleno scanline") { BackColor = Color.FromArgb(18, 29, 52) };
            fill.Controls.Add(new ScanlineDemo { Dock = DockStyle.Fill }); tabs.TabPages.Add(fill);
            tabs.TabPages.Add(BuildInfo("Polígonos · 3 a 10 lados", "En Dibujar, elige un polígono regular y arrastra desde el centro.\nDespués usa Seleccionar y arrastra los nodos blancos para editar cada vértice."));
            Controls.Add(tabs); Controls.Add(nav);
        }

        private static TabPage BuildInfo(string title, string text)
        {
            TabPage page = new TabPage(title) { BackColor = Color.FromArgb(18, 29, 52) };
            page.Controls.Add(new Label { Text = title, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 20F), AutoSize = true, Location = new Point(30, 30) });
            page.Controls.Add(new Label { Text = text, ForeColor = Color.FromArgb(205, 217, 240), Font = new Font("Segoe UI", 12F), AutoSize = true, Location = new Point(34, 85) });
            return page;
        }
    }

    internal sealed class ScanlineDemo : Panel
    {
        private readonly Timer timer = new Timer { Interval = 45 };
        private int step;
        private readonly Dictionary<Point, int> order = BuildOrder();
        public ScanlineDemo()
        {
            DoubleBuffered = true; BackColor = Color.FromArgb(18, 29, 52);
            timer.Tick += (s, e) => { step = step >= order.Count ? 0 : step + 1; Invalidate(); }; timer.Start();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); Graphics g = e.Graphics;
            using (Font title = new Font("Segoe UI Semibold", 20F)) using (Font info = new Font("Segoe UI", 10F))
            using (Brush white = new SolidBrush(Color.White))
            {
                g.DrawString("Relleno por scanline", title, white, 24, 20);
                g.DrawString("La semilla busca los extremos izquierdo y derecho; después propaga la franja a las filas vecinas.", info, white, 26, 58);
            }
            Rectangle panel = new Rectangle(24, 96, 260, Height - 125);
            using (Brush card = new SolidBrush(Color.FromArgb(44, 57, 95))) g.FillRectangle(card, panel);
            TextRenderer.DrawText(g, "Semilla: centro", Font, new Point(44, 125), Color.White);
            TextRenderer.DrawText(g, "Delay: 45 ms", Font, new Point(44, 155), Color.White);
            TextRenderer.DrawText(g, "Pasos: " + step, Font, new Point(44, 185), Color.White);
            TextRenderer.DrawText(g, "1. Tomar semilla\n2. Pintar franja\n3. Revisar arriba/abajo", Font, new Rectangle(44, 235, 210, 100), Color.FromArgb(210, 220, 245));
            int cell = Math.Max(18, Math.Min(30, (Height - 170) / 15)); int cols = 23, rows = 15;
            int sx = 340, sy = 115;
            for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++)
            {
                Point point = new Point(x, y); int visit;
                bool inside = order.TryGetValue(point, out visit);
                Color c = !inside ? Color.FromArgb(37, 49, 82) : visit <= step ? Color.FromArgb(251, 183, 124) : Color.FromArgb(25, 35, 62);
                using (Brush b = new SolidBrush(c)) g.FillRectangle(b, sx + x * cell, sy + y * cell, cell - 1, cell - 1);
                if (inside && visit <= step) TextRenderer.DrawText(g, visit.ToString(), Font, new Point(sx + x * cell + 3, sy + y * cell + 3), Color.White);
                else if (inside && x == 11 && y == 7) TextRenderer.DrawText(g, "S", Font, new Point(sx + x * cell + 4, sy + y * cell + 3), Color.White);
            }
        }

        private static Dictionary<Point, int> BuildOrder()
        {
            Dictionary<Point, int> result = new Dictionary<Point, int>();
            Visit(new Point(11, 7), result);
            return result;
        }

        // Mismo orden del algoritmo por semilla: arriba, derecha, abajo, izquierda.
        // Cada rama se completa antes de volver a probar la siguiente dirección.
        private static void Visit(Point p, Dictionary<Point, int> result)
        {
            if (p.X < 6 || p.X > 17 || p.Y < 5 || p.Y > 9 || result.ContainsKey(p)) return;
            result[p] = result.Count + 1;
            Visit(new Point(p.X, p.Y - 1), result);
            Visit(new Point(p.X + 1, p.Y), result);
            Visit(new Point(p.X, p.Y + 1), result);
            Visit(new Point(p.X - 1, p.Y), result);
        }
    }
}
