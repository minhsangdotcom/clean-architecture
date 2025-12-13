using Application.Contracts.Dtos.Requests;
using RazorLight;

namespace Infrastructure.Services.Mail;

public class RazorViewToStringRenderer
{
    private readonly RazorLightEngine razorLightEngine = new RazorLightEngineBuilder()
        .UseFileSystemProject(GetRootPath())
        .UseMemoryCachingProvider()
        .Build();

    public async Task<string> RenderViewToStringAsync(MailTemplate mailTemplate)
    {
        try
        {
            return await razorLightEngine.CompileRenderAsync(
                Path.Combine("Templates", $"{mailTemplate.ViewName}.cshtml"),
                mailTemplate.Template
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to render view {mailTemplate.ViewName}. Exception: {ex.Message}"
            );
        }
    }

    private static string GetRootPath() => Path.Join(Directory.GetCurrentDirectory(), "wwwroot");
}
