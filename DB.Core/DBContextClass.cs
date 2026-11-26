using AppConfig;
using Crypto;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DB.Core
{
    public class DBContextClass
    {
        public readonly SqlConnection Connection;
        public ConfigModel? Config { get; set; }
        public bool IsConnected => Is_Connected();
        public AppAuth.LoginClass MyLogin;

        public DBContextClass(string _ConnectionString)
        {
            Connection = new(_ConnectionString);
            MyLogin = new(Connection, Config!);
        }

        public void SetConfig(string ConnectionHash)
        {
            var _ConnectionString = Decrypt.DecryptString(ConnectionHash, "SQLite");
            Config = new(_ConnectionString);
        }

        private bool Is_Connected()
        {
            try
            {
                if (Connection.State != ConnectionState.Open) { Connection.Open(); }
                return Connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }



        //public UserProfile? => MyLogin.GetUserProfile(loginModel);
       

    }
}
