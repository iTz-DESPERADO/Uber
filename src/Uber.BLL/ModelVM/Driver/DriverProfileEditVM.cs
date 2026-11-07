using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.BLL.ModelVM.User;
using Uber.DAL.Entities;
using Uber.DAL.Enums;

namespace Uber.BLL.ModelVM.Driver
{
     public class DriverProfileEditVM
    {
        public DriverProfileVM Profile { get; set; } = new DriverProfileVM();
        public Vehicle? Vehicle { get; set; }

        DriverProfileEditVM(){}
        public DriverProfileEditVM(string name, string id, double balance, DateTime DateOfBirth, string Email, string PhoneNumber, bool isdeleted)
        {
            //Profile.Name = name;
            //Profile.Id = id;
            //Profile.Balance = balance;
            //this.DateOfBirth = DateOfBirth;
            //this.Email = Email;
            //this.PhoneNumber = PhoneNumber;
            //this.IsDeleted = isdeleted;


        }
    }
}