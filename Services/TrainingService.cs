using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

/// <summary>
/// Gestion des formations (table "trainings") : lister, créer, supprimer.
/// Le DbContext est injecté par le constructeur ; les pages appellent ce service,
/// jamais le DbContext directement.
/// </summary>
public class TrainingService
{
    private readonly AppDbContext _db;

    public TrainingService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lecture : toutes les formations (toutes langues), de la plus récente à la plus ancienne.</summary>
    public async Task<List<Training>> GetAllAsync()
    {
        return await _db.Trainings
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Lecture : les formations d'une langue donnée (pour le catalogue public).</summary>
    public async Task<List<Training>> GetByLanguageAsync(string language)
    {
        return await _db.Trainings
            .Where(t => t.Language == language)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Lecture : une formation par son id (null si introuvable).</summary>
    public async Task<Training?> GetByIdAsync(ulong id)
    {
        return await _db.Trainings.FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Écriture : CRÉE une formation et l'enregistre en base.
    /// Renvoie la formation créée (avec son Id rempli par la base).
    /// </summary>
    public async Task<Training> CreerAsync(
        string title, string language, string descriptionHtml,
        string? stripeProductId, string? stripePriceId, string? confirmationEmailHtml)
    {
        var training = new Training
        {
            Title = title,
            Language = language,
            DescriptionHtml = descriptionHtml,
            StripeProductId = stripeProductId,
            StripePriceId = stripePriceId,
            ConfirmationEmailHtml = confirmationEmailHtml,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Trainings.Add(training);
        await _db.SaveChangesAsync();
        return training;
    }

    /// <summary>
    /// Écriture : MET À JOUR une formation existante (les 6 champs éditables).
    /// On conserve <see cref="Training.CreatedAt"/>. Renvoie false si introuvable.
    /// </summary>
    public async Task<bool> UpdateAsync(
        ulong id,
        string title, string language, string descriptionHtml,
        string? stripeProductId, string? stripePriceId, string? confirmationEmailHtml)
    {
        var training = await _db.Trainings.FindAsync(id);
        if (training is null)
            return false;

        training.Title = title;
        training.Language = language;
        training.DescriptionHtml = descriptionHtml;
        training.StripeProductId = stripeProductId;
        training.StripePriceId = stripePriceId;
        training.ConfirmationEmailHtml = confirmationEmailHtml;

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Écriture : SUPPRIME une formation. Renvoie false si introuvable.</summary>
    public async Task<bool> SupprimerAsync(ulong id)
    {
        var training = await _db.Trainings.FindAsync(id);
        if (training is null)
            return false;

        _db.Trainings.Remove(training);
        await _db.SaveChangesAsync();
        return true;
    }
}
