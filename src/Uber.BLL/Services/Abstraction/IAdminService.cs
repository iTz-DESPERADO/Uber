using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.BLL.ModelVM.Admin;
using Uber.DAL.Entities;

namespace Uber.BLL.Services.Abstraction
{
    public interface IAdminService
    {

        (string?, Admin?) GetByID(string id);
        List<EditAdmin> GetAll();
        (bool, string?) Delete(string id);
        (bool, string?) Edit(EditAdmin admin);

        public (string?, EditAdmin?) GetByIDToEdit(string id);
    }
}
