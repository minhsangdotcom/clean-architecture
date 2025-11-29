using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.ResetPassword;

public class UpdateUserPasswordValidator(
    IHttpContextAccessorService httpContextAccessor,
    IMessageTranslatorService translator
) : FluentValidator<ResetUserPasswordCommand>(httpContextAccessor, translator)
{
    protected sealed override void ApplyRules(IMessageTranslatorService translator)
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
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    ) { }
}
