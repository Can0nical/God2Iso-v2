using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace God2Iso
{
    /// <summary>
    /// Persistent app settings stored at %APPDATA%\God2Iso\settings.json.
    /// Everything the user picks (output folder, default source, UI colors,
    /// toggles) survives across runs.
    /// </summary>
    public sealed class AppSettings
    {
        public string DefaultOutputFolder { get; set; } = "";
        public string DefaultSourceFolder { get; set; } = "";
        public bool AutoPopulateOnStart { get; set; } = false;
        public bool FixCreateIsoHeader { get; set; } = false;
        public bool SkipBatchConfirmPrompt { get; set; } = false;
        public bool ShowCompletionDialog { get; set; } = true;

        // Theme — hex strings, applied by ThemePalette.
        public string BackgroundColorHex { get; set; } = "#0F1115";
        public string SurfaceColorHex { get; set; } = "#171A21";
        public string AccentColorHex { get; set; } = "#4F8CFF";
        public string TextColorHex { get; set; } = "#F2F3F5";
        public string MutedTextColorHex { get; set; } = "#8A91A3";
        public string BorderColorHex { get; set; } = "#242938";

        [JsonIgnore]
        public static string ConfigDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "God2Iso");

        [JsonIgnore]
        public static string ConfigPath => Path.Combine(ConfigDir, "settings.json");

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true,
        };

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return new AppSettings();
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, _opts));
            }
            catch
            {
                // Settings persistence failure shouldn't kill the app.
            }
        }
    }
}
