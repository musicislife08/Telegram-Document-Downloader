using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TelegramTools.Database.Extensions;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        db.CreateDatabase();
        runner.MigrateUp();
        return host;
    }
}