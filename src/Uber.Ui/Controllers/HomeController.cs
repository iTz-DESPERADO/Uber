using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Uber.Ui.Models;
using System.Security.Claims;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetActiveRide;
using Uber.Domain.Enums;

namespace Uber.Ui.Controllers
{
    public class HomeController(IMediator mediator) : Controller
    {
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("User"))
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    var activeRide = await mediator.Send(
                        new GetActiveRideQuery(customerId), cancellationToken);

                    if (activeRide.IsSuccess && activeRide.Value is { } ride)
                    {
                        return ride.Status == RideStatus.Pending
                            ? RedirectToAction("WaitingForDriver", "Ride", new { id = ride.Id })
                            : RedirectToAction("RideDetails", "Ride", new { id = ride.Id });
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult DataDeletion()
        {
            return View();
        }
    }
}
