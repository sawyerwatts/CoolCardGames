using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Options;

namespace CoolCardGames.Cli;

public class CliPlayerFactory(IServiceProvider services)
{
    public CliPlayer Make(PlayerAccountCard accountCard)
    {
        var cliUserSettings = services.GetRequiredService<IOptionsMonitor<CliPlayerUserSettings>>();
        var cliSystemSettings = services.GetRequiredService<IOptionsMonitor<CliPlayerSystemSettings>>();
        return new CliPlayer(
            accountCard,
            cliUserSettings,
            cliSystemSettings,
            services.GetRequiredService<ILogger<CliPlayer>>());
    }
}