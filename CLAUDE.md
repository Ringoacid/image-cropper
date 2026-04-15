# CLAUDE.md — ImageCropper Codebase Guide

## Project Overview

**ImageCropper** is a Windows desktop application for batch image cropping. Users load images (individually or by folder), draw a crop rectangle interactively, then export all images cropped to that region. Built with WPF and .NET 10, using OpenCV for image processing.

- **Language:** C# 13 / .NET 10.0-windows7.0
- **UI Framework:** WPF (Windows Presentation Foundation)
- **Architecture Pattern:** MVVM (Model-View-ViewModel)
- **Current Version:** 1.1.0.0
- **Documentation language:** Japanese (README, comments, UI strings)

---

## Repository Structure

```
image-cropper/
├── ImageCropper/                  # Main C# project
│   ├── ImageCropper.csproj        # .NET 10 project file (NuGet dependencies here)
│   ├── App.xaml / App.xaml.cs     # Application entry point
│   ├── AssemblyInfo.cs            # Assembly metadata
│   ├── Assets/                    # App icons and default preview image (copied to output)
│   ├── Behaviors/                 # WPF attached behaviors
│   ├── Converters/                # WPF IValueConverter implementations
│   ├── Helpers/                   # Utility classes (settings persistence)
│   ├── Models/                    # Plain data models (settings, metadata)
│   ├── ViewModels/Windows/        # MVVM ViewModels
│   └── Views/
│       ├── UserControls/          # Reusable custom WPF controls
│       └── Windows/               # Window XAML + code-behind
├── Assets/                        # Screenshots for documentation
├── installer/
│   └── ImageCropperSetup.iss      # Inno Setup installer script
├── ImageCropper.sln               # Visual Studio solution
├── README.md                      # End-user documentation (Japanese)
├── HowToInstall.md                # Installation guide (Japanese)
├── VersionHistory.md              # Changelog
└── CLAUDE.md                      # This file
```

---

## Key Dependencies (NuGet)

Defined in `ImageCropper/ImageCropper.csproj`:

