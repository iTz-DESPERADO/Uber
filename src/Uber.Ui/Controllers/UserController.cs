using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Uber.BLL.ModelVM.Account;
using Uber.BLL.ModelVM.Driver;
using Uber.BLL.ModelVM.User;
using Uber.BLL.Services.Abstraction;
using Uber.BLL.Services.Impelementation;
using Uber.DAL.DataBase;
using Uber.DAL.Entities;

namespace Uber.Ui.Controllers
{
    public class UserController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUserService service;
        private readonly IDriverService driverService;

        public UserController(IUserService service, SignInManager<ApplicationUser> signInManager, IDriverService driverService)
        {
            this.service = service;
            this.signInManager = signInManager;
            this.driverService = driverService;
        }



        [HttpPost]
        public async Task<IActionResult> RegisterUser(CreateUser user)
        {
            if (ModelState.IsValid)
            {
                var (success, error) = await service.CreateAsync(user);

                if (!success)
                {
                    ModelState.AddModelError("", error ?? "An error occurred");
                    return View("~/Views/User/Register.cshtml", user);
                }

                return RedirectToAction("Login", "Account");
            }

            return View("~/Views/User/Register.cshtml", user);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
            );


            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {

                ViewBag.LoginError = "*Invalid username or password";
                return View();
            }


        }


        [HttpGet]
        public IActionResult RequestRide()
        {
            return View();
        }


        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var result = await service.GetProfileInfo();
            return View(result.Item3);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var result = await service.GetProfileInfo();
                if (result.Item3 != null)
                {
                    return Json(new { balance = result.Item3.Profile.Balance });
                }
                return Json(new { balance = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { balance = 0, error = ex.Message });
            }
        }







        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> EditUser(UserProfileEditVM model)
        {
            try
            {
                if (model!=null)
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized("User not authenticated");
                    }

                    EditUser us = new EditUser
                    {
                        Name = model.Profile.Name,
                        DateOfBirth = model.DateOfBirth,
                        Email=model.Email,
                        PhoneNumber = model.PhoneNumber
                    };
                    us.Id = userId;

                    var result = service.Edit(us);
                    if (result.Item1)
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                        return RedirectToAction("UserProfile");
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Item2 ?? "An error occurred while updating the profile");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

        
            var profileResult = await service.GetProfileInfo();
            return View("UserProfile", profileResult.Item3);
        }






    }
}
