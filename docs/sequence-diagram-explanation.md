# EasySave v1.0 - Explication du Diagramme de Sequence

> Ce document explique le diagramme de sequence du projet EasySave. Le diagramme au format Mermaid est disponible dans le fichier [sequence-diagram.md](sequence-diagram.md).

---

## Vue d'ensemble

Le diagramme de sequence illustre le flux d'execution complet d'un travail de sauvegarde, depuis l'action de l'utilisateur jusqu'a la fin de la copie de tous les fichiers. Il met en evidence :

- La chaine d'appels entre les composants
- Le mecanisme de callback pour la mise a jour en temps reel
- La boucle de traitement fichier par fichier
- L'ecriture simultanee dans le log et le fichier d'etat

---

## Participants

Le diagramme fait intervenir 6 participants, presentes ci-dessous dans l'ordre de leur apparition :

| Participant        | Type       | Role dans le flux                                          |
|--------------------|------------|------------------------------------------------------------|
| **User**           | Acteur     | L'utilisateur qui lance la sauvegarde                      |
| **Program**        | Classe     | Point d'entree, recoit la commande de l'utilisateur        |
| **BackupManager**  | Classe     | Orchestrateur, coordonne l'execution et les mises a jour   |
| **BackupJob**      | Classe     | Travail de sauvegarde individuel, delegue a sa strategie   |
| **IBackupStrategy**| Interface  | Strategie de sauvegarde (FullBackup ou DifferentialBackup)  |
| **StateManager**   | Classe     | Ecrit le fichier d'etat temps reel                         |
| **Logger**         | Classe     | Ecrit le fichier log journalier (EasyLog.dll)              |

---

## Description detaillee du flux

### Etape 1 : Lancement par l'utilisateur

```
User ->> Program : Run EasySave.exe 1
```

L'utilisateur lance le programme avec un argument en ligne de commande (ici `1`, indiquant qu'il souhaite executer le premier travail de sauvegarde). Cela peut aussi se faire via le menu interactif de la console.

Le `Program` est **active** : il prend le controle de l'execution.

---

### Etape 2 : Delegation au BackupManager

```
Program ->> BackupManager : ExecuteJob(1)
```

Le `Program` ne contient aucune logique metier. Il se contente de transmettre la demande au `BackupManager` en appelant `ExecuteJob(1)`.

Le `BackupManager` est **active** a son tour.

---

### Etape 3 : Execution du travail

```
BackupManager ->> BackupJob : Execute(onProgressUpdate)
```

Le `BackupManager` recupere le travail de sauvegarde a l'index demande et appelle sa methode `Execute`. Il lui passe en parametre le callback `onProgressUpdate`, qui est une reference vers sa propre methode interne de mise a jour de l'etat.

Le `BackupJob` est **active**.

---

### Etape 4 : Lancement de la strategie

```
BackupJob ->> IBackupStrategy : Save(sourcePath, targetPath, backupProgress, onProgressUpdate)
```

Le `BackupJob` delegue l'execution a sa strategie de sauvegarde (`IBackupStrategy`). Il lui transmet :

- `sourcePath` : le chemin du repertoire source
- `targetPath` : le chemin du repertoire cible
- `backupProgress` : le modele de progression a mettre a jour
- `onProgressUpdate` : le callback a appeler apres chaque fichier

La strategie concrete (`FullBackup` ou `DifferentialBackup`) est **activee**. C'est elle qui effectue le travail de copie.

---

### Etape 5 : Boucle de traitement des fichiers

```
loop For each file
    IBackupStrategy ->> Logger : Write(data)
    IBackupStrategy ->> BackupManager : onProgressUpdate()
    BackupManager ->> StateManager : Write(backupJobs)
end
```

C'est le coeur du flux. Pour **chaque fichier** du repertoire source, la sequence suivante se repete :

#### 5a. Copie du fichier

