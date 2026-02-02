using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleView.Models
{
    internal class BackupJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SourceReportory { get; set; }
        public string TargetRepository { get; set; }
        public BackupType Type { get; set; }

    }
}
