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
    /// Pas besoin d'appeler .Update() : comme on a LU le user via _db.Users,
    /// EF Core le "suit" déjà et détecte tout seul le changement.
    /// Renvoie false si l'utilisateur n'existe pas.
    /// </summary>
    public async Task<bool> ChangerMotDePasseAsync(ulong id, string nouveauMotDePasse)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
        await _db.SaveChangesAsync();
        return true;
    }
}
