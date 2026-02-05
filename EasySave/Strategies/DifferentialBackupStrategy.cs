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
            if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
            {
                string[] files = Directory.GetFiles(sourcePath);
                foreach (string file in files)
                {
                    var sourceFile = Path.GetFileName(file);
                    var destPath = Path.Combine(targetPath, sourceFile);
                    File.Copy(file, destPath, true);


                }
            }
            else
            {

            }
        }
    }
}
