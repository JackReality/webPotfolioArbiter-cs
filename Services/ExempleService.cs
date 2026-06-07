using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

/// <summary>
/// EXEMPLE de service = la logique métier.
/// Le DbContext lui est injecté par le constructeur (injection de dépendances).
/// Les pages appellent ce service ; elles ne touchent jamais le DbContext directement.
/// </summary>
public class ExempleService
{
    private readonly AppDbContext _db;

    public ExempleService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lecture : récupère toutes les lignes.</summary>
    public async Task<List<ExempleEntite>> GetAllAsync()
    {
        return await _db.Exemples.AsNoTracking().ToListAsync();
    }

    /// <summary>Écriture : ajoute une ligne puis enregistre en base.</summary>
    public async Task AjouterAsync(string nom)
    {
        _db.Exemples.Add(new ExempleEntite { Nom = nom });
        await _db.SaveChangesAsync();   // c'est ICI que ça part en base MySQL
    }
}
