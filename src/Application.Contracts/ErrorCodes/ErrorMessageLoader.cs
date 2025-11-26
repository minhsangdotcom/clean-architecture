using System.Reflection;

namespace Application.Contracts.ErrorCodes;

public class ErrorMessageLoader
{
    public static void LoadFromType(Type type)
    {
        foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            var attribute = prop.GetCustomAttribute<ErrorKeyAttribute>();
            if (attribute == null)
            {
                continue;
            }

            string key = attribute.Key;
            string message = (string)prop.GetValue(null)!;

            ErrorMessageRegistry.Register(key, message);
        }
    }
}
