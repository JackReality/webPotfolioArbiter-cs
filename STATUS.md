# État du projet — Portfolio Arbiter

> Site de vente de formations (Blazor Server .NET 10 · MudBlazor · MySQL/Pomelo).
> Ce fichier suit l'avancement. Dernière mise à jour : 2026-06-06.

---

## ✅ Fait

### Socle & conventions
- Architecture Blazor Server, projet unique, MudBlazor, EF Core + Pomelo (MySQL).
- **Gestion d'erreurs centralisée** : `UseExceptionHandler("/Error")` + `<ErrorBoundary>` global dans `MainLayout` (voir `Doc-du-projet.txt`).
- Secrets dans `.env` (DotNetEnv). ⚠️ **`DB_HOST=127.0.0.1`** obligatoire (le user MySQL `portfolio` refuse `localhost`). Base = `portfolio_arbiter`.

### Multilingue (FR / EN / ES)
- `IStringLocalizer<SharedResource>` + `.resx` (FR par défaut/repli, EN + ES remplis).
- Sélecteur de langue dans le header (cookie + rechargement), `<html lang>` dynamique.
- **Traduit** : page d'accueil, page légale, menu, pied de page.

### Pages
- **Accueil** (`/`) : 6 sections façon maquette (hero + position + stratégie + transactions + rendement + arbitrage).
- **Légal** (`/legal`) : 3 onglets — CGV (9 art.), Confidentialité (7 art.), Mentions (5 art.). Email de contact anti-robots (révélé au clic, `contact@realityexplorer.com`).
- Pages vierges : `Telecharger`, `Formation`, `Login`, `Register`, `Reset`, `EspaceClient`, `AccesReserve`.

### Authentification & accès (3 niveaux) — étape 1
- Auth par **cookie officiel ASP.NET Core** sur la table `users` (bcrypt via BCrypt.Net-Next).
- 3 niveaux par attribut : public / `[Authorize]` / `[Authorize(Roles="client,admin")]`.
- Endpoints `/auth/login` et `/auth/logout`. Header dynamique (Connexion ↔ nom + Déconnexion).
- **Testé OK** : login, mauvais mot de passe rejeté, redirection des pages protégées, accès réservé client.
- Compte de test : `facebook@grillet.ch` / `Test1234!` (subscriber) · `jack@grillet.ch` (admin).

### Gestion des utilisateurs (CRUD) — étape 1
- **`Services/UserService.cs` créé** (enregistré dans `Program.cs` via `AddScoped`). Méthodes :
  `GetByIdAsync`, `GetByEmailAsync`, `CreerAsync` (hache bcrypt + `Add` + `SaveChanges`),
  `ChangerMotDePasseAsync`. ⚠️ Pas encore branché à une page → à utiliser dans `/register` et `/reset`.
- Rappel archi (la « recette en 4 gestes » pour CHAQUE table) :
  1. Model dans `Models/` (1 classe = 1 table, ex. `User`).
  2. `DbSet<T>` dans `AppDbContext` (nom au pluriel = la table, ex. `Users`).
  3. Service dans `Services/` (la logique, reçoit le `DbContext` par injection).
  4. `builder.Services.AddScoped<XxxService>()` dans `Program.cs` (sinon `@inject` impossible).

---

## ✅ Fait

### Auth étape 2 : Changement de mot de passe et réinitialisation (Lot 1 terminé)
- [x] Modèle `EmailToken` et `AppDbContext` configurés.
- [x] `TokenService` créé pour générer, valider et supprimer les jetons de manière sécurisée (isolation des scopes Blazor via `IServiceScopeFactory`).
- [x] **Page unique `ResetPassword.razor`** : gère à la fois le flux "Profil" (`/profil/changer-mdp`) et le flux "Oubli" (`/reset-password?token=...`). **Simplifié** : ne demande plus l'ancien mot de passe dans les deux cas, uniquement "Nouveau" et "Confirmation".
- [x] Validation stricte avec composants **MudBlazor** (`MudForm`, `MudTextField`, `MudButton`) : affiche un message d'erreur clair si les mots de passe ne correspondent pas, **sans recharger la page**.
- [x] `Reset.razor` implémentée pour demander l'email, générer le jeton et envoyer le lien via `EmailService`.
- [x] `EmailService` (MailKit) configuré avec les variables `.env` (Infomaniak, port 465, `BASE_URL`, `MAIL_FROM`).
- [x] Règle de processus ajoutée dans `QWEN.md` : vérification OBLIGATOIRE du "Build succeeded" avant tout test utilisateur.

