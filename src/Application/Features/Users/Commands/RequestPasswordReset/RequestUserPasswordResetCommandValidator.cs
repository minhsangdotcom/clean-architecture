using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetCommandValidator(
    IHttpContextAccessorService contextAccessor,
    IMessageTranslatorService translator
) : FluentValidator<RequestUserPasswordResetCommand>(contextAccessor, translator)
{
    protected override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    ) { }

    protected sealed override void ApplyRules(IMessageTranslatorService translator)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailRequired);
    }
}
