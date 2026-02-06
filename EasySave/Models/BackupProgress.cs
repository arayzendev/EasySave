using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasySave.Models
{
    internal class BackupProgress
    {
        public DateTime? DateTime { get; set; }
        public BackupState State { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public long FileSize { get; set; }
        public float Progress { get; set; }
        public float TransferTime { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
    }
}
