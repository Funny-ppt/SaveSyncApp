using SaveSyncApp.IO;

namespace SaveSyncApp;

public interface IPathToDirectoryAccess
{
    IDirectoryAccess FromPath(string path);
}
