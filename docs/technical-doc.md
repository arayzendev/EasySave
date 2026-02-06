# EasySave v1.0 - Documentation Technique

> Document a destination du support client ProSoft.

---

## Table des matieres

1. [Configuration minimale requise](#1-configuration-minimale-requise)
2. [Installation et emplacement par defaut](#2-installation-et-emplacement-par-defaut)
3. [Architecture du projet](#3-architecture-du-projet)
4. [Structure des repertoires du projet](#4-structure-des-repertoires-du-projet)
5. [Design patterns utilises](#5-design-patterns-utilises)
6. [Fichiers de configuration et de donnees](#6-fichiers-de-configuration-et-de-donnees)
7. [Bibliotheque EasyLog (DLL)](#7-bibliotheque-easylog-dll)
8. [Fonctionnement interne](#8-fonctionnement-interne)
9. [Depannage](#9-depannage)

---

## 1. Configuration minimale requise

| Element                  | Exigence                                         |
|--------------------------|--------------------------------------------------|
| Systeme d'exploitation   | Windows 10 (64 bits) ou superieur                |
| Framework                | .NET 8.0 Runtime                                 |
| Espace disque            | 50 Mo minimum pour l'application                 |
| RAM                      | 512 Mo minimum                                   |
| Droits                   | Lecture/ecriture sur les repertoires source/cible |
| Reseau (optionnel)       | Acces aux lecteurs reseau si utilises comme source ou cible |

---

## 2. Installation et emplacement par defaut

### Installation

EasySave est distribue sous forme d'un fichier executable portable (`EasySave.exe`). Aucun installateur n'est necessaire.

**Emplacement recommande** :

```
C:\Program Files\ProSoft\EasySave\
```

### Structure apres installation

```
<repertoire d'installation>/
    EasySave.exe            # Executable principal
    EasyLog.dll             # Bibliotheque de journalisation
```

### Fichiers generes a l'execution

Au premier lancement, le logiciel cree automatiquement le repertoire de donnees suivant :

```
<repertoire d'installation>/EasySave/
    config.json             # Configuration des travaux de sauvegarde
    state.json              # Etat temps reel des sauvegardes
    Logs/
        yyyy-MM-dd.json     # Fichier log journalier (un par jour)
```

> **Important** : Les fichiers de donnees sont stockes relativement au repertoire de l'executable (`AppDomain.CurrentDomain.BaseDirectory`), et non dans un chemin absolu. Cela garantit un fonctionnement correct sur les serveurs clients, quel que soit le disque d'installation.

---

## 3. Architecture du projet

Le projet est compose de deux assemblies :

### EasySave (Application console)

L'application principale, point d'entree du logiciel. Elle contient toute la logique metier : gestion des travaux de sauvegarde, strategies de copie, gestion de la configuration et du suivi en temps reel.

- **Type** : Application console (.NET 8.0)
- **Sortie** : `EasySave.exe`

### EasyLog (Bibliotheque)

Bibliotheque de journalisation concue pour etre reutilisable par d'autres projets de la suite ProSoft. Elle est responsable de l'ecriture des fichiers log journaliers au format JSON.

- **Type** : Bibliotheque de classes (.NET 8.0)
- **Sortie** : `EasyLog.dll`

---

## 4. Structure des repertoires du projet

```
EasySave/                          # Solution
|
+-- EasySave/                      # Projet principal (application console)
|   +-- Program.cs                 # Point d'entree, menu console, parsing CLI
|   +-- Models/
|   |   +-- BackupJob.cs           # Modele d'un travail de sauvegarde
|   |   +-- BackupProgress.cs      # Suivi de progression en temps reel
|   |   +-- BackupState.cs         # Enumeration des etats (Active, Inactive, Ended, Failed)
|   +-- Interfaces/
|   |   +-- IBackupStrategy.cs     # Interface du pattern Strategy
|   +-- Strategies/
|   |   +-- FullBackupStrategy.cs          # Sauvegarde complete
|   |   +-- DifferentialBackupStrategy.cs  # Sauvegarde differentielle
|   +-- Factory/
|   |   +-- BackupStrategyFactory.cs       # Factory pour creer les strategies
|   +-- Managers/
|       +-- BackupManager.cs       # Orchestrateur central
|       +-- ConfigManager.cs       # Persistance de la configuration (config.json)
|       +-- StateManager.cs        # Ecriture de l'etat temps reel (state.json)
|
+-- EasyLog/                       # Projet DLL (bibliotheque de journalisation)
|   +-- Logger.cs                  # Classe principale de journalisation
|   +-- LogEntry.cs                # Modele d'une entree de log
|
+-- docs/                          # Documentation
    +-- class-diagram.md           # Diagramme de classes (Mermaid)
    +-- sequence-diagram.md        # Diagramme de sequence (Mermaid)
    +-- class-diagram-explanation.md
    +-- sequence-diagram-explanation.md
    +-- user-manual.md             # Manuel utilisateur
    +-- technical-doc.md           # Ce document
```

---

## 5. Design patterns utilises

### Pattern Strategy

Permet d'interchanger l'algorithme de sauvegarde sans modifier le code appelant.

- **Interface** : `IBackupStrategy` - definit la methode `Save(sourcePath, targetPath, backupProgress, onProgressUpdate)`
- **Implementations** :
  - `FullBackupStrategy` : copie integrale de tous les fichiers
  - `DifferentialBackupStrategy` : copie uniquement les fichiers modifies depuis la derniere sauvegarde complete

Chaque `BackupJob` possede une reference vers une `IBackupStrategy`, ce qui permet de choisir le type de sauvegarde a la creation du travail.

### Pattern Factory

`BackupStrategyFactory` centralise la creation des strategies de sauvegarde a partir d'une chaine de caracteres (`"full"` ou `"differential"`), isolant la logique d'instanciation.

### Architecture Manager

Les classes `BackupManager`, `ConfigManager` et `StateManager` suivent une architecture de type gestionnaire avec separation des responsabilites :

- `BackupManager` : orchestration (creation, suppression, modification, execution des travaux)
- `ConfigManager` : persistance de la configuration
- `StateManager` : ecriture de l'etat en temps reel

---

## 6. Fichiers de configuration et de donnees

### 6.1 config.json

**Emplacement** : `<repertoire d'installation>/EasySave/config.json`

**Role** : Stocke la liste des travaux de sauvegarde configures par l'utilisateur. Ce fichier est lu au demarrage et mis a jour a chaque creation, modification ou suppression d'un travail.

**Format** :

```json
[
  {
    "Name": "Sauvegarde Documents",
    "SourcePath": "C:\\Users\\Jean\\Documents",
    "TargetPath": "D:\\Backups\\Documents",
    "BackupStrategy": "full"
  },
  {
    "Name": "Sauvegarde Photos",
    "SourcePath": "\\\\serveur\\partage\\photos",
    "TargetPath": "E:\\Backups\\Photos",
    "BackupStrategy": "differential"
  }
]
```

**Contraintes** : Maximum 5 travaux de sauvegarde.

---

### 6.2 state.json

**Emplacement** : `<repertoire d'installation>/EasySave/state.json`

**Role** : Fichier d'etat temps reel, mis a jour apres chaque fichier copie. Permet de suivre l'avancement des sauvegardes en cours depuis un outil externe.

**Format** :

```json
[
  {
    "Name": "Sauvegarde Documents",
    "SourcePath": "C:\\Users\\Jean\\Documents",
    "TargetPath": "D:\\Backups\\Documents",
    "BackupProgress": {
      "DateTime": "2026-02-06T14:30:15",
      "State": "Active",
      "TotalFiles": 150,
      "TotalSize": 52428800,
      "FileSize": 15234,
      "Progress": 45.5,
      "TransferTime": 12,
      "RemainingFiles": 82,
      "RemainingSize": 28835840,
      "SourceFilePath": "C:\\Users\\Jean\\Documents\\rapport.docx",
      "TargetFilePath": "D:\\Backups\\Documents\\rapport.docx"
    }
  },
  {
    "Name": "Sauvegarde Photos",
    "SourcePath": "\\\\serveur\\partage\\photos",
    "TargetPath": "E:\\Backups\\Photos",
    "BackupProgress": {
      "DateTime": null,
      "State": "Inactive",
      "TotalFiles": 0,
      "TotalSize": 0,
      "FileSize": 0,
      "Progress": 0,
      "TransferTime": 0,
      "RemainingFiles": 0,
      "RemainingSize": 0,
      "SourceFilePath": null,
      "TargetFilePath": null
    }
  }
]
```

**Etats possibles** (`BackupState`) :

| Etat       | Description                                              |
|------------|----------------------------------------------------------|
| `Active`   | La sauvegarde est en cours d'execution                   |
| `Inactive` | Le travail est configure mais n'est pas en cours         |
| `Ended`    | La sauvegarde s'est terminee avec succes                 |
| `Failed`   | La sauvegarde a echoue (erreur d'acces, disque plein...) |

---

### 6.3 Fichier log journalier

**Emplacement** : `<repertoire d'installation>/EasySave/Logs/yyyy-MM-dd.json`

**Role** : Journalise toutes les actions de sauvegarde effectuees durant une journee. Un nouveau fichier est cree chaque jour. Ce fichier est gere par la bibliotheque `EasyLog.dll`.

**Nom du fichier** : La date du jour au format `yyyy-MM-dd.json` (exemple : `2026-02-06.json`).

**Format** :

```json
{
  "Timestamp": "2026-02-06T14:30:12",
  "Application": "EasySave",
  "Data": {
    "BackupName": "Sauvegarde Documents",
    "SourcePath": "C:\\Users\\Jean\\Documents\\rapport.docx",
    "TargetPath": "D:\\Backups\\Documents\\rapport.docx",
    "FileSize": 15234,
    "TransferTime": 12
  }
}
{
  "Timestamp": "2026-02-06T14:30:13",
  "Application": "EasySave",
  "Data": {
    "BackupName": "Sauvegarde Documents",
    "SourcePath": "C:\\Users\\Jean\\Documents\\notes.txt",
    "TargetPath": "D:\\Backups\\Documents\\notes.txt",
    "FileSize": 1024,
    "TransferTime": 2
  }
}
```

> **Note** : Chaque entree JSON est separee par un retour a la ligne pour faciliter la lecture avec un editeur de texte (Notepad).

**Champs de l'entree de log** :

| Champ          | Type     | Description                                                 |
|----------------|----------|-------------------------------------------------------------|
| `Timestamp`    | DateTime | Horodatage de l'action                                      |
| `Application`  | string   | Nom de l'application emettrice (`"EasySave"`)               |
| `Data`         | object   | Donnees de l'action (voir sous-champs ci-dessous)           |

**Sous-champs de `Data`** :

| Champ          | Type   | Description                                                   |
|----------------|--------|---------------------------------------------------------------|
| `BackupName`   | string | Nom du travail de sauvegarde                                  |
| `SourcePath`   | string | Chemin complet du fichier source (format UNC si reseau)       |
| `TargetPath`   | string | Chemin complet du fichier de destination (format UNC si reseau)|
| `FileSize`     | long   | Taille du fichier en octets                                   |
| `TransferTime` | float  | Temps de transfert en millisecondes (negatif si erreur)       |

---

## 7. Bibliotheque EasyLog (DLL)

### Presentation

`EasyLog.dll` est une bibliotheque de journalisation concue pour etre reutilisable par d'autres projets de la suite ProSoft. Elle est developpee dans un projet separe au sein de la solution, et peut etre referenceee independamment.

### Classes

#### Logger

Classe principale de la bibliotheque. Responsable de l'ecriture des entrees de log dans des fichiers journaliers.

| Membre                         | Description                                                       |
|--------------------------------|-------------------------------------------------------------------|
| `Logger(string logDirectory)`  | Constructeur. Cree le repertoire de logs s'il n'existe pas.       |
| `void Write(LogEntry entry)`   | Ecrit une entree dans le fichier log du jour (`yyyy-MM-dd.json`). |

#### LogEntry

Modele de donnees representant une entree de log.

| Propriete                            | Type                         | Description                       |
|--------------------------------------|------------------------------|-----------------------------------|
| `Timestamp`                          | `DateTime?`                  | Horodatage (rempli automatiquement par `Logger.Write`) |
| `Application`                        | `string`                     | Nom de l'application emettrice    |
| `Data`                               | `Dictionary<string, object>` | Donnees cle-valeur de l'action    |

### Integration avec EasySave

Les strategies de sauvegarde (`FullBackupStrategy`, `DifferentialBackupStrategy`) appellent `Logger.Write()` apres chaque copie de fichier reussie, en transmettant les informations de la copie (nom du travail, chemins, taille, temps de transfert).

---

## 8. Fonctionnement interne

### Cycle de vie d'une sauvegarde

1. **Demarrage** : `BackupManager.ExecuteJob(index)` est appele (via le menu ou la ligne de commande).
2. **Delegation** : `BackupJob.Execute()` appelle `IBackupStrategy.Save()` avec les chemins, le modele de progression et le callback de mise a jour.
3. **Initialisation** : La strategie cree le repertoire cible si necessaire, enumere tous les fichiers (et sous-repertoires) du repertoire source, et initialise les compteurs de progression.
4. **Copie** : Pour chaque fichier :
   - Le fichier est copie de la source vers la cible
   - Le modele `BackupProgress` est mis a jour (fichiers restants, taille, progression, temps de transfert)
   - Le callback `onProgressUpdate` est invoque
   - `BackupManager.OnProgressUpdate()` declenche `StateManager.Write()` pour mettre a jour `state.json`
   - `Logger.Write()` (via EasyLog) enregistre l'action dans le fichier log journalier
5. **Fin** : L'etat passe a `Ended` (succes) ou `Failed` (erreur).

### Sauvegarde complete vs differentielle

| Aspect               | Sauvegarde complete             | Sauvegarde differentielle                          |
|-----------------------|---------------------------------|----------------------------------------------------|
| Fichiers copies       | Tous les fichiers               | Uniquement les fichiers modifies depuis la derniere sauvegarde complete |
| Critere de selection  | Aucun (copie tout)              | Comparaison de la date de derniere modification    |
| Temps d'execution     | Plus long                       | Plus rapide (apres la premiere execution)          |
| Espace disque         | Plus important                  | Plus reduit                                        |

### Gestion des arguments en ligne de commande

Le programme supporte deux syntaxes pour l'execution automatique :

| Syntaxe       | Signification                                  | Exemple                  |
|---------------|-------------------------------------------------|--------------------------|
| `X-Y`         | Executer les travaux de X a Y (inclus)          | `EasySave.exe 1-3`      |
| `X;Y;Z`       | Executer les travaux X, Y et Z                 | `EasySave.exe 1;3`      |

Le parsing des arguments est effectue dans `Program.Main(string[] args)`. Si des arguments sont fournis, les sauvegardes sont executees sequentiellement sans afficher le menu interactif.

---

## 9. Depannage

### Problemes courants

| Symptome                                  | Cause probable                                   | Resolution                                                |
|-------------------------------------------|--------------------------------------------------|-----------------------------------------------------------|
| Le fichier `config.json` est vide ou absent | Premier lancement ou fichier supprime           | Le fichier est recree automatiquement au prochain lancement |
| Erreur "Repertoire source introuvable"    | Chemin invalide ou lecteur reseau deconnecte     | Verifier le chemin et la connectivite reseau              |
| La sauvegarde echoue immediatement        | Droits insuffisants sur le repertoire cible       | Verifier les permissions en ecriture                      |
| Le fichier log n'est pas cree            | Repertoire de logs inaccessible                   | Verifier les permissions sur le repertoire `Logs/`        |
| `state.json` ne se met pas a jour        | Erreur de serialisation ou fichier verrouille     | Verifier qu'aucun autre processus ne verrouille le fichier |
| Le logiciel ne se lance pas              | .NET 8.0 Runtime non installe                     | Installer le .NET 8.0 Runtime depuis https://dotnet.microsoft.com |

### Reinitialisation

Pour reinitialiser completement le logiciel, supprimez le repertoire de donnees :

```
<repertoire d'installation>/EasySave/
```

Le repertoire et ses fichiers (config.json, state.json, Logs/) seront recrees automatiquement au prochain lancement.

### Logs de diagnostic

En cas de probleme, les fichiers suivants sont les premieres sources d'information :

1. **`state.json`** : Verifier l'etat des travaux (notamment l'etat `Failed`)
2. **`Logs/yyyy-MM-dd.json`** : Consulter les actions effectuees et identifier les fichiers en erreur (TransferTime negatif)
3. **`config.json`** : Verifier que les chemins source et cible sont corrects
