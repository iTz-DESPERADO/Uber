using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.DataBase;
using Uber.DAL.Entities;
using Uber.DAL.Repo.Abstraction;

namespace Uber.DAL.Repo.Implementation
{
    public class DriverRepo : IDriverRepo
    {
        private readonly UberDBContext db;

        public DriverRepo(UberDBContext db)
        {
            this.db = db;
        }

        public (bool, string?) CreateVehicle(Vehicle vehicle)
        {
            try
            {
                //db.Wallets.Add(driver.Wallet);

                db.Vehicles.Add(vehicle);
                db.SaveChanges();

                //db.Drivers.Add(driver);
                //db.SaveChanges();
                return (true, null);
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
                var driver = db.Drivers.Where(a => a.Id == id).FirstOrDefault();
                if (driver == null)
                {
                    return (false, "Driver not found");
                }
                driver.Delete();
                db.SaveChanges();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

   

        public (bool, string?) Edit(Driver driver)
        {
            try
            {
                var d = db.Drivers.Where(a => a.Id == driver.Id).FirstOrDefault();
                if (d == null)
                {
                    return (false, "Driver not found");
                }

                d.Edit(driver.Name,driver.DateOfBirth,driver.ImagePath,driver.Email,driver.PhoneNumber,driver.IsDeleted);               
               
                db.SaveChanges ();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }




        public Vehicle? GetVehicleById(int id)
        {
            try
            {
                var vehicle = db.Vehicles.Where(a => a.Id == id).FirstOrDefault();
               
                return vehicle;
            }
            catch (Exception ex)
            {
                return null;
            }
        }





        public List<Driver> GetAll()
        {
            return db.Drivers.ToList();
        }

        public (string?, Driver?) GetByID(string id)
        {
            try
            {
                var driver = db.Drivers.Where(a => a.Id == id).FirstOrDefault();
                if (driver == null)
                {
                    return ("Driver not found", null);
                }
                return (null, driver);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }
        public (bool, string?) MakeUserActive(string id)
        {
            try
            {
                var driver = db.Drivers.Where(b => b.Id == id).FirstOrDefault();
                if (driver == null)
                    return (false, "No Driver with this Id");
                var result = driver.MakeActive();

                db.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) MakeUserInactive(string id)
        {
            try
            {
                var driver = db.Drivers.Where(b => b.Id == id).FirstOrDefault();
                if (driver == null)
                    return (false, "No Driver with this Id");
                var result = driver.MakeInactive();
                db.SaveChanges();
                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string?) AddBalance(Driver driver, double amount)
        {
            try
            {
                var result = driver.AddBalance(amount);
                db.SaveChanges();
                return result;
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
