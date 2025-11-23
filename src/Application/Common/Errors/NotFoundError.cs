using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class NotFoundError(string title, LocalizedTextResult message)
    : ErrorDetails(title, message, nameof(NotFoundError), StatusCodes.Status404NotFound);
