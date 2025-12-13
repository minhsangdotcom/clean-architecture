using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Users;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommandValidator(
    IEfUnitOfWork unitOfWork,
    IRequestContextProvider contextProvider,
    IMessageTranslatorService translator
) : FluentValidator<UpdateUserCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        IMessageTranslatorService translator
    )
    {
        RuleFor(x => x.UpdateData)
            .SetValidator(new UserValidator(unitOfWork, contextProvider, translator));
    }

    protected override void ApplyRules(IMessageTranslatorService translator) { }
}
