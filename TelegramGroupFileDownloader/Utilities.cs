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
    }

    //public static PathTestResult TestFilePermissions(string? path)
    //{
    //    if (string.IsNullOrWhiteSpace(path))
    //        return new()
    //        {
    //            IsSuccessful = false,
    //            Reason = "Path was blank.  Unable to test permissions"
    //        };
    //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //    {
    //        var fi = new FileInfo(path);
    //        var isInRoleWithAccess = false;
    //        var currentUser = WindowsIdentity.GetCurrent();
    //        try
    //        {
    //            var acl = fi.GetAccessControl();
    //            var rules = acl.GetAccessRules(true, true, typeof(NTAccount));
    //            var principal = new WindowsPrincipal(currentUser);
    //            foreach (AuthorizationRule rule in rules)
    //            {
    //                if (rule is not FileSystemAccessRule fsAccessRule)
    //                    continue;

    //                if ((fsAccessRule.FileSystemRights & FileSystemRights.Write) <= 0) continue;
    //                var ntAccount = rule.IdentityReference as NTAccount;
    //                if (ntAccount == null)
    //                    continue;

    //                if (!principal.IsInRole(ntAccount.Value)) continue;
    //                if (fsAccessRule.AccessControlType == AccessControlType.Deny)
    //                    return new()
    //                    {
    //                        IsSuccessful = false,
    //                        Reason = $"Permission Denied for {fi.FullName}"
    //                    };
    //                isInRoleWithAccess = true;
    //            }
    //        }
    //        catch (UnauthorizedAccessException e)
    //        {
    //            return new()
    //            {
    //                Exception = e,
    //                Reason = e.Message,
    //                IsSuccessful = false
    //            };
    //        }
    //        catch (FileNotFoundException e)
    //        {
    //            return new()
    //            {
    //                Exception = e,
    //                Reason = $"{fi.FullName} Not Found",
    //                IsSuccessful = false
    //            };
    //        }
    //        return isInRoleWithAccess
    //            ? (new()
    //            {
    //                IsSuccessful = true,
    //                Reason = $"{currentUser.Name} has write access to path {fi.FullName}"
    //            })
    //            : (new()
    //        {
    //            IsSuccessful = false,
    //            Reason = $"{currentUser.Name} does not have permission to {path}"
    //        });
    //    }
    //    else
    //    {

    //        var info = new UnixFileInfo(path);
    //    if (!info.Exists)
    //        return new()
    //        {
    //            IsSuccessful = false,
    //            Reason = $"{path} Does not exist"
    //        };
    //    return !info.IsRegularFile
    //        ? (new()
    //        {
    //            IsSuccessful = false,
    //            Reason = $"{path} is not a file"
    //        })
    //        : CreateUnixTestPathResult(path, info);
    //    }
    //}

    private static PathTestResult CreateUnixTestPathResult(string path, UnixFileSystemInfo info)
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