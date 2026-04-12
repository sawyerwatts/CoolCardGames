using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayerFactory(IServiceProvider services)
{
    public AiPlayer Make(PlayerAccountCard playerAccountCard)
    {
        return new AiPlayer(playerAccountCard, services.GetRequiredService<ILogger<AiPlayer>>());
    }
}