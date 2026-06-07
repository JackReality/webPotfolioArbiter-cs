using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

/// <summary>
/// Gestion des templates d'emails (table "email_templates").
/// Permet de lire et modifier le contenu des emails envoyés aux utilisateurs.
/// </summary>
public class EmailTemplateService
{
    private readonly AppDbContext _db;

    public EmailTemplateService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retourne TOUS les templates, groupés par type (key).
    /// Ex : une liste de 9 lignes (3 types × 3 langues).
    /// </summary>
    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        return await _db.EmailTemplates
            .OrderBy(t => t.Key)
            .ThenBy(t => t.Lang)
            .ToListAsync();
    }

    /// <summary>
    /// Retourne les 3 templates (FR/EN/ES) pour un type donné.
    /// Ex : GetAllByTypeAsync("recovery") → 3 lignes.
    /// </summary>
    public async Task<List<EmailTemplate>> GetAllByTypeAsync(string key)
    {
        return await _db.EmailTemplates
            .Where(t => t.Key == key)
            .OrderBy(t => t.Lang)
            .ToListAsync();
    }

    /// <summary>
    /// Sauvegarde les modifications (subject + html) d'un template.
    /// </summary>
    public async Task SaveAsync(ulong id, string subject, string html)
    {
        var template = await _db.EmailTemplates.FindAsync(id);
        if (template is null)
            throw new Exception("Template introuvable.");

        template.Subject = subject;
        template.Html = html;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
