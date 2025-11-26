using Application.Common.ErrorCodes;
using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Localization;
using Application.SharedFeatures.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IMessageTranslatorService translator;

    public CreateUserCommandValidator(
        IEfUnitOfWork unitOfWork,
        IMessageTranslatorService translator
    )
    {
        this.unitOfWork = unitOfWork;
        this.translator = translator;
        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new UserValidator(unitOfWork, translator)!);
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

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailRequired,
                translator.Translate(UserErrorMessages.UserEmailRequired)
            ))
            .Must(x => x!.IsValidEmail())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailInvalid,
                translator.Translate(UserErrorMessages.UserEmailInvalid)
            ))
            .MustAsync((email, ct) => IsEmailAvailableAsync(email!, cancellationToken: ct))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                translator.Translate(UserErrorMessages.UserEmailExistent)
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

    private async Task<bool> IsEmailAvailableAsync(
        string email,
        Ulid? id = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x => x.Email == email && (!id.HasValue || x.Id != id.Value),
                cancellationToken
            );
}
