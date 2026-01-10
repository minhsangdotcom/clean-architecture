using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordHandler(
    IUserManager userManager,
    ICurrentUser currentUser,
    ITranslator<Messages> translator
) : IRequestHandler<ChangeUserPasswordCommand, Result<string>>
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
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }

        if (!Verify(request.OldPassword, user.Password))
        {
            string errorMessage = Messenger
                .Create<ChangeUserPasswordCommand>(nameof(User))
                .Property(x => x.OldPassword!)
                .WithError(MessageErrorType.Correct)
                .Negative()
                .GetFullMessage();
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with password",
                    new(
                        UserErrorMessages.UserOldPasswordIncorrect,
                        translator.Translate(UserErrorMessages.UserOldPasswordIncorrect)
                    )
                )
            );
        }

        user.ChangePassword(HashPassword(request.NewPassword));
        await userManager.UpdateAsync(user, cancellationToken);

        return Result<string>.Success();
    }
}
