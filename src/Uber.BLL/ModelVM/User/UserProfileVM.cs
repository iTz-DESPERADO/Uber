using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;

namespace Uber.BLL.ModelVM.User
{
    public class UserProfileVM
    {
        public string Name { get; set; }
        public List<Uber.DAL.Entities.Ride> Rides { get; set; }
        public double Balance { get; set; }
        public string Id{ get; set; }

    }
}
