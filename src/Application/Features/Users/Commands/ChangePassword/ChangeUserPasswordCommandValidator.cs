using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordCommandValidator(
    IHttpContextAccessorService accessorService,
    IMessageTranslatorService translator
) : FluentValidator<ChangeUserPasswordCommand>(accessorService, translator)
{
    protected sealed override void ApplyRules(IMessageTranslatorService translator)
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserOldPasswordRequired);

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserNewPasswordRequired)
            .Must(x => x!.IsValidPassword())
            .WithTranslatedError(translator, UserErrorMessages.UserNewPasswordNotStrong);
    }

    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    ) { }
}
