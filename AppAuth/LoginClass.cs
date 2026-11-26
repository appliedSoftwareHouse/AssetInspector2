using AppConfig;
using AppModels.Login;
using Crypto;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AppAuth
{
    interface ILoginClass
    {
        bool IsConnected { get; set; }
        UserProfile GetUserProfile(LoginModel loginModel);
        bool UpdateLoginAttempt(string userName);
        void UserLocked(string userName);
        void ResetLoginAttempts(string userName);
    }


    public class LoginClass
    {
        private SqlConnection? Connection;
        private ConfigModel? Config;
        public LoginClass(SqlConnection _Connection, ConfigModel _Config)
        {
            Connection = _Connection;
            Config = _Config;
        }

        public LoginClass(ConfigModel _Config)
        {
            Config = _Config;
        }


        public LoginClass()
        {
        }

        public bool IsConnected => Is_Connected();
        private bool Is_Connected()
        {
            try
            {
                if(Connection != null)
                {
                    if (Connection!.State != ConnectionState.Open) { Connection.Open(); }
                    return Connection.State == ConnectionState.Open;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public UserProfile GetUserProfile(LoginModel loginModel)
        {
            

            UserProfile? _UserProfile = new() { IsLogin = false };
            if (!Is_Connected())
            {
                _UserProfile.LoginMessage = "dbNotConnected";
                return _UserProfile;
            }
            string storedPasswordHash = string.Empty;
            bool IsUserFound = false;

            try
            {
                const string _Query = @"
                    SELECT
                        [UserName],
                        [PasswordHash],
                        [FullName],
                        [Email],
                        [PhoneNumber],
                        [IsActive],
                        [DefaultLanguageID],
                        [TwoFactorEnabled],
                        [LockoutEnabled],
                        [LockoutEndDateUtc],
                        [AccessFailedCount]
                    FROM [Web2].[dbo].[Users]
                    WHERE [UserName] = @UserName;";

                using (SqlCommand _Command = new(_Query, Connection))
                {
                    _Command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = loginModel.UserID;

                    
                    using SqlDataReader _Reader = _Command.ExecuteReader();
                    if (_Reader.Read())
                    {
                        storedPasswordHash = _Reader["PasswordHash"]?.ToString() ?? "";

                        _UserProfile.UserID = _Reader["UserName"]?.ToString() ?? "";
                        _UserProfile.DisplayName = _Reader["FullName"]?.ToString() ?? "";
                        _UserProfile.Email = _Reader["Email"]?.ToString() ?? "";
                        _UserProfile.PhoneNumber = _Reader["PhoneNumber"]?.ToString() ?? "";
                        _UserProfile.IsActive = Convert.ToBoolean(_Reader["IsActive"]);
                        _UserProfile.Language = _Reader["DefaultLanguageID"]?.ToString() ?? "";
                        _UserProfile.MFAuth = Convert.ToBoolean(_Reader["TwoFactorEnabled"]);
                        _UserProfile.IsLock = Convert.ToBoolean(_Reader["LockoutEnabled"]);
                        _UserProfile.LockOutTime = _Reader["LockoutEndDateUtc"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(_Reader["LockoutEndDateUtc"]);
                        _UserProfile.LogonAttampt = _Reader["AccessFailedCount"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(_Reader["AccessFailedCount"]);

                        _UserProfile.LoginTime = DateTime.Now;
                        IsUserFound = true;
                    }
                } // _Reader is closed here so subsequent SQL commands can run safely

                if(!IsUserFound)
                {
                    _UserProfile.LoginMessage = "UserNoFound";
                    return _UserProfile;
                }


                if (!_UserProfile.IsActive)
                {
                    _UserProfile.LoginMessage = "UserNoActive";
                    return _UserProfile;
                }

                // 1. Check if account is currently locked
                if (_UserProfile.IsLock && _UserProfile.LockOutTime.HasValue)
                {
                    if (_UserProfile.LockOutTime.Value > DateTime.UtcNow)
                    {
                        _UserProfile.LoginMessage = "UserLocked";
                        return _UserProfile;
                    }
                    else
                    {
                        // Lockout expired -> unlock and reset counter
                        ResetLoginAttempts(_UserProfile.UserID);
                        _UserProfile.IsLock = false;
                        _UserProfile.LogonAttampt = 0;
                    }
                }

                // 2. Verify password
                var _DecryptPW = Decrypt.DecryptString(storedPasswordHash, "Modern");
                loginModel.Password = Decrypt.DecryptString(loginModel.PasswordHash, "Modern");

                if (_DecryptPW.Equals(loginModel.Password))
                {
                    loginModel.Password = string.Empty;             // for security purpose, palin password make empty.

                    if (_UserProfile.LogonAttampt > 0)
                    {
                        ResetLoginAttempts(_UserProfile.UserID);
                    }
                    _UserProfile.IsLogin = true;
                    _UserProfile.LoginMessage = "UserLogin";
                    return _UserProfile;
                }
                else
                {
                    int newAttemptCount = _UserProfile.LogonAttampt + 1;
                    UpdateLoginAttempt(_UserProfile.UserID);

                    if (newAttemptCount >= 5)
                    {
                        UserLocked(_UserProfile.UserID);
                        _UserProfile.LoginMessage = "UserLocked";
                        return _UserProfile;
                    }
                    _UserProfile.LoginMessage = "UserNotLogin";
                    return _UserProfile;

                }
            }
            catch
            {
                _UserProfile.LoginMessage = "LoginError";
                return _UserProfile;
            }
        }

        public bool UpdateLoginAttempt(string userName)
        {
            if (!Is_Connected()) return false;

            try
            {
                const string _SQL = @"
                    UPDATE [Web2].[dbo].[Users]
                    SET [AccessFailedCount] = ISNULL([AccessFailedCount], 0) + 1
                    WHERE [UserName] = @UserName;";

                using SqlCommand _Command = new(_SQL, Connection);
                _Command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName;

                return _Command.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }

        public void UserLocked(string userName)
        {
            if (!Is_Connected()) return;

            int lockoutMinutes = Config?.GetValue<int>("LockoutMinutes") ?? 15; // Fallback to 15 mins if Config is null

            const string _SQL = @"
                UPDATE [Web2].[dbo].[Users]
                SET [LockoutEnabled]    = 1,
                    [LockoutEndDateUtc] = DATEADD(MINUTE, @Minutes, GETUTCDATE())
                WHERE [UserName] = @UserName;";

            using SqlCommand _Command = new(_SQL, Connection);
            _Command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName;
            _Command.Parameters.Add("@Minutes", SqlDbType.Int).Value = lockoutMinutes;

            _Command.ExecuteNonQuery();
        }

        public void ResetLoginAttempts(string userName)
        {
            if (!Is_Connected()) return;

            const string _SQL = @"
                UPDATE [Web2].[dbo].[Users]
                SET [AccessFailedCount] = 0,
                    [LockoutEnabled]    = 0,
                    [LockoutEndDateUtc] = NULL
                WHERE [UserName] = @UserName;";

            using SqlCommand _Command = new(_SQL, Connection);
            _Command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName;

            _Command.ExecuteNonQuery();
        }

        
    }
}
