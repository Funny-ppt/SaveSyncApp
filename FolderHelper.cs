using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveSyncApp
{
    public static class FolderHelper
    {
        public static void CopyOrOverwriteFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                throw new ArgumentException(nameof(sourceFolder));
            }

            bool destFolderExists = Directory.Exists(destFolder);
            if (!destFolderExists)
            {
                Directory.CreateDirectory(destFolder);
            }

            // 从源目录复制文件
            string[] sourceFiles = Directory.GetFiles(sourceFolder);
            foreach (string sourceFile in sourceFiles)
            {
                string fileName = Path.GetFileName(sourceFile);
                string destFile = Path.Combine(destFolder, fileName);

                // 如果目标目录有该文件
                if (File.Exists(destFile))
                {
                    // 如果目标目录文件更老就不写入
                    var sourceInfo = new FileInfo(sourceFile);
                    var destInfo = new FileInfo(destFile);

                    if (sourceInfo.LastWriteTimeUtc > destInfo.LastWriteTimeUtc)
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

                CopyOrOverwriteFolder(sourceSubfolder, destSubfolder);
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
                        File.Delete(destFile);
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
                        Directory.Delete(destSubfolder, true);
                    }
                }
            }
        }
    }
}
