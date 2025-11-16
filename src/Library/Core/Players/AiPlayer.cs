using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayer<TCard>(AccountCard accountCard) : IPlayer<TCard>
    where TCard : Card
{
    public AccountCard AccountCard => accountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}