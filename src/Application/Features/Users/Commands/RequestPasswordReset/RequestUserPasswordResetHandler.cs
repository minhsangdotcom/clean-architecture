using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetHandler(
    IEfUnitOfWork unitOfWork,
    IPublisher publisher,
    IConfiguration configuration,
    IMessageTranslatorService translator
) : IRequestHandler<RequestUserPasswordResetCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        RequestUserPasswordResetCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByEmailIncludePasswordResetRequestSpecification(command.Email),
                cancellationToken
            );

        if (user == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }

        if (user.Status == UserStatus.Inactive)
        {
            string errorMessage = Messenger
                .Create<User>()
                .WithError(MessageErrorType.Active)
                .Negative()
                .GetFullMessage();
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with the current user",
                    new(
                        UserErrorMessages.UserInactive,
                        translator.Translate(UserErrorMessages.UserInactive)
                    )
                )
            );
        }

        string token = StringExtension.GenerateRandomString(40);
        DateTimeOffset expiredTime = DateTimeOffset.UtcNow.AddHours(
            configuration.GetValue<int>("ForgotPasswordExpiredTimeInHour")
        );

        await unitOfWork
            .Repository<UserPasswordReset>()
            .DeleteRangeAsync(user.PasswordResetRequests);
        await unitOfWork
            .Repository<UserPasswordReset>()
            .AddAsync(
                new()
                {
                    Token = token,
                    UserId = user.Id,
                    Expiry = expiredTime,
                },
                cancellationToken
            );
        await unitOfWork.SaveAsync(cancellationToken);

        string forgotPasswordUrl = configuration.GetValue<string>("ForgotPasswordUrl")!;
        RequestUserPasswordResetNotification notification =
            new(user.Email, token, user.Id, expiredTime, forgotPasswordUrl);

        await publisher.Publish(notification, cancellationToken);
        return Result<string>.Success();
    }
}
