using System;
using System.Drawing;
using System.Windows.Forms;

namespace God2Iso
{
    /// <summary>
    /// Dark modal dialog used in place of the system MessageBox — matches the
    /// rest of the app's chrome instead of flashing a bright Windows popup.
    /// Fixed palette so it doesn't track live theme edits.
    /// </summary>
    public static class MessageDialog
    {
        private static readonly Color BgDark = Color.FromArgb(18, 20, 26);
        private static readonly Color Surface = Color.FromArgb(28, 32, 40);
        private static readonly Color BorderDark = Color.FromArgb(45, 51, 66);
        private static readonly Color TextLight = Color.FromArgb(240, 242, 246);
        private static readonly Color MutedText = Color.FromArgb(140, 148, 165);
        private static readonly Color AccentFixed = Color.FromArgb(79, 140, 255);
        private static readonly Color Good = Color.FromArgb(16, 185, 129);
        private static readonly Color Bad = Color.FromArgb(239, 68, 68);
        private static readonly Color Warn = Color.FromArgb(245, 158, 11);

        public enum Kind { Info, Success, Warning, Error }

        public static DialogResult Show(IWin32Window? owner, string title, string message, Kind kind = Kind.Info)
        {
            using var dlg = Build(title, message, kind);
            return dlg.ShowDialog(owner);
        }

        private static Form Build(string title, string message, Kind kind)
        {
            var dlg = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                BackColor = BgDark,
                ForeColor = TextLight,
                Font = new Font("Segoe UI", 9.5f),
                KeyPreview = true,
            };

            var (glyphColor, glyph) = kind switch
            {
                Kind.Success => (Good, "✓"),
                Kind.Warning => (Warn, "⚠"),
                Kind.Error => (Bad, "✕"),
                _ => (AccentFixed, "i"),
            };

            // Measure text so the dialog sizes to fit
            using var g = dlg.CreateGraphics();
            var textSize = TextRenderer.MeasureText(g, message, new Font("Segoe UI", 10f),
                new Size(360, 400), TextFormatFlags.WordBreak);
            int bodyWidth = Math.Max(360, Math.Min(560, textSize.Width + 40));
            int bodyHeight = Math.Max(80, textSize.Height + 24);
            int w = bodyWidth + 110;
            int h = bodyHeight + 110;
            dlg.ClientSize = new Size(w, h);

            // Glyph circle
            var glyphPanel = new GlyphCircle(glyphColor, glyph)
            {
                Location = new Point(24, 24),
                Size = new Size(56, 56),
            };
            dlg.Controls.Add(glyphPanel);

            var titleLbl = new Label
            {
                Text = title,
                Location = new Point(96, 24),
                Size = new Size(w - 120, 24),
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = TextLight,
            };
            dlg.Controls.Add(titleLbl);

            var body = new Label
            {
                Text = message,
                Location = new Point(96, 54),
                Size = new Size(bodyWidth, bodyHeight),
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.Transparent,
                ForeColor = MutedText,
            };
            dlg.Controls.Add(body);

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(w - 110, h - 50),
                Size = new Size(86, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentFixed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnOk.FlatAppearance.BorderColor = AccentFixed;
            btnOk.FlatAppearance.BorderSize = 1;
            btnOk.Click += (_, _) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            dlg.Controls.Add(btnOk);

            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnOk;
            dlg.KeyDown += (_, e) =>
            {
                if (e.KeyCode is Keys.Enter or Keys.Escape) { dlg.DialogResult = DialogResult.OK; dlg.Close(); }
            };
            dlg.HandleCreated += (_, _) => ThemePalette.ApplyDarkTitleBar(dlg);
            dlg.Shown += (_, _) => ThemePalette.ApplyDarkTitleBar(dlg);
            return dlg;
        }

        private sealed class GlyphCircle : Control
        {
            private readonly Color _color;
            private readonly string _glyph;
            public GlyphCircle(Color color, string glyph)
            {
                _color = color;
                _glyph = glyph;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                BackColor = Color.Transparent;
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using var fill = new SolidBrush(Color.FromArgb(50, _color));
                g.FillEllipse(fill, rect);
                using var ring = new Pen(_color, 2);
                g.DrawEllipse(ring, rect);
                using var textBrush = new SolidBrush(_color);
                using var font = new Font("Segoe UI", 20f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_glyph, font, textBrush, rect, sf);
            }
        }
    }
}
