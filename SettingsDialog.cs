using System;
using System.Drawing;
using System.Windows.Forms;

namespace God2Iso
{
    /// <summary>
    /// Modern settings editor — fixed dark chrome (doesn't track the palette
    /// being edited so you can turn Background pink without pinking the dialog).
    /// </summary>
    public sealed class SettingsDialog : Form
    {
        public AppSettings ResultSettings { get; private set; }

        // Fixed chrome palette — intentionally independent of ThemePalette so
        // the dialog looks the same while the user is mid-edit.
        private static readonly Color BgDark = Color.FromArgb(18, 20, 26);
        private static readonly Color Surface = Color.FromArgb(28, 32, 40);
        private static readonly Color SurfaceHi = Color.FromArgb(36, 40, 51);
        private static readonly Color BorderDark = Color.FromArgb(45, 51, 66);
        private static readonly Color TextLight = Color.FromArgb(240, 242, 246);
        private static readonly Color MutedText = Color.FromArgb(140, 148, 165);
        private static readonly Color AccentFixed = Color.FromArgb(79, 140, 255);

        private readonly TextBox _txtOutput = new();
        private readonly TextBox _txtSource = new();
        private readonly CheckBox _cbAutoPopulate = new();
        private readonly CheckBox _cbSkipConfirm = new();
        private readonly CheckBox _cbShowCompletion = new();

        private readonly Panel _swatchBg = new();
        private readonly Panel _swatchSurface = new();
        private readonly Panel _swatchAccent = new();
        private readonly Panel _swatchText = new();
        private readonly Panel _swatchMuted = new();
        private readonly Panel _swatchBorder = new();

        private readonly TextBox _hexBg = new();
        private readonly TextBox _hexSurface = new();
        private readonly TextBox _hexAccent = new();
        private readonly TextBox _hexText = new();
        private readonly TextBox _hexMuted = new();
        private readonly TextBox _hexBorder = new();

        public SettingsDialog(AppSettings current)
        {
            ResultSettings = new AppSettings
            {
                DefaultOutputFolder = current.DefaultOutputFolder,
                DefaultSourceFolder = current.DefaultSourceFolder,
                AutoPopulateOnStart = current.AutoPopulateOnStart,
                FixCreateIsoHeader = current.FixCreateIsoHeader,
                SkipBatchConfirmPrompt = current.SkipBatchConfirmPrompt,
                ShowCompletionDialog = current.ShowCompletionDialog,
                BackgroundColorHex = current.BackgroundColorHex,
                SurfaceColorHex = current.SurfaceColorHex,
                AccentColorHex = current.AccentColorHex,
                TextColorHex = current.TextColorHex,
                MutedTextColorHex = current.MutedTextColorHex,
                BorderColorHex = current.BorderColorHex,
            };
            BuildUi();
            HandleCreated += (_, _) => ThemePalette.ApplyDarkTitleBar(this);
            Shown += (_, _) => ThemePalette.ApplyDarkTitleBar(this);
        }

        private void BuildUi()
        {
            Text = "Settings";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(620, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 9.5f);
            BackColor = BgDark;
            ForeColor = TextLight;
            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };

            int y = 24;

            Controls.Add(SectionHeader("Defaults", 28, y)); y += 36;

            Controls.Add(FieldLabel("Default output folder", 28, y)); y += 22;
            StyleInput(_txtOutput, 28, y, 460);
            _txtOutput.Text = ResultSettings.DefaultOutputFolder;
            Controls.Add(_txtOutput);
            Controls.Add(BrowseButton(498, y - 1, picked => _txtOutput.Text = picked));
            y += 44;

            Controls.Add(FieldLabel("Default source folder (auto-populate on launch)", 28, y)); y += 22;
            StyleInput(_txtSource, 28, y, 460);
            _txtSource.Text = ResultSettings.DefaultSourceFolder;
            Controls.Add(_txtSource);
            Controls.Add(BrowseButton(498, y - 1, picked => _txtSource.Text = picked));
            y += 42;

