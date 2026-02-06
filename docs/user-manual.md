# EasySave v1.0 - Manuel Utilisateur

## Presentation

EasySave est un logiciel de sauvegarde developpe par ProSoft. Il permet de creer et d'executer des travaux de sauvegarde de fichiers et de repertoires, avec un suivi en temps reel de l'avancement et une journalisation complete des operations.

EasySave prend en charge deux types de sauvegarde :

- **Sauvegarde complete** : copie l'integralite des fichiers du repertoire source vers le repertoire cible.
- **Sauvegarde differentielle** : copie uniquement les fichiers modifies depuis la derniere sauvegarde complete.

---

## Prerequis

- **Systeme d'exploitation** : Windows 10 ou superieur
- **Framework** : .NET 8.0 Runtime (ou superieur) installe sur la machine
- Le logiciel ne necessite pas d'installation. Il suffit de placer le fichier executable dans un repertoire de votre choix.

---

## Lancement du logiciel

### Mode interactif

Lancez le programme en double-cliquant sur `EasySave.exe` ou depuis un terminal :

```
EasySave.exe
```

Le logiciel affiche alors un menu interactif dans la console, permettant de gerer et d'executer vos travaux de sauvegarde.

### Mode ligne de commande

Vous pouvez executer directement des travaux de sauvegarde en passant des arguments en ligne de commande :

```bash
# Executer les sauvegardes 1 a 3 (sequentiellement)
EasySave.exe 1-3

# Executer les sauvegardes 1 et 3 uniquement
EasySave.exe 1;3

# Executer la sauvegarde 2 uniquement
EasySave.exe 2
```

> **Note** : Les numeros correspondent a la position des travaux de sauvegarde dans la liste (de 1 a 5).

---

## Choix de la langue

Au premier lancement, le logiciel vous propose de choisir votre langue :

```
1. Francais
2. English
```

Ce choix est sauvegarde et sera conserve pour les prochains lancements. Vous pouvez le modifier a tout moment via le menu principal.

---

## Menu principal

Une fois la langue selectionnee, le menu principal s'affiche :

```
========== EasySave v1.0 ==========

1. Creer un travail de sauvegarde
2. Afficher les travaux de sauvegarde
3. Modifier un travail de sauvegarde
4. Supprimer un travail de sauvegarde
5. Executer un travail de sauvegarde
6. Executer tous les travaux de sauvegarde
7. Changer la langue
8. Quitter

Choix :
```

---

## Gestion des travaux de sauvegarde

### Creer un travail de sauvegarde

Selectionnez l'option **1** dans le menu principal. Le logiciel vous demandera les informations suivantes :

| Information           | Description                                                        | Exemple                          |
|-----------------------|--------------------------------------------------------------------|----------------------------------|
| Nom de la sauvegarde  | Un nom unique pour identifier le travail                           | `Sauvegarde Documents`           |
| Repertoire source     | Le chemin du repertoire a sauvegarder                              | `C:\Users\Jean\Documents`        |
| Repertoire cible      | Le chemin du repertoire de destination                             | `D:\Backups\Documents`           |
| Type de sauvegarde    | `complete` ou `differentielle`                                     | `complete`                       |

**Limites** :
- Vous pouvez creer un maximum de **5 travaux de sauvegarde**.
- Les repertoires source et cible peuvent se trouver sur des disques locaux, des disques externes ou des lecteurs reseau.

### Afficher les travaux de sauvegarde

Selectionnez l'option **2** pour afficher la liste de tous les travaux configures avec leurs details (nom, repertoire source, repertoire cible, type).

### Modifier un travail de sauvegarde

Selectionnez l'option **3**, puis le numero du travail a modifier. Vous pourrez mettre a jour le repertoire source et/ou le repertoire cible.

### Supprimer un travail de sauvegarde

Selectionnez l'option **4**, puis le numero du travail a supprimer. Le travail sera supprime de la configuration.

---

## Execution des sauvegardes

### Executer un travail individuel

Selectionnez l'option **5**, puis le numero du travail a executer. Le logiciel effectue la sauvegarde et affiche l'avancement en temps reel dans la console.

### Executer tous les travaux

Selectionnez l'option **6**. Tous les travaux de sauvegarde sont executes de maniere sequentielle (l'un apres l'autre).

### Deroulement d'une sauvegarde

Lors de l'execution :

1. Le repertoire cible est cree s'il n'existe pas.
2. Tous les fichiers et sous-repertoires du repertoire source sont copies vers le repertoire cible.
3. Pour une **sauvegarde differentielle**, seuls les fichiers modifies depuis la derniere sauvegarde complete sont copies.
4. L'avancement est affiche en temps reel (nombre de fichiers, taille, progression en pourcentage).
5. Un fichier log est ecrit pour chaque fichier copie.
6. Le fichier d'etat est mis a jour apres chaque fichier.

---

## Fichiers generes

EasySave genere automatiquement plusieurs fichiers dans le repertoire de l'application :

### Fichier log journalier

- **Emplacement** : `<repertoire de l'application>/EasySave/Logs/yyyy-MM-dd.json`
- **Format** : JSON avec retours a la ligne pour une lecture aisee
- **Contenu** : un enregistrement par fichier copie contenant :
  - Horodatage
  - Nom du travail de sauvegarde
  - Chemin complet du fichier source
  - Chemin complet du fichier de destination
  - Taille du fichier (en octets)
  - Temps de transfert (en millisecondes, negatif en cas d'erreur)

**Exemple** :

```json
{
  "Timestamp": "2026-02-06T14:30:00",
  "Application": "EasySave",
  "Data": {
    "BackupName": "Sauvegarde Documents",
    "SourcePath": "C:\\Users\\Jean\\Documents\\rapport.docx",
    "TargetPath": "D:\\Backups\\Documents\\rapport.docx",
    "FileSize": 15234,
    "TransferTime": 12
  }
}
```

### Fichier d'etat temps reel

- **Emplacement** : `<repertoire de l'application>/EasySave/state.json`
- **Format** : JSON avec retours a la ligne
- **Contenu** : l'etat de chaque travail de sauvegarde, mis a jour en temps reel

**Exemple** :

```json
[
  {
    "Name": "Sauvegarde Documents",
    "SourcePath": "C:\\Users\\Jean\\Documents",
    "TargetPath": "D:\\Backups\\Documents",
    "BackupProgress": {
      "DateTime": "2026-02-06T14:30:00",
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
  }
]
```

### Fichier de configuration

- **Emplacement** : `<repertoire de l'application>/EasySave/config.json`
- **Contenu** : la liste des travaux de sauvegarde configures (nom, chemins source et cible, type de sauvegarde)
- Ce fichier est mis a jour automatiquement a chaque creation, modification ou suppression d'un travail.

---

## Messages d'erreur courants

| Message                                        | Cause                                        | Solution                                             |
|------------------------------------------------|----------------------------------------------|------------------------------------------------------|
| Le repertoire source n'existe pas              | Chemin source invalide ou inaccessible       | Verifiez le chemin et les droits d'acces             |
| Nombre maximum de sauvegardes atteint          | 5 travaux deja configures                    | Supprimez un travail existant avant d'en creer un    |
| Erreur lors de la copie du fichier             | Probleme d'acces au fichier ou disque plein  | Verifiez les droits et l'espace disponible           |
| Le repertoire cible n'est pas accessible       | Lecteur reseau deconnecte ou chemin invalide | Verifiez la connexion reseau et le chemin            |

---

## Support

Pour toute question ou assistance technique, contactez le support ProSoft.

- **Contrat de maintenance** : 5j/7, 8h-17h
- **Email** : support@prosoft.fr
