# Projet Portfolio

> Dossier : `webPortfolio-C#` · Projet C# : `Portfolio` (le nom diffère car `#` est
> interdit dans un nom de projet .NET). L'utilisateur **débute en C#** — expliquer
> simplement, répondre **en français**.

## But du projet

Site web de **vente de formations en ligne**, avec :
- un **forum** pour les visiteurs,
- l'**achat** de formations,
- trois espaces : **public** (anonyme), **connecté**, et **client** (a acheté une formation).

⚠️ **État actuel.**
- ✅ **Fait** : socle d'architecture, **authentification** (connexion/déconnexion par
  cookie), **autorisation par rôles**, **multilingue FR/EN/ES**. Les trois espaces
  (public / connecté / client) sont déjà protégés par `[Authorize]`.
- ⏳ **Pas encore** : le **métier** (forum, achat, contenu réel des formations). Les
  pages protégées affichent pour l'instant un simple « bientôt disponible ».

## Stack technique

| Élément | Choix |
|---|---|
| Framework | **Blazor Web App** (.NET 10), **rendu serveur** (`InteractiveServer`) |
| WebAssembly | **Non** (choix délibéré : accès direct à la base, bon SEO, plus simple) |
| UI | **MudBlazor** (bibliothèque de composants) |
| Base de données | **MySQL** via EF Core + provider **Pomelo** |
| Authentification | **cookie natif .NET** (pas d'Identity), mots de passe hachés **bcrypt** |
| Multilingue | **localisation .NET** (`IStringLocalizer` + fichiers `.resx`) : FR / EN / ES |
| Secrets | fichier **`.env`** (chargé par **DotNetEnv**), ignoré par Git |

## Architecture — un seul projet, à plat

```
webPortfolio-C#/
├── .env                  # identifiants MySQL (NE JAMAIS committer)
├── Program.cs            # démarrage : .env, MySQL, MudBlazor, AUTH (cookie), i18n, endpoints /auth/*
├── Models/               # 1 classe = 1 table (ex. User → table "users")
├── Data/                 # AppDbContext (le pont vers MySQL ; DbSet<User>)
├── Services/             # logique métier, injectée dans les pages (ex. AuthService)
├── Resources/            # fichiers .resx (traductions FR / EN / ES)
├── SharedResource.cs     # classe "marqueur" qui référence les .resx pour IStringLocalizer
└── Components/
    ├── App.razor         # page hôte (styles/scripts MudBlazor + mode interactif)
    ├── Routes.razor      # routeur : AuthorizeRouteView (fait respecter les [Authorize])
    ├── RedirectToLogin.razor  # redirige un visiteur non connecté vers /login
    ├── Layout/           # MainLayout (providers MudBlazor, barre, menu) + NavMenu
    └── Pages/            # les pages .razor (Login, Register, Reset, AccesReserve, …)
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
- ⚠️ La table **`users`** est gérée « à la main » (le modèle `User` mappe une table
  existante via `[Table]`/`[Column]`), on ne la recrée pas par migration.

## Authentification & rôles

- **Mécanisme** : cookie natif .NET (`AddCookie`), **pas** ASP.NET Identity. Après
  connexion réussie, un cookie chiffré contient l'identité (id, email, nom, **rôle**).
- **Flux de connexion** : la page `/login` poste vers l'endpoint **`/auth/login`**
  (dans `Program.cs`). Cet endpoint appelle `AuthService.VerifierIdentifiantsAsync`
  (vérif **bcrypt**), puis pose le cookie (`SignInAsync`). Déconnexion : `/auth/logout`.
  > Pourquoi un endpoint et pas un bouton ? Poser/retirer le cookie exige le
  > `HttpContext`, indisponible dans un composant interactif Blazor Server.
- **Protéger une page** : ajouter en haut du `.razor`
  - `@attribute [Authorize]` → réservé aux **connectés** ;
  - `@attribute [Authorize(Roles = "client,admin")]` → réservé à certains **rôles**.
- **Rôles** (champ `User.Role`) : `visitor`, `subscriber`, `client`, `admin`.
- **Que se passe-t-il si refusé ?** Géré dans `Routes.razor` (`AuthorizeRouteView`) :
  non connecté → `RedirectToLogin` (vers `/login`) ; connecté mais mauvais rôle →
  page `AccesReserve`.
- **Lire l'utilisateur courant** dans une page : via une `<AuthorizeView>` ou un
  paramètre en cascade `Task<AuthenticationState>` (activé par
  `AddCascadingAuthenticationState` dans `Program.cs`).

## Multilingue (FR / EN / ES)

- **Textes** : ne JAMAIS écrire le texte en dur. Injecter
  `@inject IStringLocalizer<SharedResource> L` et utiliser `@L["Ma.Cle"]`.
- **Traductions** : dans `Resources/` (un fichier `.resx` par langue). `fr` = défaut.
- **Changer de langue** : le sélecteur du header appelle l'endpoint `/Culture/Set`,
  qui pose un cookie de langue puis recharge la page.

## Lancer l'application

```powershell
dotnet run
```
- HTTP : **http://localhost:5252**
- HTTPS : **https://localhost:7178**

Ports définis dans `Properties/launchSettings.json` (modifiables).
