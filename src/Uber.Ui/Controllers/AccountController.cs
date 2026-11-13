using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Uber.Ui.Models;
using Uber.DAL.Entities;
using Uber.Application.Abstractions.Email;



namespace Uber.Ui.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult ChooseLoginType() => View();
        public IActionResult ChooseRegisterType() => View();

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginModel model = new LoginModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUrl = returnUrl });
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(
            string? returnUrl = null,
            string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginModel loginViewModel = new LoginModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

         
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
           
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email is not null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        Name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0],
                        DateOfBirth = DateTime.UtcNow.AddYears(-18),
                        Gender = 0
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        AddIdentityErrors(createResult);
                        return View("Login", loginViewModel);
                    }

                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        AddIdentityErrors(roleResult);
                        return View("Login", loginViewModel);
                    }
                }

                var loginResult = await _userManager.AddLoginAsync(user, info);
                if (!loginResult.Succeeded)
                {
                    AddIdentityErrors(loginResult);
                    return View("Login", loginViewModel);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
            ViewBag.ErrorMessage = "Please contact support.";
            return View("Error");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
            );


            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is null)
                {
                    ViewBag.LoginError = "*Invalid username or password";
                    return View(model);
                }

                if (user.IsDeleted)
                {
                    await _signInManager.SignOutAsync();
                    ViewBag.LoginError = "Account was deleted";
                    return View(model);
                }

                if (await _userManager.IsInRoleAsync(user, "User"))
                    return RedirectToAction("Index", "Home");
                if (await _userManager.IsInRoleAsync(user, "Driver"))
                    return RedirectToAction("Dashboard", "Driver");
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("AdminDashBoard", "Admin");

                return RedirectToAction("Index", "Home");
            }
            else
            {

                ViewBag.LoginError = "*Invalid username or password";
                return View(model);
            }
        }
            [HttpGet]
        public IActionResult Register(string role)
        {
            if (role == "Driver")
                return View("~/Views/Driver/Register.cshtml");

            if (role == "User")
                return View("~/Views/User/Register.cshtml");

            return NotFound();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordResetLink = Url.Action(
                        "ResetPassword",
                        "Account",
                        new { Email = model.Email, Token = token },
                        Request.Scheme
                    );

                   
                    var emailBody = $"<p>Hello,</p>" +
                                    $"<p>You requested a password reset. Click the link below to reset your password:</p>" +
                                    $"<p><a href='{passwordResetLink}'>Reset Your Password</a></p>" +
                                    $"<p>If you did not request this, please ignore this email.</p>";

                    await _emailService.SendAsync(
                        model.Email,
                        "Password Reset",
                        emailBody,
                        HttpContext.RequestAborted);

                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }

            return View(new ResetPasswordModel
            {
                Token = token ?? string.Empty,
                Email = email ?? string.Empty
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

                return View("ResetPasswordConfirmation");
            }

            return View(model);
        }
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() { return View(); }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
