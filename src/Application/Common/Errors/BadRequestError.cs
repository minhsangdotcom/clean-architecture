using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Common.Errors;

public class BadRequestError(string title, MessageResult messageResult)
    : ErrorDetails(title, messageResult, nameof(BadRequestError), StatusCodes.Status400BadRequest);
