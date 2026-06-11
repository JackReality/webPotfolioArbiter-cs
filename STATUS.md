# État du projet — Portfolio Arbiter

Règle : 7 sections maximum. Section 1 = À faire. Sections 2-7 = Fait le YYYY-MM-DD (la plus ancienne est supprimée quand une nouvelle est ajoutée).

---

## 1. À faire

### Stripe — tâches Jack restantes
- [ ] Basculer sur la clé live dans `.env` quand prêt pour la production : `STRIPE_SECRET_KEY=sk_live_...`

### Pages formation PORTFOLIO
- [ ] Contenu de `PortfolioHome.razor` (`/training/portfolio_home`) — actuellement affiche juste la description
- [ ] Autres pages de la formation (leçons, etc.) dans `training_portfolio/`

### Admin

- [ ] Gestion des rôles utilisateurs (liste et suppression déjà faites, manque la modification)
- [ ] Finaliser l'éditeur des templates d'emails (FR/EN/ES)

### Divers

- [ ] URL Google Sheets dans `/download`
- [ ] Remplir l'email de contact dans les Mentions légales (actuellement `contact@realityexplorer.com`)
- [ ] Décider du sort de la table `schema_migrations` (supprimable)

---

## 2. Fait le 2026-06-11

### Stripe — flux complet ✅

- [x] Colonne `code` + colonne `page_url` sur `trainings` (URL relative de la page formation)
- [x] Table `user_trainings` + `UserTrainingService`
- [x] `IDbContextFactory` sur tous les services (fix concurrence DbContext Blazor Server)
- [x] Handler `/stripe-ok` : logging, check `status=complete || payment_status=paid`, re-emit cookie avec claim `training:<code>`, redirect `/stripe-success`
- [x] Page `/stripe-success?code=...` : confirmation avec bouton "Accéder à la formation" → `training.PageUrl`
- [x] Page `/stripe-error` : message d'erreur + bouton retour
- [x] Admin : champ `page_url` dans le formulaire formations

### Pages de formation

- [x] Convention répertoires : `training_[code]/` (ex. `training_portfolio/`)
- [x] `PortfolioHome.razor` → route `/training/portfolio_home`, accès restreint au claim `training:PORTFOLIO`

### Réorganisation des pages

- [x] Pages déplacées dans sous-dossiers : `Public/`, `Admin/`, `Subscriber/`, `training_portfolio/`
- [x] Page `/myspace` (subscriber+) : ressources Google Sheets + formations achetées avec bouton Accéder → `training.PageUrl`
- [x] Menu : "Mon Espace" si connecté, "Télécharger" sinon → `/myspace`
- [x] Route `/trainings` ajoutée comme alias de `/catalog`
- [x] Page `/download` (subscriber+) : lien Google Sheets

---

## 3. Fait le 2026-06-10

### Stripe — étape 1

- [x] Package `Stripe.net 52.0.0` ajouté au projet
- [x] Placeholders `STRIPE_SECRET_KEY` et `STRIPE_WEBHOOK_SECRET` dans `.env`
- [x] `StripeService` : `CreateCheckoutSessionAsync(training, userId, successUrl, cancelUrl)` → URL Stripe. Lance `BusinessException("Stripe.NotConfigured")` si `stripe_price_id` absent.
- [x] Clé i18n `Stripe.NotConfigured` (FR/EN/ES)

### Auth — refonte complète codes 6 chiffres

