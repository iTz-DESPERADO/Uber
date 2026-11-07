using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.BLL.Helper;
using Uber.BLL.ModelVM.Driver;
using Uber.BLL.ModelVM.User;
using Uber.BLL.Services.Abstraction;
using Uber.DAL.Entities;
using Uber.DAL.Repo.Abstraction;
using Uber.DAL.Repo.Impelementation;

namespace Uber.BLL.Services.Impelementation
{
    public class DriverService : IDriverService
    {
        private readonly IMapper mapper;
        private readonly IDriverRepo driverRepo;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRideRepo rideRepo;
        public DriverService(IDriverRepo _driverRepo, IMapper _mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IRideRepo rideRepo)
        {
            this.driverRepo = _driverRepo;
            mapper = _mapper;
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
            this.rideRepo = rideRepo;
        }

        public async Task<(bool, string?)> CreateAsync(CreateDriver driver)
        {
            try
            {
                string errors = "";

                var driv = mapper.Map<Driver>(driver);

                driv.UserName = driver.Email;

                driv.AddProfilePhoto(Upload.UploadFile("Files", driver.File));


                var result = driverRepo.CreateVehicle(driv.Vehicle);

                if (!result.Item1)
                    return result;


                var res = await userManager.CreateAsync(driv, driver.Password);

                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(driv, "Driver");
                    return (true, null);
                }
                else
                {
                    foreach (var error in res.Errors)
                    {
                        errors += (($"{error.Code} - {error.Description}\n"));
                    }

                    return (false, errors);
                }




            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) Delete(string id)
        {
            try
            {
                var result = driverRepo.Delete(id);
                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string?, List<string>?) GetNearestDriver(double lat, double lng)
        {
            try
            {
                var list = driverRepo.GetAll().Where(a => a.IsActive && (!a.IsDeleted)).OrderBy(d => ((d.CurrentLng - lng) * (d.CurrentLng - lng) + (d.CurrentLat - lat) * (d.CurrentLat - lat))).ToList();
                List<string> list2 = list.Select(a => a.Id).ToList();
                return (true, null, list2);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        //public object SendRequest(GetDriver getDriver, string id)
        //{
        //    throw new NotImplementedException();
        //}

        public (bool, string?) MakeUserActive(string Id)
        {
            return driverRepo.MakeUserActive(Id);
        }

        public (bool, string?) MakeUserInactive(string Id)
        {
            return driverRepo.MakeUserInactive(Id);
        }

        public (string?, Driver?) GetByID(string id)
        {
            return driverRepo.GetByID(id);
        }



        public (bool, string?) EditProfile(ProfileVM driver)
        {
            try
            {
                var result = driverRepo.GetByID(driver.Id);
                var existingDriver = result.Item2;
                if (existingDriver == null)
                {
                    return (false, "Driver not found");
                }
                mapper.Map(driver, existingDriver);

                driverRepo.Edit(existingDriver);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) Edit(EditDriver driver)
        {
            try
            {
                var result = driverRepo.GetByID(driver.Id);
                var existingDriver = result.Item2;
                if (existingDriver == null)
                {
                    return (false, "Driver not found");
                }
                mapper.Map(driver, existingDriver);

                driverRepo.Edit(existingDriver);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public List<EditDriver> GetAll()
        {

            var result = driverRepo.GetAll();
            var list = new List<EditDriver>();
            foreach (var driv in result)
            {
                list.Add(mapper.Map<EditDriver>(driv));
            }
            return list;
        }


        public (string?, EditDriver?) GetByIDToEdit(string id)
        {
            try
            {
                var result = driverRepo.GetByID(id);
                if (result.Item1 != null) return (result.Item1, null);
                var driv = mapper.Map<EditDriver>(result.Item2);
                return (null, driv);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }



        public async Task<(bool, string?, Driver?)> GetDriverProfileInfo()
        {
            try
            {
                var driver = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
                if (driver == null)
                {
                    return (false, "Driver not found or not logged in.", null);
                }
                var div = driverRepo.GetByID(driver.Id);

                if (div.Item1 != null || div.Item2 == null)
                {
                    return (false, "Driver not found.", null);
                }


                div.Item2.Vehicle = driverRepo.GetVehicleById(div.Item2.VehicleId);

                div.Item2.Rides = rideRepo.GetAll().Where(a => a.DriverId == div.Item2.Id).ToList();


                return (true, null, div.Item2);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public (bool, string?) AddBalance(string id, double amount)
        {
            try
            {
                var result = driverRepo.GetByID(id);
                var user = result.Item2;
                if (user == null)
                {
                    return (false, "User Not Found");
                }
                return driverRepo.AddBalance(user, amount);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        //public Task<(bool, string?, DriverProfileVM?)> GetProfileInfo()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
