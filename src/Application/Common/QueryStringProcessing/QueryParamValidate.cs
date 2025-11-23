using System.Reflection;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Messages;
using DotNetCoreExtension.Extensions;
using DotNetCoreExtension.Extensions.Reflections;
using DotNetCoreExtension.Results;
using Microsoft.Extensions.Logging;

namespace Application.Common.QueryStringProcessing;

public static partial class QueryParamValidate
{
    public static ValidationRequestResult<T> ValidateQuery<T>(this T request)
        where T : QueryParamRequest
    {
        if (!string.IsNullOrWhiteSpace(request.Before) && !string.IsNullOrWhiteSpace(request.After))
        {
            return new(
                Error: Messenger
                    .Create<QueryParamRequest>("QueryParam")
                    .Property("Cursor")
                    .WithError(MessageErrorType.Redundant)
                    .GetFullMessage()
            );
        }

        return new(request);
    }

    public static ValidationRequestResult<TRequest> ValidateFilter<TRequest, TResponse>(
        this TRequest request,
        ILogger logger
    )
        where TResponse : class
        where TRequest : QueryParamRequest
    {
        if (request.OriginFilters?.Length <= 0)
        {
            return new(request);
        }

        List<QueryResult> queries =
        [
            .. StringExtension.TransformStringQuery(
                request.OriginFilters!,
                nameof(QueryParamRequest.Filter)
            ),
        ];

        int length = queries.Count;

        for (int i = 0; i < length; i++)
        {
            QueryResult query = queries[i];

            //if it's $and,$or,$in and $between then they must have a index after like $or[0],$[in][1]
            if (!ValidateArrayOperator(query.CleanKey))
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .WithError(MessageErrorType.Missing)
                        .ToObject("ArrayIndex")
                        .GetFullMessage()
                );
            }

