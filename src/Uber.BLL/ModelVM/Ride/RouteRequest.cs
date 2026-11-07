using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.ModelVM.Ride
{
   

        public class RouteRequest
        {
            public double StartLat { get; set; }
            public double StartLng { get; set; }
            public double EndLat { get; set; }
            public double EndLng { get; set; }
        
    }
}
