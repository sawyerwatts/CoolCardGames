using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Games.Hearts;

public static class HeartsServicesRegistration
{
    public static void AddHeartsServices(this IHostApplicationBuilder builder, string gamesConfigSectionName)
    {
        builder.Services.AddSingleton<HeartsFactory>();
        builder.Services.AddOptions<HeartsSettings>()
            .BindConfiguration($"{gamesConfigSectionName}:Hearts")
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}