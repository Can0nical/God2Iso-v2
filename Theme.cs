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
                b.FlatAppearance.BorderSize = 1;
                b.Cursor = Cursors.Hand;
                if (b.Tag as string == "primary")
                {
                    b.BackColor = Accent;
                    b.ForeColor = Color.White;
                    b.FlatAppearance.BorderColor = Accent;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                        255, Math.Min(255, Accent.R + 20),
                        Math.Min(255, Accent.G + 20),
                        Math.Min(255, Accent.B + 20));
                    b.FlatAppearance.MouseDownBackColor = Color.FromArgb(
                        255, Math.Max(0, Accent.R - 20),
                        Math.Max(0, Accent.G - 20),
                        Math.Max(0, Accent.B - 20));
                }
                else
                {
                    b.BackColor = Surface;
                    b.ForeColor = Text;
                    b.FlatAppearance.BorderColor = Border;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                        255, Math.Min(255, Surface.R + 15),
                        Math.Min(255, Surface.G + 15),
                        Math.Min(255, Surface.B + 15));
                    b.FlatAppearance.MouseDownBackColor = Accent;
                }
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
            else if (c is FlowLayoutPanel flow)
            {
                flow.BackColor = Color.Transparent;
                flow.ForeColor = Text;
            }
            else if (c is TableLayoutPanel tlp)
            {
                tlp.BackColor = Background;
                tlp.ForeColor = Text;
            }
            else if (c is Panel p)
            {
                var tag = p.Tag as string;
                p.BackColor = tag switch
                {
                    "header" => Surface,
                    "surface" => Surface,
                    "card" => Surface,
                    _ => Background,
                };
                p.ForeColor = Text;
            }
            else if (c is ThemedProgressBar)
            {
                // ThemedProgressBar owner-draws using palette directly; no setup needed.
            }

            foreach (Control child in c.Controls) ApplyRecursive(child);
        }

        // ─── Windows 11 dark title bar via DWMWA_USE_IMMERSIVE_DARK_MODE ───
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

        public static void ApplyDarkTitleBar(Form form)
        {
            try
            {
                int on = 1;
                // Try the current attribute first (20), fall back to the pre-build 18985 one (19).
                if (DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref on, sizeof(int));
                }
            }
            catch { /* older Windows — just skip */ }
        }

        private static void ApplyDarkTitleBar(Control root)
        {
            if (root is Form f) ApplyDarkTitleBar(f);
        }
    }
}
