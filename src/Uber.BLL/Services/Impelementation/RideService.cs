using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.BLL.Helper;
using Uber.BLL.ModelVM.Driver;
using Uber.BLL.ModelVM.Ride;
using Uber.BLL.ModelVM.User;
using Uber.BLL.Services.Abstraction;
using Uber.DAL.Entities;
using Uber.DAL.Enums;
using Uber.DAL.Repo.Abstraction;
using Uber.DAL.Repo.Impelementation;
using Uber.DAL.Repo.Implementation;

namespace Uber.BLL.Services.Impelementation
{
    public class RideService : IRideService
    {
        private readonly IRideRepo rideRepo;
        private readonly IMapper mapper;
        private readonly IUserRepo userRepo;
        private readonly IDriverRepo driverRepo;
        public RideService(IRideRepo rideRepo, IMapper mapper, IUserRepo userRepo, IDriverRepo driverRepo)
        {
            this.rideRepo = rideRepo;
            this.mapper = mapper;
            this.userRepo = userRepo;
            this.driverRepo = driverRepo;
        }
        public (bool, string?, Ride?) CreatePendingRide(
        string userId, string driverId,
        double startLat, double startLng,
        double endLat, double endLng,
        double Distance, double Duration, double Price, string paymentMethod, bool create = true)
        {
            try
            {
                var ride = new Ride
                {
                    UserId = userId,
                    DriverId = driverId,
                    StartLat = startLat,
                    StartLng = startLng,
                    EndLat = endLat,
                    EndLng = endLng,
                    Status = RideStatus.Pending,
                    Duration = Duration,
                    Distance = Distance,
                    Price = Price,      
                    PaymentMethod = Enum.Parse<PaymentMethod>(paymentMethod)
                };
                var (ok, err) = rideRepo.Create(ride, create);
                return (ok, err, ok ? ride : null);
            }
            catch (Exception ex) { return (false, ex.Message, null); }
        }

        public (bool, string?) MarkAccepted(int id) => rideRepo.UpdateStatus(id, RideStatus.Accepted);
        public (bool, string?) MarkRejected(int id) => rideRepo.UpdateStatus(id, RideStatus.Rejected);


