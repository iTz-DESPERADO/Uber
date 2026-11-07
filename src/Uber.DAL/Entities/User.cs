using System.Runtime.CompilerServices;
using Uber.DAL.Enums;

namespace Uber.DAL.Entities
{
    public class User : ApplicationUser
    {

        // ID
        // Pass
        // Phone
        //public string Email { get; private set; }

        public DateTime? ModifiedAt { get; set; }


        public int TotalRatingPoints { get; set; } 
        public int TotalRatings { get; set; } 
        public double Rating() => TotalRatings != 0 ? (double)TotalRatingPoints / TotalRatings : 5;
        public string? StripeId { get; set; }

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

        public User() { }

        public User( string name,Gender gender,DateTime DateOfBirth/*, string street,string city*/)
        {
            this.Name = name;
            this.Gender = gender;         
            this.DateOfBirth = DateOfBirth;
           
            //this.Address = new Location { Street = street, City = city };

        }
        //Navigation Property

        //public int WalletId { get;  set; }

        //public Wallet Wallet { get;  set; }



        public List <Ride> Rides { get; set; }



        public double Balance { get;  set; } = 0;


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

        //public void AddProfilePhoto(string path)
        //{
        //    if (IsDeleted) return;
        //    ImagePath = path;
        //}


       

        
        public (bool, string?) Edit(string name, DateTime dateofbirth, string Email, string PhoneNumber,bool isdeleted)
        {

         

            try
            {
                this.Name = name;
                this.DateOfBirth = dateofbirth;
                this.Email = Email;
                this.PhoneNumber = PhoneNumber;
                this.IsDeleted = isdeleted;


                ModifiedAt = DateTime.Now;
                return (true, null);
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
