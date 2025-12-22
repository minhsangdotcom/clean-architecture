using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Roles;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommandValidator(
    IEfUnitOfWork unitOfWork,
    IRequestContextProvider contextProvider,
    IMessageTranslatorService translator
) : FluentValidator<RoleUpdateData>(contextProvider, translator)
{
    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        IMessageTranslatorService translator
    )
    {
        Include(new RoleValidator(unitOfWork, contextProvider, translator));
    }

    protected override void ApplyRules(IMessageTranslatorService translator) { }
}
