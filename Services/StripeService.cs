using Portfolio.Exceptions;
using Portfolio.Models;
using Stripe.Checkout;

namespace Portfolio.Services;

public class StripeService
{
    public StripeService()
    {
        Stripe.StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
    }

    public async Task<string> CreateCheckoutSessionAsync(
        Training training, ulong userId, string successUrl, string cancelUrl)
    {
        if (string.IsNullOrWhiteSpace(training.StripePriceId))
            throw new BusinessException("Stripe.NotConfigured");

        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = training.StripePriceId,
                    Quantity = 1,
                }
            ],
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = userId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                ["training_code"] = training.Code,
            },
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url!;
    }
}
