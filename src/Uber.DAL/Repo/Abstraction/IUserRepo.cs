using Uber.DAL.Entities;
namespace Uber.DAL.Repo.Abstraction
{
    public interface IUserRepo
    {
        //(bool, string?) Create(User user);
        (string?, User?) GetByID(string id);
        List<User> GetAll();
        (bool, string?) Delete(string id);
        (bool, string?) Edit(User user);
        (bool, string?) AddBalance(User user, double amount);
    }
}