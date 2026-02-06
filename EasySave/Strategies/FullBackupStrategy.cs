using EasySave.Interfaces;
using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;
using EasyLog;

namespace EasySave.Strategies
{
    internal class FullBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public FullBackupStrategy() { }
        /// <summary>
        /// Sauvegarde complète
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="backupProgress"></param>
        /// <param name="OnProgressupdate"></param>
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate)
        {
            try
            {   //Vérifie si un chemin source et cible existe
                if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
                {
                    //Créer un dossier dans path et stocke les fichiers de la source
                    Directory.CreateDirectory(targetPath);
                    string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                    //Met à jour le progrès et l'état
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

                    //Boucle pour copier tous les fichiers vers le chemin cible
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
                        backupProgress.Progress = (float)copiedSize / backupProgress.TotalSize * 100;
                        backupProgress.RemainingFiles = backupProgress.TotalFiles - copiedFiles;
                        backupProgress.RemainingSize = backupProgress.TotalSize - copiedSize;
                        OnProgressupdate?.Invoke();

                        //Ecrit les logs 
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
                //Réussite de la sauvegarde
                backupProgress.State = BackupState.Ended;
                backupProgress.Progress = 100;
                OnProgressupdate?.Invoke();
            }
            catch (Exception ex) 
            {
                //Log d'erreur
                Logger log = new Logger(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySaveData", "Logs"°);
                log.Write(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Application = "EasySave",
                    data = new Dictionary<string, object>
                            {
                                { "Error FullBackup", ex.Message.ToString()},
                            }
                });
            }
        }
    }
}
