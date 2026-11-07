using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;

namespace Uber.DAL.Repo.Abstraction
{
    public interface IDriverRepo
    {
        (bool, string?) CreateVehicle(Vehicle vehicle);
        (string?, Driver?) GetByID(string id);
        List<Driver> GetAll();
        (bool, string?) Delete(string id);
        (bool, string?) Edit(Driver driver);
        (bool, string?) MakeUserActive(string id);
        (bool, string?) MakeUserInactive(string id);
        (bool, string?) AddBalance(Driver driver, double amount);
        public Vehicle? GetVehicleById(int id);
    }
}
