using System.Runtime.InteropServices;

namespace TaskbarGroups.Services;

/// <summary>
/// Win32 helpers for taskbar position (ABM_GETTASKBARPOS).
/// </summary>
public static class TaskbarHelper
{
    private const int ABM_GETTASKBARPOS = 5;

    [DllImport("shell32.dll")]
    private static extern int SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    public static (int Left, int Top, int Right, int Bottom) GetTaskbarRect()
    {
        var abd = new APPBARDATA
        {
            cbSize = Marshal.SizeOf<APPBARDATA>()
        };

        _ = SHAppBarMessage(ABM_GETTASKBARPOS, ref abd);
        return (abd.rc.Left, abd.rc.Top, abd.rc.Right, abd.rc.Bottom);
    }

    public static bool IsTaskbarOnBottom()
    {
        var abd = new APPBARDATA { cbSize = Marshal.SizeOf<APPBARDATA>() };
        _ = SHAppBarMessage(ABM_GETTASKBARPOS, ref abd);
        return abd.uEdge == 2; // ABE_BOTTOM = 2
    }
}
