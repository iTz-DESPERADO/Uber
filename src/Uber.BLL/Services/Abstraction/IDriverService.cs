using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.BLL.ModelVM.Driver;
using Uber.BLL.ModelVM.User;
using Uber.DAL.Entities;

namespace Uber.BLL.Services.Abstraction
{
    public interface IDriverService
    {
        public Task<(bool, string?)> CreateAsync(CreateDriver driver);
        public (bool, string?) Delete(string id);
        public (bool, string?, List<string>?) GetNearestDriver(double lat, double lng);
        public (bool, string?) MakeUserActive(string Id);
        public (bool, string?) MakeUserInactive(string Id);
        (string?, Driver?) GetByID(string id);

        //object SendRequest(GetDriver getDriver, string id);
        public (string?, EditDriver?) GetByIDToEdit(string id);
        public List<EditDriver> GetAll();
        public (bool, string?) EditProfile(ProfileVM driver);
        public (bool, string?) Edit(EditDriver driv);

        public Task<(bool, string?, Driver?)> GetDriverProfileInfo();
        public (bool, string?) AddBalance(string id, double amount);
    }
}
