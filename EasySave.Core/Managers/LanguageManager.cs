namespace EasySave.Core.Managers
{
    public class LanguageManager
    {
        // 1. L'instance unique et statique (Singleton)
        private static LanguageManager _instance = null;
        private static readonly object _lock = new object();

        // 2. Le dictionnaire de traductions
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        public string CurrentLanguage { get; private set; }

        // 3. CONSTRUCTEUR PRIVÉ
        private LanguageManager()
        {
            // Langue par défaut à l'initialisation
            SetLanguage("FR");
        }

        // 4. PROPRIÉTÉ D'ACCÈS
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
                //  INTERFACE GENERALe
                _translations.Add("lang_FR", "Français");
                _translations.Add("lang_EN", "Anglais");
                _translations.Add("Btn_Start", "Commencer");
                _translations.Add("Btn_Cancel", "ANNULER");
                _translations.Add("Btn_Validate", "VALIDER");
                _translations.Add("Menu_Titre", "--- TABLEAU DE BORD ---");

                // ACTIONS DASHBOARD
                _translations.Add("Menu_Create", " 1.Créer un travail de sauvegarde");
                _translations.Add("Menu_ExecuteAll", "Tout Exécuter");
                _translations.Add("Btn_Edit", "ÉDITER");
                _translations.Add("Btn_Delete", "SUPPRIMER");
                _translations.Add("Label_Progress", "PROGRESSION SAUVEGARDE");

                // PARTITIONNEMENT DASHBOARD
                _translations.Add("Label_ActiveMissions", "MISSIONS ACTIVES & EN ATTENTE");
                _translations.Add("Label_History", "HISTORIQUE & ARCHIVES");

                // ETATS DE SAUVEGARDE
                _translations.Add("State_Inactive", "INACTIF");
                _translations.Add("State_Active", "EN COURS");
                _translations.Add("State_Paused", "EN PAUSE");
                _translations.Add("State_Stopped", "ARRÊTÉ");
                _translations.Add("State_Ended", "TERMINÉ");
                _translations.Add("State_Failed", "ÉCHEC");
                _translations.Add("State_Success", "RÉUSSI");

                // JOB EDITOR LABELS
                _translations.Add("Label_Name", "NOM DU TRAVAIL");
                _translations.Add("Label_Source", "RÉPERTOIRE SOURCE");
                _translations.Add("Label_Target", "RÉPERTOIRE CIBLE");
                _translations.Add("Label_Crypto", "CLÉ CRYPTOSOFT");
                _translations.Add("Label_Strategy", "STRATÉGIE");

                //  JOB EDITOR DESCRIPTIONS
                _translations.Add("Desc_JobName", "Identifiant unique du profil de sauvegarde");
                _translations.Add("Desc_Source", "Répertoire d'origine des fichiers à copier");
                _translations.Add("Desc_Target", "Emplacement de stockage final sécurisé");
                _translations.Add("Desc_Crypto", "Clé AES pour le chiffrement CryptoSoft");
                _translations.Add("Desc_Strategy", "Méthode de transfert des données");

                //STRATEGIES 
                _translations.Add("Strategy_Full", "Sauvegarde Complète");
                _translations.Add("Strategy_Diff", "Sauvegarde Différentielle");

                // HOME & SETTINGS
                _translations.Add("Home_Version", "INFRASTRUCTURE ENTREPRISE v3.0");
                _translations.Add("Home_GeneralTitle", "PARAMÈTRES GÉNÉRAUX");
                _translations.Add("Home_LangLabel", "Langue de l'interface");
                _translations.Add("Home_LogLabel", "Format des logs de sortie");
                _translations.Add("Home_SecurityTitle", "SÉCURITÉ DE SAUVEGARDE");
                _translations.Add("Home_SoftwareLabel", "Détection logiciel métier");
                _translations.Add("Home_ExtensionsLabel", "Extensions prioritaires");
                _translations.Add("Home_Footer", "Prosoft Fevrier 2026 © ");
                _translations.Add("Home_SoftwareWatermark", "ex: Calculator, chrome.exe...");
                _translations.Add("Home_ExtensionsWatermark", ".pdf; .db; .rar");

                // MENU CLI
                _translations.Add("Menu_List", "2. Lister les travaux");
                _translations.Add("Menu_Execute", "3. Exécuter un travail");
                _translations.Add("Menu_Modify", "5. Modifier un travail");
                _translations.Add("Menu_Delete", "6. Supprimer un travail");
                _translations.Add("Menu_ForbiddenSoftware", "7. Configurer le logiciel métier");
                _translations.Add("Menu_Quit", "8. Quitter");
                _translations.Add("Menu_Choice", "Votre choix : ");
                _translations.Add("Menu_Invalid", "Choix invalide.");

                // SAISIE
                _translations.Add("Saisie_Nom", "Nom du travail : ");
                _translations.Add("Saisie_Source", "Répertoire Source : ");
                _translations.Add("Saisie_Dest", "Répertoire Destination : ");
                _translations.Add("Question_Type", "Choisissez le type de sauvegarde :");
                _translations.Add("Type_Complet", "1. Sauvegarde Complète");
                _translations.Add("Type_Diff", "2. Sauvegarde Différentielle");

                // PROMPTS
                _translations.Add("Prompt_JobNumber", "Numéro du travail : ");
                _translations.Add("Encryption_Key_Input", "Entrer une clé de chiffrement (optionnel) : ");
                _translations.Add("Prompt_ForbiddenSoftware", "Nom du logiciel métier (ex: Calculator), actuel : ");

                // --- MESSAGES LOGS & ERRREURS ---
                _translations.Add("Msg_Succes", "Le travail a été sauvegardé avec succès");
                _translations.Add("Msg_Execution", "Sauvegarde en cours...");
                _translations.Add("Msg_Execute_Succes", "Sauvegarde terminée avec succès");
                _translations.Add("Msg_Execute_Fail", "Erreur lors de la sauvegarde : ");
                _translations.Add("Msg_Modify", "Travail modifié avec succès.");
                _translations.Add("Msg_Deleted", "Travail supprimé avec succès.");
                _translations.Add("Msg_ForbiddenSoftwareSet", "Logiciel métier configuré avec succès.");
                _translations.Add("Msg_QuotaFull", "Limite de 5 travaux atteinte.");
                _translations.Add("Err_Chemin", "ERREUR : Le répertoire source n'existe pas.");
                _translations.Add("Err_Create", "ERREUR : Le travail n'a pas pu être créé");
                _translations.Add("Err_Index", "ERREUR : Numéro de travail invalide.");
                _translations.Add("Err_NoJobs", "Aucun travail de sauvegarde.");
                _translations.Add("Fin_Prog", "Fin du programme.");

                // LIST
                _translations.Add("List_Header", "--- Liste des travaux ---");
                _translations.Add("List_Name", "Nom : ");
                _translations.Add("List_Source", "Source : ");
                _translations.Add("List_Target", "Destination : ");
            }
            else
            {
                // --- GENERAL INTERFACE ---
                _translations.Add("lang_EN", "English");
                _translations.Add("lang_FR", "French");
                _translations.Add("Btn_Start", "Start");
                _translations.Add("Btn_Cancel", "CANCEL");
                _translations.Add("Btn_Validate", "VALIDATE");
                _translations.Add("Menu_Titre", "--- DASHBOARD ---");

                // --- DASHBOARD ACTIONS ---
                _translations.Add("Menu_Create", "Create a backup job");
                _translations.Add("Menu_ExecuteAll", "Execute All");
                _translations.Add("Btn_Edit", "EDIT");
                _translations.Add("Btn_Delete", "DELETE");
                _translations.Add("Label_Progress", "BACKUP PROGRESS");

                // --- DASHBOARD PARTITIONING ---
                _translations.Add("Label_ActiveMissions", "ACTIVE & PENDING MISSIONS");
                _translations.Add("Label_History", "HISTORY & ARCHIVES");

                // --- BACKUP STATES ---
                _translations.Add("State_Inactive", "INACTIVE");
                _translations.Add("State_Active", "ACTIVE");
                _translations.Add("State_Paused", "PAUSED");
                _translations.Add("State_Stopped", "STOPPED");
                _translations.Add("State_Ended", "FINISHED");
                _translations.Add("State_Failed", "FAILED");
                _translations.Add("State_Success", "SUCCESS");

                // --- JOB EDITOR LABELS ---
                _translations.Add("Label_Name", "JOB NAME");
                _translations.Add("Label_Source", "SOURCE DIRECTORY");
                _translations.Add("Label_Target", "TARGET DIRECTORY");
                _translations.Add("Label_Crypto", "CRYPTOSOFT KEY");
                _translations.Add("Label_Strategy", "STRATEGY");

                // --- JOB EDITOR DESCRIPTIONS ---
                _translations.Add("Desc_JobName", "Unique identifier for the backup profile");
                _translations.Add("Desc_Source", "Original directory of the files to copy");
                _translations.Add("Desc_Target", "Final secure storage location");
                _translations.Add("Desc_Crypto", "AES key for CryptoSoft encryption");
                _translations.Add("Desc_Strategy", "Data transfer method");

                // --- STRATEGIES ---
                _translations.Add("Strategy_Full", "Full Backup");
                _translations.Add("Strategy_Diff", "Differential Backup");

                // --- HOME & SETTINGS ---
                _translations.Add("Home_Version", "ENTERPRISE INFRASTRUCTURE v3.0");
                _translations.Add("Home_GeneralTitle", "GENERAL SETTINGS");
                _translations.Add("Home_LangLabel", "Interface Language");
                _translations.Add("Home_LogLabel", "Log Output Format");
                _translations.Add("Home_SecurityTitle", "BACKUP SECURITY");
                _translations.Add("Home_SoftwareLabel", "Business Software Detection");
                _translations.Add("Home_ExtensionsLabel", "High Priority Extensions");
                _translations.Add("Home_Footer", "Prosoft February 2026 © ");
                _translations.Add("Home_SoftwareWatermark", "e.g. Calculator, chrome.exe...");
                _translations.Add("Home_ExtensionsWatermark", ".pdf; .db; .rar");

                // --- MENU CLI ---
                _translations.Add("Menu_List", "2. List backup jobs");
                _translations.Add("Menu_Execute", "3. Execute a backup job");
                _translations.Add("Menu_Modify", "5. Modify a backup job");
                _translations.Add("Menu_Delete", "6. Delete a backup job");
                _translations.Add("Menu_ForbiddenSoftware", "7. Configure forbidden software");
                _translations.Add("Menu_Quit", "8. Quit");
                _translations.Add("Menu_Choice", "Your choice: ");
                _translations.Add("Menu_Invalid", "Invalid choice.");

                // --- INPUT ---
                _translations.Add("Saisie_Nom", "Job Name: ");
                _translations.Add("Saisie_Source", "Source Directory: ");
                _translations.Add("Saisie_Dest", "Destination Directory: ");
                _translations.Add("Question_Type", "Choose the backup type:");
                _translations.Add("Type_Complet", "1. Full Backup");
                _translations.Add("Type_Diff", "2. Differential Backup");

                // --- PROMPTS ---
                _translations.Add("Prompt_JobNumber", "Job number: ");
                _translations.Add("Encryption_Key_Input", "Enter encryption key (leave empty to skip): ");
                _translations.Add("Prompt_ForbiddenSoftware", "Forbidden software name (e.g. Calculator), current: ");

                // --- LOGS & ERRORS MESSAGES ---
                _translations.Add("Msg_Succes", "The job has been successfully saved");
                _translations.Add("Msg_Execution", "Backup in progress...");
                _translations.Add("Msg_Execute_Succes", "Backup completed successfully");
                _translations.Add("Msg_Execute_Fail", "Error during backup: ");
                _translations.Add("Msg_Modify", "Job modified successfully.");
                _translations.Add("Msg_Deleted", "Job deleted successfully.");
                _translations.Add("Msg_ForbiddenSoftwareSet", "Forbidden software configured successfully.");
                _translations.Add("Msg_QuotaFull", "Job limit (5) reached.");
                _translations.Add("Err_Chemin", "ERROR: Source directory does not exist.");
                _translations.Add("Err_Create", "ERROR: The job could not be created.");
                _translations.Add("Err_Index", "ERROR: Invalid job number.");
                _translations.Add("Err_NoJobs", "No backup jobs found.");
                _translations.Add("Fin_Prog", "End of program.");

                // --- LIST ---
                _translations.Add("List_Header", "--- Backup Jobs ---");
                _translations.Add("List_Name", "Name: ");
                _translations.Add("List_Source", "Source: ");
                _translations.Add("List_Target", "Destination: ");
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