using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.ModelVM.Ride
{
    public class RideRequestModel
    {
        public double StartLat { get; set; }
        public double StartLng { get; set; }
        public double EndLat { get; set; }
        public double EndLng { get; set; }
        public double Distance { get; set; }
        public double Duration { get; set; }
        public double Price { get; set; }
        public string PaymentMethod { get; set; } = "Wallet";
    }
}
