using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.Localization;
using Application.SharedFeatures.Validators.Roles;
using FluentValidation;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleCommandValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        IMessageTranslatorService translator
    )
    {
        Include(new RoleValidator(unitOfWork, httpContextAccessorService, translator));
    }
}
