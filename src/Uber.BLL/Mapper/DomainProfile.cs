using AutoMapper;
using Uber.BLL.ModelVM.Admin;
using Uber.BLL.ModelVM.Driver;
using Uber.BLL.ModelVM.Ride;
using Uber.BLL.ModelVM.User;
using Uber.DAL.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Uber.BLL.Mapper
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<User, CreateUser>().ReverseMap();
            CreateMap<User, EditUser>().ReverseMap();
            CreateMap<Driver, CreateDriver>().ReverseMap();
            CreateMap<Driver, GetDriver>().ReverseMap();  
            CreateMap<Admin, EditAdmin>().ReverseMap();
            CreateMap<Driver, EditDriver>().ReverseMap();
            CreateMap<Driver, DriverProfileVM>().ReverseMap();
            CreateMap<Ride, RideVM>().ReverseMap();
            CreateMap<Driver, ProfileVM>().ReverseMap();
            

        }
    }
}
