using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Roles;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleCommandValidator(
    IEfUnitOfWork unitOfWork,
    IHttpContextAccessorService httpContextAccessorService,
    IMessageTranslatorService translator
) : FluentValidator<CreateRoleCommand>(httpContextAccessorService, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        Include(new RoleValidator(unitOfWork, contextAccessor, translator));
    }

    protected override void ApplyRules(IMessageTranslatorService translator) { }
}
