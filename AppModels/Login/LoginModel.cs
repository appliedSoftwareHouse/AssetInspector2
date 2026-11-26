using System;
using System.Collections.Generic;
using System.Text;

namespace AppModels.Login
{
    public class LoginModel
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public bool RememberMe { get; set; }
    }
}
