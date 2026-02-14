using EasySave.Core.Managers;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            BackupManager backupManager = new BackupManager();
            bool langue = true;
            bool logActive = true;

            //GESTIONNAIRE DE LANGUE
            while (langue == true)
            {
                Console.Write("Choisir la Langue (FR/EN) :");
                string langChoice = Console.ReadLine().ToUpper();

                if (langChoice == "FR" || langChoice == "EN")
                {
                    backupManager.SetLanguage(langChoice);
                    langue = false;
                }
                else
                {
                    Console.WriteLine("Veuillez Réessayez.");
                }
            }

            //GESTIONNAIRE DE logs
            while (logActive == true)
            {
                Console.Write("Choisir le type de logs (JSON/XML) :");
                string logChoice = Console.ReadLine().ToUpper();

                if (logChoice == "JSON" || logChoice == "XML")
                {
                    backupManager.SetLog(logChoice);
                    logActive = false;
                }
                else
                {
                    Console.WriteLine("Veuillez Réessayez.");
                }

            }

            // CLI argument mode
            if (args.Length > 0)
            {
                ExecuteFromArgs(backupManager, args[0]);
                return;
            }

            // Interactive menu mode
            bool running = true;
            while (running)
            {
                Console.WriteLine($"\n{LanguageManager.Instance.GetText("Menu_Titre")}");
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_Create"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_List"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_Execute"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_ExecuteAll"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_Modify"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_Delete"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_ForbiddenSoftware"));
                Console.WriteLine(LanguageManager.Instance.GetText("Menu_Quit"));
                Console.Write(LanguageManager.Instance.GetText("Menu_Choice"));

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        CreateJob(backupManager);
                        break;
                    case "2":
                        ListJobs(backupManager);
                        break;
                    case "3":
                        ExecuteSingleJob(backupManager);
                        break;
                    case "4":
                        ExecuteAllJobs(backupManager);
                        break;
                    case "5":
                        Modify(backupManager);
                        break;
                    case "6":
                        DeleteJob(backupManager);
                        break;
                    case "7":
                        ConfigureForbiddenSoftware(backupManager);
                        break;
                    case "8":
                        running = false;
                        break;
                    default:
                        Console.WriteLine(LanguageManager.Instance.GetText("Menu_Invalid"));
                        break;
                }
            }

            Console.WriteLine(LanguageManager.Instance.GetText("Fin_Prog"));
        }

        /// <summary>
        /// Création d'un travailleur 
        /// </summary>
        /// <param name="backupManager"></param>
        static void CreateJob(BackupManager backupManager)
        {

            //récupère le nom du travailleur
            Console.Write(LanguageManager.Instance.GetText("Saisie_Nom"));
            string nom = Console.ReadLine();

            //Récupère la source du chemin
            Console.Write(LanguageManager.Instance.GetText("Saisie_Source"));
            string source = Console.ReadLine();

            if (!System.IO.Directory.Exists(source))
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_Chemin"));
                return;
            }

            //Récupère la cible du chemin
            Console.Write(LanguageManager.Instance.GetText("Saisie_Dest"));
            string dest = Console.ReadLine();

            //Récupère le type de travailleur
            Console.WriteLine(LanguageManager.Instance.GetText("Question_Type"));
            Console.WriteLine(LanguageManager.Instance.GetText("Type_Complet"));
            Console.WriteLine(LanguageManager.Instance.GetText("Type_Diff"));

            string typeChoix = Console.ReadLine();
            string backupStrategy;

            //Choix type de travailleur
            switch (typeChoix)
            {
                case "1":
                    backupStrategy = "full";
                    break;
                case "2":
                    backupStrategy = "differential";
                    break;
                default:
                    Console.WriteLine(LanguageManager.Instance.GetText("Menu_Invalid"));
                    return;
            }

            //Crée et retourne si le travailleur a été créé ou non 
            bool success = backupManager.CreateJob(nom, source, dest, backupStrategy);

            if (success)
            {
                Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Succes")} : {nom}");
            }
            else
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_Create"));
            }
        }

        /// <summary>
        /// retourne les travailleurs existants et sa configuration
        /// </summary>
        /// <param name="backupManager"></param>
        static void ListJobs(BackupManager backupManager)
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            Console.WriteLine(LanguageManager.Instance.GetText("List_Header"));
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"\n[{i + 1}]");
                Console.WriteLine($"  {LanguageManager.Instance.GetText("List_Name")}{jobs[i].name}");
                Console.WriteLine($"  {LanguageManager.Instance.GetText("List_Source")}{jobs[i].sourcePath}");
                Console.WriteLine($"  {LanguageManager.Instance.GetText("List_Target")}{jobs[i].targetPath}");
            }
        }

        /// <summary>
        /// Execute un seul travailleur
        /// </summary>
        /// <param name="backupManager"></param>
        static void ExecuteSingleJob(BackupManager backupManager)
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            ListJobs(backupManager);
            Console.Write(LanguageManager.Instance.GetText("Prompt_JobNumber"));

            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= jobs.Count)
            {
                try
                {
                    Console.Write(LanguageManager.Instance.GetText("Encryption_Key_Input"));
                    string encryptionKey = Console.ReadLine();
                    Console.WriteLine(LanguageManager.Instance.GetText("Msg_Execution"));
                    backupManager.ExecuteJob(index - 1, encryptionKey);
                    Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Execute_Succes")} : {jobs[index - 1].name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(LanguageManager.Instance.GetText("Msg_Execute_Fail") + ex.Message);
                }
            }
            else
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_Index"));
            }
        }


        /// <summary>
        /// Execute tous les travailleurs
        /// </summary>
        /// <param name="backupManager"></param>
        static void ExecuteAllJobs(BackupManager backupManager)
        {
            var jobs = backupManager.ListJobs();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4
            };

            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }
            Console.Write(LanguageManager.Instance.GetText("Encryption_Key_Input"));
            string encryptionKey = Console.ReadLine();

            Parallel.For(0, jobs.Count, options, i =>
            {
                try
                {
                    Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Execution")} {jobs[i].name}");
                    backupManager.ExecuteJob(i, encryptionKey);
                    Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Execute_Succes")} : {jobs[i].name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(LanguageManager.Instance.GetText("Msg_Execute_Fail") + ex.Message);
                }
            });
        }

        /// <summary>
        /// Modification des chemins du trvailleur
        /// </summary>
        /// <param name="backupManager"></param>
        static void Modify(BackupManager backupManager)
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            ListJobs(backupManager);
            Console.Write(LanguageManager.Instance.GetText("Prompt_JobNumber"));

            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= jobs.Count)
            {
                Console.Write(LanguageManager.Instance.GetText("Saisie_Source"));
                string sourcePath = Console.ReadLine();
                Console.Write(LanguageManager.Instance.GetText("Saisie_Dest"));
                string targetPath = Console.ReadLine();
                backupManager.ModifyJob(index - 1, sourcePath, targetPath);
                Console.WriteLine(LanguageManager.Instance.GetText("Msg_Modify"));
            }
            else
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_Index"));
            }
        }

        /// <summary>
        /// Supprime un travailleur
        /// </summary>
        /// <param name="backupManager"></param>
        static void DeleteJob(BackupManager backupManager)
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            ListJobs(backupManager);
            Console.Write(LanguageManager.Instance.GetText("Prompt_JobNumber"));

            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= jobs.Count)
            {
                backupManager.DeleteJob(index - 1);
                Console.WriteLine(LanguageManager.Instance.GetText("Msg_Deleted"));
            }
            else
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_Index"));
            }
        }

        /// <summary>
        /// Configure le logiciel métier
        /// </summary>
        /// <param name="backupManager"></param>
        static void ConfigureForbiddenSoftware(BackupManager backupManager)
        {
            Console.WriteLine($"\n{LanguageManager.Instance.GetText("Prompt_ForbiddenSoftware")} ({backupManager.GetForbiddenSoftware()}): ");
            string softwareName = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(softwareName))
            {
                backupManager.SetForbiddenSoftware(softwareName);
                Console.WriteLine(LanguageManager.Instance.GetText("Msg_ForbiddenSoftwareSet"));
            }
        }

        /// <summary>
        /// Exucution CLI
        /// </summary>
        /// <param name="backupManager"></param>
        /// <param name="arg"></param>
        static void ExecuteFromArgs(BackupManager backupManager, string arg)
        {
            var jobs = backupManager.ListJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.Instance.GetText("Err_NoJobs"));
                return;
            }

            List<int> indices = new List<int>();

            // Range format: "1-3"
            if (arg.Contains("-"))
            {
                string[] parts = arg.Split('-');
                if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                        indices.Add(i);
                }
            }
            // List format: "1;3"
            else if (arg.Contains(";"))
            {
                string[] parts = arg.Split(';');
                foreach (string part in parts)
                {
                    if (int.TryParse(part, out int idx))
                        indices.Add(idx);
                }
            }
            // Single format: "2"
            else
            {
                if (int.TryParse(arg, out int idx))
                    indices.Add(idx);
            }

            Console.Write(LanguageManager.Instance.GetText("Encryption_Key_Input"));
            string encryptionKey = Console.ReadLine();

            foreach (int i in indices)
            {
                if (i >= 1 && i <= jobs.Count)
                {
                    try
                    {
                        Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Execution")} {jobs[i - 1].name}");
                        backupManager.ExecuteJob(i - 1, encryptionKey);
                        Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Execute_Succes")} : {jobs[i - 1].name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(LanguageManager.Instance.GetText("Msg_Execute_Fail") + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine(LanguageManager.Instance.GetText("Err_Index"));
                }
            }
        }

    }
}
