using Application.Common.Interfaces.Services.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Mail;

public static class MailExtension
{
    public static IServiceCollection AddMail(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions<EmailSettings>().Bind(configuration.GetSection(nameof(EmailSettings)));
        services
            .AddTransient<IMailService, MailService>()
            .AddSingleton<TemplateRenderer>()
            .AddSingleton(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                return env.WebRootFileProvider;
            });

        return services;
    }
}
