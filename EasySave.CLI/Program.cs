using EasySave.CLI;
using EasySave.Core.Managers;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            BackupManager manager = new BackupManager();
            MainViewModel vm = new MainViewModel(manager);
            ConsoleView view = new ConsoleView(vm);

            view.Run(args);
        }
    }
}
