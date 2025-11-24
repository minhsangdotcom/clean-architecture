using Application.Common.Interfaces.UnitOfWorks;
using Application.SharedFeatures.Validators.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommandValidator : AbstractValidator<UserUpdateRequest>
{
    public UpdateUserCommandValidator(IEfUnitOfWork unitOfWork, IStringLocalizer stringLocalizer)
    {
        Include(new UserValidator(unitOfWork, stringLocalizer));
    }
}
