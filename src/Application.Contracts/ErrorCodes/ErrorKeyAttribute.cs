namespace Application.Contracts.ErrorCodes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ErrorKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
