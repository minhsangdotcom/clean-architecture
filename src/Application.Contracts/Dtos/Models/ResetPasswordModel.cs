namespace Application.Contracts.Dtos.Models;

public class ResetPasswordModel
{
    public required string ResetLink { get; set; }

    public required double ExpiredTimeInHour { get; set; }

    public required string UserName { get; set; }
    public required string SupportEmail { get; set; }
    public required string UserEmail { get; set; }

    public required int Year { get; set; }
}
