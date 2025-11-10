using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Repositories;
using Contracts.ApiWrapper;
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
                new GetUserByIdIncludeResetPassword(Ulid.Parse(command.UserId)),
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
        UserResetPassword? resetPassword = user.UserResetPasswords?.FirstOrDefault(x =>
            x.Token == updateUserPassword!.Token
        );

        if (resetPassword == null)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with reset password token",
                    Messenger
                        .Create<UserResetPassword>()
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
                        .Create<UserResetPassword>()
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

        user.SetPassword(HashPassword(updateUserPassword!.Password));

        await unitOfWork.Repository<UserResetPassword>().DeleteAsync(resetPassword);
        await unitOfWork.Repository<User>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result<string>.Success();
    }
}
