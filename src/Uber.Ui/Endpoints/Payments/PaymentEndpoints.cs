using Microsoft.AspNetCore.Identity;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Abstractions.Payments;
using Uber.Application.Drivers.AddBalance;
using Uber.Application.Users.AddBalance;
using Uber.DAL.Entities;
using Uber.Domain.Common;
using Uber.Domain.Identity;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Common;

namespace Uber.Ui.Endpoints.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/payments").WithTags("Payments");

        group.MapPost("/checkout", CheckoutAsync)
            .RequirePermission(PermissionNames.PaymentsUse);

        group.MapPost("/quick-charge", QuickChargeAsync)
            .RequirePermission(PermissionNames.PaymentsUse);

        group.MapGet("/success", SuccessAsync)
            .RequirePermission(PermissionNames.PaymentsUse);

        group.MapGet("/cancel", Cancel)
            .RequirePermission(PermissionNames.PaymentsUse);

        return endpoints;
    }

    private static async Task<IResult> CheckoutAsync(
        PaymentAmountRequest request,
        HttpRequest httpRequest,
        UserManager<ApplicationUser> userManager,
        IPaymentGateway paymentGateway,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Results.BadRequest(new { message = "Amount must be greater than zero" });
        }

        var user = await userManager.GetUserAsync(httpRequest.HttpContext.User);
        if (user is null) return Results.Unauthorized();

        var stripeId = GetStripeId(user);

        if (string.IsNullOrWhiteSpace(stripeId))
        {
            stripeId = await paymentGateway.CreateCustomerAsync(
                user.Email ?? string.Empty,
                user.Name,
                cancellationToken);

            SetStripeId(user, stripeId);
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Results.Problem(
                    detail: string.Join("; ", updateResult.Errors.Select(error => error.Description)),
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        var root = $"{httpRequest.Scheme}://{httpRequest.Host}";
        var checkoutUrl = await paymentGateway.CreateCheckoutSessionAsync(
            stripeId,
            checked((long)(request.Amount * 100)),
            $"{root}/api/payments/success?sessionId={{CHECKOUT_SESSION_ID}}",
            $"{root}/api/payments/cancel",
            cancellationToken);

        return Results.Ok(new { checkoutUrl });
    }

    private static async Task<IResult> QuickChargeAsync(
        PaymentAmountRequest request,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IPaymentGateway paymentGateway,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Results.BadRequest(new { message = "Amount must be greater than zero" });
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user is null) return Results.Unauthorized();

        var stripeId = GetStripeId(user);
        if (string.IsNullOrWhiteSpace(stripeId))
            return Results.BadRequest(new { message = "User has no Stripe customer ID" });

        var cards = await paymentGateway.GetSavedCardsAsync(stripeId, cancellationToken);
        if (cards.Count == 0)
            return Results.BadRequest(new { message = "No saved cards for this user" });

        var charge = await paymentGateway.ChargeSavedCardAsync(
            stripeId, cards[0].Id, checked((long)(request.Amount * 100)), cancellationToken);

        return Results.Ok(charge);
    }

    private static async Task<IResult> SuccessAsync(
        string sessionId,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IPaymentGateway paymentGateway,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null) return Results.Unauthorized();

        var stripeId = GetStripeId(user);
        var payment = await paymentGateway.GetCheckoutPaymentAsync(sessionId, cancellationToken);

        if (!payment.IsPaid
            || payment.Amount <= 0
            || !string.Equals(payment.Currency, "egp", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(payment.CustomerId, stripeId, StringComparison.Ordinal))
        {
            return Results.BadRequest(new
            {
                message = "Stripe could not verify this wallet payment."
            });
        }

        var amount = payment.Amount / 100d;

        Result result = user switch
        {
            User customer => await mediator.Send(
                new AddCustomerBalanceCommand(customer.Id, amount), cancellationToken),
            Driver driver => await mediator.Send(
                new AddDriverBalanceCommand(driver.Id, amount), cancellationToken),
            _ => Result.Failure(new Error("Payment.UnsupportedUser", "User type is not supported"))
        };

        if (result.IsFailure) return result.ToProblem();

        return Results.Redirect(user is Driver
            ? "/Driver/DriverProfile"
            : "/User/UserProfile");
    }

    private static IResult Cancel(HttpContext context)
    {
        var target = context.User.IsInRole("Driver")
            ? "/Driver/DriverProfile"
            : "/User/UserProfile";

        return Results.Redirect(target);
    }

    private static string? GetStripeId(ApplicationUser user) => user switch
    {
        User customer => customer.StripeId,
        Driver driver => driver.StripeId,
        _ => null
    };

    private static void SetStripeId(ApplicationUser user, string stripeId)
    {
        if (user is User customer) customer.StripeId = stripeId;
        if (user is Driver driver) driver.StripeId = stripeId;
    }

    public sealed record PaymentAmountRequest(double Amount);
}
