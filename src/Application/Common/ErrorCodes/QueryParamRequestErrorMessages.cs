using Application.Contracts.Dtos.Requests;
using Application.Contracts.ErrorCodes;
using Application.Contracts.Messages;

namespace Application.Common.ErrorCodes;

[ErrorMessageContainer]
public class QueryParamRequestErrorMessages
{
    // ---------------------------------------------------------
    // Before and After cannot both be provided
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamBeforeAfterRedundant))]
    public static string QueryParamBeforeAfterRedundant =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property("Cursor")
            .WithError(MessageErrorType.Redundant)
            .GetFullMessage();

    // ---------------------------------------------------------
    // Filter array operator missing index e.g. $or[]
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterArrayIndexMissing))]
    public static string QueryParamFilterArrayIndexMissing =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property(x => x.Filter!)
            .WithError(MessageErrorType.Missing)
            .ToObject("ArrayIndex")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Array operator must start at index 0
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterArrayIndexInvalid))]
    public static string QueryParamFilterArrayIndexInvalid =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property(x => x.Filter!)
            .WithError(MessageErrorType.Valid)
            .Negative()
            .ToObject("ArrayIndex")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Missing operator
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterOperatorMissing))]
    public static string QueryParamFilterOperatorMissing =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property(x => x.Filter!)
            .WithError(MessageErrorType.Missing)
            .ToObject("Operator")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Missing element inside logical operator
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterElementMissing))]
    public static string QueryParamFilterElementMissing =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property(x => x.Filter!)
            .WithError(MessageErrorType.Missing)
            .ToObject("Element")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Missing property name in filter
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterPropertyMissing))]
    public static string QueryParamFilterPropertyMissing =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property(x => x.Filter!)
            .WithError(MessageErrorType.Missing)
            .ToObject("Property")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Filter value must be integer / enum
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterValueIntegerInvalid))]
    public static string QueryParamFilterValueIntegerInvalid =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property("FilterValue")
            .Negative()
            .ToObject("Integer")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Filter value must be datetime
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterValueDatetimeInvalid))]
    public static string QueryParamFilterValueDatetimeInvalid =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property("FilterValue")
            .Negative()
            .ToObject("Datetime")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Filter value must be ULID
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterValueUlidInvalid))]
    public static string QueryParamFilterValueUlidInvalid =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property("FilterValue")
            .Negative()
            .ToObject("Ulid")
            .GetFullMessage();

    // ---------------------------------------------------------
    // Between operator format invalid
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamBetweenOperatorInvalid))]
    public static string QueryParamBetweenOperatorInvalid =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property(x => x.Filter!)
            .WithError(MessageErrorType.Valid)
            .ToObject("BetweenOperator")
            .Negative()
            .GetFullMessage();

    // ---------------------------------------------------------
    // Duplicated filter elements
    // ---------------------------------------------------------
    [ErrorKey(nameof(QueryParamFilterElementDuplicate))]
    public static string QueryParamFilterElementDuplicate =>
        Messenger
            .Create<QueryParamRequest>("QueryParam")
            .Property("FilterElement")
            .WithError(MessageErrorType.Unique)
            .Negative()
            .GetFullMessage();
}
