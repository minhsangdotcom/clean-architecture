using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class UnauthorizedError(string title, LocalizedTextResult message)
    : ErrorDetails(title, message, nameof(UnauthorizedError), StatusCodes.Status401Unauthorized);
