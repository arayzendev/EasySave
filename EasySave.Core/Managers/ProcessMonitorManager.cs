using System.Diagnostics;

namespace EasySave.Managers
{
    /// <summary>
    /// Classe utilitaire pour surveiller les processus
    /// </summary>
    class ProcessMonitor
    {
        /// <summary>
        /// Verifie si un processus est en cours d'execution (insensible a la casse)
        /// </summary>
        public bool IsRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            // Recherche insensible a la casse
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.ToLower()==processName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
    }
}