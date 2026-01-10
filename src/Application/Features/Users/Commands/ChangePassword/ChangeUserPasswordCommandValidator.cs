using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordCommandValidator(
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<ChangeUserPasswordCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(ITranslator<Messages> translator)
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserOldPasswordRequired);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserNewPasswordRequired)
            .BeValidPassword()
            .WithTranslatedError(translator, UserErrorMessages.UserNewPasswordNotStrong);
    }

    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    ) { }
}
