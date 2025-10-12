using CoolCardGames.Library.Core;
using CoolCardGames.Library.Core.CardTypes;

namespace CoolCardGames.Cli;

// TODO: have a configurable delay b/w messages
public class CliPlayerSession<TCard>(AccountCard accountCard) : PlayerSession<TCard>(accountCard)
    where TCard : Card
{
    public override Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}