using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        ICurrentUser currentUser
    )
    {
        Include(new UserValidator(unitOfWork, httpContextAccessorService, currentUser));
    }
}
