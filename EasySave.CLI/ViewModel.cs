using EasyLog;
using EasySave.CLI;
using EasySave.Core.Managers;
using EasySave.Core.Models;

namespace EasySave.CLI
{
    public class MainViewModel
    {
        private readonly BackupManager backupManager;
        private View view;

        public MainViewModel(BackupManager manager)
        {
            backupManager = manager;
        }

        public void AttachView(View v)
        {
            view = v;
        }

        public void Run(string[] args)
        {
            if (args.Length > 0)
            {
                ExecuteFromArgs(args[0]);
                return;
            }

            bool langue = true;
            bool logActive = true;
            bool logModeActive = true;
            bool extensionActive = true;

            ShowBanner();

            while (langue)
            {
                view.Write("Choisir la Langue (FR/EN) :");
                string langChoice = view.Read().ToUpper();

                if (langChoice == "FR" || langChoice == "EN")
                {
                    backupManager.SetLanguage(langChoice);
                    langue = false;
                }
                else
                    view.Write("Veuillez Réessayez.");
            }

            while (logActive)
            {
                view.Write("Choisir le type de logs (JSON/XML) :");
                string logChoice = view.Read().ToUpper();

                if (logChoice == "JSON" || logChoice == "XML")
                {
                    backupManager.SetLog(logChoice);
                    logActive = false;
                }
                else
                    view.Write("Veuillez Réessayez.");
            }

            while (logModeActive)
            {
                view.Write("Choisir le mode de logs (Local/Docker/All) :");
                string logModeChoice = view.Read().ToUpper();

                if (logModeChoice == "LOCAL" || logModeChoice == "DOCKER" || logModeChoice == "ALL")
                {
                    backupManager.SetLogMode(logModeChoice);
                    logModeActive = false;
                }
                else
                    view.Write("Veuillez Réessayez.");
            }

            while (extensionActive)
            {
                view.Write("Choisir une extension prioritaire (y/n) :");
                string extChoice = view.Read().ToUpper();

                if (extChoice == "Y")
                {
                    Console.WriteLine("Entrez les extensions prioritaires séparées par ';' (ex: .txt;.pdf;.docx) :");

                    List<string> extensions = Console.ReadLine()
                        .Split(';')
                        .Select(e => e.Trim().ToLower())
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .ToList();

                    backupManager.UpdatePriorityExtensions(extensions);
                    extensionActive = false;
                }
                else if (extChoice == "N")
                {
                    extensionActive = false;
                }
                else
                    view.Write("Veuillez Réessayez.");
            }

            bool running = true;

            while (running)
            {
                ShowMenu();

                string choix = view.Read();

                switch (choix)
                {
                    case "1": CreateJob(); break;
                    case "2": ListJobs(); break;
                    case "3": ExecuteSingleJob(); break;
                    case "4": ExecuteAllJobs(); break;
                    case "5": Modify(); break;
                    case "6": DeleteJob(); break;
                    case "7": ConfigureForbiddenSoftware(); break;
                    case "8": backupManager.ShutdownLogger(); running = false; break;

                    default:
                        view.Write(LanguageManager.Instance.GetText("Menu_Invalid"));
                        break;
                }
            }

            view.Write(LanguageManager.Instance.GetText("Fin_Prog"));
        }

        private void ShowMenu()
        {
            view.Write("\n" + LanguageManager.Instance.GetText("Menu_Titre"));
            view.Write(LanguageManager.Instance.GetText("Menu_Create"));
            view.Write(LanguageManager.Instance.GetText("Menu_List"));
            view.Write(LanguageManager.Instance.GetText("Menu_Execute"));
            view.Write(LanguageManager.Instance.GetText("Menu_ExecuteAll"));
            view.Write(LanguageManager.Instance.GetText("Menu_Modify"));
            view.Write(LanguageManager.Instance.GetText("Menu_Delete"));
            view.Write(LanguageManager.Instance.GetText("Menu_ForbiddenSoftware"));
            view.Write(LanguageManager.Instance.GetText("Menu_Quit"));
            view.Write(LanguageManager.Instance.GetText("Menu_Choice"));
        }

