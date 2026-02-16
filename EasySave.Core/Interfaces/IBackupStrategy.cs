using EasyLog;
using EasySave.Core.Models;
using System.Threading;

namespace EasySave.Core.Interfaces
{
    public interface IBackupStrategy
    {
        /// <summary>
        /// M?thode de sauvegarde
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="backupProgress"></param>
        /// <param name="OnProgressupdate"></param>
        /// <param name="logger"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="cancellationToken"></param>
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate, Logger logger, string encryptionKey = null, CancellationToken cancellationToken = default);
    }
}