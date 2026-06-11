using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public class EmailTemplateService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EmailTemplateService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.EmailTemplates
            .OrderBy(t => t.Key)
            .ThenBy(t => t.Language)
            .ToListAsync();
    }

    public async Task<List<EmailTemplate>> GetAllByTypeAsync(string key)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.EmailTemplates
            .Where(t => t.Key == key)
            .OrderBy(t => t.Language)
            .ToListAsync();
    }

    public async Task<EmailTemplate?> GetAsync(string key, string language)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Key == key && t.Language == language);

        if (template is null && language != "fr")
        {
            template = await db.EmailTemplates
                .FirstOrDefaultAsync(t => t.Key == key && t.Language == "fr");
        }

        return template;
    }

    public async Task SaveAsync(ulong id, string subject, string html)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var template = await db.EmailTemplates.FindAsync(id);
        if (template is null)
            throw new Exception("Template introuvable.");

        template.Subject = subject;
        template.Html = html;
        template.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
