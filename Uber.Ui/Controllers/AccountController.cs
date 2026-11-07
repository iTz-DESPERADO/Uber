using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Uber.BLL.ModelVM.Account;
using Uber.DAL.Entities;



namespace Uber.Ui.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult ChooseLoginType() => View();
        public IActionResult ChooseRegisterType() => View();

        //[HttpGet]
        //public IActionResult Login(string role)
        //{
        //    if (role == "Driver")
        //        return View("~/Views/Driver/Login.cshtml");

        //    if (role == "User")
        //        return View("~/Views/User/Login.cshtml");

        //    return NotFound();
        //}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginVM model = new LoginVM
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
        public async Task<IActionResult>
            ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginVM loginViewModel = new LoginVM
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
           
            else
            {
              
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                 
                    var user = await _userManager.FindByEmailAsync(email);
                    bool created = false;
                    if (user == null)
                    {
                        user = new User()
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split("@")[0],
                            DateOfBirth = DateTime.Now.AddYears(-18),
                            Gender = 0
                        };
                        created = true;
                        await _userManager.CreateAsync(user);
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                  
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on Pragim@PragimTech.com";

                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
            );


            if (result.Succeeded)
            {
                if(_userManager.Users.FirstOrDefault().IsDeleted == true)
                {
                    ViewBag.LoginError = "Account was deleted";
                    return View(model);
                }
                else if (User.IsInRole("User"))
                    return RedirectToAction("Index", "Home");
                else if (User.IsInRole("Driver"))
                    return RedirectToAction("Dashboard", "Driver");
                else if (User.IsInRole("Admin"))
                    return RedirectToAction("AdminDashBoard", "Admin");
                else
                    return View();
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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {                    
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var item in result.Errors)
                        ModelState.AddModelError("Password", item.Description);
                }
            }
            return View(model);
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
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
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

                    MailSender.Mail("Password Reset", emailBody, model.Email);

                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
          
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
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
    }
}
