using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace God2Iso
{
    /// <summary>
    /// Modern dark color picker — HSV box + hue slider + hex + RGB/HSV inputs.
    /// Replaces the ancient system ColorDialog that still looks like Windows 95.
    /// Fixed dark chrome so it doesn't shift based on the palette being edited.
    /// </summary>
    public sealed class ColorPickerDialog : Form
    {
        public Color SelectedColor { get; private set; }

        private readonly SvBox _svBox;
        private readonly HueBar _hueBar;
        private readonly Panel _preview;
        private readonly TextBox _hexBox;
        private readonly TextBox _rBox;
        private readonly TextBox _gBox;
        private readonly TextBox _bBox;
        private readonly FlowLayoutPanel _presets;

        private float _hue;
        private float _sat;
        private float _val;
        private bool _syncing;

        // Fixed dark palette so the picker doesn't inherit a half-edited theme.
        private static readonly Color BgDark = Color.FromArgb(23, 26, 33);
        private static readonly Color Surface = Color.FromArgb(31, 35, 44);
        private static readonly Color BorderDark = Color.FromArgb(45, 51, 66);
        private static readonly Color TextLight = Color.FromArgb(240, 242, 246);
        private static readonly Color MutedText = Color.FromArgb(140, 148, 165);
        private static readonly Color AccentFixed = Color.FromArgb(79, 140, 255);

        public ColorPickerDialog(Color startColor)
        {
            SelectedColor = startColor;
            RgbToHsv(startColor, out _hue, out _sat, out _val);

            Text = "Pick a colour";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(460, 400);
            BackColor = BgDark;
            ForeColor = TextLight;
            Font = new Font("Segoe UI", 9.5f);
            KeyPreview = true;

            // SV box — large draggable square
            _svBox = new SvBox { Location = new Point(20, 20), Size = new Size(260, 260) };
            _svBox.Changed += (_, args) =>
            {
                if (_syncing) return;
                _sat = args.s;
                _val = args.v;
                SyncFromHsv();
            };
            Controls.Add(_svBox);

            // Hue bar — vertical slider on the right of the SV box
            _hueBar = new HueBar { Location = new Point(292, 20), Size = new Size(28, 260) };
            _hueBar.Changed += h =>
            {
                if (_syncing) return;
                _hue = h;
                _svBox.Hue = h;
                SyncFromHsv();
            };
            Controls.Add(_hueBar);

            // Preview swatch
            _preview = new Panel
            {
                Location = new Point(340, 20),
                Size = new Size(100, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = startColor,
            };
            Controls.Add(_preview);

            // Hex + RGB inputs
            Controls.Add(MakeLabel("Hex", 340, 130));
            _hexBox = MakeInput(340, 150, 100);
            _hexBox.Font = new Font("Consolas", 10f);
            _hexBox.MaxLength = 7;
            _hexBox.TextChanged += OnHexChanged;
            Controls.Add(_hexBox);

            Controls.Add(MakeLabel("R", 340, 184));
            _rBox = MakeInput(360, 182, 70);
            _rBox.TextChanged += OnRgbChanged;
            Controls.Add(_rBox);

            Controls.Add(MakeLabel("G", 340, 214));
            _gBox = MakeInput(360, 212, 70);
            _gBox.TextChanged += OnRgbChanged;
            Controls.Add(_gBox);

            Controls.Add(MakeLabel("B", 340, 244));
            _bBox = MakeInput(360, 242, 70);
            _bBox.TextChanged += OnRgbChanged;
            Controls.Add(_bBox);

            // Preset swatches row (6 brand-friendly accents)
            Controls.Add(MakeLabel("Presets", 20, 290));
            _presets = new FlowLayoutPanel
            {
                Location = new Point(20, 310),
                Size = new Size(420, 36),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = BgDark,
            };
            foreach (var hex in new[]
            {
                "#4F8CFF", "#8B5CF6", "#EC4899", "#EF4444",
                "#F59E0B", "#10B981", "#06B6D4", "#F2F3F5",
                "#0F1115", "#171A21",
            })
            {
                _presets.Controls.Add(MakePresetSwatch(hex));
            }
            Controls.Add(_presets);

            // Buttons
            var btnCancel = MakeFlatButton("Cancel", 260, 358, 80, 32, primary: false);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            var btnOk = MakeFlatButton("Pick", 350, 358, 90, 32, primary: true);
            btnOk.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Load += (_, _) =>
            {
                _hueBar.Hue = _hue;
                _svBox.Hue = _hue;
                _svBox.SetSV(_sat, _val);
                SyncFromHsv();
            };
            HandleCreated += (_, _) => ThemePalette.ApplyDarkTitleBar(this);
            Shown += (_, _) => ThemePalette.ApplyDarkTitleBar(this);
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };
        }

        private void SyncFromHsv()
        {
            _syncing = true;
            SelectedColor = HsvToRgb(_hue, _sat, _val);
            _preview.BackColor = SelectedColor;
            _hexBox.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            _rBox.Text = SelectedColor.R.ToString();
            _gBox.Text = SelectedColor.G.ToString();
            _bBox.Text = SelectedColor.B.ToString();
            _svBox.SetSV(_sat, _val);
            _hueBar.Hue = _hue;
            _syncing = false;
        }

        private void OnHexChanged(object? sender, EventArgs e)
        {
            if (_syncing) return;
            try
            {
                var c = ColorTranslator.FromHtml(_hexBox.Text);
                _syncing = true;
                SelectedColor = c;
                RgbToHsv(c, out _hue, out _sat, out _val);
                _preview.BackColor = c;
                _rBox.Text = c.R.ToString();
                _gBox.Text = c.G.ToString();
                _bBox.Text = c.B.ToString();
                _svBox.Hue = _hue;
                _svBox.SetSV(_sat, _val);
                _hueBar.Hue = _hue;
                _syncing = false;
            }
            catch { _syncing = false; }
        }

        private void OnRgbChanged(object? sender, EventArgs e)
        {
            if (_syncing) return;
            if (!int.TryParse(_rBox.Text, out var r) ||
                !int.TryParse(_gBox.Text, out var g) ||
                !int.TryParse(_bBox.Text, out var b)) return;
            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);
            var c = Color.FromArgb(r, g, b);
            _syncing = true;
            SelectedColor = c;
            RgbToHsv(c, out _hue, out _sat, out _val);
            _preview.BackColor = c;
            _hexBox.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            _svBox.Hue = _hue;
            _svBox.SetSV(_sat, _val);
            _hueBar.Hue = _hue;
            _syncing = false;
        }

        private Label MakeLabel(string text, int x, int y) => new()
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            ForeColor = MutedText,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
        };

        private TextBox MakeInput(int x, int y, int width) => new()
        {
            Location = new Point(x, y),
            Size = new Size(width, 24),
            BackColor = Surface,
            ForeColor = TextLight,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10f),
        };

        private Button MakeFlatButton(string text, int x, int y, int w, int h, bool primary)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            if (primary)
            {
                b.BackColor = AccentFixed;
                b.ForeColor = Color.White;
                b.FlatAppearance.BorderColor = AccentFixed;
            }
            else
            {
                b.BackColor = Surface;
                b.ForeColor = TextLight;
                b.FlatAppearance.BorderColor = BorderDark;
            }
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Panel MakePresetSwatch(string hex)
        {
            var color = ColorTranslator.FromHtml(hex);
            var swatch = new Panel
            {
                Size = new Size(30, 30),
                BackColor = color,
                Margin = new Padding(0, 0, 6, 0),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
            };
            swatch.Click += (_, _) =>
            {
                SelectedColor = color;
                RgbToHsv(color, out _hue, out _sat, out _val);
                SyncFromHsv();
            };
            return swatch;
        }

        // ─── HSV ↔ RGB utilities ───
        private static void RgbToHsv(Color c, out float h, out float s, out float v)
        {
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            v = max;
            float delta = max - min;
            s = max == 0 ? 0 : delta / max;
            if (delta < 1e-5f) { h = 0; return; }
            if (max == r) h = (g - b) / delta;
            else if (max == g) h = 2f + (b - r) / delta;
            else h = 4f + (r - g) / delta;
            h *= 60f;
            if (h < 0) h += 360f;
        }

        private static Color HsvToRgb(float h, float s, float v)
        {
            float c = v * s;
            float x = c * (1 - Math.Abs(((h / 60f) % 2) - 1));
            float m = v - c;
            float rp, gp, bp;
            if (h < 60) { rp = c; gp = x; bp = 0; }
            else if (h < 120) { rp = x; gp = c; bp = 0; }
            else if (h < 180) { rp = 0; gp = c; bp = x; }
            else if (h < 240) { rp = 0; gp = x; bp = c; }
            else if (h < 300) { rp = x; gp = 0; bp = c; }
            else { rp = c; gp = 0; bp = x; }
            int r = (int)Math.Round((rp + m) * 255);
            int g = (int)Math.Round((gp + m) * 255);
            int b = (int)Math.Round((bp + m) * 255);
            return Color.FromArgb(Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
        }

        // ─── Saturation / Value picker box ───
        private sealed class SvBox : Control
        {
            public event EventHandler<(float s, float v)>? Changed;
            public float Hue { get => _hue; set { _hue = value; Invalidate(); } }
            private float _hue;
            private float _sat;
            private float _val = 1f;
            private bool _dragging;

            public SvBox()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Cross;
            }

            public void SetSV(float s, float v)
            {
                _sat = s; _val = v; Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs e) { _dragging = true; PickFrom(e); }
            protected override void OnMouseMove(MouseEventArgs e) { if (_dragging) PickFrom(e); }
            protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; }

            private void PickFrom(MouseEventArgs e)
            {
                _sat = Math.Clamp((float)e.X / Math.Max(1, Width - 1), 0f, 1f);
                _val = 1f - Math.Clamp((float)e.Y / Math.Max(1, Height - 1), 0f, 1f);
                Invalidate();
                Changed?.Invoke(this, (_sat, _val));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Saturation gradient (white → full-sat hue), left to right
                var hueColor = HsvToRgb(_hue, 1f, 1f);
                using (var baseBrush = new LinearGradientBrush(ClientRectangle,
                    Color.White, hueColor, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(baseBrush, ClientRectangle);
                }
                // Value overlay (transparent → black), top to bottom
                using (var vBrush = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(0, 0, 0, 0), Color.Black, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(vBrush, ClientRectangle);
                }
                // Crosshair at (sat, 1-val)
                int cx = (int)(_sat * Width);
                int cy = (int)((1f - _val) * Height);
                using var pen = new Pen(Color.White, 2);
                g.DrawEllipse(pen, cx - 6, cy - 6, 12, 12);
                using var innerPen = new Pen(Color.Black, 1);
                g.DrawEllipse(innerPen, cx - 7, cy - 7, 14, 14);
            }
        }

        // ─── Vertical hue slider ───
        private sealed class HueBar : Control
        {
            public event Action<float>? Changed;
            private float _hue;
            public float Hue { get => _hue; set { _hue = Math.Clamp(value, 0f, 360f); Invalidate(); } }
            private bool _dragging;

            public HueBar()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
            }

            protected override void OnMouseDown(MouseEventArgs e) { _dragging = true; PickFrom(e); }
            protected override void OnMouseMove(MouseEventArgs e) { if (_dragging) PickFrom(e); }
            protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; }

            private void PickFrom(MouseEventArgs e)
            {
                _hue = Math.Clamp((float)e.Y / Math.Max(1, Height - 1), 0f, 1f) * 360f;
                Invalidate();
                Changed?.Invoke(_hue);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                var rect = ClientRectangle;
                for (int y = 0; y < rect.Height; y++)
                {
                    float h = y / (float)rect.Height * 360f;
                    var c = HsvToRgb(h, 1f, 1f);
                    using var pen = new Pen(c);
                    g.DrawLine(pen, 0, y, rect.Width, y);
                }
                // Marker
                int my = (int)(_hue / 360f * rect.Height);
                using var markerPen = new Pen(Color.White, 2);
                g.DrawRectangle(markerPen, 0, Math.Clamp(my - 2, 0, rect.Height - 4), rect.Width - 1, 4);
                using var innerMarker = new Pen(Color.Black, 1);
                g.DrawRectangle(innerMarker, 0, Math.Clamp(my - 3, 0, rect.Height - 6), rect.Width - 1, 6);
            }
        }
    }
}
