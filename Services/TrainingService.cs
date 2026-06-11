using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public class TrainingService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public TrainingService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Training>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Trainings
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Training>> GetByLanguageAsync(string language)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Trainings
            .Where(t => t.Language == language)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Training?> GetByIdAsync(ulong id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Trainings.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Training>> GetByCodesAsync(IEnumerable<string> codes)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var list = codes.ToList();
        return await db.Trainings
            .Where(t => list.Contains(t.Code))
            .ToListAsync();
    }

    public async Task<Training?> GetByCodeAsync(string code)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Trainings.FirstOrDefaultAsync(t => t.Code == code);
    }

    public async Task<Training> CreerAsync(
        string title, string language, string descriptionHtml, string code,
        string? pageUrl, string? stripeProductId, string? stripePriceId, string? confirmationEmailHtml)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var training = new Training
        {
            Title = title,
            Language = language,
            DescriptionHtml = descriptionHtml,
            Code = code,
            PageUrl = pageUrl,
            StripeProductId = stripeProductId,
            StripePriceId = stripePriceId,
            ConfirmationEmailHtml = confirmationEmailHtml,
            CreatedAt = DateTime.UtcNow,
        };

        db.Trainings.Add(training);
        await db.SaveChangesAsync();
        return training;
    }

    public async Task<bool> UpdateAsync(
        ulong id,
        string title, string language, string descriptionHtml, string code,
        string? pageUrl, string? stripeProductId, string? stripePriceId, string? confirmationEmailHtml)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var training = await db.Trainings.FindAsync(id);
        if (training is null)
            return false;

        training.Title = title;
        training.Language = language;
        training.DescriptionHtml = descriptionHtml;
        training.Code = code;
        training.PageUrl = pageUrl;
        training.StripeProductId = stripeProductId;
        training.StripePriceId = stripePriceId;
        training.ConfirmationEmailHtml = confirmationEmailHtml;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SupprimerAsync(ulong id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var training = await db.Trainings.FindAsync(id);
        if (training is null)
            return false;

        db.Trainings.Remove(training);
        await db.SaveChangesAsync();
        return true;
    }
}
