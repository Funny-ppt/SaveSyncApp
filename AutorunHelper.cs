using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                registryKey.SetValue(key, Assembly.GetEntryAssembly().Location);
            }
            else
            {
                registryKey.DeleteValue(key);
            }
            return true;
        } catch { }
        return false;
    }
}
