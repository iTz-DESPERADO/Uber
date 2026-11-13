namespace Uber.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    Task<string> CreateCustomerAsync(
        string email,
        string name,
        CancellationToken cancellationToken = default);

    Task<string> CreateCheckoutSessionAsync(
        string customerId,
        long amount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<CheckoutPayment> GetCheckoutPaymentAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SavedPaymentMethod>> GetSavedCardsAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<PaymentCharge> ChargeSavedCardAsync(
        string customerId,
        string paymentMethodId,
        long amount,
        CancellationToken cancellationToken = default);
}

public sealed record SavedPaymentMethod(string Id);

public sealed record PaymentCharge(long Amount, string Currency);

public sealed record CheckoutPayment(
    string SessionId,
    string CustomerId,
    long Amount,
    string Currency,
    bool IsPaid,
    bool IsLive);
