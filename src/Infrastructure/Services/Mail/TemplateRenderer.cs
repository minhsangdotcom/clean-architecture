using Application.Common.Interfaces.Services.Cache;
using Application.Contracts.Dtos.Requests;
using Microsoft.Extensions.FileProviders;
using Scriban;

namespace Infrastructure.Services.Mail;

public class TemplateRenderer(IMemoryCacheService cache, IFileProvider fileProvider)
{
    public async Task<string> RenderViewToStringAsync(MailTemplate mailTemplate)
    {
        string cacheKey = $"razor_tpl_{mailTemplate.ViewName}";
        string rootPath = Path.Join(Directory.GetCurrentDirectory(), "wwwroot");
        string templatePath = Path.Combine(rootPath, "Templates", mailTemplate.ViewName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found: {templatePath}");
        }
        string watchingPath = Path.Combine("Templates", mailTemplate.ViewName);
        try
        {
            CacheOptions cacheOptions = new()
            {
                ExpirationType = CacheExpirationType.Sliding,
                Expiration = TimeSpan.FromHours(1),
                ChangeToken = fileProvider.Watch(watchingPath),
            };
            var template = await cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    string templateContent = await File.ReadAllTextAsync(templatePath);
                    return Template.Parse(templateContent);
                },
                cacheOptions
            );
            return await template!.RenderAsync(mailTemplate.Template) ?? "";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to render view {mailTemplate.ViewName}. Exception: {ex.Message}"
            );
        }
    }
}
