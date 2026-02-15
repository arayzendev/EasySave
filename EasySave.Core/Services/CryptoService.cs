using System.Diagnostics;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Service de cryptage utilisant CryptoSoft.exe
    /// </summary>
    internal class CryptoService
    {
        // Extensions à crypter
        private static readonly List<string> ExtensionsToEncrypt = new() { ".txt", ".pdf", ".docx" };

        /// <summary>
        /// Vérifie si un fichier doit être crypté
        /// </summary>
        public bool ShouldEncrypt(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return ExtensionsToEncrypt.Contains(extension);
        }

        /// <summary>
        /// Crypte un fichier avec CryptoSoft.exe
        /// </summary>
        public int EncryptFile(string filePath, string key)
        {
            int retries = 10;
            int delay = 500;

            for (int attempt = 0; attempt < retries; attempt++)
            {
                int result = ExecuteCryptoSoft(filePath, key);
                if (result != -1) return result; // Succès ou erreur autre que "occupé"
                Thread.Sleep(delay); // Attendre si occupé
            }
            return -1; // Échec après 10 tentatives
        }

        /// <summary>
        /// Exécute CryptoSoft
        /// </summary>
        private int ExecuteCryptoSoft(string filePath, string key)
        {
            try
            {
                string cryptoSoftPath = AppDomain.CurrentDomain.BaseDirectory;
                
                ProcessStartInfo startInfo;
                
                if (OperatingSystem.IsWindows())
                {
                    string exePath = Path.Combine(cryptoSoftPath, "CryptoSoft.exe");
                    if (!File.Exists(exePath)) return -99;
                    
                    startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = $"\"{filePath}\" \"{key}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else
                {
                    string dllPath = Path.Combine(cryptoSoftPath, "CryptoSoft.dll");
                    if (!File.Exists(dllPath)) return -99;
                    
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"\"{dllPath}\" \"{filePath}\" \"{key}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }

                using Process process = Process.Start(startInfo);
                process.WaitForExit();
                return process.ExitCode;
            }
            catch
            {
                return -99;
            }
        }
    }
}
