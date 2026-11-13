using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Users.GetCustomerProfile;

namespace Uber.Ui.Controllers
{
    public class UserController : Controller
    {
        private readonly IMediator mediator;
        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> UserProfile(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await mediator.Send(new GetCustomerProfileQuery(userId), cancellationToken);
            return result.IsSuccess ? View(result.Value) : NotFound(result.Error!.Message);
        }



    }
}
