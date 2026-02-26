# 1. Introduction
Ce document a pour but d'expliquer les choix techniques que nous avons faits pour implementer les nouvelles fonctionnalites de la version 3.0 d'EasySave.

---

# 2. Parallelisme des sauvegardes

## Le probleme

En version 2.0, les sauvegardes etaient sequentielles : on lancait un travail, on attendait qu'il finisse, puis on lancait le suivant. C'etait simple mais lent, surtout quand on avait plusieurs travaux a executer. Nous avons mis en place le parallelisme a deux niveaux :

**Niveau 1 - Entre les travaux :** Chaque appel a `BackupManager.ExecuteJob()` lance le travail dans un `Task.Run()`. Ca permet de lancer plusieurs travaux en meme temps sans bloquer l'interface graphique ni les autres travaux.

**Niveau 2 - A l'interieur d'un travail :** Dans les strategies `FullBackupStrategy` et `DifferentialBackupStrategy`, on utilise `Parallel.ForEach` avec un `MaxDegreeOfParallelism = 4` pour copier plusieurs fichiers en parallele au sein d'un meme travail.

## Pourquoi ces choix ?

- **`Task.Run()`** : C'est la maniere la plus simple et propre de lancer du travail en arriere-plan en .NET. Ca retourne une `Task` qu'on peut attendre si besoin, et ca utilise le ThreadPool donc pas de creation inutile de threads.

- **`Parallel.ForEach`** : On a choisi ca plutot qu'un `Task.WhenAll` sur une liste de taches parce que `Parallel.ForEach` gere automatiquement le partitionnement du travail et le nombre de threads. Le `MaxDegreeOfParallelism = 4` evite de creer trop de threads et de saturer le systeme.

- **`SemaphoreSlim(4)` global** : On a ajoute un semaphore global (dans `BackupStrategyFactory.GlobalSemaphore`) pour limiter le nombre total d'operations de copie en parallele, tous travaux confondus. Sans ca, si on lance 5 travaux en meme temps avec chacun 4 threads, ca ferait 20 copies en parallele et ca risquerait de saturer les I/O disque.

---

# 3. Gestion des fichiers prioritaires

## Le probleme

