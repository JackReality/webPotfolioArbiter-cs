# État du projet — Portfolio Arbiter

Règle : 7 sections maximum. Section 1 = À faire. Sections 2-7 = Fait le YYYY-MM-DD (la plus ancienne est supprimée quand une nouvelle est ajoutée).

---

## 1. À faire

### Stripe — paiement

Principe : le rôle passe de `subscriber` à `client` uniquement après vérification serveur que Stripe a bien encaissé le paiement.

Tâches Jack :
- [ ] Mettre la vraie clé live dans `.env` : `STRIPE_SECRET_KEY=sk_live_...`
- [ ] Dans le dashboard Stripe, créer un webhook pointant vers `https://portfolioarbiter.com/stripe/webhook`, événement `checkout.session.completed`, puis copier la clé `whsec_...` dans `.env` : `STRIPE_WEBHOOK_SECRET=whsec_...` *(pour étape 3)*

Tâches Claude :
- [x] Étape 1 — Terrain : package `Stripe.net`, placeholders `.env`, `StripeService.CreateCheckoutSessionAsync`
- [ ] Étape 2 — Bouton Acheter dans le catalogue : non connecté → `/login`, connecté → session Stripe → redirect
- [ ] Étape 3 — Pages de retour : `/stripe-ok` (vérifie paiement via Stripe, monte le rôle) + `/stripe-error`
- [ ] Étape 4 — Email de confirmation après achat via `training.ConfirmationEmailHtml`

### Pages de contenu

- [ ] `/download` : bouton de copie de la Google Sheet selon le rôle
- [ ] `/training` : présentation de la formation + bouton achat
- [ ] `/client-area` : contenu réservé aux clients

### Admin

- [ ] Gestion des rôles utilisateurs (liste et suppression déjà faites, manque la modification)
- [ ] Finaliser l'éditeur des templates d'emails (FR/EN/ES)

### Divers

- [ ] Remplir l'email de contact dans les Mentions légales (actuellement `contact@realityexplorer.com`)
- [ ] Décider du sort de la table `schema_migrations` (supprimable)
- [ ] `EmailService` : mapping SSL explicite `465 → SslOnConnect` / `587 → StartTls` (optionnel, `Auto` fonctionne)

---

## 2. Fait le 2026-06-10

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

## 3. Fait le 2026-06-09

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

## 4. Fait le 2026-06-08

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

## 5. Fait le 2026-06-07

### Authentification

- [x] Auth cookie ASP.NET Core sur la table `users`, bcrypt
- [x] 3 niveaux d'accès : public, `[Authorize]`, `[Authorize(Roles="client,admin")]`
- [x] Endpoints `/auth/login` et `/auth/logout`, header dynamique

### Gestion des utilisateurs

- [x] `UserService` : `GetByIdAsync`, `GetByEmailAsync`, `CreerAsync`, `ChangerMotDePasseAsync`, `SauvegarderProfilAsync`

---

## 6. Fait le 2026-06-06

### Socle

- [x] Blazor Server, MudBlazor, EF Core + Pomelo MySQL
- [x] `UseExceptionHandler` + `ErrorBoundary` global
- [x] Secrets dans `.env` via DotNetEnv

### Multilingue et pages de base

- [x] `IStringLocalizer<SharedResource>` + `.resx` FR/EN/ES
- [x] Sélecteur de langue dans le header
- [x] Pages : accueil, légal (CGV / Confidentialité / Mentions)