        public static void ShowBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("  ███████╗ █████╗ ███████╗██╗   ██╗ ██████╗ █████╗ ██╗   ██╗███████╗");
            Console.WriteLine("  ██╔════╝██╔══██╗██╔════╝╚██╗ ██╔╝██╔════╝██╔══██╗██║   ██║██╔════╝");
            Console.WriteLine("  █████╗  ███████║███████╗ ╚████╔╝ ███████╗███████║██║   ██║█████╗  ");
            Console.WriteLine("  ██╔══╝  ██╔══██║╚════██║  ╚██╔╝  ╚════██║██╔══██║╚██╗ ██╔╝██╔══╝  ");
            Console.WriteLine("  ███████╗██║  ██║███████║   ██║   ███████║██║  ██║ ╚████╔╝ ███████╗");
            Console.WriteLine("  ╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝   ╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝");
            Console.WriteLine();
            Console.WriteLine("                  ░░░ EASYSAVE V3.0 ░░░");
            Console.WriteLine();
            Console.WriteLine();

            Console.ResetColor();

        }

        private void CreateJob()
        {
            view.Write(LanguageManager.Instance.GetText("Saisie_Nom"));
            string nom = view.Read();

            view.Write(LanguageManager.Instance.GetText("Saisie_Source"));
            string source = view.Read();

            if (!Directory.Exists(source))
            {
                view.Write(LanguageManager.Instance.GetText("Err_Chemin"));
                return;
            }

            view.Write(LanguageManager.Instance.GetText("Saisie_Dest"));
            string dest = view.Read();

            view.Write(LanguageManager.Instance.GetText("Question_Type"));
            view.Write(LanguageManager.Instance.GetText("Type_Complet"));
            view.Write(LanguageManager.Instance.GetText("Type_Diff"));

            string typeChoix = view.Read();

            string strategy = typeChoix switch
            {
                "1" => "full",
                "2" => "differential",
                _ => null
            };

            if (strategy == null)
            {
                view.Write(LanguageManager.Instance.GetText("Menu_Invalid"));
                return;
            }

            bool success = backupManager.CreateJob(nom, source, dest, strategy);

            view.Write(success
                ? $"{LanguageManager.Instance.GetText("Msg_Succes")} : {nom}"
                : LanguageManager.Instance.GetText("Err_Create"));
        }

        private void ListJobs()
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                view.Write(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            view.Write(LanguageManager.Instance.GetText("List_Header"));

            for (int i = 0; i < jobs.Count; i++)
            {
                view.Write($"\n[{i + 1}]");
                view.Write($"  {LanguageManager.Instance.GetText("List_Name")}{jobs[i].name}");
                view.Write($"  {LanguageManager.Instance.GetText("List_Source")}{jobs[i].sourcePath}");
                view.Write($"  {LanguageManager.Instance.GetText("List_Target")}{jobs[i].targetPath}");
            }
        }

        private void ExecuteSingleJob()
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                view.Write(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            ListJobs();

            view.Write(LanguageManager.Instance.GetText("Prompt_JobNumber"));

            if (int.TryParse(view.Read(), out int index) && index >= 1 && index <= jobs.Count)
            {
                try
                {
                    view.Write(LanguageManager.Instance.GetText("Encryption_Key_Input"));
                    string key = view.Read();

                    view.Write(LanguageManager.Instance.GetText("Msg_Execution"));

                    backupManager.ExecuteJob(index - 1, key);

                    view.Write($"{LanguageManager.Instance.GetText("Msg_Execute_Succes")} : {jobs[index - 1].name}");
                }
                catch (Exception ex)
                {
                    view.Write(LanguageManager.Instance.GetText("Msg_Execute_Fail") + ex.Message);
                }
            }
            else
                view.Write(LanguageManager.Instance.GetText("Err_Index"));
        }

