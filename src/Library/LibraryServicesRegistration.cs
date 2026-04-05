using CoolCardGames.Library.Games;
using CoolCardGames.Library.Games.Hearts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library;

public static class LibraryServicesRegistration
{
    private const string GamesConfigSectionName = "Games";

    public static void AddLibraryServices(this IHostApplicationBuilder builder)
    {
        builder.AddCoreServices();
        builder.Services.AddSingleton<GameRegistry>();
        builder.AddHeartsServices(GamesConfigSectionName);
    }
}