using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetCommandValidator(
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<RequestUserPasswordResetCommand>(contextProvider, translator)
{
    protected override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    ) { }

    protected sealed override void ApplyRules(ITranslator<Messages> translator)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailRequired);
    }
}
