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

## Base de données

- Identifiants dans `.env` : `DB_HOST=localhost`, `DB_PORT=13306`,
  `DB_NAME=portfolio`, `DB_USER=portfolio`, `DB_PASSWORD=...` (à remplir).
- La connexion **n'est pas testée au démarrage** (version serveur fixée
  manuellement dans `Program.cs`) → l'app démarre même sans base lancée.
- Pour créer/mettre à jour les tables : migrations EF Core
  (`dotnet ef migrations add <Nom>` puis `dotnet ef database update`).

## Lancer l'application

```powershell
dotnet run
```
- HTTP : **http://localhost:5252**
- HTTPS : **https://localhost:7178**

Ports définis dans `Properties/launchSettings.json` (modifiables).

## Instructions de sauvegarde

Lorsque vous demandez une sauvegarde, veuillez effectuer les actions suivantes :

1. Faites un commit Git local avec un message descriptif
2. Poussez les modifications vers GitHub à l'adresse : https://github.com/JackReality/webPotfolioArbiter-cs.git

Ces instructions sont à suivre automatiquement lors des demandes de sauvegarde.
