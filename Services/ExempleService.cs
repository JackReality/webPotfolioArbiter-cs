using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public class ExempleService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ExempleService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<ExempleEntite>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Exemples.AsNoTracking().ToListAsync();
    }

    public async Task AjouterAsync(string nom)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Exemples.Add(new ExempleEntite { Nom = nom });
        await db.SaveChangesAsync();
    }
}
