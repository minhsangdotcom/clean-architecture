using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Common.Errors;

public class UnauthorizedError(string title, MessageResult? message = null)
    : ErrorDetails(
        title,
        message
            ?? Messenger
                .Create<User>()
                .CustomProperty("", "ViToBeTranslation=chưa")
                .Negative()
                .Message(
                    new CustomMessage(
                        "authorized",
                        new Dictionary<string, string>()
                        {
                            { "En", "unauthorized" },
                            { "Vi", "đăng nhập" },
                        },
                        "unauthorized"
                    )
                )
                .Build(),
        nameof(UnauthorizedError),
        StatusCodes.Status401Unauthorized
    );
