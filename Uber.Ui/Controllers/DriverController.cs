using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using Uber.BLL.ModelVM.Account;
using Uber.BLL.ModelVM.Driver;
using Uber.BLL.Services.Abstraction;
using Uber.BLL.Services.Impelementation;
using Uber.DAL.DataBase; 
using Uber.DAL.Entities;

namespace Uber.Ui.Controllers
{
    public class DriverController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IDriverService service;

        public DriverController(IDriverService service, SignInManager<ApplicationUser> signInManager)
        {
            this.service = service;
            this.signInManager = signInManager;
        }


        [HttpPost]
        public async Task<IActionResult> RegisterDriver(CreateDriver driver)
        {
            if (ModelState.IsValid)
            {
                var (success, error) = await service.CreateAsync(driver);

                if (!success)
                {
                    ModelState.AddModelError("", error ?? "An error occurred");
                    return View("~/Views/Driver/Register.cshtml", driver);
                }
                return RedirectToAction("Login", "Account");
            }

            return View("~/Views/Driver/Register.cshtml", driver);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Login(LoginVM model)
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
        [Authorize(Roles = "Driver")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCurrentDriverId()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }
                
                return Json(new { driverId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult MakeUserInactive()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = service.MakeUserInactive(userId);
                if (result.Item1)
                {
                    return Json(new { success = true, message = "Driver status set to inactive" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Item2 });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult MakeUserActive()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = service.MakeUserActive(userId);
                if (result.Item1)
                {
                    return Json(new { success = true, message = "Driver status set to active" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Item2 });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetCurrentDriverStatus()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var (err, driver) = service.GetByID(userId);
                if (err != null || driver == null)
                {
                    return NotFound("Driver not found");
                }

                return Json(new { isActive = driver.IsActive });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ToggleDriverStatus([FromBody] ToggleDriverStatusRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = request.IsActive ? service.MakeUserActive(userId) : service.MakeUserInactive(userId);
                if (result.Item1)
                {
                    var status = request.IsActive ? "active" : "inactive";
                    return Json(new { success = true, message = $"Driver status set to {status}" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Item2 });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult AcceptRide(string rideId)
        {
      

            return Json(new { success = true });
        }





        [Authorize(Roles = "Driver")]
        [HttpGet]
        public async Task<IActionResult> DriverProfile()
        {
            var result = await service.GetDriverProfileInfo();
            return View(result.Item3);
        }

        [Authorize(Roles = "Driver")]
        [HttpPost]
        public async Task<IActionResult> EditDriver(ProfileVM model)
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

                    model.Id = userId;

                    var result = service.EditProfile(model);
                    if (result.Item1)
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                        return RedirectToAction("DriverProfile");
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

            var profileResult = await service.GetDriverProfileInfo();
            return View("DriverProfile", profileResult.Item3);
        }
    }
}
