using System.Reflection;
using Application.Common.ErrorCodes;
using Application.Contracts.Dtos.Requests;
using DotNetCoreExtension.Extensions;
using DotNetCoreExtension.Extensions.Reflections;
using DotNetCoreExtension.Results;
using Microsoft.Extensions.Logging;

namespace Application.Common.QueryStringProcessing;

public static partial class QueryParamValidator
{
    public static ValidationRequestResult<T> ValidateQuery<T>(this T request)
        where T : QueryParamRequest
    {
        if (!string.IsNullOrWhiteSpace(request.Before) && !string.IsNullOrWhiteSpace(request.After))
        {
            return new(Error: QueryParamRequestErrorMessages.QueryParamBeforeAfterRedundant);
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
            if (string.IsNullOrWhiteSpace(query.Value))
            {
                return new(Error: QueryParamRequestErrorMessages.QueryParamFilterValueIsRequired);
            }

            // Array operator $in, $between and Logical operator $and, $or must have index right after them
            // ex: [status][$in][0],[status][$between][0],[$and][0],[$or][0]
            // or [status][$in][1],[status][$between][1],[$and][1],[$or][1]
            if (!ValidateArrayAndLogicalOperator(query.CleanKey))
            {
                return new(Error: QueryParamRequestErrorMessages.QueryParamFilterArrayIndexMissing);
            }

            // Array operator $in, $between and Logical operator $and, $or, their index must go with 0
            // ex: [status][$in][0],[status][$between][0],[$and][0],[$or][0]
            if (i == 0 && !ValidateArrayAndLogicalOperatorInvalidIndex(query.CleanKey))
            {
                return new(Error: QueryParamRequestErrorMessages.QueryParamFilterArrayIndexInvalid);
            }

            // lack of operator
            // ex: [name]=john,[name][0] = join
            if (!ValidateLackOfOperator(query.CleanKey))
            {
                return new(Error: QueryParamRequestErrorMessages.QueryParamFilterOperatorMissing);
            }

            // lack of property
            List<string> properties =
            [
                .. query.CleanKey.Where(x =>
                    string.Compare(x, "$or", StringComparison.OrdinalIgnoreCase) != 0
                    && string.Compare(x, "$and", StringComparison.OrdinalIgnoreCase) != 0
                    && !x.IsDigit()
                    && !validOperators.Contains(x.ToLower())
                ),
            ];
            if (properties.Count == 0)
            {
                return new(Error: QueryParamRequestErrorMessages.QueryParamFilterPropertyMissing);
            }

            Type type = typeof(TResponse);
            PropertyInfo propertyInfo = type.GetNestedPropertyInfo(string.Join(".", properties));
            Type[] arguments = propertyInfo.PropertyType.GetGenericArguments();
            Type nullableType = arguments.Length > 0 ? arguments[0] : propertyInfo.PropertyType;

            // value must be enum or number
            if (
                (nullableType.IsEnum || IsNumericType(nullableType))
                && query.Value?.IsDigit() == false
            )
            {
                return new(
                    Error: QueryParamRequestErrorMessages.QueryParamFilterValueIntegerInvalid
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
                    Error: QueryParamRequestErrorMessages.QueryParamFilterValueDatetimeInvalid
                );
            }

            // value must be Ulid
            if ((nullableType == typeof(Ulid)) && !Ulid.TryParse(query.Value, out _))
            {
                return new(Error: QueryParamRequestErrorMessages.QueryParamFilterValueUlidInvalid);
            }
        }

        // validate between operator is correct format like:
        // [age][$between][0] = 1 && [age][$between][1] = 2
        if (!ValidateBetweenOperator("$between", queries))
        {
            return new(Error: QueryParamRequestErrorMessages.QueryParamBetweenOperatorInvalid);
        }

        // duplicated element of filter
        List<string> trimQueries = queries.ConvertAll(x => string.Join(".", x.CleanKey));
        if (trimQueries.Distinct().Count() != queries.Count)
        {
            return new(Error: QueryParamRequestErrorMessages.QueryParamFilterElementDuplicate);
        }

        request.Filter = StringExtension.Parse(queries);

        string filter = SerializerExtension.Serialize(request.Filter!).StringJson;
        logger.LogInformation("Filter has been bound {filter}", filter);

        return new(request);
    }

    private static bool ValidateBetweenOperator(string operation, List<QueryResult> queries)
    {
        List<QueryResult> betweenOperators = queries.FindAll(x => x.CleanKey.Contains(operation));

        var betweenOperatorsGroup = betweenOperators
            .ConvertAll(betweenOperator =>
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
    private static bool IsNumericType(Type type) =>
        Type.GetTypeCode(type) switch
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

    private static bool ValidateArrayAndLogicalOperator(List<string> input)
    {
        HashSet<string> operators = [.. arrayOperators, .. logicalOperators];

        for (int i = 0; i < input.Count; i++)
        {
            if (!operators.Contains(input[i]))
            {
                continue;
            }

            if (i + 1 >= input.Count)
            {
                return false;
            }

            if (!input[i + 1].IsDigit())
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateArrayAndLogicalOperatorInvalidIndex(List<string> input)
    {
        HashSet<string> operators = [.. arrayOperators, .. logicalOperators];

        for (int i = 0; i < input.Count; i++)
        {
            if (!operators.Contains(input[i]))
            {
                continue;
            }

            if (i + 1 >= input.Count)
            {
                return false;
            }

            string indexItem = input[i + 1];
            if (!indexItem.IsDigit() || long.Parse(indexItem) != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateLackOfOperator(List<string> input)
    {
        if (input.Count == 0 || input.Count == 1)
        {
            return false;
        }
        Stack<string> stackInput = new(input);

        string last = stackInput.Pop();
        if (validOperators.Contains(last.ToLower()))
        {
            return true;
        }
        if (last.IsDigit() && arrayOperators.Contains(stackInput.Pop().ToLower()))
        {
            return true;
        }
        return false;
    }

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

    private static readonly string[] arrayOperators = ["$in", "$between"];
    private static readonly string[] logicalOperators = ["$and", "$or"];
}
