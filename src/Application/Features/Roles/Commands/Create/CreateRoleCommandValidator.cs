using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Roles;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleCommandValidator(
    IEfUnitOfWork unitOfWork,
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<CreateRoleCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    )
    {
        Include(new RoleValidator(unitOfWork, contextProvider, translator));
    }

    protected override void ApplyRules(ITranslator<Messages> translator) { }
}
