using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class BadRequestError(string title, LocalizedTextResult message)
    : ErrorDetails(title, message, nameof(BadRequestError), StatusCodes.Status400BadRequest);
