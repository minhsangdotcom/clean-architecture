using System.Linq.Expressions;
using DotNetCoreExtension.Extensions.Expressions;

namespace Application.Contracts.Messages;

public static class Messenger
{
    public static Message<T> Create<T>(string? entity = null)
        where T : class => new(entity);

    public static Message<T> Property<T>(
        this Message<T> message,
        Expression<Func<T, object>> property
    )
        where T : class
    {
        message.Property = property.ToStringProperty();
        return message;
    }

    public static Message<T> Property<T>(this Message<T> message, string property)
        where T : class
    {
        message.Property = property;
        return message;
    }

    public static Message<T> Negative<T>(this Message<T> message)
        where T : class
    {
        message.IsNegative = true;
        return message;
    }

    public static Message<T> ToObject<T>(this Message<T> message, string name)
        where T : class
    {
        message.Object = name;
        return message;
    }

    public static Message<T> Message<T>(this Message<T> message, string customMessage)
        where T : class
    {
        message.CustomErrorMessage = customMessage;
        return message;
    }

    public static Message<T> WithError<T>(this Message<T> message, MessageErrorType errorType)
        where T : class
    {
        message.ErrorType = errorType;
        return message;
    }

    public static string GetMessage<T>(this Message<T> message)
        where T : class
    {
        return message.GetFullMessage();
    }
}
