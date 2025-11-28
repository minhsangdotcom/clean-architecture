using Application.Common.ErrorCodes;
using Application.Common.Extensions;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using FluentValidation;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator(IMessageTranslatorService translator)
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithState(state => new ErrorReason(
                UserErrorMessages.UserOldPasswordRequired,
                translator.Translate(UserErrorMessages.UserOldPasswordRequired)
            ));

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(state => new ErrorReason(
                UserErrorMessages.UserNewPasswordRequired,
                translator.Translate(UserErrorMessages.UserNewPasswordRequired)
            ))
            .Must(x => x!.IsValidPassword())
            .WithState(state => new ErrorReason(
                UserErrorMessages.UserNewPasswordNotStrong,
                translator.Translate(UserErrorMessages.UserNewPasswordNotStrong)
            ));
    }
}
