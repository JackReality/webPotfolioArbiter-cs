using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

/// <summary>
/// Un utilisateur = une ligne de la table MySQL "users" (déjà existante).
/// Les attributs [Table]/[Column] font le pont entre les noms C# (PascalCase)
/// et les noms de colonnes de la base (snake_case). On ne recrée PAS la table.
/// </summary>
[Table("users")]
public class User
{
    [Column("id")]
    public ulong Id { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    // Mot de passe HACHÉ avec bcrypt (jamais le mot de passe en clair).
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("language")]
    public string Language { get; set; } = "fr";

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    // Rôle = le niveau d'accès : "visitor", "subscriber", "client", "admin".
    [Column("role")]
    public string Role { get; set; } = "subscriber";

    [Column("email_confirmed")]
    public bool EmailConfirmed { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
