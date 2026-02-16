using EasyLog;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Services;
using System.Threading;

namespace EasySave.Core.Strategies
{
    internal class FullBackupStrategy : IBackupStrategy
    {
        public FullBackupStrategy() { }

        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate, Logger logger, string encryptionKey = null, CancellationToken cancellationToken = default)
        {
            CryptoService cryptoService = new CryptoService();

            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentException("Source or target path cannot be null or empty.");
            }

            Directory.CreateDirectory(targetPath);
            string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

            backupProgress.TotalFiles = files.Length;
            backupProgress.RemainingFiles = files.Length;
            backupProgress.State = BackupState.Active;
            backupProgress.DateTime = DateTime.Now;

            long totalSize = 0;
            foreach (var file in files)
            {
                totalSize += new FileInfo(file).Length;
            }
            backupProgress.TotalSize = totalSize;
            backupProgress.RemainingSize = totalSize;

            int copiedFiles = 0;
            long copiedSize = 0;

            foreach (string file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    backupProgress.State = BackupState.Stopped;
                    OnProgressupdate?.Invoke();
                    return;
                }
                    MaxDegreeOfParallelism = 3 // maximum 4 threads en parallèle
                };

                if (backupProgress.State == BackupState.Paused)
                {
                    while (backupProgress.State == BackupState.Paused && !cancellationToken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        backupProgress.State = BackupState.Stopped;
                        OnProgressupdate?.Invoke();
                        return;
                    }
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
                copiedFiles++;
                copiedSize += fileSize;

                backupProgress.FileSize = fileSize;
                backupProgress.TransferTime = (float)stopwatch.ElapsedMilliseconds;
                backupProgress.Progress = (float)copiedSize / backupProgress.TotalSize * 100;
                backupProgress.RemainingFiles = backupProgress.TotalFiles - copiedFiles;
                backupProgress.RemainingSize = backupProgress.TotalSize - copiedSize;
                OnProgressupdate?.Invoke();

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
            }

            backupProgress.State = BackupState.Ended;
            backupProgress.Progress = 100;
            OnProgressupdate?.Invoke();
        }
    }
}
