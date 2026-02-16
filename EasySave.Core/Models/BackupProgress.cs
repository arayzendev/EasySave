namespace EasySave.Core.Models
{
    //Progr�s du backup
    public class BackupProgress
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
    }
}
