using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.Localization;
using Application.SharedFeatures.Validators.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommandValidator : AbstractValidator<UserUpdateRequest>
{
    public UpdateUserCommandValidator(
        IEfUnitOfWork unitOfWork,
        IMessageTranslatorService translator
    )
    {
        Include(new UserValidator(unitOfWork, translator));
    }
}
