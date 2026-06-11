using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Exceptions;
using Portfolio.Models;

namespace Portfolio.Services;

public class UserService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UserService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<User?> GetByIdAsync(ulong id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> SupprimerAsync(ulong id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return false;

        if (user.Role == "admin")
            throw new InvalidOperationException("Impossible de supprimer un administrateur.");

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<ulong> RegisterAsync(string email, string password, string displayName, string language)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var exists = await db.Users.AnyAsync(u => u.Email == email);
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

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

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

        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            throw new BusinessException("ChangePassword.Error");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
        await db.SaveChangesAsync();
    }

    public async Task SauvegarderAsync(User user)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task SauvegarderProfilAsync(ulong id, string displayName, string language)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(id);
        if (user is null)
            throw new Exception("Utilisateur introuvable.");

        user.DisplayName = displayName;
        user.Language = language;
        await db.SaveChangesAsync();
    }

    public async Task<bool> VerifierMotDePasseActuelAsync(ulong id, string motDePasse)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return false;

        return BCrypt.Net.BCrypt.Verify(motDePasse, user.PasswordHash);
    }

    public async Task ChangeEmailAsync(ulong id, string newEmail)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var taken = await db.Users.AnyAsync(u => u.Email == newEmail);
        if (taken)
            throw new BusinessException("Register.EmailTaken");

        var user = await db.Users.FindAsync(id);
        if (user is null)
            throw new BusinessException("ChangePassword.Error");

        user.Email = newEmail;
        await db.SaveChangesAsync();
    }
}
