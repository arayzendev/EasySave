using System.ComponentModel;

namespace EasySave.Core.Models
{
    public class BackupProgress : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime? _dateTime;
        public DateTime? DateTime
        {
            get => _dateTime;
            set { _dateTime = value; OnPropertyChanged(nameof(DateTime)); }
        }

        private BackupState _state = BackupState.Inactive;
        public BackupState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(nameof(State)); }
        }

        private int _totalFiles;
        public int TotalFiles
        {
            get => _totalFiles;
            set { _totalFiles = value; OnPropertyChanged(nameof(TotalFiles)); }
        }

        private long _totalSize;
        public long TotalSize
        {
            get => _totalSize;
            set { _totalSize = value; OnPropertyChanged(nameof(TotalSize)); }
        }

        private long _fileSize;
        public long FileSize
        {
            get => _fileSize;
            set { _fileSize = value; OnPropertyChanged(nameof(FileSize)); }
        }

        private float _progress;
        public float Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        private float _transferTime;
        public float TransferTime
        {
            get => _transferTime;
            set { _transferTime = value; OnPropertyChanged(nameof(TransferTime)); }
        }

        private int _remainingFiles;
        public int RemainingFiles
        {
            get => _remainingFiles;
            set { _remainingFiles = value; OnPropertyChanged(nameof(RemainingFiles)); }
        }

        private long _remainingSize;
        public long RemainingSize
        {
            get => _remainingSize;
            set { _remainingSize = value; OnPropertyChanged(nameof(RemainingSize)); }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
