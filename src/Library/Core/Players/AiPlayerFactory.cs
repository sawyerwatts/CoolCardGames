namespace CoolCardGames.Library.Core.Players;

public class AiPlayerFactory
{
    public AiPlayer<TCard> Make<TCard>(AccountCard accountCard)
        where TCard : Card
    {
        return new AiPlayer<TCard>(accountCard);
    }
}