using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

/// <summary>
/// Représente un jeton temporaire (ex: confirmation d'email, réinitialisation de mot de passe).
/// Mappé sur la table existante "email_tokens".
/// </summary>
[Table("email_tokens")]
public class EmailToken
{
    [Column("id")]
    public ulong Id { get; set; }

    [Column("user_id")]
    public ulong UserId { get; set; }

    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Column("purpose")]
    public string Purpose { get; set; } = string.Empty; // ex: "confirm", "recovery"

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
}