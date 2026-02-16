
namespace EasySave.CLI
{
    public class View
    {
        private readonly MainViewModel vm;

        public View(MainViewModel viewModel)
        {
            vm = viewModel;
            vm.AttachView(this);
        }

        public void Run(string[] args)
        {
            vm.Run(args);
        }

        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public string Read()
        {
            return Console.ReadLine();
        }
    }
}
