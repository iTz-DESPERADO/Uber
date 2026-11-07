using Microsoft.AspNetCore.Routing.Constraints;
using Uber.DAL.Enums;

namespace Uber.BLL.ModelVM.Ride
{
    public class RideVM
    {
        public DateTime CreatedAt { get; set; }

        public int ID { get; set; }

        public string DriverName { get; set; }

        public string RiderName { get; set; }
        public RideStatus Status { get; set; }
        public double Price { get; set; }
        public double? Duration { get; set; }
        public double? Distance { get; set; }

    }
}
