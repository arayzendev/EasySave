using EasySave.Interfaces;
using EasySave.Strategies;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Factory
{
    internal class BackupStrategyFactory
    {
        BackupStrategyFactory() { }
        public static IBackupStrategy Create(string BackupStrategy)
        {
            if (BackupStrategy == null) 
                throw new ArgumentNullException("BackupStrategy cannot be null or empty.");

            switch (BackupStrategy.ToLower())
            {
                case "full":
                    return new FullBackupStrategy();
                case "differential":
                    return new DifferentialBackupStrategy();
                default:
                    throw new NotSupportedException($"Backup strategy '{BackupStrategy}' is not supported.");
            }

        }
    }
}
