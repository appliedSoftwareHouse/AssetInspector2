using AppModels.Login;
using Crypto;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DB.Core
{
    public class DBContextClass
    {
        private SqlConnection Connection;
        public bool IsConnected => Is_Connected();

        public DBContextClass(string _ConnectionStraing)
        {
            Connection = new(_ConnectionStraing);
        }

        private bool Is_Connected()
        {
            try
            {
                if (Connection.State != ConnectionState.Open) { Connection.Open(); }
                return Connection.State == ConnectionState.Open;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public UserProfile? GetUserProfile(LoginModel loginModel)
        {
            var _UserProfile = new UserProfile();

            try
            {

                var _Query = $@"
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

                using SqlCommand _Command = new(_Query, Connection);

                _Command.Parameters.AddWithValue("@UserName", loginModel.UserID);
                //_Command.Parameters.AddWithValue("@PasswordHash", loginModel.PasswordHash);

                using SqlDataReader _Reader = _Command.ExecuteReader();

                if (_Reader.Read())
                {
                    
                    _UserProfile.UserName = _Reader["UserName"]?.ToString() ?? "";
                    _UserProfile.FullName = _Reader["FullName"]?.ToString() ?? "";
                    _UserProfile.Email = _Reader["Email"]?.ToString() ?? "";
                    _UserProfile.Phone = _Reader["PhoneNumber"]?.ToString() ?? "";

                    _UserProfile.IsActive = Convert.ToBoolean(_Reader["IsActive"]);

                    _UserProfile.Language = _Reader["DefaultLanguageID"]?.ToString() ?? "";

                    _UserProfile.MFAuth = Convert.ToBoolean(_Reader["TwoFactorEnabled"]);

                    _UserProfile.IsLock = Convert.ToBoolean(_Reader["LockoutEnabled"]);

                    _UserProfile.LockOutTime =
                        _Reader["LockoutEndDateUtc"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(_Reader["LockoutEndDateUtc"]);

                    _UserProfile.LogonAttampt = Convert.ToInt32(_Reader["AccessFailedCount"]);


                    var _DecryptPW = Decrypt.DecryptString(_Reader["PasswordHash"]?.ToString() ?? "", "Modern");

                    if (_DecryptPW.Equals(loginModel.Password))
                    {
                        return _UserProfile;
                    }



                }
            }
            catch
            {
                return null;
            }
            return null;

        }

        public bool UpdateLoginAttempt(string UserName)
        {
            try
            {
                var _SQL = @"
            UPDATE [dbo].[Users]
            SET [AccessFailedCount] = ISNULL([AccessFailedCount], 0) + 1
            WHERE [UserName] = @UserName;";

                using SqlCommand _Command = new(_SQL, Connection);

                _Command.Parameters.AddWithValue("@UserName", UserName);

                return _Command.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
