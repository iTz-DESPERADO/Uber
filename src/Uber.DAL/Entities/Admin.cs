using System.IO;
using Uber.DAL.Enums;

namespace Uber.DAL.Entities
{
    public class Admin: ApplicationUser
    {
        public Admin(string name, Gender gender, DateTime DateOfBirth) { 
            this.Name = Name;
            this.Gender = Gender;        
            this.DateOfBirth = DateOfBirth;
           
     

        }
        public Admin()
        {
           
        }

        public (bool, string?) Delete()
        {
            try
            {
                IsDeleted = true;
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) Edit(string name, DateTime dateofbirth, string Email, string PhoneNumber,bool isdeleted)
        {



            try
            {
                this.Name = name;
                this.DateOfBirth = dateofbirth;
                this.Email = Email;
                this.PhoneNumber = PhoneNumber;
                this.IsDeleted = isdeleted;



                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
