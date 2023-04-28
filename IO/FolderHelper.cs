using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaveSyncApp.IO;

internal static class FolderHelper
{
    static void CopyOrOverwriteFolderOnBackground(BackgroundWorker worker, IDirectoryAccess sourceFolder, IDirectoryAccess destFolder, Action<OverwritePreview>? onOverwrite = null)
    {
        worker.WorkerReportsProgress = true;
        worker.DoWork += (sender, e) =>
        {
            CopyOrOverwriteFolderImpl(sourceFolder, destFolder, onOverwrite, (curr, total) => worker.ReportProgress(curr * 100 / total));
        };
    }

    static void OverwriteDefaultHandler(OverwritePreview e)
    {
        if (e.SourceInfo == null)
        {
            var message = $"将删除该文文件(夹):\n" +
                          $"{e.DestinationInfo.Name}";

            var result = CustomDialog.ShowDialog("SaveSync", message, new[] { ("删除所有", "OverwriteAll"), ("确认", "Overwrite"), ("跳过", "Skip"), ("取消", "Cancel") }, "Skip");
            e.Action = Enum.Parse<FileSystemAction>(result["action"]);
            return;
        }

        var sourceLWT = e.SourceInfo.LastWriteTimeUtc;
        var destLWT = e.SourceInfo.LastWriteTimeUtc;
        if (sourceLWT == destLWT)
        {
            e.Action = FileSystemAction.Skip;
        }
        else if (sourceLWT > destLWT)
        {
            e.Action = FileSystemAction.Overwrite;
        }
        else if (sourceLWT < destLWT)
        {
            var message = $"源文件：{e.SourceInfo.Name}\n" +
                          $"源文件修改时间：{sourceLWT}\n\n" +
                          $"目标文件：{e.DestinationInfo.Name}\n" +
                          $"目标文件修改时间：{destLWT}\n\n" +
                          $"是否要覆盖目标文件？";

            var result = CustomDialog.ShowDialog("SaveSync", message, new[] { ("覆盖所有", "OverwriteAll"), ("确认", "Overwrite"), ("跳过", "Skip"), ("取消", "Cancel") }, "Cancel");
            e.Action = Enum.Parse<FileSystemAction>(result["action"]);
        }
    }
    public static void CopyOrOverwriteFolder(string sourceFolder, string destFolder, Action<OverwritePreview>? onOverwrite = null)
    {
        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder);
        }
        CopyOrOverwriteFolderImpl(new NativeDirectoryAccess(sourceFolder), new NativeDirectoryAccess(destFolder), onOverwrite ?? OverwriteDefaultHandler, null);
    }
    public static void CopyOrOverwriteFolder(IDirectoryAccess sourceFolder, IDirectoryAccess destFolder, Action<OverwritePreview>? onOverwrite = null)
        => CopyOrOverwriteFolderImpl(sourceFolder, destFolder, onOverwrite ?? OverwriteDefaultHandler, null);

    /// <summary>
    /// 将源目录（含目录）拷贝到目标目录下
    /// </summary>
    /// <param name="sourceFolder">源目录</param>
    /// <param name="destParentFolder">目标目录</param>
    /// <param name="onOverwrite">当需要覆盖时，调用该函数以确认覆盖结果</param>
    /// <param name="reportProgress">回报进度函数（未实现）</param>
    /// <exception cref="ArgumentException">如果sourceFolder不存在，就会触发该异常</exception>
    static void CopyOrOverwriteFolderImpl(IDirectoryAccess sourceFolder, IDirectoryAccess destParentFolder, Action<OverwritePreview>? onOverwrite, Action<int, int>? reportProgress)
    {
        var destFolderExists = true;
        var destFolder = destParentFolder.GetDirectory(sourceFolder.Name);
        if (destFolder == null)
        {
            destFolderExists = false;
            destFolder = destParentFolder.CreateSubdirectory(sourceFolder.Name);
        }

        // 从源目录复制文件
        var skipConfirm = false;
        var sourceFiles = sourceFolder.GetFiles();
        foreach (var sourceFile in sourceFiles)
        {
            var destFile = destFolder.GetFile(sourceFile.Name);

            // 如果目标目录有该文件
            if (destFile != null)
            {
                // 如果目标目录文件更老就不写入
                if (skipConfirm)
                {
                    sourceFile.CopyTo(destFile);
                    continue;
                }

                var preview = new OverwritePreview(sourceFile, destFile);
                onOverwrite?.Invoke(preview);

                switch (preview.Action)
                {
                    case FileSystemAction.Overwrite:
                        sourceFile.CopyTo(destFile);
                        break;
                    case FileSystemAction.OverwriteAll:
                        sourceFile.CopyTo(destFile);
                        skipConfirm = true;
                        break;
                    case FileSystemAction.Cancel:
                        throw new OperationCanceledException();
                }
            }
            else
            {
                using var sourceStream = sourceFile.OpenRead();
                sourceStream.CopyTo(destFolder.CreateFile(sourceFile.Name));
            }
        }

        // 从源目录复制子目录
        var sourceSubfolders = sourceFolder.GetDirectories();
        foreach (var sourceSubfolder in sourceSubfolders)
        {
            CopyOrOverwriteFolderImpl(sourceSubfolder, destFolder, onOverwrite, reportProgress);
        }

        if (destFolderExists)
        {
            // 删除目标目录中不存在于源目录中的文件
            skipConfirm = false;
            var destFiles = destFolder.GetFiles();
            foreach (var destFile in destFiles)
            {
                var sourceFile = sourceFolder.GetFile(destFile.Name);

                if (sourceFile == null)
                {
                    if (skipConfirm)
                    {
                        destFile.Delete();
                        continue;
                    }

                    var preview = new OverwritePreview(null, destFile);
                    onOverwrite?.Invoke(preview);

                    switch (preview.Action)
                    {
                        case FileSystemAction.Overwrite:
                            destFile.Delete();
                            break;
                        case FileSystemAction.OverwriteAll:
                            destFile.Delete();
                            skipConfirm = true;
                            break;
                        case FileSystemAction.Cancel:
                            throw new OperationCanceledException();
                    }
                }
            }

            // 删除目标目录中不存在于源目录中的子目录
            var destSubfolders = destFolder.GetDirectories();
            foreach (var destSubfolder in destSubfolders)
            {
                var sourceSubfolder = sourceFolder.GetDirectory(destSubfolder.Name);

                if (sourceSubfolder == null)
                {
                    if (skipConfirm)
                    {
                        destFolder.Delete(true);
                        continue;
                    }

                    var preview = new OverwritePreview(null, destSubfolder);
                    onOverwrite?.Invoke(preview);

                    switch (preview.Action)
                    {
                        case FileSystemAction.Overwrite:
                            destFolder.Delete(true);
                            break;
                        case FileSystemAction.OverwriteAll:
                            destFolder.Delete(true);
                            skipConfirm = true;
                            break;
                        case FileSystemAction.Cancel:
                            throw new OperationCanceledException();
                    }
                }
            }
        }
    }
}
