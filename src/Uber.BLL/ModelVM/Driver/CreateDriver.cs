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
    public class CreateDriver
    {

        public Vehicle? Vehicle { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public IFormFile? File { get; set; }
        public string? ImagePath { get; set; }
    }
}
