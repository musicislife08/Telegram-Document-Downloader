using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TelegramTools.Downloader.Archive;
using TelegramTools.Downloader.Config;
using TelegramTools.Downloader.Files;
using TelegramTools.Downloader.Logging;
using TelegramTools.Downloader.Queue;
using TelegramTools.Downloader.Stats;

# region Init
Initializer.Initialize();

var configBuilder = new ConfigurationBuilder();
configBuilder.Sources.Clear();
var config = configBuilder
    .AddYamlFile("config.yaml")
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddOptions()
            .Configure<TgConfig>(x => config.GetSection("Config").Bind(x))
            .AddSingleton<IQueueService, QueueService>()
            .AddSingleton<IStatsService, StatsService>()
            .AddSingleton<ILoggerService, LoggerService>()
            .AddScoped<IArchiveService, ArchiveService>()
            .AddScoped<FileWorker>();
    })
    .Build();

#endregion

#region Run

var token = new CancellationToken();

await host.StartAsync(token);

var scope = host.Services.CreateScope();
var fileWorker = scope.ServiceProvider.GetRequiredService<FileWorker>();
await fileWorker.RunAsync(token);

#endregion