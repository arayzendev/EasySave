using EasyLog;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Services;
using System.Threading;

namespace EasySave.Core.Strategies
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        public DifferentialBackupStrategy() { }

        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate, Logger logger, string encryptionKey = null, CancellationToken cancellationToken = default)
        {
            CryptoService cryptoService = new CryptoService();

            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentException("Source or target path cannot be null or empty.");
            }

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4
            };

            Directory.CreateDirectory(targetPath);
            string[] allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

            List<string> files = new List<string>();
            Parallel.ForEach(allFiles, options, file =>
            {
                string rel = Path.GetRelativePath(sourcePath, file);
                string dest = Path.Combine(targetPath, rel);
                if (!File.Exists(dest) || File.GetLastWriteTime(file) > File.GetLastWriteTime(dest))
                {
                    lock (files)
                    {
                        files.Add(file);
                    }
                }
            });

            backupProgress.TotalFiles = files.Count;
            backupProgress.RemainingFiles = files.Count;
            backupProgress.State = BackupState.Active;
            backupProgress.DateTime = DateTime.Now;

            long totalSize = 0;
            Parallel.ForEach(files, options, file =>
            {
                Interlocked.Add(ref totalSize, new FileInfo(file).Length);
            });
            backupProgress.TotalSize = totalSize;
            backupProgress.RemainingSize = totalSize;

            int copiedFiles = 0;
            long copiedSize = 0;
            object lockObj = new object();

            try
            {
                Parallel.ForEach(files, options, (file, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                        return;
                    }

                    while (backupProgress.State == BackupState.Paused && !cancellationToken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                        return;
                    }

                    string relativePath = Path.GetRelativePath(sourcePath, file);
                    var destPath = Path.Combine(targetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    File.Copy(file, destPath, true);
                    stopwatch.Stop();

                    int encryptionTime = 0;
                    if (!string.IsNullOrEmpty(encryptionKey) && cryptoService.ShouldEncrypt(file))
                    {
                        encryptionTime = cryptoService.EncryptFile(destPath, encryptionKey);
                    }

                    long fileSize = new FileInfo(file).Length;

                    lock (lockObj)
                    {
                        copiedFiles++;
                        copiedSize += fileSize;

                        backupProgress.FileSize = fileSize;
                        backupProgress.TransferTime = (float)stopwatch.ElapsedMilliseconds;
                        backupProgress.Progress = totalSize > 0 ? (float)copiedSize / totalSize * 100 : 100;
                        backupProgress.RemainingFiles = backupProgress.TotalFiles - copiedFiles;
                        backupProgress.RemainingSize = backupProgress.TotalSize - copiedSize;
                        OnProgressupdate?.Invoke();
                    }

                    logger.Write(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Application = "EasySave",
                        data = new Dictionary<string, object>
                        {
                            { "SourceFile", file },
                            { "TargetFile", destPath },
                            { "FileSize", fileSize },
                            { "TransferTimeMs", stopwatch.ElapsedMilliseconds },
                            { "EncryptionTimeMs", encryptionTime }
                        }
                    });
                });

                if (cancellationToken.IsCancellationRequested)
                {
                    backupProgress.State = BackupState.Stopped;
                }
                else
                {
                    backupProgress.State = BackupState.Ended;
                    backupProgress.Progress = 100;
                }
            }
            catch (Exception ex)
            {
                logger.Write(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Application = "EasySave",
                    data = new Dictionary<string, object>
                    {
                        { "Error DifferentialBackup", ex.Message.ToString() }
                    }
                });
                backupProgress.State = BackupState.Stopped;
            }

            OnProgressupdate?.Invoke();
        }
    }
}
