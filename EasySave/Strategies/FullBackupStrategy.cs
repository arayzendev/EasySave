using EasySave.Interfaces;
using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Strategies
{
    internal class FullBackupStrategy : IBackupStrategy
    {

        public FullBackupStrategy() { }
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                    string[] files = Directory.GetFiles(sourcePath);
                    backupProgress.TotalFiles = files.Length;
                    backupProgress.RemainingFiles = files.Length;
                    backupProgress.State = BackupState.Active;
                    backupProgress.DateTime = DateTime.Now;

                    long totalSize = 0;
                    foreach (var file in files)
                        totalSize += new FileInfo(file).Length;
                    backupProgress.TotalSize = totalSize;
                    backupProgress.RemainingSize = totalSize;

                    int copiedFiles = 0;
                    long copiedSize = 0;

                    foreach (string file in files)
                    {
                        var sourceFile = Path.GetFileName(file);
                        var destPath = Path.Combine(targetPath, sourceFile);
                        File.Copy(file, destPath, true);

                        // Mise à jour du backupProgress
                        long fileSize = new FileInfo(file).Length;
                        copiedFiles++;
                        copiedSize += fileSize;

                        backupProgress.FileSize = fileSize;
                        backupProgress.SourceFilePath = file;
                        backupProgress.TargetFilePath = destPath;
                        backupProgress.Progress = (float)copiedSize / backupProgress.TotalSize * 100;
                        backupProgress.RemainingFiles = backupProgress.TotalFiles - copiedFiles;
                        backupProgress.RemainingSize = backupProgress.TotalSize - copiedSize;
                        OnProgressupdate?.Invoke();
                    }
                }
                else
                {
                    throw new ArgumentException("Source or target path cannot be null or empty.");
                }

                backupProgress.State = BackupState.Ended;
                backupProgress.Progress = 100;
                OnProgressupdate?.Invoke();
            }
            catch (Exception ex) 
            {
               Console.WriteLine(ex.ToString());
            }
        }
    }
}
