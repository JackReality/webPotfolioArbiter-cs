# Projet Portfolio

> Dossier : `webPortfolio-C#` · Projet C# : `Portfolio` (le nom diffère car `#` est
> interdit dans un nom de projet .NET). L'utilisateur **débute en C#** — expliquer
> simplement, répondre **en français**.

## But du projet

Site web de **vente de formations en ligne**, avec :
- un **forum** pour les visiteurs,
- l'**achat** de formations,
- trois espaces : **public** (anonyme), **connecté**, et **client** (a acheté une formation).

⚠️ **État actuel : squelette d'architecture uniquement.** Les fonctionnalités métier
ci-dessus ne sont **pas encore** implémentées — elles seront ajoutées progressivement.

## Stack technique

| Élément | Choix |
|---|---|
| Framework | **Blazor Web App** (.NET 10), **rendu serveur** (`InteractiveServer`) |
| WebAssembly | **Non** (choix délibéré : accès direct à la base, bon SEO, plus simple) |
| UI | **MudBlazor** (bibliothèque de composants) |
| Base de données | **MySQL** via EF Core + provider **Pomelo** |
| Secrets | fichier **`.env`** (chargé par **DotNetEnv**), ignoré par Git |

## Architecture — un seul projet, à plat

```
webPortfolio-C#/
├── .env                  # identifiants MySQL (NE JAMAIS committer)
├── Program.cs            # démarrage : charge .env, branche MySQL + MudBlazor + services
├── Models/               # 1 classe = 1 table (entités)
├── Data/                 # AppDbContext (le pont vers MySQL)
├── Services/             # logique métier, injectée dans les pages
└── Components/
    ├── App.razor         # page hôte (styles/scripts MudBlazor + mode interactif)
    ├── Layout/           # MainLayout (providers MudBlazor, barre, menu) + NavMenu
    └── Pages/            # les pages .razor
```

**Sens des dépendances :** `Pages → Services → Data → Models`. Les `Models` ne
dépendent de rien.

## Conventions de codage (à respecter)

1. **Pages en fichier unique** : pas de code-behind `.cs` séparé. Le markup
   (HTML / MudBlazor) en haut, le C# dans un bloc **`@code`** en bas du même `.razor`.
2. **Une classe par table** dans `Models/`. Une nouvelle table = un `DbSet<T>`
   ajouté dans `AppDbContext`.
3. **Logique dans `Services/`**, jamais dans les pages. Le service reçoit le
   `DbContext` par **injection de constructeur** ; les pages appellent le service
   (`@inject MonService`), jamais le `DbContext` directement.
4. **Databinding** : `@bind-Value` lie un champ à une variable (temps réel,
   automatique). L'**enregistrement en base n'est PAS automatique** : il se fait
   sur action (clic bouton → `await service.XxxAsync()` → `SaveChangesAsync()`).
5. **MudBlazor d'abord** pour les composants (tables triables, formulaires,
   dialogues…). HTML/CSS classique possible en complément.
6. **Formulaire : champs modifiables uniquement.** Seules les données modifiables
   par l'utilisateur sont affichées dans un champ `<input>`/`@bind`. Les infos en
   lecture seule (email non confirmé, etc.) sont du **texte brut** (`<MudText>`),
   jamais un champ gris ou disabled — sinon l'utilisateur croit qu'il peut éditer.

## Processus de travail

- **Vérification de compilation OBLIGATOIRE** : Après **toute** modification de code, je DOIS vérifier dans les logs du serveur que la compilation a réussi (`Build succeeded`) **avant** de demander à l'utilisateur de tester. Si la compilation échoue, je dois corriger l'erreur immédiatement. Il est strictement interdit de demander un test sur une version non compilée (qui servirait l'ancien cache).
- **Suivi des lots** : Avant de lancer un lot de modifications, il doit être noté dans `STATUS.md`.
- **Avancement** : Une fois la tâche du lot codée, son statut doit être indiqué.
- **Validation** : Une fois le lot complet terminé et **validé par l'utilisateur**, il est coché comme validé.
- Voir le fichier [`PROCESSUS_TRAVAIL.md`](PROCESSUS_TRAVAIL.md) pour les règles détaillées.

## Base de données

- Identifiants dans `.env` : `DB_HOST=127.0.0.1`, `DB_PORT=13306`,
  `DB_NAME=portfolio_arbiter`, `DB_USER=portfolio`, `DB_PASSWORD=123`.
- ⚠️ **`DB_HOST=127.0.0.1`** obligatoire (le user MySQL `portfolio` refuse `localhost`).
- La connexion **n'est pas testée au démarrage** (version serveur fixée
  manuellement dans `Program.cs`) → l'app démarre même sans base lancée.