            StyleCheck(_cbAutoPopulate, "Auto-populate list from source folder on launch", 28, y);
            _cbAutoPopulate.Checked = ResultSettings.AutoPopulateOnStart;
            Controls.Add(_cbAutoPopulate); y += 26;

            StyleCheck(_cbSkipConfirm, "Skip \"Scan complete\" popup (show toast instead)", 28, y);
            _cbSkipConfirm.Checked = ResultSettings.SkipBatchConfirmPrompt;
            Controls.Add(_cbSkipConfirm); y += 26;

            StyleCheck(_cbShowCompletion, "Show popup when conversion finishes", 28, y);
            _cbShowCompletion.Checked = ResultSettings.ShowCompletionDialog;
            Controls.Add(_cbShowCompletion); y += 36;

            Controls.Add(SectionHeader("Theme", 28, y)); y += 36;

            // 2×3 grid of color swatches — bigger, cleaner
            int row1 = y;
            int row2 = y + 76;
            AddSwatchRow("Background", _swatchBg, _hexBg,
                ResultSettings.BackgroundColorHex, 28, row1,
                hex => ResultSettings.BackgroundColorHex = hex);
            AddSwatchRow("Surface", _swatchSurface, _hexSurface,
                ResultSettings.SurfaceColorHex, 220, row1,
                hex => ResultSettings.SurfaceColorHex = hex);
            AddSwatchRow("Accent", _swatchAccent, _hexAccent,
                ResultSettings.AccentColorHex, 412, row1,
                hex => ResultSettings.AccentColorHex = hex);

            AddSwatchRow("Text", _swatchText, _hexText,
                ResultSettings.TextColorHex, 28, row2,
                hex => ResultSettings.TextColorHex = hex);
            AddSwatchRow("Muted text", _swatchMuted, _hexMuted,
                ResultSettings.MutedTextColorHex, 220, row2,
                hex => ResultSettings.MutedTextColorHex = hex);
            AddSwatchRow("Border", _swatchBorder, _hexBorder,
                ResultSettings.BorderColorHex, 412, row2,
                hex => ResultSettings.BorderColorHex = hex);

            y = row2 + 76;

            var btnReset = MakeFlatButton("↺  Reset theme to default", 28, y, 220, 36, primary: false);
            btnReset.Click += (_, _) =>
            {
                var def = new AppSettings();
                ResultSettings.BackgroundColorHex = def.BackgroundColorHex;
                ResultSettings.SurfaceColorHex = def.SurfaceColorHex;
                ResultSettings.AccentColorHex = def.AccentColorHex;
                ResultSettings.TextColorHex = def.TextColorHex;
                ResultSettings.MutedTextColorHex = def.MutedTextColorHex;
                ResultSettings.BorderColorHex = def.BorderColorHex;
                RefreshSwatch(_swatchBg, _hexBg, def.BackgroundColorHex);
                RefreshSwatch(_swatchSurface, _hexSurface, def.SurfaceColorHex);
                RefreshSwatch(_swatchAccent, _hexAccent, def.AccentColorHex);
                RefreshSwatch(_swatchText, _hexText, def.TextColorHex);
                RefreshSwatch(_swatchMuted, _hexMuted, def.MutedTextColorHex);
                RefreshSwatch(_swatchBorder, _hexBorder, def.BorderColorHex);
            };
            Controls.Add(btnReset);

            // Footer buttons
            var btnCancel = MakeFlatButton("Cancel", 400, 570, 90, 34, primary: false);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            var btnOk = MakeFlatButton("Save", 500, 570, 90, 34, primary: true);
            btnOk.Click += (_, _) =>
            {
                ResultSettings.DefaultOutputFolder = _txtOutput.Text.Trim();
                ResultSettings.DefaultSourceFolder = _txtSource.Text.Trim();
                ResultSettings.AutoPopulateOnStart = _cbAutoPopulate.Checked;
                ResultSettings.SkipBatchConfirmPrompt = _cbSkipConfirm.Checked;
                ResultSettings.ShowCompletionDialog = _cbShowCompletion.Checked;
                DialogResult = DialogResult.OK;
                Close();
            };
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
        }

