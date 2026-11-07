using Uber.DAL.Repo.Abstraction;
using Uber.DAL.DataBase;
using Uber.DAL.Entities;

namespace Uber.DAL.Repo.Impelementation
{
    public class UserRepo : IUserRepo
    {
        private readonly UberDBContext db;

        public UserRepo(UberDBContext db)
        {
            this.db = db;
        }

        //public (bool, string?) Create(User user)
        //{
        //    try
        //    {
        //        db.Users.Add(user);
        //        db.SaveChanges();
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, ex.Message);
        //    }
        //}

        public (bool, string?) Delete(string id)
        {
            try
            {
                var user = db.Users.Where(a => a.Id == id).FirstOrDefault();
                if (user == null)
                {
                    return (false, "User not found");
                }
                user.Delete();
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public List<User> GetAll()
        {
            return db.Users.ToList();
        }

        public (string?, User?) GetByID(string id)
        {
            try
            {
                var user = db.Users.Where(a => a.Id == id).FirstOrDefault();
                if (user == null)
                {
                    return ("User not found", null);
                }
                return (null, user);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }

        public (bool, string?) Edit(User user)
        {
            try
            {
                var u = db.Users.Where(a => a.Id == user.Id).FirstOrDefault();
                if (u == null)
                {
                    return (false, "User not found");
                }
                u.Edit(user.Name, user.DateOfBirth, user.Email, user.PhoneNumber,user.IsDeleted);
                
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string?) AddBalance(User user, double amount)
        {
            try
            {
                var result = user.AddBalance(amount);
                db.SaveChanges();
                return result;
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}