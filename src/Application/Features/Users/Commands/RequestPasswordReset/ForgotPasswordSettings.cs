namespace Application.Features.Users.Commands.RequestPasswordReset;

public class ForgotPasswordSettings
{
    public string Uri { get; set; } = "http://localhost:3000/reset-password";
    public double ExpiredTimeInHour { get; set; } = 1;
    public string TemplateName { get; set; } = "ForgotPassword.html";
}
