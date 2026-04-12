namespace CoolCardGames.WebApi;

public static class WebApiServicesRegistration
{
    private const string WebApiSectionName = "WebApi";

    public static void AddWebApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<AppLevelCancellationTokenHostedService>();
        builder.Services.AddHostedService<AppLevelCancellationTokenHostedService>(services =>
            services.GetRequiredService<AppLevelCancellationTokenHostedService>());

        builder.Services.AddOptions<WebPlayer.Settings>()
            .BindConfiguration("Player")
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}