- [x] `BusinessException` pattern : services lancent les erreurs métier, pages catchent uniquement `BusinessException`, le reste remonte à `ErrorBoundary`. Règle ajoutée dans `CLAUDE.md` (5b).
- [x] `CodeService` (singleton) : code 6 chiffres en mémoire, expiration 20 min, max 5 tentatives
- [x] Inscription `/register` : code envoyé avant création du compte, user créé en DB seulement après validation du code
- [x] Email de bienvenue envoyé après inscription via template `welcome`
- [x] Mot de passe oublié `/forgot-password` : flux code 6 chiffres, anti-énumération préservé
- [x] Changement de mot de passe `/reset-password` : code envoyé à l'email courant
- [x] Changement d'email `/profile` : code envoyé au nouvel email, changement en DB après validation
- [x] Colonne `email_confirmed` supprimée de la DB, du modèle `User` et de `AuthService`
- [x] `TokenService`, `EmailToken`, `ConfirmEmail.razor` supprimés. Table `email_tokens` supprimée.
- [x] Templates emails : `{{ .ConfirmationURL }}` remplacé par `{{ .Code }}` (HTML FR/EN/ES fourni)
- [x] Bug ResetPassword (double-render Blazor) : corrigé avec `OnAfterRenderAsync(firstRender)`
- [x] Bug logout "Not Found" : `MapGet("/auth/logout")` ajouté
- [x] Bug Register figé sur erreur SMTP : exception catchée, circuit Blazor préservé
- [x] Validation métier (longueur mot de passe, email déjà pris) centralisée dans `UserService`

---

## 4. Fait le 2026-06-09

### Édition des formations

- [x] `TrainingService.UpdateAsync` : mise à jour des 6 champs
- [x] Bouton édition par ligne dans `AdminTraining.razor` + snackbar confirmation

### Contact et catalogue

- [x] Page `/contact` : formulaire nom/email/message, envoi via `EmailService`, i18n FR/EN/ES
- [x] Menu Formation et CTA AccessDenied redirigés vers `/catalog`
- [x] Catalogue : message + bouton Contact si aucune formation

### Inscription (base, refondue le 06-10)

- [x] `UserService.RegisterAsync` + page `/register` + email `confirm_signup` via template
- [x] Page `/confirm-email?token=` : validation jeton, `email_confirmed=1`
- [x] Lien S'inscrire dans le header, clés i18n FR/EN/ES

---

## 5. Fait le 2026-06-08

### Documentation et conventions

- [x] `CLAUDE.md` réduit + annexes `ARCHITECTURE.md`, `AUTH.md`, `I18N.md`, `database.md`, `PROCESSUS_TRAVAIL.md`
- [x] Convention tout en anglais, colonne `language`, règle `dotnet watch` documentées

### Renommage anglais

- [x] Table `trainings` (ex `formations`), colonne `language`, modèle et service mis à jour
- [x] `email_templates.lang` → `language` partout
- [x] Routes et fichiers `.razor` en anglais : `/profile`, `/download`, `/training`, `/catalog`, `/client-area`, `/access-denied`, `/forgot-password`
- [x] Clés i18n en anglais dans les 3 fichiers `.resx`

### Login et langue

- [x] Cookie de langue posé au login depuis `user.Language`
- [x] Sélecteur FR/EN/ES fiable via `NavigateTo(url, forceLoad:true)`
- [x] Login : validation C# en premier, erreur affichée sur place, vrai POST seulement si identifiants corrects

### Admin et utilisateurs

- [x] `/admin/users` : liste + suppression (sauf admin)
- [x] `/admin/trainings` : liste + ajout + suppression
- [x] `/catalog` : formations par langue + boutons Acheter

---

## 6. Fait le 2026-06-07

### Authentification

- [x] Auth cookie ASP.NET Core sur la table `users`, bcrypt
- [x] 3 niveaux d'accès : public, `[Authorize]`, `[Authorize(Roles="client,admin")]`
- [x] Endpoints `/auth/login` et `/auth/logout`, header dynamique

### Gestion des utilisateurs

- [x] `UserService` : `GetByIdAsync`, `GetByEmailAsync`, `CreerAsync`, `ChangerMotDePasseAsync`, `SauvegarderProfilAsync`

---

## 7. Fait le 2026-06-06

### Socle

- [x] Blazor Server, MudBlazor, EF Core + Pomelo MySQL
- [x] `UseExceptionHandler` + `ErrorBoundary` global
- [x] Secrets dans `.env` via DotNetEnv

### Multilingue et pages de base

- [x] `IStringLocalizer<SharedResource>` + `.resx` FR/EN/ES
- [x] Sélecteur de langue dans le header
- [x] Pages : accueil, légal (CGV / Confidentialité / Mentions)
