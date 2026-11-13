using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetRideById;

namespace Uber.Ui.Controllers;

public sealed class RideController(IMediator mediator) : Controller
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> RideDetails(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetRideByIdQuery(id),
            cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error!.Message);
        }

        var ride = result.Value;
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        if (ride.UserId != currentUserId
            && ride.DriverId != currentUserId
            && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(ride);
    }

    [Authorize]
    [HttpGet]
    public IActionResult WaitingForDriver(int? id = null) =>
        View(model: id?.ToString());
}
