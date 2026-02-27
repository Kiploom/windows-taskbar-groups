using System.Runtime.InteropServices;

namespace TaskbarGroups.Services;

/// <summary>
/// Extracts icons from executables using Shell32.
/// </summary>
public static class IconExtractorService
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    /// <summary>
    /// Extracts the first icon from an executable and returns its path as a bitmap source path.
    /// For WinUI we need to convert to a format we can use. We'll save to temp and return path.
    /// </summary>
    public static string? ExtractIconToFile(string exePath, int iconIndex = 0)
    {
        if (!File.Exists(exePath))
            return null;

        var hIcon = ExtractIcon(IntPtr.Zero, exePath, iconIndex);
        if (hIcon == IntPtr.Zero)
            return null;

        try
        {
            using var icon = Icon.FromHandle(hIcon);
            var tempDir = Path.Combine(Path.GetTempPath(), "TaskbarGroups", "icons");
            Directory.CreateDirectory(tempDir);
            var safeName = Path.GetFileName(exePath).Replace(".", "_") + "_" + iconIndex + ".ico";
            var tempPath = Path.Combine(tempDir, safeName);

            using (var fs = new FileStream(tempPath, FileMode.Create))
            {
                icon.Save(fs);
            }

            return tempPath;
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    /// <summary>
    /// Gets the path to use for displaying an app icon. Uses extracted icon if available.
    /// </summary>
    public static string? GetIconPathForApp(string exePath)
    {
        return ExtractIconToFile(exePath);
    }
}
