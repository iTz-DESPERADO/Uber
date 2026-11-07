using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Enums;

namespace Uber.BLL.ModelVM.User
{
    public class CreateUser
    {
        public string Name { get; set; }
        public string Email { get;  set; }
        public string Password { get;  set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}