- Pour créer/mettre à jour les tables : migrations EF Core
  (`dotnet ef migrations add <Nom>` puis `dotnet ef database update`).

### Accéder à la DB en ligne de commande

- **`mysql.exe` est dans** : `C:\Program Files\MariaDB 11.8\bin\mysql.exe`
  (PAS dans le PATH → ne pas taper `mysql` directement).
- **Commande pour requêter** (via PowerShell, les guillemets posent pb sous cmd.exe) :
  ```powershell
  powershell -Command "& 'C:\Program Files\MariaDB 11.8\bin\mysql.exe' -h 127.0.0.1 -P 13306 -u portfolio -p123 portfolio_arbiter -e 'SELECT ...;'"
  ```
- **Alternative fiable** : créer un fichier `.sql` + un `.bat` qui l'appelle :
  ```bat
  @echo off
  "C:\Program Files\MariaDB 11.8\bin\mysql.exe" -h 127.0.0.1 -P 13306 -u portfolio -p123 portfolio_arbiter < query.sql
  ```
- **Colonne `key`** : mot réservé SQL → toujours la backtiquer : `` `key` ``

## Lancer l'application

```powershell
dotnet run
```
- HTTP : **http://localhost:5252**
- HTTPS : **https://localhost:7178**

⚠️ **En développement, utiliser `dotnet watch`** (et non `dotnet run`) pour que
le site se **recharge automatiquement** à chaque modification de fichier. Lancer
`dotnet watch` au début de chaque session de travail.

Ports définis dans `Properties/launchSettings.json` (modifiables).

## Instructions de sauvegarde

Lorsque vous demandez une sauvegarde, veuillez effectuer les actions suivantes :

1. Faites un commit Git local avec un message descriptif
2. Poussez les modifications vers GitHub à l'adresse : https://github.com/JackReality/webPotfolioArbiter-cs.git

Ces instructions sont à suivre automatiquement lors des demandes de sauvegarde.

## ⚠️⚠️⚠️ AU DÉMARRAGE DE CHAQUE SESSION — À FAIRE AUTOMATIQUEMENT ⚠️⚠️⚠️

1. **Lancer `dotnet watch`** (le serveur de dev avec rechargement auto)
2. **Lister les tâches** de la section "Session en cours"
3. **Commencer à travailler** sur la première tâche — NE PAS demander ce qu'il faut faire, C'EST ÉCRIT

**NE JAMAIS demander à l'utilisateur de lancer quoi que ce soit qui est documenté ici.**

## Session en cours

> **Règle :** ne garder QUE la dernière session dans ce bloc. Chaque nouvelle session remplace la précédente. Cela permet d'avoir le contexte immediat sans polluer le fichier.

### Session du 2026-06-07

**Tâches à faire :**
- Auth étape 2 : inscription (`/register`), mot de passe oublié (`/reset`), envoi d'emails SMTP, page profil
- Contenu : page Télécharger, page Formation + achat, Espace client
- Metier : achat de formation + paiement (Stripe), Forum
- Admin : gestion utilisateurs, editeur `email_templates`
- Divers : notifier les erreurs par mail (`mailErrorSend()` dans `Program.cs` + `MainLayout`)
- Ajouter dans `Jack-PromptInitProjet` la demande d'intégrer le login selon l'existant

**Tâches faites cette session :**
- Page `/profil` corrigée : email en texte brut (non modifiable), avatar = picto par défaut
- Admin : Model `EmailTemplate` + `DbSet` + `EmailTemplateService` + enregistrement dans Program.cs
- Page `/admin-dashboard` (accessible admin seul)
- Page `/admin/email-templates` : 3 onglets (inscription, recovery, welcome) × 3 langues, éditeur subject + HTML
- Bouton "Admin Dashboard" sur page Profil (visible role=admin uniquement, style Warning)
