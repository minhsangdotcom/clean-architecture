namespace Contracts.Messages;

public record MessageErrorMetadata(
    string Error,
    string? NegativeForm = null,
    string? Preposition = null
);
