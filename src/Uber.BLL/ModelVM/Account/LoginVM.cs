using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.ModelVM.Account
{
    public class LoginVM
    {

        public string Email { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; } = true;

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
