using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Interfaces
{
    public interface IBackupStrategy
    {
        public void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate);
    }
}
