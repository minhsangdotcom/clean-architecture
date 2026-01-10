using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.ResetPassword;

public class ResetUserPasswordCommandValidator(
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<ResetUserPasswordCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(ITranslator<Messages> translator)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailRequired)
            .BeValidEmail()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailInvalid);

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserPasswordResetTokenRequired);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserPasswordRequired)
            .BeValidPassword()
            .WithTranslatedError(translator, UserErrorMessages.UserNewPasswordNotStrong);
    }

    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    ) { }
}
