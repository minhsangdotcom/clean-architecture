using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator(
    IEfUnitOfWork unitOfWork,
    IMessageTranslatorService translator,
    IHttpContextAccessorService contextAccessor
) : FluentValidator<CreateUserCommand>(contextAccessor, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        Include(new UserValidator(unitOfWork, contextAccessor, translator));
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserUsernameRequired,
                translator.Translate(UserErrorMessages.UserUsernameRequired)
            ))
            .Must((_, x) => x!.IsValidUsername())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserUsernameInvalid,
                translator.Translate(UserErrorMessages.UserUsernameInvalid)
            ))
            .MustAsync((username, ct) => IsUsernameAvailableAsync(username!, cancellationToken: ct))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserUsernameExistent,
                translator.Translate(UserErrorMessages.UserUsernameExistent)
            ));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPasswordRequired,
                translator.Translate(UserErrorMessages.UserPasswordRequired)
            ))
            .Must((_, x) => x!.IsValidPassword())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPasswordWeak,
                translator.Translate(UserErrorMessages.UserPasswordWeak)
            ));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserGenderNotInEnum,
                translator.Translate(UserErrorMessages.UserGenderNotInEnum)
            ));
    }

    protected sealed override void ApplyRules(IMessageTranslatorService translator) { }

    private async Task<bool> IsUsernameAvailableAsync(
        string username,
        Ulid? id = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x => (!id.HasValue && x.Username == username) || x.Username == username,
                cancellationToken
            );
}
