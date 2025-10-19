using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Cli;

// TODO: have a configurable delay b/w messages
public class CliUser<TCard>(AccountCard accountCard) : User<TCard>(accountCard)
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