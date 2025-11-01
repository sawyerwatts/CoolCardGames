using System.Collections.Concurrent;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayerSession<TCard>(AccountCard accountCard) : IPlayerSession<TCard>
    where TCard : Card
{
    public AccountCard AccountCard => accountCard;

    public ConcurrentQueue<GameEvent> UnprocessedGameEvents { get; } = [];

    public Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}