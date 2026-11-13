using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uber.DAL.Entities;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetRides;
using Uber.Application.Admins.GetAdmins;
using Uber.Application.Users.GetCustomerProfile;
using Uber.Application.Users.GetCustomers;
using Uber.Application.Drivers.GetDrivers;
using Uber.Application.Drivers.GetDriverProfile;

namespace Uber.Ui.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMediator _mediator;
        public AdminController(
            IMediator mediator)
        {
            _mediator = mediator;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashBoard()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin_Drivers(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDriversQuery(), cancellationToken);
            return result.IsSuccess ? View(result.Value) : BadRequest(result.Error!.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditDriver(string id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDriverProfileQuery(id), cancellationToken);
            return result.IsSuccess ? View(result.Value) : NotFound(result.Error!.Message);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin_Riders(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCustomersQuery(), cancellationToken);
            return result.IsSuccess ? View(result.Value) : BadRequest(result.Error!.Message);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditRider(string id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCustomerProfileQuery(id), cancellationToken);
            return result.IsSuccess ? View("EditUser", result.Value) : NotFound(result.Error!.Message);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin_Admins(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAdminsQuery(), cancellationToken);
            return result.IsSuccess ? View(result.Value) : BadRequest(result.Error!.Message);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin_Rides(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetRidesQuery(),
                cancellationToken);

            return result.IsSuccess
                ? View(result.Value)
                : BadRequest(result.Error!.Message);
        }
    }
}
