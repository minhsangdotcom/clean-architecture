namespace Application.Common.RequestHandler.Query;

public record QueryValidationResult<TResult>(TResult? Result = null, string? Error = null)
    where TResult : class;
