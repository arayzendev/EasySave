using System;
using System.Collections.Generic;

namespace EasySave.Models
{
    public class LanguageManager
    {
        // 1. L'instance unique et statique
        private static LanguageManager _instance = null;
        private static readonly object _lock = new object();

        // 2. Le dictionnaire de traductions
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        public string CurrentLanguage { get; private set; }

        // 3. CONSTRUCTEUR PRIVÉ : Empêche l'utilisation de "new LanguageManager()" à l'extérieur
        private LanguageManager() { }

        // 4. PROPRIÉTÉ D'ACCÈS : Le point d'entrée unique
        public static LanguageManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LanguageManager();
                    }
                    return _instance;
                }
            }
        }

        public void SetLanguage(string langue)
        {
            CurrentLanguage = langue.ToUpper();
            _translations.Clear();

            if (CurrentLanguage == "FR")
            {
                _translations.Add("Menu_Titre", "--- Paramétrage du Travail ---");
                _translations.Add("Travail_Numero", "Travail n°");
                _translations.Add("Saisie_Nom", "Nom du travail : ");
                _translations.Add("Saisie_Source", "Répertoire Source : ");
                _translations.Add("Saisie_Dest", "Répertoire Destination : ");
                _translations.Add("Question_NbTravaux", "Combien de travaux voulez-vous faire ?");
                _translations.Add("Question_Type", "Choisissez le type de sauvegarde :");
                _translations.Add("Type_Complet", "Sauvegarde Complète");
                _translations.Add("Type_Diff", "Sauvegarde Différentielle");
                _translations.Add("Msg_Execution", "Sauvegarde en cours...");
                _translations.Add("Msg_Succes", "Le travail a été sauvegardé avec succès");
                _translations.Add("Err_Quota_Ou_Chemin", "ERREUR : Chemin invalide ou quota de 5 travaux atteint.");
                _translations.Add("Question_NouvelleSession", "Voulez-vous créer une nouvelle session de sauvegarde ? (O/N)");
                _translations.Add("Fin_Prog", "Fin du programme. Appuyez sur une touche pour quitter.");
                // ... les autres traductions FR
            }
            else
            {
                _translations.Add("Menu_Titre", "--- Job Configuration ---");
                _translations.Add("Travail_Numero", "Job n°");
                _translations.Add("Saisie_Nom", "Job Name: ");
                _translations.Add("Saisie_Source", "Source Directory: ");
                _translations.Add("Saisie_Dest", "Destination Directory: ");
                _translations.Add("Question_NbTravaux", "How many jobs do you want to create?");
                _translations.Add("Question_Type", "Choose the backup type:");
                _translations.Add("Type_Complet", "Full Backup");
                _translations.Add("Type_Diff", "Differential Backup");
                _translations.Add("Msg_Execution", "Backup in progress...");
                _translations.Add("Msg_Succes", "The job has been successfully saved");
                _translations.Add("Err_Quota_Ou_Chemin", "ERROR: Invalid path or job quota (5) reached.");
                _translations.Add("Question_NouvelleSession", "Do you want to create a new backup session? (Y/N)");
                _translations.Add("Fin_Prog", "End of program. Press any key to exit.");
                // ... les autres traductions EN
            }
        }

        public string GetText(string key)
        {
            return _translations.ContainsKey(key) ? _translations[key] : key;
        }
    }
}