Le cahier des charges demande que les fichiers avec des extensions prioritaires (definies par l'utilisateur) soient sauvegardes en premier. Tant qu'il reste des fichiers prioritaires en attente sur au moins un travail, aucun fichier non prioritaire ne peut etre copie.

## Notre solution

On a utilise un mecanisme de "porte" (gate) base sur un `ManualResetEventSlim` :

- **`ManualResetEventSlim _priorityBlocker`** : C'est notre porte. Quand elle est fermee (`Reset()`), les threads qui essaient de copier des fichiers non prioritaires sont bloques en attente. Quand elle est ouverte (`Set()`), tout le monde peut passer.

- **`Interlocked.Increment / Decrement`** : On utilise un compteur atomique (`_priorityFileCount`) pour savoir combien de fichiers prioritaires sont encore en attente. A chaque fois qu'un fichier prioritaire est copie, on decremente le compteur. Quand il arrive a 0, on ouvre la porte.

- **Pre-scan** : Avant de lancer la copie, `BackupJob.Execute()` scanne d'abord tous les fichiers pour identifier ceux qui ont une extension prioritaire. Si des fichiers prioritaires sont trouves, la porte est fermee immediatement pour bloquer les non prioritaires.

## Pourquoi ces choix ?

- **`ManualResetEventSlim`** plutot qu'un simple `bool` : Un booleen aurait necessite du polling (boucle infinie avec `Thread.Sleep`), ce qui gaspille du CPU. `ManualResetEventSlim` met le thread en attente de maniere efficace sans consommer de ressources, et le reveille instantanement quand la porte s'ouvre.

- **`Interlocked`** plutot qu'un `lock` : Pour un simple compteur, `Interlocked` est beaucoup plus performant qu'un `lock` car c'est une operation atomique au niveau CPU, sans besoin de verrou.

- On a aussi utilise un `ConcurrentDictionary<string, object>` pour eviter que deux threads essaient d'ecrire le meme fichier en meme temps (cas rare mais possible avec le parallelisme).

## Les extensions prioritaires

L'utilisateur peut configurer les extensions prioritaires dans les parametres generaux. Elles sont stockees dans `config.priorityExtensions` (par exemple : `[".pdf", ".db"]`). On a choisi de stocker ca dans le fichier `config.json` pour que ce soit persistant et modifiable sans recompiler.

---

# 4. Limitation des fichiers volumineux

## Le probleme

Pour ne pas saturer la bande passante, le cahier des charges interdit de transferer en meme temps deux fichiers dont la taille depasse un seuil configurable (en Ko).

## Notre solution

On a mis en place un mecanisme de verrou avec `Monitor.Wait` / `Monitor.PulseAll` :

- Un **booleen statique `largeFileInProgress`** indique si un gros fichier est en cours de transfert.
- Avant de copier un fichier volumineux, le thread acquiert le `largeFileLock` et verifie si `largeFileInProgress` est `true`. Si oui, il attend avec `Monitor.Wait()`.
- Une fois la copie terminee, on remet `largeFileInProgress` a `false` et on appelle `Monitor.PulseAll()` pour reveiller tous les threads en attente.
- Les fichiers dont la taille est inferieure au seuil ne sont pas concernes et passent directement.

## Pourquoi ces choix ?

- **`Monitor.Wait / PulseAll`** : C'est le pattern classique "producteur-consommateur" en C#. On aurait pu utiliser un `SemaphoreSlim(1)`, mais `Monitor` nous donne plus de controle car on peut verifier une condition (`largeFileInProgress`) avant de decider d'attendre ou non.

- **Seuil configurable** : Le seuil est stocke dans `config.maxFileSizeKB` (par defaut 102400 Ko = 100 Mo). L'utilisateur peut le modifier dans les parametres generaux. On a choisi un defaut assez eleve pour ne pas bloquer inutilement les transferts dans la plupart des cas.

- **Compatibilite avec les fichiers prioritaires** : Les deux mecanismes fonctionnent ensemble. Un fichier peut etre a la fois prioritaire et volumineux, donc les deux verrous s'appliquent.

---

# 5. Interaction temps reel (Play / Pause / Stop)

## Le probleme

L'utilisateur doit pouvoir mettre en pause, reprendre ou arreter chaque travail de sauvegarde individuellement, ou tous les travaux d'un coup.

## Notre solution

On a ajoute les etats `Paused` et `Stopped` a l'enum `BackupState` (en plus de `Active`, `Inactive`, `Ended`, `Failed` qui existaient deja).

**Fonctionnement :**

- **Pause** : On change l'etat du `BackupProgress` a `Paused`. A l'interieur du `Parallel.ForEach`, chaque iteration verifie l'etat. Si c'est `Paused`, le thread entre dans une boucle d'attente avec `Thread.Sleep(100)` jusqu'a ce que l'etat change.

- **Resume** : On remet l'etat a `Active`. Les threads en attente reprennent automatiquement.

- **Stop** : On change l'etat a `Stopped`. Les threads detectent ce changement et appellent `loopState.Stop()` pour interrompre le `Parallel.ForEach` immediatement.

`BackupManager` expose les methodes `PauseJob()`, `ResumeJob()`, `StopJob()` qui sont appelees depuis l'interface graphique (boutons Play/Pause/Stop dans le `DashboardViewModel`).

## Pourquoi ces choix ?

- **Polling avec `Thread.Sleep(100)`** pour la pause : on sait que c'est pas la solution la plus elegante (un `ManualResetEventSlim` serait plus propre), mais c'est simple a implementer et 100ms c'est suffisamment court pour que la reprise soit quasi instantanee pour l'utilisateur. Le cout CPU est negligeable.

- **`loopState.Stop()`** pour le stop : C'est le mecanisme natif de `Parallel.ForEach` pour arreter les iterations en cours. Ca permet un arret propre sans lancer d'exceptions.

## Suivi temps reel dans l'interface

Le `DashboardViewModel` utilise un `DispatcherTimer` avec un intervalle de 100ms pour rafraichir l'affichage. A chaque tick, il recupere la liste des travaux depuis `BackupManager.ListJobs()` et met a jour les proprietes de l'`ObservableCollection`. Ca permet de voir en temps reel la progression de chaque travail (pourcentage, nombre de fichiers restants, etc.).

---

# 6. Detection du logiciel metier

## Le probleme

Si un logiciel metier est detecte, tous les travaux de sauvegarde doivent etre mis en pause automatiquement. Ils doivent reprendre automatiquement quand le logiciel metier est ferme.

## Evolution par rapport a la v2.0

En v2.0, la detection du logiciel metier **empechait le lancement** d'un travail. En v3.0, ca doit **mettre en pause** les travaux deja en cours et les reprendre automatiquement quand le logiciel est ferme.

## Notre solution

On a mis en place un **thread de monitoring en arriere-plan** lance dans le constructeur de `BackupManager` via `Task.Run(() => StartBackgroundMonitor())` :

- Ce thread tourne en boucle infinie et verifie toutes les secondes si le logiciel metier est lance (via `Process.GetProcesses()` et comparaison insensible a la casse).
- **Quand le logiciel est detecte** : on met en pause tous les travaux actifs et on enregistre leurs indices dans une liste `jobsPausedBySoftware`.
- **Quand le logiciel est ferme** : on reprend automatiquement uniquement les travaux qui avaient ete mis en pause par cette detection (pas ceux que l'utilisateur a mis en pause manuellement).

La classe `ProcessMonitor` (dans `ProcessMonitorManager.cs`) s'occupe de la verification. Le nom du logiciel metier est configurable via `config.forbiddenSoftwareName` (par defaut : `"explorer"` pour les tests, mais en prod ca serait le vrai logiciel metier du client).

## Pourquoi ces choix ?

- **Thread en arriere-plan** plutot qu'une verification ponctuelle : ca permet une detection en temps reel, meme si l'utilisateur lance le logiciel metier pendant une sauvegarde.

- **`Process.GetProcesses()`** : c'est la methode standard en .NET pour lister les processus. On aurait pu utiliser WMI mais c'est plus lourd et moins portable.

- **Liste `jobsPausedBySoftware`** : c'est important pour distinguer les pauses automatiques des pauses manuelles. Sans ca, on risquerait de reprendre un travail que l'utilisateur a volontairement mis en pause.

---

# 7. CryptoSoft mono-instance

## Le probleme

CryptoSoft ne peut etre execute qu'une seule fois en meme temps sur un meme ordinateur. Avec le parallelisme, plusieurs travaux pourraient essayer de lancer CryptoSoft en meme temps.

## Notre solution

**Cote CryptoSoft :**

On a ajoute un `Mutex` nomme (`"CryptoSoftInstance"`) dans le `Program.cs` de CryptoSoft. Au lancement, le programme essaie d'acquerir le mutex. Si un autre processus CryptoSoft est deja en cours, il quitte immediatement avec le code de retour `-99`.

**Cote EasySave :**

Dans `CryptoService.ExecuteCryptoSoft()`, on a mis en place un systeme de **retry** :
- On essaie de lancer CryptoSoft jusqu'a **10 fois**.
- Si le code de retour est `-99` (mono-instance, deja occupe), on attend **500ms** avant de reessayer.
- Si apres 10 tentatives ca echoue toujours, on retourne un code d'erreur.

## Pourquoi ces choix ?

- **`Mutex` nomme** : c'est le mecanisme standard en .NET/Windows pour garantir qu'un seul processus avec ce nom tourne en meme temps. C'est fiable et simple.

- **Retry avec delai** plutot qu'une file d'attente : on a prefere un mecanisme simple de retry parce que CryptoSoft est un processus externe qu'on ne controle pas directement. Une file d'attente aurait ete plus complexe a maintenir pour un gain minimal.

- CryptoSoft est toujours lance via `Process.Start()` et non comme une bibliotheque. On a garde cette separation pour respecter le cahier des charges qui demande un "logiciel externe".

---

# 8. Centralisation des logs (Docker)

## Le probleme

EasySave peut etre deploye sur plusieurs serveurs. Il faut pouvoir centraliser les logs journaliers sur un serveur Docker pour simplifier leur gestion.

## Notre solution

Cette fonctionnalite est en cours de developpement sur la branche `47-featurecentralizedlogs`. L'idee est de creer un service Docker qui recoit les logs en temps reel depuis les differentes instances d'EasySave.

L'utilisateur pourra choisir entre trois modes :
- **Local uniquement** : les logs restent sur le PC de l'utilisateur
- **Centralise uniquement** : les logs sont envoyes uniquement au serveur Docker
- **Les deux** : les logs sont ecrits localement ET envoyes au serveur Docker

Le serveur Docker maintiendra un fichier log journalier unique quel que soit le nombre d'utilisateurs, avec un champ supplementaire pour identifier l'utilisateur et la machine source.

---

# 9. Recapitulatif des mecanismes de synchronisation

Vu que le parallelisme est au coeur de cette version, voici un tableau recapitulatif de tous les mecanismes de synchronisation qu'on utilise et pourquoi :

| Mecanisme | Localisation | Role |
|-----------|-------------|------|
| `SemaphoreSlim(4)` | `BackupStrategyFactory.GlobalSemaphore` | Limiter le nombre total d'operations de copie en parallele |
| `Parallel.ForEach` (max 4) | Strategies de sauvegarde | Paralleliser la copie de fichiers au sein d'un travail |
| `ManualResetEventSlim` | `BackupManager._priorityBlocker` | Bloquer les fichiers non prioritaires tant qu'il reste des prioritaires |
| `Interlocked` | `_priorityFileCount` | Compteur atomique de fichiers prioritaires restants |
| `Monitor.Wait / PulseAll` | Strategies (`largeFileLock`) | Empecher deux gros fichiers d'etre copies en meme temps |
| `ConcurrentDictionary` | Strategies | Verrou par fichier pour eviter les ecritures concurrentes |
| `lock (stateFileLock)` | `StateManager`, `Logger` | Ecriture thread-safe dans state.json et les fichiers log |
| `Mutex` nomme | CryptoSoft | Garantir que CryptoSoft ne tourne qu'une seule fois |
| Polling `BackupProgress.State` | `Parallel.ForEach` | Gerer Play / Pause / Stop par travail |

---

> Globalement, la v3.0 c'est surtout un gros chantier sur la partie synchronisation. Le plus dur c'etait de faire cohabiter tous ces mecanismes sans creer de deadlock. On a fait pas mal de tests pour s'assurer que les fichiers prioritaires, les gros fichiers, la pause et le logiciel metier fonctionnent correctement ensemble sans se marcher dessus.
