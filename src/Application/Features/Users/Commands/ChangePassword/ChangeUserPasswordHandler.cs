using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordHandler(IUserManager userManager, ICurrentUser currentUser)
    : IRequestHandler<ChangeUserPasswordCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        ChangeUserPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        User? user = await userManager.FindByIdAsync(
            currentUser.Id!.Value,
            false,
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

        if (!Verify(request.OldPassword, user.Password))
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occured with password",
                    Messenger
                        .Create<ChangeUserPasswordCommand>(nameof(User))
                        .Property(x => x.OldPassword!)
                        .Message(MessageType.Correct)
                        .Negative()
                        .Build()
                )
            );
        }

        user.ChangePassword(HashPassword(request.NewPassword));
        await userManager.UpdateAsync(user, cancellationToken);

        return Result<string>.Success();
    }
}
