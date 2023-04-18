using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaveSyncApp;

public class FileProfileProvider : IProfileProvider
{
    string _path;

    public FileProfileProvider(string path)
    {
        _path = path;
    }

    public bool TryGetProfile(out Profile profile)
    {
        profile = null;
        try
        {
            using var fs = File.Open(_path, FileMode.Open);
            using var sr = new StreamReader(fs);
            string jsonString = sr.ReadToEnd();
            profile = JsonSerializer.Deserialize<Profile>(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySaveProfile(Profile profile)
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var fs = File.Exists(_path) ? File.Open(_path, FileMode.Truncate) : File.Open(_path, FileMode.Create);
            using var sw = new StreamWriter(fs);
            string jsonString = JsonSerializer.Serialize(profile);
            sw.Write(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