---

## 🤔 À réfléchir (décisions à figer)
- [ ] **Fusionner `AuthService` + `UserService` ?** Aujourd'hui : `AuthService` = vérifier le
      mot de passe (connexion) ; `UserService` = gérer le compte (créer/modifier). Choix à trancher
      quand le rôle de chaque objet sera bien digéré.
- [ ] **Préfixe `ft` sur les tables ?** Décision : NE PAS préfixer les classes C# (`User`, `Client`…).
      Si une table porte un préfixe (cas du projet en prod), le mettre UNIQUEMENT dans
      `[Table("ftUser")]` — la classe C# reste propre. À appliquer si on reprend cette base.
- [ ] **Documenter la « recette en 4 gestes » dans `CLAUDE.md`** comme convention officielle.

---

## 🔜 À faire

### ##Git local et sur github

### Notifier si erreur
- Créer un code global mailErrorSend() et l'intégrer dans erreur program.cs et mainlayout

### Ajouter dans Jack-PromptInitProjet 
- Ajouter la demande intégrer le login selon ce qui est fait actuellement

### Auth — étape 2 (prioritaire)
- [ ] **Inscription** (`/register`) : formulaire + création user (bcrypt) + email de confirmation
      via `email_templates` (FR/EN/ES) et jeton dans `email_tokens` (purpose `confirm`).
- [ ] **Mot de passe oublié** (`/reset`) : demande email → jeton `recovery` → page nouveau mot de passe.
- [ ] **Envoi d'emails** : service SMTP (Infomaniak ?) qui lit `email_templates` selon type + langue.
- [ ] Bloquer la connexion si `email_confirmed = 0` (au choix).
- [ ] Page **profil** (`/profile`) : changer pseudo, langue, photo, mot de passe.

### Contenu des pages
- [ ] **Télécharger** : bouton de copie de la Google Sheet (accès selon rôle).
- [ ] **Formation** : présentation + achat (passe le rôle à `client`).
- [ ] **Espace client** : contenu réservé aux clients.
- [ ] Traduire EN/ES le contenu de ces pages quand il existera (tout est dans les `jack-import/*.json`).

### Métier (cœur du site)
- [ ] **Achat de formation** → paiement (Stripe ?) → passage du rôle à `client`.
- [ ] **Forum** des visiteurs/clients.

### Admin (présent dans les JSON, pas encore implémenté)
- [ ] Gestion des utilisateurs (liste, rôles).
- [ ] Éditeur des `email_templates` (FR/EN/ES).

### Divers
- [ ] Décider du sort de la table `schema_migrations` (laissée pour l'instant, supprimable).
- [ ] Remplir l'email de contact réel dans les Mentions (actuellement `contact@realityexplorer.com`).
- [ ] Avant prod : revoir les CGV/mentions par un juriste ; ne pas exposer les détails d'erreur (déjà géré : seulement en Development).

---

## 🔑 Rappels techniques
- Lancer : `dotnet run` → http://localhost:5252 (https://localhost:7178).
- Base : `127.0.0.1:13306`, db `portfolio_arbiter`, user `portfolio`.
- Tables : `users`, `email_templates`, `email_tokens`, `schema_migrations`.
- Pas de migration EF : les entités sont mappées sur les tables existantes (`[Table]`/`[Column]`).
