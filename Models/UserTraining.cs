using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

[Table("user_trainings")]
public class UserTraining
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public ulong UserId { get; set; }

    [Column("training_code")]
    public string TrainingCode { get; set; } = string.Empty;

    [Column("stripe_session_id")]
    public string? StripeSessionId { get; set; }

    [Column("purchased_at")]
    public DateTime PurchasedAt { get; set; }
}