| Package | Version | Purpose |
|---|---|---|
| `CommunityToolkit.Mvvm` | 8.4.2 | MVVM base classes (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`) |
| `Microsoft.Xaml.Behaviors.Wpf` | 1.1.142 | WPF behavior support |
| `OpenCvSharp4` | 4.13.0.20260330 | Image processing (C# wrapper for OpenCV) |
| `OpenCvSharp4.runtime.win` | 4.13.0.20260302 | OpenCV native Windows runtime |
| `ReactiveProperty` | 9.8.0 | Reactive property extensions |
| `ReactiveProperty.WPF` | 9.8.0 | WPF-specific reactive bindings |

---

## Architecture: MVVM Pattern

### Models (`ImageCropper/Models/`)

Plain data classes with no UI dependencies:

- **`AppSettings.cs`** — Top-level JSON-serializable settings container. Wraps `OutputSettings` and `UISettings`. Contains backward-compatibility shims for flat-format settings files.
- **`OutputSettings.cs`** — Output extension, folder path, multi-threading toggle, crop-outside-bounds flag.
- **`UISettings.cs`** — `RangeDisplayMode` enum controlling how crop coordinates are shown (pixel/percent, XYWH/XY1XY2).
- **`ImageInformation.cs`** — Record type holding `Width`, `Height`, `Channels` for a loaded image. Uses OpenCV for loading to handle non-ASCII paths.
- **`SelectionItem<T>.cs`** — Generic wrapper pairing a value with a display name, used in ListBox bindings.
- **`ObservableHashSet<T>.cs`** — WPF-compatible observable `HashSet` that raises collection-change notifications.

### ViewModels (`ImageCropper/ViewModels/Windows/`)

All ViewModels inherit `ObservableObject` from `CommunityToolkit.Mvvm`. Properties use `[ObservableProperty]` source generation; commands use `[RelayCommand]`.

- **`MainViewModel.cs`** (1,243 lines) — Core application logic:
  - Image file loading (single file, folder, recursive folder) with async cancellation
  - Crop range management (`RectRange` pixel coordinates normalized across image sizes)
  - Batch image processing pipeline via OpenCV with multi-threading support
  - Settings persistence (load on startup, save on exit) via `SettingsHelper`
  - GitHub Releases API integration for update checking
  - Drag-and-drop handling
- **`SettingsWindowViewModel.cs`** — Settings dialog state
- **`CropRangeManualSettingsWindowViewModel.cs`** — Numeric range entry dialog (all four coordinate/dimension modes)
- **`ProgressViewModel.cs`** — Progress value and cancellation token for async operations
- **`DebugViewModel.cs`** — Debug information display

### Views (`ImageCropper/Views/`)

Each Window has a paired `.xaml` + `.xaml.cs`. Code-behind is minimal — only UI event handling that cannot be done in XAML/behaviors.

**Windows:**
- `MainWindow` — Primary UI: image list (left), preview with crop overlay (center), settings panel (right)
- `SettingsWindow` — Output settings dialog
- `ProgressWindow` — Modal progress indicator during batch operations
- `CropRangeManualSettingsWindow` — Numeric crop range input
- `MessageWindow` — Generic message display
- `DebugWindow` — Debug information

**UserControls:**
- **`EditableImage`** — The most complex control. Renders the preview image with an interactive crop rectangle. Supports:
  - Zoom/pan (mouse wheel + Shift+wheel)
  - Draw/drag the crop rectangle
  - 8 resize handles (4 corners + 4 edge midpoints)
  - Aspect ratio lock on corner handles while Shift is held
  - Real-time coordinate overlay that moves to avoid clipping at image edges
- **`NumberBox`** — Custom numeric-only text input with validation
- **`ToggleSwitch`** — Custom toggle switch control

### Converters (`ImageCropper/Converters/`)

Standard WPF `IValueConverter` implementations:

- `BooleanToVisibilityConverter` — `bool` ↔ `Visibility`
- `InverseBooleanConverter` — Boolean negation
- `EnumToBooleanConverter` — Enum ↔ `bool` (for radio buttons bound to enum properties)
- `EnumToDisplayStringConverter` — `RangeDisplayMode` → localized display string

### Behaviors (`ImageCropper/Behaviors/`)

- `MultiSelectionBehavior` — Enables multi-item selection in ListBox with MVVM binding

### Helpers (`ImageCropper/Helpers/`)

- **`SettingsHelper.cs`** — Reads/writes `settings.json` in the application's base directory using `System.Text.Json` with camelCase naming policy. Used by `MainViewModel` on startup/exit.

---

## Build and Development

### Prerequisites

- Visual Studio 2022 (or later) with `.NET desktop development` workload
- .NET 10 SDK
- Windows OS (WPF is Windows-only)

### Build

Open `ImageCropper.sln` in Visual Studio and build normally (F6 / Ctrl+Shift+B).

```
# From command line:
dotnet build ImageCropper/ImageCropper.csproj
dotnet run --project ImageCropper/ImageCropper.csproj
```

Build output: `ImageCropper/bin/{Debug|Release}/net10.0-windows7.0/`

### Installer

Built with [Inno Setup](https://jrsoftware.org/isinfo.php) using `installer/ImageCropperSetup.iss`. Expects Release build output at `ImageCropper\bin\Release\net10.0-windows7.0\`. Produces `installer/output/ImageCropperSetup_{version}.exe`.

### No Automated Tests

This project has no unit or integration test suite. Testing is manual.

---

## Key Conventions

### C# Style

- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`). Always handle nullability — use `?` types and null checks.
- **Implicit usings enabled** — common namespaces are auto-imported.
- **File-scoped namespaces** — each file uses `namespace Foo.Bar;` (no braces).
- **`[ObservableProperty]`** source generator — backing fields are named with leading lowercase (e.g., `private string myProp;` generates `MyProp` property). Do not write property boilerplate by hand.
- **`[RelayCommand]`** source generator — decorate async methods with `[RelayCommand]` to auto-generate `ICommand` properties.
- **`#region` blocks** used in `MainViewModel.cs` to group logical sections (file/settings I/O, image loading, processing, UI state, etc.).

### Comments

All XML doc comments and inline comments are written in **Japanese**. Continue this convention when adding comments.

### Settings Persistence

