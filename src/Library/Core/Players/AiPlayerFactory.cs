namespace CoolCardGames.Library.Core.Players;

public class AiPlayerFactory
{
    public AiPlayerSession<TCard> Make<TCard>(AccountCard accountCard)
        where TCard : Card
    {
        return new AiPlayerSession<TCard>(accountCard);
    }
}