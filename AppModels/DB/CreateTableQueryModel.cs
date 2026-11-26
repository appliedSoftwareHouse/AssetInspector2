using System;
using System.Collections.Generic;
using System.Text;

namespace AppModels.DB
{
    public class CreateTableQueryModel
    {
        public long Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string ColumnsScript { get; set; } = string.Empty;

    }
}
