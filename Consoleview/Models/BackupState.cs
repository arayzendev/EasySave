using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoleView.Models
{
    internal class BackuState 
    { 
        public string BackupName {  get; set; }
        public DateTime? LastBackup { get; set; }
        public string BackupState { get; set; }
        public string TotalFiles { get; set; }
        public int TotalFilesSizeTransfer { get; set; }
        public string Progression { get; set; }
        public int TotalRemainingFilesSize { get; set; }
        public int TotailRemainingFiles { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }

    }
}
