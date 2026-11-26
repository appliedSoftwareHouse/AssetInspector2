using AppModels.DB;
using DB.Core;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Web.Services
{
    public class SQLiteService
    {
        public SQLiteClass MySQLite { get; set; }

        public SQLiteService(ConfigurationService Config)
        {
            var SQLiteString = Config.GetValue<string>("SQLiteCreateDB");
            MySQLite = new(SQLiteString);
        }


        public DataTable GetDataTable(string query, params SqliteParameter[] parameters) => MySQLite.GetDataTable(query, parameters);
        public List<CreateTableQueryModel> GetCreateTableQueries()=>MySQLite.GetCreateTableQueries();
        public DataRow? GetDataRow(string query, params SqliteParameter[] parameters)=> MySQLite.GetDataRow(query, parameters);
        
    }
}
