using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public enum LoginStatus { Success, InvalidCredentials }
public record LoginResult(LoginStatus Status, User? User = null);

public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LoginResult> VerifierIdentifiantsAsync(string email, string motDePasse)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        var ok = BCrypt.Net.BCrypt.Verify(motDePasse, user.PasswordHash);
        if (!ok)
            return new LoginResult(LoginStatus.InvalidCredentials);

        return new LoginResult(LoginStatus.Success, user);
    }
}