            /// check if the index of array operator has to start with 0 like $and[0][firstName]
            if (i == 0 && !ValidateArrayOperatorInvalidIndex(query.CleanKey))
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .WithError(MessageErrorType.Valid)
                        .Negative()
                        .ToObject("ArrayIndex")
                        .GetFullMessage()
                );
            }

            // lack of operator
            if (!ValidateLackOfOperator(query.CleanKey))
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .WithError(MessageErrorType.Missing)
                        .ToObject("Operator")
                        .GetFullMessage()
                );
            }

            // if the last element is logical operator ($and, $or) it's wrong like filter[$and][0] which is lack of body
            if (LackOfElementInArrayOperator(query.CleanKey))
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .WithError(MessageErrorType.Missing)
                        .ToObject("Element")
                        .GetFullMessage()
                );
            }

            IEnumerable<string> properties = query.CleanKey.Where(x =>
                string.Compare(x, "$or", StringComparison.OrdinalIgnoreCase) != 0
                && string.Compare(x, "$and", StringComparison.OrdinalIgnoreCase) != 0
                && !x.IsDigit()
                && !validOperators.Contains(x.ToLower())
            );

            // lack of property
            if (!properties.Any())
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .WithError(MessageErrorType.Missing)
                        .ToObject("Property")
                        .GetFullMessage()
                );
            }
            Type type = typeof(TResponse);
            PropertyInfo propertyInfo = type.GetNestedPropertyInfo(string.Join(".", properties));
            Type[] arguments = propertyInfo.PropertyType.GetGenericArguments();
            Type nullableType = arguments.Length > 0 ? arguments[0] : propertyInfo.PropertyType;

            // value must be enum
            if (
                (nullableType.IsEnum || IsNumericType(nullableType))
                && query.Value?.IsDigit() == false
            )
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property("FilterValue")
                        .Negative()
                        .ToObject("Integer")
                        .GetFullMessage()
                );
            }

            // value must be datetime
            if (
                (nullableType == typeof(DateTime) && !DateTime.TryParse(query.Value, out _))
                || (
                    nullableType == typeof(DateTimeOffset)
                    && !DateTimeOffset.TryParse(query.Value, out _)
                )
            )
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property("FilterValue")
                        .Negative()
                        .ToObject("Datetime")
                        .GetFullMessage()
                );
            }

            // value must be Ulid
            if ((nullableType == typeof(Ulid)) && !Ulid.TryParse(query.Value, out _))
            {
                return new(
                    Error: Messenger
                        .Create<QueryParamRequest>("QueryParam")
                        .Property("FilterValue")
                        .Negative()
                        .ToObject("Ulid")
                        .GetFullMessage()
                );
            }
        }

        // validate between operator is correct in format like [age][$between][0] = 1 & [age][$between][1] = 2
        if (!ValidateBetweenOperator("$between", queries))
        {
            return new(
                Error: Messenger
                    .Create<QueryParamRequest>("QueryParam")
                    .Property(x => x.Filter!)
                    .WithError(MessageErrorType.Valid)
                    .ToObject("BetweenOperator")
                    .Negative()
                    .GetFullMessage()
            );
        }

        // duplicated element of filter
        var trimQueries = queries.Select(x => string.Join(".", x.CleanKey));
        if (trimQueries.Distinct().Count() != queries.Count)
        {
            return new(
                Error: Messenger
                    .Create<QueryParamRequest>("QueryParam")
                    .Property("FilterElement")
                    .WithError(MessageErrorType.Unique)
                    .Negative()
                    .GetFullMessage()
            );
        }

        request.Filter = StringExtension.Parse(queries);

        string filter = SerializerExtension.Serialize(request.Filter!).StringJson;
        logger.LogInformation("Filter has been bound {filter}", filter);

        return new(request);
    }

    private static bool ValidateBetweenOperator(string operation, IEnumerable<QueryResult> queries)
    {
        IEnumerable<QueryResult> betweenOperators = queries.Where(x =>
            x.CleanKey.Contains(operation)
        );

        var betweenOperatorsGroup = betweenOperators
            .Select(betweenOperator =>
            {
                int betweenIndex = betweenOperator.CleanKey.IndexOf(operation);
                int index = betweenIndex - 1;

                if (index < 0)
                {
                    throw new InvalidOperationException("Invalid format of cleanKey.");
                }

                string key = string.Join(
                    ".",
                    betweenOperator
                        .CleanKey.Skip(index)
                        .Take(betweenOperator.CleanKey.Count - betweenIndex)
                );

                _ = int.TryParse(betweenOperator.CleanKey.Last(), out int indexValue);

                if (
                    int.TryParse(
                        betweenOperator.CleanKey[betweenOperator.CleanKey.IndexOf("$and") + 1],
                        out int andIndex
                    )
                )
                {
                    return new { key = $"$and.{andIndex}.{key}", indexValue };
                }

                if (
                    int.TryParse(
                        betweenOperator.CleanKey[betweenOperator.CleanKey.IndexOf("$or") + 1],
                        out int orIndex
                    )
                )
                {
                    return new { key = $"$or.{orIndex}.{key}", indexValue };
                }

                return new { key = $"{key}", indexValue };
            })
            .GroupBy(x => x.key)
            .Select(x => new { x.Key, values = x.Select(x => x.indexValue).ToList() })
            .ToList();

        foreach (var betweenOperator in betweenOperatorsGroup)
        {
            if (!betweenOperator.values.SequenceEqual([0, 1]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// just for enum
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static bool IsNumericType(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Byte
            or TypeCode.SByte
            or TypeCode.UInt16
            or TypeCode.UInt32
            or TypeCode.UInt64
            or TypeCode.Int16
            or TypeCode.Int32
            or TypeCode.Int64
            or TypeCode.Decimal
            or TypeCode.Double
            or TypeCode.Single => true,
            _ => false,
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="input"></param>
    /// <returns>true if any element is after $and,$or,$in,$between isn't degit otherwise false</returns>
    private static bool ValidateArrayOperator(List<string> input)
    {
        var validOperators = new HashSet<string> { "$and", "$or", "$in", "$between" };

        for (int i = 0; i < input.Count; i++)
        {
            if (!validOperators.Contains(input[i]))
            {
                continue;
            }

            if (i + 1 >= input.Count)
            {
                continue;
            }

            if (!input[i + 1].IsDigit())
            {
                return false;
            }
        }

        if (input[^1] == validOperators.Last() || input[^1] == "$in")
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// check if array operator has invalid index like $and[1][firstName], index must start with 0.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static bool ValidateArrayOperatorInvalidIndex(List<string> input)
    {
        var validOperators = new HashSet<string> { "$and", "$or", "$in", "$between" };

        for (int i = 0; i < input.Count; i++)
        {
            if (!validOperators.Contains(input[i]))
            {
                continue;
            }

            if (i + 1 >= input.Count)
            {
                continue;
            }

            string theNextItem = input[i + 1];
            if (!theNextItem.IsDigit() || int.Parse(theNextItem) != 0)
            {
                return false;
            }
        }

        if (input[^1] == validOperators.Last() || input[^1] == "$in")
        {
            return false;
        }

        return true;
    }

    private static bool ValidateLackOfOperator(List<string> input)
    {
        Stack<string> inputs = new(input);

        string last = inputs.Pop();
        string preLast = inputs.Pop();

        if (arrayOperators.Contains(preLast.ToLower()))
        {
            return true;
        }

        return validOperators.Contains(last.ToLower());
    }

    private static bool LackOfElementInArrayOperator(List<string> input)
    {
        Stack<string> inputs = new(input);
        string last = inputs.Pop();
        string preLast = inputs.Pop();

        return logicalOperators.Contains(preLast.ToLower()) && last.IsDigit();
    }

    // Array of valid operators
    private static readonly string[] validOperators =
    [
        "$eq",
        "$eqi",
        "$ne",
        "$nei",
        "$in",
        "$notin",
        "$lt",
        "$lte",
        "$gt",
        "$gte",
        "$between",
        "$notcontains",
        "$notcontainsi",
        "$contains",
        "$containsi",
        "$startswith",
        "$endswith",
    ];

    // Operators that don't require further validation after them
    private static readonly string[] arrayOperators = ["$in", "$between"];

    private static readonly string[] logicalOperators = ["$and", "$or"];
}
