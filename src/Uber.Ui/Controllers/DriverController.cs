using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Drivers.GetDriverProfile;
using Uber.Application.Rides.GetActiveRide;
using Uber.Domain.Enums;

namespace Uber.Ui.Controllers
{
    public class DriverController : Controller
    {
        private readonly IMediator mediator;
        public DriverController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var driverId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(driverId))
            {
                return Unauthorized();
            }

            var activeRide = await mediator.Send(
                new GetActiveRideQuery(driverId), cancellationToken);

            if (activeRide.IsSuccess
                && activeRide.Value is { Status: RideStatus.Accepted or RideStatus.DriverWaiting or RideStatus.InProgress } ride)
            {
                return RedirectToAction("RideDetails", "Ride", new { id = ride.Id });
            }

            return View();
        }

        [Authorize(Roles = "Driver")]
        [HttpGet]
        public async Task<IActionResult> DriverProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await mediator.Send(new GetDriverProfileQuery(userId));
            return result.IsSuccess ? View(result.Value) : NotFound(result.Error!.Message);
        }

    }
}
