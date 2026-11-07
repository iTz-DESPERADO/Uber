using AutoMapper;
using Uber.BLL.ModelVM.Admin;
using Uber.BLL.ModelVM.User;
using Uber.BLL.Services.Abstraction;
using Uber.DAL.Entities;
using Uber.DAL.Repo.Abstraction;
using Uber.DAL.Repo.Impelementation;

namespace Uber.BLL.Services.Impelementation
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepo adminRepo;
        private readonly IMapper mapper;

        public AdminService(IAdminRepo adminRepo, IMapper mapper)
        {
            this.adminRepo = adminRepo;
            this.mapper = mapper;
        }

        public (bool, string?) Delete(string id)
        {

            try
            {
                var result = adminRepo.Delete(id);
                return result;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) Edit(EditAdmin admin)
        {
            try
            {
                var result = adminRepo.GetByID(admin.Id);
                var existingAdmin = result.Item2;
                if (existingAdmin == null)
                    return (false, "Admin not found");

                //existingAdmin.Edit(admin.Name, admin.DateOfBirth, admin.Email, admin.PhoneNumber);
             

                adminRepo.Edit(existingAdmin);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public List<EditAdmin> GetAll()
        {
            var result = adminRepo.GetAll();
            var list = new List<EditAdmin>();

            foreach (var admin in result)
            {
                list.Add(mapper.Map<EditAdmin>(admin));
            }
            return list;

             
        }

        public (string?, Admin?) GetByID(string id)
        {
            return adminRepo.GetByID(id);
        }


        public (string?, EditAdmin?) GetByIDToEdit(string id)
        {
            try
            {
                var result = adminRepo.GetByID(id);
                if (result.Item1 != null) return (result.Item1, null);
                var user = mapper.Map<EditAdmin>(result.Item2);
                return (null, user);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }

    }
}
