using System.ComponentModel.DataAnnotations;

namespace AppModels.Login
{

    public class UserSession
    {
        public string UserID { get; set; }
        public string DisplayName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserRole { get; set; }
        public string Designation { get; set; }
        public Guid Session { get; set; }
        public bool RememberMe { get; set; }
        public DateTime LoginTime { get; set; }

        

    }

    

    }
