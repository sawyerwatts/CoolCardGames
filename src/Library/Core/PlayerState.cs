namespace CoolCardGames.Library.Core;

public class PlayerState<TCard>
    where TCard : Card
{
    public Cards<TCard> Hand { get; set; } = [];
}