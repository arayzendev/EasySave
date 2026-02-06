using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Interfaces
{
    public interface IBackupStrategy
    {
        /// <summary>
        /// M�thode de sauvegarde
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="backupProgress"></param>
        /// <param name="OnProgressupdate"></param>
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate);
    }
}
