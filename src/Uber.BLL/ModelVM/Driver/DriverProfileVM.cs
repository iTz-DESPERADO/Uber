using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;

namespace Uber.BLL.ModelVM.Driver
{
     public class DriverProfileVM
    {
        public string Name { get; set; }
        public List<Uber.DAL.Entities.Ride> Rides { get; set; }
        public double Balance { get; set; }
        public string Id { get; set; }
        public string? ImagePath { get; set; }
        public IFormFile file { get; set; }
        public string PaymentMethod { get; set; }
        public EditDriver Edit { get; set; }

    }
}
