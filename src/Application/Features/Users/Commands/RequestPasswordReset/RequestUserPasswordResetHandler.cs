using Application.Common.Errors;
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
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetHandler(
    IEfUnitOfWork unitOfWork,
    IPublisher publisher,
    IConfiguration configuration,
    IStringLocalizer<RequestUserPasswordResetHandler> stringLocalizer
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
            string errorMessage = Messenger
                .Create<User>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(errorMessage, stringLocalizer[errorMessage])
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
                    new(errorMessage, stringLocalizer[errorMessage])
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
