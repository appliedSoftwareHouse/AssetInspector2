using AppConfig;
using AppModels.DB;
using DB.Core;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Web.Services
{
    public class SQLiteService
    {
        public SQLiteClass LiteService { get; set; }

        public SQLiteService(ConfigKeys keys)
        {
            var SQLiteString = keys.Get<string>("DBCreateQuriesFile");
            LiteService = new(SQLiteString);
        }


        public DataTable GetDataTable(string query, params SqliteParameter[] parameters) => LiteService.GetDataTable(query, parameters);
        public List<CreateTableQueryModel> GetCreateTableQueries()=> LiteService.GetCreateTableQueries();
        public DataRow? GetDataRow(string query, params SqliteParameter[] parameters)=> LiteService.GetDataRow(query, parameters);
        
    }
}
