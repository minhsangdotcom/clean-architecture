using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Common.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IStringLocalizer stringLocalizer;

    public CreateUserCommandValidator(IEfUnitOfWork unitOfWork, IStringLocalizer stringLocalizer)
    {
        this.unitOfWork = unitOfWork;
        this.stringLocalizer = stringLocalizer;
        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new UserValidator(unitOfWork, stringLocalizer)!);
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Username)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .Must((_, x) => x!.IsValidUsername())
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Username)
                    .WithError(MessageErrorType.Valid)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MustAsync(
                (username, cancellationToken) =>
                    IsUsernameAvailableAsync(username!, cancellationToken: cancellationToken)
            )
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Username)
                    .WithError(MessageErrorType.Existent)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Email)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .Must(x => x!.IsValidEmail())
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Email)
                    .WithError(MessageErrorType.Valid)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MustAsync(
                (email, cancellationToken) =>
                    IsEmailAvailableAsync(email!, cancellationToken: cancellationToken)
            )
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Email)
                    .WithError(MessageErrorType.Existent)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Password)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .Must((_, x) => x!.IsValidPassword())
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Password)
                    .WithError(MessageErrorType.Strong)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Gender!)
                    .Negative()
                    .WithError(MessageErrorType.AmongTheAllowedOptions)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });
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
