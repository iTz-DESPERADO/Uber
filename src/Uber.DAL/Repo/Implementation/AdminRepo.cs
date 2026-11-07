using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;
using Uber.DAL.Repo.Abstraction;
using Uber.DAL.DataBase;


namespace Uber.DAL.Repo.Implementation
{
    public class AdminRepo : IAdminRepo
    {
        private readonly UberDBContext db;

        public AdminRepo(UberDBContext db)
        {
            this.db = db;
        }

        public (bool, string?) Delete(string id)
        {
            try
            {
                var admin = db.Admins.Where(a => a.Id == id).FirstOrDefault();
                if (admin == null)
                {
                    return (false, "Admin not found");
                }
                admin.Delete();
                db.SaveChanges();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) Edit(Admin admin)
        {
            try
            {
                var adm = db.Admins.Where(a => a.Id == admin.Id).FirstOrDefault();
                if (adm == null)
                {
                    return (false, "Admin not found");
                }

                adm.Edit(admin.Name, admin.DateOfBirth, admin.Email, admin.PhoneNumber,admin.IsDeleted);
                db.SaveChanges();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public List<Admin> GetAll()
        {
            return db.Admins.ToList();
        }

        public (string?, Admin?) GetByID(string id)
        {
            try
            {
                var admin = db.Admins.Where(a => a.Id == id).FirstOrDefault();
                if (admin == null)
                {
                    return ("Admin not found", null);
                }
                return (null, admin);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }
















        


    }
}
