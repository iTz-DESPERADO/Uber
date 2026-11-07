using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Uber.BLL.Services.Abstraction;
using Uber.BLL.Services.Impelementation;
using Uber.DAL.Entities;

namespace Uber.Ui.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserService _userService;
        private readonly IDriverService _driverService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(IConfiguration config, IUserService userService, IDriverService driverService, UserManager<ApplicationUser> userManager)
        {
            _driverService = driverService;
            _userService = userService;
            _paymentService = new PaymentService(config["Stripe:SecretKey"]);
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User, Driver")]
        public async Task<IActionResult> Checkout(double amount)
        {
            var appUser = await _userManager.GetUserAsync(User);
            bool isUser = false;
            bool isDriver = false;
            dynamic user = 1;
            try
            {
                user = (User)appUser;
                isUser = true;
            }
            catch { }
            try
            {
                user = (Driver)appUser;
                isDriver = true;
            }
            catch { }
            if (!isUser && !isDriver)
                return NotFound("User not found");

            if (string.IsNullOrEmpty(user.StripeId))
            {
                user.StripeId = _paymentService.CreateStripeCustomer(
                    user.Email,
                    user.Name
                );
            }

            var checkoutUrl = _paymentService.CreateCheckoutSession(
                user.StripeId,
                (int)(amount * 100),
                successUrl: Url.Action("Success", "Payment", new { amount }, Request.Scheme),
                cancelUrl: Url.Action("Cancel", "Payment", null, Request.Scheme)
            );

            return Redirect(checkoutUrl);
        }

        [Authorize(Roles = "User, Driver")]
        public async Task<IActionResult> QuickCharge(double amount)
        {
            var appUser = await _userManager.GetUserAsync(User);
            bool isUser = false;
            bool isDriver = false;
            dynamic user= 1;
            try 
            { 
                user = (User)appUser;
                isUser = true;
            }
            catch { }
            try
            {
                user = (Driver)appUser;
                isDriver = true;
            }
            catch { }
            if (!isUser && !isDriver)
            return NotFound("User not found");

            if (string.IsNullOrEmpty(user.StripeId))
                return Content("User has no Stripe customer ID");

            var cards = _paymentService.GetSavedCards(user.StripeId);
            if (!cards.Any())
                return Content("No saved cards for this user");

            var paymentMethodId = cards.First().Id;
            var intent = _paymentService.ChargeSavedCard(user.StripeId, paymentMethodId, (int)(amount * 100));

            return Content($"Charged {intent.Amount / 100.0} {intent.Currency} successfully!");
        }

        [Authorize(Roles = "User, Driver")]
        public async Task<IActionResult> Success(double amount)
        {
            var appUser = await _userManager.GetUserAsync(User);

            if (appUser is User normalUser)
            {
                _userService.AddBalance(normalUser.Id, amount);
            }
            else if (appUser is Driver driver)
            {
                _driverService.AddBalance(driver.Id, amount);
            }
            else
            {
                return NotFound("User type not recognized");
            }


            if (await _userManager.IsInRoleAsync(appUser, "User"))
            {
                TempData["PaymentMessage"] = "Payment successful!";
                return RedirectToAction("UserProfile", "User");
            }
            else if (await _userManager.IsInRoleAsync(appUser, "Driver"))
            {
                TempData["PaymentMessage"] = "Payment successful!";
                return RedirectToAction("DriverProfile", "Driver");
            }

            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "User, Driver")]
        public async Task<IActionResult> Cancel()
        {
            var appUser = await _userManager.GetUserAsync(User);

            if (await _userManager.IsInRoleAsync(appUser, "User"))
            {
                TempData["PaymentMessage"] = "Payment successful!";
                return RedirectToAction("UserProfile", "User");
            }
            else if (await _userManager.IsInRoleAsync(appUser, "Driver"))
            {
                TempData["PaymentMessage"] = "Payment successful!";
                return RedirectToAction("DriverProfile", "Driver");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
