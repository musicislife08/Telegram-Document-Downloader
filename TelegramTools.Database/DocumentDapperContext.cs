using System.Data;
using Microsoft.Data.Sqlite;

namespace TelegramTools.Database;

public class DocumentDapperContext
{ 
        public IDbConnection CreateConnection()
                => new SqliteConnection("Data Source=DocumentFiles.db;");
}