        private Label SectionHeader(string text, int x, int y) => new()
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, y),
            BackColor = Color.Transparent,
            ForeColor = TextLight,
        };

        private Label FieldLabel(string text, int x, int y) => new()
        {
            Text = text,
            AutoSize = true,
            BackColor = Color.Transparent,
            ForeColor = MutedText,
            Location = new Point(x, y),
            Font = new Font("Segoe UI", 9f),
        };

        private void StyleInput(TextBox t, int x, int y, int width)
        {
            t.Location = new Point(x, y);
            t.Size = new Size(width, 28);
            t.BackColor = Surface;
            t.ForeColor = TextLight;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = new Font("Segoe UI", 10f);
        }

        private void StyleCheck(CheckBox cb, string label, int x, int y)
        {
            cb.Text = label;
            cb.AutoSize = true;
            cb.Location = new Point(x, y);
            cb.BackColor = Color.Transparent;
            cb.ForeColor = TextLight;
            cb.FlatStyle = FlatStyle.Flat;
            cb.Cursor = Cursors.Hand;
        }

        private Button BrowseButton(int x, int y, Action<string> onPicked)
        {
            var b = MakeFlatButton("Browse…", x, y, 90, 30, primary: false);
            b.Click += (_, _) =>
            {
                using var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };
                if (dlg.ShowDialog(this) == DialogResult.OK) onPicked(dlg.SelectedPath);
            };
            return b;
        }

        private Button MakeFlatButton(string text, int x, int y, int w, int h, bool primary)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, primary ? FontStyle.Bold : FontStyle.Regular),
            };
            if (primary)
            {
                b.BackColor = AccentFixed;
                b.ForeColor = Color.White;
                b.FlatAppearance.BorderColor = AccentFixed;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                    255, Math.Min(255, AccentFixed.R + 20),
                    Math.Min(255, AccentFixed.G + 20),
                    Math.Min(255, AccentFixed.B + 20));
            }
            else
            {
                b.BackColor = SurfaceHi;
                b.ForeColor = TextLight;
                b.FlatAppearance.BorderColor = BorderDark;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 50, 64);
            }
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void AddSwatchRow(string label, Panel swatch, TextBox hexBox,
            string initialHex, int x, int y, Action<string> onPicked)
        {
            Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = MutedText,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            });

            swatch.Size = new Size(44, 44);
            swatch.Location = new Point(x, y + 22);
            swatch.BackColor = SafeColor(initialHex);
            swatch.Cursor = Cursors.Hand;
            swatch.BorderStyle = BorderStyle.FixedSingle;
            swatch.Click += (_, _) =>
            {
                using var dlg = new ColorPickerDialog(swatch.BackColor);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var hex = $"#{dlg.SelectedColor.R:X2}{dlg.SelectedColor.G:X2}{dlg.SelectedColor.B:X2}";
                    swatch.BackColor = dlg.SelectedColor;
                    hexBox.Text = hex;
                    onPicked(hex);
                }
            };
            Controls.Add(swatch);

            hexBox.Location = new Point(x + 52, y + 32);
            hexBox.Size = new Size(110, 24);
            hexBox.BackColor = Surface;
            hexBox.ForeColor = TextLight;
            hexBox.BorderStyle = BorderStyle.FixedSingle;
            hexBox.Font = new Font("Consolas", 10f);
            hexBox.Text = initialHex;
            hexBox.TextChanged += (_, _) =>
            {
                try
                {
                    swatch.BackColor = ColorTranslator.FromHtml(hexBox.Text);
                    onPicked(hexBox.Text);
                }
                catch { }
            };
            Controls.Add(hexBox);
        }

        private void RefreshSwatch(Panel swatch, TextBox hexBox, string hex)
        {
            swatch.BackColor = SafeColor(hex);
            hexBox.Text = hex;
        }

        private static Color SafeColor(string hex)
        {
            try { return ColorTranslator.FromHtml(hex); } catch { return Color.Black; }
        }
    }
}
