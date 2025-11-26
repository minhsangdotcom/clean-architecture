using System.Collections.Concurrent;

namespace Application.Contracts.ErrorCodes;

public class ErrorMessageRegistry
{
    private static readonly Dictionary<string, string> messages = [];
    public static Dictionary<string, string> Messages => messages;

    public static void Register(string key, string message)
    {
        messages[key] = message;
    }

    public static string GetMessage(string key)
    {
        return messages.TryGetValue(key, out string? message) ? message : key;
    }
}
