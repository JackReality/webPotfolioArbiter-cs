using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
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

    public UserService(AppDbContext db)
    {
        _db = db;
    }

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

    /// <summary>
    /// Écriture : CRÉE un compte. On hache le mot de passe (jamais en clair),
    /// on ajoute la ligne, puis on enregistre en base.
    /// Renvoie l'utilisateur créé (avec son Id rempli par la base).
    /// </summary>
    public async Task<User> CreerAsync(string email, string motDePasse, string displayName)
    {
        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(motDePasse),
            Role = "subscriber",   // rôle par défaut d'un nouvel inscrit
        };

        _db.Users.Add(user);            // <-- le fameux "Add"
        await _db.SaveChangesAsync();   // c'est ICI que ça part en base MySQL
        return user;
    }

    /// <summary>
    /// Écriture : MODIFIE le mot de passe d'un utilisateur existant.
    /// </summary>
    public async Task<bool> ChangerMotDePasseAsync(ulong id, string nouveauMotDePasse)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
        var rowsAffected = await _db.SaveChangesAsync();
        return rowsAffected > 0;
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
}
