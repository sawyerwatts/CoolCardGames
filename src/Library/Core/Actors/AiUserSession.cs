namespace CoolCardGames.Library.Core.Actors;

public class AiUserSession<TCard>(AccountCard accountCard) : UserSession<TCard>(accountCard)
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