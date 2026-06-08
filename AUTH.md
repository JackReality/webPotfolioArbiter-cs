# Authentification & rôles

> Annexe de [`CLAUDE.md`](CLAUDE.md). Mécanisme d'auth, rôles, protection des pages.

## Mécanisme
Cookie natif .NET (`AddCookie`), **pas** ASP.NET Identity. Après connexion réussie, un cookie
chiffré contient l'identité : id, email, nom et **rôle** (claims). Mots de passe hachés **bcrypt**.

## Flux de connexion / déconnexion
- La page `/login` **poste** vers l'endpoint **`/auth/login`** (dans `Program.cs`). Cet endpoint :
  1. appelle `AuthService.VerifierIdentifiantsAsync` (vérif bcrypt),
  2. pose le cookie d'identité (`SignInAsync`),
  3. pose aussi le **cookie de langue** depuis `user.Language` (voir [`I18N.md`](I18N.md)),
  4. redirige (`LocalRedirect`).
- Déconnexion : endpoint **`/auth/logout`** (`SignOutAsync`).

> Pourquoi des endpoints et pas un bouton ? Poser/retirer le cookie exige le `HttpContext`,
> indisponible dans un composant interactif Blazor Server.

## Rôles
Champ `User.Role` : **`visitor`**, **`subscriber`**, **`client`**, **`admin`**.

## Protéger une page
En haut du `.razor` :
- `@attribute [Authorize]` → réservé aux **connectés** ;
- `@attribute [Authorize(Roles = "client,admin")]` → réservé à certains **rôles**.

## Si l'accès est refusé
Géré dans `Routes.razor` (`AuthorizeRouteView`) :
- non connecté → `RedirectToLogin` (vers `/login`) ;
- connecté mais mauvais rôle → page d'accès réservé.

## Lire l'utilisateur courant dans une page
Via une `<AuthorizeView>` **ou** un paramètre en cascade `Task<AuthenticationState>`
(activé par `AddCascadingAuthenticationState` dans `Program.cs`).

> ⚠️ Le claim `Name` (nom affiché) est figé dans le cookie au moment du login : après un
> changement de pseudo, il reste périmé jusqu'à reconnexion (point connu, voir `STATUS.md`).
