using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.ModelVM.User
{
    public class EditUser
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
