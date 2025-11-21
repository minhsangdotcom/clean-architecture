using Application.Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Common.Errors;

public class NotFoundError(string title, MessageResult message)
    : ErrorDetails(title, message, nameof(NotFoundError), StatusCodes.Status404NotFound);
