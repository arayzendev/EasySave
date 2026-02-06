using EasyLog;
using EasySave.Interfaces;
using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Strategies
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        public DifferentialBackupStrategy() { }
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                    string[] allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                    // Build list of files to copy (newer or missing in target)
                    List<string> files = new List<string>();
                    foreach (string file in allFiles)
                    {
                        string rel = Path.GetRelativePath(sourcePath, file);
                        string dest = Path.Combine(targetPath, rel);
                        if (!File.Exists(dest) || File.GetLastWriteTime(file) > File.GetLastWriteTime(dest))
                            files.Add(file);
                    }

                    backupProgress.TotalFiles = files.Count;
                    backupProgress.RemainingFiles = files.Count;
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
                        string relativePath = Path.GetRelativePath(sourcePath, file);
                        var destPath = Path.Combine(targetPath, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        File.Copy(file, destPath, true);
                        stopwatch.Stop();

                        // Mise à jour du backupProgress
                        long fileSize = new FileInfo(file).Length;
                        copiedFiles++;
                        copiedSize += fileSize;

                        backupProgress.FileSize = fileSize;
                        backupProgress.TransferTime = (float)stopwatch.ElapsedMilliseconds;
                        backupProgress.SourceFilePath = file;
                        backupProgress.TargetFilePath = destPath;
                        backupProgress.Progress = totalSize > 0 ? (float)copiedSize / totalSize * 100 : 100;
                        backupProgress.RemainingFiles = backupProgress.TotalFiles - copiedFiles;
                        backupProgress.RemainingSize = backupProgress.TotalSize - copiedSize;
                        OnProgressupdate?.Invoke();

                        Logger logger = new Logger(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySaveData", "Logs"));
                        logger.Write(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            Application = "EasySave",
                            data = new Dictionary<string, object>
                            {
                                { "SourceFile", file },
                                { "TargetFile", destPath },
                                { "FileSize", fileSize },
                                { "TransferTimeMs", stopwatch.ElapsedMilliseconds }
                            }
                        });
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
