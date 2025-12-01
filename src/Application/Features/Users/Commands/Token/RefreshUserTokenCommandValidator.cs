using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.Token;

public class RefreshUserTokenCommandValidator(
    IHttpContextAccessorService contextAccessor,
    IMessageTranslatorService messageTranslator
) : FluentValidator<RefreshUserTokenCommand>(contextAccessor, messageTranslator)
{
    protected sealed override void ApplyRules(IMessageTranslatorService translator)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserRefreshTokenTokenRequired);
    }

    protected override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    ) { }
}
