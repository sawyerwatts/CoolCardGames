namespace CoolCardGames.Cli;

public static class CliServicesRegistration
{
    private const string CliSectionName = "Cli";

    public static void AddCliServices(this IHostApplicationBuilder builder)
    {
        AddDriver(builder);
        AddCliPlayer(builder);
    }

    private static void AddDriver(IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<Driver>();
        builder.Services.AddOptions<Driver.Settings>()
            .BindConfiguration($"{CliSectionName}:Driver")
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddCliPlayer(IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CliPlayerFactory>();
        builder.Services.AddOptions<CliPlayerUserSettings>()
            .BindConfiguration($"{CliSectionName}:Player")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddOptions<CliPlayerSystemSettings>()
            .BindConfiguration($"{CliSectionName}:System")
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

}