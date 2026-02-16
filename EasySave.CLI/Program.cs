using EasySave.Core.Managers;

namespace EasySave.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            BackupManager manager = new BackupManager();
            MainViewModel vm = new MainViewModel(manager);
            View view = new View(vm);

            view.Run(args);
        }
    }
}
