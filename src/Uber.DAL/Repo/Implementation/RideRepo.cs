using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.DAL.Entities;
using Uber.DAL.Enums;
using Uber.DAL.Repo.Abstraction;

namespace Uber.DAL.Repo.Implementation
{
    public class RideRepo : IRideRepo
    {

        private readonly UberDBContext db;
        public RideRepo(UberDBContext db)
        {
            this.db = db;
        }

        public (bool, string?) Create(Ride ride, bool create = true)
        {
            try
            {
                db.Rides.Add(ride);
                if (create)
                    db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) Cancel(int id)
        {
            try
            {
                var ride = db.Rides.Where(a => a.Id == id).FirstOrDefault();
                if (ride == null)
                {
                    return (false, "Ride not found");
                }
                var result = ride.Cancel();
                db.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public List<Ride> GetAll()
        {

            return db.Rides.ToList();

        }

        public (string, Ride?) GetByID(int id)
        {
            try
            {
                var ride = db.Rides
                    .Include(r => r.User)
                    .Include(r => r.Driver)
                    .Where(a => a.Id == id)
                    .FirstOrDefault();
                    
                if (ride == null)
                {
                    return ("Ride not found", null);
                }
                return (null, ride);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }
    
    
        public (bool, string?) UpdateStatus(int id, RideStatus status)
        {
            var r = db.Rides.Find(id);
            if (r == null) return (false, "Not found");
            r.Status = status;
            db.SaveChanges();
            return (true, null);
        }

        public (bool, string?) AssignNewDriver(int rideId, string newDriverId)
        {
            try
            {
                var ride = db.Rides.Find(rideId);
                if (ride == null) return (false, "Ride not found");
                
                ride.DriverId = newDriverId;
                ride.Status = RideStatus.Pending; // Reset status to pending for new driver
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) UpdateUserRating(int rideId, User user)
        {
            try
            {
                var ride = db.Rides.Find(rideId);
                if (ride == null) return (false, "Ride not found");
                
                // Update the user's rating in the database
                var existingUser = db.Users.Find(user.Id);
                if (existingUser == null) return (false, "User not found");
                
                existingUser.TotalRatingPoints = user.TotalRatingPoints;
                existingUser.TotalRatings = user.TotalRatings;
                
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) UpdateDriverRating(int rideId, Driver driver)
        {
            try
            {
                var ride = db.Rides.Find(rideId);
                if (ride == null) return (false, "Ride not found");
                
                // Update the driver's rating in the database
                var existingDriver = db.Drivers.Find(driver.Id);
                if (existingDriver == null) return (false, "Driver not found");
                
                existingDriver.TotalRatingPoints = driver.TotalRatingPoints;
                existingDriver.TotalRatings = driver.TotalRatings;
                
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) UpdateUserBalance(int rideId, User user)
        {
            try
            {
                var ride = db.Rides.Find(rideId);
                if (ride == null) return (false, "Ride not found");
                
                // Update the user's balance in the database
                var existingUser = db.Users.Find(user.Id);
                if (existingUser == null) return (false, "User not found");
                
                existingUser.Balance = user.Balance;
                
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) UpdateDriverBalance(int rideId, Driver driver)
        {
            try
            {
                var ride = db.Rides.Find(rideId);
                if (ride == null) return (false, "Ride not found");
                
                // Update the driver's balance in the database
                var existingDriver = db.Drivers.Find(driver.Id);
                if (existingDriver == null) return (false, "Driver not found");
                
                existingDriver.Balance = driver.Balance;
                
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
