namespace Application.Contracts.Messages;

public enum MessageErrorType
{
    TooLong = 1,
    TooShort = 2,
    Valid = 3,
    Found = 4,
    Existent = 5,
    Correct = 6,
    Active = 7,
    AmongTheAllowedOptions = 8,
    GreaterThan = 9,
    GreaterThanEqual = 10,
    LessThan = 11,
    LessThanEqual = 12,
    Required = 13,
    Unique = 14,
    Strong = 15,
    Expired = 16,
    Redundant = 17,
    Missing = 18,
    Identical = 19,
}
