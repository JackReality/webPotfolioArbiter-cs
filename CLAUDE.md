# Portfolio Arbiter (C# / Blazor)

## Stack
Blazor Web App (.NET 10) · InteractiveServer · MudBlazor · MySQL (EF Core Pomelo) · Auth cookie + bcrypt · i18n FR/EN/ES (.resx)

## Règles non négociables
1. Code/identifiants en anglais, SQL `snake_case`, C# `PascalCase`, colonne langue = `language`
2. `DB_HOST=127.0.0.1` obligatoire
3. Pas de migrations EF — tables créées en SQL à la main, puis mappées
4. Pages en fichier unique `.razor` (markup + `@code`, pas de code-behind)
5. Logique dans `Services/` injectés — une page n'appelle jamais le `DbContext`
5b. Toute méthode `public` d'un service peut être appelée par une page. Elle lance `BusinessException("clé.i18n")` pour les erreurs métier prévues ; la page attrape `BusinessException` et affiche `L[ex.Key]`. Toute autre exception remonte à `ErrorBoundary`.
6. MudBlazor en priorité
7. Répondre en français · expliquer simplement (débutant C#)

## Mode de travail
Proposer les tâches sans coder → validation → une tâche à la fois → s'arrêter après chaque tâche.
→ détail : [PROCESSUS_TRAVAIL.md](PROCESSUS_TRAVAIL.md)

## Démarrage
1. Lancer `dotnet watch` (HTTP 5252 / HTTPS 7178)
2. Lire uniquement [STATUS.md](STATUS.md)

## ⚠️ Lecture des fichiers
- Lire UNIQUEMENT les fichiers concernés par la tâche en cours
- Ne PAS explorer le projet de façon autonome
- Demander avant de lire un fichier non mentionné
- Lire ces fichiers **seulement si la tâche le nécessite** :
  - [ARCHITECTURE.md](ARCHITECTURE.md) — arborescence et conventions
  - [AUTH.md](AUTH.md) — auth cookie et rôles
  - [I18N.md](I18N.md) — localisation FR/EN/ES
  - [database.md](database.md) — tables, clés étrangères, CLI MySQL
 
## Démarrage
1. Lancer `dotnet watch` (HTTP 5252 / HTTPS 7178)
2. ⚠️ Nouvelle route `@page` ou nouveau fichier → redémarrer le watch (hot-reload insuffisant)
3. Lire uniquement [STATUS.md](STATUS.md)