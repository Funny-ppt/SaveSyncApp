﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaveSyncApp;

internal static class FolderHelper
{
    static void CopyOrOverwriteFolderOnBackground(BackgroundWorker worker, string sourceFolder, string destFolder, Action<FileSystemOperationPreview>? onOverwrite = null)
    {
        worker.WorkerReportsProgress = true;
        worker.DoWork += (sender, e) =>
        {
            CopyOrOverwriteFolderImpl(sourceFolder, destFolder, onOverwrite, (curr, total) => worker.ReportProgress(curr * 100 / total));
        };
    }

    static void OverwriteDefaultHandler(FileSystemOperationPreview e)
    {
        if (e.SourceInfo == null)
        {
            var message = $"将删除该文文件(夹):\n" +
                          $"{e.DestinationInfo}";

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
            var message = $"源文件：{e.SourceInfo}\n" +
                          $"源文件修改时间：{sourceLWT}\n\n" +
                          $"目标文件：{e.DestinationInfo}\n" +
                          $"目标文件修改时间：{destLWT}\n\n" +
                          $"是否要覆盖目标文件？";

            var result = CustomDialog.ShowDialog("SaveSync", message, new[] { ("覆盖所有", "OverwriteAll"), ("确认", "Overwrite"), ("跳过", "Skip"), ("取消", "Cancel") }, "Cancel");
            e.Action = Enum.Parse<FileSystemAction>(result["action"]);
        }
    }

    public static void CopyOrOverwriteFolder(string sourceFolder, string destFolder, Action<FileSystemOperationPreview>? onOverwrite = null)
     => CopyOrOverwriteFolderImpl(sourceFolder, destFolder, onOverwrite ?? OverwriteDefaultHandler, null);

    /// <summary>
    /// 将源目录（含目录）拷贝到目标目录下
    /// </summary>
    /// <param name="sourceFolder">源目录</param>
    /// <param name="destFolder">目标目录</param>
    /// <param name="onOverwrite">当需要覆盖时，调用该函数以确认覆盖结果</param>
    /// <param name="reportProgress">回报进度函数（未实现）</param>
    /// <exception cref="ArgumentException">如果sourceFolder不存在，就会触发该异常</exception>
    static void CopyOrOverwriteFolderImpl(string sourceFolder, string destFolder, Action<FileSystemOperationPreview>? onOverwrite, Action<int, int>? reportProgress)
    {
        if (!Directory.Exists(sourceFolder))
        {
            throw new ArgumentException(null, nameof(sourceFolder));
        }
        destFolder = Path.Combine(destFolder, Path.GetFileName(sourceFolder));
        bool destFolderExists = Directory.Exists(destFolder);
        if (!destFolderExists)
        {
            Directory.CreateDirectory(destFolder);
        }

        // 从源目录复制文件
        var skipConfirm = false;
        var sourceFiles = Directory.GetFiles(sourceFolder);
        for (int i = 0; i < sourceFiles.Length; i++)
        {
            var sourceFile = sourceFiles[i];
            var fileName = Path.GetFileName(sourceFile);
            var destFile = Path.Combine(destFolder, fileName);

            // 如果目标目录有该文件
            if (File.Exists(destFile))
            {
                // 如果目标目录文件更老就不写入
                var sourceInfo = new FileInfo(sourceFile);
                var destInfo = new FileInfo(destFile);

                if (skipConfirm)
                {
                    File.Copy(sourceFile, destFile, true);
                    continue;
                }

                var preview = new FileSystemOperationPreview(sourceInfo, destInfo);
                onOverwrite?.Invoke(preview);

                switch (preview.Action)
                {
                    case FileSystemAction.Overwrite:
                        File.Copy(sourceFile, destFile, true);
                        break;
                    case FileSystemAction.OverwriteAll:
                        File.Copy(sourceFile, destFile, true);
                        skipConfirm = true;
                        break;
                    case FileSystemAction.Cancel:
                        return;
                }
            }
            else
            {
                File.Copy(sourceFile, destFile);
            }
        }

        // 从源目录复制子目录
        string[] sourceSubfolders = Directory.GetDirectories(sourceFolder);
        foreach (string sourceSubfolder in sourceSubfolders)
        {
            string subfolderName = Path.GetFileName(sourceSubfolder);
            string destSubfolder = Path.Combine(destFolder, subfolderName);

            CopyOrOverwriteFolderImpl(sourceSubfolder, destFolder, onOverwrite, reportProgress);
        }

        if (destFolderExists)
        {
            // 删除目标目录中不存在于源目录中的文件
            skipConfirm = false;
            string[] destFiles = Directory.GetFiles(destFolder);
            foreach (string destFile in destFiles)
            {
                string fileName = Path.GetFileName(destFile);
                string sourceFile = Path.Combine(sourceFolder, fileName);

                if (!File.Exists(sourceFile))
                {
                    if (skipConfirm)
                    {
                        File.Delete(destFile);
                        continue;
                    }

                    var preview = new FileSystemOperationPreview(null, new FileInfo(destFile));
                    onOverwrite?.Invoke(preview);

                    switch (preview.Action)
                    {
                        case FileSystemAction.Overwrite:
                            File.Delete(destFile);
                            break;
                        case FileSystemAction.OverwriteAll:
                            File.Delete(destFile);
                            skipConfirm = true;
                            break;
                        case FileSystemAction.Cancel:
                            return;
                    }
                }
            }

            // 删除目标目录中不存在于源目录中的子目录
            string[] destSubfolders = Directory.GetDirectories(destFolder);
            foreach (string destSubfolder in destSubfolders)
            {
                string subfolderName = Path.GetFileName(destSubfolder);
                string sourceSubfolder = Path.Combine(sourceFolder, subfolderName);

                if (!Directory.Exists(sourceSubfolder))
                {
                    if (skipConfirm)
                    {
                        Directory.Delete(destSubfolder, true);
                        continue;
                    }

                    var preview = new FileSystemOperationPreview(null, new DirectoryInfo(destSubfolder));
                    onOverwrite?.Invoke(preview);

                    switch (preview.Action)
                    {
                        case FileSystemAction.Overwrite:
                            Directory.Delete(destSubfolder, true);
                            break;
                        case FileSystemAction.OverwriteAll:
                            Directory.Delete(destSubfolder, true);
                            skipConfirm = true;
                            break;
                        case FileSystemAction.Cancel:
                            return;
                    }
                }
            }
        }
    }
}
