using System;
using System.Collections.Generic;

namespace EasySave.Core.Models
{
    public class LanguageManager
    {
        // 1. L'instance unique et statique
        private static LanguageManager _instance = null;
        private static readonly object _lock = new object();

        // 2. Le dictionnaire de traductions
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        public string CurrentLanguage { get; private set; }

        // 3. CONSTRUCTEUR PRIV� : Emp�che l'utilisation de "new LanguageManager()" � l'ext�rieur
        private LanguageManager() { }

        // 4. PROPRI�T� D'ACC�S : Le point d'entr�e unique
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

        /// <summary>
        /// Méthode de définition de langues
        /// </summary>
        /// <param name="langue"></param>
        public void SetLanguage(string langue)
        {
            CurrentLanguage = langue.ToUpper();
            _translations.Clear();

            if (CurrentLanguage == "FR")
            {
                _translations.Add("lang_FR", "Français");
                _translations.Add("lang_EN", "Anglais");
                _translations.Add("Btn_Start", "Commencer");
                _translations.Add("Btn_Cancel", "Annuler");
                _translations.Add("Btn_Validate", "Valider");
                _translations.Add("Menu_Titre", "--- Menu Principal ---");
                _translations.Add("Travail_Numero", "Travail n�");
                _translations.Add("Saisie_Nom", "Nom du travail : ");
                _translations.Add("Saisie_Source", "Répertoire Source : ");
                _translations.Add("Saisie_Dest", "Répertoire Destination : ");
                _translations.Add("Question_NbTravaux", "Combien de travaux voulez-vous faire ?");
                _translations.Add("Question_Type", "Choisissez le type de sauvegarde :");
                _translations.Add("Type_Complet", "1. Sauvegarde Compl�te");
                _translations.Add("Type_Diff", "2. Sauvegarde Diff�rentielle");
                _translations.Add("Msg_Execution", "Sauvegarde en cours...");
                _translations.Add("Msg_Succes", "Le travail a été sauvegardé avec succès");
                _translations.Add("Err_Job_Ou_Chemin", "ERREUR : Chemin invalide ou travail.");
                _translations.Add("Question_NouvelleSession", "Voulez-vous cr�er une nouvelle session de sauvegarde ? (O/N)");
                _translations.Add("Fin_Prog", "Fin du programme.");
                _translations.Add("Menu_Create", "1. Cr�er un travail de sauvegarde");
                _translations.Add("Menu_List", "2. Lister les travaux");
                _translations.Add("Menu_Execute", "3. Ex�cuter un travail");
                _translations.Add("Menu_ExecuteAll", "4. Ex�cuter tous les travaux");
                _translations.Add("Menu_Delete", "5. Supprimer un travail");
                _translations.Add("Menu_Quit", "6. Quitter");
                _translations.Add("Menu_Choice", "Votre choix : ");
                _translations.Add("Menu_Invalid", "Choix invalide.");
                _translations.Add("Msg_Execute_Succes", "Sauvegarde termin�e avec succ�s");
                _translations.Add("Msg_Execute_Fail", "Erreur lors de la sauvegarde : ");
                _translations.Add("Msg_Modify", "Travail modifié avec succ�s.");
                _translations.Add("Msg_Deleted", "Travail supprim� avec succ�s.");
                _translations.Add("Err_Chemin", "ERREUR : Le r�pertoire source n'existe pas.");
                _translations.Add("Err_Create", "ERREUR : Le travail n'a pas pu etre cree");
                _translations.Add("Err_Index", "ERREUR : Num�ro de travail invalide.");
                _translations.Add("Err_NoJobs", "Aucun travail de sauvegarde.");
                _translations.Add("List_Header", "--- Liste des travaux ---");
                _translations.Add("List_Name", "Nom : ");
                _translations.Add("List_Source", "Source : ");
                _translations.Add("List_Target", "Destination : ");
                _translations.Add("Prompt_JobNumber", "Numéro du travail : ");
                _translations.Add("Prompt_ForbiddenSoftware", "Nom du logiciel metier (ex: Calculator), actuel : ");
                _translations.Add("Msg_ForbiddenSoftwareBlocked", "Sauvegarde bloquee - logiciel metier en cours d'execution : ");
                _translations.Add("Msg_ForbiddenSoftwareSet", "Logiciel metier configure avec succes.");
                _translations.Add("Encryption_Key_Input","Entrer une clé de chiffremenr (optionnel) : ");
            }
            else
            {
                _translations.Add("lang_EN", "English");
                _translations.Add("lang_FR", "French");
                _translations.Add("Btn_Start", "Start");
                _translations.Add("Btn_Cancel", "Cancel");
                _translations.Add("Btn_Validate", "Validate");
                _translations.Add("Menu_Titre", "--- Main Menu ---");
                _translations.Add("Travail_Numero", "Job n�");
                _translations.Add("Saisie_Nom", "Job Name: ");
                _translations.Add("Saisie_Source", "Source Directory: ");
                _translations.Add("Saisie_Dest", "Destination Directory: ");
                _translations.Add("Question_NbTravaux", "How many jobs do you want to create?");
                _translations.Add("Question_Type", "Choose the backup type:");
                _translations.Add("Type_Complet", "1. Full Backup");
                _translations.Add("Type_Diff", "2. Differential Backup");
                _translations.Add("Msg_Execution", "Backup in progress...");
                _translations.Add("Msg_Succes", "The job has been successfully saved");
                _translations.Add("Err_Quota_Ou_Chemin", "ERROR: Invalid path or The job could not be created.");
                _translations.Add("Question_NouvelleSession", "Do you want to create a new backup session? (Y/N)");
                _translations.Add("Fin_Prog", "End of program.");
                _translations.Add("Lang_Choice", "Choose Language (FR/EN): ");
                _translations.Add("Menu_Create", "1. Create a backup job");
                _translations.Add("Menu_List", "2. List backup jobs");
                _translations.Add("Menu_Execute", "3. Execute a backup job");
                _translations.Add("Menu_ExecuteAll", "4. Execute all backup jobs");
                _translations.Add("Menu_Delete", "5. Delete a backup job");
                _translations.Add("Menu_Quit", "6. Quit");
                _translations.Add("Menu_Choice", "Your choice: ");
                _translations.Add("Menu_Invalid", "Invalid choice.");
                _translations.Add("Msg_Execute_Succes", "Backup completed successfully");
                _translations.Add("Msg_Execute_Fail", "Error during backup: ");
                _translations.Add("Msg_Modify", "Job modified successfully.");
                _translations.Add("Msg_Deleted", "Job deleted successfully.");
                _translations.Add("Err_Chemin", "ERROR: Source directory does not exist.");
                _translations.Add("Err_Create", "ERROR: The job could not be created.");
                _translations.Add("Err_Index", "ERROR: Invalid job number.");
                _translations.Add("Err_NoJobs", "No backup jobs found.");
                _translations.Add("List_Header", "--- Backup Jobs ---");
                _translations.Add("List_Name", "Name: ");
                _translations.Add("List_Source", "Source: ");
                _translations.Add("List_Target", "Destination: ");
                _translations.Add("Prompt_JobNumber", "Job number: ");
                _translations.Add("Prompt_ForbiddenSoftware", "Forbidden software name (e.g. Calculator), ");
                _translations.Add("Msg_ForbiddenSoftwareBlocked", "Backup blocked - forbidden software running: ");
                _translations.Add("Msg_ForbiddenSoftwareSet", "Forbidden software configured successfully.");
                _translations.Add("Encryption_Key_Input","Enter encryption key (leave empty to skip): ");
            }
        }

        /// <summary>
        /// Récupère le texte
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetText(string key)
        {
            return _translations.ContainsKey(key) ? _translations[key] : key;
        }
    }
}