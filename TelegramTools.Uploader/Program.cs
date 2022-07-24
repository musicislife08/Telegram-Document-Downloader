using ByteSizeLib;
using Spectre.Console;
using Spectre.Console.Rendering;
using TelegramTools.Uploader.Archive;
using TelegramTools.Uploader.Config;
using TelegramTools.Uploader.Files;
using TelegramTools.Uploader.Queue;
using TelegramTools.Uploader.Stats;
using TelegramTools.Uploader.Telegram;
using ConfigurationManager = TelegramTools.Uploader.Config.ConfigurationManager;

#region Init

var firstRun = false;
var config = ConfigurationManager.GetConfiguration();

if (string.IsNullOrWhiteSpace(config.PhoneNumber))
{
    config.PhoneNumber = AnsiConsole.Prompt(new TextPrompt<string?>("Enter Phone Number in [yellow]+1xxxxxxx format:[/]"));
    ConfigurationManager.SaveConfiguration(config);
    firstRun = true;
}

if (string.IsNullOrWhiteSpace(config.GroupName))
{
    config.GroupName = AnsiConsole.Prompt(
        new TextPrompt<string?>("Enter Telegram group name:"));
    ConfigurationManager.SaveConfiguration(config);
    firstRun = true;
}

if (string.IsNullOrWhiteSpace(config.UploadPath))
{
    config.UploadPath = AnsiConsole.Prompt(
                              new TextPrompt<string?>("Enter path to the folder to upload files from:"))
                          ?? Environment.CurrentDirectory;
    ConfigurationManager.SaveConfiguration(config);
    firstRun = true;
}

if (string.IsNullOrWhiteSpace(config.GroupName))
    throw new ConfigValueException(nameof(config.GroupName));
if (string.IsNullOrWhiteSpace(config.PhoneNumber))
    throw new ConfigValueException(nameof(config.PhoneNumber));
if (string.IsNullOrWhiteSpace(config.UploadPath))
    throw new ConfigValueException(nameof(config.UploadPath));
if (!Directory.Exists(config.UploadPath))
    Directory.CreateDirectory(config.UploadPath);

if (firstRun)
{
    AnsiConsole.MarkupLine("[bold yellow]Initial configuration created.[/]  [bold red]Before running again double check config.yaml file[/]");
    Environment.Exit(0);
}
WTelegram.Helpers.Log = (lvl, str) => System.Diagnostics.Debug.WriteLine($"WTelegram {lvl}: {str}");
using var client = new WTelegram.Client(ConfigurationManager.WTelegramConfig);
await client.LoginUserIfNeeded();
client.Dispose();

#endregion

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddSingleton(config)
            .AddSingleton<IQueueService, QueueService>()
            .AddSingleton<IStatsService, StatsService>()
            .AddScoped<IArchiveService, ArchiveService>()
            .AddScoped<ITelegramService, TelegramService>()
            .AddScoped<FileWorker>()
            .AddScoped<TelegramWorker>()
            .BuildServiceProvider(false);
    })
    .Build();

#region Run

var token = new CancellationToken();
await host.StartAsync(token);
var scope = host.Services.CreateScope();
var fileWorker = scope.ServiceProvider.GetService<FileWorker>();
if (fileWorker is not null)
{
  await fileWorker.RunAsync(token);
}

var tgWorker = scope.ServiceProvider.GetService<TelegramWorker>();
if (tgWorker is not null)
{
    await tgWorker.RunAsync(token);
}
var statsService = scope.ServiceProvider.GetService<IStatsService>();
if (statsService is null)
    throw new NullReferenceException($"StatsService is null");
var stats = statsService.GetStats();
var table = new Table
{
    Title = new TableTitle("Run Statistics")
};
table.AddColumn("Uploaded");
table.AddColumn("Existing");
table.AddColumn("Duplicate");
table.AddColumn("Filtered");
table.AddColumn("Errored");
table.AddColumn("Total Uploaded");
var row = new TableRow(new List<IRenderable>()
{
    new Markup($"[green]{stats.Uploaded}[/]"),
    new Markup($"[green]{stats.Existing}[/]"),
    new Markup($"[yellow]{stats.Duplicate}[/]"),
    new Markup($"[grey]{stats.Filtered}[/]"),
    new Markup($"[red]{stats.Errored}[/]"),
    new Markup($"[green]{ByteSize.FromBytes(stats.UploadedBytes)}[/]")
});
table.AddRow(row);
await host.StopAsync();
try
{
    Directory.Delete(config.TempPath, true);
}
catch
{
    // ignored
}

AnsiConsole.Write(table);

#endregion
