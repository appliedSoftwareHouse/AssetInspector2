using AppModels;
using Microsoft.Data.SqlClient;

namespace Web.Services
{
    public class DatabaseService
    {
        public SqlConnection GetConnection()
        {
            string connectionString = DB.Core.Connections.GetConnectionString();
            return new SqlConnection(connectionString);
        }

        public Login GetUserLogin(string userId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT UserID, Password FROM Users WHERE UserID = @UserID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Login
                            {
                                UserID = reader["UserID"].ToString(),
                                Password = reader["Password"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }


    }
}
