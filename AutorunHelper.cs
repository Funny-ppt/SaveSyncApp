using Microsoft.Win32;

namespace SaveSyncApp;

public static class AutorunHelper
{
    public static bool TrySetAutorun(string key, bool enable = true, string? arguments = null)
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
                string processPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string commandLine = arguments == null ? processPath : $"\"{processPath}\" {arguments}";
                registryKey.SetValue(key, commandLine);
            }
            else
            {
                registryKey.DeleteValue(key);
            }
            return true;
        }
        catch { }
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
            string processPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var value = registryKey.GetValue(key);
            enabled = value != null && ((string)value).Contains(processPath);
            return true;
        }
        catch { }
        return false;
    }
}
