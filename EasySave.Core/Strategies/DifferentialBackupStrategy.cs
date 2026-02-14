using EasyLog;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.Core.Strategies
{
    internal class DifferentialBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public DifferentialBackupStrategy() { }

        /// <summary>
        /// Méthode de sauvegarde différencielle
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="backupProgress"></param>
        /// <param name="OnProgressupdate"></param>
        /// <param name="logger"></param>
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate, Logger logger)
        {
            try
            {
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 4
                };

                //Vérifie si un chemin source et cible existe
                if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
                {
                    //Création d'un dossier et tableau qui stocke les récupération des fichiers
                    Directory.CreateDirectory(targetPath);
                    string[] allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                    //Sauvegarde ce qui est nouveau
                    List<string> files = new List<string>();
                    foreach(string file in allFiles)
                    {
                        string rel = Path.GetRelativePath(sourcePath, file);
                        string dest = Path.Combine(targetPath, rel);
                        if (!File.Exists(dest) || File.GetLastWriteTime(file) > File.GetLastWriteTime(dest))
                            files.Add(file);
                    }

                    //Met à jour le model backupProgress
                    backupProgress.TotalFiles = files.Count;
                    backupProgress.RemainingFiles = files.Count;
                    backupProgress.State = BackupState.Active;
                    backupProgress.DateTime = DateTime.Now;

                    long totalSize = 0;
                    Parallel.ForEach(files, options, file =>
                    {
                        totalSize += new FileInfo(file).Length;
                        backupProgress.TotalSize = totalSize;
                        backupProgress.RemainingSize = totalSize;
                    });

                    int copiedFiles = 0;
                    long copiedSize = 0;

                    //Boucle qui ajoute les fichiers du chemin source aux chemin cible
                    Parallel.ForEach(files, options, file =>
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
                        backupProgress.Progress = totalSize > 0 ? (float)copiedSize / totalSize * 100 : 100;
                        backupProgress.RemainingFiles = backupProgress.TotalFiles - copiedFiles;
                        backupProgress.RemainingSize = backupProgress.TotalSize - copiedSize;
                        OnProgressupdate?.Invoke();


                        //Ecriture des logs
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
                    });
                }
                else
                {
                    throw new ArgumentException("Source or target path cannot be null or empty.");
                }

                //Réussite du programme
                backupProgress.State = BackupState.Ended;
                backupProgress.Progress = 100;
                OnProgressupdate?.Invoke();
            }
            catch (Exception ex)
            {
                //Log d'erreur
                logger.Write(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Application = "EasySave",
                    data = new Dictionary<string, object>
                            {
                                { "Error DifferentialBackup", ex.Message.ToString()},
                            }
                });
            }
        }
    }
}