using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Stripe.Tax;
using System.Linq;
using Uber.BLL.Helper;
using Uber.BLL.ModelVM.User;
using Uber.BLL.Services.Abstraction;
using Uber.DAL.Entities;
using Uber.DAL.Repo.Abstraction;
using Uber.DAL.Repo.Implementation;
namespace Uber.BLL.Services.Impelementation
{
    public class UserService : IUserService
    {
        private readonly IMapper mapper;
        private readonly IUserRepo userRepo;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRideRepo rideRepo;
        private readonly IDriverRepo driverRepo;

        public UserService(IUserRepo _userRepo, IMapper _mapper, UserManager<ApplicationUser> _userManager, IHttpContextAccessor _httpContextAccessor, IRideRepo rideRepo,IDriverRepo driverRepo)
        {
            userRepo = _userRepo;
            mapper = _mapper;
            userManager = _userManager;
            httpContextAccessor = _httpContextAccessor;
            this.rideRepo = rideRepo;
            this.driverRepo = driverRepo;
        }

        public async Task<(bool, string?)> CreateAsync(CreateUser user)
        {
            try
            {

                string errors = "";

                var user1 = mapper.Map<User>(user);

                user1.UserName = user.Email;

                //var result = userRepo.Create(user1);

                var res = await userManager.CreateAsync(user1, user.Password);


                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user1, "User");
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
                var result = userRepo.Delete(id);
                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string?) Edit(EditUser user) 
        {
            try
            {
                var result = userRepo.GetByID(user.Id);
                var existingUser = result.Item2;
                if (existingUser == null)
                    return (false, "User not found");

                mapper.Map(user, existingUser);

                userRepo.Edit(existingUser);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }


        public (string?, User?) GetByID(string id)
        {
            return userRepo.GetByID(id);
        }
        public List<EditUser> GetAll()
        {
            var result = userRepo.GetAll();
            var list = new List<EditUser>();
            foreach (var user in result)
            {
                list.Add(mapper.Map<EditUser>(user));
            }
            return list;
        }

        public async Task<(bool, string?, UserProfileEditVM?)> GetProfileInfo()
        {
            try
            {
                var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
                if (user == null)
                {
                    return (false, "User not found or not logged in.", null);
                }
                var us = userRepo.GetByID(user.Id);

                var userProfileEdit = new UserProfileEditVM(us.Item2.Name,us.Item2.Id,us.Item2.Balance,us.Item2.DateOfBirth,us.Item2.Email,us.Item2.PhoneNumber,us.Item2.IsDeleted);




                userProfileEdit.Profile.Rides = rideRepo.GetAll().Where(a => a.UserId != null && a.UserId == userProfileEdit.Profile.Id).ToList();

                foreach(var ride in userProfileEdit.Profile.Rides)
                {
                    ride.Driver = driverRepo.GetByID(ride.DriverId).Item2;

                }

        


              
                return (true, null, userProfileEdit);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        

        public (string?, EditUser?) GetByIDToEdit(string id)
        {
            try
            {
                var result = userRepo.GetByID(id);
                if (result.Item1 != null) return (result.Item1, null);
                var user = mapper.Map<EditUser>(result.Item2);
                return (null, user);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }

        public (bool, string?) AddBalance(string id, double amount)
        {
            try
            {
                var result = userRepo.GetByID(id);
                var user = result.Item2;
                if (user == null)
                {
                    return (false, "User Not Found");
                }
                return userRepo.AddBalance(user, amount);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
