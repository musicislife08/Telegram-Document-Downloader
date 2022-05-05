using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace TelegramGroupFileDownloader.Config;

public class CliOptions : CommandSettings
{
        [Description("Enable Debug Logging")]
        [CommandOption( "-d|--debug")]
        public bool Debug { get; set; }
        
        [Description("Enable Logging to a file.  Specify filename/path")]
        [CommandOption("-l|--log")]
        public string? LogPath { get; set; }
        
        public override ValidationResult Validate()
        {
                if (string.IsNullOrWhiteSpace(LogPath))
                        return ValidationResult.Error("Must provide path to log file");
                var info = new FileInfo(LogPath);
                if (info.Directory is not null)
                {
                        return info.Directory.Exists
                                ? ValidationResult.Success() 
                                : ValidationResult.Error("Specified log path invalid");
                }
                return LogPath.Length < 2
                        ? ValidationResult.Error("Names must be at least two characters long")
                        : ValidationResult.Success();
        }
}