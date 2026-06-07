using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data;

/// <summary>
/// Le DbContext = le pont entre tes objets C# et la base MySQL.
/// Il est injecté automatiquement via le constructeur (configuré dans Program.cs).
/// Chaque DbSet&lt;T&gt; correspond à une table.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 1 ligne = 1 table. Ajoute ici tes futures tables.
    public DbSet<ExempleEntite> Exemples => Set<ExempleEntite>();

    // La table des utilisateurs (login, rôles).
    public DbSet<User> Users => Set<User>();

    // Les templates d'emails (multilingues : 1 ligne = 1 type + 1 langue).
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    // Les jetons temporaires (confirmation, réinitialisation de mot de passe).
    public DbSet<EmailToken> EmailTokens => Set<EmailToken>();
}
