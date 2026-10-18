using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace God2Iso
{
    public partial class MainForm : Form
    {
        private AppSettings _settings = new();
        private byte[]? _xsfHeader;
        private readonly BackgroundWorker _worker = new();
        private string? _lastBrowsePath;
        private int _currentPackageIndex;
        private int _completedPackages;

        public MainForm()
        {
            _settings = AppSettings.Load();
            ThemePalette.Load(_settings);
            _xsfHeader = LoadEmbeddedXsfHeader();
            InitializeComponent();
            Text = GetAppName();
            Icon = TryLoadIcon();
            AllowDrop = true;
            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;

            _worker.WorkerReportsProgress = true;
            _worker.DoWork += BackgroundWorker_DoWork;
            _worker.ProgressChanged += BackgroundWorker_ProgressChanged;
            _worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            ApplySettingsToUi();
            ThemePalette.Apply(this);
            // Dark title bar needs to fire against a real handle AND after the
            // window has been shown — DWM caches the non-client area at first paint.
            HandleCreated += (_, _) => ThemePalette.ApplyDarkTitleBar(this);
            Shown += (_, _) =>
            {
                ThemePalette.ApplyDarkTitleBar(this);
                // Force a non-client redraw so the title bar flips immediately.
                var r = new Rectangle(0, 0, 1, 1);
                Invalidate(r, true);
            };

            if (_settings.AutoPopulateOnStart &&
                !string.IsNullOrEmpty(_settings.DefaultSourceFolder) &&
                Directory.Exists(_settings.DefaultSourceFolder))
            {
                ScanFolderRecursively(_settings.DefaultSourceFolder);
            }
        }

        private void ApplySettingsToUi()
        {
            if (!string.IsNullOrEmpty(_settings.DefaultOutputFolder) &&
                Directory.Exists(_settings.DefaultOutputFolder))
            {
                _textOutput.Text = _settings.DefaultOutputFolder;
            }
            _cbFix.Checked = _settings.FixCreateIsoHeader;
        }

        private static Icon? TryLoadIcon()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Tribalmarkings-Colorflow-Xbox-360.ico");
                if (File.Exists(path)) return new Icon(path);
            }
            catch { }
            return null;
        }

        private static byte[]? LoadEmbeddedXsfHeader()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("XSFHeader.bin", StringComparison.OrdinalIgnoreCase));
                if (name is null) return null;
                using var s = asm.GetManifestResourceStream(name);
                if (s is null) return null;
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                return ms.ToArray();
            }
            catch { return null; }
        }

        private static string GetAppName()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            if (v is null) return "God2Iso v3";
            return $"God2Iso v{v.Major}.{v.Minor}.{v.Build}";
        }

        // ─────────────────────────────────────────────────────────────
        // UI event handlers
        // ─────────────────────────────────────────────────────────────

        private void OnAddClick(object? sender, EventArgs e)
        {
            using var browser = new OpenFileDialog
            {
                CheckFileExists = true,
                Title = "Select a GoD package header",
                InitialDirectory = _lastBrowsePath ?? _settings.DefaultSourceFolder ?? "",
            };
            if (browser.ShowDialog(this) != DialogResult.OK) return;

            var dataPath = Path.Combine(browser.FileName + ".data", "Data0000");
            if (_listPackages.Items.Contains(browser.FileName))
            {
                Toast("Already in the list.");
                return;
            }
            if (File.Exists(dataPath))
            {
                _listPackages.Items.Add(browser.FileName);
                _lastBrowsePath = Path.GetDirectoryName(browser.FileName);
            }
            else
            {
                MessageBox.Show(this, "Could not find associated data file:\n" + dataPath,
                    "God2Iso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnBrowseOutputClick(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Pick an output folder for the ISO",
                SelectedPath = Directory.Exists(_textOutput.Text) ? _textOutput.Text : "",
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _textOutput.Text = dlg.SelectedPath;
            }
        }

        private void OnScanFolderClick(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = "Pick a folder to scan recursively for GoD packages",
                SelectedPath = _settings.DefaultSourceFolder,
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            int before = _listPackages.Items.Count;
            ScanFolderRecursively(dlg.SelectedPath);
            int added = _listPackages.Items.Count - before;

            if (!_settings.SkipBatchConfirmPrompt)
            {
                MessageBox.Show(this,
                    $"Scan complete. {added} package(s) added to the list.",
                    "God2Iso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Toast($"Scan complete — {added} package(s) added.");
            }
        }

        private void OnRemoveClick(object? sender, EventArgs e)
        {
            int selected = _listPackages.SelectedIndex;
            if (selected < 0) return;
            if (selected < _listPackages.Items.Count - 1) _listPackages.SelectedIndex = selected + 1;
            else if (_listPackages.Items.Count > 1) _listPackages.SelectedIndex = selected - 1;
            _listPackages.Items.RemoveAt(selected);
        }

        private void OnClearClick(object? sender, EventArgs e)
        {
            _listPackages.SelectedIndex = -1;
            _listPackages.Items.Clear();
        }

        private void OnListSelectionChanged(object? sender, EventArgs e)
        {
            _btnRemove.Enabled = _listPackages.SelectedIndex >= 0;
        }

        private void OnGoClick(object? sender, EventArgs e)
        {
            if (_listPackages.Items.Count < 1)
            {
                MessageBox.Show(this, "No GoD packages specified.", "God2Iso",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(_textOutput.Text))
            {
                MessageBox.Show(this, "Pick an output folder first.", "God2Iso",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Directory.Exists(_textOutput.Text))
            {
                MessageBox.Show(this, "Output folder does not exist.", "God2Iso",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_xsfHeader is null)
            {
                MessageBox.Show(this, "Embedded XSF header missing — build issue.",
                    "God2Iso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ── Pre-flight disk space check ──
            long totalSize = 0;
            int totalFiles = 0;
            for (int i = 0; i < _listPackages.Items.Count; i++)
            {
                var path = (string)_listPackages.Items[i]!;
                int count = 0;
                while (true)
                {
                    var part = Path.Combine(path + ".data", "Data" + count.ToString("D4"));
                    if (!File.Exists(part)) break;
                    totalSize += new FileInfo(part).Length;
                    count++;
                }
                totalFiles += count;
            }
            try
            {
                var driveRoot = Path.GetPathRoot(_textOutput.Text);
                if (!string.IsNullOrEmpty(driveRoot))
                {
                    var drive = new DriveInfo(driveRoot);
                    if (drive.IsReady && drive.AvailableFreeSpace < totalSize)
                    {
                        MessageBox.Show(this,
                            $"Not enough free space in the output folder.\nNeed approximately {FormatSize((ulong)totalSize)}.",
                            "God2Iso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch { /* skip disk-space check on failure */ }

            _progressIso.Value = 0;
            _progressTotal.Value = 0;
            _progressTotal.Maximum = Math.Max(1, totalFiles);
            SetBusy(true);
            _currentPackageIndex = 0;
            _completedPackages = 0;
            _worker.RunWorkerAsync();
        }

        private void OnSettingsClick(object? sender, EventArgs e)
        {
            using var dlg = new SettingsDialog(_settings);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _settings = dlg.ResultSettings;
                _settings.Save();
                ThemePalette.Load(_settings);
                ThemePalette.Apply(this);
                ApplySettingsToUi();
            }
        }

        private void OnFixCheckedChanged(object? sender, EventArgs e)
        {
            _settings.FixCreateIsoHeader = _cbFix.Checked;
            _settings.Save();
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths) return;
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    ScanFolderRecursively(path);
                }
                else if (File.Exists(path))
                {
                    var dataPath = Path.Combine(path + ".data", "Data0000");
                    if (File.Exists(dataPath) && !_listPackages.Items.Contains(path))
                    {
                        _listPackages.Items.Add(path);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Background conversion — algorithm from God2Iso v2, preserved verbatim
        // ─────────────────────────────────────────────────────────────

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender!;
            var packages = new List<string>();
            Invoke(() =>
            {
                foreach (var o in _listPackages.Items) packages.Add((string)o!);
            });

            for (int i = 0; i < packages.Count; i++)
            {
                _currentPackageIndex = i;
                FileStream? iso = null;
                FileStream? data = null;
                try
                {
                    string baseName = Path.GetFileName(packages[i]);
                    string dataDir = packages[i] + ".data";

                    int totalFiles;
                    for (totalFiles = 0; ; totalFiles++)
                    {
                        var p = Path.Combine(dataDir, "Data" + totalFiles.ToString("D4"));
                        if (!File.Exists(p)) break;
                    }
                    if (totalFiles < 1) continue;

                    bool hasXsf = HasXsfHeader(Path.Combine(dataDir, "Data0000"));
                    string outputFolder = "";
                    Invoke(() => outputFolder = _textOutput.Text);
                    iso = new FileStream(Path.Combine(outputFolder, baseName + ".iso"),
                        FileMode.Create, FileAccess.ReadWrite);

                    if (!hasXsf) iso.Write(_xsfHeader!, 0, _xsfHeader!.Length);

                    for (int fileNum = 0; fileNum < totalFiles; fileNum++)
                    {
                        var path = Path.Combine(dataDir, "Data" + fileNum.ToString("D4"));
                        data = new FileStream(path, FileMode.Open, FileAccess.Read);
                        data.Position = 0x2000;
                        int len;
                        while (true)
                        {
                            byte[] buff = new byte[0xcc000];
                            len = data.Read(buff, 0, buff.Length);
                            iso.Write(buff, 0, len);
                            if (len < 0xcc000) break;
                            len = data.Read(buff, 0, 0x1000);
                            if (len < 0x1000) break;
                        }
                        data.Close();
                        data = null;
                        worker.ReportProgress(0, new ProgressState(fileNum + 1, totalFiles));
                    }

                    if (!hasXsf)
                    {
                        FixXfsHeader(iso);
                        FixSectorOffsets(iso, packages[i]);
                    }
                    bool fixHdr = false;
                    Invoke(() => fixHdr = _cbFix.Checked);
                    if (fixHdr) FixCreateIsoGoodHeader(iso);

                    _completedPackages++;
                }
                catch (Exception ex)
                {
                    Invoke(() => MessageBox.Show(this, ex.Message, "God2Iso",
                        MessageBoxButtons.OK, MessageBoxIcon.Error));
                }
                finally
                {
                    try { iso?.Close(); } catch { }
                    try { data?.Close(); } catch { }
                }
            }
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is not ProgressState st) return;
            _progressIso.Maximum = st.Files;
            _progressIso.Value = st.File;
            if (_progressTotal.Value < _progressTotal.Maximum) _progressTotal.PerformStep();
            TaskbarProgress.SetProgress(Handle, _progressTotal.Value, _progressTotal.Maximum);
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            SetBusy(false);
            TaskbarProgress.ClearProgress(Handle);
            _progressIso.Value = _progressIso.Maximum;
            _progressTotal.Value = _progressTotal.Maximum;

            int done = _completedPackages;
            int total = _listPackages.Items.Count;

            if (_settings.ShowCompletionDialog)
            {
                MessageBox.Show(this,
                    done == total
                        ? $"All done — {done} ISO{(done == 1 ? "" : "s")} written to:\n{_textOutput.Text}"
                        : $"Finished with {done}/{total} ISO(s) written. Check the error popups for details.",
                    "God2Iso — Conversion Complete",
                    MessageBoxButtons.OK,
                    done == total ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            else
            {
                Toast(done == total
                    ? $"All done — {done} ISO(s) written."
                    : $"Finished with {done}/{total} ISO(s). Some failed.");
            }
        }

        private void SetBusy(bool busy)
        {
            _btnAdd.Enabled = !busy;
            _btnGo.Enabled = !busy;
            _btnBrowseOutput.Enabled = !busy;
            _btnClear.Enabled = !busy;
            _btnScanFolder.Enabled = !busy;
            _btnSettings.Enabled = !busy;
            _listPackages.Enabled = !busy;
            _textOutput.Enabled = !busy;
            _cbFix.Enabled = !busy;
            _statusLabel.Text = busy ? "Converting…" : "Idle";
            _statusLabel.ForeColor = busy ? ThemePalette.Accent : ThemePalette.MutedText;
        }

        // ─────────────────────────────────────────────────────────────
        // Scan folder recursively (fixed: no longer renames the user's files!
        // The original code auto-renamed detected GoD files, which was
        // destructive. We now just list what we find.)
        // ─────────────────────────────────────────────────────────────

        private void ScanFolderRecursively(string folderPath)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folderPath))
                {
                    if (_listPackages.Items.Contains(file)) continue;
                    var dataPath = Path.Combine(file + ".data", "Data0000");
                    if (File.Exists(dataPath))
                    {
                        _listPackages.Items.Add(file);
                    }
                }
                foreach (var sub in Directory.EnumerateDirectories(folderPath))
                {
                    ScanFolderRecursively(sub);
                }
            }
            catch (UnauthorizedAccessException) { /* ignore locked folders */ }
            catch (DirectoryNotFoundException) { }
        }

        // ─────────────────────────────────────────────────────────────
        // Original algorithm bits — unchanged
        // ─────────────────────────────────────────────────────────────

        private static bool HasXsfHeader(string file)
        {
            byte[] buff = new byte[3];
            using var data = new FileStream(file, FileMode.Open, FileAccess.Read);
            data.Position = 0x2000;
            data.Read(buff, 0, buff.Length);
            return buff[0] == 0x58 && buff[1] == 0x53 && buff[2] == 0x46;
        }

        private void FixSectorOffsets(FileStream iso, string godPath)
        {
            int sector, size, offset;
            byte[] buffer;
            Queue<DirEntry> directories = new();

            buffer = File.ReadAllBytes(godPath);
            if ((buffer[0x391] & 0x40) != 0x40) return;
            offset = BitConverter.ToInt32(buffer, 0x395);
            if (offset == 0) return;
            offset *= 2;
            offset -= 34;

            buffer = new byte[4];
            iso.Position = 0x10014;
            iso.Read(buffer, 0, 4);
            sector = BitConverter.ToInt32(buffer, 0);
            if (sector > 0)
            {
                sector -= offset;
                byte[] corrected = BitConverter.GetBytes(sector);
                iso.Position -= 4;
                iso.Write(corrected, 0, 4);
                iso.Read(buffer, 0, 4);
                size = BitConverter.ToInt32(buffer, 0);
                directories.Enqueue(new DirEntry(sector, size));
            }

            while (directories.Count > 0)
            {
                var dirEntry = directories.Dequeue();
                iso.Position = dirEntry.StartPos();
                while ((iso.Position + 4) < dirEntry.EndPos())
                {
                    if ((iso.Position + 4) / 2048L > iso.Position / 2048L)
                        iso.Position += 2048L - (iso.Position % 2048L);

                    iso.Read(buffer, 0, 4);
                    if (buffer[0] == 0xff && buffer[1] == 0xff && buffer[2] == 0xff && buffer[3] == 0xff)
                    {
                        if (dirEntry.EndPos() - iso.Position > 2048)
                        {
                            iso.Position += 2048L - (iso.Position % 2048L);
                            continue;
                        }
                        break;
                    }

                    iso.Read(buffer, 0, 4);
                    sector = BitConverter.ToInt32(buffer, 0);
                    if (sector > 0)
                    {
                        sector -= offset;
                        byte[] corrected = BitConverter.GetBytes(sector);
                        iso.Position -= 4;
                        iso.Write(corrected, 0, 4);
                    }

                    iso.Read(buffer, 0, 4);
                    size = BitConverter.ToInt32(buffer, 0);
                    iso.Read(buffer, 0, 1);

                    if ((buffer[0] & 0x10) == 0x10) directories.Enqueue(new DirEntry(sector, size));
                    iso.Read(buffer, 0, 1);
                    iso.Position += buffer[0];
                    if ((14 + buffer[0]) % 4 > 0) iso.Position += 4 - ((14 + buffer[0]) % 4);
                }
            }
        }

        private void FixCreateIsoGoodHeader(FileStream iso)
        {
            byte[] bytes = new byte[8];
            iso.Position = 8;
            iso.Read(bytes, 0, 8);
            if (BitConverter.ToInt64(bytes, 0) == 2587648L)
            {
                iso.Position = 0;
                iso.Write(_xsfHeader!, 0, _xsfHeader!.Length);
                FixXfsHeader(iso);
            }
        }

        private void FixXfsHeader(FileStream iso)
        {
            byte[] bytes;
            iso.Position = 8;
            bytes = BitConverter.GetBytes(iso.Length - 0x400);
            iso.Write(bytes, 0, bytes.Length);

            iso.Position = 0x8050;
            bytes = BitConverter.GetBytes((uint)(iso.Length / 2048));
            iso.Write(bytes, 0, bytes.Length);
            for (int i = bytes.Length - 1; i >= 0; i--) iso.WriteByte(bytes[i]);

            iso.Position = 0x7a69;
            bytes = Encoding.ASCII.GetBytes(GetAppName());
            iso.Write(bytes, 0, bytes.Length);
        }

        // ─────────────────────────────────────────────────────────────
        // Toast — tiny bottom-right popup, no interrupt
        // ─────────────────────────────────────────────────────────────
        private void Toast(string message)
        {
            _statusLabel.Text = message;
            _statusLabel.ForeColor = ThemePalette.Accent;
            var t = new System.Windows.Forms.Timer { Interval = 3500 };
            t.Tick += (_, _) =>
            {
                _statusLabel.Text = "Idle";
                _statusLabel.ForeColor = ThemePalette.MutedText;
                t.Stop();
                t.Dispose();
            };
            t.Start();
        }

        private static string FormatSize(ulong byteCount)
        {
            if (byteCount >= 1073741824) return $"{byteCount / 1073741824.0:0.##} GB";
            if (byteCount >= 1048576) return $"{byteCount / 1048576.0:0.##} MB";
            if (byteCount >= 1024) return $"{byteCount / 1024.0:0.##} KB";
            return $"{byteCount} B";
        }

        private sealed record ProgressState(int File, int Files);

        private sealed class DirEntry
        {
            public int Sector;
            public int Length;
            public DirEntry(int sector, int length) { Sector = sector; Length = length; }
            public long StartPos() => Sector * 2048L;
            public long EndPos() => (Sector * 2048L) + Length;
        }
    }
}
