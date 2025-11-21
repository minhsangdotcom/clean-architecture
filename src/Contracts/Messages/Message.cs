using CaseConverter;

namespace Contracts.Messages;

public class Message
{
    public const string SUCCESS = "Success";

    public const string LOGIN_SUCCESS = $"Login {nameof(SUCCESS)}";

    public const string UNAUTHORIZED = "Unauthorized";

    public const string FORBIDDEN = "Forbidden";

    public const string TOKEN_EXPIRED = "Token expired";
}

public class Message<T>(string? entity = null)
    where T : class
{
    public string Entity { get; internal set; } =
        string.IsNullOrWhiteSpace(entity) ? typeof(T).Name : entity;
    public string Property { get; internal set; } = string.Empty;

    public MessageErrorType ErrorType { get; internal set; } = 0;

    // object that the error targets to
    // ex: shift_end-time_less-than_start-time
    // object is tart-time
    public string? Object { get; set; }

    public string? CustomMessage { get; set; }

    public bool IsNegative { get; internal set; }

    public Dictionary<MessageErrorType, MessageErrorMetadata?> Messages =
        new()
        {
            { MessageErrorType.TooLong, new(MessageErrorType.TooLong.ToString().ToKebabCase()) },
            { MessageErrorType.TooShort, new(MessageErrorType.TooShort.ToString().ToKebabCase()) },
            {
                MessageErrorType.Valid,
                new(MessageErrorType.Valid.ToString().ToKebabCase(), "invalid", "for")
            },
            { MessageErrorType.Found, new(MessageErrorType.Found.ToString().ToKebabCase()) },
            { MessageErrorType.Existent, new(MessageErrorType.Existent.ToString().ToKebabCase()) },
            {
                MessageErrorType.Correct,
                new(MessageErrorType.Correct.ToString().ToKebabCase(), "incorrect")
            },
            {
                MessageErrorType.Active,
                new(MessageErrorType.Active.ToString().ToKebabCase(), "inactive")
            },
            {
                MessageErrorType.AmongTheAllowedOptions,
                new(MessageErrorType.AmongTheAllowedOptions.ToString().ToKebabCase())
            },
            {
                MessageErrorType.GreaterThan,
                new(MessageErrorType.GreaterThan.ToString().ToKebabCase())
            },
            {
                MessageErrorType.GreaterThanEqual,
                new(MessageErrorType.GreaterThanEqual.ToString().ToKebabCase())
            },
            { MessageErrorType.LessThan, new(MessageErrorType.LessThan.ToString().ToKebabCase()) },
            {
                MessageErrorType.LessThanEqual,
                new(MessageErrorType.LessThanEqual.ToString().ToKebabCase())
            },
            { MessageErrorType.Required, new(MessageErrorType.Required.ToString().ToKebabCase()) },
            { MessageErrorType.Unique, new(MessageErrorType.Unique.ToString().ToKebabCase()) },
            {
                MessageErrorType.Strong,
                new(MessageErrorType.Strong.ToString().ToKebabCase(), "weak")
            },
            { MessageErrorType.Expired, new(MessageErrorType.Expired.ToString().ToKebabCase()) },
            {
                MessageErrorType.Redundant,
                new(MessageErrorType.Redundant.ToString().ToKebabCase())
            },
            { MessageErrorType.Missing, new(MessageErrorType.Missing.ToString().ToKebabCase()) },
            {
                MessageErrorType.Identical,
                new(MessageErrorType.Identical.ToString().ToKebabCase(), Preposition: "to")
            },
        };

    public string GetFullMessage()
    {
        if (!string.IsNullOrWhiteSpace(CustomMessage))
        {
            return CustomMessage;
        }
        string subjectProperty = Entity.ToKebabCase();
        if (!string.IsNullOrWhiteSpace(Property))
        {
            subjectProperty += $"_{Property.ToKebabCase()}";
        }

        List<string?> results = [subjectProperty];

        string? error = Messages.GetValueOrDefault(ErrorType)?.Error;
        string? negativeFormError = Messages.GetValueOrDefault(ErrorType)?.NegativeForm;

        string? message =
            IsNegative && !string.IsNullOrWhiteSpace(negativeFormError)
                ? negativeFormError
                : GetMessage(IsNegative, error);

        results.Add(message);

        if (!string.IsNullOrWhiteSpace(Object))
        {
            results.Add(Object.ToKebabCase());
        }

        return string.Join("_", results).ToLower();
    }

    private static string? GetMessage(bool isNegative, string? message)
    {
        if (isNegative == false)
        {
            return message;
        }
        string? mess = string.IsNullOrWhiteSpace(message) ? string.Empty : $"_{message}";
        return "not" + mess;
    }
}
