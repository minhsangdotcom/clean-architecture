using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommandValidator : AbstractValidator<UserUpdateRequest>
{
    public UpdateUserCommandValidator(IEfUnitOfWork unitOfWork)
    {
        Include(new UserValidator(unitOfWork)!);
    }
}
