using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Commands.ResetPassword;

public class ResetUserPasswordHandler(
    IEfUnitOfWork unitOfWork,
    IMessageTranslatorService translator
) : IRequestHandler<ResetUserPasswordCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .ReadonlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByEmailIncludePasswordResetRequestSpecification(command.Email!),
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

        UserPasswordReset? resetPassword = user.PasswordResetRequests?.FirstOrDefault(x =>
            x.Token == command.Token
        );

        if (resetPassword == null)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with reset password token",
                    new(
                        UserErrorMessages.UserResetPasswordTokenInvalid,
                        translator.Translate(UserErrorMessages.UserResetPasswordTokenInvalid)
                    )
                )
            );
        }

        if (resetPassword.Expiry <= DateTimeOffset.UtcNow)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with reset password token",
                    new(
                        UserErrorMessages.UserPasswordResetTokenExpired,
                        translator.Translate(UserErrorMessages.UserPasswordResetTokenExpired)
                    )
                )
            );
        }

        if (user.Status == UserStatus.Inactive)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with current user",
                    new(
                        UserErrorMessages.UserInactive,
                        translator.Translate(UserErrorMessages.UserInactive)
                    )
                )
            );
        }

        user.ChangePassword(HashPassword(command.Password));

        await unitOfWork.Repository<UserPasswordReset>().DeleteAsync(resetPassword);
        await unitOfWork.Repository<User>().UpdateAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success();
    }
}
