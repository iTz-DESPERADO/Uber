using Microsoft.AspNetCore.Identity;
using Uber.DAL.Enums;

namespace Uber.DAL.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() { }

        public string Name { get; set; }

        public Gender Gender { get; set; }

        public int Age()
        {
            var today = DateTime.Today;
            int age = today.Year - DateOfBirth.Year;

            if (DateOfBirth.Date > today.AddYears(-age))
                age--;

            return age;
        }
        public DateTime DateOfBirth { get; set; }

        public bool IsDeleted { get; protected set; } = false;
        public DateTime CreatedAt { get; } = DateTime.Now;

        //public Location Address { get; protected set; }

    }
}

