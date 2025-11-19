using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IEfUnitOfWork unitOfWork;

    public CreateUserCommandValidator(IEfUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new UserValidator(unitOfWork)!);
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Username!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must((_, x) => x!.IsValidUsername())
            .WithState(x =>
                Messenger
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Username!)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            )
            .MustAsync(
                (username, cancellationToken) =>
                    IsUsernameAvailableAsync(username!, cancellationToken: cancellationToken)
            )
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Username)
                    .Message(MessageType.Existence)
                    .Build()
            );

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Email)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x => x!.IsValidEmail())
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Email)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            )
            .MustAsync(
                (email, cancellationToken) =>
                    IsEmailAvailableAsync(email!, cancellationToken: cancellationToken)
            )
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
                    .Build()
            );

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Password)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must((_, x) => x!.IsValidPassword())
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Password)
                    .Message(MessageType.Strong)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Gender!)
                    .Negative()
                    .Message(MessageType.AmongTheAllowedOptions)
                    .Build()
            );
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
