using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SaveSyncApp.Win32;
using static SaveSyncApp.Win32.WinAPI;

namespace SaveSyncApp;

public static class FullScreenDetect
{
    public static bool IsFullScreen(Process process) => IsFullScreen(process.MainWindowHandle);

    public static bool IsFullScreen(IntPtr hWnd)
    {
        GetWindowRect(hWnd, out RECT windowRect);

        // Get the screen bounds of the monitor that the window is on
        var screenRect = new RECT();
        bool screenEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            var monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
            GetMonitorInfo(hMonitor, ref monitorInfo);

            // Check if the window is on the current monitor
            if (windowRect.Left >= monitorInfo.rcMonitor.Left &&
                windowRect.Top >= monitorInfo.rcMonitor.Top &&
                windowRect.Right <= monitorInfo.rcMonitor.Right &&
                windowRect.Bottom <= monitorInfo.rcMonitor.Bottom)
            {
                screenRect = monitorInfo.rcMonitor;
                return false;
            }

            return true;
        }

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, screenEnum, IntPtr.Zero);

        if (windowRect.Left == screenRect.Left &&
            windowRect.Top == screenRect.Top &&
            windowRect.Right == screenRect.Right &&
            windowRect.Bottom == screenRect.Bottom)
        {
            return true;
        }

        return false;
    }
}
