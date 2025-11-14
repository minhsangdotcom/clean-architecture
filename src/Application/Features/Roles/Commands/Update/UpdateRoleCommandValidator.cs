using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Roles;
using FluentValidation;

namespace Application.Features.Roles.Commands.Update;

public class UpdateRoleCommandValidator : AbstractValidator<RoleUpdateRequest>
{
    public UpdateRoleCommandValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService
    )
    {
        Include(new RoleValidator(unitOfWork, httpContextAccessorService));
    }
}