        private void ExecuteAllJobs()
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                view.Write(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            view.Write(LanguageManager.Instance.GetText("Encryption_Key_Input"));
            string key = view.Read();

            Parallel.For(0, jobs.Count, new ParallelOptions { MaxDegreeOfParallelism = 3 }, i =>
            {
                try
                {
                    view.Write($"{LanguageManager.Instance.GetText("Msg_Execution")} {jobs[i].name}");

                    backupManager.ExecuteJob(i, key);

                    view.Write($"{LanguageManager.Instance.GetText("Msg_Execute_Succes")} : {jobs[i].name}");
                }
                catch (Exception ex)
                {
                    view.Write(LanguageManager.Instance.GetText("Msg_Execute_Fail") + ex.Message);
                }
            });
        }

        private void Modify()
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                view.Write(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            ListJobs();

            view.Write(LanguageManager.Instance.GetText("Prompt_JobNumber"));

            if (int.TryParse(view.Read(), out int index) && index >= 1 && index <= jobs.Count)
            {
                view.Write(LanguageManager.Instance.GetText("Saisie_Source"));
                string src = view.Read();

                view.Write(LanguageManager.Instance.GetText("Saisie_Dest"));
                string dest = view.Read();

                backupManager.ModifyJob(index - 1, src, dest);

                view.Write(LanguageManager.Instance.GetText("Msg_Modify"));
            }
            else
                view.Write(LanguageManager.Instance.GetText("Err_Index"));
        }

        private void DeleteJob()
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                view.Write(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            ListJobs();

            view.Write(LanguageManager.Instance.GetText("Prompt_JobNumber"));

            if (int.TryParse(view.Read(), out int index) && index >= 1 && index <= jobs.Count)
            {
                backupManager.DeleteJob(index - 1);
                view.Write(LanguageManager.Instance.GetText("Msg_Deleted"));
            }
            else
                view.Write(LanguageManager.Instance.GetText("Err_Index"));
        }

        private void ConfigureForbiddenSoftware()
        {
            view.Write($"\n{LanguageManager.Instance.GetText("Prompt_ForbiddenSoftware")}\nactuel : ({backupManager.GetForbiddenSoftware()})");

            string software = view.Read();

            if (!string.IsNullOrWhiteSpace(software))
            {
                backupManager.SetForbiddenSoftware(software);
                view.Write(LanguageManager.Instance.GetText("Msg_ForbiddenSoftwareSet"));
            }
        }

        private void ExecuteFromArgs(string arg)
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                view.Write(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            List<int> indices = new List<int>();

            if (arg.Contains("-"))
            {
                var parts = arg.Split('-');
                if (int.TryParse(parts[0], out int start) &&
                    int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                        indices.Add(i);
                }
            }
            else if (arg.Contains(";"))
            {
                foreach (var part in arg.Split(';'))
                    if (int.TryParse(part, out int idx))
                        indices.Add(idx);
            }
            else if (int.TryParse(arg, out int single))
                indices.Add(single);

            view.Write(LanguageManager.Instance.GetText("Encryption_Key_Input"));
            string key = view.Read();

            foreach (int i in indices)
            {
                if (i >= 1 && i <= jobs.Count)
                {
                    try
                    {
                        view.Write($"{LanguageManager.Instance.GetText("Msg_Execution")} {jobs[i - 1].name}");

                        backupManager.ExecuteJob(i - 1, key);

                        view.Write($"{LanguageManager.Instance.GetText("Msg_Execute_Succes")} : {jobs[i - 1].name}");
                    }
                    catch (Exception ex)
                    {
                        view.Write(LanguageManager.Instance.GetText("Msg_Execute_Fail") + ex.Message);
                    }
                }
                else
                    view.Write(LanguageManager.Instance.GetText("Err_Index"));
            }
        }
    }
}
