
namespace AppModels.Login
{
    public class UserProfile
    {
        public UserProfile() { }

        public string UserID { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Language { get; set; } = string.Empty;
        public bool? MFAuth { get; set; }
        public bool IsLock { get; set; }
        public DateTime? LockOutTime { get; set; } 
        public DateTime LoginTime { get; set; } 
        public int LogonAttampt { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public bool IsLogin { get; set; }
        public string LoginMessage { get; set; } = string.Empty;
        public Guid Session = Guid.NewGuid();
        public bool RememberMe { get; set; }
        
    }
}
