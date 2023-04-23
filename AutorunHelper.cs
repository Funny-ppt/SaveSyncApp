using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SaveSyncApp;

public static class AutorunHelper
{
    public static bool TrySetAutorun(string key, bool enable = true)
    {
        try
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (registryKey == null)
            {
                return false;
            }
            if (enable)
            {
                registryKey.SetValue(key, Process.GetCurrentProcess().MainModule.FileName);
            }
            else
            {
                registryKey.DeleteValue(key);
            }
            return true;
        } catch { }
        return false;
    }
    public static bool TryCheckAutorun(string key, out bool enabled)
    {
        enabled = false;
        try
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (registryKey == null)
            {
                return false;
            }
            var value = registryKey.GetValue(key);
            enabled = value != null && (string)value == Process.GetCurrentProcess().MainModule.FileName;
            return true;
        }
        catch { }
        return false;
    }
}
