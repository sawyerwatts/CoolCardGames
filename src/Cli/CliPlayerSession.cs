using System.Collections.Concurrent;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.GameEvents;
using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Cli;

// TODO: have a configurable delay b/w messages
public class CliPlayerSession<TCard>(AccountCard accountCard) : IPlayerSession<TCard>
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