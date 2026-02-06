# EasySave v1.0 - Explication du Diagramme de Classes

> Ce document explique le diagramme de classes du projet EasySave. Le diagramme au format Mermaid est disponible dans le fichier [class-diagram.md](class-diagram.md).

---

## Vue d'ensemble

Le diagramme de classes represente l'architecture logicielle d'EasySave v1.0. Le projet est organise en deux espaces de noms principaux :

- **EasySave** : l'application console principale, contenant toute la logique metier
- **EasyLog.dll** : une bibliotheque de journalisation independante et reutilisable

L'architecture repose sur les design patterns **Strategy** et **Factory**, garantissant une extensibilite et une maintenabilite optimales.

---

## Description des classes

### Espace de noms EasySave

#### Program

Point d'entree de l'application. Responsable de :

- L'affichage du menu interactif dans la console
- Le parsing des arguments en ligne de commande (`Main(string[] args)`)
- La delegation des actions utilisateur vers le `BackupManager`

`Program` n'a aucune logique metier : il sert uniquement de couche d'interface entre l'utilisateur et le `BackupManager`.

---

#### BackupManager

Orchestrateur central du logiciel. Il gere le cycle de vie complet des travaux de sauvegarde.

**Attributs** :

| Attribut                   | Visibilite | Type                     | Role                                            |
|----------------------------|------------|--------------------------|--------------------------------------------------|
| `backupJobs`               | prive      | `List<BackupJob>`        | Liste des travaux de sauvegarde (max 5)          |
| `stateManager`             | prive      | `StateManager`           | Gestionnaire du fichier d'etat temps reel        |
| `configManager`            | prive      | `ConfigManager`          | Gestionnaire de la persistance de configuration  |
| `backupStrategyFactory`    | prive      | `BackupStrategyFactory`  | Fabrique de strategies de sauvegarde             |

**Methodes** :

| Methode                    | Visibilite | Role                                                                 |
|----------------------------|------------|----------------------------------------------------------------------|
| `CreateJob(...)`           | public     | Cree un travail de sauvegarde (retourne `false` si la limite de 5 est atteinte) |
| `DeleteJob(index)`         | public     | Supprime un travail par son index                                    |
| `ModifyJob(index, ...)`    | public     | Modifie les chemins source/cible d'un travail                        |
| `ExecuteJob(index)`        | public     | Lance l'execution d'un travail de sauvegarde                        |
| `ListJobs()`               | public     | Retourne la liste des travaux configures                             |

Le `BackupManager` est le seul point d'acces pour `Program`. Il coordonne les interactions entre le `ConfigManager` (persistance), le `StateManager` (suivi temps reel) et les `BackupJob` (execution).

---

#### BackupJob

Represente un travail de sauvegarde individuel. Chaque travail encapsule sa propre strategie de sauvegarde et son modele de progression.

**Attributs** :

| Attribut           | Visibilite | Type               | Role                                   |
|--------------------|------------|--------------------|-----------------------------------------|
| `name`             | prive      | `string`           | Nom unique du travail                   |
| `sourcePath`       | public     | `string`           | Chemin du repertoire source             |
| `targetPath`       | public     | `string`           | Chemin du repertoire cible              |
| `backupStrategy`   | prive      | `IBackupStrategy`  | Strategie de sauvegarde utilisee        |
| `backupProgress`   | prive      | `BackupProgress`   | Modele de suivi de progression          |

**Methodes** :

| Methode                         | Role                                                        |
|---------------------------------|-------------------------------------------------------------|
| `Execute(Action onProgressUpdate)` | Lance la sauvegarde en delegant a la strategie           |
| `UpdatePaths(source, target)`   | Met a jour les chemins source et cible                      |

La methode `Execute` accepte un callback `onProgressUpdate` qui est appele apres chaque fichier copie, permettant au `BackupManager` de mettre a jour le fichier d'etat.

---

#### BackupProgress

Modele de donnees contenant les informations de progression d'un travail de sauvegarde en cours.

