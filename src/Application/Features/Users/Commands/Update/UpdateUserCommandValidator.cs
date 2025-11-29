using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Users;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommandValidator(
    IEfUnitOfWork unitOfWork,
    IHttpContextAccessorService contextAccessor,
    IMessageTranslatorService translator
) : FluentValidator<UserUpdateRequest>(contextAccessor, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        Include(new UserValidator(unitOfWork, contextAccessor, translator));
    }

    protected override void ApplyRules(IMessageTranslatorService translator) { }
}
