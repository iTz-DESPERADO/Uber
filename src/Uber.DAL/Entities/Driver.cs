using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Uber.DAL.Enums;

namespace Uber.DAL.Entities
{
    public class Driver : ApplicationUser
    {
        //License  **

        public DateTime? ModifiedAt { get; private set; }


        public double Balance { get; set; } = 0;
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? file { get; set; }
        public int TotalRatingPoints { get; set; } 
                                                           
        public int TotalRatings { get; set; } 
        public double Rating() => TotalRatings != 0 ? (double)TotalRatingPoints / TotalRatings : 5;

        public (bool, string?) AddRating(int rating)
        {
            try
            {
                if (rating < 1 || rating > 5)
                {
                    return (false, "Rating must be between 1 and 5");
                }
                
                TotalRatingPoints += rating;
                TotalRatings += 1;
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public Driver() 
        { 
            //Wallet =new Wallet();
        }

        public Driver(string name, Gender gender, DateTime DateOfBirth, string? ImagePath/*, string street, string city*/)
        {
            this.Name = name;
            this.Gender = gender;
            this.DateOfBirth = DateOfBirth;
            this.ImagePath = ImagePath;
            //Wallet = new Wallet();
            //this.Address = new Location { Street = street, City = city };

        }

        //Navigation Property


        //public int WalletId { get; set; }

        //public Wallet Wallet { get; set; }

        public double CurrentLng { get; set; }
        public double CurrentLat { get; set; }
        public bool IsActive { get; private set; } = false;
        public List<Ride> Rides { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public string? StripeId { get; set; }
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

        public void AddProfilePhoto(string path)
        {
            if (IsDeleted) return;
            ImagePath = path;
        }
        public (bool, string?) Edit(string name, DateTime dateofbirth,string ImagePath, string Email, string PhoneNumber,bool isdeleted)
        {



            try
            {
                this.Name = name;
                this.DateOfBirth = dateofbirth;
                this.Email = Email;
                this.PhoneNumber = PhoneNumber;
                this.ImagePath = ImagePath;
                this.IsDeleted = isdeleted; 


                ModifiedAt = DateTime.Now;
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) MakeActive()
        {
            try
            {
                IsActive = true;
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public (bool, string?) MakeInactive()
        {

            try
            {
                IsActive = false; return (true, null);
            }

            catch (Exception ex) 
            { 
                return (false, ex.Message); 
            }

        }

        public (bool, string?) AddBalance(double amount)
        {
            try
            {
                Balance += amount;
                return (true, null);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
