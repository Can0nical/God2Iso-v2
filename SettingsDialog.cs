using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace God2Iso
{
    /// <summary>
    /// Modal settings editor — covers every customization SeriesXM asked for
    /// (output default, source default, auto-populate toggle, confirmation
    /// toggle, completion dialog toggle, color palette).
    /// </summary>
    public sealed class SettingsDialog : Form
    {
        public AppSettings ResultSettings { get; private set; }

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
            ThemePalette.Apply(this);
        }

        private void BuildUi()
        {
            Text = "Settings";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(540, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            int y = 20;

            Controls.Add(SectionHeader("Defaults", 20, y)); y += 32;

            Controls.Add(FieldLabel("Default output folder", 20, y)); y += 22;
            _txtOutput.Location = new Point(20, y);
            _txtOutput.Size = new Size(400, 28);
            _txtOutput.Text = ResultSettings.DefaultOutputFolder;
            Controls.Add(_txtOutput);
            Controls.Add(BrowseButton(430, y, picked => _txtOutput.Text = picked));
            y += 42;

            Controls.Add(FieldLabel("Default source folder (auto-populate on launch)", 20, y)); y += 22;
            _txtSource.Location = new Point(20, y);
            _txtSource.Size = new Size(400, 28);
            _txtSource.Text = ResultSettings.DefaultSourceFolder;
            Controls.Add(_txtSource);
            Controls.Add(BrowseButton(430, y, picked => _txtSource.Text = picked));
            y += 40;

            _cbAutoPopulate.Text = "Auto-populate list from source folder on launch";
            _cbAutoPopulate.AutoSize = true;
            _cbAutoPopulate.Location = new Point(20, y);
            _cbAutoPopulate.Checked = ResultSettings.AutoPopulateOnStart;
            Controls.Add(_cbAutoPopulate); y += 24;

            _cbSkipConfirm.Text = "Skip \"Scan complete\" confirmation popup (show toast instead)";
            _cbSkipConfirm.AutoSize = true;
            _cbSkipConfirm.Location = new Point(20, y);
            _cbSkipConfirm.Checked = ResultSettings.SkipBatchConfirmPrompt;
            Controls.Add(_cbSkipConfirm); y += 24;

            _cbShowCompletion.Text = "Show popup when conversion finishes";
            _cbShowCompletion.AutoSize = true;
            _cbShowCompletion.Location = new Point(20, y);
            _cbShowCompletion.Checked = ResultSettings.ShowCompletionDialog;
            Controls.Add(_cbShowCompletion); y += 32;

            Controls.Add(SectionHeader("Theme", 20, y)); y += 32;

            AddSwatch("Background", _swatchBg, ResultSettings.BackgroundColorHex, 20, y,
                hex => ResultSettings.BackgroundColorHex = hex);
            AddSwatch("Surface", _swatchSurface, ResultSettings.SurfaceColorHex, 190, y,
                hex => ResultSettings.SurfaceColorHex = hex);
            AddSwatch("Accent", _swatchAccent, ResultSettings.AccentColorHex, 360, y,
                hex => ResultSettings.AccentColorHex = hex);
            y += 56;

            AddSwatch("Text", _swatchText, ResultSettings.TextColorHex, 20, y,
                hex => ResultSettings.TextColorHex = hex);
            AddSwatch("Muted text", _swatchMuted, ResultSettings.MutedTextColorHex, 190, y,
                hex => ResultSettings.MutedTextColorHex = hex);
            AddSwatch("Border", _swatchBorder, ResultSettings.BorderColorHex, 360, y,
                hex => ResultSettings.BorderColorHex = hex);
            y += 72;

            var btnReset = new Button
            {
                Text = "Reset theme to default",
                Location = new Point(20, y),
                Size = new Size(200, 32),
            };
            btnReset.Click += (_, _) =>
            {
                var def = new AppSettings();
                ResultSettings.BackgroundColorHex = def.BackgroundColorHex;
                ResultSettings.SurfaceColorHex = def.SurfaceColorHex;
                ResultSettings.AccentColorHex = def.AccentColorHex;
                ResultSettings.TextColorHex = def.TextColorHex;
                ResultSettings.MutedTextColorHex = def.MutedTextColorHex;
                ResultSettings.BorderColorHex = def.BorderColorHex;
                _swatchBg.BackColor = ThemePalette.ParseHex(def.BackgroundColorHex, Color.Black);
                _swatchSurface.BackColor = ThemePalette.ParseHex(def.SurfaceColorHex, Color.Black);
                _swatchAccent.BackColor = ThemePalette.ParseHex(def.AccentColorHex, Color.Blue);
                _swatchText.BackColor = ThemePalette.ParseHex(def.TextColorHex, Color.White);
                _swatchMuted.BackColor = ThemePalette.ParseHex(def.MutedTextColorHex, Color.Gray);
                _swatchBorder.BackColor = ThemePalette.ParseHex(def.BorderColorHex, Color.DarkSlateGray);
            };
            Controls.Add(btnReset);

            // Buttons
            var btnCancel = new Button { Text = "Cancel", Location = new Point(340, 510), Size = new Size(90, 32) };
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            var btnOk = new Button { Text = "Save", Location = new Point(440, 510), Size = new Size(80, 32) };
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

        private static Label SectionHeader(string text, int x, int y) => new()
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, y),
        };

        private static Label FieldLabel(string text, int x, int y) => new()
        {
            Text = text,
            AutoSize = true,
            Tag = "muted",
            Location = new Point(x, y),
        };

        private Button BrowseButton(int x, int y, Action<string> onPicked)
        {
            var b = new Button { Text = "Browse…", Location = new Point(x, y - 1), Size = new Size(90, 30) };
            b.Click += (_, _) =>
            {
                using var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };
                if (dlg.ShowDialog(this) == DialogResult.OK) onPicked(dlg.SelectedPath);
            };
            return b;
        }

        private void AddSwatch(string label, Panel swatch, string hex, int x, int y, Action<string> onPicked)
        {
            var lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Tag = "muted" };
            swatch.Size = new Size(28, 28);
            swatch.Location = new Point(x, y + 20);
            swatch.BorderStyle = BorderStyle.FixedSingle;
            swatch.BackColor = ThemePalette.ParseHex(hex, Color.Black);
            swatch.Cursor = Cursors.Hand;
            var txt = new TextBox
            {
                Text = hex,
                Location = new Point(x + 36, y + 22),
                Size = new Size(100, 24),
                Font = new Font("Consolas", 9f),
            };
            txt.TextChanged += (_, _) =>
            {
                try
                {
                    var c = ColorTranslator.FromHtml(txt.Text);
                    swatch.BackColor = c;
                    onPicked(txt.Text);
                }
                catch { }
            };
            swatch.Click += (_, _) =>
            {
                using var cd = new ColorDialog { Color = swatch.BackColor, FullOpen = true };
                if (cd.ShowDialog(this) == DialogResult.OK)
                {
                    swatch.BackColor = cd.Color;
                    txt.Text = ThemePalette.ToHex(cd.Color);
                    onPicked(txt.Text);
                }
            };
            Controls.Add(lbl);
            Controls.Add(swatch);
            Controls.Add(txt);
        }
    }
}