        public (bool, string?) Create(Ride ride)
        {


            try
            {
                var result = rideRepo.Create(ride);
                return result;
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
                var result = rideRepo.Cancel(id);
                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (string?, Ride?) GetByID(int id)
        {
            return rideRepo.GetByID(id);
        }


        public List<RideVM> GetAll()
        {
            var result = rideRepo.GetAll();
            var list = new List<RideVM>();
            string tempDrivName, tempRiderName;

            foreach (var ride in result)
            {
                tempDrivName = driverRepo.GetByID(ride.DriverId).Item2.Name;
                tempRiderName = userRepo.GetByID(ride.UserId).Item2.Name;



                list.Add(new RideVM
                {

                    CreatedAt = ride.CreatedAt,
                    ID = ride.Id,
                    DriverName = tempDrivName ?? "No Driver Assigned",
                    RiderName = tempRiderName ?? "No User Assigned",
                    Status = ride.Status,
                    Price = ride.Price ?? 0,
                    Duration = ride.Duration,
                    Distance = ride.Distance

                });
            }
            return list;
        }



        public (bool, string?) AssignNewDriver(int rideId, string newDriverId)
        {
            try
            {
                var (ok, err) = rideRepo.AssignNewDriver(rideId, newDriverId);
                return (ok, err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) MarkInProgress(int id)
        {
            try
            {
                var (err, ride) = rideRepo.GetByID(id);
                if (err != null || ride == null)
                {
                    return (false, "Ride not found");
                }

                if (ride.Status != RideStatus.DriverWaiting)
                {
                    return (false, $"Cannot start ride. Ride is currently in {ride.Status} status. Expected: DriverWaiting");
                }

                var (ok, updateErr) = rideRepo.UpdateStatus(id, RideStatus.InProgress);
                return (ok, updateErr);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) MarkCompleted(int id)
        {
            try
            {
                var (err, ride) = rideRepo.GetByID(id);
                if (err != null || ride == null)
                {
                    return (false, "Ride not found");
                }

                if (ride.Status != RideStatus.InProgress)
                {
                    return (false, $"Cannot complete ride. Ride is currently in {ride.Status} status. Expected: InProgress");
                }

                var (ok, updateErr) = rideRepo.UpdateStatus(id, RideStatus.Completed);
                return (ok, updateErr);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) MarkDriverWaiting(int id)
        {
            try
            {
                var (err, ride) = rideRepo.GetByID(id);
                if (err != null || ride == null)
                {
                    return (false, "Ride not found");
                }

                if (ride.Status != RideStatus.Accepted)
                {
                    return (false, $"Cannot mark driver as arrived. Ride is currently in {ride.Status} status. Expected: Accepted");
                }

                var (ok, updateErr) = rideRepo.UpdateStatus(id, RideStatus.DriverWaiting);
                return (ok, updateErr);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) AddUserRating(int rideId, int rating)
        {
            try
            {
                Console.WriteLine($"AddUserRating called with rideId: {rideId}, rating: {rating}");
                
                var (err, ride) = GetByID(rideId);
                if (err != null || ride == null)
                {
                    Console.WriteLine($"Ride not found: {err}");
                    return (false, "Ride not found");
                }

                Console.WriteLine($"Ride found: Id={ride.Id}, UserId={ride.UserId}, DriverId={ride.DriverId}");
                Console.WriteLine($"User navigation property: {(ride.User != null ? "Loaded" : "NULL")}");
                Console.WriteLine($"Driver navigation property: {(ride.Driver != null ? "Loaded" : "NULL")}");

                if (ride.User == null)
                {
                    Console.WriteLine($"User navigation property is null for ride {rideId}");
                    return (false, "User not found - navigation property not loaded");
                }

                var (ok, ratingErr) = ride.User.AddRating(rating);
                if (!ok)
                {
                    Console.WriteLine($"Failed to add rating to user: {ratingErr}");
                    return (false, ratingErr);
                }

                Console.WriteLine($"Rating added to user successfully. New total: {ride.User.TotalRatingPoints}/{ride.User.TotalRatings}");

                var (updateOk, updateErr) = rideRepo.UpdateUserRating(rideId, ride.User);
                if (!updateOk)
                {
                    Console.WriteLine($"Failed to update user rating in database: {updateErr}");
                    return (false, updateErr);
                }
                
                Console.WriteLine($"User rating updated in database successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddUserRating: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return (false, ex.Message);
            }
        }

        public (bool, string?) AddDriverRating(int rideId, int rating)
        {
            try
            {
                Console.WriteLine($"AddDriverRating called with rideId: {rideId}, rating: {rating}");
                
                var (err, ride) = GetByID(rideId);
                if (err != null || ride == null)
                {
                    Console.WriteLine($"Ride not found: {err}");
                    return (false, "Ride not found");
                }

                Console.WriteLine($"Ride found: Id={ride.Id}, UserId={ride.UserId}, DriverId={ride.DriverId}");
                Console.WriteLine($"User navigation property: {(ride.User != null ? "Loaded" : "NULL")}");
                Console.WriteLine($"Driver navigation property: {(ride.Driver != null ? "Loaded" : "NULL")}");

                if (ride.Driver == null)
                {
                    Console.WriteLine($"Driver navigation property is null for ride {rideId}");
                    return (false, "Driver not found - navigation property not loaded");
                }

                var (ok, ratingErr) = ride.Driver.AddRating(rating);
                if (!ok)
                {
                    Console.WriteLine($"Failed to add rating to driver: {ratingErr}");
                    return (false, ratingErr);
                }

                Console.WriteLine($"Rating added to driver successfully. New total: {ride.Driver.TotalRatingPoints}/{ride.Driver.TotalRatings}");

                // Update the driver in the database
                var (updateOk, updateErr) = rideRepo.UpdateDriverRating(rideId, ride.Driver);
                if (!updateOk)
                {
                    Console.WriteLine($"Failed to update driver rating in database: {updateErr}");
                    return (false, updateErr);
                }
                
                Console.WriteLine($"Driver rating updated in database successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddDriverRating: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return (false, ex.Message);
            }
        }

        //public (bool, string?) ProcessPayment(int rideId)
        //{
        //    try
        //    {
        //        Console.WriteLine($"ProcessPayment called with rideId: {rideId}");
                
        //        var (err, ride) = GetByID(rideId);
        //        if (err != null || ride == null)
        //        {
        //            Console.WriteLine($"Ride not found: {err}");
        //            return (false, "Ride not found");
        //        }

        //        if (ride.Price == null || ride.Price <= 0)
        //        {
        //            Console.WriteLine($"Invalid ride price: {ride.Price}");
        //            return (false, "Invalid ride price");
        //        }

        //        var ridePrice = ride.Price.Value;
        //        Console.WriteLine($"Processing payment for ride {rideId} with price {ridePrice} using {ride.PaymentMethod}");

        //        if (ride.PaymentMethod == PaymentMethod.Cash)
        //        {
        //            // Cash payment: deduct 0.2 × (ride price) from driver's balance
        //            var deductionAmount = 0.2 * ridePrice;
        //            var (deductOk, deductErr) = ride.Driver.AddBalance(-deductionAmount);
        //            if (!deductOk)
        //            {
        //                Console.WriteLine($"Failed to deduct from driver balance: {deductErr}");
        //                return (false, deductErr);
        //            }
        //            Console.WriteLine($"Deducted {deductionAmount} from driver balance. New balance: {ride.Driver.Balance}");
        //        }
        //        else if (ride.PaymentMethod == PaymentMethod.Wallet)
        //        {
        //            // Wallet payment: subtract ride price from user's wallet, add 0.8 × ride price to driver
        //            var (deductOk, deductErr) = ride.User.AddBalance(-ridePrice);
        //            if (!deductOk)
        //            {
        //                Console.WriteLine($"Failed to deduct from user wallet: {deductErr}");
        //                return (false, deductErr);
        //            }

        //            var driverEarning = 0.8 * ridePrice;
        //            var (addOk, addErr) = ride.Driver.AddBalance(driverEarning);
        //            if (!addOk)
        //            {
        //                Console.WriteLine($"Failed to add to driver balance: {addErr}");
        //                return (false, addErr);
        //            }

        //            Console.WriteLine($"Deducted {ridePrice} from user wallet. New user balance: {ride.User.Balance}");
        //            Console.WriteLine($"Added {driverEarning} to driver balance. New driver balance: {ride.Driver.Balance}");
        //        }

        //        // Update both user and driver in the database
        //        var (updateUserOk, updateUserErr) = rideRepo.UpdateUserBalance(rideId, ride.User);
        //        if (!updateUserOk)
        //        {
        //            Console.WriteLine($"Failed to update user balance in database: {updateUserErr}");
        //            return (false, updateUserErr);
        //        }

        //        var (updateDriverOk, updateDriverErr) = rideRepo.UpdateDriverBalance(rideId, ride.Driver);
        //        if (!updateDriverOk)
        //        {
        //            Console.WriteLine($"Failed to update driver balance in database: {updateDriverErr}");
        //            return (false, updateDriverErr);
        //        }

        //        Console.WriteLine($"Payment processed successfully for ride {rideId}");
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception in ProcessPayment: {ex.Message}");
        //        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        //        return (false, ex.Message);
        //    }
        //}
    }
}
