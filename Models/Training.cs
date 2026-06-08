using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

/// <summary>
/// Une formation à vendre = une ligne de la table "trainings".
/// Une formation existe par langue (colonne "language") : le catalogue n'affiche
/// que les formations de la langue courante.
/// Les attributs [Table]/[Column] font le pont entre les noms C# (PascalCase)
/// et les colonnes de la base (snake_case).
/// </summary>
[Table("trainings")]
public class Training
{
    [Column("id")]
    public ulong Id { get; set; }

    // Titre affiché dans le catalogue.
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    // Langue de la formation ("fr", "en", "es").
    [Column("language")]
    public string Language { get; set; } = "fr";

    // Description en HTML (affichée telle quelle dans le catalogue).
    [Column("description_html")]
    public string DescriptionHtml { get; set; } = string.Empty;

    // Identifiants Stripe (le "produit" et son "prix"). Servent à construire
    // le lien de paiement. Peuvent être vides tant que Stripe n'est pas configuré.
    [Column("stripe_product_id")]
    public string? StripeProductId { get; set; }

    [Column("stripe_price_id")]
    public string? StripePriceId { get; set; }

    // Contenu HTML de l'email de confirmation envoyé après l'achat.
    [Column("confirmation_email_html")]
    public string? ConfirmationEmailHtml { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
