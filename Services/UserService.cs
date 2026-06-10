using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Exceptions;
using Portfolio.Models;

namespace Portfolio.Services;

/// <summary>
/// Gestion des utilisateurs (table "users") : créer, lire, modifier.
/// Le DbContext lui est injecté par le constructeur (injection de dépendances).
/// Les pages appellent ce service ; elles ne touchent jamais le DbContext directement.
///
/// À ne pas confondre avec AuthService, qui ne fait QUE vérifier un mot de passe
/// au moment de la connexion. Ici, c'est la gestion du compte lui-même.
/// </summary>
public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    /// <summary>Lecture : retrouve un utilisateur par son id (null si introuvable).</summary>
    public async Task<User?> GetByIdAsync(ulong id)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>Lecture : TOUS les utilisateurs, du plus récent au plus ancien.</summary>
    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Écriture : SUPPRIME un utilisateur.
    /// Sécurité : on REFUSE de supprimer un compte ayant le rôle "admin".
    /// Grâce à la clé étrangère "ON DELETE CASCADE", supprimer l'utilisateur
    /// efface aussi automatiquement ses jetons (table email_tokens).
    /// Renvoie false si l'utilisateur est introuvable.
    /// </summary>
    public async Task<bool> SupprimerAsync(ulong id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return false;

        if (user.Role == "admin")
            throw new InvalidOperationException("Impossible de supprimer un administrateur.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Lecture : retrouve un utilisateur par son email (null si introuvable).</summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>Lecture : vérifie si un email est déjà utilisé.</summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    /// <summary>
    /// Écriture : INSCRIPTION d'un nouvel utilisateur.
    /// Lance BusinessException("Register.EmailTaken") si l'email est déjà utilisé.
    /// Sinon crée le compte (bcrypt, rôle subscriber, email_confirmed=false) et renvoie l'id.
    /// </summary>
    public async Task<ulong> RegisterAsync(string email, string password, string displayName, string language)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
            throw new BusinessException("Register.EmailTaken");

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Language = language,
            Role = "subscriber",
            CreatedAt = DateTime.UtcNow,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    /// <summary>
    /// Écriture : MODIFIE le mot de passe d'un utilisateur existant.
    /// Lance BusinessException("ChangePassword.TooShort") si trop court (1 car en dev, 6 en prod).
    /// </summary>
    public async Task ChangerMotDePasseAsync(ulong id, string nouveauMotDePasse)
    {
        if (string.IsNullOrWhiteSpace(nouveauMotDePasse))
            throw new BusinessException("ChangePassword.TooShort");

        var isDev = string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development", StringComparison.OrdinalIgnoreCase);
        var minLength = isDev ? 1 : 6;

        if (nouveauMotDePasse.Length < minLength)
            throw new BusinessException("ChangePassword.TooShort");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            throw new BusinessException("ChangePassword.Error");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Écriture : SAUVEGARDE les modifications d'un utilisateur (nom, email, langue, avatar).
    /// Comme l'utilisateur a été lu via le DbContext, EF Core le suit déjà
    /// et détecte les changements. On appelle juste SaveChangesAsync.
    /// </summary>
    public async Task SauvegarderAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Écriture : SAUVEGARDE UNIQUEMENT le nom affiché et la langue du profil.
    /// L'email et les autres champs sensibles ne sont PAS modifiés ici
    /// (l'email nécessite une confirmation par mail).
    /// </summary>
    public async Task SauvegarderProfilAsync(ulong id, string displayName, string language)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            throw new Exception("Utilisateur introuvable.");

        user.DisplayName = displayName;
        user.Language = language;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Vérifie si le mot de passe actuel est correct pour un utilisateur donné.
    /// Renvoie false si l'utilisateur n'existe pas ou si le mot de passe ne correspond pas.
    /// </summary>
    public async Task<bool> VerifierMotDePasseActuelAsync(ulong id, string motDePasse)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return false;

        return BCrypt.Net.BCrypt.Verify(motDePasse, user.PasswordHash);
    }

    /// <summary>
    /// Écriture : CHANGE l'email d'un utilisateur.
    /// Lance BusinessException("Register.EmailTaken") si l'email est déjà pris.
    /// </summary>
    public async Task ChangeEmailAsync(ulong id, string newEmail)
    {
        var taken = await _db.Users.AnyAsync(u => u.Email == newEmail);
        if (taken)
            throw new BusinessException("Register.EmailTaken");

        var user = await _db.Users.FindAsync(id);
        if (user is null)
            throw new BusinessException("ChangePassword.Error");

        user.Email = newEmail;
        await _db.SaveChangesAsync();
    }
}
