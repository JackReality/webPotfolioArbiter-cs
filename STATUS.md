# État du projet — Portfolio Arbiter

> **Règle de structure de ce fichier** : 7 chapitres maximum.
> 1. "À faire"
> 2 à 7. "Fait le [YYYY-MM-DD]" (le plus ancien est supprimé quand un nouveau est ajouté).

---

## 1. À faire

### 🟢 Lot « Achat (Stripe) » — squelette, 1 sous-étape à la fois

> Décisions actées : **squelette seulement** (clés en placeholders dans `.env`, à remplir
> plus tard) ; montée en rôle `subscriber → client` par **vérification au retour** sur
> `/stripe-ok` (le serveur interroge Stripe pour confirmer le paiement). Un `admin` reste `admin`.
> ⚠️ Non testable tant que les vraies clés Stripe ne sont pas dans `.env`.

- [ ] **Étape 1 — Préparer le terrain.** Package `Stripe.net` (`.csproj`) + placeholders `STRIPE_SECRET_KEY` dans `.env` + `StripeService` (création d'une session Stripe Checkout à partir de `training.StripePriceId`, `success_url`=`/stripe-ok?session_id=...`, `cancel_url`=`/stripe-error`). *(rien de visible encore)*
- [ ] **Étape 2 — Brancher le bouton « Acheter »** du catalogue : si non connecté → `/login` ; sinon créer la session et rediriger vers l'URL Stripe (`NavigateTo(url, forceLoad:true)`).
- [ ] **Étape 3 — Pages de retour** : `/stripe-ok` (vérifie via Stripe que la session est payée, puis monte le rôle `subscriber → client`) et `/stripe-error` (paiement annulé/échoué).
- [ ] **Étape 4 — Email de confirmation** après achat, via `training.ConfirmationEmailHtml` (envoyé par `EmailService`).

### 🔵 Auth — étape 2 (prioritaire)
- [x] **Inscription** (`/register`) + confirmation email (`/confirm-email`) + lien « S'inscrire » dans le header.
- [ ] Bloquer la connexion si `email_confirmed = 0` (au choix).
- [ ] Page **profil** (`/profile`) : il reste la **photo** (changement pseudo/langue/mot de passe déjà faits).

### 🔧 Revue — restes à corriger
- [ ] `EmailService` : remplacer `SecureSocketOptions.Auto` par un mapping explicite (`465 → SslOnConnect`, `587 → StartTls`). *(actuellement `Auto` — choix volontairement gardé, voir Fait 06-08)*
- [ ] `TokenService` : pas de nettoyage des jetons expirés ; une nouvelle demande n'invalide pas les précédents (plusieurs liens valides en parallèle).
- [ ] `Profile` : après sauvegarde du pseudo, le claim `Name` du cookie reste périmé jusqu'à reconnexion (nom du header non rafraîchi).

### 📄 Contenu des pages
- [ ] **Download** (`/download`) : bouton de copie de la Google Sheet (accès selon rôle).
- [ ] **Training** (`/training`) : présentation + achat (passe le rôle à `client`).
- [ ] **Client area** (`/client-area`) : contenu réservé aux clients.

### 🏗️ Métier (cœur du site)
- [ ] **Achat de formation** → paiement (Stripe) → passage du rôle à `client`. *(voir lot Stripe)*
- [ ] **Forum** des visiteurs/clients.

### 🛠️ Admin
- [x] **Modifier une formation** (`/admin/trainings`) : édition via `_editingId` + `TrainingService.UpdateAsync`.
- [ ] Gestion des rôles utilisateurs (la **liste + suppression** est faite, voir Fait 06-08).
- [ ] Éditeur des `email_templates` (FR/EN/ES) — *partiellement fait, à finaliser*.

### 🔩 Divers & Technique
- [ ] **Notifier si erreur** : créer un code global `mailErrorSend()` et l'intégrer dans `Program.cs` (gestion d'erreur) et `MainLayout`.
- [ ] **Jack-PromptInitProjet** : ajouter la demande d'intégrer le login selon ce qui est fait actuellement.
- [ ] Décider du sort de la table `schema_migrations` (laissée pour l'instant, supprimable).
- [ ] Remplir l'email de contact réel dans les Mentions (actuellement `contact@realityexplorer.com`).

---

## 2. Fait le 2026-06-08

### Doc & conventions (tout en anglais)
- [x] **Doc remaniée.** `CLAUDE.md` 180→66 l. + annexes `ARCHITECTURE.md`, `AUTH.md`, `I18N.md` ; accès CLI DB + schéma dans `database.md` (ex-`BASE_DE_DONNEES.md`) ; workflow détaillé dans `PROCESSUS_TRAVAIL.md`. Convention (tout en anglais, colonne `language`) + règle `watch` gravées. Convention de nommage des pages documentée dans `database.md`.

### Renommage anglais : table, pages, clés i18n
- [x] **Table `trainings`** (ex-`formations`) + colonne `language` : modèle `Training`, `TrainingService` (`GetByLanguageAsync`), `DbSet Trainings`, route `/admin/trainings`, sélecteur de langue dans l'admin + colonne « Langue », **catalogue filtré sur la langue courante**.
- [x] **`email_templates.lang` → `language`** (DB + `EmailTemplate.Language` + `EmailTemplateService` + `LanguageTabs.razor`).
- [x] **Routes + fichiers `.razor` en anglais** : `/profil`→`/profile`, `/telecharger`→`/download` (`Download.razor`), `/formation`→`/training` (`Training.razor`), `/catalogue`→`/catalog`, `/espace-client`→`/client-area` (`ClientArea.razor`), `/acces-reserve`→`/access-denied` (`AccessDenied.razor`), `/reset`→`/forgot-password` (`ForgotPassword.razor`). Répercuté partout (`Program.cs`, `MainLayout`, `Routes.razor`, liens). **Vérifié** : nouvelles routes 302/200, anciennes 404.
- [x] **Clés i18n en anglais** (3 `.resx` + usages) : `Nav.Home/Download/Training/Login`, `Download.Title`, `Training.Title`, `AccessDenied.*`, `ForgotPassword.*`, `Legal.Terms.*`/`Legal.Tab.Terms`, `Legal.LegalNotice.*`/`Legal.Tab.LegalNotice`, + nouvelle clé `ClientArea.Title` (titre auparavant en dur). Valeurs/traductions inchangées → zéro impact visuel.

### Langue, login, mot de passe
- [x] **Langue au login** : `/auth/login` pose le cookie de langue depuis `user.Language`.
- [x] **Sélecteur FR/EN/ES fiable** : clic intercepté (`@onclick:preventDefault` + `SetCulture` → `NavigateTo(url, forceLoad:true)`) → changement systématique.
- [x] **Page mot de passe : une seule route** `/reset-password` (2ᵉ route retirée ; lien profil re-pointé).
- [x] **Bouton « Admin Dashboard »** du profil : déjà en `Color.Warning` (ambre), visible admins seulement. Conservé.
- [x] **Login : erreur sans rechargement.** Validation C# d'abord (`AuthService`) ; mauvais identifiants → message sur place ; bons identifiants → vrai POST (JS `submitForm`) pour poser le cookie.

### Admin, utilisateurs, catalogue
- [x] **`/admin/users`** (`AdminUsers.razor`) : liste (email, nom, rôle, langue, date) + **Supprimer** (sauf admin, côté UI + serveur). Bouton ajouté au dashboard.
- [x] **Cascade de suppression user vérifiée** : seule FK `email_tokens.user_id → users.id` en `ON DELETE CASCADE` → suppression propre. `email_templates`/`trainings` globaux. Doc : `database.md`.
- [x] **`/admin/trainings`** (`AdminTraining.razor`) : liste + ajout + suppression.
- [x] **`/catalog`** (`Catalog.razor`, publique) : formations (titre + HTML) séparées par `MudDivider`, boutons « Acheter » (message « Stripe à configurer »).

### Revue du commit `c30d584` (corrections)
- [x] Templates d'emails admin branchés sur le mail réel (`ForgotPassword.razor` lit le template `recovery`, repli FR + repli HTML minimal).
- [x] Pages admin protégées par `@attribute [Authorize(Roles = "admin")]`.
- [x] Bug `LanguageTabs` (texte qui revenait) : `@bind` direct sur l'objet en mémoire.
- [x] Erreur SMTP brute **non révélée** à l'utilisateur (loggée serveur, succès générique anti-énumération).
- [x] `Console.WriteLine` de debug retirés (`UserService.ChangerMotDePasseAsync`).
- [x] Langue du profil appliquée (option A : `/Culture/Set` + forceLoad, message via `?saved=1`).
- [x] `AdminEmailTemplates` : spinner infini si table vide corrigé (flag `_loaded`).
- ~~`EmailService` : `Auto` → mapping SSL explicite.~~ **Abandonné** : le port vient du `.env` et `Auto` choisit le bon mode → adapté. *(une variante « mapping explicite » reste listée en À faire, optionnelle.)*

### Auth étape 2 : Changement de mot de passe et réinitialisation
- [x] Modèle `EmailToken` et `AppDbContext` configurés.
- [x] `TokenService` (générer / valider / supprimer les jetons ; isolation des scopes Blazor via `IServiceScopeFactory`).
- [x] Page unique `ResetPassword.razor` (flux « Profil » + « Oubli »), simplifiée (ne demande plus l'ancien mot de passe).
- [x] Validation stricte (MudBlazor `MudForm`/`MudTextField`/`MudButton`).
- [x] `ForgotPassword.razor` : demande l'email, génère le jeton, envoie le lien via `EmailService`.
- [x] `EmailService` (MailKit) configuré avec `.env` (Infomaniak, port 465, `BASE_URL`, `MAIL_FROM`).

---

## 3. Fait le 2026-06-07

### Authentification & accès (étape 1)
- [x] Auth par cookie officiel ASP.NET Core sur la table `users` (bcrypt via BCrypt.Net-Next).
- [x] 3 niveaux par attribut : public / `[Authorize]` / `[Authorize(Roles="client,admin")]`.
- [x] Endpoints `/auth/login` et `/auth/logout`. Header dynamique.
- [x] Compte de test : `facebook@grillet.ch` / `Test1234!` (subscriber) · `jack@grillet.ch` (admin).

### Gestion des utilisateurs (CRUD)
- [x] `Services/UserService.cs` créé et enregistré dans `Program.cs`.
- [x] Méthodes : `GetByIdAsync`, `GetByEmailAsync`, `CreerAsync` (hache bcrypt), `ChangerMotDePasseAsync`, `SauvegarderProfilAsync`.

---

## 4. Fait le 2026-06-06

### Socle & conventions
- [x] Architecture Blazor Server, projet unique, MudBlazor, EF Core + Pomelo (MySQL).
- [x] Gestion d'erreurs centralisée : `UseExceptionHandler("/Error")` + `<ErrorBoundary>` global dans `MainLayout`.
- [x] Secrets dans `.env` (DotNetEnv). `DB_HOST=127.0.0.1` obligatoire. Base = `portfolio_arbiter`.

### Multilingue (FR / EN / ES)
- [x] `IStringLocalizer<SharedResource>` + `.resx` (FR par défaut/repli, EN + ES remplis).
- [x] Sélecteur de langue dans le header (cookie + rechargement), `<html lang>` dynamique.

### Pages de base
- [x] Accueil (`/`) : 6 sections façon maquette.
- [x] Légal (`/legal`) : 3 onglets (CGV, Confidentialité, Mentions). Email de contact anti-robots.
- [x] Pages vierges créées (depuis renommées en anglais — voir Fait 06-08).

---

## 5. Fait le 2026-06-09

### Lot « Édition formation »
- [x] `TrainingService.UpdateAsync` : met à jour les 6 champs, garde `CreatedAt`.
- [x] Bouton ✏️ par ligne dans `AdminTraining.razor` → mode édition (`_editingId`) + snackbar « Formation modifiée. »

### Lot « Contact + Catalog »
- [x] Page `/contact` (`Contact.razor`) : formulaire nom/email/message, envoi via `EmailService`, clés i18n `Contact.*` (FR/EN/ES), lien au menu.
- [x] Menu « Formation » et CTA `AccessDenied` redirigés vers `/catalog`.
- [x] Catalog : état vide remplacé par message + bouton `/contact` (`Catalog.Empty`).

### Auth étape 2 — Inscription
- [x] `UserService.RegisterAsync` (vérifie email, crée user bcrypt, `email_confirmed=0`) + `ConfirmEmailAsync`.
- [x] Page `/register` : formulaire + envoi email `confirm_signup` via template MySQL (placeholder `{{ .ConfirmationURL }}`), lien de confirmation valide 24h.
- [x] Page `/confirm-email?token=` : valide le jeton, met `email_confirmed=1`, supprime le token.
- [x] Lien « S'inscrire » (`Nav.Register`) dans le header (visible si non connecté). Clés i18n FR/EN/ES.

## 6. Fait le [À venir]
*(Aucun historique supplémentaire pour le moment)*

## 7. Fait le [À venir]
*(Aucun historique supplémentaire pour le moment)*
