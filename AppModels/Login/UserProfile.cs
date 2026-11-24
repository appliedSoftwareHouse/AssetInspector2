
namespace AppModels.Login
{
    public class UserProfile
    {
        public UserProfile() { }

        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Language { get; set; } = string.Empty;
        public bool? MFAuth { get; set; }
        public bool IsLock { get; set; }
        public DateTime? LockOutTime { get; set; } 
        public int LogonAttampt { get; set; }
        public string UserRole { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        public string UserID { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    
}
