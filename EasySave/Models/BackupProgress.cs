using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasySave.Models
{
    internal class BackupProgress
    {
        public DateTime? _dateTime { get; private set; }
        public BackupState _state { get; private set; }
        public int _totalFiles { get; private set; }
        public long _totalSize { get; private set; }
        public long _fileSize { get; private set; }
        public float _progress { get; private set; }
        public float _transferTime { get; private set; }
        public int _remainingFiles { get; private set; }
        public long _remainingSize { get; private set; }
        public string sourceFilePath { get; private set; }
        public string targetFilePath { get; private set; }
    }
}
