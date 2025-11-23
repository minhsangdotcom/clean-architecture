using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class ForbiddenError(string title, LocalizedTextResult message)
    : ErrorDetails(title, message, nameof(ForbiddenError), StatusCodes.Status403Forbidden);
