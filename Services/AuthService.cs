using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public enum LoginStatus { Success, InvalidCredentials }
public record LoginResult(LoginStatus Status, User? User = null);

public class AuthService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AuthService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<LoginResult> VerifierIdentifiantsAsync(string email, string motDePasse)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        var ok = BCrypt.Net.BCrypt.Verify(motDePasse, user.PasswordHash);
        if (!ok)
            return new LoginResult(LoginStatus.InvalidCredentials);

        return new LoginResult(LoginStatus.Success, user);
    }
}
