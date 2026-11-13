using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Uber.Application.Abstractions.Payments;

namespace Uber.Infrastructure.Payments;

internal sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly RequestOptions _requestOptions;

    public StripePaymentGateway(IConfiguration configuration)
    {
        var apiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe secret key is not configured.");

        if (configuration.GetValue("Stripe:UseSandbox", true)
            && !apiKey.StartsWith("sk_test_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Stripe sandbox requires a test secret key beginning with 'sk_test_'.");
        }

        _requestOptions = new RequestOptions { ApiKey = apiKey };
    }

    public async Task<string> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default)
    {
        var customer = await new CustomerService().CreateAsync(
            new CustomerCreateOptions { Email = email, Name = name },
            _requestOptions,
            cancellationToken);

        return customer.Id;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        string customerId,
        long amount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            Mode = "payment",
            Customer = customerId,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        UnitAmount = amount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Wallet balance"
                        }
                    },
                    Quantity = 1
                }
            ],
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session"
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var session = await new SessionService().CreateAsync(
            options,
            _requestOptions,
            cancellationToken);

        return session.Url;
    }

    public async Task<CheckoutPayment> GetCheckoutPaymentAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await new SessionService().GetAsync(
            sessionId,
            requestOptions: _requestOptions,
            cancellationToken: cancellationToken);

        return new CheckoutPayment(
            session.Id,
            session.CustomerId ?? string.Empty,
            session.AmountTotal ?? 0,
            session.Currency ?? string.Empty,
            string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase),
            session.Livemode);
    }

    public async Task<IReadOnlyList<SavedPaymentMethod>> GetSavedCardsAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var methods = await new PaymentMethodService().ListAsync(
            new PaymentMethodListOptions { Customer = customerId, Type = "card" },
            _requestOptions,
            cancellationToken);

        return methods.Data.Select(method => new SavedPaymentMethod(method.Id)).ToList();
    }

    public async Task<PaymentCharge> ChargeSavedCardAsync(
        string customerId,
        string paymentMethodId,
        long amount,
        CancellationToken cancellationToken = default)
    {
        var intent = await new PaymentIntentService().CreateAsync(
            new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "egp",
                Customer = customerId,
                PaymentMethod = paymentMethodId,
                OffSession = true,
                Confirm = true
            },
            _requestOptions,
            cancellationToken);

        return new PaymentCharge(intent.Amount, intent.Currency);
    }
}
