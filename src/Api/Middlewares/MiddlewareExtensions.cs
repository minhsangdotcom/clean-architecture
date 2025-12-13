namespace Api.Middlewares;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLocalizationMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LocalizerMiddleware>();
    }
}
