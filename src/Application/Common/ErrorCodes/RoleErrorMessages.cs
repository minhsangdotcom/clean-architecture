using Application.Contracts.ErrorCodes;
using Application.Contracts.Messages;
using Domain.Aggregates.Roles;

namespace Application.Common.ErrorCodes;

public class RoleErrorMessages
{
    // -------------------------
    // Name
    // -------------------------
    [ErrorKey(nameof(RoleNameRequired))]
    public static string RoleNameRequired =>
        Messenger
            .Create<Role>()
            .Property(x => x.Name)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

    [ErrorKey(nameof(RoleNameTooLong))]
    public static string RoleNameTooLong =>
        Messenger
            .Create<Role>(nameof(Role))
            .Property(x => x.Name!)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

    [ErrorKey(nameof(RoleNameExistent))]
    public static string RoleNameExistent =>
        Messenger
            .Create<Role>()
            .Property(x => x.Name!)
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

    // -------------------------
    // Description
    // -------------------------
    [ErrorKey(nameof(RoleDescriptionTooLong))]
    public static string RoleDescriptionTooLong =>
        Messenger
            .Create<Role>()
            .Property(x => x.Description!)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

    // -------------------------
    // Permissions
    // -------------------------
    [ErrorKey(nameof(RolePermissionsRequired))]
    public static string RolePermissionsRequired =>
        Messenger
            .Create<Role>()
            .Property(x => x.Permissions)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

    [ErrorKey(nameof(RolePermissionsUnique))]
    public static string RolePermissionsUnique =>
        Messenger
            .Create<Role>()
            .Property(x => x.Permissions)
            .Negative()
            .WithError(MessageErrorType.Unique)
            .GetFullMessage();

    [ErrorKey(nameof(RolePermissionsExistent))]
    public static string RolePermissionsExistent =>
        Messenger
            .Create<Role>()
            .Property(x => x.Permissions)
            .Negative()
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

    //--------
    // role
    //-------

    [ErrorKey(nameof(RoleNotFound))]
    public static string RoleNotFound =>
        Messenger.Create<Role>().WithError(MessageErrorType.Found).Negative().GetFullMessage();
}
