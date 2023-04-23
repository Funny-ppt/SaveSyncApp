using SaveSyncApp.Properties;
using System.IO;
using System.Text.Json;

namespace SaveSyncApp;

public class FileProfileProvider : IProfileProvider
{
    string _path;

    public FileProfileProvider(string path)
    {
        _path = path;
    }

    // todo: 其实应该在加载时替换ProfileItem的占位符为地址，保存时替换为占位符
    // 不过由于需要改动JsonSerializer设置，有点懒了没弄，暂时搞了个ReplacedSavePath使用

    public bool TryGetProfile(out Profile? profile)
    {
        profile = null;
        try
        {
            using var fs = File.Open(_path, FileMode.Open);
            using var sr = new StreamReader(fs);
            string jsonString = sr.ReadToEnd();
            profile = JsonSerializer.Deserialize<Profile>(jsonString);
            foreach (var item in profile.Items.Values)
            {
                item.IconPath = Path.Combine(Path.GetDirectoryName(_path), "Saves", "SaveSyncCache", $"{item.ProcessName}.ico");
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySaveProfile(Profile profile)
    {
        if (profile == null)
        {
            return false;
        }
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
