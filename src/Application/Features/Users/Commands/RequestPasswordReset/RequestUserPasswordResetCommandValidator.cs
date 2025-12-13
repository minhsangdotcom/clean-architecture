using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetCommandValidator(
    IRequestContextProvider contextProvider,
    IMessageTranslatorService translator
) : FluentValidator<RequestUserPasswordResetCommand>(contextProvider, translator)
{
    protected override void ApplyRules(
        IRequestContextProvider contextProvider,
        IMessageTranslatorService translator
    ) { }

    protected sealed override void ApplyRules(IMessageTranslatorService translator)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailRequired);
    }
}
