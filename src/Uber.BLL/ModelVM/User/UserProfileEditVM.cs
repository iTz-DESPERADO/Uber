using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;
using Uber.DAL.Enums;

namespace Uber.BLL.ModelVM.User
{
     public class UserProfileEditVM
    {
        public UserProfileVM Profile { get; set; } = new UserProfileVM();

        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsDeleted { get; set; }
        public UserProfileEditVM()
        {
            
        }
        public UserProfileEditVM(string name,string id,double balance, DateTime DateOfBirth,string Email,string PhoneNumber,bool isdeleted)
        {
            Profile.Name = name;
            Profile.Id = id;    
            Profile.Balance = balance;
            this.DateOfBirth=DateOfBirth;
            this.Email = Email;
            this.PhoneNumber = PhoneNumber;
            this.IsDeleted = isdeleted;
            
            
        }

    }
}