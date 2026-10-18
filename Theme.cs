using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace God2Iso
{
    /// <summary>
    /// Applies a dark Fluent-inspired palette to every control recursively.
    /// Reads colors from AppSettings so the user can retint from Settings.
    /// </summary>
    public static class ThemePalette
    {
        public static Color Background { get; private set; } = Color.FromArgb(15, 17, 21);
        public static Color Surface { get; private set; } = Color.FromArgb(23, 26, 33);
        public static Color Accent { get; private set; } = Color.FromArgb(79, 140, 255);
        public static Color Text { get; private set; } = Color.FromArgb(242, 243, 245);
        public static Color MutedText { get; private set; } = Color.FromArgb(138, 145, 163);
        public static Color Border { get; private set; } = Color.FromArgb(36, 41, 56);

        public static void Load(AppSettings s)
        {
            Background = ParseHex(s.BackgroundColorHex, Background);
            Surface = ParseHex(s.SurfaceColorHex, Surface);
            Accent = ParseHex(s.AccentColorHex, Accent);
            Text = ParseHex(s.TextColorHex, Text);
            MutedText = ParseHex(s.MutedTextColorHex, MutedText);
            Border = ParseHex(s.BorderColorHex, Border);
        }

        public static Color ParseHex(string hex, Color fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return fallback;
                return ColorTranslator.FromHtml(hex);
            }
            catch { return fallback; }
        }

        public static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        /// <summary>Walks every control in the tree and applies the palette.</summary>
        public static void Apply(Control root)
        {
            ApplyDarkTitleBar(root);
            ApplyRecursive(root);
        }

        private static void ApplyRecursive(Control c)
        {
            // Order matters: check derived types before their base types.
            if (c is Form f)
            {
                f.BackColor = Background;
                f.ForeColor = Text;
            }
            else if (c is CheckBox cb)
            {
                cb.BackColor = Color.Transparent;
                cb.ForeColor = Text;
                cb.FlatStyle = FlatStyle.Flat;
            }
            else if (c is Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Border;
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.MouseOverBackColor = Surface;
                b.FlatAppearance.MouseDownBackColor = Accent;
                b.BackColor = Surface;
                b.ForeColor = Text;
                b.Cursor = Cursors.Hand;
            }
            else if (c is TextBox tb)
            {
                tb.BackColor = Surface;
                tb.ForeColor = Text;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is ListBox lb)
            {
                lb.BackColor = Surface;
                lb.ForeColor = Text;
                lb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is ProgressBar pb)
            {
                pb.BackColor = Surface;
                pb.ForeColor = Accent;
            }
            else if (c is LinkLabel link)
            {
                link.LinkColor = Accent;
                link.ActiveLinkColor = Accent;
                link.VisitedLinkColor = Accent;
                link.BackColor = Color.Transparent;
            }
            else if (c is Label lbl)
            {
                lbl.BackColor = Color.Transparent;
                lbl.ForeColor = lbl.Tag as string == "muted" ? MutedText : Text;
            }
            else if (c is GroupBox gb)
            {
                gb.BackColor = Background;
                gb.ForeColor = Accent;
            }
            else if (c is Panel p)
            {
                p.BackColor = p.Tag as string == "surface" ? Surface : Background;
                p.ForeColor = Text;
            }

            foreach (Control child in c.Controls) ApplyRecursive(child);
        }

        // ─── Windows 11 dark title bar via DWMWA_USE_IMMERSIVE_DARK_MODE ───
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static void ApplyDarkTitleBar(Control root)
        {
            if (root is not Form form) return;
            try
            {
                int on = 1;
                DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int));
            }
            catch { /* older Windows — just skip */ }
        }
    }
}
