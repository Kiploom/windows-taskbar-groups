# Taskbar Groups

A Windows 11 taskbar app that lets you create groups of apps and launch them from a flyout above the system tray.

## Features

- **Group apps** – Create custom groups (e.g., "Dev Tools", "Games", "Productivity")
- **System tray icons** – Each group gets its own icon in the system tray
- **Hover flyout** – Hover over a group icon to see a popup with all apps in that group
- **Click to launch** – Click an app icon to start it
- **Settings UI** – Create, rename, delete groups; add apps via file picker or drag-and-drop

## Requirements

- **Windows 11** (or Windows 10 22H2+)
- **.NET 8 SDK**
- **Visual Studio 2022** with the "Windows application development" workload (recommended)

## Setup

### 1. Install .NET 8 SDK

Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Install Visual Studio 2022 (recommended)

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community or higher)
2. In the installer, select the **"Windows application development"** workload
3. This installs WinUI 3, Windows App SDK, and MSIX packaging tools

### 3. Build and run

**Option A: Visual Studio**

1. Open `TaskbarGroups.sln` or `TaskbarGroups/TaskbarGroups.csproj` in Visual Studio
2. Set the build configuration to **x64** or **x86**
3. Press **F5** to build and run

**Option B: Command line**

```powershell
cd TaskbarGroups
dotnet build -c Release
dotnet run
```

For MSIX packaging (Store deployment):

```powershell
dotnet publish -c Release -p:PublishProfile=win-x64
```

The packaged app will be in `bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\`.

## Usage

1. **First run** – The app starts minimized to the system tray. Right-click the "Taskbar Groups" icon and choose **Settings**.
2. **Create a group** – Click **+ New Group**, then rename it.
3. **Add apps** – Click **+ Add App** to browse for `.exe` files, or drag-and-drop executables onto the app list.
4. **Use the flyout** – Hover over a group’s tray icon to open the flyout, then click an app to launch it.

## Configuration

Group data is stored in:

```
%AppData%\TaskbarGroups\groups.json
```

You can edit this file to back up or restore your groups.

## Project structure

```
TaskbarGroups/
├── App.xaml / App.xaml.cs     # Entry point, tray icon setup
├── Program.cs                 # Application entry
├── Models/
│   ├── AppGroup.cs            # Group model (name, icon, apps)
│   └── AppEntry.cs            # App model (name, path)
├── Services/
│   ├── GroupStorageService.cs # JSON persistence
│   ├── IconExtractorService.cs# Shell32 icon extraction
│   └── TaskbarHelper.cs       # Taskbar position (Win32)
├── Windows/
│   ├── FlyoutWindow.xaml      # Hover popup with app icons
│   └── SettingsWindow.xaml   # Group management UI
└── Assets/                    # Icons for tray and packaging
```

## Tech stack

- **C# / .NET 8**
- **WinUI 3** (Windows App SDK 1.5+)
- **H.NotifyIcon.WinUI** – System tray icons
- **System.Text.Json** – Config storage

## License

MIT
