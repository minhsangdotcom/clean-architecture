using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.SharedFeatures.Validators.Roles;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Roles.Commands.Create;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        IStringLocalizer stringLocalizer
    )
    {
        Include(new RoleValidator(unitOfWork, httpContextAccessorService, stringLocalizer));
    }
}
