using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.ResetPassword;

public class ResetUserPasswordHandler(IEfUnitOfWork unitOfWork)
    : IRequestHandler<ResetUserPasswordCommand, Result<string>>
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
                    "The TitleMessage.RESOURCE_NOT_FOUND",
                    Messenger
                        .Create<User>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_USER_NOT_FOUND)
                        .Build()
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
                    Messenger
                        .Create<UserPasswordReset>()
                        .Property(x => x.Token)
                        .Message(MessageType.Correct)
                        .Negative()
                        .Build()
                )
            );
        }

        if (resetPassword.Expiry <= DateTimeOffset.UtcNow)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with reset password token",
                    Messenger
                        .Create<UserPasswordReset>()
                        .Property(x => x.Token)
                        .Message(MessageType.Expired)
                        .Build()
                )
            );
        }

        if (user.Status == UserStatus.Inactive)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with current user",
                    Messenger.Create<User>().Message(MessageType.Active).Negative().Build()
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
