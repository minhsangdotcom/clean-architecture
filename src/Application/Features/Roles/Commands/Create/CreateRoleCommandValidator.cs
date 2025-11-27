using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.SharedFeatures.Validators.Roles;
using FluentValidation;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        IMessageTranslatorService translator
    )
    {
        Include(new RoleValidator(unitOfWork, httpContextAccessorService, translator));
    }
}
