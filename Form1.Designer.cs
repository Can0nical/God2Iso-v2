#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace God2Iso
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer? components = null;

        private TableLayoutPanel _root = null!;
        private Panel _headerPanel = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private Button _btnSettings = null!;

        private Panel _packagesCard = null!;
        private Label _lblPackages = null!;
        private ListBox _listPackages = null!;
        private FlowLayoutPanel _packageButtons = null!;
        private Button _btnAdd = null!;
        private Button _btnScanFolder = null!;
        private Button _btnRemove = null!;
        private Button _btnClear = null!;

        private Panel _outputCard = null!;
        private Label _lblOutput = null!;
        private TextBox _textOutput = null!;
        private Button _btnBrowseOutput = null!;
        private CheckBox _cbFix = null!;

        private Panel _progressCard = null!;
        private Label _lblIso = null!;
        private ThemedProgressBar _progressIso = null!;
        private Label _lblTotal = null!;
        private ThemedProgressBar _progressTotal = null!;

        private Panel _footerPanel = null!;
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

            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(820, 680);
            MinimumSize = new Size(720, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "God2Iso";

            // ─── Root: 5-row stack (header / packages / output / progress / footer) ───
            _root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(0),
            };
            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));   // header
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // packages (grows)
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110f));  // output
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110f));  // progress
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));   // footer

            // ─── Header ───
            _headerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 14, 20, 14) };
            _headerPanel.Tag = "header";
            _titleLabel = new Label
            {
                Text = "GoD → ISO",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 15f, FontStyle.Bold),
                Location = new Point(24, 12),
            };
            _subtitleLabel = new Label
            {
                Text = "Convert Xbox 360 Games on Demand packages into bootable ISO images.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                Tag = "muted",
                Location = new Point(26, 42),
            };
            _btnSettings = new Button
            {
                Text = "⚙  Settings",
                Size = new Size(118, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
            };
            _btnSettings.Location = new Point(_headerPanel.Width - 142, 20);
            _headerPanel.Resize += (_, _) =>
            {
                _btnSettings.Location = new Point(_headerPanel.ClientSize.Width - _btnSettings.Width - 20, 20);
            };
            _btnSettings.Click += OnSettingsClick;
            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_subtitleLabel);
            _headerPanel.Controls.Add(_btnSettings);

            // ─── Packages card (list + 4-button row) ───
            _packagesCard = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 14), Tag = "card" };
            _lblPackages = new Label
            {
                Text = "Packages",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _packageButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0),
            };
            _btnAdd = MakeButton("＋  Add file");
            _btnAdd.Click += OnAddClick;
            _btnScanFolder = MakeButton("🔎  Scan folder…");
            _btnScanFolder.Click += OnScanFolderClick;
            _btnRemove = MakeButton("－  Remove");
            _btnRemove.Enabled = false;
            _btnRemove.Click += OnRemoveClick;
            _btnClear = MakeButton("Clear all");
            _btnClear.Click += OnClearClick;
            _packageButtons.Controls.Add(_btnAdd);
            _packageButtons.Controls.Add(_btnScanFolder);
            _packageButtons.Controls.Add(_btnRemove);
            _packageButtons.Controls.Add(_btnClear);

            _listPackages = new ListBox
            {
                Dock = DockStyle.Fill,
                HorizontalScrollbar = true,
                IntegralHeight = false,
                SelectionMode = SelectionMode.One,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
            };
            _listPackages.SelectedIndexChanged += OnListSelectionChanged;
            _packagesCard.Controls.Add(_listPackages);
            _packagesCard.Controls.Add(_packageButtons);
            _packagesCard.Controls.Add(_lblPackages);

            // ─── Output card ───
            _outputCard = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 10, 20, 10), Tag = "card" };
            _lblOutput = new Label
            {
                Text = "Output folder",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            var outRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
            };
            outRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            outRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            _textOutput = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                Margin = new Padding(0, 4, 10, 0),
            };
            _btnBrowseOutput = MakeButton("Browse…");
            _btnBrowseOutput.Dock = DockStyle.Fill;
            _btnBrowseOutput.Margin = new Padding(0, 2, 0, 2);
            _btnBrowseOutput.Click += OnBrowseOutputClick;
            outRow.Controls.Add(_textOutput, 0, 0);
            outRow.Controls.Add(_btnBrowseOutput, 1, 0);

            _cbFix = new CheckBox
            {
                Text = "Fix / rebuild Create-ISO Good headers",
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 8, 0, 0),
            };
            _cbFix.CheckedChanged += OnFixCheckedChanged;

            _outputCard.Controls.Add(_cbFix);
            _outputCard.Controls.Add(outRow);
            _outputCard.Controls.Add(_lblOutput);

            // ─── Progress card ───
            _progressCard = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 10, 20, 14), Tag = "card" };
            _lblIso = new Label { Text = "Current ISO", Dock = DockStyle.Top, Height = 20, Tag = "muted", Font = new Font("Segoe UI", 9f) };
            _progressIso = new ThemedProgressBar { Dock = DockStyle.Top, Height = 10, Margin = new Padding(0, 2, 0, 10) };
            _lblTotal = new Label { Text = "Total", Dock = DockStyle.Top, Height = 20, Margin = new Padding(0, 10, 0, 0), Tag = "muted", Font = new Font("Segoe UI", 9f) };
            _progressTotal = new ThemedProgressBar { Dock = DockStyle.Top, Height = 10, Margin = new Padding(0, 2, 0, 0), Step = 1 };
            _progressCard.Controls.Add(_progressTotal);
            _progressCard.Controls.Add(_lblTotal);
            _progressCard.Controls.Add(_progressIso);
            _progressCard.Controls.Add(_lblIso);

            // ─── Footer ───
            _footerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 14), Tag = "surface" };
            _btnGo = new Button
            {
                Text = "▶  Convert",
                Size = new Size(190, 44),
                Dock = DockStyle.Left,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Tag = "primary",
            };
            _btnGo.Click += OnGoClick;
            _statusLabel = new Label
            {
                Text = "Idle",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Tag = "muted",
                Padding = new Padding(0, 0, 12, 0),
            };
            _footerPanel.Controls.Add(_statusLabel);
            _footerPanel.Controls.Add(_btnGo);

            // Assemble
            _root.Controls.Add(_headerPanel, 0, 0);
            _root.Controls.Add(_packagesCard, 0, 1);
            _root.Controls.Add(_outputCard, 0, 2);
            _root.Controls.Add(_progressCard, 0, 3);
            _root.Controls.Add(_footerPanel, 0, 4);
            Controls.Add(_root);

            ResumeLayout(false);
        }

        private static Button MakeButton(string text)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 34),
                Margin = new Padding(0, 0, 8, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
            };
        }
    }
}
