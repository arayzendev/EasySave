using System;
using System.ComponentModel;
using EasySave.Core.Managers;

namespace EasySave.Core.Models
{
    public class BackupProgress : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // --- PROPRIÉTÉS CALCULÉES POUR L'UI ---

        public string StateTranslated => LanguageManager.Instance.GetText("State_" + State.ToString());

        // Play : Possible tant que ce n'est pas fini
        public bool CanPlay => State != BackupState.Ended;

        // Pause / Stop : Possible SEULEMENT si en cours ou déjà en pause
        public bool CanPauseOrStop => State == BackupState.Active || State == BackupState.Paused;

        // Modifier : SEULEMENT si Inactif ou Failed
        public bool CanEdit => State == BackupState.Inactive || State == BackupState.Failed;

        // Supprimer : Si fini, échec, jamais lancé ou arrêté
        public bool CanDelete => State == BackupState.Ended ||
                                 State == BackupState.Failed ||
                                 State == BackupState.Inactive ||
                                 State == BackupState.Stopped;

        // --- ATTRIBUTS ET NOTIFICATIONS ---

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
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(StateTranslated));
                    // Mise à jour de tous les états de boutons
                    OnPropertyChanged(nameof(CanPlay));
                    OnPropertyChanged(nameof(CanPauseOrStop));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));
                }
            }
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
