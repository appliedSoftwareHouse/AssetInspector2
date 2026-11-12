using System;
using System.Collections.Generic;
using System.Text;

namespace DB.Core
{
    public static class Connections
    {
        public static string GetConnectionString()
        {
            // Retrieve the connection string from configuration or environment variables
            return "Data Source=.\\SQLEXPRESS;Initial Catalog=Web2;User ID=sa;Password=P@ssw0rdsqlserver;";
        }

    }
}
