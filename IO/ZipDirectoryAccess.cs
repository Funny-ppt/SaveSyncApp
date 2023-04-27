using SaveSyncApp.IO;
using SaveSyncApp;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System;

namespace SaveSyncApp.IO;

public class ZipDirectoryAccess : IDirectoryAccess
{
    private ZipArchive _zipArchive;
    private string? _zipArchivePath;
    private ZipArchiveEntry? _zipEntry;

    public ZipDirectoryAccess(string zipFilePath)
    {
        _zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update);
        _zipArchivePath = zipFilePath;
    }

    private ZipDirectoryAccess(ZipArchiveEntry zipEntry)
    {
        _zipArchive = zipEntry.Archive;
        _zipEntry = zipEntry;
    }

    public string Name => _zipEntry != null ? _zipEntry.Name : Path.GetFileName(_zipArchivePath);

    public DateTime LastWriteTimeUtc => _zipEntry != null ? _zipEntry.LastWriteTime.UtcDateTime : File.GetLastWriteTimeUtc(_zipArchivePath);

    public CommonFileSystemInfo GetCommonFileSystemInfo() => new(Name, LastWriteTimeUtc);

    public bool Exists(string path)
    {
        return _zipArchive.GetEntry(path) != null;
    }

    public IEnumerable<IDirectoryAccess> GetDirectories()
    {
        var pathPrefix = _zipEntry?.FullName;
        return _zipArchive.Entries
            .Where(entry => entry.FullName.EndsWith("/") && (pathPrefix == null || entry.FullName.StartsWith(pathPrefix)))
            .Select(entry => new ZipDirectoryAccess(entry));
    }

    public IEnumerable<IFileAccess> GetFiles()
    {
        var pathPrefix = _zipEntry?.FullName;
        return _zipArchive.Entries
            .Where(entry => !entry.FullName.EndsWith("/") && (pathPrefix == null || entry.FullName.StartsWith(pathPrefix)))
            .Select(entry => new ZipFileAccess(entry));
    }

    public IDirectoryAccess? GetDirectory(string name)
    {
        var path = _zipEntry != null ? Path.Combine(_zipEntry.FullName, name + "/") : name + "/";
        var entry = _zipArchive.GetEntry(path);
        return entry != null ? new ZipDirectoryAccess(entry) : null;
    }

    public IFileAccess? GetFile(string name)
    {
        var path = _zipEntry != null ? Path.Combine(_zipEntry.FullName, name) : name;
        var entry = _zipArchive.GetEntry(path);
        return entry != null ? new ZipFileAccess(entry) : null;
    }

    public IDirectoryAccess CreateSubdirectory(string name)
    {
        if (_zipEntry != null)
        {
            return new ZipDirectoryAccess(_zipArchive.CreateEntry(Path.Combine(_zipEntry.FullName, name + "/")));
        }
        else
        {
            return new ZipDirectoryAccess(_zipArchive.CreateEntry(name + "/"));
        }
    }

    public void Delete(bool recursive)
    {
        if (_zipEntry != null)
        {
            _zipEntry.Delete();
        }
        else
        {
            File.Delete(_zipArchivePath);
        }
    }

    public void Dispose()
    {
        _zipArchive.Dispose();
    }
}
