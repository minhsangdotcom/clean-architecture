using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.Token;

public class RefreshUserTokenCommandValidator(
    IRequestContextProvider contextProvider,
    ITranslator<Messages> messageTranslator
) : FluentValidator<RefreshUserTokenCommand>(contextProvider, messageTranslator)
{
    protected sealed override void ApplyRules(ITranslator<Messages> translator)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserRefreshTokenTokenRequired);
    }

    protected override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    ) { }
}
