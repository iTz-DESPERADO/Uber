using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;
using Uber.DAL.Enums;

namespace Uber.BLL.ModelVM.Driver
{
    public class ProfileVM
    {

        public string Id { get; set; }
        public required string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender Gender { get; set; }

        public Vehicle? Vehicle { get; set; }
     
    }
}
