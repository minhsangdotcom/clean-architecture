using System.Text.RegularExpressions;
using Application.Features.Common.Projections.Users;
using Application.Features.Common.Requests.Users;
using Application.Features.Common.Validators.Users;
using Application.Features.Users.Commands.Create;
using AutoFixture;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using SharedKernel.Common.Messages;

namespace Application.UnitTest.Users;

public partial class CreateUserCommandValidatorTest
{
    private readonly InlineValidator<CreateUserCommand> mockValidator;

    private readonly Fixture fixture = new();
    private readonly CreateUserCommand? command = null;
    private readonly Ulid roleId;

    public CreateUserCommandValidatorTest()
    {
        mockValidator = [];
        roleId = Ulid.Parse("01JS72XZJ6NFKFVWA9QM03RY5G");
        // command = fixture
        //     .Build<CreateUserCommand>()
        //     .With(x => x.ProvinceId, Ulid.Parse("01JRQHWS3RQR1N0J84EV1DQXR1"))
        //     .With(x => x.DistrictId, Ulid.Parse("01JRQHWSNPR3Z8Z20GBSB22CSJ"))
        //     .With(x => x.CommuneId, Ulid.Parse("01JRQHWTCHN5WBZ12WC08AZCZ8"))
        //     .Without(x => x.Avatar)
        //     .With(
        //         x => x.UserClaims,
        //         [new UserClaimUpsertCommand() { ClaimType = "test", ClaimValue = "test.value" }]
        //     )
        //     .With(x => x.Roles, [roleId])
        //     .With(x => x.Email, "admin@gmail.com")
        //     .With(x => x.PhoneNumber, "0123456789")
        //     .With(x => x.Username, "admin.super")
        //     .Create();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenFirstNameNullOrEmpty_ShouldReturnNullFailure(string? firstName)
    {
        //arrage
        command!.FirstName = firstName;
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Message(MessageType.Null)
            .Negative()
            .Build();

        mockValidator.RuleFor(x => x.FirstName).NotEmpty().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState)
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfFirstName_ShouldReturnMaximumLengthFailure()
    {
        command!.FirstName = new string([.. fixture.CreateMany<char>(257)]);
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Message(MessageType.MaximumLength)
            .Build();
        mockValidator.RuleFor(x => x.FirstName).MaximumLength(256).WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState)
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenLastNameNullOrEmpty_ShouldReturnNullFailure(string? lastName)
    {
        command!.LastName = lastName;

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        mockValidator.RuleFor(x => x.LastName).NotEmpty().WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfLastName_ShouldReturnMaximumLengthFailure()
    {
        command!.LastName = new string([.. fixture.CreateMany<char>(257)]);

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .Message(MessageType.MaximumLength)
            .Build();
        mockValidator.RuleFor(x => x.LastName).MaximumLength(256).WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenEmailNullOrEmpty_ShouldReturnNullFailure(string? email)
    {
        command!.Email = email;
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        mockValidator.RuleFor(x => x.Email).NotEmpty().WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("admin@gmail")]
    [InlineData("admingmail.com")]
    [InlineData("@gmail.com")]
    public async Task CreateUser_WhenEmailInvalidFormat_ShouldReturnInvalidFailure(string email)
    {
        command!.Email = email;

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Message(MessageType.Valid)
            .Negative()
            .Build();

        mockValidator
            .RuleFor(x => x.Email)
            .Must(x =>
            {
                Regex regex = EmailValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task Validate_WhenEmailDuplicated_ShouldReturnExistFailure()
    {
        const string existedEmail = "admin@gmail.com";
        command!.Email = existedEmail;

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Message(MessageType.Existence)
            .Build();

        mockValidator
            .RuleFor(x => x.Email)
            .MustAsync(
                (email, cancellationToken) =>
                    IsEmailAvailableAsync(email!, existedEmail, cancellationToken)
            )
            .When(_ => true)
            .WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPhoneNumberNullOrEmpty_ShouldReturNullFailure(string phoneNumber)
    {
        command!.PhoneNumber = phoneNumber;
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        mockValidator.RuleFor(x => x.PhoneNumber).NotEmpty().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState)
            .Only();
    }

    [Theory]
    [InlineData("1234567890123456")]
    [InlineData("+12345")]
    public async Task Validate_WhenPhoneNumberInvalidFormat_ShouldReturnInvalidFailure(
        string phoneNumber
    )
    {
        command!.PhoneNumber = phoneNumber;

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Message(MessageType.Valid)
            .Negative()
            .Build();

        mockValidator
            .RuleFor(x => x.PhoneNumber)
            .Must(x =>
            {
                Regex regex = PhoneValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState)
            .Only();
    }

    [Fact]
    public async Task Validate_WhenProvinceEmpty_ShouldReturnNullFailure()
    {
        // command!.ProvinceId = Ulid.Empty;

        // var expectedState = Messenger
        //     .Create<User>()
        //     .Property(nameof(CreateUserCommand.ProvinceId))
        //     .Message(MessageType.Null)
        //     .Negative()
        //     .Build();
        // mockValidator.RuleFor(x => x.ProvinceId).NotEmpty().WithState(x => expectedState);

        // //act
        // var result = await mockValidator.TestValidateAsync(command);

        // //assert
        // result
        //     .ShouldHaveValidationErrorFor(x => x.ProvinceId)
        //     .WithCustomState(expectedState)
        //     .Only();
    }

    [Fact]
    public async Task Validate_WhenDistrictEmpty_ShouldReturnNullFailure()
    {
        // command!.DistrictId = Ulid.Empty;

        // var expectedState = Messenger
        //     .Create<User>()
        //     .Property(nameof(CreateUserCommand.DistrictId))
        //     .Message(MessageType.Null)
        //     .Negative()
        //     .Build();
        // mockValidator.RuleFor(x => x.DistrictId).NotEmpty().WithState(x => expectedState);
        // //act
        // var result = await mockValidator.TestValidateAsync(command);

        // result
        //     .ShouldHaveValidationErrorFor(x => x.DistrictId)
        //     .WithCustomState(expectedState)
        //     .Only();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenStreetNullOrEmpty_ShouldReturnNullFailure(string? street)
    {
        // command!.Street = street;

        // var expectedState = Messenger
        //     .Create<User>()
        //     .Property(nameof(CreateUserCommand.Street))
        //     .Message(MessageType.Null)
        //     .Negative()
        //     .Build();

        // mockValidator.RuleFor(x => x.Street).NotEmpty().WithState(x => expectedState);

        // //act
        // var result = await mockValidator.TestValidateAsync(command);

        // //assert
        // result.ShouldHaveValidationErrorFor(x => x.Street).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenUsernameNullOrEmpty_ShouldReturnNullFailure(string username)
    {
        command!.Username = username;

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Username!)
            .Message(MessageType.Null)
            .Negative()
            .Build();

        mockValidator.RuleFor(x => x.Username).NotEmpty().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result.ShouldHaveValidationErrorFor(x => x.Username).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("admin-super")]
    [InlineData("admin@super")]
    [InlineData("admin123!")]
    public async Task CreateUser_WhenInvalidUsername_ShouldReturnInValidFailure(string username)
    {
        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Username!)
            .Message(MessageType.Valid)
            .Negative()
            .Build();
        command!.Username = username;
        mockValidator
            .RuleFor(x => x.Username)
            .Must(
                (_, x) =>
                {
                    Regex regex = UsernameValidationRegex();
                    return regex.IsMatch(x!);
                }
            )
            .WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Username).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task Validate_WhenUsernameDuplicated_ShouldReturnNotExistenceFailure()
    {
        string existedUserName = "admin";
        command!.Username = existedUserName;

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Username)
            .Message(MessageType.Existence)
            .Build();

        mockValidator
            .RuleFor(x => x.Username)
            .MustAsync(
                (username, cancellationToken) =>
                    IsUsernameAvailableAsync(
                        username!,
                        existedUserName,
                        cancellationToken: cancellationToken
                    )
            )
            .WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Username).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPasswordNullOrEmpty_ShouldReturnNullFailure(string password)
    {
        command!.Password = password;
        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Password!)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        mockValidator.RuleFor(x => x.Password).NotEmpty().WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData("admin@123")]
    [InlineData("adminadmin")]
    [InlineData("admin")]
    public async Task Validate_WhenPasswordInvalid_ShouldReturnInvalidFailure(string password)
    {
        command!.Password = password;

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Password!)
            .Message(MessageType.Strong)
            .Negative()
            .Build();
        mockValidator
            .RuleFor(x => x.Password)
            .Must(
                (_, x) =>
                {
                    Regex regex = PassowordValidationRegex();
                    return regex.IsMatch(x!);
                }
            )
            .WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task Validate_WhenGenderInvalid_ShouldReturnNotAmongTheAllowedOptionsFailure(
        int gender
    )
    {
        command!.Gender = (Gender)gender;

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Gender!)
            .Negative()
            .Message(MessageType.AmongTheAllowedOptions)
            .Build();

        mockValidator.RuleFor(x => x.Gender).IsInEnum().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Gender).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task CreateUser_WhenStatusNull_ShouldReturnNullFailure()
    {
        command!.Status = 0;

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Status!)
            .Message(MessageType.Null)
            .Negative()
            .Build();

        mockValidator.RuleFor(x => x.Status).NotEmpty().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Status).WithCustomState(expectedState).Only();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task Validate_WhenInvalidStatus_ShouldReturnNotAmongTheAllowedOptionsFailure(
        int status
    )
    {
        command!.Status = (UserStatus)status;
        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Status!)
            .Negative()
            .Message(MessageType.Null)
            .Build();

        mockValidator.RuleFor(x => x.Status).IsInEnum().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Status).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task Validate_WhenRolesNull_ShouldReturnNullFailure()
    {
        command!.Roles = null;

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Roles!)
            .Message(MessageType.Null)
            .Negative()
            .Build();

        mockValidator.RuleFor(x => x.Roles).NotEmpty().WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Roles).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task Validate_WhenDuplicatedRole_ShouldReturnNotUniqueFailure()
    {
        command!.Roles!.Add(roleId);

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Roles!)
            .Message(MessageType.Unique)
            .Negative()
            .Build();
        mockValidator
            .RuleFor(x => x.Roles)
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Roles).WithCustomState(expectedState).Only();
    }

    [Fact]
    public async Task Validate_WhenNotFoundRole_ShouldReturnNotFoundFailure()
    {
        command!.Roles!.Add(Ulid.NewUlid());
        List<Ulid> existedroles = [roleId, Ulid.NewUlid()];

        var expectedState = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Roles!)
            .Message(MessageType.Found)
            .Negative()
            .Build();

        mockValidator
            .RuleFor(x => x.Roles)
            .MustAsync(
                (roles, cancellationToken) =>
                    IsRolesAvailableAsync(roles!, existedroles, cancellationToken)
            )
            .WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.Roles).WithCustomState(expectedState);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenClaimTypeisNullOrEmpty_ShouldReturnNullFailure(string type)
    {
        command!.UserClaims!.ForEach(x => x.ClaimType = type);

        var expectedState = Messenger
            .Create<UserClaim>(nameof(User.Claims))
            .Property(x => x.ClaimType!)
            .Message(MessageType.Null)
            .Negative()
            .Build();

        mockValidator.RuleForEach(x => x.UserClaims).SetValidator(new UserClaimValidator());

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(
            $"{nameof(User.Claims)}[0].{nameof(UserClaimUpsertCommand.ClaimType)}"
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenClaimValueNullOrEmpty_ShouldReturnNullFailure(string? value)
    {
        command!.UserClaims!.ForEach(x => x.ClaimValue = value);

        var expectedState = Messenger
            .Create<UserClaim>(nameof(User.Claims))
            .Property(x => x.ClaimValue!)
            .Message(MessageType.Null)
            .Negative()
            .Build();

        mockValidator.RuleForEach(x => x.UserClaims).SetValidator(new UserClaimValidator());

        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(
            $"{nameof(User.Claims)}[0].{nameof(UserClaimUpsertCommand.ClaimValue)}"
        );
    }

    [Fact]
    public async Task Validate_WhenDuplicateClaim_ShouldReturnUniqueFailure()
    {
        command!.UserClaims!.Add(
            new UserClaimUpsertCommand() { ClaimType = "test", ClaimValue = "test.value" }
        );

        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Claims!)
            .Message(MessageType.Unique)
            .Negative()
            .BuildMessage();

        mockValidator
            .RuleFor(x => x.UserClaims)
            .Must(x =>
                x!
                    .FindAll(x => x.Id == null)
                    .DistinctBy(x => new { x.ClaimType, x.ClaimValue })
                    .Count() == x.FindAll(x => x.Id == null).Count
            )
            .WithState(x => expectedState);
        //act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result.ShouldHaveValidationErrorFor(x => x.UserClaims).WithCustomState(expectedState);
    }

    private static async Task<bool> IsEmailAvailableAsync(
        string email,
        string existedEmail,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(() => email != existedEmail, cancellationToken);
    }

    private static async Task<bool> IsRolesAvailableAsync(
        List<Ulid> roles,
        List<Ulid> existedRoles,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(() => existedRoles.All(x => roles.Contains(x)), cancellationToken);
    }

    private static async Task<bool> IsUsernameAvailableAsync(
        string username,
        string existedUsername,
        CancellationToken cancellationToken
    )
    {
        return await Task.Run(() => username != existedUsername, cancellationToken);
    }

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
    private static partial Regex UsernameValidationRegex();

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PassowordValidationRegex();
}