La strategie copie le fichier de la source vers la cible (cette action n'est pas representee explicitement dans le diagramme car elle est interne a la strategie).

#### 5b. Journalisation (Logger)

```
IBackupStrategy ->> Logger : Write(data)
```

La strategie cree une entree de log (`LogEntry`) contenant :
- Le nom du travail de sauvegarde
- Le chemin complet du fichier source
- Le chemin complet du fichier de destination
- La taille du fichier
- Le temps de transfert

Elle appelle `Logger.Write(entry)`, qui ecrit cette entree dans le fichier log journalier (`yyyy-MM-dd.json`). Le `Logger` est active puis desactive immediatement (operation synchrone rapide).

#### 5c. Mise a jour de la progression (callback)

```
IBackupStrategy ->> BackupManager : onProgressUpdate()
```

Apres la journalisation, la strategie appelle le callback `onProgressUpdate()`. Cet appel remonte au `BackupManager`, qui sait alors qu'un fichier vient d'etre traite.

#### 5d. Ecriture de l'etat temps reel

```
BackupManager ->> StateManager : Write(backupJobs)
```

Le `BackupManager` reagit au callback en appelant `StateManager.Write()`, qui serialise l'etat complet de tous les travaux de sauvegarde (y compris leur `BackupProgress`) dans le fichier `state.json`.

Le `StateManager` est active puis desactive (ecriture synchrone).

---

### Etape 6 : Fin de l'execution

```
BackupManager -->> Program : Success
```

Une fois tous les fichiers traites, la boucle se termine. Les participants sont **desactives** dans l'ordre inverse :

1. `IBackupStrategy` se desactive (tous les fichiers ont ete copies)
2. `BackupJob` se desactive (l'execution est terminee)
3. `BackupManager` retourne le resultat a `Program`
4. `Program` se desactive

La fleche en pointilles (`-->>`) indique un retour (response), par opposition aux appels (`->>`) qui sont des messages d'invocation.

---

## Mecanismes cles illustres

### Pattern Callback (Observer simplifie)

Le diagramme met en evidence un mecanisme de callback :

1. Le `BackupManager` passe sa methode `OnProgressUpdate` au `BackupJob`
2. Le `BackupJob` la transmet a la strategie (`IBackupStrategy`)
3. La strategie appelle ce callback apres chaque fichier copie
4. Le callback remonte jusqu'au `BackupManager`, qui declenche l'ecriture de l'etat

Ce mecanisme permet a la strategie de signaler sa progression **sans connaitre** le `BackupManager` ni le `StateManager`. Elle se contente d'appeler le callback qu'on lui a fourni.

### Double ecriture synchrone

A chaque fichier copie, deux ecritures ont lieu :

1. **Logger** (EasyLog.dll) : ecriture dans le fichier log journalier - **historique permanent** de toutes les actions
2. **StateManager** : ecriture dans `state.json` - **photographie instantanee** de l'etat courant

Ces deux fichiers ont des roles complementaires :
- Le log est un **journal append-only** (on ajoute des entrees, on ne les supprime jamais)
- L'etat est un **fichier ecrase** a chaque mise a jour (il ne reflete que l'etat present)

### Execution sequentielle

Le diagramme montre que tout le flux est **sequentiel** : un seul fichier est traite a la fois, et chaque etape (copie, log, callback, ecriture d'etat) se termine avant que la suivante ne commence. Cela garantit la coherence des donnees dans les fichiers de sortie.

---

## Lien avec le diagramme de classes

Les interactions montrees dans ce diagramme de sequence correspondent directement aux relations definies dans le diagramme de classes :

| Interaction dans la sequence               | Relation dans le diagramme de classes          |
|--------------------------------------------|------------------------------------------------|
| `Program -> BackupManager`                 | `Program --> BackupManager : uses`             |
| `BackupManager -> BackupJob`               | `BackupManager "1" --> "0..5" BackupJob`       |
| `BackupJob -> IBackupStrategy`             | `BackupJob "1" --> "1" IBackupStrategy : uses` |
| `IBackupStrategy -> Logger`                | `IBackupStrategy ..> Logger : uses`            |
| `BackupManager -> StateManager`            | `BackupManager "1" --> "1" StateManager`       |
