# Projet Portfolio Arbiter

> Dossier `webPortfolio-cs` · projet C# `Portfolio` (`#` est interdit dans un nom .NET).
> L'utilisateur **débute en C#** → expliquer simplement, **répondre en français**.

## But
Site de **vente de formations en ligne** : forum, achat de formations, 3 espaces
(**public** / **connecté** / **client**).
**État** : ✅ socle d'architecture, auth cookie + rôles, multilingue FR/EN/ES.
⏳ le métier (forum, achat, contenu des formations) n'est pas encore fait.

## Stack
- **Blazor Web App** (.NET 10), rendu **serveur** (`InteractiveServer`), **pas** de WebAssembly.
- UI **MudBlazor** · DB **MySQL** (EF Core + provider **Pomelo**) · Auth **cookie natif** (pas Identity), mots de passe **bcrypt**.
- i18n **.NET** (`IStringLocalizer` + `.resx`) FR/EN/ES · secrets dans **`.env`** (DotNetEnv, ignoré par Git).

## ⚠️ Règles non négociables (à lire AVANT de coder)
1. **Tout le code/identifiants en anglais** : fichiers, routes, classes, services,
   **tables + colonnes SQL**, propriétés, variables. SQL en `snake_case`, C# en `PascalCase`.
   **Colonne de langue = `language`** partout. (Les textes affichés passent par l'i18n ;
   la doc et nos échanges restent en français.)
2. **`DB_HOST=127.0.0.1`** obligatoire (le user MySQL `portfolio` refuse `localhost`).
3. **Pas de migrations EF** : on **crée les tables en SQL à la main**, puis on mappe le modèle
   (`[Table]`/`[Column]`). La table `users` est gérée à la main.
4. **Pages en fichier unique** (`.razor` : markup en haut + bloc `@code` en bas, pas de code-behind).
5. **Logique dans `Services/`** (injectés par constructeur) ; une page n'appelle **jamais** le `DbContext`.
6. **MudBlazor d'abord** pour les composants.
→ détail des conventions de codage : [`ARCHITECTURE.md`](ARCHITECTURE.md).

## Mode de travail (boucle à respecter)
1. L'utilisateur donne un **objectif**. 2. Je **propose les tâches** (sans coder).
3. Il valide → j'ajoute dans **`STATUS.md`**. 4. J'exécute **UNE tâche à la fois**, je vérifie
la compilation, puis **je m'arrête et je fais le point** ; je ne continue qu'après son **feu vert**.
→ règles de suivi détaillées : [`PROCESSUS_TRAVAIL.md`](PROCESSUS_TRAVAIL.md).

## Serveur de dev (`dotnet watch`)
- **C'est Claude qui tient `dotnet watch`** en arrière-plan : il **recompile et rafraîchit le
  navigateur tout seul** à chaque sauvegarde. Claude **lit ses logs** pour confirmer `Build succeeded`
  (plus besoin d'un build manuel séparé).
- ⚠️ Le hot-reload n'applique **pas** les changements structurels (nouvelle **route `@page`**,
  nouveau fichier) → après ce type de modif, **Claude redémarre le `watch`**.
- HTTP http://localhost:5252 · HTTPS https://localhost:7178 (ports dans `Properties/launchSettings.json`).

## Base de données
Identifiants dans `.env` (`DB_NAME=portfolio_arbiter`, port `13306`).
Schéma des tables, **règles de liaison** (clés étrangères) et **accès CLI `mysql.exe`** :
→ [`database.md`](database.md).

## Authentification & multilingue
- Auth cookie, rôles (`visitor` / `subscriber` / `client` / `admin`), `[Authorize]` → [`AUTH.md`](AUTH.md).
- Localisation FR/EN/ES (`IStringLocalizer`, `.resx`, `/Culture/Set`, langue au login) → [`I18N.md`](I18N.md).

## Sauvegarde (uniquement sur demande explicite)
1. **commit** Git local avec message descriptif. 2. **push** → https://github.com/JackReality/webPotfolioArbiter-cs.git

## Au démarrage de chaque session
1. Claude lance `dotnet watch`. 2. Lire **`STATUS.md`** (section « À faire »).
3. Attaquer la 1ʳᵉ tâche — ne pas redemander ce qui est déjà documenté ici ou dans `STATUS.md`.

## Fichiers annexes (lus à la demande)
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — arborescence, sens des dépendances, **conventions de codage détaillées**.
- [`AUTH.md`](AUTH.md) — mécanisme d'auth cookie, rôles, protection des pages.
- [`I18N.md`](I18N.md) — localisation FR/EN/ES, colonne `language`, langue au login.
- [`database.md`](database.md) — tables, clés étrangères, accès CLI MySQL, convention de nommage des pages.
- [`PROCESSUS_TRAVAIL.md`](PROCESSUS_TRAVAIL.md) — règles de suivi des lots et de `STATUS.md`.
- [`STATUS.md`](STATUS.md) — tâches en cours / faites.
