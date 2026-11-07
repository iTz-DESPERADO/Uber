using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uber.DAL.Enums;

namespace Uber.DAL.Entities
{
    public class Ride
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Coordinates
        public double StartLat { get; set; }
        public double StartLng { get; set; }
        public double EndLat { get; set; }
        public double EndLng { get; set; }

        // Relationships
        public string DriverId { get; set; }   // the chosen (target) driver
        public Driver? Driver { get; set; }
        public string UserId { get; set; }     // requester
        public User? User { get; set; }

        // Workflow
        public RideStatus Status { get; set; } = RideStatus.Pending;

        public double? Duration { get; set; }
        public double? Distance { get; set; }
        public double? Price { get; set; }
        
        // Payment
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Wallet;
        public (bool, string?) Cancel()
        {
            try
            {
                Status = RideStatus.Cancelled;
                return (true, null);
            }

            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }






    }
}
