using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Core;

public static class CoreServicesRegistration
{
    public static void AddCoreServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDealerFactory, DealerFactory>();
        builder.Services.AddSingleton<Dealer.IRng, Dealer.Rng>();

        builder.Services.AddSingleton<IGameEventMultiplexerFactory, GameEventMultiplexerFactory>();
    }
}