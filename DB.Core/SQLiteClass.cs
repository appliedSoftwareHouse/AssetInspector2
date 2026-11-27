using AppModels.DB;
using Microsoft.Data.Sqlite;
using System.Data;

namespace DB.Core
{
    public class SQLiteClass
    {
        public SqliteConnection? Connection { get; }


        public SQLiteClass(string connectionString)
        {
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", connectionString);
            if(File.Exists(dbPath))
            {
                Connection = new SqliteConnection($"Data Source={dbPath}");
            }
        }

        public SQLiteClass()
        {
        }

        public bool IsConnected => Is_Connected();

        private bool Is_Connected()
        {
            if(Connection != null)
            {
                if (Connection.State != ConnectionState.Open) Connection.Open();
                return Connection.State == ConnectionState.Open;
            }
            return false;
        }


        // 1. Get full DataTable
        public DataTable GetDataTable(string query, params SqliteParameter[] parameters)
        {


            var dataTable = new DataTable();

            if(!IsConnected) { return dataTable; }

            try
            {
               
                using var command = Connection!.CreateCommand();
                command.CommandText = query;

                if (parameters is { Length: > 0 })
                    command.Parameters.AddRange(parameters);

                using var reader = command.ExecuteReader();
                dataTable.Load(reader);
            }
            finally
            {
                if (Connection!.State == ConnectionState.Open)
                    Connection.Close();
            }

            return dataTable;
        }

        // 2. Get List<DataRow> using ToList()
        public List<CreateTableQueryModel> GetCreateTableQueries()
        {
            string _Query = "SELECT * FROM [CreateTables] ORDER BY [SR_NO];";

            return [.. GetDataTable(_Query).AsEnumerable().Select(e => new CreateTableQueryModel
                    {
                        Id = Convert.ToInt64(e["ID"]),
                        TableName = Convert.ToString(e["DataTableName"]) ?? string.Empty,
                        ColumnsScript = Convert.ToString(e["ColumnsQuery"]) ?? string.Empty
                    })];
        }

        // 3. Get single DataRow (returns null if no rows found)
        public DataRow? GetDataRow(string query, params SqliteParameter[] parameters)
        {
            DataTable table = GetDataTable(query, parameters);
            return table.Rows.Count > 0 ? table.Rows[0] : null;
        }
    }
}
