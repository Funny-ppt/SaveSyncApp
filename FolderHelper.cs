using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaveSyncApp
{
    public static class FolderHelper
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
                var result = MessageBox.Show(message, "删除文件确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        e.Action = FileSystemAction.Overwrite;
                        break;
                    case MessageBoxResult.No:
                        e.Action = FileSystemAction.Skip;
                        break;
                }
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

                var result = MessageBox.Show(message, "覆盖文件确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        e.Action = FileSystemAction.Overwrite;
                        break;
                    case MessageBoxResult.No:
                        e.Action = FileSystemAction.Skip;
                        break;
                    case MessageBoxResult.Cancel:
                        e.Action = FileSystemAction.Cancel;
                        break;
                }
            }
        }

        public static void CopyOrOverwriteFolder(string sourceFolder, string destFolder, Action<FileSystemOperationPreview>? onOverwrite = null)
         => CopyOrOverwriteFolderImpl(sourceFolder, destFolder, onOverwrite ?? OverwriteDefaultHandler, null);

        // todo: reportProgress未实现
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

                    var preview = new FileSystemOperationPreview(sourceInfo, destInfo);
                    onOverwrite?.Invoke(preview);

                    if (preview.Action == FileSystemAction.Cancel) return;
                    if (preview.Action == FileSystemAction.Overwrite)
                    {
                        File.Copy(sourceFile, destFile, true);
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

                CopyOrOverwriteFolderImpl(sourceSubfolder, destSubfolder, onOverwrite, reportProgress);
            }

            if (destFolderExists)
            {
                // 删除目标目录中不存在于源目录中的文件
                string[] destFiles = Directory.GetFiles(destFolder);
                foreach (string destFile in destFiles)
                {
                    string fileName = Path.GetFileName(destFile);
                    string sourceFile = Path.Combine(sourceFolder, fileName);

                    if (!File.Exists(sourceFile))
                    {
                        var preview = new FileSystemOperationPreview(null, new FileInfo(destFile));
                        onOverwrite?.Invoke(preview);

                        if (preview.Action == FileSystemAction.Cancel) return;
                        if (preview.Action == FileSystemAction.Overwrite)
                        {
                            File.Delete(destFile);
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
                        var preview = new FileSystemOperationPreview(null, new DirectoryInfo(destSubfolder));
                        onOverwrite?.Invoke(preview);

                        if (preview.Action == FileSystemAction.Cancel) return;
                        if (preview.Action == FileSystemAction.Overwrite)
                        {
                            Directory.Delete(destSubfolder, true);
                        }
                    }
                }
            }
        }
    }
}
