using System.Diagnostics.CodeAnalysis;
using Dapper;

namespace TelegramTools.Database;

public class Database
{
    private readonly DocumentDapperContext _context;

    public Database(DocumentDapperContext context)
    {
        _context = context;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public void CreateDatabase()
    {
        const string db = "TelegramTools.db";
        if (!File.Exists(db))
            System.Data.SQLite.SQLiteConnection.CreateFile(db);
    }
}