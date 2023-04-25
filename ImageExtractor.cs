using SaveSyncApp.Win32;
using static SaveSyncApp.Win32.WinAPI;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SaveSyncApp;

public static class ImageExtractor
{
    public const uint SHGFI_SYSICONINDEX = 0x4000;
    public const uint SHGFI_ICON = 0x100;
    public const uint SHGFI_LARGEICON = 0x0;
    public const uint SHGFI_SMALLICON = 0x1;
    public const uint SHGFI_OPENICON = 0x2;

    public const int ILD_TRANSPARENT = 0x00000001;
    public const int ILD_IMAGE = 0x00000020;

    public static int GetIconIndex(string fileName)
    {
        var info = new SHFILEINFO();
        var iconIntPtr = SHGetFileInfo(fileName, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI_SYSICONINDEX | SHGFI_OPENICON);
        if (iconIntPtr == IntPtr.Zero)
            return -1;
        return info.iIcon;
    }
    
    public static Icon? GetIcon(int iIcon, IMAGELIST_SIZE_FLAG flag)
    {
        if (iIcon == -1) return null;

        var list = default(IImageList?);
        var guid = new Guid(IImageList.IImageList_GUID);
        var hResult = SHGetImageList(flag, ref guid, ref list);
        if (hResult == 0 && list != null)
        {
            var hIcon = IntPtr.Zero;
            list.GetIcon(iIcon, ILD_TRANSPARENT | ILD_IMAGE, ref hIcon);
            return hIcon == IntPtr.Zero ? null : Icon.FromHandle(hIcon);
        }
        return null;
    }

    public static void SaveProgramIcon(string filePath, string iconPath)
    {
        var iconIndex = GetIconIndex(filePath);
        using var icon = GetIcon(iconIndex, IMAGELIST_SIZE_FLAG.SHIL_JUMBO)
            ?? GetIcon(iconIndex, IMAGELIST_SIZE_FLAG.SHIL_SMALL)
            ?? throw new InvalidOperationException("fail to extract icon");
        using var bitmap = icon.ToBitmap();
        bitmap.Save(iconPath);
    }
    public static bool TrySaveProgramIcon(string filePath, string iconPath)
    {
        try
        {
            SaveProgramIcon(filePath, iconPath);
            return true;
        }
        catch { return false; }
    }
}
