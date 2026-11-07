using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;
using Uber.DAL.Enums;

namespace Uber.DAL.Repo.Abstraction
{
    public interface IRideRepo
    {
        public (bool, string?) Create(Ride ride, bool create = true);
        public (bool, string?) Cancel(int id);
        public (string, Ride?) GetByID(int id);
        public List<Ride> GetAll();

        public (bool, string?) UpdateStatus(int id, RideStatus status);
        public (bool, string?) AssignNewDriver(int rideId, string newDriverId);
        public (bool, string?) UpdateUserRating(int rideId, User user);
        public (bool, string?) UpdateDriverRating(int rideId, Driver driver);
        public (bool, string?) UpdateUserBalance(int rideId, User user);
        public (bool, string?) UpdateDriverBalance(int rideId, Driver driver);
    }
}
