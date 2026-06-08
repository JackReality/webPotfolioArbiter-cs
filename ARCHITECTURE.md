# Architecture & conventions de codage

> Annexe de [`CLAUDE.md`](CLAUDE.md). Détaille la structure du projet et les règles de codage.

## Un seul projet, à plat

```
webPortfolio-cs/
├── .env                  # secrets MySQL/SMTP (NE JAMAIS committer)
├── Program.cs            # démarrage : .env, MySQL, MudBlazor, AUTH (cookie), i18n, endpoints /auth/*, /Culture/Set
├── Models/               # 1 classe = 1 table (ex. User → table "users")
├── Data/                 # AppDbContext (le pont vers MySQL ; les DbSet<T>)
├── Services/             # logique métier, injectée dans les pages (ex. AuthService, UserService)
├── Resources/            # fichiers .resx (traductions FR / EN / ES)
├── SharedResource.cs     # classe "marqueur" référencée par IStringLocalizer
└── Components/
    ├── App.razor         # page hôte (styles/scripts MudBlazor + mode interactif)
    ├── Routes.razor      # routeur : AuthorizeRouteView (fait respecter les [Authorize])
    ├── RedirectToLogin.razor  # redirige un visiteur non connecté vers /login
    ├── Layout/           # MainLayout (providers MudBlazor, barre, menu) + NavMenu
    ├── Shared/           # composants réutilisables (ex. LanguageTabs)
    └── Pages/            # les pages .razor (une route @page par page)
```

**Sens des dépendances :** `Pages → Services → Data → Models`. Les `Models` ne dépendent de rien.

## Conventions de nommage (RÈGLE PRINCIPALE)

**Tout le code et les identifiants sont en anglais** : noms de fichiers, routes `@page`,
classes, services, **tables et colonnes SQL**, propriétés, variables.

- **SQL** : `snake_case` (ex. `display_name`, `created_at`, `stripe_price_id`).
- **C#** : `PascalCase` pour classes/propriétés (ex. `DisplayName`), mappé à la colonne via `[Column("...")]`.
- **Colonne de langue : toujours `language`** (C# `Language`), jamais `lang`.
- Les **textes affichés à l'utilisateur** ne sont pas en dur : ils passent par l'i18n (voir [`I18N.md`](I18N.md)).
- La **doc du projet** (`.md`) et **nos échanges** restent en **français**.

## Conventions de codage

1. **Pages en fichier unique** : pas de code-behind `.cs` séparé. Markup (HTML / MudBlazor) en haut,
   C# dans un bloc **`@code`** en bas du même `.razor`.
2. **Une classe par table** dans `Models/` (mappée par `[Table]`/`[Column]`). Nouvelle table =
   un `DbSet<T>` ajouté dans `AppDbContext`. ⚠️ On crée la table **en SQL** (pas de migration EF) —
   voir [`database.md`](database.md).
3. **Logique dans `Services/`**, jamais dans les pages. Le service reçoit le `DbContext` par
   **injection de constructeur** ; les pages font `@inject MonService` et appellent le service,
   **jamais** le `DbContext` directement. Un service se déclare dans `Program.cs`
   (`builder.Services.AddScoped<MonService>();`).
4. **Databinding** : `@bind-Value` lie un champ à une variable (temps réel). L'**enregistrement en base
   n'est PAS automatique** : il se fait sur action (clic bouton → `await service.XxxAsync()` →
   `SaveChangesAsync()`).
5. **MudBlazor d'abord** pour les composants (tables triables, formulaires, dialogues…). HTML/CSS
   classique possible en complément.
6. **Formulaire : champs modifiables uniquement.** Seules les données modifiables sont dans un
   `<input>`/`@bind`. Les infos en lecture seule sont du **texte brut** (`<MudText>`), jamais un champ
   gris/disabled — sinon l'utilisateur croit qu'il peut éditer.
