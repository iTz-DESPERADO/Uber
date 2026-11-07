using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uber.DAL.Enums;

namespace Uber.DAL.Entities
{
    public class Vehicle
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get;  set; }

       

        public string Brand { get;  set; }

        public string Model { get;  set; }

        public int YearMade { get;  set; }

        public string Plate { get;  set; }

        public string Color { get;  set; }

        public int SeatingCapacity {  get;  set; }

        public VehicleType Type { get;  set; }

        public Transmission Transmission { get;  set; } // Manual or Automatic

        public VehicleEngine VehicleEngine { get;  set; } // Gas or Electric



        public string Get_Type() {

            switch (Type)
            {
                case VehicleType.Car:
                    return "Car";
                    break;
                case VehicleType.Scooter:
                    return "Scooter";
                    break;
                    case VehicleType.Shuttle:
                    return "Shuttle";
                    break;

                default:
                    return "";
                    break;

            }

        }

        public Vehicle() { }

        public Vehicle(string brand, string model, int year, string plate, string color, int seatingCapacity, VehicleType type, Transmission transmission, VehicleEngine vehicleEngine)
        {
            Brand = brand;
            Model = model;
            YearMade = year;
            Plate = plate;
            Color = color;
            SeatingCapacity = seatingCapacity;
            Type = type;
            Transmission = transmission;
            VehicleEngine = vehicleEngine;
        }




        //License  **  

    }
}
