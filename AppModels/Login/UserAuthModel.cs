using System.ComponentModel.DataAnnotations;

namespace AppModels.Login
{
    public class UserAuthModel
    {
        [Required]
        public string UserID { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public Guid Session { get; set; } = Guid.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime ReleaseTime { get; set; }
        public int LoginAttempts { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public bool IsLocked => LoginAttempts >= 5 && DateTime.Now < ReleaseTime;
    }

}
