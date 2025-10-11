using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Games.HeartsGame;

public static class HeartsServicesRegistration
{
    public static IHostApplicationBuilder AddHeartsServices(this IHostApplicationBuilder builder, string gamesConfigSectionName)
    {
        builder.Services.AddSingleton<HeartsGame.Factory>();
        builder.Services.AddOptions<HeartsGame.Settings>()
            .BindConfiguration($"{gamesConfigSectionName}:Hearts")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }
}