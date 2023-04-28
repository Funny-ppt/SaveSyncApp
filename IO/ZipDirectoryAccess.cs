using SaveSyncApp.IO;
using SaveSyncApp;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace SaveSyncApp.IO;

public class ZipDirectoryAccess : IDirectoryAccess, IDisposable
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

    private ZipDirectoryAccess() { }

    public static ZipDirectoryAccess Create(string zipFilePath)
    {
        return new ZipDirectoryAccess()
        {
            _zipArchive = new ZipArchive(File.Create(zipFilePath), ZipArchiveMode.Update),
            _zipArchivePath = zipFilePath
        };
    }

    public string Name => _zipEntry != null ? Regex.Match(_zipEntry.FullName, @"^([^/]+/)*([^/]+)/$").Groups[2].Value : Path.GetFileNameWithoutExtension(_zipArchivePath);

    public DateTime LastWriteTimeUtc => _zipEntry != null ? _zipEntry.LastWriteTime.UtcDateTime : File.GetLastWriteTimeUtc(_zipArchivePath);

    public CommonFileSystemInfo GetCommonFileSystemInfo() => new(Name, LastWriteTimeUtc);

    public bool Exists(string path)
    {
        return _zipArchive.GetEntry(path) != null;
    }

    public IEnumerable<IDirectoryAccess> GetDirectories()
    {
        var pathPrefix = _zipEntry?.FullName ?? string.Empty;
        return _zipArchive.Entries
            .Where(entry => Regex.IsMatch(entry.FullName, $@"^{Regex.Escape(pathPrefix)}[^/]+/$"))
            .Select(entry => new ZipDirectoryAccess(entry));
    }

    public IEnumerable<IFileAccess> GetFiles()
    {
        var pathPrefix = _zipEntry?.FullName ?? string.Empty;
        return _zipArchive.Entries
            .Where(entry => Regex.IsMatch(entry.FullName, $@"^{Regex.Escape(pathPrefix)}[^/]+$"))
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

    public Stream CreateFile(string name)
    {
        var entry = _zipArchive.CreateEntry(
            _zipEntry != null ? Path.Combine(_zipEntry.FullName, name) : name);
        return entry.Open();
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
