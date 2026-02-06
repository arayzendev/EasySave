using EasySave.Interfaces;
using EasySave.Strategies;
class BackupManager {
    private List<BackupJob>? backupJobs;
    private StateManager stateManager;
    private ConfigManager configManager;

    public BackupManager()
    {
        configManager = new ConfigManager();
        stateManager = new StateManager();
        backupJobs = configManager.Load();
    }
    public bool CreateJob(string name, string sourcePath, string targetPath, string backupStrategy)
    {
        IBackupStrategy strategy;
        switch (backupStrategy)
        {
            case "diff":
                strategy = new DifferentialBackupStrategy();
                break;

            default:
                strategy = new FullBackupStrategy();
                break;
        }
        if (backupJobs.Count >= 5)
        {
            return false;
        }
        backupJobs.Add(new BackupJob(name,sourcePath,targetPath,strategy));
        configManager.Save(backupJobs);
        return true;
    }
    public void DeleteJob(int index)
    {
        backupJobs.RemoveAt(index);
        configManager.Save(backupJobs);
    }
    public void ModifyJob(int index, string sourcePath, string targetPath)
    {
        backupJobs[index].UpdatePaths(sourcePath,targetPath);
        configManager.Save(backupJobs);
    }
    public void ExecuteJob(int index)
    {
        backupJobs[index].Execute(OnProgressUpdate);
    }
    public List<BackupJob> ListJobs()
    {
        return backupJobs;
    }

    private void OnProgressUpdate()
    {
        stateManager.Write(backupJobs);   
    }
}