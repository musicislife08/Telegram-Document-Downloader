using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Mono.Unix;
using Mono.Unix.Native;
using Spectre.Console;
using TelegramGroupFileDownloader.Config;

namespace TelegramGroupFileDownloader;

public static class Utilities
{
    public static void CleanupLogs()
    {
        var files = Directory.GetFiles(Environment.CurrentDirectory, "*.log");
        if (files.Length <= 3)
            return;
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            var now = DateTimeOffset.Now;
            if (info.CreationTimeUtc > now.AddDays(7))
            {
                info.Delete();
            }
        }
    }

    public static void WriteLogToFile(string path, string message)
    {
        using var writer = new StreamWriter(path, true);
        writer.WriteLine(message);
    }

    public static void EnsurePathExists(string path)
    {
        var permissions = Utilities.TestPermissions(path, true);
        if (!permissions.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]Error Accessing download Path: {path}[/]");
            AnsiConsole.MarkupLine($"[red]Exception: {permissions.Reason}[/]");
            Environment.Exit(3);
        }

        Directory.CreateDirectory(path);
    }

    public static void TestApplicationFolderPath()
    {
        var logFilePathAccess = TestPermissions(Environment.CurrentDirectory);
        if (logFilePathAccess.IsSuccessful) return;
        AnsiConsole.MarkupLine($"[red]Unable to access the application folder:[/] {Environment.CurrentDirectory}");
        AnsiConsole.MarkupLine($"[red]{logFilePathAccess.Reason}[/]");
        AnsiConsole.MarkupLine("Please correct the folder permissions to allow the application to access this path");
        Environment.Exit(1);
    }

    public static void TestConfiguredFolderPaths(Configuration config)
    {
        var sessionPathAccess = TestPermissions(config.SessionPath);
        if (!sessionPathAccess.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]Unable to access configured session path:[/] {config.SessionPath}");
            AnsiConsole.MarkupLine($"[red]{sessionPathAccess.Reason}[/]");
            AnsiConsole.MarkupLine(
                "Please correct the folder permissions to allow the application to access this path");
            Environment.Exit(1);
        }

        var downloadPathAccess = TestPermissions(config.DownloadPath);
        if (downloadPathAccess.IsSuccessful) return;
        AnsiConsole.MarkupLine($"[red]Unable to access configured download path:[/] {config.DownloadPath}");
        AnsiConsole.MarkupLine($"[red]{downloadPathAccess.Reason}[/]");
        AnsiConsole.MarkupLine("Please correct the folder permissions to allow the application to access this path");
        Environment.Exit(1);
    }

    public static PathTestResult TestPermissions(string? path, bool testParent = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new()
            {
                IsSuccessful = false,
                Reason = "Path was blank.  Unable to test permissions"
            };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var info = new FileInfo(path);
            var parentExists = info.Directory is not null && info.Directory.Exists;
            if (!parentExists)
                return new()
                {
                    IsSuccessful = false,
                    Reason = $"Cannot find {info.Name}'s parent directory {info.DirectoryName}"
                };
            var isInRoleWithAccess = false;
            var currentUser = WindowsIdentity.GetCurrent();
            try
            {
                var acl = info.GetAccessControl();
                var rules = acl.GetAccessRules(true, true, typeof(NTAccount));
                var principal = new WindowsPrincipal(currentUser);
                foreach (AuthorizationRule rule in rules)
                {
                    if (rule is not FileSystemAccessRule fsAccessRule)
                        continue;

                    if ((fsAccessRule.FileSystemRights & FileSystemRights.Write) <= 0) continue;
                    var ntAccount = rule.IdentityReference as NTAccount;
                    if (ntAccount == null)
                        continue;

                    if (!principal.IsInRole(ntAccount.Value)) continue;
                    if (fsAccessRule.AccessControlType == AccessControlType.Deny)
                        return new()
                        {
                            IsSuccessful = false,
                            Reason = $"Permission Denied for {info.FullName}"
                        };
                    isInRoleWithAccess = true;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                return new()
                {
                    Exception = e,
                    Reason = e.Message,
                    IsSuccessful = false
                };
            }
            catch (FileNotFoundException e)
            {
                var canAccessParent = TestPermissions(info.Directory!.FullName);
                return canAccessParent.IsSuccessful
                    ? (new()
                    {
                        Exception = e,
                        Reason = $"{info.FullName} Not Found",
                        IsSuccessful = true
                    })
                    : (new()
                    {
                        IsSuccessful = false,
                        Exception = canAccessParent.Exception,
                        Reason = $"Cannot Access parent directory of {info.FullName}"
                    });
            }
            catch (DirectoryNotFoundException e)
            {
                var canAccessParent = TestPermissions(info.Directory!.FullName);
                return canAccessParent.IsSuccessful
                    ? (new()
                    {
                        Exception = e,
                        Reason = $"{info.FullName} Not Found",
                        IsSuccessful = true
                    })
                    : (new()
                    {
                        IsSuccessful = false,
                        Exception = canAccessParent.Exception,
                        Reason = $"Cannot Access parent directory of {info.FullName}"
                    });
            }

            return isInRoleWithAccess
                ? (new()
                {
                    IsSuccessful = true,
                    Reason = $"{currentUser.Name} has write access to path {info.FullName}"
                })
                : (new()
                {
                    IsSuccessful = false,
                    Reason = $"{currentUser.Name} does not have permission to {path}"
                });
        }

        try
        {
            var info = new UnixFileInfo(path);
            return CreateUnixTestPathResult(path, info);
        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                Reason = e.Message,
                IsSuccessful = false
            };
        }
    }

    public static string GetFileHash(string filename)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filename);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "");
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private static PathTestResult CreateUnixTestPathResult(string path, UnixFileInfo info)
    {
        if (!info.Exists)
        {
            var canAccessParent = info.Directory.Exists && info.Directory.CanAccess(AccessModes.W_OK);
            if (canAccessParent)
                return new()
                {
                    IsSuccessful = true,
                    Reason = "Can Access Parent"
                };
            return new()
            {
                IsSuccessful = false,
                Reason = $"{path} Does not exist and user does not have access to parent folder"
            };
        }

        var canAccess = info.CanAccess(AccessModes.W_OK);
        if (canAccess)
            return new()
            {
                IsSuccessful = canAccess,
                Reason = "Has Write Permissions"
            };
        return new()
        {
            IsSuccessful = canAccess,
            Reason = $"Does not have write permissions to: {path}"
        };
    }
}