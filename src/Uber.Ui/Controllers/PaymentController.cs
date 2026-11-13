using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Uber.Ui.Controllers;

[Authorize(Roles = "User, Driver")]
public sealed class PaymentController(IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var secretKey = configuration["Stripe:SecretKey"];
        ViewBag.StripeConfigured = !string.IsNullOrWhiteSpace(secretKey)
            && secretKey.StartsWith("sk_test_", StringComparison.Ordinal);

        return View();
    }
}
