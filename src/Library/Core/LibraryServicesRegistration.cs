using CoolCardGames.Library.Games.HeartsGame;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Core;

public static class LibraryServicesRegistration
{
    private const string GamesConfigSectionName = "Games";

    public static IHostApplicationBuilder AddLibraryServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDealer, Dealer>();

        builder.AddHeartsServices(GamesConfigSectionName);

        return builder;
    }
}