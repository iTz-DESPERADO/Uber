using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.ModelVM.Ride
{
    public class AcceptRejectRequest
    {
        public int id { get; set; }
        public string rideGroup { get; set; } = string.Empty;
    }

}
