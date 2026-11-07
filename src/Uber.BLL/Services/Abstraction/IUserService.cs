using Uber.BLL.ModelVM.User;
using Uber.DAL.Entities;

namespace Uber.BLL.Services.Abstraction
{
    public interface IUserService
    {
        public Task<(bool, string?)> CreateAsync(CreateUser user);
        public (bool, string?) Delete(string id);
        public (bool, string?) Edit(EditUser user);
        public (string?, User?) GetByID(string id); // TODO Change this
        public List<EditUser> GetAll();

        public Task<(bool, string?, UserProfileEditVM?)> GetProfileInfo();
        public (string?, EditUser?) GetByIDToEdit(string id);
        public (bool, string?) AddBalance(string id, double amount);
    }
}
