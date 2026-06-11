using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public class UserTrainingService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UserTrainingService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task CreateAsync(ulong userId, string trainingCode, string? stripeSessionId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.UserTrainings.Add(new UserTraining
        {
            UserId = userId,
            TrainingCode = trainingCode,
            StripeSessionId = stripeSessionId,
            PurchasedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task<bool> HasAccessAsync(ulong userId, string trainingCode)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.UserTrainings
            .AnyAsync(ut => ut.UserId == userId && ut.TrainingCode == trainingCode);
    }

    public async Task<List<UserTraining>> GetByUserAsync(ulong userId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.UserTrainings
            .Where(ut => ut.UserId == userId)
            .OrderByDescending(ut => ut.PurchasedAt)
            .ToListAsync();
    }
}
