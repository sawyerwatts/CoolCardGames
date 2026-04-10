namespace CoolCardGames.WebApi;

public static class WebApiServicesRegistration
{
    private const string WebApiSectionName = "WebApi";

    public static void AddWebApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<AppLevelCancellationTokenHostedService>();
    }
}