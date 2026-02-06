using System;

using EasySave.Models;




namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            //GESTIONNAIRE DE LANGUE
            Console.WriteLine("Choisir la Langue (FR/EN) : ");
            string langChoice = Console.ReadLine();
            LanguageManager.Instance.SetLanguage(langChoice);

            bool sessionActive = true;
            while (sessionActive)
            {
                //Choix du Nombre de travaux
                Console.WriteLine($"\n{LanguageManager.Instance.GetText("Question_NbTravaux")} (1-5) :");
                if (!int.TryParse(Console.ReadLine(), out int totalASaisir) || totalASaisir < 1 || totalASaisir > 5)
                {
                    totalASaisir = 1; // Valeur par défaut si erreur
                }


                //Configurer les travaux
                for (int i = 1; i <= totalASaisir; i++)
                {
                    Console.WriteLine($"\n--- {LanguageManager.Instance.GetText("Travail_Numero")} {i}/{totalASaisir} ---");

                    // Saisie des infos de base
                    Console.Write(LanguageManager.Instance.GetText("Saisie_Nom"));
                    string nom = Console.ReadLine();

                    Console.Write(LanguageManager.Instance.GetText("Saisie_Source"));
                    string source = Console.ReadLine();

                    Console.Write(LanguageManager.Instance.GetText("Saisie_Dest"));
                    string dest = Console.ReadLine();

                    // Validation du chemin source 
                    if (!System.IO.Directory.Exists(source))
                    {
                        Console.WriteLine(LanguageManager.Instance.GetText("Err_Chemin"));
                        i--; // On recommence la saisie pour ce travail
                        continue;
                    }

                    //Choix du type de sauvegarde
                    Console.WriteLine(LanguageManager.Instance.GetText("Question_Type"));
                    Console.WriteLine("1" + LanguageManager.Instance.GetText("Type_Complet"));
                    Console.WriteLine("2" + LanguageManager.Instance.GetText("Type_Diff"));

                    string choix = Console.ReadLine();
                    string choixType;

                    switch (choix)
                    {
                        case "1":
                            choixType = "default";
                            break;
                        case "2":
                            choixType = "diff";
                            break;
                        default:
                            Console.WriteLine("Choix invalide");
                            choixType = "";
                            break;
                    }



                    //Sauvegarde 
                    BackupManager backupManager = new BackupManager();


                    bool success = backupManager.CreateJob(nom, source, dest, choixType);

                    if (success)
                    {
                        int lastIndex = backupManager.ListJobs().Count - 1;
                        Console.WriteLine(LanguageManager.Instance.GetText("Msg_Execution"));
                        Console.WriteLine($"{LanguageManager.Instance.GetText("Msg_Succes")} : {nom}");
                    }
                    else
                    {
                        Console.WriteLine(LanguageManager.Instance.GetText("Err_Quota_Ou_Chemin"));
                        i--;
                    }
                }

                //Nouvelle session
                Console.WriteLine($"\n{LanguageManager.Instance.GetText("Question_NouvelleSession")}");
                string rep = Console.ReadLine()?.ToUpper();
                sessionActive = (rep == "O" || rep == "Y");
            }

            Console.WriteLine(LanguageManager.Instance.GetText("Fin_Prog"));
            Console.ReadKey();
        }

    }
}

