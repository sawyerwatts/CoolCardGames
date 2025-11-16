using System.Threading.Channels;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Cli;

// TODO: have a configurable delay b/w messages

public class CliPlayer<TCard>(AccountCard accountCard) : IPlayer<TCard>
    where TCard : Card
{
    public AccountCard AccountCard => accountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}