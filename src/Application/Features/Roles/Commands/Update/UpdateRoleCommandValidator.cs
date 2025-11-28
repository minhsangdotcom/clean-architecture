using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.SharedFeatures.Validators.Roles;
using FluentValidation;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        IMessageTranslatorService translator
    )
    {
        RuleFor(x => x.UpdateData)
            .SetValidator(new RoleValidator(unitOfWork, httpContextAccessorService, translator));
    }
}
