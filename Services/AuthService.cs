using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

/// <summary>
/// Logique d'authentification : retrouve un utilisateur par email et vérifie
/// son mot de passe contre le hash bcrypt stocké en base.
/// Le service NE pose PAS le cookie (ça, c'est l'endpoint /auth/login qui le fait,
/// car il a besoin du HttpContext).
/// </summary>
public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Renvoie l'utilisateur si l'email existe ET que le mot de passe correspond,
    /// sinon null. (On ne dit jamais lequel des deux est faux : sécurité.)
    /// </summary>
    public async Task<User?> VerifierIdentifiantsAsync(string email, string motDePasse)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return null;

        // bcrypt : compare le mot de passe saisi au hash stocké.
        var ok = BCrypt.Net.BCrypt.Verify(motDePasse, user.PasswordHash);
        return ok ? user : null;
    }
}