- Settings file: `settings.json` in `AppDomain.CurrentDomain.BaseDirectory`
- Loaded in `MainViewModel` constructor; saved on application exit
- `AppSettings` contains backward-compatibility properties (`[JsonIgnore(Condition = WhenWritingDefault)]`) — do not remove these without a migration strategy

### Image Processing

- **Always use OpenCV (`OpenCvSharp`) for image I/O**, not `System.Drawing` or WPF's `BitmapImage`, because OpenCV handles non-ASCII (Japanese) file paths correctly via byte array encoding.
- `ImageInformation.cs` shows the correct pattern: `Cv2.ImDecode(File.ReadAllBytes(path), ...)`.
- Output format is determined by file extension passed to `Cv2.ImEncode`.

### Supported Formats

| Direction | Extensions |
|---|---|
| **Input** | `.jpg`, `.jpeg`, `.jpe`, `.png`, `.bmp`, `.dib`, `.gif`, `.tiff`, `.tif`, `.webp`, `.jp2`, `.pbm`, `.pgm`, `.ppm`, `.sr`, `.ras`, `.exr`, `.hdr` |
| **Output** | `.bmp`, `.dib`, `.jpeg`, `.jpg`, `.jpe`, `.jp2`, `.png`, `.pbm`, `.pgm`, `.ppm`, `.sr`, `.ras`, `.tiff`, `.tif` |

These are defined as `HashSet<string>` constants in `MainViewModel.cs`.

### Crop Coordinate System

- Crop range is stored in **pixel coordinates relative to the original image** in `CropRangePixelCoordinates` (a `RectRange?` in `MainViewModel`).
- The `EditableImage` control works in **display coordinates** (scaled to the control's render size) and converts to/from pixel coordinates via scale factors.
- When the crop rectangle extends outside image bounds and `OutputSettings.IsCropOutside` is `false`, the rectangle is clamped before processing.

### Async Operations

- Long operations (loading images, batch cropping) use `async`/`await` with `CancellationToken`.
- Progress is reported via `ProgressViewModel` passed to a `ProgressWindow`.
- `ConcurrentBag` is used for multi-threaded batch cropping when `IsUseMultiThreading` is enabled.

### Update Checking

- `MainViewModel` calls the GitHub Releases API (`https://api.github.com/repos/Ringoacid/image-cropper/releases/latest`) on startup (or on demand).
- If a newer version is found, it opens the releases page in the default browser. No in-app patching.

---

## Common Tasks for AI Assistants

### Adding a New Setting

1. Add the property to the appropriate model (`OutputSettings.cs` or `UISettings.cs`).
2. If needed, add a backward-compat shim in `AppSettings.cs` with `[JsonIgnore(Condition = WhenWritingDefault)]`.
3. Expose it in the relevant ViewModel with `[ObservableProperty]`.
4. Bind it in the Settings window XAML.
5. No migration needed — missing JSON keys deserialize to default values.

### Adding a New Image Format

1. Add the extension to the appropriate `HashSet<string>` in `MainViewModel.cs`.
2. Verify that OpenCvSharp4 supports it (check OpenCV docs for `Cv2.ImDecode` / `Cv2.ImEncode`).

### Modifying the Crop Rectangle Behavior

The logic lives in `EditableImage.xaml.cs` (mouse event handlers). Key methods handle: `OnMouseLeftButtonDown` (start draw/move/resize), `OnMouseMove` (update rectangle), `OnMouseLeftButtonUp` (finalize). Aspect ratio locking is applied during corner-handle drag when `Shift` is pressed.

### Adding a New Window

1. Create `Views/Windows/MyWindow.xaml` + `MyWindow.xaml.cs`.
2. Create `ViewModels/Windows/MyWindowViewModel.cs` (extend `ObservableObject`).
3. Instantiate and show the window from the relevant parent ViewModel using `new MyWindow { DataContext = new MyWindowViewModel(...) }`.

---

## Version History Summary

| Version | Key Changes |
|---|---|
| 1.1.0.0 | Replaced Velopack with GitHub Releases API for updates; added Shift+drag aspect-ratio lock on corner handles |
| 1.0.0.0 | Initial release |
