namespace CoolCardGames.Library.Core.Players;

public class AiPlayerSession<TCard>(AccountCard accountCard) : PlayerSession<TCard>
    where TCard : Card
{
    public override AccountCard AccountCard => accountCard;

    public override Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}