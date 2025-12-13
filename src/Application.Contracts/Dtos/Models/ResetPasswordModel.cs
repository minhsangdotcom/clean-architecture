namespace Application.Contracts.Dtos.Models;

public class ResetPasswordModel
{
    public string? ResetLink { get; set; }

    public string? Expiry { get; set; }
}
