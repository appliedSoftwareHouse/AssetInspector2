using AppConfig;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DB.Core
{
    public class DBContextClass
    {
        public readonly SqlConnection Connection;
        public SqlCommand Command;
        public ConfigModel? Config { get; set; }
        public bool IsConnected => Is_Connected();

        public DBContextClass(string _ConnectionString)
        {
            Connection = new(_ConnectionString);
            Command = new("", Connection);
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
    }
}
