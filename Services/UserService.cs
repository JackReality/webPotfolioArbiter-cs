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
    /// </summary>
    public async Task<bool> ChangerMotDePasseAsync(ulong id, string nouveauMotDePasse)
    {
        System.Console.WriteLine($"[DEBUG UserService] Début ChangerMotDePasseAsync pour ID: {id}");
        
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            System.Console.WriteLine($"[DEBUG UserService] ERREUR CRITIQUE: Utilisateur ID {id} introuvable en base.");
            return false;
        }
        
        System.Console.WriteLine($"[DEBUG UserService] Utilisateur trouvé: {user.Email}. Application du nouveau hash...");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
        
        System.Console.WriteLine($"[DEBUG UserService] Appel de SaveChangesAsync...");
        var rowsAffected = await _db.SaveChangesAsync();
        
        System.Console.WriteLine($"[DEBUG UserService] Résultat SaveChangesAsync: {rowsAffected} ligne(s) affectée(s).");
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
