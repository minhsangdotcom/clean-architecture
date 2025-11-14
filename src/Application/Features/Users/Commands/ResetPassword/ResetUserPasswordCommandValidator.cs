using Application.Common.Extensions;
using Domain.Aggregates.Users;
using FluentValidation;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.ResetPassword;

public class UpdateUserPasswordValidator : AbstractValidator<UpdateUserPassword>
{
    public UpdateUserPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<UserPasswordReset>()
                    .Property(x => x.Token)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.Password!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x => x!.IsValidPassword())
            .WithState(x =>
                Messenger
                    .Create<User>(nameof(User))
                    .Property(x => x.Password)
                    .Message(MessageType.Strong)
                    .Negative()
                    .Build()
            );
    }
}
