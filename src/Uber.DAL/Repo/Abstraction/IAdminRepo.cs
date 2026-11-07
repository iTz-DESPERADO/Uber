using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.DAL.Entities;

namespace Uber.DAL.Repo.Abstraction
{
    public interface IAdminRepo
    {
        (string?, Admin?) GetByID(string id);
        List<Admin> GetAll();
        (bool, string?) Delete(string id);
        (bool, string?) Edit(Admin admin);

    }
}
