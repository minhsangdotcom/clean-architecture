using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Roles;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommandValidator(
    IEfUnitOfWork unitOfWork,
    IHttpContextAccessorService contextAccessor,
    IMessageTranslatorService translator
) : FluentValidator<UpdateRoleCommand>(contextAccessor, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        RuleFor(x => x.UpdateData)
            .SetValidator(new RoleValidator(unitOfWork, contextAccessor, translator));
    }

    protected override void ApplyRules(IMessageTranslatorService translator) { }
}
