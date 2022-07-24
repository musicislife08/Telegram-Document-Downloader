using Spectre.Console;
using TL;
using System.Security.Cryptography;
using ByteSizeLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramTools.Database;
using TelegramTools.Database.Entities;
using TelegramTools.LegacyDownloader.Config;
using TelegramTools.LegacyDownloader.Documents;
using ConfigurationManager = TelegramTools.LegacyDownloader.Config.ConfigurationManager;
using Document = TL.Document;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

const string apiId = "2252206";
const string apiHash = "4dcf9af0c05042ca938a0a44bfb522dd";
var config = new Configuration();

try
{
    var debug = args.Contains("-d");
    AnsiConsole.Write(
        new FigletText("Telegram Group Downloader")
            .Centered()
            .Color(Color.Orange1));
    await AnsiConsole.Status()
        .StartAsync("Initializing...", async ctx =>
        {
            ctx.Spinner = Spinner.Known.Bounce;
            await using (var db = new DocumentContext())
            {
                await db.Database.MigrateAsync();
                //await db.Database.EnsureCreatedAsync();
            }

            config = ConfigurationManager.GetConfiguration();
            WTelegram.Helpers.Log = (lvl, str) => System.Diagnostics.Debug.WriteLine($"{lvl}: {str}");
        });

    if (string.IsNullOrWhiteSpace(config.PhoneNumber))
    {
        config.PhoneNumber =
            AnsiConsole.Prompt(new TextPrompt<string?>("Enter Phonenumber in [yellow]+1xxxxxxx[/] format:"));
        ConfigurationManager.SaveConfiguration(config);
    }

    if (string.IsNullOrWhiteSpace(config.GroupName))
    {
        config.GroupName = AnsiConsole.Prompt(
            new TextPrompt<string?>("Enter Telegram group name:"));
        ConfigurationManager.SaveConfiguration(config);
    }

    if (string.IsNullOrWhiteSpace(config.DownloadPath))
    {
        config.DownloadPath = AnsiConsole.Prompt(
                                  new TextPrompt<string?>("Enter target download folder:"))
                              ?? Environment.CurrentDirectory;
        ConfigurationManager.SaveConfiguration(config);
    }

    if (string.IsNullOrWhiteSpace(config.DocumentExtensionFilter))
    {
        config.DocumentExtensionFilter = AnsiConsole.Prompt(
            new TextPrompt<string?>("Enter comma separated list of allowed extensions:"));
        ConfigurationManager.SaveConfiguration(config);
    }

    if (debug)
    {
        AnsiConsole.MarkupLine("[blue]DEBUG: Config[/]");
        AnsiConsole.MarkupLine("CONFIG__PHONENUMBER: [yellow]{0}[/]", config.PhoneNumber!);
        AnsiConsole.MarkupLine("CONFIG__SESSIONPATH: [yellow]{0}[/]", config.SessionPath);
        AnsiConsole.MarkupLine("CONFIG__DOWNLOADPATH: [yellow]{0}[/]", config.DownloadPath);
        AnsiConsole.MarkupLine("CONFIG__GROUPNAME: [yellow]{0}[/]", config.GroupName!);
        AnsiConsole.MarkupLine("CONFIG__DOCUMENTEXTENSIONFILTER: [yellow]{0}[/]", config.DocumentExtensionFilter!);
        WTelegram.Helpers.Log = (lvl, str) =>
            AnsiConsole.MarkupLineInterpolated($"WTelegram: {Enum.GetName(typeof(LogLevel), lvl)} - {str}");
    }

    if (string.IsNullOrWhiteSpace(config.GroupName))
        throw new ConfigValueException(nameof(config.GroupName));
    if (string.IsNullOrWhiteSpace(config.PhoneNumber))
        throw new ConfigValueException(nameof(config.PhoneNumber));

    EnsureDownloadPathExists(config.DownloadPath);
    if (!string.IsNullOrWhiteSpace(config.SessionPath))
        EnsureDownloadPathExists(config.SessionPath);
    using var client = new WTelegram.Client(Config);
    client.CollectAccessHash = true;
    client.PingInterval = 60;
    client.MaxAutoReconnects = 30;
    //client.FilePartSize = 10240;
    await client.LoginUserIfNeeded();

    var groups = await client.Messages_GetAllChats();
    var group = (Channel)groups.chats.First(x => x.Value.Title == config.GroupName && x.Value.IsActive).Value;
    //AnsiConsole.MarkupLine($"Getting documents from group [yellow]{config.GroupName}[/]");
    var hc = client.GetAccessHashFor<Channel>(group.ID);
    var msgs = await client.Messages_Search(new InputPeerChannel(group.ID, hc), string.Empty,
        new InputMessagesFilterDocument());
    var totalGroupFiles = msgs.Count;
    var downloadedFiles = 0;
    var duplicateFiles = 0;
    var filteredFiles = 0;
    var existingFiles = 0;
    var erroredFiles = 0;
    long downloadedBytes = 0;
    long totalBytes = 0;
    var logs = new List<Markup>();
    var errorLogs = new List<Markup>();
    var table = new Table()
        .Centered()
        .HideHeaders();
    table.AddColumn("1");
    table.Columns[0].NoWrap = true;
    var textData = Markup.FromInterpolated($"Found [green]{totalGroupFiles}[/] Documents");
    logs = AddLog(logs, textData);
    table = BuildTable(table, logs, totalGroupFiles, 0, 0, 0, 0, 0);
    await AnsiConsole.Live(table)
        .StartAsync(async ctx =>
        {
            ctx.Refresh();
            for (var i = 0; i <= totalGroupFiles; i += 100)
            {
                msgs = await client.Messages_Search(new InputPeerChannel(group.ID, hc), string.Empty,
                    new InputMessagesFilterDocument(), offset_id: 0, limit: 100, add_offset: i);
                foreach (var msg in msgs.Messages)
                {
                    if (msg is not Message { media: MessageMediaDocument { document: Document document } })
                    {
                        erroredFiles++;
                        var message = (Message)msg;
                        var logMsg = Markup.FromInterpolated($"Error: [orange1]{message.message}[/]");
                        logs = AddLog(logs, logMsg);
                        errorLogs = AddLog(errorLogs, logMsg, false);
                        table = BuildTable(
                            table,
                            logs,
                            totalGroupFiles,
                            downloadedFiles,
                            duplicateFiles,
                            filteredFiles,
                            existingFiles,
                            erroredFiles);
                        ctx.Refresh();
                        continue;
                    }

                    var sanitizedName = SanitizeString(document.Filename);
                    var info = new FileInfo(config.DownloadPath + $"/{sanitizedName}");
                    var wanted = config.DocumentExtensionFilter!.Split(",");
                    if (wanted.Length > 0 && !wanted.Contains(info.Extension.Replace(".", "").ToLower()))
                    {
                        filteredFiles++;
                        logs = AddLog(logs, Markup.FromInterpolated($"Skipping Filtered: [red]{sanitizedName}[/]"));
                        table = BuildTable(
                            table,
                            logs,
                            totalGroupFiles,
                            downloadedFiles,
                            duplicateFiles,
                            filteredFiles,
                            existingFiles,
                            erroredFiles);
                        ctx.Refresh();
                        continue;
                    }

                    var choice = await DocumentManager.DecidePreDownload(info, document.ID, document.size);
                    switch (choice)
                    {
                        case PreDownloadProcessingDecision.Update:
                        {
                            await using var db = new DocumentContext();
                            var uHash = GetFileHash(info.FullName);
                            if (await db.DocumentFiles.AnyAsync(x => x.Hash == uHash))
                                break;
                            await db.DocumentFiles.AddAsync(new DocumentFile()
                            {
                                Name = info.Name,
                                Extension = info.Extension.Remove(0, 1),
                                FullName = info.FullName,
                                Hash = GetFileHash(info.FullName),
                                TelegramId = document.ID
                            });
                            await db.SaveChangesAsync();
                            totalBytes += info.Length;
                            existingFiles++;
                            logs = AddLog(logs,
                                Markup.FromInterpolated($"Updating Existing: [green]{sanitizedName}[/]"));
                            table = BuildTable(
                                table,
                                logs,
                                totalGroupFiles,
                                downloadedFiles,
                                duplicateFiles,
                                filteredFiles,
                                existingFiles,
                                erroredFiles);
                            ctx.Refresh();
                            continue;
                        }
                        case PreDownloadProcessingDecision.Nothing:
                            totalBytes += document.size;
                            existingFiles++;
                            logs = AddLog(logs,
                                Markup.FromInterpolated($"Skipping Existing: [green]{sanitizedName}[/]"));
                            table = BuildTable(
                                table,
                                logs,
                                totalGroupFiles,
                                downloadedFiles,
                                duplicateFiles,
                                filteredFiles,
                                existingFiles,
                                erroredFiles);
                            ctx.Refresh();
                            continue;
                        case PreDownloadProcessingDecision.ExistingDuplicate:
                        {
                            duplicateFiles++;
                            await using var dupeDb = new DocumentContext();
                            var existing = await dupeDb.DuplicateFiles.FirstAsync(x => x.TelegramId == document.ID);
                                logs = AddLog(logs,
                                Markup.FromInterpolated($"Existing Duplicate: [red]{sanitizedName}[/] is duplicate of [green]{existing.OrignalName}[/]"));
                            table = BuildTable(
                                table,
                                logs,
                                totalGroupFiles,
                                downloadedFiles,
                                duplicateFiles,
                                filteredFiles,
                                existingFiles,
                                erroredFiles);
                            ctx.Refresh();
                            continue;
                        }
                    }
                    switch (choice)
                    {
                        case PreDownloadProcessingDecision.ReDownload:
                            logs = AddLog(logs, Markup.FromInterpolated($"Re-downloading Partially Downloaded: [yellow] {sanitizedName}[/]"));
                            table = BuildTable(
                                table,
                                logs,
                                totalGroupFiles,
                                downloadedFiles,
                                duplicateFiles,
                                filteredFiles,
                                existingFiles,
                                erroredFiles);
                            ctx.Refresh();
                            break;
                        case PreDownloadProcessingDecision.SaveAndDownload:
                            logs = AddLog(logs, Markup.FromInterpolated($"Downloading: [yellow] {sanitizedName}[/]"));
                            table = BuildTable(
                                table,
                                logs,
                                totalGroupFiles,
                                downloadedFiles,
                                duplicateFiles,
                                filteredFiles,
                                existingFiles,
                                erroredFiles);
                            ctx.Refresh();
                            break;
                    }

                    try
                    {
                        await using var fs = info.Create();
                        await client.DownloadFileAsync(document, fs);
                        fs.Close();
                    }
                    catch (RpcException e)
                    {
                        erroredFiles++;
                        var errorMessage = Markup.FromInterpolated(
                            $"Download Error: {e.Message} - [red]{sanitizedName}[/]");
                        logs = AddLog(logs, errorMessage);
                        errorLogs = AddLog(errorLogs, errorMessage, false);
                        table = BuildTable(
                            table,
                            logs,
                            totalGroupFiles,
                            downloadedFiles,
                            duplicateFiles,
                            filteredFiles,
                            existingFiles,
                            erroredFiles);
                        ctx.Refresh();
                        continue;
                    }
                    var hash = GetFileHash(info.FullName);
                    var postChoice = await DocumentManager.DecidePostDownload(info, hash);
                    await using var context = new DocumentContext();
                    if (postChoice == PostDownloadProcessingDecision.ProcessDuplicate)
                    {
                        var dbFile = context.DocumentFiles.First(x => x.Hash == hash);
                        if (!context.DuplicateFiles.Any(x => x.TelegramId == document.ID))
                        {
                            context.DuplicateFiles.Add(new DuplicateFile()
                            {
                                OrignalName = dbFile.Name,
                                DuplicateName = sanitizedName,
                                Hash = hash,
                                TelegramId = document.ID
                            });
                            await context.SaveChangesAsync();
                        }
                        info.Delete();
                        duplicateFiles++;
                        logs = AddLog(logs,
                            Markup.FromInterpolated(
                                $"Cleaned Up:[red] {sanitizedName}[/] is duplicate of [green] {dbFile.Name}[/]"));
                        table = BuildTable(
                            table,
                            logs,
                            totalGroupFiles,
                            downloadedFiles,
                            duplicateFiles,
                            filteredFiles,
                            existingFiles,
                            erroredFiles);
                        ctx.Refresh();
                        continue;
                    }

                    var doc = new DocumentFile()
                    {
                        Name = info.Name,
                        Extension = info.Extension.Replace(".", ""),
                        Hash = hash,
                        FullName = info.FullName,
                        TelegramId = document.ID
                    };
                    context.DocumentFiles.Add(doc);
                    await context.SaveChangesAsync();
                    downloadedBytes += info.Length;
                    totalBytes += info.Length;
                    downloadedFiles++;
                    logs = AddLog(logs, Markup.FromInterpolated($"Downloaded:[green bold] {SanitizeString(sanitizedName)}[/]"));
                    table = BuildTable(
                        table,
                        logs,
                        totalGroupFiles,
                        downloadedFiles,
                        duplicateFiles,
                        filteredFiles,
                        existingFiles,
                        erroredFiles);
                    ctx.Refresh();
                }
            }
        });

    await using var docContext = new DocumentContext();
    var totalSize = string.Empty;
    var downloadedSize = string.Empty;
    var archiveSize = string.Empty;
    AnsiConsole.Clear();
    await AnsiConsole.Status()
        .StartAsync("Calculating results...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Pong);
            totalSize = await CalculateDirectorySize(new DirectoryInfo(config.DownloadPath));
            archiveSize = ConvertBytesToString(totalBytes);
            downloadedSize = ConvertBytesToString(downloadedBytes);
        });
    var finalTable = new Table().Centered().Expand();
    var runTable = new Table().Centered();
    var groupTable = new Table().Centered();
    var errorTable = new Table().Centered();

    runTable
        .AddColumn("Existing")
        .AddColumn("Downloaded")
        .AddColumn("Errored")
        .AddColumn("Filtered")
        .AddColumn("Download Folder Size")
        .AddColumn("Downloaded Files Size");

    groupTable
        .AddColumn("Total Files")
        .AddColumn("Duplicated Files")
        .AddColumn("Total Unique Files")
        .AddColumn("Total Archive Size");

    errorTable
        .AddColumn("").HideHeaders();

    finalTable.AddColumn("Run Stats").AddColumn("Group Stats");

    runTable.AddRow(
        new Markup($"[green]{existingFiles}[/]"),
        new Markup($"[purple]{downloadedFiles}[/]"),
        new Markup($"[red]{erroredFiles}[/]"),
        new Markup($"[grey]{filteredFiles}[/]"),
        new Markup($"[green]{totalSize}[/]"),
        new Markup($"[green]{downloadedSize}[/]"));

    groupTable.AddRow(
        new Markup($"[green]{totalGroupFiles}[/]"),
        new Markup($"[red]{duplicateFiles}[/]"),
        new Markup($"[green]{existingFiles + downloadedFiles}[/]"),
        new Markup($"[green]{archiveSize}[/]")
    );

    foreach (var log in errorLogs)
    {
        errorTable.AddRow(log);
    }
    
    finalTable.AddRow(runTable, groupTable);
    finalTable.AddRow(errorTable);

    AnsiConsole.Write(finalTable);

    string Config(string what)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(what));
        return what switch
        {
            "api_id" => apiId,
            "api_hash" => apiHash,
            "phone_number" => config.PhoneNumber!,
            "verification_code" => AnsiConsole.Prompt(new TextPrompt<string>("[bold red]Enter Verification Code:[/]")
                .PromptStyle("red")
                .Secret()),
            "first_name" => "John" // if sign-up is required
            ,
            "last_name" => "Doe" // if sign-up is required
            ,
            "password" => AnsiConsole.Prompt(new TextPrompt<string>("[bold red]Enter 2fa password:[/] ")
                .PromptStyle("red")
                .Secret()),
            "session_pathname" => Path.Combine(config.SessionPath, "tg.session"),
            _ => null!
        };
    }
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
}

