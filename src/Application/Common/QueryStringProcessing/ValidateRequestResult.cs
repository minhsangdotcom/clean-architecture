namespace Application.Common.QueryStringProcessing;

public record ValidationRequestResult<TResult>(TResult? Result = null, string? Error = null)
    where TResult : class;
