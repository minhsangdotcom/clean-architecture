using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.ResetPassword;

public class ResetUserPasswordHandler(
    IEfUnitOfWork unitOfWork,
    IStringLocalizer<ResetUserPasswordHandler> stringLocalizer
) : IRequestHandler<ResetUserPasswordCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdIncludePasswordResetRequestSpecification(Ulid.Parse(command.UserId)),
                cancellationToken
            );

        if (user == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        stringLocalizer[UserErrorMessages.UserNotFound]
                    )
                )
            );
        }

        UpdateUserPassword? updateUserPassword = command.UpdateUserPassword;
        UserPasswordReset? resetPassword = user.PasswordResetRequests?.FirstOrDefault(x =>
            x.Token == updateUserPassword!.Token
        );

        if (resetPassword == null)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with reset password token",
                    new(
                        UserErrorMessages.UserResetPasswordTokenInvalid,
                        stringLocalizer[UserErrorMessages.UserResetPasswordTokenInvalid]
                    )
                )
            );
        }

        if (resetPassword.Expiry <= DateTimeOffset.UtcNow)
        {
            string errorMessage = Messenger
                .Create<UserPasswordReset>()
                .Property(x => x.Token)
                .WithError(MessageErrorType.Expired)
                .GetFullMessage();
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with reset password token",
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
                    "Error has occurred with current user",
                    new(errorMessage, stringLocalizer[errorMessage])
                )
            );
        }

        user.ChangePassword(HashPassword(updateUserPassword!.Password));

        await unitOfWork.Repository<UserPasswordReset>().DeleteAsync(resetPassword);
        await unitOfWork.Repository<User>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result<string>.Success();
    }
}
