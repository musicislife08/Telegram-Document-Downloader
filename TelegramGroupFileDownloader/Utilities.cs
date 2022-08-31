using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Mono.Unix;
using Mono.Unix.Native;
using Spectre.Console;
using TelegramGroupFileDownloader.Config;

namespace TelegramGroupFileDownloader;

public static class Utilities
{
    public static void TestApplicationFolderPath()
    {
        var logFilePathAccess = TestFolderPermissions(Environment.CurrentDirectory);
        if (logFilePathAccess.IsSuccessful) return;
        AnsiConsole.MarkupLine($"[red]Unable to access the application folder:[/] {Environment.CurrentDirectory}");
        AnsiConsole.MarkupLine($"[red]{logFilePathAccess.Reason}[/]");
        AnsiConsole.MarkupLine("Please correct the folder permissions to allow the application to access this path");
        Environment.Exit(1);
    }

    public static void TestConfiguredFolderPaths(Configuration config)
    {
        var sessionPathAccess = TestFolderPermissions(config.SessionPath);
        if (!sessionPathAccess.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]Unable to access configured session path:[/] {config.SessionPath}");
            AnsiConsole.MarkupLine($"[red]{sessionPathAccess.Reason}[/]");
            AnsiConsole.MarkupLine(
                "Please correct the folder permissions to allow the application to access this path");
            Environment.Exit(1);
        }

        var downloadPathAccess = TestFolderPermissions(config.DownloadPath);
        if (downloadPathAccess.IsSuccessful) return;
        AnsiConsole.MarkupLine($"[red]Unable to access configured download path:[/] {config.DownloadPath}");
        AnsiConsole.MarkupLine($"[red]{downloadPathAccess.Reason}[/]");
        AnsiConsole.MarkupLine("Please correct the folder permissions to allow the application to access this path");
        Environment.Exit(1);
    }

    public static PathTestResult TestFolderPermissions(string? path, bool testParent = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new()
            {
                IsSuccessful = false,
                Reason = "Path was blank.  Unable to test permissions"
            };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var di = new DirectoryInfo(path);
            var isInRoleWithAccess = false;
            var currentUser = WindowsIdentity.GetCurrent();
            try
            {
                var acl = di.GetAccessControl();
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
                            Reason = $"Permission Denied for {di.FullName}"
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
            if (isInRoleWithAccess)
                return new()
                {
                    IsSuccessful = false,
                    Reason = $"Unknown error getting file permissions for path {di.FullName}"
                };
            return new()
            {
                IsSuccessful = false,
                Reason = $"{currentUser.Name} does not have permission to {path}"
            };
        }
        else
        {
            try
            {
                var info = new UnixDirectoryInfo(path);
                if (!info.Exists)
                    return new()
                    {
                        IsSuccessful = false,
                        Reason = $"{path} Does not exist"
                    };
                if (!info.IsDirectory)
                    return new()
                    {
                        IsSuccessful = false,
                        Reason = $"{path} is not a directory"
                    };
                return CreatePathTestResult(path, info);
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
    }

    public static PathTestResult TestFilePermissions(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new()
            {
                IsSuccessful = false,
                Reason = "Path was blank.  Unable to test permissions"
            };
        var info = new UnixFileInfo(path);
        if (!info.Exists)
            return new()
            {
                IsSuccessful = false,
                Reason = $"{path} Does not exist"
            };
        return !info.IsRegularFile
            ? (new()
            {
                IsSuccessful = false,
                Reason = $"{path} is not a file"
            })
            : CreatePathTestResult(path, info);
    }

    private static PathTestResult CreatePathTestResult(string path, UnixFileSystemInfo info)
    {
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
        // return info.FileAccessPermissions switch
        // {
        //     FileAccessPermissions.UserReadWriteExecute => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has Read Write Execute Permissions: {path}"
        //     },
        //     FileAccessPermissions.UserRead => new() { IsSuccessful = false, Reason = $"Has Read Permissions: {path}" },
        //     FileAccessPermissions.UserWrite => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has Read Write Permissions: {path}"
        //     },
        //     FileAccessPermissions.UserExecute => new()
        //     {
        //         IsSuccessful = false, Reason = $"Has Execute Permissions: {path}"
        //     },
        //     FileAccessPermissions.GroupReadWriteExecute => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has Group Read Write Execute Permissions: {path}"
        //     },
        //     FileAccessPermissions.GroupRead => new()
        //     {
        //         IsSuccessful = false, Reason = $"Has Group Read Permissions: {path}"
        //     },
        //     FileAccessPermissions.GroupWrite => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has Group Write Permissions: {path}"
        //     },
        //     FileAccessPermissions.GroupExecute => new()
        //     {
        //         IsSuccessful = false, Reason = $"Has Group Execute Permissions: {path}"
        //     },
        //     FileAccessPermissions.OtherReadWriteExecute => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has Other Read Write Execute Permissions: {path}"
        //     },
        //     FileAccessPermissions.OtherRead => new()
        //     {
        //         IsSuccessful = false, Reason = $"Has Other Read Permissions: {path}"
        //     },
        //     FileAccessPermissions.OtherWrite => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has Other Write Permissions: {path}"
        //     },
        //     FileAccessPermissions.OtherExecute => new()
        //     {
        //         IsSuccessful = false, Reason = $"Has Other Execute Permissions: {path}"
        //     },
        //     FileAccessPermissions.DefaultPermissions => new()
        //     {
        //         IsSuccessful = false, Reason = $"Has Default Permissions: {path}"
        //     },
        //     FileAccessPermissions.AllPermissions => new()
        //     {
        //         IsSuccessful = true, Reason = $"Has All Permissions: {path}"
        //     },
        //     _ => new() { IsSuccessful = false, Reason = $"Unable To Determine Permissions: {path}" }
        // };
    }
}