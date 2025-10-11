using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolCardGames.Library.Core;

public static class CoreServicesRegistration
{
    public const string GamesConfigSection = "Games";

    public static IHostApplicationBuilder AddCoreServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDealer, Dealer>();

        return builder;
    }
}