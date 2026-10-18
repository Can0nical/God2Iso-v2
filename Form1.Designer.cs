#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace God2Iso
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _headerPanel = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private Button _btnSettings = null!;

        private Panel _bodyPanel = null!;

        private Label _lblPackages = null!;
        private ListBox _listPackages = null!;
        private Button _btnAdd = null!;
        private Button _btnScanFolder = null!;
        private Button _btnRemove = null!;
        private Button _btnClear = null!;

        private Label _lblOutput = null!;
        private TextBox _textOutput = null!;
        private Button _btnBrowseOutput = null!;

        private CheckBox _cbFix = null!;

        private Label _lblIso = null!;
        private ProgressBar _progressIso = null!;
        private Label _lblTotal = null!;
        private ProgressBar _progressTotal = null!;

        private Button _btnGo = null!;
        private Label _statusLabel = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components is not null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();

            // ── Form ──
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(760, 600);
            MinimumSize = new Size(640, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "God2Iso";

            // ── Header strip ──
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                Tag = "surface",
            };

            _titleLabel = new Label
            {
                Text = "GoD → ISO",
                Location = new Point(20, 14),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 15f, FontStyle.Bold),
            };
            _subtitleLabel = new Label
            {
                Text = "Convert Xbox 360 Games on Demand packages into bootable ISO images.",
                Location = new Point(22, 44),
                AutoSize = true,
                Tag = "muted",
                Font = new Font("Segoe UI", 9f),
            };
            _btnSettings = new Button
            {
                Text = "⚙  Settings",
                Size = new Size(110, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _btnSettings.Location = new Point(ClientSize.Width - _btnSettings.Width - 20, 20);
            _btnSettings.Click += OnSettingsClick;

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_subtitleLabel);
            _headerPanel.Controls.Add(_btnSettings);

            // ── Body ──
            _bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 16, 20, 16),
            };

            // Packages label
            _lblPackages = new Label
            {
                Text = "Packages",
                Location = new Point(20, 86),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            };

            // List
            _listPackages = new ListBox
            {
                Location = new Point(20, 112),
                Size = new Size(530, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                HorizontalScrollbar = true,
                IntegralHeight = false,
                SelectionMode = SelectionMode.One,
            };
            _listPackages.SelectedIndexChanged += OnListSelectionChanged;

            // Action buttons column
            _btnAdd = MakeActionButton("＋  Add file", new Point(560, 112));
            _btnAdd.Click += OnAddClick;
            _btnScanFolder = MakeActionButton("🔎  Scan folder…", new Point(560, 150));
            _btnScanFolder.Click += OnScanFolderClick;
            _btnRemove = MakeActionButton("－  Remove", new Point(560, 188));
            _btnRemove.Enabled = false;
            _btnRemove.Click += OnRemoveClick;
            _btnClear = MakeActionButton("Clear all", new Point(560, 226));
            _btnClear.Click += OnClearClick;

            // Output field
            _lblOutput = new Label
            {
                Text = "Output folder",
                Location = new Point(20, 310),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            };
            _textOutput = new TextBox
            {
                Location = new Point(20, 336),
                Size = new Size(530, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10f),
            };
            _btnBrowseOutput = MakeActionButton("Browse…", new Point(560, 334));
            _btnBrowseOutput.Click += OnBrowseOutputClick;

            // Fix checkbox
            _cbFix = new CheckBox
            {
                Text = "Fix / rebuild Create-ISO Good headers",
                Location = new Point(20, 376),
                AutoSize = true,
            };
            _cbFix.CheckedChanged += OnFixCheckedChanged;

            // Progress labels + bars
            _lblIso = new Label
            {
                Text = "Current ISO",
                Location = new Point(20, 410),
                AutoSize = true,
                Tag = "muted",
                Font = new Font("Segoe UI", 9f),
            };
            _progressIso = new ProgressBar
            {
                Location = new Point(20, 430),
                Size = new Size(720, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Style = ProgressBarStyle.Continuous,
            };
            _lblTotal = new Label
            {
                Text = "Total",
                Location = new Point(20, 456),
                AutoSize = true,
                Tag = "muted",
                Font = new Font("Segoe UI", 9f),
            };
            _progressTotal = new ProgressBar
            {
                Location = new Point(20, 476),
                Size = new Size(720, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Style = ProgressBarStyle.Continuous,
                Step = 1,
            };

            // Go button
            _btnGo = new Button
            {
                Text = "▶  Convert",
                Location = new Point(20, 510),
                Size = new Size(170, 44),
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            };
            _btnGo.Click += OnGoClick;

            _statusLabel = new Label
            {
                Text = "Idle",
                Location = new Point(210, 524),
                AutoSize = true,
                Tag = "muted",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            };

            // Compose
            Controls.Add(_bodyPanel);
            Controls.Add(_headerPanel);

            _bodyPanel.Controls.Add(_lblPackages);
            _bodyPanel.Controls.Add(_listPackages);
            _bodyPanel.Controls.Add(_btnAdd);
            _bodyPanel.Controls.Add(_btnScanFolder);
            _bodyPanel.Controls.Add(_btnRemove);
            _bodyPanel.Controls.Add(_btnClear);
            _bodyPanel.Controls.Add(_lblOutput);
            _bodyPanel.Controls.Add(_textOutput);
            _bodyPanel.Controls.Add(_btnBrowseOutput);
            _bodyPanel.Controls.Add(_cbFix);
            _bodyPanel.Controls.Add(_lblIso);
            _bodyPanel.Controls.Add(_progressIso);
            _bodyPanel.Controls.Add(_lblTotal);
            _bodyPanel.Controls.Add(_progressTotal);
            _bodyPanel.Controls.Add(_btnGo);
            _bodyPanel.Controls.Add(_statusLabel);

            ResumeLayout(false);
        }

        private Button MakeActionButton(string text, Point location)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(160, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter,
            };
        }
    }
}
