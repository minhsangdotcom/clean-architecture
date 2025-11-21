namespace Application.Contracts.ApiWrapper;

public sealed class Result<TResult>
    where TResult : class
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TResult? Value { get; }

    public ErrorDetails? Error { get; set; }

    private Result(bool isSuccess, TResult? value, ErrorDetails? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TResult> Success()
    {
        return new Result<TResult>(true, null, null);
    }

    public static Result<TResult> Success(TResult value)
    {
        return new Result<TResult>(true, value, null);
    }

    public static Result<TResult> Failure<TError>(TError error)
        where TError : ErrorDetails
    {
        return new Result<TResult>(false, default, error);
    }
}

public static class ResultExtension
{
    public static TReturn Match<TResult, TReturn>(
        this Result<TResult> result,
        Func<TResult, TReturn> onSuccess,
        Func<ErrorDetails, TReturn> onFailure
    )
        where TResult : class
    {
        return result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Error!);
    }
}

public static class ResultTypeHelper
{
    public static object? GetResultValue<TResult>(this Result<TResult> result)
        where TResult : class
    {
        var propertyInfo = typeof(Result<TResult>).GetProperty(nameof(Result<TResult>.Value));
        return propertyInfo?.GetValue(result);
    }

    public static TResult? GetTypedResultValue<TResult>(this Result<TResult> result)
        where TResult : class
    {
        var value = GetResultValue(result);
        return value != null ? (TResult)value : default;
    }

    public static object? ExtractValue(object? result)
    {
        if (result == null)
        {
            return null;
        }

        var resultType = result.GetType();
        if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new ArgumentException("Object must be Result<T>", nameof(result));
        }

        var valueProperty = resultType.GetProperty(nameof(Result<object>.Value));
        return valueProperty?.GetValue(result);
    }
}