| Propriete          | Type          | Description                                                |
|--------------------|---------------|------------------------------------------------------------|
| `dateTime`         | `DateTime`    | Horodatage de la derniere action                           |
| `state`            | `BackupState` | Etat courant du travail                                    |
| `totalFiles`       | `int`         | Nombre total de fichiers a traiter                         |
| `totalSize`        | `long`        | Taille totale des fichiers a traiter (octets)              |
| `fileSize`         | `long`        | Taille du fichier en cours de traitement                   |
| `progress`         | `float`       | Pourcentage de progression (0 a 100)                       |
| `transferTime`     | `float`       | Temps de transfert du dernier fichier (ms)                 |
| `remainingFiles`   | `int`         | Nombre de fichiers restants                                |
| `remainingSize`    | `long`        | Taille restante a copier (octets)                          |
| `sourceFilePath`   | `string`      | Chemin du fichier source en cours                          |
| `targetFilePath`   | `string`      | Chemin du fichier cible en cours                           |

Ce modele est mis a jour a chaque fichier copie par la strategie de sauvegarde, puis serialise dans `state.json` par le `StateManager`.

---

#### BackupState (Enumeration)

Enumeration definissant les etats possibles d'un travail de sauvegarde :

| Valeur     | Signification                                        |
|------------|------------------------------------------------------|
| `Active`   | La sauvegarde est en cours d'execution               |
| `Inactive` | Le travail est configure mais pas en cours           |
| `Ended`    | La sauvegarde s'est terminee avec succes             |
| `Failed`   | La sauvegarde a echoue suite a une erreur            |

---

#### IBackupStrategy (Interface)

Interface definissant le contrat pour les strategies de sauvegarde (pattern Strategy).

| Methode | Signature                                                                                     |
|---------|-----------------------------------------------------------------------------------------------|
| `Save`  | `void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action onProgressUpdate)` |

Cette interface permet d'interchanger l'algorithme de sauvegarde sans modifier le code de `BackupJob` ou de `BackupManager`. Toute nouvelle strategie de sauvegarde peut etre ajoutee en implementant cette interface.

---

#### FullBackup

Implementation de `IBackupStrategy` pour la sauvegarde **complete**.

- Copie l'integralite des fichiers et sous-repertoires du repertoire source vers le repertoire cible
- Met a jour le `BackupProgress` apres chaque fichier copie
- Appelle le callback `onProgressUpdate` pour declencher la mise a jour de l'etat
- Ecrit une entree dans le log via `Logger` (EasyLog) pour chaque fichier copie

---

#### DifferentialBackup

Implementation de `IBackupStrategy` pour la sauvegarde **differentielle**.

- Compare la date de derniere modification de chaque fichier source avec la version presente dans le repertoire cible
- Ne copie que les fichiers nouveaux ou modifies depuis la derniere sauvegarde complete
- Meme interface et meme cycle de mise a jour que `FullBackup`

---

#### BackupStrategyFactory

Fabrique responsable de la creation des instances de strategies de sauvegarde (pattern Factory).

| Methode                           | Description                                                    |
|-----------------------------------|----------------------------------------------------------------|
| `Create(string backupStrategy)`   | Retourne une instance de `FullBackup` ou `DifferentialBackup` selon la chaine passee |

Accepte les valeurs `"full"` et `"differential"`. Centralise la logique d'instanciation pour eviter de la disperser dans le code.

---

#### StateManager

Gestionnaire de l'ecriture du fichier d'etat temps reel (`state.json`).

| Attribut / Methode  | Description                                                              |
|---------------------|--------------------------------------------------------------------------|
| `filePath` (prive)  | Chemin vers le fichier `state.json`                                      |
| `Write(backupJobs)` | Serialise la liste des travaux (avec leur progression) en JSON et l'ecrit dans le fichier |

Le fichier `state.json` est ecrase a chaque appel de `Write`, garantissant que le contenu reflete toujours l'etat le plus recent.

---

#### ConfigManager

Gestionnaire de la persistance de la configuration des travaux de sauvegarde (`config.json`).

| Attribut / Methode   | Description                                                             |
|----------------------|-------------------------------------------------------------------------|
| `filePath` (prive)   | Chemin vers le fichier `config.json`                                    |
| `Load()`             | Charge et retourne la liste des travaux depuis `config.json`            |
| `Save(backupJobs)`   | Serialise et ecrit la liste des travaux dans `config.json`              |

Le `ConfigManager` permet de persister la configuration entre les sessions. Il est appele a chaque creation, modification ou suppression de travail.

---

### Espace de noms EasyLog.dll

#### Logger

Classe de journalisation reutilisable, concue pour etre integree dans d'autres projets ProSoft.