string GetFileHash(string filename)
{
    using var sha256 = SHA256.Create();
    using var stream = File.OpenRead(filename);
    var hash = sha256.ComputeHash(stream);
    return BitConverter.ToString(hash).Replace("-", "");
}

async Task<string> CalculateDirectorySize(DirectoryInfo directory)
{
    var files = Array.Empty<FileInfo>();
    await Task.Run(() => files = directory.GetFiles());
    return ConvertBytesToString(files.Sum(file => file.Length));
}

static string ConvertBytesToString(long bytes)
{
    return ByteSize.FromBytes(bytes).ToBinaryString();
}

static string SanitizeString(string value)
{
    var validCharacters = new char[value.Length];
    var next = 0;
    foreach(var c in value)
    {
        switch(c)
        {
            case '\r':
                break;
            case '\n':
                break;
            default:
                validCharacters[next++] = c;
                break;
        }
    }
    return new string(validCharacters, 0, next).Trim();
}

static Table BuildTable(Table table,
    IEnumerable<Markup> logEntries,
    int totalFiles,
    int downloadedFiles,
    int duplicateFiles,
    int filteredFiles,
    int existingFiles,
    int erroredFiles) {
    var data1 = new BreakdownChartItem("Existing", existingFiles, Color.Green);
    var data2 = new BreakdownChartItem("Downloaded", downloadedFiles, Color.Purple);
    var data3 = new BreakdownChartItem("Errored", erroredFiles, Color.Red3);
    var data4 = new BreakdownChartItem("Duplicate", duplicateFiles, Color.Red);
    var data5 = new BreakdownChartItem("Filtered", filteredFiles, Color.Grey);
    var data6 = new BreakdownChartItem("Unprocessed",
        totalFiles - (downloadedFiles + duplicateFiles + filteredFiles + existingFiles), Color.Orange1);
    table.Rows.Clear();
    table
        .AddRow(new BreakdownChart() { Data = { data1, data2, data3, data4, data5, data6 } })
        .AddRow(new Rule());
    foreach (var log in logEntries)
    {
        table.AddRow(log);
    }
    return table;
}

static List<Markup> AddLog(List<Markup> list, Markup markup, bool removeOld = true)
{
    if (removeOld && list.Count >= 15)
        list.Remove(list[0]);
    list.Add(markup);
    return list;
}

static void EnsureDownloadPathExists(string path)
{
    Directory.CreateDirectory(path);
}