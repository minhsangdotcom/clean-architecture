using Application.Contracts.ErrorCodes;
using Application.Contracts.Messages;
using Application.Features.Users.Commands.ChangePassword;
using Application.SharedFeatures.Requests.Users;
using Domain.Aggregates.Users;

namespace Application.Common.ErrorCodes;

public static class UserErrorMessages
{
    // -------------------------
    // LastName
    // -------------------------
    [ErrorKey(nameof(UserLastNameRequired))]
    public static string UserLastNameRequired =>
        Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserLastNameTooLong))]
    public static string UserLastNameTooLong =>
        Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

    // -------------------------
    // FirstName
    // -------------------------
    [ErrorKey(nameof(UserFirstNameRequired))]
    public static string UserFirstNameRequired =>
        Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserFirstNameTooLong))]
    public static string UserFirstNameTooLong =>
        Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

    // -------------------------
    // PhoneNumber
    // -------------------------
    [ErrorKey(nameof(UserPhoneNumberInvalid))]
    public static string UserPhoneNumberInvalid =>
        Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber!)
            .WithError(MessageErrorType.Valid)
            .Negative()
            .GetFullMessage();

    // -------------------------
    // Status
    // -------------------------
    [ErrorKey(nameof(UserStatusRequired))]
    public static string UserStatusRequired =>
        Messenger
            .Create<User>()
            .Property(x => x.Status)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserStatusNotInEnum))]
    public static string UserStatusNotInEnum =>
        Messenger
            .Create<User>()
            .Property(x => x.Status)
            .Negative()
            .WithError(MessageErrorType.AmongTheAllowedOptions)
            .GetFullMessage();

    // -------------------------
    // Roles
    // -------------------------
    [ErrorKey(nameof(UserRolesRequired))]
    public static string UserRolesRequired =>
        Messenger
            .Create<UserUpsertCommand>(nameof(User))
            .Property(x => x.Roles!)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserRolesNotUnique))]
    public static string UserRolesNotUnique =>
        Messenger
            .Create<UserUpsertCommand>(nameof(User))
            .Property(x => x.Roles!)
            .WithError(MessageErrorType.Unique)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserRolesNotFound))]
    public static string UserRolesNotFound =>
        Messenger
            .Create<UserUpsertCommand>(nameof(User))
            .Property(x => x.Roles!)
            .WithError(MessageErrorType.Found)
            .Negative()
            .GetFullMessage();

    // -------------------------
    // Permissions
    // -------------------------
    [ErrorKey(nameof(UserPermissionsNotUnique))]
    public static string UserPermissionsNotUnique =>
        Messenger
            .Create<UserUpsertCommand>(nameof(User))
            .Property(req => req.Permissions!)
            .WithError(MessageErrorType.Unique)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserPermissionsNotFound))]
    public static string UserPermissionsNotFound =>
        Messenger
            .Create<UserUpsertCommand>(nameof(User))
            .Property(req => req.Permissions!)
            .WithError(MessageErrorType.Found)
            .Negative()
            .GetFullMessage();

    // --------------------------------
    // Username
    // --------------------------------
    [ErrorKey(nameof(UserUsernameRequired))]
    public static string UserUsernameRequired =>
        Messenger
            .Create<User>()
            .Property(x => x.Username)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserUsernameInvalid))]
    public static string UserUsernameInvalid =>
        Messenger
            .Create<User>()
            .Property(x => x.Username)
            .WithError(MessageErrorType.Valid)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserUsernameExistent))]
    public static string UserUsernameExistent =>
        Messenger
            .Create<User>()
            .Property(x => x.Username)
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

    // --------------------------------
    // Email
    // --------------------------------
    [ErrorKey(nameof(UserEmailRequired))]
    public static string UserEmailRequired =>
        Messenger
            .Create<User>()
            .Property(x => x.Email)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserEmailInvalid))]
    public static string UserEmailInvalid =>
        Messenger
            .Create<User>()
            .Property(x => x.Email)
            .WithError(MessageErrorType.Valid)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserEmailExistent))]
    public static string UserEmailExistent =>
        Messenger
            .Create<User>()
            .Property(x => x.Email)
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

    // --------------------------------
    // Password
    // --------------------------------
    [ErrorKey(nameof(UserPasswordRequired))]
    public static string UserPasswordRequired =>
        Messenger
            .Create<User>()
            .Property(x => x.Password)
            .WithError(MessageErrorType.Required)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserPasswordWeak))]
    public static string UserPasswordWeak =>
        Messenger
            .Create<User>()
            .Property(x => x.Password)
            .WithError(MessageErrorType.Strong)
            .Negative()
            .GetFullMessage();

    // --------------------------------
    // Gender
    // --------------------------------
    [ErrorKey(nameof(UserGenderNotInEnum))]
    public static string UserGenderNotInEnum =>
        Messenger
            .Create<User>()
            .Property(x => x.Gender!)
            .Negative()
            .WithError(MessageErrorType.AmongTheAllowedOptions)
            .GetFullMessage();

    // --------------------------------
    // User
    // --------------------------------
    [ErrorKey(nameof(UserNotFound))]
    public static string UserNotFound =>
        Messenger.Create<User>().WithError(MessageErrorType.Found).Negative().GetFullMessage();

    [ErrorKey(nameof(UserOldPasswordIncorrect))]
    public static string UserOldPasswordIncorrect =>
        Messenger
            .Create<ChangeUserPasswordCommand>(nameof(User))
            .Property(x => x.OldPassword!)
            .WithError(MessageErrorType.Correct)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserPasswordIncorrect))]
    public static string UserPasswordIncorrect =>
        Messenger
            .Create<User>()
            .Property(x => x.Password)
            .WithError(MessageErrorType.Correct)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserInactive))]
    public static string UserInactive =>
        Messenger.Create<User>().WithError(MessageErrorType.Active).Negative().GetFullMessage();

    [ErrorKey(nameof(UserResetPasswordTokenInvalid))]
    public static string UserResetPasswordTokenInvalid =>
        Messenger
            .Create<UserPasswordReset>()
            .Property(x => x.Token)
            .WithError(MessageErrorType.Correct)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserRefreshTokenInvalid))]
    public static string UserRefreshTokenInvalid =>
        Messenger
            .Create<UserRefreshToken>(nameof(User))
            .Property(x => x.Token!)
            .WithError(MessageErrorType.Valid)
            .Negative()
            .GetFullMessage();

    [ErrorKey(nameof(UserRefreshTokenIdentical))]
    public static string UserRefreshTokenIdentical =>
        Messenger
            .Create<UserRefreshToken>(nameof(User))
            .Property(x => x.Token!)
            .Negative()
            .WithError(MessageErrorType.Identical)
            .ToObject("TheCurrentOne")
            .GetFullMessage();

    [ErrorKey(nameof(UserRefreshTokenExpired))]
    public static string UserRefreshTokenExpired =>
        Messenger
            .Create<UserRefreshToken>(nameof(User))
            .Property(x => x.Token!)
            .WithError(MessageErrorType.Expired)
            .GetFullMessage();
}
