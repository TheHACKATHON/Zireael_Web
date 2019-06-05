namespace Web.Models
{
    //.public enum NotifyType { Error = 0, Success, Notice, Warning };
}

public static class NotifyType
{
    public static string Error { get; } = "Error";
    public static string Success { get; } = "Success";
    public static string Notice { get; } = "Notice";
    public static string Warning { get; } = "Warning";
}