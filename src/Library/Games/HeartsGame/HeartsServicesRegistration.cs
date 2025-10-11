using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Games.HeartsGame;

public static class HeartsServicesRegistration
{
    public static IHostApplicationBuilder AddHeartsServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<HeartsGame.Factory>();
        builder.Services.AddOptions<HeartsGame.Settings>()
            .BindConfiguration($"{CoreServicesRegistration.GamesConfigSection}:Hearts")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }
}