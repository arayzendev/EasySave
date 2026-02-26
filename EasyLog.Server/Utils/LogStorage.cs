using EasyLog.Models;
using System;
using System.IO;

namespace EasyLog.Server.Utils
{
    public static class LogStorage
    {
        private static readonly object fileLock = new object();

        private static string GetLogDirectory()
        {
            bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            string baseDir = isDocker ? "/logs" : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            return baseDir;
        }

        public static void Save(LogEntry entry, string extension)
        {
            if (entry == null) return;

            string dir = GetLogDirectory();
            string fileName = $"logs_{DateTime.Now:yyyy-MM-dd}.{extension.ToLower()}"; // Un seul fichier journalier
            string path = Path.Combine(dir, fileName);

            string timestamp = entry.Timestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string user = string.IsNullOrWhiteSpace(entry.User) ? "UnknownUser" : entry.User;
            string app = string.IsNullOrWhiteSpace(entry.Application) ? "UnknownApp" : entry.Application;

            string line = $"[{timestamp}] User: {user} | App: {app} | ";

            if (entry.data != null)
            {
                foreach (var kv in entry.data)
                {
                    line += $"{kv.Key}: {kv.Value}; ";
                }
            }

            lock (fileLock)
            {
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
    }
}