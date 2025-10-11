using CoolCardGames.Library.Games.HeartsGame;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Core;

public static class ServicesRegistration
{
    private const string GamesConfigSection = "Games";

    public static IHostApplicationBuilder AddLibraryServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDealer, Dealer>();

        builder.AddHeartsServices(GamesConfigSection);

        return builder;
    }
}