using Microsoft.AspNetCore.Mvc;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Drivers.RegisterDriver;
using Uber.Application.Users.RegisterCustomer;
using Uber.Ui.Endpoints.Common;
using Uber.Ui.Models;
using Uber.Ui.Services;

namespace Uber.Ui.Endpoints.Authentication;

public static class RegistrationEndpoints
{
    public static IEndpointRouteBuilder MapRegistrationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth/register").WithTags("Authentication");

        group.MapPost("/user", RegisterUserAsync);
        group.MapPost("/driver", RegisterDriverAsync);

        return endpoints;
    }

    private static async Task<IResult> RegisterUserAsync(
        [FromForm] CreateUserModel request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterCustomerCommand(
            request.Name,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Gender), cancellationToken);

        return result.IsSuccess
            ? Results.Redirect("/Account/Login")
            : result.ToProblem();
    }

    private static async Task<IResult> RegisterDriverAsync(
        [FromForm] CreateDriverModel request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (request.Vehicle is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Vehicle)] = ["Vehicle information is required."]
            });
        }

        var imagePath = request.File is null
            ? null
            : await FileStorage.SaveAsync("Files", request.File, cancellationToken);

        var result = await mediator.Send(new RegisterDriverCommand(
            request.Name,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Gender,
            request.Vehicle.Brand,
            request.Vehicle.Model,
            request.Vehicle.YearMade,
            request.Vehicle.Plate,
            request.Vehicle.Color,
            request.Vehicle.SeatingCapacity,
            request.Vehicle.Type,
            request.Vehicle.Transmission,
            request.Vehicle.VehicleEngine,
            imagePath), cancellationToken);

        if (result.IsFailure && imagePath is not null)
        {
            FileStorage.Delete("Files", imagePath);
        }

        return result.IsSuccess
            ? Results.Redirect("/Account/Login")
            : result.ToProblem();
    }
}
