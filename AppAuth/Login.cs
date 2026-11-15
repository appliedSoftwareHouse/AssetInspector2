using AppModels.Login;

namespace AppAuth
{
    public class Login
    {
        public bool CanLogin(UserAuthModel user)
        {
            if (!user.IsActive) return false;           // blocked account
            if (user.IsLocked) return false;            // temporarily locked
            return true;
        }

        public void RegisterFailedAttempt(UserAuthModel user)
        {
            // Reset counter if lockout window has expired
            if (user.LoginAttempts >= 5 && DateTime.Now >= user.ReleaseTime)
            {
                user.LoginAttempts = 0;
                user.ReleaseTime = DateTime.MinValue;
            }

            user.LoginAttempts++;

            if (user.LoginAttempts >= 5)
            {
                user.ReleaseTime = DateTime.Now.AddMinutes(5);
            }
        }

        public void RegisterSuccessfulLogin(UserAuthModel user)
        {
            user.LoginAttempts = 0;
            user.ReleaseTime = DateTime.MinValue;
            user.LoginTime = DateTime.Now;
            user.Session = Guid.NewGuid();
        }
    }
}
