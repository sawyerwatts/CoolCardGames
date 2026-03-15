using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayerFactory(IServiceProvider services)
{
    public AiPlayer<TCard> Make<TCard>(PlayerAccountCard playerAccountCard)
        where TCard : Card
    {
        return new AiPlayer<TCard>(playerAccountCard, services.GetRequiredService<ILogger<AiPlayer<TCard>>>());
    }
}