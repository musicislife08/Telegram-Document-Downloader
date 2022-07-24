namespace TelegramTools.Downloader.Exceptions;

public class TelegramAccountException : Exception
{
    public TelegramAccountException()
        : base("Invalid Account.  Please signup for an account before using this tool")
    {
    } 
}