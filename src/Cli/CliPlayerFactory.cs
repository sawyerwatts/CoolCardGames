using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Options;

namespace CoolCardGames.Cli;

public class CliPlayerFactory(IServiceProvider services)
{
    public CliPlayer<TCard> Make<TCard>(PlayerAccountCard accountCard)
        where TCard : Card
    {
        var cliUserSettings = services.GetRequiredService<IOptionsMonitor<CliPlayerUserSettings>>();
        var cliSystemSettings = services.GetRequiredService<IOptionsMonitor<CliPlayerSystemSettings>>();
        return new CliPlayer<TCard>(
            accountCard,
            cliUserSettings,
            cliSystemSettings,
            services.GetRequiredService<ILogger<CliPlayer<TCard>>>());
    }
}