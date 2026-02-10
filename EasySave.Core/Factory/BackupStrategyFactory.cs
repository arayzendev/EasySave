using EasySave.Core.Interfaces;
using EasySave.Core.Strategies;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Core.Factory
{
    internal class BackupStrategyFactory
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public BackupStrategyFactory() { }

        /// <summary>
        /// Créationn d'une stratégie de sauvegarde
        /// </summary>
        /// <param name="BackupStrategy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public IBackupStrategy Create(string BackupStrategy)
        {
            if (BackupStrategy == null) 
                throw new ArgumentNullException("BackupStrategy cannot be null or empty.");

            //Choix de création de stratégie entre différentiel et complète
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
