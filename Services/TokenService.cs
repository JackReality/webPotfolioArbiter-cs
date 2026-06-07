using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

/// <summary>
/// Gestion des jetons temporaires (table "email_tokens") : génération, validation et suppression.
/// Utilise IServiceScopeFactory pour éviter les conflits de concurrence (concurrency) dans Blazor Server.
/// </summary>
public class TokenService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TokenService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Génère un nouveau jeton pour un utilisateur et un objectif donné (ex: "recovery").
    /// Le jeton est valide pendant la durée spécifiée (par défaut 1 heure).
    /// Renvoie le jeton généré (à envoyer par email).
    /// </summary>
    public async Task<string> GenererJetonsAsync(ulong userId, string purpose, int dureeHeures = 1)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var token = Guid.NewGuid().ToString("N");

        var emailToken = new EmailToken
        {
            UserId = userId,
            Token = token,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddHours(dureeHeures)
        };

        db.EmailTokens.Add(emailToken);
        await db.SaveChangesAsync();

        return token;
    }

    /// <summary>
    /// Récupère l'ID de l'utilisateur associé à un jeton valide, sans le supprimer.
    /// Renvoie null si le jeton est invalide ou expiré.
    /// </summary>
    public async Task<ulong?> GetUserIdFromTokenAsync(string token, string purpose)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var emailToken = await db.EmailTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.Purpose == purpose);

        if (emailToken == null || emailToken.ExpiresAt < DateTime.UtcNow)
            return null;

        return emailToken.UserId;
    }

    /// <summary>
    /// Supprime un jeton après utilisation (ou s'il est invalide) pour éviter la réutilisation.
    /// </summary>
    public async Task SupprimerJetonAsync(string token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var emailToken = await db.EmailTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (emailToken != null)
        {
            db.EmailTokens.Remove(emailToken);
            await db.SaveChangesAsync();
        }
    }
}