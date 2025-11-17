namespace CoolCardGames.Library.Core.Players;

public class AiPlayerFactory
{
    public AiPlayer<TCard> Make<TCard>(PlayerAccountCard playerAccountCard)
        where TCard : Card
    {
        return new AiPlayer<TCard>(playerAccountCard);
    }
}