using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

/// <summary>
/// Templates d'emails (table "email_templates").
/// 1 ligne = 1 type de mail + 1 langue.
/// Le nom de colonne "key" est un mot réservé SQL → [Column("key")] obligatoire.
/// </summary>
[Table("email_templates")]
public class EmailTemplate
{
    [Column("id")]
    public ulong Id { get; set; }

    [Column("key")]
    public string Key { get; set; } = string.Empty;   // ex: "confirm_signup", "recovery", "welcome"

    [Column("language")]
    public string Language { get; set; } = "fr";      // "fr", "en", "es"

    [Column("subject")]
    public string Subject { get; set; } = string.Empty;

    [Column("html")]
    public string Html { get; set; } = string.Empty;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
