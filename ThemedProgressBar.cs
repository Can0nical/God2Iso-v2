using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace God2Iso
{
    /// <summary>
    /// Owner-drawn progress bar — the default WinForms ProgressBar is a system
    /// control that ignores BackColor / ForeColor on Win10/11 (it reads the
    /// theme engine), so we draw our own slim bar in the current accent color.
    /// </summary>
    public sealed class ThemedProgressBar : Control
    {
        private int _value;
        private int _maximum = 100;

        public ThemedProgressBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            Height = 10;
        }

        public int Value
        {
            get => _value;
            set
            {
                var clamped = Math.Max(0, Math.Min(value, _maximum));
                if (_value != clamped) { _value = clamped; Invalidate(); }
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(1, value);
                if (_value > _maximum) _value = _maximum;
                Invalidate();
            }
        }

        public int Step { get; set; } = 1;

        public void PerformStep()
        {
            Value = Math.Min(_value + Step, _maximum);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Track
            using (var trackBrush = new SolidBrush(ThemePalette.Border))
            using (var trackPath = RoundedRect(rect, Height / 2))
            {
                g.FillPath(trackBrush, trackPath);
            }

            // Fill
            if (_value > 0 && _maximum > 0)
            {
                double pct = (double)_value / _maximum;
                int fillWidth = Math.Max(Height, (int)(rect.Width * pct));
                var fillRect = new Rectangle(0, 0, fillWidth, rect.Height);
                using var fillBrush = new LinearGradientBrush(
                    fillRect,
                    ThemePalette.Accent,
                    Color.FromArgb(255, Math.Min(255, ThemePalette.Accent.R + 40),
                                        Math.Min(255, ThemePalette.Accent.G + 40),
                                        Math.Min(255, ThemePalette.Accent.B + 40)),
                    LinearGradientMode.Horizontal);
                using var fillPath = RoundedRect(fillRect, Height / 2);
                g.FillPath(fillBrush, fillPath);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0 || rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
