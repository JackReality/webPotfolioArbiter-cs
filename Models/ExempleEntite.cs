using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

/// <summary>
/// EXEMPLE de modèle = une table de la base.
/// Remplace/duplique cette classe par tes vraies tables (Utilisateur, Formation, etc.).
/// Une classe = une table ; une propriété = une colonne.
/// </summary>
public class ExempleEntite
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;
}
