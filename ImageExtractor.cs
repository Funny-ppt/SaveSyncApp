using SaveSyncApp.Win32;
using static SaveSyncApp.Win32.WinAPI;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SaveSyncApp;

public static class ImageExtractor
{
    public const uint SHGFI_ICON = 0x100;
    public const uint SHGFI_LARGEICON = 0x0;
    public const uint SHGFI_SMALLICON = 0x1;

    public static void SaveProgramIcon(string programPath, string iconPath)
    {
        var shinfo = new SHFILEINFO();
        IntPtr hImgSmall = SHGetFileInfo(programPath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);

        var icon = Icon.FromHandle(shinfo.hIcon);

        using Bitmap bitmap = icon.ToBitmap();
        bitmap.Save(iconPath);
    }
}