| Attribut / Methode          | Description                                                          |
|-----------------------------|----------------------------------------------------------------------|
| `logDirectory` (prive)      | Repertoire de stockage des fichiers log                              |
| `Logger(string logDirectory)` | Constructeur. Cree le repertoire de logs si necessaire.            |
| `Write(LogEntry entry)`     | Ecrit une entree dans le fichier journalier du jour (`yyyy-MM-dd.json`) |

#### LogEntry

Modele generique d'une entree de log.

| Propriete      | Type                         | Description                                     |
|----------------|------------------------------|-------------------------------------------------|
| `Timestamp`    | `DateTime?`                  | Horodatage (rempli automatiquement par `Logger`) |
| `Application`  | `string`                     | Application emettrice                            |
| `Data`         | `Dictionary<string, object>` | Donnees cle-valeur de l'action loguee           |

Le `Dictionary<string, object>` pour `Data` offre une grande flexibilite : chaque application peut y stocker des informations differentes sans modifier la classe `LogEntry`.

---

## Relations entre les classes

### Associations et compositions

| Relation                                    | Type          | Cardinalite | Description                                                          |
|---------------------------------------------|---------------|-------------|----------------------------------------------------------------------|
| `Program` --> `BackupManager`               | Association   | 1..1        | `Program` utilise un unique `BackupManager`                          |
| `BackupManager` --> `BackupJob`             | Composition   | 1..0..5     | Le manager gere de 0 a 5 travaux de sauvegarde                      |
| `BackupManager` --> `BackupStrategyFactory` | Association   | 1..1        | Le manager utilise la factory pour creer les strategies              |
| `BackupManager` --> `StateManager`          | Composition   | 1..1        | Le manager possede un gestionnaire d'etat                            |
| `BackupManager` --> `ConfigManager`         | Composition   | 1..1        | Le manager possede un gestionnaire de configuration                  |
| `BackupJob` --> `IBackupStrategy`           | Association   | 1..1        | Chaque travail utilise une strategie de sauvegarde                   |
| `BackupJob` --> `BackupProgress`            | Composition   | 1..1        | Chaque travail possede son modele de progression                     |
| `BackupProgress` --> `BackupState`          | Association   | 1..1        | La progression utilise l'enumeration d'etat                          |
| `StateManager` --> `BackupJob`              | Dependance    | 1..*        | Le `StateManager` ecrit les donnees de tous les travaux              |
| `ConfigManager` --> `BackupJob`             | Dependance    | 1..*        | Le `ConfigManager` charge et sauvegarde les travaux                  |
| `IBackupStrategy` --> `Logger`              | Dependance    | *..1        | Les strategies utilisent le `Logger` pour journaliser les actions    |

### Implementations d'interface

| Classe               | Interface         | Description                                |
|----------------------|-------------------|--------------------------------------------|
| `FullBackup`         | `IBackupStrategy` | Implemente la sauvegarde complete          |
| `DifferentialBackup` | `IBackupStrategy` | Implemente la sauvegarde differentielle    |

### Creations (Factory)

| Createur                 | Produit           | Description                                          |
|--------------------------|-------------------|------------------------------------------------------|
| `BackupStrategyFactory`  | `IBackupStrategy` | Cree des instances de `FullBackup` ou `DifferentialBackup` |

---

## Design patterns illustres

### Pattern Strategy

Le diagramme illustre clairement le pattern Strategy :

1. **Contexte** : `BackupJob` - possede une reference vers `IBackupStrategy`
2. **Strategie (interface)** : `IBackupStrategy` - definit le contrat `Save(...)`
3. **Strategies concretes** : `FullBackup` et `DifferentialBackup`

Ce pattern permet d'ajouter facilement de nouveaux types de sauvegarde (ex: sauvegarde incrementielle) sans modifier les classes existantes, conformement au **principe Open/Closed** (SOLID).

### Pattern Factory

Le `BackupStrategyFactory` isole la logique d'instanciation des strategies. Le `BackupManager` n'a pas besoin de connaitre les classes concretes : il passe une chaine de caracteres a la factory et recoit une instance de `IBackupStrategy`.

### Separation des responsabilites

Le diagramme montre une architecture ou chaque classe a une responsabilite unique :

- `BackupManager` : orchestration
- `ConfigManager` : persistance de la configuration
- `StateManager` : suivi temps reel
- `BackupJob` : representation d'un travail
- `IBackupStrategy` / implementations : algorithme de copie
- `Logger` / `LogEntry` : journalisation

Cette separation facilite la maintenance, les tests et l'evolution vers la version 2.0 (interface graphique MVVM).
