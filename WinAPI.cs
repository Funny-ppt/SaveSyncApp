using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SaveSyncApp.Win32;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct MONITORINFO
{
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_BASIC_INFORMATION
{
    public IntPtr Reserved1;
    public IntPtr PebBaseAddress;
    public IntPtr Reserved2_0;
    public IntPtr Reserved2_1;
    public IntPtr UniqueProcessId;
    public IntPtr InheritedFromUniqueProcessId;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct SHFILEINFO
{
    public IntPtr hIcon;
    public int iIcon;
    public uint dwAttributes;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szDisplayName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;
}

public enum IMAGELIST_SIZE_FLAG : uint
{
    SHIL_LARGE = 0x0,
    SHIL_SMALL = 0x1,
    SHIL_EXTRALARGE = 0x2,
    SHIL_SYSSMALL = 0x3,
    SHIL_JUMBO = 0x4
}

[StructLayout(LayoutKind.Sequential)]
public struct IMAGELISTDRAWPARAMS
{
    public int cbSize;
    public IntPtr himl;
    public int i;
    public IntPtr hdcDst;
    public int x;
    public int y;
    public int cx;
    public int cy;
    public int xBitmap;
    public int yBitmap;
    public int rgbBk;
    public int rgbFg;
    public int fStyle;
    public int dwRop;
    public int fState;
    public int Frame;
    public int crEffect;
}


[ComImport]
[Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IImageList
{
    public const string IImageList_GUID = "46EB5926-582E-4017-9FDF-E8998DAA0950";

    int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
    int ReplaceIcon(int i, IntPtr hicon, ref int pi);
    int SetOverlayImage(int iImage, int iOverlay);
    int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
    int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
    int Draw(ref IMAGELISTDRAWPARAMS pimldp);
    int Remove(int i);
    int GetIcon(int i, int flags, ref IntPtr picon);
}

public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

internal static class WinAPI
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = false)]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = false)]
    public static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("shell32.dll", SetLastError = true, EntryPoint = "#727")]
    public static extern int SHGetImageList(IMAGELIST_SIZE_FLAG iImageList, ref Guid riid, ref IImageList? ppv);

    [DllImport("Shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);




}
