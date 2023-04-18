using SaveSyncApp.Win32;
using static SaveSyncApp.Win32.WinAPI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SaveSyncApp;

public static class ProcessHelper
{
    public static string GetProcessFilePath(Process process)
    {
        var filePath = new StringBuilder(1024);
        int length = filePath.Capacity;

        if (QueryFullProcessImageName(process.Handle, 0, filePath, ref length))
        {
            return filePath.ToString();
        }
        else
        {
            throw new System.ComponentModel.Win32Exception();
        }
    }

    public static bool IsProcessLaunchedBySteam(Process process)
    {
        try
        {
            var parentProcess = GetParentProcess(process);
            return parentProcess != null && parentProcess.ProcessName == "steam";
        }
        catch
        {
            return false;
        }
    }

    public static Process GetParentProcess(Process process)
    {
        var pbi = new PROCESS_BASIC_INFORMATION();
        int status = NtQueryInformationProcess(process.Handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);

        if (status != 0)
        {
            throw new InvalidOperationException("获取进程信息失败");
        }

        try
        {
            return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
