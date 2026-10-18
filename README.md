# God2Iso v3

**Convert Xbox 360 Games on Demand packages into bootable ISO images.**

A modern rewrite of the classic GoD → ISO utility. .NET 8, dark Fluent-inspired
UI, drag-and-drop support, persistent settings, and full user customization.

![version](https://img.shields.io/badge/version-3.0.0-blue) ![license](https://img.shields.io/badge/license-MIT-green) ![platform](https://img.shields.io/badge/windows-10%20%7C%2011-informational)

## What's new in v3

- **Dark Fluent-inspired UI** with per-element theme customization (background,
  surface, accent, text, muted, border — all configurable via color picker or hex)
- **Windows 11 dark title bar** via DWMWA_USE_IMMERSIVE_DARK_MODE
- **Persistent settings** stored at `%APPDATA%\God2Iso\settings.json`
- **Default output folder** — no more hard-coded `D:\`
- **Default source folder with auto-populate on launch** — list fills itself
- **Drag-and-drop** files or folders onto the window
- **Completion notification** — popup or toast (your choice)
- **Settings dialog** with live preview
- **.NET 8 single-file self-contained executable** — no runtime install needed
- Dead `WindowsAPICodePack` dependency replaced with native COM `ITaskbarList3`
- Non-destructive folder scan — never renames your files

## Download

Grab the latest build from the [Releases](https://github.com/Can0nical/God2Iso-v2/releases) tab.

## Usage

1. Launch `God2Iso.exe`
2. Open **Settings** (gear icon, top right) and set your default output + source
   folders if you want. Toggle **Auto-populate on launch** if you'd like the
   list to fill itself every time.
3. Add packages:
   - Click **+ Add file** and pick a GoD header file, OR
   - Click **🔎 Scan folder…** to recursively find every GoD package, OR
   - **Drag & drop** files or folders onto the window
4. Pick your output folder (persisted from settings if set)
5. Hit **▶ Convert**

Progress for both the current ISO and the batch total is shown; Windows taskbar
also reflects progress. When finished, a completion dialog or toast tells you
how many ISOs were written and where.

## Customization

Everything requested in [issue #1](https://github.com/Can0nical/God2Iso-v2/issues/1) is now in
**Settings**:

| Setting | Default | Description |
|---------|---------|-------------|
| Default output folder | *(unset)* | Pre-fills the Output field on launch |
| Default source folder | *(unset)* | Used by auto-populate + Scan dialog |
| Auto-populate on launch | off | Scans the source folder on startup |
| Skip "Scan complete" confirmation | off | Shows a toast instead of a popup |
| Show popup on conversion complete | on | Uncheck for toast-only |
| Background / Surface / Accent / Text / Muted / Border colors | dark palette | Click any swatch to pick, or type the hex |

## Build from source

Requires the .NET 8 SDK.

```powershell
dotnet build -c Release
dotnet run --project God2Iso.csproj
```

Publish a single-file self-contained exe:

```powershell
dotnet publish God2Iso.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

## Credits

- Original **GoD2ISO** author for the conversion algorithm
- **God2Iso v2** fork that added batch input and name preservation
- v3 modernization by Can0nical

## Legal

God2Iso neither distributes nor downloads copyrighted content. It operates only
on dumps the user has lawfully produced from their own media. Released under
the MIT License (see `LICENSE.md`).
